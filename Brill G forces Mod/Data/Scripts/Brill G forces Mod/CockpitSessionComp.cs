using VRage.Game.Components;
using Draygo.API;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Utils;
using VRageMath;

// ReSharper disable once CheckNamespace
namespace Brill.Gforces
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CockpitSessionComp : MySessionComponentBase
    {
        public static CockpitSessionComp Instance;
        
        public HudAPIv2 HudAPI;

        public bool FirstRun = true;

        public bool WasInCockpit;

        public HudAPIv2.BillBoardHUDMessage Vignette;
        
        private const string VignetteTexture = "GLocVignette";
        
        public static bool IsDedicatedServer =>
            MyAPIGateway.Multiplayer.MultiplayerActive && MyAPIGateway.Utilities.IsDedicated;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            Instance = this;
            Instance.HudAPI = new HudAPIv2();
            if (Instance.HudAPI == null)
                MyAPIGateway.Utilities.ShowMessage("G forces Mod", "TextHudApi failed to register");
        }

        public override void UpdateBeforeSimulation()
        {
            if (Instance.HudAPI.Heartbeat && Instance.FirstRun)
            {
                var vignetteTexture = MyStringId.GetOrCompute("VignetteTexture");
                Instance.Vignette = new HudAPIv2.BillBoardHUDMessage(vignetteTexture, Vector2D.Zero, new Color(255, 255, 255, 0),
                    Shadowing: false)
                { 
                    Visible = false
                };
                Instance.FirstRun = false;
            }
            if (!WasInCockpit && MyAPIGateway.Session.Player.Controller.ControlledEntity is IMyCockpit)
            {
                Instance.Vignette.Visible = true;
                WasInCockpit = true;
            }
            else if (WasInCockpit && MyAPIGateway.Session.Player.Controller.ControlledEntity is IMyCockpit == false)
            {
                Instance.Vignette.Visible = false;
                WasInCockpit = false;
            }
        }
    }
}