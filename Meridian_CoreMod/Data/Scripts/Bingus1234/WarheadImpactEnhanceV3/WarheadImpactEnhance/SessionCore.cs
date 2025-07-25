using ProtoBuf;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using WarheadRaycastImpact.Utils;

namespace WarheadRaycastImpact
{

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation)]

    public class SessionCore : MySessionComponentBase
    {

        public static long ModId = -1;
        private static List<IHitInfo> _ListOfIHitInfo_Temp = new List<IHitInfo>();
        private static List<Vector3I> _ListOfIHitInfo_InVectorI_Temp = new List<Vector3I>();
        private static Vector3I WarheadGrid_SizeInflate = new Vector3I(2, 2, 2);
        private object SyncLock_TrackingWarheadGrid = new object();
        private static Dictionary<IMyEntity, Vector3D> _TrackingWarheadGrid = new Dictionary<IMyEntity, Vector3D>();
        private static List<IMyEntity> _Tmp_KeysOfTrackingGrids = new List<IMyEntity>();
        private static Dictionary<IMyEntity, DateTime> _RecycleWarheadGrid = new Dictionary<IMyEntity, DateTime>();
        private static HashSet<IMyEntity> _EntityPenddingToDelete = new HashSet<IMyEntity>();
        private static ParallelTasks.Task? ScanTask;

        HashSet<IMyEntity> _PanddingAddToWarheadGrid = new HashSet<IMyEntity>();

        private static ObjectPool<HashSet<IMyEntity>> IMyEntityPool;
        private static DateTime _LastUpdateTime = DateTime.MinValue;

        BoundingBoxD WarheadGrid_AABB = new BoundingBoxD();
        BoundingSphereD sphere = new BoundingSphereD();
        BoundingBoxD ObjAABB = new BoundingBoxD();


        private void InitializeArmedWarhead()
        {
            var entityList = IMyEntityPool.Pop();
            MyAPIGateway.Entities.GetEntities(entityList, x => (x as IMyCubeGrid)?.GetTopMostParent(typeof(IMyCubeGrid)) == x);
            _PanddingAddToWarheadGrid.Clear();
            foreach (var entity in entityList)
            {
                var cubeGrid = entity as IMyCubeGrid;
                if (cubeGrid == null || entity.Closed) continue;

                if (_TrackingWarheadGrid.ContainsKey(entity)) continue;

                foreach (var block in cubeGrid.GetFatBlocks<IMyWarhead>())
                {
                    if (!block.IsArmed) continue;
                    var _parentGrid = block.CubeGrid;
                    if (_parentGrid == null) continue;

                    if (_parentGrid == cubeGrid)
                    {
                        _PanddingAddToWarheadGrid.Add(_parentGrid);
                    }
                    break;
                }
            }

            if(_PanddingAddToWarheadGrid.Count > 0){
                lock (SyncLock_TrackingWarheadGrid)
                {
                    foreach (var e in _PanddingAddToWarheadGrid)
                    {
                        _TrackingWarheadGrid[e] = Vector3D.Zero;
                    }
                }
            }
            

            entityList.Clear();
            IMyEntityPool.Push(entityList);

            if(_LastUpdateTime + TimeSpan.FromSeconds(60) < DateTime.UtcNow)
            {
                _LastUpdateTime = DateTime.UtcNow;
                //MyVisualScriptLogicProvider.SendChatMessage($"Thread Alive: {_TrackingWarheadGrid.Count}");
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            Setup();
        }

        private void Flush_EntityPenddingToDelete()
        {
            if(_EntityPenddingToDelete.Count > 0){
                lock (SyncLock_TrackingWarheadGrid)
                {
                    foreach (var e in _EntityPenddingToDelete)
                    {
                        _TrackingWarheadGrid.Remove(e);
                    }
                }
            }
        }
        private void MarkGridPendingToRecycle(ref HashSet<IMyEntity> ListOfEntities)
        {
            foreach (var e in ListOfEntities)
            {
                if (e as IMyCubeGrid == null) continue;
                if (!e.Closed)
                {
                    _RecycleWarheadGrid[e] = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                }
            }
        }

        private void DelGridIfTimeout()
        {
            foreach(var g in _RecycleWarheadGrid.Keys.ToList())
            {
                if(_RecycleWarheadGrid.ContainsKey(g) && _RecycleWarheadGrid[g] < DateTime.UtcNow)
                {
                    if(!g.Closed) g.Close();
                    _RecycleWarheadGrid.Remove(g);
                }
            }
        }

        public void Setup()
        {
            IMyEntityPool = new ObjectPool<HashSet<IMyEntity>>(
                () => new HashSet<IMyEntity>(),
                startSize: 10
            );
        }

        private void TeleportWarheadToPointAndDetonate(IMyEntity WarheadGrid, IMyEntity HitEntity, Vector3D IntersectedPos)
        {
            //MyVisualScriptLogicProvider.SendChatMessage($"Intersect w/ Grid... teleport {WarheadGrid.DisplayName}, {Vector3D.Distance(IntersectedPos, WarheadGrid.GetPosition())}");
            WarheadGrid.SetPosition(IntersectedPos + HitEntity.Physics.LinearVelocity/60);
            //WarheadGrid.Physics.SetSpeeds(HitEntity.Physics.LinearVelocity + Vector3D.Normalize(HitEntity.GetPosition() - WarheadGrid.GetPosition()) * 50, Vector3D.Zero);
            WarheadGrid.Physics.SetSpeeds(Vector3.Zero, Vector3D.Zero);
            foreach (IMyWarhead w in (WarheadGrid as IMyCubeGrid).GetFatBlocks<IMyWarhead>())
            {
                if (!w.Closed && w.IsFunctional && !w.IsCountingDown) { w.Detonate(); w.DetonationTime = 0.5f; w.StartCountdown(); w.DoDamage(0.01f, MyDamageType.Bullet, true); (w as VRage.Game.ModAPI.IMyCubeBlock).OnDestroy(); }
                //if (!w.IsFunctional)
                //{
                //    MyVisualScriptLogicProvider.SendChatMessage($"Warhead-Malfunctional {WarheadGrid.DisplayName}");
                //}
            }
        }
        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (!MyAPIGateway.Multiplayer.IsServer) return;
            if (ScanTask?.IsComplete ?? true)
            {
                MyAPIGateway.Parallel.Start(delegate {
                    InitializeArmedWarhead();
                });
            }

            if(ScanTask?.Exceptions.Length > 0)
            {
                foreach(var line in ScanTask?.Exceptions)
                {
                    MyLog.Default.WriteLine(line);
                }
                MyLog.Default.Flush();

                //throw new Exception("Thread was dead with unknow Exceptions :(");
            }

            DelGridIfTimeout();
            _EntityPenddingToDelete.Clear();

            foreach (var g in _Tmp_KeysOfTrackingGrids)
            {
                if (!_TrackingWarheadGrid.ContainsKey(g)) continue;
                if (_RecycleWarheadGrid.ContainsKey(g)) continue;
                _ListOfIHitInfo_Temp.Clear();
                if (g.Closed) continue;
                MyAPIGateway.Physics.CastRay(_TrackingWarheadGrid[g], g.GetPosition(), _ListOfIHitInfo_Temp);
                foreach (var hit in _ListOfIHitInfo_Temp)
                {
                    if (hit.HitEntity as IMyCubeGrid == null) continue;
                    if (hit.HitEntity.EntityId == g.EntityId || hit.HitEntity.EntityId == g.GetTopMostParent(typeof(IMyCubeGrid)).EntityId) continue;
                    if (hit.HitEntity.Closed) continue;

                    TeleportWarheadToPointAndDetonate(g, hit.HitEntity, hit.Position);
                    _EntityPenddingToDelete.Add(g);

                    break;
                }
            }


            MarkGridPendingToRecycle(ref _EntityPenddingToDelete);
            Flush_EntityPenddingToDelete();
        }
        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();

            if (!MyAPIGateway.Multiplayer.IsServer) return;

            /*
             * lock(SyncLock_TrackingWarheadGrid)
            {
                _Tmp_KeysOfTrackingGrids = _TrackingWarheadGrid.Keys.ToList();
            }
            */

            //CLR Cpy func is atom... (internal static extern Copy)
            _Tmp_KeysOfTrackingGrids = _TrackingWarheadGrid.Keys.ToList();

            _EntityPenddingToDelete.Clear();
            foreach (var e in _Tmp_KeysOfTrackingGrids)
            {
                if (!_TrackingWarheadGrid.ContainsKey(e)) continue;
                if (e.Closed)
                {
                    _EntityPenddingToDelete.Add(e);
                    continue;
                }
                _TrackingWarheadGrid[e] = e.GetPosition();
            }


            Vector3D WarheadPos = Vector3D.Zero, Warhead_Velocity = Vector3D.Zero, Warhead_VelocityDirection = Vector3D.Zero;
            double WarheadSpeed = 0;
            float Discrete_Error = 3000f / 60 + 0.5f;
            bool _WarheadHasBeenTeleported = false;
            foreach (IMyCubeGrid w in _Tmp_KeysOfTrackingGrids)
            {
                if (!_TrackingWarheadGrid.ContainsKey(w)) continue;
                if (_RecycleWarheadGrid.ContainsKey(w)) continue;
                if (_EntityPenddingToDelete.Contains(w)) continue;
                if (w == null) continue;
                if (w.Closed) continue;
                WarheadPos = w.PositionComp.WorldAABB.Center;
                Warhead_Velocity = w.Physics.LinearVelocity;
                Warhead_VelocityDirection = Warhead_Velocity.Normalized();
                WarheadSpeed = w.Physics.LinearVelocity.Length();
                _WarheadHasBeenTeleported = false;
                if (WarheadSpeed < 100) continue;
                sphere.Center = w.PositionComp.GetPosition() + Warhead_VelocityDirection * Discrete_Error * 0.5;
                sphere.Radius = 2 * Discrete_Error;
                WarheadGrid_AABB.Min = w.PositionComp.WorldAABB.Min;
                WarheadGrid_AABB.Max = w.PositionComp.WorldAABB.Max;

                List<MyEntity> entitiesInSphere = MyEntities.GetTopMostEntitiesInSphere(ref sphere);

                foreach (var e in entitiesInSphere)
                {
                    if (e == null) continue;
                    if (e as IMyCubeGrid == null) continue;
                    if (e.Closed || e == w.GetTopMostParent() || e.Physics == null) continue;
                    ObjAABB.Min = e.PositionComp.WorldAABB.Min;
                    ObjAABB.Max = e.PositionComp.WorldAABB.Max;

                    Vector3D TargetCenterPos = e.PositionComp.WorldAABB.Center;
                    Vector3D RelativeVector = TargetCenterPos - WarheadPos;
                    Vector3D TargetTTIOffset = Vector3D.Zero;

                    double W_FwdRange = RelativeVector.Dot(Warhead_VelocityDirection);
                    double W_tti = W_FwdRange / Math.Max(WarheadSpeed, 1);

                    WarheadGrid_AABB.Centerize(WarheadPos + Warhead_Velocity * W_tti);
                    WarheadGrid_AABB.Inflate(5);
                    ObjAABB.Centerize(TargetCenterPos + (Vector3D) e.Physics.LinearVelocity * W_tti);

                    TargetTTIOffset = W_FwdRange * Warhead_VelocityDirection;
                    //TargetCenterPos += (Vector3D)e.Physics.LinearVelocity / 60

                    if (ObjAABB.Contains(WarheadGrid_AABB) == ContainmentType.Contains || ObjAABB.Intersects(WarheadGrid_AABB) || ObjAABB.Distance(WarheadGrid_AABB.Center) < 2.5)
                    {
                        _ListOfIHitInfo_Temp.Clear();
                        MyAPIGateway.Physics.CastRay(TargetCenterPos - TargetTTIOffset, TargetCenterPos + TargetTTIOffset, _ListOfIHitInfo_Temp);
                        //w.RayCastCells(WarheadPos, TargetCenterPos, _ListOfIHitInfo_InVectorI_Temp, WarheadGrid_SizeInflate, false);
                        /*var Line = new LineD(TargetCenterPos - TargetTTIOffset, TargetCenterPos +  TargetTTIOffset);
                        IMySlimBlock HitBlock = null;
                        double Distance = 0;
                        Vector3D? Intersection_Position = w.GetLineIntersectionExactAll(ref Line, out Distance, out HitBlock);;
                        if (Intersection_Position.HasValue)
                        {
                            if (HitBlock.CubeGrid.GetTopMostParent(typeof(IMyCubeGrid)) != e.GetTopMostParent(typeof(IMyCubeGrid))) continue;
                            TeleportWarheadToPointAndDetonate(w, HitBlock.CubeGrid, Intersection_Position.Value);
                            _EntityPenddingToDelete.Add(w);

                            break;
                        }*/

                        //MyVisualScriptLogicProvider.SendChatMessage($"Raycasted Entities: {_ListOfIHitInfo_Temp.Count}");
                        foreach (var hit in _ListOfIHitInfo_Temp)
                        {
                            if (hit.HitEntity.Closed) continue;
                            if (hit.HitEntity.GetTopMostParent(typeof(IMyCubeGrid)) != e.GetTopMostParent(typeof(IMyCubeGrid))) continue;

                            _WarheadHasBeenTeleported = true;
                            TeleportWarheadToPointAndDetonate(w, hit.HitEntity, hit.Position);
                            _EntityPenddingToDelete.Add(w);

                            break;
                        }

                        Vector3D _RaycastLine_Norm = Warhead_VelocityDirection;
                       // MyVisualScriptLogicProvider.SendChatMessage($"Raycast Failed w/ Grid... {w.DisplayName}, {(TargetCenterPos - WarheadPos).Dot(_RaycastLine_Norm)}");
                    } 
                    else
                    {
                        //MyVisualScriptLogicProvider.SendChatMessage($"Missed w/ Grid... {w.DisplayName}, {Vector3D.Distance(ObjAABB.Center, WarheadGrid_AABB.Center)}");
                    }
                    if (_WarheadHasBeenTeleported) break;
                }
                entitiesInSphere.Clear();
            }


            MarkGridPendingToRecycle(ref _EntityPenddingToDelete);
            Flush_EntityPenddingToDelete();
        }
        protected override void UnloadData()
        {

        }

    }

}
