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
        private const int INTERVAL_SECONDS = 14400;
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

            _intervalTicks = INTERVAL_SECONDS * 1;
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
                   "Sector patrol complete—no contacts of concern.",
                    "Flight plan updates required before orbital adjustment.",
                    "Hold short at marker BRAVO; traffic inbound.",
                    "Advisory: Launch corridor GREEN for the next window.",
                    "Advisory: minor debris field reported in lane C-12. Inform Kessler crews.",
                    "Ping response delayed network-wide; expect latency.",
                    "Docking priority granted to medical transport.",
                    "Watchtower's reporting clear skies for the day.",

                    "Scopes are picking up unusual congestion around Polemos.",
                    "Commencing monitoring on outbound transit certifications.",
                    "Rotating Customs Enforcement shifts. EED has been advised.",
                    "Hold short at marker DELTA; pads are full.",
                    "Advisory: Launch corridor is RED for the next two cycles.",
                    "Realigning Watchtower scan array.",
                    "Adjusting Warden search patterns.",
                    "Contraband detected, inform Overlord.",
                    "Standing down Watchtower-1.",
                    "Watchtower-2 is cycling power.",
                    "Be advised, Watchtower array is recalibrating.",
                    "Tripoli Flight Control, be advised - high congestion.",
                    "Nairobi Flight Control, do you read?",
                    "Cujo 5-1, flight of two Kano-pattern gunships, ready for tasking.",
                    "Overlord, be advised - Cujo 5-1 and Vantage 3-1 are standing by for tasking.",
                    "Retasking all available flights to temporary EED duties.",
                    "Warden is reporting two new orbital assets ready for tasking.",
                    "Kessler crews relaying the all-clear.",
                    "Watchtower, Vendetta 4-1, flight of two Lusaka ships assigned to Kessler crews out of Nairobi. Our work out here is done, standing by for tasking.",
                },
                Surnames = new[]
                {
                    "VSV-12","EED-23","EED-77","K-99","K-72","V-49",
                    "V-87","22-Petrov","VK-24","Officer","Commander","Superintendent","KB-25","Chen-29","LCR-1"
                },
                InitMessage = "Entering Coalition-controlled space - you are under Coalition flight governance."
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
                    "Shipment receipts must be posted within two cycles.",
                    "Notice: surcharge applied to unregistered cargo crates.",
                    "Manifest mismatch flagged, report to the Registry.",
                    "Exchange rate looks good for the day; minor spread on bulk ore.",
                    "Audit sweep in effect. Keep your paperwork tight.",
                    "Bonded freight lane open, present bill of lading.",
                    "Tax relief for certified salvage closes at sundown.",
                    "Could this day get any longer?",
                    "If it isn't stamped, it's not going anywhere. Do the paperwork.",
                    "Grease the docking clamps this time, please. Paint's scuffed enough as is.",
                    "We got a counterfeiter over here. What? Yes, of course I know it's fake.",
                    "Heard there's an auction out there tonight. Think they're selling what I think they're selling.",
                    "Disputing this invoice. It just doesn't add up.",
                    "There's a holiday on Geras? Why am I always the last to know?",
                    "Receipts above our threshold require at least two cosigners.",
                    "Brokerage open for business, come through.",
                    "Liqueur restocked at Veka, I've got post-shift plans.",
                    "Market escrow's hitting extra hard today.",
                    "Number your pallets lest you'd like them impounded.",
                    "Weighbridge variance above tolerance, going to need a re-measure.",
                    "Apply for a tariff remission on life-support goods at Desk C.",
                    "Ledger statements now available for select clients at local kiosks.",
                    "Scrip redemptions capped today, adjust accordingly.",
                    "Routine maintenance scheduled on that loading bay, thanks.",
                    "You're going to be paying extra, transporting that much organic mass.",
                    "We don't do salvage titles here.",
                    "Major spill at Bay 4-C. Get a hazmat hardsuit team here ASAP.",
                    "Finalize those manifest changes before I have to come down there.",
                    "Exchange auditors boarding at random, cooperate fully."
                },
                Surnames = new[]
                {
                    "Vekara","Visegrad","Antos","Antalek","Veyran","Karric",
                    "Lakatos","Korval","Vancura","Vorn","Tamas","Veyda","Brannic",
                    "Orman","Czarda","Mirek","Halvek","Vorric"
                },
                InitMessage = "Welcome to Csillan territory - behave yourself."
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
                    "Production targets on track, we might be looking at a pizza party at this rate.",
                    "Contract window opened for satellite servicing.",
                    "Reading a few new contacts in orbit.",
                    "Does she ever SHUT UP?!",
                    "Safety reminder: EVA in designated zones only.",
                    "Quality Assurance is going to have a field day with this one.",
                    "Unscheduled ping from utility node - Regional Directive has been informed.",
                    "Wait for your clearance codes, this is a restricted berth.",
                    "Comply with Security Contractor searches - these are authorized under your contract.",
                    "Heading back home, whole lot of nothing on that one.",
                    "Noncompliant tooling flagged, we've just touched base with QA.",
                    "Get your foreman on the line, contractor check-ins are due.",
                    "Metrics review upcoming, inform our guys down there to look sharp.",
                    "Maintenance period begins in five, secure whatever's not strapped down.",
                    "Network latency above DuPont, check your channels.",
                    "Safety stand-down at pad E—incident under review.",
                    "Man, shareholder value just gets me feeling a certain way.",
                    "Access badge anomalies detected - someone get SpaceControl down here.",
                    "Client demonstration scheduled, polish your bays, gear, and signage.",
                    "Asset transfer to local transfer node approved - we'll handle that shipment now.",
                    "We've got missiles to spend, quarterly reports are due.",
                    "Remember, every round is billable.",
                    "Lunch break's over. You're lucky to get one.",
                    "Remember what we're here for - shareholder value.",
                    "Breach-of-contract warnings issued to delinquent vendors.",
                    "Drone traffic dense near tower, do as the ATC tells you.",
                    "Hazard pay activated for night-side EVA.",
                    "New IFF codes are going out soon, stand by.",
                    "Good news. Quality hold lifted on fasteners lot A-17.",
                    "Backlog on the filters are clear, tell Navigation we can all breathe a little more easily now.",
                    "Personnel shuttle's running behind again, someone get me a foreman.",
                    "Wear your safety equipment, we're not paying you to get hurt.",
                    "We've got a high-G burn coming up, check your seals and prepare for HCP-221 cycling.",
                    "Auditing starts today, get those reports looking sharp.",
                    "Unauthorized use of company cranes will void coverage.",
                    "If I have to work one more shift with this guy, EVERYONE will feel it."
                },
                Surnames = new[]
                {
                    "Jeffries","Calhoun","Briggs","Sato","Ellison","Kearns",
                    "Feldman","Rourke","Ashford","Chang-Hartlett","Cuirass-Allison","Bennett","Harlow","Preston","Collins",
                    "Han","O'Rourke","Barker","Graves","Donovan","Hollister","Quinn","Nakamura","Higashikata","Matsumoto","Kato",
                },
                InitMessage = "Hello and welcome to the right side of space! Patching you into corporate channels."
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

                MyAPIGateway.Utilities.ShowMessage("Regional Authority", "Registered jurisdiction: " + _activeFactionStr);
            }
            catch (Exception e)
            {
               
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
