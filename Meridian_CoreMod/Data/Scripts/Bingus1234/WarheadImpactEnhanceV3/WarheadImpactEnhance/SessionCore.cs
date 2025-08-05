//#define Debug_Message


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
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.Models;
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
        private object SyncLock_TrackingWarheadGrid = new object();
        private object SyncLock_EntityPenddingToDelete = new object();
        private object SyncLock_Possible_WarheadGrids = new object();
        private static Dictionary<IMyCubeGrid, Vector3D> _TrackingWarheadGrid = new Dictionary<IMyCubeGrid, Vector3D>();
        private static List<IMyCubeGrid> _Tmp_KeysOfTrackingGrids = new List<IMyCubeGrid>();
        private static Dictionary<IMyEntity, DateTime> _RecycleWarheadGrid = new Dictionary<IMyEntity, DateTime>();
        private static HashSet<IMyCubeGrid> _EntityPenddingToDelete = new HashSet<IMyCubeGrid>();
        private static ParallelTasks.Task? ScanTask;

        private static ObjectPool<MyConcurrentHashSet<IMyEntity>> IMyEntity_MyConcurrentHashSetPool;
        private static ObjectPool<HashSet<IMyEntity>> IMyEntity_HashSetPool;
        private static ObjectPool<List<IHitInfo>> IHitInfoPool;
        private static ObjectPool<List<MyLineSegmentOverlapResult<MyEntity>>> LineSegmentOverlapResultPool;
        private static ObjectPool<List<MyEntity>> MyEntityListPool;
        //private static ObjectPool<LineD> LineDPool;

        private static HashSet<IMyCubeGrid> Possible_WarheadGrids = new HashSet<IMyCubeGrid>();

        BoundingBoxD WarheadGrid_AABB = new BoundingBoxD();
        BoundingSphereD sphere = new BoundingSphereD();
        BoundingBoxD ObjAABB = new BoundingBoxD();
        Type TYPEOF_IMyCubeGrid = typeof(IMyCubeGrid);

        const float MaxSpeed = 3000;

        const float Discrete_Error = MaxSpeed / 60 + 0.5f;
        bool Fetch_AllGrid_Once_Complete = false;

        private void InitializeArmedWarhead(bool init = false)
        {
            MyConcurrentHashSet<IMyEntity> _PanddingAddToWarheadGrid = IMyEntity_MyConcurrentHashSetPool.Pop();
            MyConcurrentHashSet<IMyEntity> _Mark_Closed_WarheadGrid = IMyEntity_MyConcurrentHashSetPool.Pop();

            if (init)
            {
                HashSet<IMyEntity> entityList = IMyEntity_HashSetPool.Pop();
                MyAPIGateway.Entities.GetEntities(entityList, x => (x as IMyCubeGrid)?.GetTopMostParent(TYPEOF_IMyCubeGrid) == x);

                lock (SyncLock_Possible_WarheadGrids)
                {
                    foreach (IMyCubeGrid entity in entityList)
                    {
                        if (entity == null || entity.Closed)
                            continue;
                        if (entity.CustomName == "SPIKER" || entity.GetFatBlocks<IMyWarhead>().GetEnumerator().MoveNext())
                        {
                            Possible_WarheadGrids.Add(entity);
                        }
                    }
                }


                entityList.Clear();
                IMyEntity_HashSetPool.Push(entityList);
            }


            lock (SyncLock_Possible_WarheadGrids)
            {
                MyAPIGateway.Parallel.ForEach(Possible_WarheadGrids, entity =>
                {
                    IMyCubeGrid cubeGrid = entity as IMyCubeGrid;
                    if (cubeGrid == null)
                        return;
                    if (entity.Closed)
                    {
                        _Mark_Closed_WarheadGrid.Add(cubeGrid);
                        return;
                    }

                    if (_TrackingWarheadGrid.ContainsKey(cubeGrid))
                        return;

                    foreach (var block in cubeGrid.GetFatBlocks<IMyWarhead>())
                    {
                        if (!block.IsArmed)
                            continue;
                        var _parentGrid = block.CubeGrid;
                        if (_parentGrid == null)
                            continue;

                        if (_parentGrid == cubeGrid)
                        {
                            _PanddingAddToWarheadGrid.Add(_parentGrid);
                        }
                        break;
                    }
                });
            }

            if (_Mark_Closed_WarheadGrid.Count > 0)
            {
                lock (SyncLock_Possible_WarheadGrids)
                {
                    foreach (IMyCubeGrid g in _Mark_Closed_WarheadGrid)
                    {
                        Possible_WarheadGrids.Remove(g);
                    }
                }
            }

            if (_PanddingAddToWarheadGrid.Count > 0)
            {
                lock (SyncLock_TrackingWarheadGrid)
                {
                    foreach (IMyCubeGrid e in _PanddingAddToWarheadGrid)
                    {
                        _TrackingWarheadGrid[e] = Vector3D.Zero;
                    }
                }
            }

            _PanddingAddToWarheadGrid.Clear();
            IMyEntity_MyConcurrentHashSetPool.Push(_PanddingAddToWarheadGrid);

            _Mark_Closed_WarheadGrid.Clear();
            IMyEntity_MyConcurrentHashSetPool.Push(_Mark_Closed_WarheadGrid);
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            Setup();
        }

        private void Flush_EntityPenddingToDelete()
        {
            if (_EntityPenddingToDelete.Count > 0)
            {
                lock (SyncLock_TrackingWarheadGrid)
                {
                    foreach (IMyCubeGrid e in _EntityPenddingToDelete.ToList())
                    {
                        _TrackingWarheadGrid.Remove(e);
                    }
                }
            }
        }
        private void MarkGridPendingToRecycle(ref HashSet<IMyCubeGrid> ListOfEntities)
        {
            foreach (IMyCubeGrid e in ListOfEntities)
            {
                if (e != null && !e.Closed)
                {
                    _RecycleWarheadGrid[e] = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                }
            }
        }

        private void DelGridIfTimeout()
        {
            foreach (var g in _RecycleWarheadGrid.Keys.ToList())
            {
                if (_RecycleWarheadGrid.ContainsKey(g) && _RecycleWarheadGrid[g] < DateTime.UtcNow)
                {
                    if (g != null && !g.Closed)
                        g.Close();
                    _RecycleWarheadGrid.Remove(g);
                }
            }
        }

        public void Setup()
        {
            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove += OnEntityRemove;

            IMyEntity_MyConcurrentHashSetPool = new ObjectPool<MyConcurrentHashSet<IMyEntity>>(
             () => new MyConcurrentHashSet<IMyEntity>(),
                startSize: 10
            );
            IMyEntity_HashSetPool = new ObjectPool<HashSet<IMyEntity>>(
                () => new HashSet<IMyEntity>(),
                startSize: 10
            );
            IHitInfoPool = new ObjectPool<List<IHitInfo>>(
                () => new List<IHitInfo>(),
                startSize: 10
            );
            LineSegmentOverlapResultPool = new ObjectPool<List<MyLineSegmentOverlapResult<MyEntity>>>(
                () => new List<MyLineSegmentOverlapResult<MyEntity>>(),
                startSize: 10
            );
            MyEntityListPool = new ObjectPool<List<MyEntity>>(
                () => new List<MyEntity>(),
                startSize: 10
            );
        }

        private void OnEntityRemove(IMyEntity obj)
        {
            IMyCubeGrid grid = (obj as IMyCubeGrid) ?? (obj as IMyCubeBlock)?.CubeGrid;
            if (grid == null)
                return;
            lock (SyncLock_Possible_WarheadGrids)
            {
                if (Possible_WarheadGrids.Contains(grid))
                {
                    Possible_WarheadGrids.Remove(grid);
                    return;
                }
            }
        }

        private void OnEntityAdd(IMyEntity obj)
        {
            if (obj == null || obj.Closed || (obj as IMyCubeGrid)?.GetTopMostParent(TYPEOF_IMyCubeGrid) != obj)
                return;
            if (obj.Physics == null)
                return;

            IMyCubeGrid cubeGrid = obj as IMyCubeGrid;
            var Warhead_iter = cubeGrid.GetFatBlocks<IMyWarhead>().GetEnumerator();
            if (Warhead_iter.MoveNext())
            {
                lock (SyncLock_Possible_WarheadGrids)
                {
                    Possible_WarheadGrids.Add(Warhead_iter.Current.CubeGrid.GetTopMostParent(TYPEOF_IMyCubeGrid) as IMyCubeGrid);
                }
            }
        }

        private void TeleportWarheadToPointAndDetonate(IMyEntity WarheadGrid, IMyEntity HitEntity, Vector3D IntersectedPos)
        {
            if (WarheadGrid == null || WarheadGrid.Closed)
                return;
            WarheadGrid.SetPosition(IntersectedPos + HitEntity.Physics.LinearVelocity / 60);
            //WarheadGrid.Physics.SetSpeeds(HitEntity.Physics.LinearVelocity + Vector3D.Normalize(HitEntity.GetPosition() - WarheadGrid.GetPosition()) * 50, Vector3D.Zero);
            WarheadGrid.Physics.SetSpeeds(Vector3.Zero, Vector3D.Zero);
            foreach (IMyWarhead w in (WarheadGrid as IMyCubeGrid).GetFatBlocks<IMyWarhead>())
            {
                if (!w.Closed && w.IsFunctional && !w.IsCountingDown)
                {
                    w.Detonate();
                    w.DetonationTime = 0.5f;
                    w.StartCountdown();
                    w.DoDamage(0.01f, MyDamageType.Bullet, true);
                    (w as VRage.Game.ModAPI.IMyCubeBlock).OnDestroy();
                }
                if (!w.IsFunctional)
                {
                }
            }
        }
        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;
            if (!Fetch_AllGrid_Once_Complete)
            {
                Fetch_AllGrid_Once_Complete = true;
                ScanTask = MyAPIGateway.Parallel.Start(delegate {
                    InitializeArmedWarhead(true);
                });
            }
            if (Fetch_AllGrid_Once_Complete && (ScanTask?.IsComplete ?? true))
            {
                MyAPIGateway.Parallel.Start(delegate {
                    InitializeArmedWarhead(false);
                });
            }

            if (ScanTask?.Exceptions != null && ScanTask?.Exceptions.Length > 0)
            {
                foreach (var line in ScanTask?.Exceptions)
                {
                    MyLog.Default.WriteLine(line);
                }
                MyLog.Default.Flush();

                ScanTask = null;
            }

            DelGridIfTimeout();
            lock (SyncLock_EntityPenddingToDelete)
            {
                _EntityPenddingToDelete.Clear();
            }

            MyAPIGateway.Parallel.ForEach(_Tmp_KeysOfTrackingGrids, g =>
            {
                if (g == null || g.Closed)
                    return;
                if (!_TrackingWarheadGrid.ContainsKey(g))
                    return;
                if (_RecycleWarheadGrid.ContainsKey(g))
                    return;
                if (_EntityPenddingToDelete.Contains(g))
                    return;

                var _SegmentOverlap = LineSegmentOverlapResultPool.Pop();

                LineD ray = new LineD(_TrackingWarheadGrid[g], g.GetPosition());

                MyGamePruningStructure.GetTopmostEntitiesOverlappingRay(ref ray, _SegmentOverlap);
                MyCubeGrid.MyCubeGridHitInfo info = new MyCubeGrid.MyCubeGridHitInfo();
                foreach (var hit in _SegmentOverlap)
                {
                    if (hit.Element as IMyCubeGrid == null || hit.Element.Closed)
                        continue;
                    if (hit.Element.GetTopMostParent(TYPEOF_IMyCubeGrid) == g.GetTopMostParent(TYPEOF_IMyCubeGrid))
                        continue;

                    if (((MyCubeGrid)hit.Element).GetIntersectionWithLine(ref ray, ref info, IntersectionFlags.ALL_TRIANGLES))
                    {
                        TeleportWarheadToPointAndDetonate(g, hit.Element, info.Triangle.IntersectionPointInWorldSpace);
                        lock (SyncLock_EntityPenddingToDelete)
                        {
                            _EntityPenddingToDelete.Add(g);
                        }
                        break;
                    }
                }

                _SegmentOverlap.Clear();
                LineSegmentOverlapResultPool.Push(_SegmentOverlap);

            });

            MarkGridPendingToRecycle(ref _EntityPenddingToDelete);
            Flush_EntityPenddingToDelete();
        }
        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();

            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            /*
             * lock(SyncLock_TrackingWarheadGrid)
            {
                _Tmp_KeysOfTrackingGrids = _TrackingWarheadGrid.Keys.ToList();
            }
            */

            //CLR Cpy func is atom... (internal static extern Copy)
            _Tmp_KeysOfTrackingGrids = _TrackingWarheadGrid.Keys.ToList();

            lock (SyncLock_EntityPenddingToDelete)
            {
                _EntityPenddingToDelete.Clear();
                foreach (IMyCubeGrid e in _Tmp_KeysOfTrackingGrids)
                {
                    if (!_TrackingWarheadGrid.ContainsKey(e))
                        continue;
                    if (e != null && e.Closed)
                    {
                        _EntityPenddingToDelete.Add(e);
                        continue;
                    }
                    _TrackingWarheadGrid[e] = e.GetPosition();
                }
            }


            MyAPIGateway.Parallel.ForEach(_Tmp_KeysOfTrackingGrids, w =>
            {
                if (w == null || w.Closed)
                    return;

                Parallelized_ImpactDetection(w);
            });

            MarkGridPendingToRecycle(ref _EntityPenddingToDelete);
            Flush_EntityPenddingToDelete();
        }

        private void Parallelized_ImpactDetection(IMyCubeGrid w)
        {
            if (w == null)
                return;
            if (!_TrackingWarheadGrid.ContainsKey(w))
                return;
            if (_RecycleWarheadGrid.ContainsKey(w))
                return;
            if (_EntityPenddingToDelete.Contains(w))
                return;

            bool _WarheadHasBeenTeleported = false;
            Vector3D WarheadPos = Vector3D.Zero, Warhead_Velocity = Vector3D.Zero, Warhead_VelocityDirection = Vector3D.Zero;
            double WarheadSpeed = 0;

            WarheadPos = w.PositionComp.WorldAABB.Center;
            Warhead_Velocity = w.Physics.LinearVelocity;
            Warhead_VelocityDirection = Warhead_Velocity.Normalized();
            WarheadSpeed = w.Physics.LinearVelocity.Length();
            if (WarheadSpeed < 100)
                return;
            sphere.Center = w.PositionComp.GetPosition() + Warhead_VelocityDirection * Discrete_Error * 0.5;
            sphere.Radius = 2 * Discrete_Error;
            WarheadGrid_AABB.Min = w.PositionComp.WorldAABB.Min;
            WarheadGrid_AABB.Max = w.PositionComp.WorldAABB.Max;

            List<MyEntity> entitiesInSphere = MyEntityListPool.Pop();
            MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref sphere, entitiesInSphere);
            var _SegmentOverlap_Temp = LineSegmentOverlapResultPool.Pop();

            foreach (var e in entitiesInSphere)
            {
                if (e == null)
                    continue;
                if (e as IMyCubeGrid == null)
                    continue;
                if (e.Closed || e == w.GetTopMostParent())
                    continue;
                ObjAABB.Min = e.PositionComp.WorldAABB.Min;
                ObjAABB.Max = e.PositionComp.WorldAABB.Max;

                Vector3D TargetCenterPos = e.PositionComp.WorldAABB.Center;
                Vector3D RelativeVector = TargetCenterPos - WarheadPos;
                Vector3D TargetTTIOffset = Vector3D.Zero;

                double W_FwdRange = RelativeVector.Dot(Warhead_VelocityDirection);
                double W_tti = W_FwdRange / Math.Max(WarheadSpeed, 1);

                WarheadGrid_AABB.Centerize(WarheadPos + Warhead_Velocity * W_tti);
                WarheadGrid_AABB.Inflate(5);
                ObjAABB.Centerize(TargetCenterPos + (Vector3D)e.Physics.LinearVelocity * W_tti);

                TargetTTIOffset = W_FwdRange * Warhead_VelocityDirection;

                if (ObjAABB.Contains(WarheadGrid_AABB) == ContainmentType.Contains || ObjAABB.Intersects(WarheadGrid_AABB) || ObjAABB.Distance(WarheadGrid_AABB.Center) < 2.5)
                {
                    /* GetIntersectionWithLine...
                    MyIntersectionResultLineTriangleEx? IntersectRet = null;
                    GetIntersectionWithLine(ref w, TargetCenterPos - TargetTTIOffset, TargetCenterPos + TargetTTIOffset, out IntersectRet);

                    if (IntersectRet.HasValue)
                    {
                        IMyCubeBlock HitEntityBlock = IntersectRet?.Entity as IMyCubeBlock;

                        if (HitEntityBlock != null && HitEntityBlock.CubeGrid.GetTopMostParent(TYPEOF_IMyCubeGrid) == e.GetTopMostParent(TYPEOF_IMyCubeGrid))
                        {
                            var HitCubeGrid = HitEntityBlock.CubeGrid;
                            _WarheadHasBeenTeleported = true;
                            TeleportWarheadToPointAndDetonate(w, HitCubeGrid, IntersectRet.Value.IntersectionPointInWorldSpace - HitCubeGrid.WorldAABB.Center);
                            lock (SyncLock_EntityPenddingToDelete)
                            {
                                _EntityPenddingToDelete.Add(w);
                            }
                        }
                    }
                    //*/

                    //* Physics.CastRay...

                    _SegmentOverlap_Temp.Clear();



                    LineD ray = new LineD(TargetCenterPos - TargetTTIOffset, TargetCenterPos + TargetTTIOffset);
                    MyGamePruningStructure.GetTopmostEntitiesOverlappingRay(ref ray, _SegmentOverlap_Temp);
                    MyCubeGrid.MyCubeGridHitInfo info = new MyCubeGrid.MyCubeGridHitInfo();
                    foreach (var hit in _SegmentOverlap_Temp)
                    {
                        IMyCubeGrid HitEntity = (hit.Element as IMyCubeGrid) ?? (hit.Element as IMyCubeBlock)?.CubeGrid;
                        if (HitEntity == null || HitEntity.Closed)
                            continue;
                        if (HitEntity.GetTopMostParent(TYPEOF_IMyCubeGrid) != e.GetTopMostParent(TYPEOF_IMyCubeGrid))
                            continue;


                        if (((MyCubeGrid)hit.Element).GetIntersectionWithLine(ref ray, ref info, IntersectionFlags.ALL_TRIANGLES))
                        {
                            _WarheadHasBeenTeleported = true;
                            TeleportWarheadToPointAndDetonate(w, HitEntity, info.Triangle.IntersectionPointInWorldSpace);
                            lock (SyncLock_EntityPenddingToDelete)
                            {
                                _EntityPenddingToDelete.Add(w);
                            }

                            break;
                        }

                        
                    }
                    if (_WarheadHasBeenTeleported)
                        break;
                    //*/
                }
            }

            entitiesInSphere.Clear();
            _SegmentOverlap_Temp.Clear();
            MyEntityListPool.Push(entitiesInSphere);
            LineSegmentOverlapResultPool.Push(_SegmentOverlap_Temp);
        }

        /*private void GetIntersectionWithLine(ref IMyCubeGrid GridRef, Vector3D from, Vector3D to, out MyIntersectionResultLineTriangleEx? IntersectRet)
        {
            var Ray = LineDPool.Pop();

            Ray.From = from;
            Ray.To = to;
            Ray.Direction = to - from;
            Ray.Length = Ray.Direction.Normalize();

            GridRef.GetIntersectionWithLine(ref Ray, out IntersectRet, IntersectionFlags.ALL_TRIANGLES);
            LineDPool.Push(Ray);
        }*/
        protected override void UnloadData()
        {
            MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemove;
        }

    }

}
