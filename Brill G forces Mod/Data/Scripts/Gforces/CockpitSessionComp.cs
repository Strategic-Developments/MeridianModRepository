using VRage.Game.Components;
using Draygo.API;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace YourName.ModName.Data.Scripts.Gforces
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CockpitSessionComp : MySessionComponentBase
    {
        
        //vingette texture = GLocVignette
        
        public HudAPIv2 HudAPI;

        public bool FirstRun = true;

        public HudAPIv2.BillBoardHUDMessage Vignette;
        
        public static bool IsDedicatedServer =>
            MyAPIGateway.Multiplayer.MultiplayerActive && MyAPIGateway.Utilities.IsDedicated;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            HudAPI = new HudAPIv2();
            if (HudAPI == null)
                MyAPIGateway.Utilities.ShowMessage("G forces Mod", "TextHudApi failed to register");
        }

        public override void UpdateBeforeSimulation()
        {
            if (HudAPI.Heartbeat && FirstRun)
            {
                var vignetteTexture = MyStringId.GetOrCompute("GLocVignette");
                Vignette = new HudAPIv2.BillBoardHUDMessage(vignetteTexture, Vector2D.Zero, Color.Transparent,
                    Shadowing: false)
                {
                    Visible = false
                };
                FirstRun = false;
            }
        }
    }
}