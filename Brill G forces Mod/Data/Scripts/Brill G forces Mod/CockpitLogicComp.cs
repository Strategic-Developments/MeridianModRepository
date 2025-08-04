using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace YourName.ModName.Data.Scripts.Gforces
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Cockpit),false)]
    public class CockpitLogicComp : MyGameLogicComponent
    {
        public const string ModName = "Gforces";
        
        private IMyCockpit _cockpit;
        
        private List<Vector3> _accelList = new List<Vector3>();

        private float GLocTimer;

        private const float BlackoutAcell = 8f;
        private const float TunnelVisionAcell = 4f;

        //ok, what do I need to do
        /* I need to check the current acceleration of the grid
         if its above a threshold, then the pilot will start seeing a vingette on their screen
         if it gets above a higher threshold, the pilot can black out and any inputs to the grid are maintained.
         but having drugs in the cockpit will allow the pilot to reach much higher G forces before the effects start
         
         
         
         */
        
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyLog.Default.WriteLineAndConsole(ModName + $" JumpGameLogicComponent for block {Entity.DisplayName} ({Entity.EntityId})");
            try { throw new InvalidOperationException("break my point"); } catch(Exception) {} //debugging trigger
            _cockpit = (IMyCockpit)Entity;
            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            
        }
        
        public override void UpdateBeforeSimulation()
        {
            _accelList.Add(_cockpit.CubeGrid.Physics.LinearAcceleration);
            if (_accelList.Count >= 60) _accelList.RemoveAt(0);
            if (_cockpit.IsOccupied && _cockpit.ControllerInfo.IsLocallyHumanControlled())
            {
                CockpitSessionComp.Instance.Vignette.Visible = true;
            }
            else
            {
                CockpitSessionComp.Instance.Vignette.Visible = false;
            }
            //calculate average
            var avg = _accelList.Sum(x => x.Length()) / _accelList.Count;
            if (avg > TunnelVisionAcell)
            {
                //then start adding to the Timer
                var mult = (avg - TunnelVisionAcell) + 1;//so it stays above 1
                GLocTimer += mult;
                //and make the vingette start fading in
                CockpitSessionComp.Instance.Vignette.BillBoardColor = 
                    new Color(0f, 0f, 0f,  255*(GLocTimer/3600f));
            }
            else if (avg < BlackoutAcell && GLocTimer > 0)
            {
                //then they are below the threshold, reduce the counter
                GLocTimer -= BlackoutAcell - avg;
                if (GLocTimer < 0) GLocTimer = 0;
                CockpitSessionComp.Instance.Vignette.BillBoardColor = 
                    new Color(0f, 0f, 0f,  255*(GLocTimer/3600f));
            }

            if (GLocTimer > 3600)
            {
                //then you are blacked out
                //not going to be implementing that yet
            }
            
        }
        
        
    }
}