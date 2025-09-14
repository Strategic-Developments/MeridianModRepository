using Sandbox.Game;             
using Sandbox.ModAPI;                  
using System;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;                       

namespace YourModNamespace
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RandomChatBroadcaster : MySessionComponentBase
    {
       
        private const string ACTIVE_FACTION_DEFAULT = "CCAS"; // "CCAS" | "CSILLA" | "ENCORP"
        private const int INTERVAL_SECONDS = 5;
        private const int STARTUP_DELAY_SECONDS = 1;

        
        private const string CONFIG_FILENAME = "CommSystem.cfg";

       
        private bool _ready;
        private bool _printedInit;
        private bool _configApplied;      
        private int _ticks;
        private int _intervalTicks;
        private int _startupDelayTicks;
        private readonly Random _rng = new Random();

        
        private int _lastMessageIndex = -1;

        
        private string _activeFactionStr = ACTIVE_FACTION_DEFAULT;

       
        private class FactionProfile
        {
            public string Name;
            public Color FactionColor;
            public string[] Titles;
            public string[] Messages;
            public string[] Surnames;
            public string InitMessage;
        }

        private FactionProfile _ccas;
        private FactionProfile _csilla;
        private FactionProfile _encorp;
        private FactionProfile _profile;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            _intervalTicks = INTERVAL_SECONDS * 240;
            _startupDelayTicks = STARTUP_DELAY_SECONDS * 1;

         
            _ccas = new FactionProfile
            {
                Name = "Centauri Coalition",
                FactionColor = new Color(255, 133, 34), // Orange
                Titles = new[]
                {
                    "CSC Ops", "CCAS Control", "CFX-S Flight", "CCAS Dispatch", "CCAS Skywatch",
                    "ATC-S Watchtower", "ATC Overlord","Lunar Directorate", "ATC Warden"
                },
                Messages = new[]
                {
                    "Maintain corridor C-12, speed 75 or less.",
                    "Hold short at marker BRAVO; traffic crossing.",
                    "Cleared approach, Dock 4. Follow lead lights.",
                    "Negative vector change; remain on assigned track.",
                    "Traffic alert: unknown contact bearing 090, 1km, co-altitude.",
                    "Squawk ident on ping; signal weak.",
                    "Expedite vacate of lane A-3; maintenance convoy inbound.",
                    "Line up and wait, pad Echo-2; departing traffic on the roll.",
                    "Go-around approved; congestion on final.",
                    "Pushback approved; call ready for taxi.",
                    "Taxi via Alpha, then Bravo; hold short Charlie.",
                    "Docking corridor green for the next window.",
                    "Wake caution: heavy tug departing outer ring.",
                    "EVA ops near Gate 7; keep clear 500 meters.",
                    "Confirm tether state before crossing the spine.",
                    "Vector 270 for sequence; expect delay two minutes.",
                    "Stack level two saturated; remain in hold.",
                    "Monitoring link degraded; report visual when able.",
                    "Beacon sync in progress; brief outages expected.",
                    "Priority medevac inbound; yield right of way.",
                    "Tow underway in lane D-1; reduce closure rate.",
                    "Range cold for test firings; maintain present course.",
                    "Surface ascent traffic in corridor B; no conflicts.",
                    "Controller handoff complete; contact Tower Prime on channel 3."
                },
                Surnames = new[]
                {
                    "VSV-12","EED-23","EED-77","K-99","K-72","V-49",
                    "V-87","22-Petrov","VK-24","Officer","Commander","Superintendent","Zhukov-45",
                    "44-Kaira","Volkova-91","KB-25","Chen-29","LCR-1"
                },
                InitMessage = "Centauri Coalition Aerospace Service online — you are under Coalition flight governance."
            };

            _csilla = new FactionProfile
            {
                Name = "Csillar Banking Clan",
                FactionColor = new Color(204, 51, 51), // Persian Red
                Titles = new[]
                {
                    "Csillar Clerk", "Ledger Office", "Exchange Gatehold", "Customs Csillar",
                    "Registry Veka", "Mandator Bladesman", "Tariff Board", "Lower Bladesman"
                },
                Messages = new[]
                {
                    "Keep the line moving; pallets off first, talk later.",
                    "If it isn't stamped, it isn't sailing. Do the paper.",
                    "Straps are frayed; swap them before the foreman sees.",
                    "Tug's on the way; hold that hull steady.",
                    "Gantry 3 is squealing again; grease it or it goes offline.",
                    "Coffee break's over; back on the winch.",
                    "Watch your fingers on the knuckle boom.",
                    "Weighbridge says you're heavy; drop a crate or pay the fee.",
                    "Berth fee's ticking; tie down and clear the lane.",
                    "Dock rat tried to skirt customs; don't be that guy.",
                    "Sling that crate low; wind's up.",
                    "Union steward's counting heads; don't ghost the shift.",
                    "Foreman wants ore up front, scrap to the rear.",
                    "Hauler's late; looks like overtime.",
                    "Crane 2 is down; hand-bomb what you can.",
                    "Mark your pallets; no chalk, no claim.",
                    "Keep the berth clean; nails and straps off the deck.",
                    "Lights on in Bay C; night shift's ours.",
                    "Customs dog is sniffing; stash your lunches proper.",
                    "If the seal's broke, it stays here.",
                    "Don't cheap the straps; use the good ones.",
                    "Tugs have right of way; make a hole.",
                    "Get your boots on; deck's slick.",
                    "Stevedores to Gatehold; big ship turning."
                },
                Surnames = new[]
                {
                    "Vekara","Visegrad","Antos","Antalek","Veyran","Karric",
                    "Lakatos","Korval","Vancura","Vorn","Tamas","Veyda","Brannic",
                    "Orman","Czarda","Mirek","Halvek","Vorric"
                },
                InitMessage = "Csillar Exchange channels active — all traffic subject to audit and tariff."
            };

            _encorp = new FactionProfile
            {
                Name = "ENCORP",
                FactionColor = new Color(115, 184, 255), // Light Blue
                Titles = new[]
                {
                    "EnCorp Dispatcher", "EnCorp Security Liaison", "EnCorp Terminal Manager", "Contractor Operations",
                    "Systems Controller","Contractor CSO", "Contracts Officer", "Logistics Hub Supervisor", "Quality Controlman"
                },
                Messages = new[]
                {
                    "Heads up team: throughput is off target; unblock Line B.",
                    "Reminder: log your time in the portal before end of shift.",
                    "New SLA on satellite turnaround; align deliverables.",
                    "We have a blocker on Pad E; escalating to Facilities.",
                    "Action item: update the risk register after today's incident.",
                    "Network change window at 1800; expect brief impact.",
                    "Stand-up in five at the ops board; keep it tight.",
                    "Please park scope creep; focus on MVP for this release.",
                    "KPI check: safety, quality, schedule. Call it green or yellow.",
                    "Procurement approved fasteners; pick tickets are live.",
                    "Audit on the floor; badges visible and aisles clear.",
                    "If it isn't in the system, it didn't happen. Close your work orders.",
                    "RCA for yesterday's outage posted; read and sign.",
                    "We're capacity constrained; prioritize revenue-critical tasks.",
                    "Stakeholders on-site this afternoon; clean bays and signage.",
                    "Reminder: PPE is not optional; HSE is tracking.",
                    "Firmware push to drones at 1300; keep airspace open.",
                    "Inventory variance outside tolerance; recount bin 2A.",
                    "Contract window opens at 0900; vendors queue at Gate 2.",
                    "Coordinate cranes through Dispatch; no ad-hoc lifts.",
                    "Shift handoff needs a written note; no verbal only.",
                    "Lunch-and-learn moves to Tuesday; invites updated.",
                    "Parking-lot side projects; sprint closes tonight.",
                    "If you see something unsafe, stop the line and call it."
                },
                Surnames = new[]
                {
                    "Jeffries","Calhoun","Briggs","Sato","Ellison","Kearns",
                    "Feldman","Rourke","Ashford","Chang-Hartlett","Cuirass-Allison","Bennett","Harlow","Preston","Collins",
                    "Han","O'Rourke","Barker","Graves","Donovan","Hollister","Quinn","Nakamura","Higashikata","Matsumoto","Kato",
                },
                InitMessage = "EnCorp systems detected — patching you into corporate ops and security bandwidth."
            };

            
            ResolveProfileFromActive(ACTIVE_FACTION_DEFAULT);
        }

        public override void UpdateAfterSimulation()
        {
           
            if (MyAPIGateway.Multiplayer == null || MyAPIGateway.Multiplayer.IsServer != true)
                return;

            if (!_configApplied)
            {
                if (MyAPIGateway.Session == null)
                    return; 

                EnsureWorldConfig();     
                ResolveProfileFromActive(_activeFactionStr);
                _configApplied = true;
            }

            if (!_ready)
            {
                _ticks++;
                if (_ticks < _startupDelayTicks)
                    return;

                _ticks = 0;
                _ready = true;

                if (!_printedInit)
                {
                    MyVisualScriptLogicProvider.SendChatMessageColored(
                        _profile.InitMessage,
                        _profile.FactionColor,
                        _profile.Name
                    );
                    _printedInit = true;
                }
            }

            _ticks++;
            if (_ticks < _intervalTicks)
                return;

            _ticks = 0;

            var author = ComposeAuthor();
            var message = PickNoRepeat(_profile.Messages, ref _lastMessageIndex);
            if (!string.IsNullOrEmpty(message))
            {
                MyVisualScriptLogicProvider.SendChatMessageColored(
                    message,
                    _profile.FactionColor,
                    author
                );
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
        }

       
        private void EnsureWorldConfig()
        {
            var type = typeof(RandomChatBroadcaster);

            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(CONFIG_FILENAME, type))
                {
                    using (var w = MyAPIGateway.Utilities.WriteFileInWorldStorage(CONFIG_FILENAME, type))
                    {
                        w.WriteLine("# Meridian Comm System (WORLD) Configuration");
                        w.WriteLine("# Set JURISDICTION to CCAS, CSILLA, or ENCORP");
                        w.WriteLine("# You may also use FACTION or ACTIVE_FACTION as the key.");
                        w.WriteLine("JURISDICTION=" + ACTIVE_FACTION_DEFAULT);
                    }
                    MyAPIGateway.Utilities.ShowMessage("CommSystem", "Created world config: Storage/<mod>/<" + type.FullName + ">/" + CONFIG_FILENAME);
                    _activeFactionStr = ACTIVE_FACTION_DEFAULT;
                    return;
                }

                using (var r = MyAPIGateway.Utilities.ReadFileInWorldStorage(CONFIG_FILENAME, type))
                {
                    string line;
                    string chosen = null;

                    while ((line = r.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                            continue;

                        var parts = line.Split(new char[] { '=', ':' }, 2);
                        if (parts.Length != 2) continue;

                        var key = parts[0].Trim().ToUpperInvariant();
                        var val = parts[1].Trim();

                        if (key == "JURISDICTION" || key == "FACTION" || key == "ACTIVE_FACTION")
                            chosen = val;
                    }

                    _activeFactionStr = string.IsNullOrEmpty(chosen) ? ACTIVE_FACTION_DEFAULT : chosen;
                }

                MyAPIGateway.Utilities.ShowMessage("CommSystem", "Loaded jurisdiction: " + _activeFactionStr);
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowMessage("CommSystem", "World config error: " + e.Message);
                _activeFactionStr = ACTIVE_FACTION_DEFAULT;
            }
        }

        private void ResolveProfileFromActive(string factionStr)
        {
            var f = (factionStr ?? ACTIVE_FACTION_DEFAULT).ToUpperInvariant();
            switch (f)
            {
                case "CCAS":   _profile = _ccas;   break;
                case "CSILLA": _profile = _csilla; break;
                case "ENCORP": _profile = _encorp; break;
                default:       _profile = _encorp; break;
            }
            _lastMessageIndex = -1; 
        }

       
        private string ComposeAuthor()
        {
            var title = Pick(_profile.Titles);
            var surname = Pick(_profile.Surnames);
            if (string.IsNullOrEmpty(title)) title = _profile.Name;
            if (string.IsNullOrEmpty(surname)) return title;
            return title + " " + surname;
        }

        private T Pick<T>(T[] arr)
        {
            if (arr == null || arr.Length == 0) return default(T);
            return arr[_rng.Next(arr.Length)];
        }

        private string PickNoRepeat(string[] arr, ref int lastIndex)
        {
            if (arr == null || arr.Length == 0) return string.Empty;
            if (arr.Length == 1) { lastIndex = 0; return arr[0]; }
            int idx;
            do { idx = _rng.Next(arr.Length); } while (idx == lastIndex);
            lastIndex = idx;
            return arr[idx];
        }
    }
}
