using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Noise.Patterns;
using VRage.Utils;
using VRageMath;



[MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
public class DeclareWarWatcher : MySessionComponentBase
{

    private readonly Dictionary<string, MyRelationsBetweenFactions> _rel = new Dictionary<string, MyRelationsBetweenFactions>();
    private int _tick;
    private bool _initialized;

    public override void LoadData()
    {

        if (MyAPIGateway.Session == null || !MyAPIGateway.Multiplayer.IsServer)
            return;

        var facs = MyAPIGateway.Session.Factions;
        foreach (var a in facs.Factions)
            foreach (var b in facs.Factions)
            {
                if (a.Key >= b.Key) continue;
                var key = PairKey(a.Key, b.Key);
                var rel = facs.GetRelationBetweenFactions(a.Key, b.Key);
                _rel[key] = rel;
            }

        _initialized = true;
    }

    public override void BeforeStart()
    {
        if (_initialized && MyAPIGateway.Multiplayer.IsServer)
        {
            MyVisualScriptLogicProvider.SendChatMessageColored(
                message: "Declare War Watcher initialized successfully.",
                color: new Color(0, 122, 255),
                author: "Conflict Commissariat",
                playerId: 0,
                font: "Blue"
            );
        }
    }

    public override void UpdateAfterSimulation()
    {
        if (MyAPIGateway.Session == null || !MyAPIGateway.Multiplayer.IsServer)
            return;


        _tick++;
        if (_tick % 300 != 0)
            return;

        var facs = MyAPIGateway.Session.Factions;
        foreach (var a in facs.Factions)
            foreach (var b in facs.Factions)
            {
                if (a.Key >= b.Key) continue;

                var key = PairKey(a.Key, b.Key);
                var now = facs.GetRelationBetweenFactions(a.Key, b.Key);

                MyRelationsBetweenFactions before;
                if (!_rel.TryGetValue(key, out before))
                {
                    _rel[key] = now;
                    continue;
                }


                if (before != MyRelationsBetweenFactions.Enemies && now == MyRelationsBetweenFactions.Enemies)
                {
                    AnnounceWar(a.Value, b.Value);
                }


                if (before == MyRelationsBetweenFactions.Enemies && now != MyRelationsBetweenFactions.Enemies)
                {
                    AnnouncePeace(a.Value, b.Value, now);
                }


                if (before != now)
                    _rel[key] = now;
            }
    }

    protected override void UnloadData()
    {
        _rel.Clear();
    }

    private static string PairKey(long a, long b)
    {
        return a < b ? a.ToString() + ":" + b.ToString() : b.ToString() + ":" + a.ToString();
    }

    private static void AnnounceWar(IMyFaction fa, IMyFaction fb)
    {
        var nameA = $"{fa.Tag} ({fa.Name})";
        var nameB = $"{fb.Tag} ({fb.Name})";

        MyVisualScriptLogicProvider.SendChatMessageColored(
            message: $"War declared between {nameA} and {nameB}.",
            color: new Color(0, 122, 255),
            author: "Conflict Commissariat",
            playerId: 0,
            font: "Blue"
        );
    }

    private static void AnnouncePeace(IMyFaction fa, IMyFaction fb, MyRelationsBetweenFactions newRel)
    {
        var nameA = $"{fa.Tag} ({fa.Name})";
        var nameB = $"{fb.Tag} ({fb.Name})";


        var status = newRel == MyRelationsBetweenFactions.Friends ? "Peace accepted (Allied)" : "Peace accepted";
        MyVisualScriptLogicProvider.SendChatMessageColored(
            message: $"{status} between {nameA} and {nameB}.",
            color: new Color(0, 122, 255),
            author: "Conflict Commissariat",
            playerId: 0,
            font: "Blue"
        );
    }
}








[MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
public class WarBountyPayouts : MySessionComponentBase
{

    private const int PayoutIntervalSeconds = 15;


    private const long DefaultPayoutLargeGrid = 1000;
    private const long DefaultPayoutSmallGrid = 100;


    private static readonly Dictionary<string, long> PayoutBySubtype = new Dictionary<string, long>()
    {
        { "LargeBlockLargeTurret", 25000 },
        { "SmallGatlingGun",        8000 },
        { "LargeBlockBatteryBlock", 12000 },
        { "LargeWarhead",           40000 },
        { "SmallWarhead",           15000 },
    };


    private static readonly Dictionary<string, long> BonusByComponentSubtypeUpper =
        new Dictionary<string, long>()
    {
        { "StratLicense",50000 },
        { "OpLicense",10000 },
        { "TacLicense",7500 },
    };


    private const double HydrogenTankCreditsPerLiter = 2.0;


    private const long PlayerKillBounty = 1500;



    private int _payoutIntervalTicks;
    private bool _registered;
    private int _tick;

    //Xander - sphere / static field
    BoundingSphereD Detsphere = new BoundingSphereD();
    List<MyEntity> DetEntitiesList = null;

    private static readonly MyStringHash Damage_Deformation = MyStringHash.GetOrCompute("Deformation");
    private static readonly MyStringHash Damage_Grinding = MyStringHash.GetOrCompute("Grinding");


    private readonly Dictionary<long, long> _pending = new Dictionary<long, long>();


    private static readonly Dictionary<string, int> _tmpMissing = new Dictionary<string, int>();


    private readonly Dictionary<long, long> _warheadAttackerByGrid = new Dictionary<long, long>(); // gridId -> attacker identity
    private readonly Dictionary<long, int> _warheadGridSeenTick = new Dictionary<long, int>();
    private readonly Dictionary<long, long> _warheadAttackerByVictim = new Dictionary<long, long>(); // character entityId -> attacker identity
    private readonly Dictionary<long, int> _warheadVictimSeenTick = new Dictionary<long, int>();
    private const int WarheadAttackerTTL = 1800; // ~30s in ticks (assuming ~60 SPS)

    private Type TYPEOF_IMyCubeGrid = typeof(IMyCubeGrid);

    public override void BeforeStart()
    {
        if (MyAPIGateway.Multiplayer != null && MyAPIGateway.Multiplayer.IsServer)
        {
            _payoutIntervalTicks = PayoutIntervalSeconds * 60;
            TryRegisterDamageHooks();
        }
    }

    protected override void UnloadData()
    {
        _pending.Clear();
        _tmpMissing.Clear();
        _warheadAttackerByGrid.Clear();
        _warheadGridSeenTick.Clear();
        _warheadAttackerByVictim.Clear();
        _warheadVictimSeenTick.Clear();
        _registered = false;
    }

    public override void UpdateAfterSimulation()
    {
        if (MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
            return;

        if (!_registered)
            TryRegisterDamageHooks();

        _tick++;


        if ((_tick % 300) == 0)
            PurgeStaleWarheadCache();

        if (_tick >= _payoutIntervalTicks)
        {
            _tick = 0;
            PayAggregatedBounties();
        }
    }

    private void TryRegisterDamageHooks()
    {
        if (_registered) return;
        if (MyAPIGateway.Session?.DamageSystem == null) return;

        var retarted_sphere = new BoundingSphereD();
        DetEntitiesList = MyEntities.GetEntitiesInSphere(ref retarted_sphere);


        //MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, OnBeforeDamage);

        MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, OnDestroyed);

        _registered = true;
    }

    public static bool IsNPCFaction(IMyFaction faction)
    {
        return faction.IsEveryoneNpc();
    }


    //Xander - Only handle explosion type for now....
    private void OnBeforeDamage(object target, ref MyDamageInformation info)
    {
        if (target == null) return;
        if (info.Type != MyDamageType.Explosion) return;

        long? OwnerOfWarheadGrid = null;

        //Xander - Warhead-Dodamge ret itself as target
        IMyWarhead blockRef = target as IMyWarhead;
        if(blockRef == null) return;

        Detsphere.Center = blockRef.WorldAABB.Center;
        Detsphere.Radius = 25;

        DetEntitiesList.Clear();
        MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref Detsphere, DetEntitiesList);
        if (blockRef.CubeGrid.BigOwners.Count > 0) OwnerOfWarheadGrid = blockRef.CubeGrid.BigOwners[0];

        IMyCubeGrid TopMostGrid = (blockRef.CubeGrid.GetTopMostParent(TYPEOF_IMyCubeGrid) as IMyCubeGrid);
        if (!OwnerOfWarheadGrid.HasValue && TopMostGrid!=null && TopMostGrid.BigOwners.Count > 0) OwnerOfWarheadGrid = TopMostGrid.BigOwners[0];
        if (!OwnerOfWarheadGrid.HasValue) return;
        
        foreach (var e in DetEntitiesList)
        {
            if (e == blockRef) continue;

            //Character case
            IMyCharacter ch = e as IMyCharacter;
            if (ch != null)
            {
                long vid = ch.EntityId;
                //_warheadAttackerByVictim[vid] = OwnerOfWarheadGrid.Value;
                // _warheadVictimSeenTick[vid] = _tick;
                QueuePayout(OwnerOfWarheadGrid.Value, 1000);
                return;
            }

            //Grid / Block case
            IMyCubeGrid e_grid = (e as IMyCubeBlock)?.CubeGrid ?? (e as IMyCubeGrid);
            if (e_grid == null) continue;
            if (e_grid.GetTopMostParent(TYPEOF_IMyCubeGrid) == blockRef.CubeGrid.GetTopMostParent(TYPEOF_IMyCubeGrid)) continue;

            if (e_grid.BigOwners.Count > 0 && e_grid.BigOwners[0] != OwnerOfWarheadGrid.Value)
            {
                //_warheadAttackerByGrid[e_grid.EntityId] = OwnerOfWarheadGrid.Value;
                //_warheadGridSeenTick[e_grid.EntityId] = _tick;
                QueuePayout(OwnerOfWarheadGrid.Value, 1000);
                return;
            }
        }
    }

    private void PurgeStaleWarheadCache()
    {
        if (_warheadGridSeenTick.Count > 0)
        {
            var rm = new List<long>();
            foreach (var kv in _warheadGridSeenTick)
                if (_tick - kv.Value > WarheadAttackerTTL) rm.Add(kv.Key);
            for (int i = 0; i < rm.Count; i++)
            {
                long id = rm[i];
                _warheadGridSeenTick.Remove(id);
                _warheadAttackerByGrid.Remove(id);
            }
        }

        if (_warheadVictimSeenTick.Count > 0)
        {
            var rm = new List<long>();
            foreach (var kv in _warheadVictimSeenTick)
                if (_tick - kv.Value > WarheadAttackerTTL) rm.Add(kv.Key);
            for (int i = 0; i < rm.Count; i++)
            {
                long id = rm[i];
                _warheadVictimSeenTick.Remove(id);
                _warheadAttackerByVictim.Remove(id);
            }
        }
    }


    private void OnDestroyed(object target, MyDamageInformation info)
    {
        if (info.Type == Damage_Deformation || info.Type == Damage_Grinding) return;

        if (info.Type == MyDamageType.Explosion && target is IMyWarhead) {
            HandleWarhead(target as IMyWarhead, info);
            return;
        }

        var ch = target as IMyCharacter;
        if (ch != null)
        {
            HandleCharacterDeath(ch, info);
            return;
        }

        var slim = target as IMySlimBlock;
        if (slim == null || slim.CubeGrid == null) return;

        long defenderId = GetPrimaryOwnerIdentity(slim.CubeGrid);
        if (defenderId == 0) return;

        long attackerId;
        if (!TryResolveAttackerIdentity(info.AttackerId, out attackerId) || attackerId == 0)
        {

            long cached;
            if (!_warheadAttackerByGrid.TryGetValue(slim.CubeGrid.EntityId, out cached) || cached == 0)
                return;
            attackerId = cached;
        }

        var facs = MyAPIGateway.Session.Factions;
        if (facs == null) return;

        var atkFac = facs.TryGetPlayerFaction(attackerId);
        var defFac = facs.TryGetPlayerFaction(defenderId);
        if (atkFac == null || defFac == null) return;
        if (!facs.AreFactionsEnemies(atkFac.FactionId, defFac.FactionId)) return;


        string subtypeRaw = GetTrueSubtypeID(slim);

        long payout;
        if (!PayoutBySubtype.TryGetValue(subtypeRaw, out payout))
        {
            payout = (slim.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large)
                ? DefaultPayoutLargeGrid
                : DefaultPayoutSmallGrid;
        }

        payout += GetComponentBonuses(slim);
        payout += GetHydrogenBonusByLiters(slim, subtypeRaw);

        if (payout > 0)
            QueuePayout(attackerId, payout);
    }

    private void HandleWarhead(IMyWarhead target, MyDamageInformation info)
    {
        if (target == null) return;
        long? OwnerOfWarheadGrid = null;

        //Xander - Warhead-Dodamge ret itself as target
        IMyWarhead blockRef = target as IMyWarhead;
        if (blockRef == null) return;
        if (blockRef.CubeGrid.BigOwners.Count > 0) OwnerOfWarheadGrid = blockRef.CubeGrid.BigOwners[0];

        IMyCubeGrid TopMostGrid = (blockRef.CubeGrid.GetTopMostParent(TYPEOF_IMyCubeGrid) as IMyCubeGrid);
        if (!OwnerOfWarheadGrid.HasValue && TopMostGrid != null && TopMostGrid.BigOwners.Count > 0) OwnerOfWarheadGrid = TopMostGrid.BigOwners[0];
        if (!OwnerOfWarheadGrid.HasValue) return;

        foreach (var e in DetEntitiesList)
        {
            if (e == blockRef) continue;

            //Character case
            if (e is IMyCharacter)
            {
                IMyCharacter ch1 = e as IMyCharacter;
                long vid = ch1.EntityId;
                QueuePayout(OwnerOfWarheadGrid.Value, 10000);
                
            }

            //Grid / Block case
            IMyCubeGrid e_grid = (e as IMyCubeBlock)?.CubeGrid ?? (e as IMyCubeGrid);
            if (e_grid == null) continue;
            if (e_grid.GetTopMostParent(TYPEOF_IMyCubeGrid) == blockRef.CubeGrid.GetTopMostParent(TYPEOF_IMyCubeGrid)) continue;

            if (e_grid.BigOwners.Count > 0 && e_grid.BigOwners[0] != OwnerOfWarheadGrid.Value)
            {
                QueuePayout(OwnerOfWarheadGrid.Value, 1000);
               
            }
        }
    }

    private void HandleCharacterDeath(IMyCharacter ch, MyDamageInformation info)
    {
        if (MyAPIGateway.Session == null) return;

        long victimId = 0;
        var victimPlayer = MyAPIGateway.Players != null ? MyAPIGateway.Players.GetPlayerControllingEntity(ch) : null;
        if (victimPlayer != null)
            victimId = victimPlayer.IdentityId;
        if (victimId == 0) return;

        long killerId;
        if (!TryResolveAttackerIdentity(info.AttackerId, out killerId) || killerId == 0)
        {

            long cached;
            if (!_warheadAttackerByVictim.TryGetValue(ch.EntityId, out cached) || cached == 0)
                return;
            killerId = cached;
        }
        if (killerId == victimId) return;

        var facs = MyAPIGateway.Session.Factions;
        if (facs == null) return;

        var atkFac = facs.TryGetPlayerFaction(killerId);
        var vicFac = facs.TryGetPlayerFaction(victimId);
        if (atkFac == null || vicFac == null) return;
        if (!facs.AreFactionsEnemies(atkFac.FactionId, vicFac.FactionId)) return;


        if (PlayerKillBounty > 0)
            QueuePayout(killerId, PlayerKillBounty);


        string killerName = GetPlayerName(killerId);
        string victimName = victimPlayer.DisplayName ?? "Unknown";
        
    }

    private void QueuePayout(long identityId, long amount)
    {
        long existing;
        if (_pending.TryGetValue(identityId, out existing))
            _pending[identityId] = existing + amount;
        else
            _pending[identityId] = amount;
    }

    private void PayAggregatedBounties()
    {
        if (_pending.Count == 0) return;

        var snapshot = new List<KeyValuePair<long, long>>(_pending);
        foreach (var kv in snapshot)
        {
            long identityId = kv.Key;
            long amount = kv.Value;
            if (amount <= 0) continue;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, p => p != null && p.IdentityId == identityId);
            if (players.Count == 0) continue;

            players[0].RequestChangeBalance(amount);

            MyVisualScriptLogicProvider.SendChatMessageColored(
                string.Format("You've received {0:n0}c in aggregated bounties.", amount),
                new Color(0, 122, 255),
                "Conflict Commissariat",
                identityId,
                "Blue"
            );

            _pending[identityId] = 0;
        }

        // Remove zeroed entries
        foreach (var kv in snapshot)
        {
            long cur;
            if (_pending.TryGetValue(kv.Key, out cur) && cur == 0)
                _pending.Remove(kv.Key);
        }
    }


    public static string GetTrueSubtypeID(IMySlimBlock block)
    {
        string subtype = block.BlockDefinition.Id.SubtypeName;
        if (string.IsNullOrEmpty(subtype))
            subtype = block.BlockDefinition.Id.TypeId.ToString().Remove(0, 16);
        return subtype ?? string.Empty;
    }

    private static string GetPlayerName(long identityId)
    {
        if (identityId == 0 || MyAPIGateway.Players == null) return "Unknown";
        var list = new List<IMyPlayer>();
        MyAPIGateway.Players.GetPlayers(list, p => p != null && p.IdentityId == identityId);
        return (list.Count > 0 && list[0] != null) ? (list[0].DisplayName ?? "Unknown") : "Unknown";
    }


    private static long GetComponentBonuses(IMySlimBlock slim)
    {
        var def = MyDefinitionManager.Static.GetCubeBlockDefinition(slim.BlockDefinition.Id);
        if (def == null || def.Components == null) return 0;

        _tmpMissing.Clear();
        slim.GetMissingComponents(_tmpMissing);

        long total = 0;
        for (int i = 0; i < def.Components.Length; i++)
        {
            var compDef = def.Components[i];
            var subtype = compDef.Definition.Id.SubtypeName;
            if (string.IsNullOrEmpty(subtype)) continue;

            long perUnitBonus;
            if (!BonusByComponentSubtypeUpper.TryGetValue(subtype.ToUpperInvariant(), out perUnitBonus))
                continue;

            int required = compDef.Count;
            int missing;
            if (!_tmpMissing.TryGetValue(subtype, out missing))
                missing = 0;

            int present = required - missing;
            if (present > 0)
                total += perUnitBonus * (long)present;
        }

        _tmpMissing.Clear();
        return total;
    }


    private static long GetHydrogenBonusByLiters(IMySlimBlock slim, string subtypeRaw)
    {
        var tank = slim.FatBlock as Sandbox.ModAPI.IMyGasTank;
        if (tank == null) return 0;


        string up = (subtypeRaw ?? "").ToUpperInvariant();
        if (up.IndexOf("Hydrogen") < 0) return 0;

        double liters = tank.Capacity * tank.FilledRatio;
        if (liters <= 0) return 0;

        long bonus = (long)System.Math.Round(liters * HydrogenTankCreditsPerLiter);
        return bonus > 0 ? bonus : 0;
    }

    private static long GetPrimaryOwnerIdentity(IMyCubeGrid grid)
    {
        if (grid.BigOwners != null && grid.BigOwners.Count > 0 && grid.BigOwners[0] != 0) return grid.BigOwners[0];
        if (grid.SmallOwners != null && grid.SmallOwners.Count > 0 && grid.SmallOwners[0] != 0) return grid.SmallOwners[0];
        return 0;
    }

    private static bool TryResolveAttackerIdentity(long attackerEntityId, out long identityId)
    {
        identityId = 0;
        IMyEntity ent;
        if (!MyAPIGateway.Entities.TryGetEntityById(attackerEntityId, out ent) || ent == null) return false;

        var top = ent.GetTopMostParent();


        var player = MyAPIGateway.Players != null ? MyAPIGateway.Players.GetPlayerControllingEntity(top) : null;
        if (player != null)
        {
            identityId = player.IdentityId;
            if (identityId != 0) return true;
        }


        var block = top as IMyCubeBlock;
        if (block != null && block.OwnerId != 0)
        {
            identityId = block.OwnerId;
            return true;
        }


        var grid = top as IMyCubeGrid;
        if (grid != null && grid.BigOwners != null && grid.BigOwners.Count > 0 && grid.BigOwners[0] != 0)
        {
            identityId = grid.BigOwners[0];
            return true;
        }

        return false;
    }
}