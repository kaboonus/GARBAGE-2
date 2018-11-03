/*GARBAGE-2v4 : Gift Anomaly Ratting  salvAGE Bot with Sanderling. This bot will take only anomalies, take the loot and use salvage drones for salvaging
The child of Sanderling--ratting-bot-anomaly-or-asteroids !!
News: create a file log named after your first name char ( fill in the name) in the executable folder of Sanderling:
        - structure log: total amount of your wallet+ session isk/loot measured into station
Download the latest release of Sanderling from https://github.com/Arcitectus/Sanderling/releases 
before you start the boot you have to prepare your game interface like that:
-create your combat tab in interface with rats+players+solid/large colidables objects+non-empty wrecks like here:
 https://forum.botengine.org/t/ratting-bot-anomaly-and-or-asteroids-with-sanderling/1206/55?u=kaboonus and here:
 https://forum.botengine.org/t/ratting-bot-anomaly-and-or-asteroids-with-sanderling/1206/44?u=kaboonus
-create your salvage tab with : ALL wrecks( the empty ones visibles) +rats+players+colidables
-put their name in settings. I advice using simple words like loot/combat;
-Orbit range  in game : 43km
Warning!!: When the bot starts AND when the ship enter in station or it measure your wallet; DO NOT TOUCH THE MOUSE!! or you will take crash
Warning!!: I don't know if is working with more than 1 tractor beam - AND I don't care :d . Likewise for  usesalvageDrones = false and takeloot=false
fit used for tests:
[Vexor Navy Issue, Vexor]
Drone Damage Amplifier II 4x; Dread Guristas Omnidirectional; Tracking Enhancer; Drone Navigation Computer I; Large Compact Pb-Acid Cap Battery;
100MN Y-S8 Compact Afterburner; Omnidirectional Tracking Link I; Drone Link Augmentor I; Small Tractor Beam I; Medium Ancillary Current Router I
Medium Capacitor Control Circuit II 2x
Salvage Drone I x5;  Combat Drones x7
-new: max number in local. warp home when the players in local are more than this value;
-take only faction loot, all loot or not at all
*/
using BotSharp.ToScript.Extension;
using Parse = Sanderling.Parse;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using System.IO;
using System.Collections.Generic;
//	begin of configuration section ->
string VersionScript = "GARBAGE-2v4";
Host.Delay(4111);
 Host.Log( "Gathering and processing some info to be used later ");
 var InfoPannelRegion =
		Measurement?.InfoPanelCurrentSystem?.HeaderText.RemoveXmlTag()?.Trim();
var CurrentRegion = InfoPannelRegion.Substring(InfoPannelRegion.LastIndexOf(' ')).TrimStart();
var MyOwnChar  =chatLocal?.ParticipantView?.Entry?.FirstOrDefault(myflag =>myflag?.FlagIcon == null);
var firstname = MyOwnChar?.NameLabel?.Text;
Host.Log( "    - your current region is:");
 Host.Log( CurrentRegion);
 Host.Log( "   - Copy exactly and fill in at lines 54-58 ");

string CharName =firstname.Remove(firstname.IndexOf(" ")).ToLower();// Your char name. The bot will create a filename with this name, into executable sanderling folder
 Host.Log( CharName);
var RetreatOnNeutralOrHostileInLocal =true;   // true or false :warp to RetreatBookmark when a neutral or hostile is visible in local.
var MaxInLocal = 45;
var RattingAnomaly = true;	// true or false:	when this is set to true, you take anomaly
string WarpToAnomalyDistance = "Within 50 km"; // variants(just copy paste) : "Within 10 km" "Within 20 km" "Within 30 km" "Within 50 km" "Within 70 km" "Within 100 km"   "Within 0 m"
var UseSalvageDrones = true; //if this is true will launch all drones one by one
var TakeLoot = true;// false is you dont want at all to take the loot
var TakeOnlyCommanderLoot =true;
var TakeALLLoot = false;//wreck + cargos
string LabelNameSalvageDrones = "Salvage Drone I"; //no reason to change
string AutoNpcFaction;
Dictionary<string,string> region=new Dictionary<string,string>();
region.Add("Delve", "blood");
region.Add("Fountain", "serpentis");
region.Add("Blind", "guristas");


region.TryGetValue(CurrentRegion,out AutoNpcFaction);

string NpcFaction = AutoNpcFaction; // or if you want, put directly "serpentis" "blood"  "angel"
string LabelNameAttackDrones;//ex:  Imperial Navy Praetor ;  "Caldari Navy Wasp"  dunno if it work with partial name like "praetor"  or "wasp"
Dictionary<string,string> faction=new Dictionary<string,string>();
faction.Add("serpentis", "Caldari Navy Wasp");
faction.Add("blood", "Federation Navy Ogre");
faction.Add("angel", "Caldari Navy Wasp");
faction.Add("guristas", "Caldari Navy Wasp");
faction.Add("sansha", "Imperial Navy Praetor");
faction.Add("mordus", "Imperial Navy Praetor");
faction.Add("drones", "Imperial Navy Praetor");
faction.TryGetValue(NpcFaction,out LabelNameAttackDrones);
var ActivatePerma = true;//
string salvagingTab = "colly";
string rattingTab = "combat";
string UnsupportedArmorRepairer = "Medium 'Accommodation' Vestment Reconstructer I"; // full name of module
string messageText = "old site";
string messageTextDread = "old site dread";
// SESSION/DT TImers
var minutesToDT = 10; //value in minutes before the DT of server ( already -1min than real DT of server)
var hoursToDT = 0;//value in h before the DT of server
var hoursToSession = 10; //wanna play for 5 hours
var minutesToSession = 11;// and 15 min
//// in the end you'll play for 5h and 11m if the DT did not come
var MinimDelayUndock = 3;//in seconds
var MaximDelayUndock = 30;//in seconds
// protection distance, from this distance you forget about this rats
/// Ranges
var shipRangerMax = 63; // in km, you orbit arround them
var maxDistanceToRats = 120;// in km, you forget about them
var MaxDronesRange = 60000;
int? DistanceCelestial = 30000;
/////settings anomaly
string AnomalyToTakeColumnHeader = "name";  // the column header from table ex : name
string AnomalyToTake = "Forsaken Hub"; // his name , ex:  "forsaken hub" or " combat"
string IgnoreAnomalyName = "large|Belt|Haven|asteroid|drone|forlorn|rally|sanctum|blood hub|serpentis hub|hidden|port|den";// what anomaly to ignore : haven|Belt|asteroid|drone|forlorn|rally|sanctum|blood hub|serpentis hub|hidden|port|den
string IgnoreColumnheader = "Name";//the head  of anomaly to ignore
// you have to run from this rats:
string runFromRats = "♦|Titan|Dreadnought|Autothysian";// you run from him
//celestial to orbit
string celestialOrbit = "broken|pirate gate"; //ex: broken|pirate gate
string CelestialToAvoid = "Chemical Factory"; // ex: Chemical Factory //this one make difference between haven rock and gas
// wrecks commander etc
string commanderNameWreck = "Commander|Dark Blood|true|Shadow Serpentis|Dread Gurista|Domination Saint|Gurista Distributor|Sentient|Overseer|Spearhead|Dread Guristas|Estamel|Vepas|Thon|Kaikka|True Sansha|Chelm|Vizan|Selynne|Brokara|Dark Blood|Draclira|Ahremen|Raysere|Tairei|Cormack|Setele|Tuvan|Brynn|Domination|Tobias|Gotan|Hakim|Mizuro";
string CargoWreck =  "cargo";
int DroneNumber = 5;// set number of drones in space; ex: 5
int TargetCountMax = 2; //target numbers; ex: 4
//set  hardeners, repairer, set true if you want to run them all time, if not, there is set StartArmorRepairerHitPoints
var ActivateHardener = true;// true or false ;true for activated permanent
var ActivateOmni = true; //true or false ; true for activated permanent
var ActivateSensorBoost = true;
var ActivateArmorRepairer = false; // true or false ; true for activated permanent
var ActivateShieldBooster = false;

string OmniSup = "Omnidirectional"; // ex: Omnidirectional Tracking Link I ( you can use an autotarget module in place)
string SensorSup = "Sensor";
string TractorBeast = "Tractor";
string [] PermanentActive = new [] {
"Omnidirectional", "Sensor", "Auto", "rash"



};
///////    RATTING SHIP //////////////
var ArmorRepairsCount = 1; // how many you have
var AfterburnersCount = 1;// how many you have
var ShieldBoosterCount = 0; //??
var HardenersCount = 0; // there is no need for more, unless if your modulemeasuretooltip give errors
var OmniCount = 0; // there is no need for more, unless if your modulemeasuretooltip give errors
var SensorBoostCount = 0; // there is no need for more, unless if your modulemeasuretooltip give errors
var TractorBeastCount = 0; // there is no need for more, unless if your modulemeasuretooltip give errors
//////////////////
var EmergencyWarpOutHitpointPercent = 45; // ex : 60 ; you warp home if your armor hp % is smaller that this value
var StartArmorRepairerHitPoints = 95; // armor hp value in % , when it starts armor repairer IF is present
var StartShieldRepairerHitPoints = 35;// Shield hp value in % , when it starts shield booster IF is present
var StopCapacitorValue = 77; //capacitor value when you stop Painter
//
bool returnDronesToBayOnRetreat = true;
string UnloadBookmark = "home"; // your  home bookmark
//	Name of the container to unload to as shown in inventory.
string UnloadDestContainerName = "Item Hangar"; //supposed it is Item Hangar
//	Bookmark of place to retreat to to prevent ship loss.

string RetreatBookmark = UnloadBookmark;
/////YOU HAVE NO REASON TO CHANGE THIS ONES!!!!!
var lockTargetKeyCode = VirtualKeyCode.LCONTROL;// lock target

var targetLockedKeyCode = VirtualKeyCode.SHIFT;//locked target
int Dancer;
var orbitKeyCode = VirtualKeyCode.VK_W;// if you changed the default key
var attackDrones = VirtualKeyCode.VK_F;// if you changed the default key
/////
var EnterOffloadOreHoldFillPercent = 85;//	percentage of ore hold fill level at which to enter the offload process and warp home.
var FullCargoMessage = false;
const string StatusStringFromDroneEntryTextRegexPattern = @"\((.*)\)";

static public string StatusStringFromDroneEntryText(this string droneEntryText) => droneEntryText?.RegexMatchIfSuccess(StatusStringFromDroneEntryTextRegexPattern)?.Groups[1]?.Value?.RemoveXmlTag()?.Trim();
var startSession = DateTime.Now; // your local time, just for show
var playSession = DateTime.UtcNow.AddHours(hoursToSession).AddMinutes(minutesToSession);
var dateAndTime = DateTime.UtcNow;
Host.Log("UTC start at:  " +dateAndTime.ToString(" dd/MM/yyyy HH:mm:ss")+ " .");

var date = dateAndTime.Date;
var eveRealServerDT =date.AddHours(11).AddMinutes(-1);
if (eveRealServerDT < dateAndTime)
{
eveRealServerDT = eveRealServerDT.AddDays(1);
Host.Log(" >  eveRealServerDT :  " +eveRealServerDT.ToString(" dd/MM/yyyy HH:mm:ss")+ " .");
}
var eveSafeDT = eveRealServerDT.AddHours(-hoursToDT).AddMinutes(-minutesToDT);
Host.Log(" >  eveSafeDT : " +eveSafeDT.ToString(" dd/MM/yyyy HH:mm")+ " .");
 //just some calcs
var MaxDistanceToRats = maxDistanceToRats*1000;
var ShipRangeMax = shipRangerMax*1000;
Dictionary<string,int> dict=new Dictionary<string,int>();
dict.Add("Within 0 m",0);
dict.Add("Within 10 km",1);
dict.Add("Within 20 km",2);
dict.Add("Within 30 km",3);
dict.Add("Within 50 km",4);
dict.Add("Within 70 km",5);
dict.Add("Within 100 km",6);

int x;
dict.TryGetValue(WarpToAnomalyDistance,out x);
var OldSiteExist = false;
var NoMoreRats = false;
var SiteFinished = false;
    int K=1;
    int initial = 0;
//	<- end of configuration section
string  Dasher => Measurement?.Tooltip?.FirstOrDefault()?.LabelText?.FirstOrDefault(entry =>entry.Text.RegexMatchSuccessIgnoreCase("\\d",RegexOptions.IgnoreCase))?.Text;

long HocusPocusPreparatus;
string Paracelsus;
var LookingForSomething = new Func<string, MemoryStruct.IUIElement>(texturePathRegexPattern =>
				Measurement?.Neocom?.Button?.FirstOrDefault(candidate => candidate?.TexturePath?.RegexMatchSuccess(texturePathRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false));
var MerryChristmas = LookingForSomething("wallet");
 Sanderling.MouseMove(MerryChristmas); Sanderling.WaitForMeasurement();


//
if (string.IsNullOrEmpty(Dasher))
{ Sanderling.MouseMove(MerryChristmas);
 Sanderling.WaitForMeasurement(); }
Paracelsus =Regex.Replace(Dasher ?? "", "[^0-9]+", "") ;
HocusPocusPreparatus = Convert.ToInt64(Paracelsus);
    Host.Log("                ⊙  Kaboonus Gift From Yesterday :  " +HocusPocusPreparatus.ToString("N0")+ "");

var SecCurentSystem = Measurement?.InfoPanelCurrentSystem?.SecurityLevelMilli.Value;

//LogMessageToFile("Hello, World");
var currentSystemLocationLabelText =
		Measurement?.InfoPanelCurrentSystem?.ExpandedContent?.LabelText
		?.OrderByCenterVerticalDown()?.FirstOrDefault()?.Text;
	var currentLocationName = RegexExtension.RemoveXmlTag(currentSystemLocationLabelText)?.Trim();
currentLocationName = currentLocationName.Remove(currentLocationName.IndexOf(" "));
    Host.Log("               System :  " +currentLocationName+ " ");




Func<object> BotStopActivity = () => null;
Func<object> NextActivity = MainStep;
for(;;)
{
MemoryUpdate();
Host.Log(
		" >   " +CharName?.ToUpper()+  "    Started  in " +currentLocationName+ " at: " +  startSession.ToString(" HH:mm") +// alternative (" dd/MM/yyyy HH:mm") 
		" ;   Logout in:  "  + ((TimeSpan.FromMinutes(logoutgame) < TimeSpan.Zero) ? "-" : "") + (TimeSpan.FromMinutes(logoutgame)).ToString(@"hh\:mm\:ss")+
		" ;   Sites  " + sitescount+ "" +
        " ;   ISK:  " + HocusPocus+ "" +
		" ;   HP: S: " + ShieldHpPercent + "% ; A: " + ArmorHpPercent + "%" +
		" ;   Local Count : " +localCount+
		" ;   Hostiles: " +(chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy)-1)+ " # Msg : "  + RetreatReason +
		"\n" +
        "                    >>  Loot:  " + StatusLoot+ "" +
		" ;   Drones (space): "  +(DronesInSpaceCount + DronesInBayCount)+ "( "+ DronesInSpaceCount +  " )"+
		" ;   Rats: " + ListRatOverviewEntry?.Length +
		" ;   Targets:  " + Measurement?.Target?.Length+
		" ;   Cargo: " + OreHoldFillPercent + "%" +
		" ;   Tractors (inactive): " + SetModuleTractorBeam?.Length + "(" + SetModuleTractorInactive?.Length + ")" +
		" ;   Wrecks: " + ListWreckOverviewEntry?.Length+
		" ;   NextAct: " + NextActivity?.Method?.Name);
		CloseModalUIElement();

if(Measurement?.WindowOther != null)
    CloseWindowOther();
if(Measurement?.WindowTelecom != null)
    CloseWindowTelecom();
if (Tethering)
{
StopAfterburner();
StopRepairer();
}
if (Measurement?.IsDocked ?? false)
    MainStep();
if(null != RetreatReason && !(Measurement?.IsDocked ?? false))
{
if (!Tethering)
{
        	Host.Log("               Tactical retreat,  reason  : " + RetreatReason + ".");
	//Console.Beep(500, 200);// he will beep a lot higher
    Console.Beep(369, 125);// this beeps are  better
	StopAfterburner();
    ActivateRepairerExecute();
    DroneReturnToBay();
	//ActivateRepairerExecute();
    if (listOverviewEntryEnemy?.Length > 0)
    {
        //ActivatePermaExecute();
        ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
        Host.Log("               Enemy on Grid!!!");
        Console.Beep(1500, 200);
        Console.Beep(1500, 200);
        Console.Beep(1500, 200);
    }
	 if (!Aligning)
	{
	 ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "align");
	}
    DroneReturnToBay();
    if (null !=RetreatReasonDread)
        {
            var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
            if (probeScannerWindow == null)
                Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
            var scanActuallyAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(ActuallyAnomaly);
                Host.Log("               I'm a chicken and I'm run from dread");
                deleteBookmark();
            SavingLocation ();
            if (null != scanActuallyAnomaly)
            { 
                ClickMenuEntryOnMenuRoot(scanActuallyAnomaly, "Ignore Result");
            }
        }
	if (ReadyForManeuver
	&&(!returnDronesToBayOnRetreat || null == WindowDrones
	|| (returnDronesToBayOnRetreat && 0 == DronesInSpaceCount)))
	{
	Host.Log("               Picard : Yes, I warping home( I know ... I know ... Is an Miracle!!) ");
	ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
	                SiteFinished = false;
        Console.Beep(1500, 200);
        K=1;
	}
	else
		{ DroneEnsureInBay();}
	//continue;
}
    if (null != RetreatReason || Tethering)
    {
         Host.Log("               retreat On Tethering Zone");
         MainStep ();
    }
	continue;
}
NextActivity = NextActivity?.Invoke() as Func<object>;
if(BotStopActivity == NextActivity)
	break;
if(null == NextActivity)
	NextActivity = MainStep;
Host.Delay(1111);
}
int? localCount => chatLocal?.ParticipantView?.Entry?.Count();
int? HulHpPercent => ShipUi?.HitpointsAndEnergy?.Struct / 10;
int? ShieldHpPercent => ShipUi?.HitpointsAndEnergy?.Shield / 10;
int? ArmorHpPercent => ShipUi?.HitpointsAndEnergy?.Armor / 10;
int? CapacitorHpPercent => ShipUi?.HitpointsAndEnergy?.Capacitor / 10;
bool DefenseExit =>
    (Measurement?.IsDocked ?? false) ||
    !(0 < ListRatOverviewEntry?.Length
    || ListRatOverviewEntry?.FirstOrDefault()?.DistanceMax > MaxDistanceToRats);
bool DefenseEnter =>
    !DefenseExit;
string RetreatReasonTemporary = null;
string RetreatReasonPermanent = null;
string RetreatReasonBumped = null;
string RetreatReasonCapsuled = null;
string RetreatReasonTimeElapsed = null;
string RetreatReasonDrones = null;
string RetreatReasonCargoFull = null;

string RetreatReasonDread = null;
string RetreatReason => RetreatReasonPermanent ?? RetreatReasonBumped
        ?? RetreatReasonCapsuled ?? RetreatReasonTimeElapsed
        ?? RetreatReasonTemporary ?? RetreatReasonDrones
        ?? RetreatReasonCargoFull
        ??  RetreatReasonDread ;
int? LastCheckOreHoldFillPercent = null;
int nn;
int sitescount = 0;
bool OreHoldFilledForOffload => Math.Max(0, Math.Min(100, EnterOffloadOreHoldFillPercent)) <= OreHoldFillPercent;
bool NoHostiles =>(!hostileOrNeutralsInLocal && RetreatOnNeutralOrHostileInLocal ) || (!RetreatOnNeutralOrHostileInLocal);
Func<object> MainStep()
{
    while (ReadyForManeuverNot)
    {
        Host.Delay(2111);
    return MainStep;
    }
    EnsureWindowInventoryOpen();
    EnsureWindowInventoryOpenActiveShip();
    if (Measurement?.IsDocked ?? false)
    {   FullCargoMessage = false;
        SiteFinished = false;
        Host.Delay(4111);
        while ( K>0)
            {   //LootValue();
                KaboonusTalk ();
                ReviewSettings();
                K--;
                return MainStep;
            }
        if ( ReasonTimeElapsed || ReasonCapsuled ||ReasonDrones)
        {
           Host.Log("               Refill your drones / Times up or you are capsuled = bot stop");
        //Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LMENU, VirtualKeyCode.SHIFT, VirtualKeyCode.VK_Q});
        Host.Delay(3111);
        return BotStopActivity;
		}
    while ((RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal) || tooManyOnLocal)
    { Host.Log("               I feel a great disturbance in the Force ... taking a nap into station until hostiles go from this system");
    Host.Delay(4111);
    return MainStep;
    }
        Host.Delay(4111);
     CheckLocation();
    EnsureWindowInventoryOpen();
    Sanderling.WaitForMeasurement();
    EnsureWindowInventoryOpenActiveShip();
    Sanderling.WaitForMeasurement();
    InInventoryUnloadItems();
    Sanderling.WaitForMeasurement();
    StackAll ();
    Sanderling.WaitForMeasurement();
        FullCargoMessage = false;

    if ((!hostileOrNeutralsInLocal && RetreatOnNeutralOrHostileInLocal ) || (!RetreatOnNeutralOrHostileInLocal) || !tooManyOnLocal)
    {
    Random rnd = new Random();
    int DelayTime = rnd.Next(MinimDelayUndock, MaximDelayUndock);
        Host.Log("               Keep your horses for :  " + DelayTime+ " s ");
        Host.Delay( DelayTime*1000);
         Undock ();
      //Host.Log("               used for test");
    }

    }
    if (ReadyForManeuver)
    {
        if(!OldSiteExist)
                CheckLocation();
           EnsureWindowInventoryOpen();
    EnsureWindowInventoryOpenActiveShip();
        if (0 < DronesInSpaceCount  &&  NoRatsOnGrid)
            DroneEnsureInBay();
        if (Tethering)
        {
            Host.Log("               I am In Tethering zone");
        if (HulHpPercent < 100 ||ArmorHpPercent < 100  ||ShieldHpPercent < 100)
        {while (HulHpPercent < 100 ||ArmorHpPercent < 100  ||ShieldHpPercent < 100 || AreDronesDamaged () )
        {
        Host.Log("               Luke > I try Master Yoda, ... I try ... to refill my HP !");
        Host.Delay(5823);

        }}
        else if(null != RetreatReason)
            ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
        if(NoHostiles && OldSiteExist)
            ReturnToOldSite ();
        }
        if (0 < DronesInSpaceCount  &&  NoRatsOnGrid)
        DroneEnsureInBay();

               Host.Log("               Refreshing news: I'm ready for rats");
        if(!ImDocking || !( Measurement?.IsDocked ?? false))
        ModuleMeasureAllTooltip();
        if (ActivatePerma)
            ActivatePermaExecute();
        if (0 == DronesInSpaceCount  &&  NoRatsOnGrid)
        {     
            if ( !Tethering)
            return InBeltMineStep;
            if (RattingAnomaly && (NoHostiles || !tooManyOnLocal))
            {
                    Host.Log("               I would like to spin around rocks");
                return TakeAnomaly;
            }
        }
    }
    if (ActivatePerma)
    ActivatePermaExecute();
	return InBeltMineStep;
}
Sanderling.Parse.IMemoryMeasurement Measurement =>
    Sanderling?.MemoryMeasurementParsed?.Value;
IWindow ModalUIElement =>
    Measurement?.EnumerateReferencedUIElementTransitive()?.OfType<IWindow>()?.Where(window => window?.isModal ?? false)
    ?.OrderByDescending(window => window?.InTreeIndex ?? int.MinValue)
    ?.FirstOrDefault();
IEnumerable<Parse.IMenu> Menu => Measurement?.Menu;
Parse.IShipUi ShipUi => Measurement?.ShipUi;
Sanderling.Interface.MemoryStruct.IMenuEntry MenuEntryLockTarget =>
    Menu?.FirstOrDefault()?.Entry?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("^lock"));
Sanderling.Interface.MemoryStruct.IMenuEntry MenuEntryUnLockTarget =>
    Menu?.FirstOrDefault()?.Entry?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("^unlock"));
Sanderling.Parse.IWindowOverview WindowOverview =>
    Measurement?.WindowOverview?.FirstOrDefault();
Sanderling.Parse.IWindowInventory WindowInventory =>
    Measurement?.WindowInventory?.FirstOrDefault();
IWindowDroneView WindowDrones =>
    Measurement?.WindowDroneView?.FirstOrDefault();
Tab OverviewTabActive =>
	Measurement?.WindowOverview?.FirstOrDefault()?.PresetTab
	?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 1500)
	?.FirstOrDefault();
Tab combatTab => WindowOverview?.PresetTab
	?.OrderByDescending(tab => tab?.Label.Text.RegexMatchSuccessIgnoreCase(rattingTab))
	?.FirstOrDefault();
Tab salvageTab => WindowOverview?.PresetTab
	?.OrderByDescending(tab => tab?.Label.Text.RegexMatchSuccessIgnoreCase(salvagingTab))
	?.FirstOrDefault();
var inventoryActiveShip = WindowInventory?.ActiveShipEntry;
var inventoryActiveShipEntry = WindowInventory?.ActiveShipEntry;

ITreeViewEntry InventoryActiveShipContainer =>
       WindowInventory?.ActiveShipEntry?.TreeEntryFromCargoSpaceType(ShipCargoSpaceTypeEnum.General);
DroneViewEntryItem[] AllDrones => WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryItem>()?.ToArray();
IInventoryCapacityGauge OreHoldCapacityMilli =>
    (InventoryActiveShipContainer?.IsSelected ?? false) ? WindowInventory?.SelectedRightInventoryCapacityMilli : null;
int? OreHoldFillPercent => OreHoldCapacityMilli?.Max > 0 ? ((int?)((OreHoldCapacityMilli?.Used * 100) / OreHoldCapacityMilli?.Max )) : 0 ;
var reasonCapsule  = false;
Sanderling.Accumulation.IShipUiModule[] SetModuleWeapon =>
	Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => (module?.TooltipLast?.Value?.IsWeapon ?? false) || (module?.TooltipLast?.Value?.IsWeapon?? false))?.ToArray();
Sanderling.Accumulation.IShipUiModule[] SetModulePainter =>
	Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => (module?.TooltipLast?.Value?.IsTargetPainter ?? false) || (module?.TooltipLast?.Value?.IsTargetPainter?? false))?.ToArray();

int?		WeaponOptimalRange => SetModuleWeapon?.Select(module =>
	module?.TooltipLast?.Value?.RangeOptimal ?? module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();
int?		WeaponMaxRange => SetModuleWeapon?.Select(module =>
	 module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();
int?		PainterOptimalRange => SetModulePainter?.Select(module =>
	module?.TooltipLast?.Value?.RangeOptimal ?? module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();
int?		PainterMaxRange => SetModulePainter?.Select(module =>
	 module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();
string OverviewTypeSelectionName =>
    WindowOverview?.Caption?.RegexMatchIfSuccess(@"\(([^\)]*)\)")?.Groups?[1]?.Value;
Parse.IOverviewEntry[] ListRatOverviewEntry => WindowOverview?.ListView?.Entry?.Where(entry =>
    (entry?.MainIconIsRed ?? false))
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"battery|tower|sentry|web|strain|splinter|render|raider|friar|reaver")) //Frigate
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"coreli|centi|alvi|pithi|corpii|gistii|cleric|engraver")) //Frigate
    ?.OrderByDescending(entry => entry?.ListColumnCellLabel[3].Value.NumberParseDecimal())
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelior|centior|alvior|pithior|corpior|gistior")) //Destroyer
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelum|centum|alvum|pithum|corpum|gistum|prophet")) //Cruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelatis|centatis|alvatis|pithatis|corpatis|gistatis|apostle")) //Battlecruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"core\s|centus|alvus|pith\s|corpus|gist\s")) //Battleship
    ?.ThenBy(entry => entry?.DistanceMax ?? int.MaxValue)
    ?.ToArray();
Parse.IOverviewEntry[] listOverviewSimpleWreck =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase("wreck") ?? true)
    .ToArray();
Parse.IOverviewEntry[] listOverviewCommanderWreck =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(commanderNameWreck) ?? true)

    .ToArray(); 
Parse.IOverviewEntry[] listOverviewCommanderAll =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase("cargo|wreck" ) ?? true)
    .ToArray();

Parse.IOverviewEntry[] ListCelestialObjects => WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(celestialOrbit) ?? false)
    ?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
    ?.ToArray();
Parse.IOverviewEntry[] ListCelestialToAvoid => WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(CelestialToAvoid ) ?? false)
    ?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
    ?.ToArray();
Parse.IOverviewEntry[] listOverviewDreadCheck => WindowOverview?.ListView?.Entry
    ?.Where(entry => (entry?.Name?.RegexMatchSuccess(runFromRats) ?? true))
    .ToArray();
Parse.IOverviewEntry[] listOverviewEntryFriends =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.ListBackgroundColor?.Any(IsFriendBackgroundColor) ?? false)
    ?.ToArray();
Parse.IOverviewEntry[] listOverviewEntryEnemy =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.ListBackgroundColor?.Any(IsEnemyBackgroundColor) ?? false)
    ?.ToArray();


Sanderling.Accumulation.IShipUiModule[] SetModuleTractorBeam =>
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.LabelText?.Any(
		label => label?.Text?.RegexMatchSuccess(TractorBeast, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false)?.ToArray();	

Sanderling.Accumulation.IShipUiModule[] SetModuleTractorInactive	 =>
	SetModuleTractorBeam?.Where(module => !(module?.RampActive ?? false))?.ToArray();
	Sanderling.Parse.IShipUiTarget[] SetTargetWreck =>
	Measurement?.Target?.Where(target =>
		target?.TextRow?.Any(textRow => textRow.RegexMatchSuccessIgnoreCase("wreck")) ?? false)?.ToArray();

Parse.IOverviewEntry[] ListWreckOverviewEntry =>
	WindowOverview?.ListView?.Entry
	?.Where(entry => entry.Name.RegexMatchSuccessIgnoreCase("wreck"))
	?.OrderBy(entry => entry.DistanceMax ?? int.MaxValue)
	?.ToArray();
Parse.IOverviewEntry[] EWarToAttack =>
    WindowOverview?.ListView?.Entry
?.Where(entry => entry != null && (!entry?.EWarType?.IsNullOrEmpty() ?? false) && (entry?.EWarType).Any())
?.OrderBy(iconright => iconright?.RightIcon?.FirstOrDefault(ewar => ewar?.HintText.Contains("webifying") ?? false))
?.OrderBy(iconright => iconright?.RightIcon?.FirstOrDefault(ewar => ewar?.HintText.Contains("scrambling|disrupting") ?? false))
?.OrderBy(iconright => iconright?.RightIcon?.FirstOrDefault(ewar => ewar?.HintText.Contains("neutralizing") ?? false))
	?.ToArray();

DroneViewEntryGroup DronesInBayListFolderEntry =>
    WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"heavy", RegexOptions.IgnoreCase));


DroneViewEntryGroup DronesInBayListEntry =>
    WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in bay", RegexOptions.IgnoreCase));

DroneViewEntryGroup DronesInSpaceListEntry =>
    WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in Local Space", RegexOptions.IgnoreCase));


int? DronesInSpaceCount => DronesInSpaceListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();

int? DronesInBayCount => DronesInBayListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();


public bool Tethering =>
    Measurement?.ShipUi?.EWarElement?.Any(EwarElement => (EwarElement?.EWarType).RegexMatchSuccess("tethering")) ?? false;
public bool Aligning =>
    Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("aligning")) ?? false;

public bool ReadyForManeuverNot =>
    Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("warp|docking")) ?? false;
public bool ImDocking =>
    Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("docking")) ?? false;
public bool EmptyIndication =>
    Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("")) ?? false;
public bool ShipIsSleeping => (EmptyIndication || !(Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000));
public bool ReadyForManeuver => !ReadyForManeuverNot  && !(Measurement?.IsDocked ?? true);
Sanderling.Interface.MemoryStruct.IListEntry WindowInventoryItem =>
    WindowInventory?.SelectedRightInventory?.ListView?.Entry?.FirstOrDefault();
WindowChatChannel chatLocal =>
     Sanderling.MemoryMeasurementParsed?.Value?.WindowChatChannel
     ?.FirstOrDefault(windowChat => windowChat?.Caption?.RegexMatchSuccessIgnoreCase("local") ?? false);
//    assuming that own character is always visible in local
public bool hostileOrNeutralsInLocal => 1 < chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy);
public bool tooManyOnLocal => MaxInLocal < localCount;
void CloseModalUIElement()
{

    var ConnectionLost = Measurement?.WindowOther?.FirstOrDefault()?.LabelText?.FirstOrDefault(text => (text?.Text.RegexMatchSuccessIgnoreCase("Connection") ?? false));
    
    var NotEnoughCargo = Sanderling?.MemoryMeasurementParsed?.Value?.WindowOther?.FirstOrDefault()?.LabelText?.FirstOrDefault(text => (text?.Text.RegexMatchSuccessIgnoreCase("Not enough cargo space") ?? false));
    var ButtonClose =
        ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("quit|close|no|ok"));
    var ButtonQuit =
        ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("quit|close|ok"));
        if (ConnectionLost != null)
        {
            Host.Log("               Lost connection (modal) at : " + DateTime.Now.ToString(" HH:mm")+ "" );

            Console.Beep(1047,150);
            Host.Delay(150);
            Console.Beep(784,150);
            Host.Delay(1111);
            Sanderling.KeyboardPressCombined(new[] { lockTargetKeyCode, VirtualKeyCode.LMENU });

        }
    if (NotEnoughCargo != null)
    { 
        var OkyButton = Sanderling?.MemoryMeasurementParsed?.Value?.WindowOther?.FirstOrDefault()?.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("ok"));
        if (OkyButton != null)
            Sanderling.MouseClickLeft(OkyButton);
                    Host.Delay(3500);
        FullCargoMessage = true;
        StopAfterburner();

        ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark,"dock");
        Host.Delay(3500);
        
    }
    else
        Sanderling.MouseClickLeft(ButtonClose);
    
}
void CloseWindowTelecom()
{
    var WindowTelecom = Measurement?.WindowTelecom?.FirstOrDefault(w => (w?.Caption.RegexMatchSuccessIgnoreCase("Information") ?? false));
    var CloseButton = WindowTelecom?.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("Close"));
    var HavenTelecom = Measurement?.WindowTelecom?.FirstOrDefault()?.LabelText?.FirstOrDefault(text => (text?.Text.RegexMatchSuccessIgnoreCase("Ship Computer") ?? false));
    if (CloseButton != null)
        Sanderling.MouseClickLeft(CloseButton);
}
public void CloseWindowOther()
{
         var ConnectionLost = Measurement?.WindowOther?.FirstOrDefault()?.LabelText?.FirstOrDefault(text => (text?.Text.RegexMatchSuccessIgnoreCase("Connection") ?? false));
     var ButtonQuit =
        ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("quit|close|ok"));
        if (ConnectionLost != null)
        {
            Host.Log("               Lost connection at : " + DateTime.Now.ToString(" HH:mm")+ "" );

            Console.Beep(1047,150);
            Host.Delay(150);
            Console.Beep(784,150);
            Host.Delay(1111);
            Sanderling.KeyboardPressCombined(new[] { lockTargetKeyCode, VirtualKeyCode.LMENU });

        }
    var windowOther = Sanderling?.MemoryMeasurementParsed?.Value?.WindowOther?.FirstOrDefault();
    if (!windowOther?.HeaderButtonsVisible ?? false)
        Sanderling.MouseMove(windowOther.LabelText.FirstOrDefault());

    Sanderling.InvalidateMeasurement();
    if (windowOther?.HeaderButton != null)
    {
        var closeButton = windowOther.HeaderButton?.FirstOrDefault(x => x.HintText == "Close");
        if (closeButton != null)
            Sanderling.MouseClickLeft(closeButton);
    }
}
void Expander(string Label)
{
	var Element = Measurement?.WindowDroneView?.FirstOrDefault()?.ListView?.Entry?.FirstOrDefault(w => w?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase("^" + Label) ?? false);
	bool IsExpanded = Element?.IsExpanded ?? true;
	if(!IsExpanded) ClickMenuEntryOnMenuRoot(Element,"Expand");
}
void DroneLaunch()
{

      Expander("Drones in Bay");
  //Sanderling.MouseClickRight(DronesInBayListFolderEntry);
  Sanderling.MouseClickRight(DronesInBayListEntry);
    Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("launch", RegexOptions.IgnoreCase));
    Host.Log("               Launching my Vipers");
    Host.Delay(1444);
}
void LaunchDronesByLabelName(string LabelName)
{
	Expander("Drones in Bay");
	var Label = Measurement?.WindowDroneView?.FirstOrDefault()?.LabelText?.FirstOrDefault(w => w.Text.RegexMatchSuccessIgnoreCase("^" + LabelName) );
	if(ReadyForManeuver)
	ClickMenuEntryOnMenuRoot(Label, "^Launch Drones");
}
void DroneEnsureInBay()
{
    if (null == WindowDrones || DronesInSpaceCount==0)
        return;
     else
    DroneReturnToBay();
    Host.Delay(4444);
}
void DroneReturnToBay()
{
    Host.Log("               I do not forget my Vipers here");
     Sanderling.KeyboardPressCombined(new[]{ targetLockedKeyCode, VirtualKeyCode.VK_R });//if you like 
}


void  Undock()
{
    while  (Measurement?.IsDocked ?? true)
    {
        Sanderling.MouseClickLeft(Measurement?.WindowStation?.FirstOrDefault()?.UndockButton);
        Host.Log("             Master Yoda> When you Undock , Feel the Force Luke,... Feel the Force!");
        Host.Delay(8826);
    }
    Host.Delay(3444);
    var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
    if (!(Measurement?.IsDocked ?? true))
    {
            while ( K>0)
            {
                Sanderling.WaitForMeasurement();
                K--;
            }
        if (Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000)
           Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
        EnsureWindowInventoryOpen();
        if (probeScannerWindow == null)
            Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });


        while (HulHpPercent < 100 ||ArmorHpPercent < 100  ||ShieldHpPercent < 100 || !Tethering  || AreDronesDamaged ())
        {
        Host.Log("               Luke > I try Master Yoda, ... I try!");
        Host.Delay(5823);
        }
        if (Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000)
           Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
           RetreatUpdate();
        ModuleMeasureAllTooltip();
if(ActivatePerma)
ActivatePermaExecute();
            if ((hostileOrNeutralsInLocal && RetreatOnNeutralOrHostileInLocal)|| tooManyOnLocal )
        {                        Random rnd = new Random();
        int DelayTime = rnd.Next(MinimDelayUndock, MaximDelayUndock);
            Host.Log("               undocked but there are hostiles, next check   in :  " + DelayTime+ " s ");
            Host.Delay( DelayTime*1000);
            ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
        }
        ReturnToOldSite ();
    }
}
int kk;
Func<object> DefenseStep()
{
    if (!ReadyForManeuver)
        return MainStep;
    if (Tethering)
        return MainStep;
    var NPCtargheted = Measurement?.Target?.Length;
    var shouldAttackTarget = ListRatOverviewEntry?.Any(entry => entry?.MeActiveTarget ?? false) ?? false ;
    var targetSelected = Measurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false);
    var Broken = ListCelestialObjects?.FirstOrDefault();
    var droneListView = Measurement?.WindowDroneView?.FirstOrDefault()?.ListView;
    var droneGroupWithNameMatchingPattern = new Func<string, DroneViewEntryGroup>(namePattern =>
        droneListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(group => group?.LabelTextLargest()?.Text?.RegexMatchSuccessIgnoreCase(namePattern) ?? false));
    var overviewEntryLockTarget =
        ListRatOverviewEntry?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false)));
    var droneGroupInLocalSpace = droneGroupWithNameMatchingPattern("local space");
    var setDroneInLocalSpace = droneListView?.Entry?.OfType<DroneViewEntryItem>()
        ?.Where(drone => droneGroupInLocalSpace?.RegionCenter()?.B < drone?.RegionCenter()?.B)
        ?.ToArray();
    var droneInLocalSpaceSetStatus =
        setDroneInLocalSpace?.Select(drone => drone?.LabelText?.Select(label => label?.Text?.StatusStringFromDroneEntryText()))?.ConcatNullable()?.WhereNotDefault()?.Distinct()?.ToArray();
    var droneInLocalSpaceIdle =
        droneInLocalSpaceSetStatus?.Any(droneStatus => droneStatus.RegexMatchSuccessIgnoreCase("idle")) ?? false;
    var droneGroupInBay = droneGroupWithNameMatchingPattern("bay");
    var targetassignet = targetSelected?.Assigned?.ToArray();;
        var setTargetRatNotAssigned =
        Measurement?.Target?.Where(target => !(0 < target?.Assigned?.Length))?.ToArray();
    var targetDistance = targetSelected?.DistanceMax;
    var targetFocus = targetSelected?.IsSelected ?? false;
//HpUpdate();
if(null != RetreatReason && !Tethering && !(Measurement?.IsDocked ?? false))
{  Host.Log("               good to activate");
    ActivateRepairerExecute();
}
 if (ArmorRepairsCount>0)
{     if (ActivateArmorRepairer == true || ArmorHpPercent < StartArmorRepairerHitPoints)
    {
        Host.Log("               Armor integrity < "  + StartArmorRepairerHitPoints + "%");
        ActivateRepairerExecute();
    }
    if (ArmorHpPercent > StartArmorRepairerHitPoints && ActivateArmorRepairer == false )
    { StopRepairer(); }}
       
    if (ShieldBoosterCount >0)
{ if (ActivateShieldBooster == true || ShieldHpPercent < StartShieldRepairerHitPoints)
    { 
    	
        Host.Log("               Shield integrity < "  + StartShieldRepairerHitPoints + "%");
        ActivateRepairerExecute();
    }
    if (ShieldHpPercent > StartShieldRepairerHitPoints && ActivateShieldBooster == false )
    { StopRepairer(); }
    }

        if (Measurement?.ShipUi?.Indication?.ManeuverType != ShipManeuverTypeEnum.Orbit)
    {
        Orbitkeyboard();
    }

        if (null == targetSelected)
        LockTarget();
    if (null != targetSelected)
    {
        if (shouldAttackTarget)
        {
        if (Measurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false).DistanceMax < WeaponMaxRange)
            ActivateWeaponExecute();
		if (Measurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false).DistanceMax < PainterMaxRange && CapacitorHpPercent > StopCapacitorValue)
            ActivatePainterExecute();
		if (droneInLocalSpaceIdle && (Measurement?.Target?.Length > 0))
			{
				Sanderling.KeyboardPress(attackDrones);
				Host.Log("               Vipers message: Sir! Yes Sir! We engage the target at the distance " +targetDistance+  " not assigned count  "  +setTargetRatNotAssigned?.Length+ "" );			
            }

        }
        else
            UnlockTarget();
    }
         if (droneInLocalSpaceIdle && ListRatOverviewEntry?.FirstOrDefault()?.DistanceMax > ShipRangeMax)
        OrbitRats();
                if (droneInLocalSpaceIdle&& targetFocus && targetDistance >MaxDronesRange && targetDistance < ShipRangeMax)
            for(int kk=0; kk < setTargetRatNotAssigned.Length; ++kk)
{
                Sanderling.MouseClickLeft(setTargetRatNotAssigned?.FirstOrDefault());
                if (setTargetRatNotAssigned?.LastOrDefault()?.DistanceMax>MaxDronesRange)
            			if (Measurement?.Target?.Length < TargetCountMax && 1 < ListRatOverviewEntry?.Count())
        			LockTarget();
            }
    else if (Measurement?.Target?.Length < TargetCountMax && 1 < ListRatOverviewEntry?.Count())
        LockTarget();

    if (EWarToAttack?.Count() > 0)
    {
        var EWarSelected = EWarToAttack?.FirstOrDefault(target => target?.IsSelected ?? false);
        var EWarLocked = EWarToAttack?.FirstOrDefault(target => target?.MeTargeted ?? false);
        if (EWarLocked == null)
        {
            Sanderling.KeyDown(lockTargetKeyCode);
            Sanderling.MouseClickLeft(EWarToAttack?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false))));
            Sanderling.KeyUp(lockTargetKeyCode);
        }
        else if (EWarSelected == null)
        { Sanderling.MouseClickLeft(EWarToAttack?.FirstOrDefault()); }
        else if ( null != EWarSelected)
        {
            Sanderling.KeyboardPress(attackDrones);
            Host.Log("               Some nasty rats, engaging them ");
        }
    }
        if (Measurement?.Target?.Length < TargetCountMax && 1 < ListRatOverviewEntry?.Length)
        LockTarget();
    if ( (DronesInSpaceCount + DronesInBayCount ) < DroneNumber)
	{
                    Sanderling.InvalidateMeasurement();
            Sanderling.WaitForMeasurement();
        if ( (DronesInSpaceCount + DronesInBayCount ) < DroneNumber)
	reasonDrones = true;
	}


    if (0 < DronesInBayCount && DronesInSpaceCount < DroneNumber)
        {    if ( (AllDrones?.Any(label => label?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase(@LabelNameSalvageDrones) ?? false)?? false))
           LaunchDronesByLabelName(LabelNameAttackDrones);
             if (!UseSalvageDrones || !(AllDrones?.Any(label => label?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase(@LabelNameSalvageDrones) ?? false)?? false))
             DroneLaunch();

       }
    if (DefenseExit)
    {
	StopAfterburner();
	DroneEnsureInBay();
    if (Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000)
    Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
      //  Console.Beep(523, 125);
    Console.Beep(659, 125);
    Console.Beep(415, 125);
    NoMoreRats = true;
    return MainStep;
    }
    return DefenseStep;
}

public bool ReadyToBattle => 0 < ListRatOverviewEntry?.Length && ReadyForManeuver;
public bool NoRatsOnGrid => 0 == ListRatOverviewEntry?.Length || ListRatOverviewEntry?.FirstOrDefault()?.DistanceMax > MaxDistanceToRats;
public bool LookingAtStars => NoRatsOnGrid && ReadyForManeuver;


Sanderling.Accumulation.IShipUiModule[] SetModuleTractorActive	 =>
	SetModuleTractorBeam?.Where(module => module?.RampActive ?? false)?.ToArray();
Func<object> InBeltMineStep()
{
if(OreHoldFilledForOffload || FullCargoMessage)
return null;
if (!ReadyForManeuver)
return MainStep;
if (Tethering)
{
return MainStep;
}
var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
EnsureWindowInventoryOpen();
EnsureWindowInventoryOpenActiveShip();
var droneListView = Measurement?.WindowDroneView?.FirstOrDefault()?.ListView;
var droneGroupWithNameMatchingPattern = new Func<string, DroneViewEntryGroup>(namePattern =>
droneListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(group => group?.LabelTextLargest()?.Text?.RegexMatchSuccessIgnoreCase(namePattern) ?? false));
var droneGroupInLocalSpace = droneGroupWithNameMatchingPattern("local space");
var setDroneInLocalSpace = droneListView?.Entry?.OfType<DroneViewEntryItem>()
?.Where(drone => droneGroupInLocalSpace?.RegionCenter()?.B < drone?.RegionCenter()?.B)
?.ToArray();
var droneInLocalSpaceSetStatus =
setDroneInLocalSpace?.Select(drone => drone?.LabelText?.Select(label => label?.Text?.StatusStringFromDroneEntryText()))?.ConcatNullable()?.WhereNotDefault()?.Distinct()?.ToArray();
var droneInLocalSpaceIdle =
droneInLocalSpaceSetStatus?.Any(droneStatus => droneStatus.RegexMatchSuccessIgnoreCase("idle")) ?? false;
           if (combatTab != OverviewTabActive && 0 < ListRatOverviewEntry.Length)
{
Sanderling.MouseClickLeft(combatTab);
Host.Delay(311);
}
if (RattingAnomaly && (0 < listOverviewEntryFriends?.Length || ListCelestialToAvoid?.Length > 0 )
&& ReadyToBattle)
{     if (probeScannerWindow == null)
Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
if (  ListCelestialToAvoid?.Length > 0)
{
Host.Log("               Gas Haven, better run!!");
ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "warp");
}

if (Measurement?.ShipUi?.Indication?.ManeuverType != ShipManeuverTypeEnum.Orbit)
{
Host.Log("               Presence of friends on site! Let them be!");
return TakeAnomaly;
}
}
if (ReadyToBattle && (Measurement?.ShipUi?.Indication?.ManeuverType != ShipManeuverTypeEnum.Orbit))
        {
 if (probeScannerWindow == null)
Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
if (!OldSiteExist)
SavingLocation ();
ActivatePermaExecute();
LootValue();
Orbitkeyboard();
            if (DefenseEnter)
            {
                return DefenseStep;
            }
        }

if ( DefenseEnter)
{
return DefenseStep;
}
//   EnsureWindowInventoryOpen();
        if (( OreHoldFilledForOffload || 0 == listOverviewSimpleWreck?.Length )
        && LookingAtStars && !Tethering)
        {
        if ((AnomalyToTake == "haven"|| AnomalyToTake == "Haven") && 0 == ListRatOverviewEntry?.Length && NoMoreRats == false && 0 < ListCelestialObjects?.Length)
            {
            Host.Log("               I'm in Heaven, waiting my rats :d :))");
            while( 0 == ListRatOverviewEntry?.Length)
                {
                Host.Delay(1111);
                return InBeltMineStep;
                }
            }
            else
                {
                Host.Log("               general nothing here");
                    DroneEnsureInBay();
                    FinishedSite();
                    return TakeAnomaly;
                }

        }

if (TakeLoot && 0 == ListRatOverviewEntry.Length)
{
    if (salvageTab != OverviewTabActive )
        {
        Sanderling.MouseClickLeft(salvageTab);
        Host.Delay(1111);
        }
    if (listOverviewSimpleWreck?.LastOrDefault()?.DistanceMax > 20000)
    ClickMenuEntryOnMenuRoot(listOverviewSimpleWreck?.LastOrDefault(), "approach");
    else  if (Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000)
    Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
    if ((!OreHoldFilledForOffload
    &&  0 < listOverviewCommanderAll.Length)
    && (LookingAtStars || ShipIsSleeping ))
    {
    StopAfterburner ();
if (!(AllDrones?.Any(label => label?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase(@LabelNameSalvageDrones) ?? false)?? false))
    {
                FinishedSite();
    Host.Log("                I don't have Salvage Drones:  Site finished! ");
    SiteFinished = true;	//this is just for show, unused

    return TakeAnomaly;

    }
    if (UseSalvageDrones && 0 < listOverviewSimpleWreck?.Length)
        while (0 < DronesInBayCount && DronesInSpaceCount < DroneNumber && 0 < listOverviewCommanderAll?.Length)
         {LaunchDronesByLabelName(LabelNameSalvageDrones);}

    var moduleTractorInactive = SetModuleTractorInactive?.FirstOrDefault();
    var moduleTractorActive = SetModuleTractorActive?.FirstOrDefault();
        if (droneInLocalSpaceIdle && 0 < listOverviewSimpleWreck?.Length)
        Sanderling.KeyboardPress(attackDrones);
        if (TakeOnlyCommanderLoot)
        { Host.Log("             taking only commander loot :))");
            if(0 < listOverviewCommanderWreck?.Length && listOverviewCommanderWreck?.FirstOrDefault()?.DistanceMax < 20000)
            {  if (Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000)
            Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
            Sanderling.KeyDown(lockTargetKeyCode);
            Sanderling.MouseClickLeft(listOverviewCommanderWreck?.FirstOrDefault());
            Sanderling.KeyUp(lockTargetKeyCode);
                if (SetModuleTractorInactive?.Length >0)
                ModuleToggle(moduleTractorInactive);
                if(TakeOnlyCommanderLoot &&listOverviewCommanderWreck?.FirstOrDefault()?.DistanceMax < 2000)
                {
                WreckLoot();
                LootingCargo();

                }
            }
            else if (listOverviewCommanderWreck?.FirstOrDefault()?.DistanceMax > 20000)
            {
            WreckLoot();
            LootingCargo();

            }
            else if (( OreHoldFilledForOffload || !UseSalvageDrones   || 0 == listOverviewSimpleWreck?.Length  )
                && LookingAtStars && !Tethering)
                {
                Host.Log("               finished loot from commander");
                    DroneEnsureInBay();
                    FinishedSite();
                    return TakeAnomaly;
                }
        }
        if (TakeALLLoot)
            {   Host.Log("             taking ALL loot  :))");
            if(listOverviewCommanderAll?.FirstOrDefault()?.DistanceMax < 20000)
                {
                if (Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000)
                Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
                Sanderling.KeyDown(lockTargetKeyCode);
                Sanderling.MouseClickLeft(listOverviewCommanderAll?.FirstOrDefault());
                Sanderling.KeyUp(lockTargetKeyCode);
                if (SetModuleTractorInactive?.Length >0)
                ModuleToggle(moduleTractorInactive);
                    if(TakeOnlyCommanderLoot &&listOverviewCommanderAll?.FirstOrDefault()?.DistanceMax < 2000)
                    {
                    WreckLoot();
                    LootingCargo();

                    }

                }
            else if (listOverviewCommanderAll?.FirstOrDefault()?.DistanceMax > 20000)
                {
                WreckLoot();
                LootingCargo();

                }
            else if (( OreHoldFilledForOffload || 0 == listOverviewCommanderAll?.Length )
                && LookingAtStars && !Tethering)
                    { Host.Log("               finished loot from take all");
                    DroneEnsureInBay();
                    FinishedSite();
                    return TakeAnomaly;
                    }
            }


    if (0 == listOverviewSimpleWreck?.Length)
    { Host.Log("               no more wrecks");
     DroneEnsureInBay();}
    }
}
else if (!TakeLoot && DronesInSpaceCount == 0)
    {
                FinishedSite();
    Host.Log("                I don't love the loot! Site finished! ");
    SiteFinished = true;	//this is just for show, unused

    return TakeAnomaly;

    }

return InBeltMineStep;
}
void FinishedSite()
{               deleteBookmark ();
            DroneEnsureInBay();
            SiteFinished = true;
            KaboonusTalk();
            LootValue();
            ++sitescount;
            Console.Beep(415, 125);
                    Console.Beep(523, 125);
    Console.Beep(659, 125);
            LogMessageToFile(" "+VersionScript+ "/" +CharName+ "/" +currentLocationName+ " # Sites : " + sitescount+ " Total: " +MagicalPrepare .ToString("N0")+ " ISK # Session: " +HocusPocusIskus+ " ISK " + (string.IsNullOrEmpty(StatusLootValue) ? "0" :  StatusLootValue)+ " LOOT ");
SiteFinished = true;
 Host.Log("                Im coolest! Site finished! ");
}

var reasonDrones = false;
int L=3;
Func<object> TakeAnomaly()
{
    Host.Log("               take Anomaly");
    ModuleMeasureAllTooltip();
    if (OreHoldCapacityMilli?.Used>0 || true == AreDronesDamaged())
	//if ( OreHoldFillPercent > 0)
    {Host.Log("               cargo used  "  +OreHoldCapacityMilli?.Used+ "m3" );
        Host.Log("               You won't start a new anomaly with the cargo at : " +OreHoldFillPercent+ " %  . Go to unload !");
        ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
    }
    if (OldSiteExist)
    {
       ReturnToOldSite ();
        return MainStep;
    }
    var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
    var scanActuallyAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(ActuallyAnomaly);
    var UndesiredAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(IgnoreAnomaly);
    var scanResultCombatSite = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(AnomalySuitableGeneral);
    Host.Log("               R2D2 instruments: working at ignoring anomalies :) be patient");
   if ( (DronesInSpaceCount + DronesInBayCount ) < DroneNumber)
	{
	reasonDrones = true;
	}
    if (probeScannerWindow == null)
        Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
    if (null != scanActuallyAnomaly)
    {
        ClickMenuEntryOnMenuRoot(scanActuallyAnomaly, "Ignore Result");
        return TakeAnomaly;
    }
    if (null != UndesiredAnomaly)
    {
        ClickMenuEntryOnMenuRoot(UndesiredAnomaly, "Ignore Result");
        return TakeAnomaly;
    }

    if ((null != scanResultCombatSite) && (null == UndesiredAnomaly))
    {
        Sanderling.MouseClickRight(scanResultCombatSite);
        var menuResult = Measurement?.Menu?.ToList();
        if (null == menuResult)
        { Host.Log("                R2D2 fails: not expected resultats in menu!  "); return TakeAnomaly; }
		else
		{
        var menuResultWarp = menuResult?[0].Entry.ToArray();
        var menuResultSelectWarpMenu = menuResultWarp?[1];
        Sanderling.MouseClickLeft(menuResultSelectWarpMenu);
        var menuResultats = Measurement?.Menu?.ToList();
		if (Measurement?.Menu?.ToList() ? [1].Entry.ToArray()[x].Text !=  WarpToAnomalyDistance)
			{
			    return TakeAnomaly;
			}
			else
			{
			var menuResultWarpDestination = Measurement?.Menu?.ToList() ? [1].Entry.ToArray();
			Host.Log("               The Force be with you, in to the next journey to : " +AnomalyToTake+ "  . ");
			ClickMenuEntryOnMenuRoot(menuResultWarpDestination[x], WarpToAnomalyDistance);
            NoMoreRats = false;
              Host.Log("no more rats true? :    " +NoMoreRats+    "");
			if (probeScannerWindow != null)
			Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
			}
		}
        return MainStep;
    }
    if (null == scanResultCombatSite && Tethering && Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000)
        Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
                   if (null == scanResultCombatSite && Tethering && Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli<2000)
            {
                Host.Log("               R2D2: No anomalies, waiting ");
                 return TakeAnomaly;
            }
    if (null == scanResultCombatSite && !Tethering)
        {
            while ( L>0)
            {
                if (null == scanResultCombatSite && !Tethering)
                {
                Host.Log("               Trust the Force, Luke  " +L+ "  . ");
                L--;
                return TakeAnomaly;
                }
            }
            if (null == scanResultCombatSite && !Tethering)
            {
                Host.Log("               R2D2: no more anomalies! If you dont like the asteroids then admire the Space from a tethering zone. ");
                ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "warp|approach");
            }
        }
    return MainStep;
}
void WreckLoot()
{
    if(OreHoldFilledForOffload || FullCargoMessage)
    return ;
   if (listOverviewCommanderAll?.FirstOrDefault()?.DistanceMax > 80)
 ClickMenuEntryOnMenuRoot(listOverviewCommanderAll?.FirstOrDefault(), "open cargo");
}
void LootingCargo ()
{
    var LootButton = Measurement?.WindowInventory?[0]?.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("Loot All"));
        Host.Log("               Teleporting some loot!");
        Sanderling.MouseClickLeft(LootButton);
    EnsureWindowInventoryOpenActiveShip();
}

void ClickMenuEntryOnMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern)
{
    Sanderling.MouseClickRight(MenuRoot);
    var Menu = Measurement?.Menu?.FirstOrDefault();
    var MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
    Sanderling.MouseClickLeft(MenuEntry);
}
void EnsureWindowInventoryOpen()
{
    if (null != WindowInventory)
        return;
    Sanderling.MouseClickLeft(Measurement?.Neocom?.InventoryButton);
    Host.Delay(1111);
}
void EnsureWindowInventoryOpenActiveShip()
{
    EnsureWindowInventoryOpen();
    var inventoryActiveShip = WindowInventory?.ActiveShipEntry;
    if (!(inventoryActiveShip?.IsSelected ?? false))
		Sanderling.MouseClickLeft(inventoryActiveShip);
}
//	sample label text: Intensive Reprocessing Array <color=#66FFFFFF>1,123 m</color //
string InventoryContainerLabelRegexPatternFromContainerName(string containerName) =>
    @"^\s*" + Regex.Escape(containerName) + @"\s*($|\<)";
void InInventoryUnloadItems() => InInventoryUnloadItemsTo(UnloadDestContainerName);
void InInventoryUnloadItemsTo(string DestinationContainerName)
{
    Host.Log("               Unload items to '" + DestinationContainerName + "'.");
       EnsureWindowInventoryOpenActiveShip();

    for (; ; )
    {
        var oreHoldListItem = WindowInventory?.SelectedRightInventory?.ListView?.Entry?.ToArray();
        var oreHoldItem = oreHoldListItem?.FirstOrDefault();
        if (null == oreHoldItem)
            break;    //    0 items in Cargo
        if (1 < oreHoldListItem?.Length)
            ClickMenuEntryOnMenuRoot(oreHoldItem, @"select\s*all");
        var DestinationContainerLabelRegexPattern =
            InventoryContainerLabelRegexPatternFromContainerName(DestinationContainerName);
        var DestinationContainer =
            WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
            ?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(DestinationContainerLabelRegexPattern) ?? false);
        if (null == DestinationContainer)
            Host.Log("               Houston, we have a problem: '" + DestinationContainerName + "' not found");
        Sanderling.MouseDragAndDrop(oreHoldItem, DestinationContainer);
            Host.Log("               Unloaded ALL items to '" + DestinationContainerName + "'.");
    }

}
void StackAll ()
{       Host.Log("               Stacking  All  ");
        EnsureWindowInventoryOpen();
        var DestinationContainerLabelRegexPattern =
            InventoryContainerLabelRegexPatternFromContainerName(UnloadDestContainerName);
        var DestinationContainer =
            WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
            ?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(DestinationContainerLabelRegexPattern) ?? false);

        Sanderling.MouseClickLeft(DestinationContainer);
        Host.Delay(1111);
        Sanderling.WaitForMeasurement();
        Host.Delay(1111);
        ClickMenuEntryOnMenuRoot(WindowInventory?.SelectedRightInventory?.ListView?.Entry?.FirstOrDefault(), @"stack all");
        Host.Delay(1111);
            Host.Log("               Stack All in '" + UnloadDestContainerName+ "' .");
}
void LockTarget()
{
    Sanderling.KeyDown(lockTargetKeyCode);
    Sanderling.MouseClickLeft(ListRatOverviewEntry?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false))));
    Sanderling.KeyUp(lockTargetKeyCode);
}
void UnlockTarget()
{
    var targetSelected = Measurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false);
    Sanderling.MouseClickRight(targetSelected);
    Sanderling.MouseClickLeft(MenuEntryUnLockTarget);
    Host.Log("               Don't worry R2D2, any robot can make mistakes, this is not a target");
}



void ModuleMeasureAllTooltip()
{
	//Host.Log("               Starbuck : I'm searching my 'fumerellos' ....");
   		var armorRapairCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsArmorRepairer ?? false);
		var afterburnersCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count((module => (module?.TooltipLast?.Value?.IsAfterburner ?? false) || (module?.TooltipLast?.Value?.IsMicroWarpDrive?? false)));
		var hardenersCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsHardener ?? false);
		var omniCount  = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(module => module?.TooltipLast?.Value?.LabelText?.Any(
					label => label?.Text?.RegexMatchSuccess(OmniSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
        var shieldBoosterCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsShieldBooster ?? false);

        var sensorBoostCount  = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(module => module?.TooltipLast?.Value?.LabelText?.Any(
            label => label?.Text?.RegexMatchSuccess(SensorSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
        var tractorBeastCount  = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(module => module?.TooltipLast?.Value?.LabelText?.Any(
            label => label?.Text?.RegexMatchSuccess(TractorBeast, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);

        Host.Log(" >>  Ratting Ship Recorded modules  : Armor Repair count = " + armorRapairCount + " ( " +ArmorRepairsCount+ " ) ; Afterburners count = " + afterburnersCount + " ( " +AfterburnersCount+" ) ;     Hardeners count = " + hardenersCount + " ( " +HardenersCount+ " );     \n" + "                          >>Omni count = " + omniCount + " ( " +OmniCount+ " ) "+
                        " ) ;  Shield Booster Count = " + shieldBoosterCount + " ( " +ShieldBoosterCount+ " ) ;  SensorBoost Count = " + sensorBoostCount + " ( " +SensorBoostCount+ " )  ;  Tractor Beam = " + tractorBeastCount + " ( " +TractorBeastCount+ " ) ");
    while((armorRapairCount < ArmorRepairsCount) || (afterburnersCount <  AfterburnersCount) || (tractorBeastCount <  TractorBeastCount)
			|| (hardenersCount <  HardenersCount) || (omniCount <  OmniCount)	)
	{
		if (ImDocking || (Measurement?.IsDocked ?? false))
			break;
		//foreach(var NextModule in Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule)
        for (int i = 0; i < Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Count(); ++i)
		{
            var NextModule = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.ElementAtOrDefault(i);
   			if(!ReadyForManeuver)
				break;
			if(null == NextModule)
				break;
			Host.Log("               R2D2 : recording your modules");
			//	take multiple measurements of module tooltip to reduce risk to keep bad read tooltip.
			Sanderling.MouseMove(NextModule);
            Host.Delay(305);
			Sanderling.WaitForMeasurement();
            Host.Delay(305);
			Sanderling.MouseMove(NextModule);
		}

        tractorBeastCount  = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(module => module?.TooltipLast?.Value?.LabelText?.Any(
            label => label?.Text?.RegexMatchSuccess(TractorBeast, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);		
    	omniCount  = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(module => module?.TooltipLast?.Value?.LabelText?.Any(
					label => label?.Text?.RegexMatchSuccess(OmniSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
        shieldBoosterCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsShieldBooster ?? false);
        sensorBoostCount  = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(module => module?.TooltipLast?.Value?.LabelText?.Any(
                   label => label?.Text?.RegexMatchSuccess(SensorSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
  	
        hardenersCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsHardener ?? false);
		armorRapairCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsArmorRepairer ?? false);
		afterburnersCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count((module => (module?.TooltipLast?.Value?.IsAfterburner ?? false) || (module?.TooltipLast?.Value?.IsMicroWarpDrive?? false)));
		
        Host.Log(  " Armor Repair count = " + armorRapairCount + "; Afterburners count = " + afterburnersCount + " ;     Hardeners count = " + hardenersCount + " ;       \n" + "                          >> Omni count = " + omniCount + " "+
                    " ; Shield Booster Count =  " + shieldBoosterCount + " ;  SensorBoost Count = " + sensorBoostCount + " ;  Tractor Beam = " + tractorBeastCount + " ." );
	}
}



void ActivateRepairerExecute()
{
var SubsetModuleArmorRepairer =
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
    ?.Where(module => (module?.TooltipLast?.Value?.IsArmorRepairer ?? false) || (module?.TooltipLast?.Value?.IsShieldBooster ?? false) || (module?.TooltipLast?.Value?.LabelText?.Any(
            label => label?.Text?.RegexMatchSuccess(UnsupportedArmorRepairer, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false)) ;
    var SubsetModuleToToggle =
        SubsetModuleArmorRepairer
        ?.Where(module => !(module?.RampActive ?? false));
    
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}

void StopRepairer()
{
var SubsetModuleArmorRepairer =
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
    ?.Where(module => (module?.TooltipLast?.Value?.IsArmorRepairer ?? false) || (module?.TooltipLast?.Value?.IsShieldBooster ?? false) || (module?.TooltipLast?.Value?.LabelText?.Any( 
            label => label?.Text?.RegexMatchSuccess(UnsupportedArmorRepairer, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false)) ;
    var SubsetModuleToToggle =
        SubsetModuleArmorRepairer
        ?.Where(module => (module?.RampActive ?? false));
    
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivateWeaponExecute()
{
    var SubsetModuleWeapon =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => (module?.TooltipLast?.Value?.IsWeapon ?? false) || (module?.TooltipLast?.Value?.IsWeapon?? false));
    var SubsetModuleToToggle =
        SubsetModuleWeapon
        ?.Where(module => !(module?.RampActive ?? false));
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivatePainterExecute()
{
    var SubsetModulePainter=
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => (module?.TooltipLast?.Value?.IsTargetPainter ?? false) || (module?.TooltipLast?.Value?.IsTargetPainter?? false));
    var SubsetModuleToToggle =
        SubsetModulePainter
        ?.Where(module => !(module?.RampActive ?? false));
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void StopPainterExecute()
{
    var SubsetModulePainter =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => (module?.TooltipLast?.Value?.IsTargetPainter ?? false) || (module?.TooltipLast?.Value?.IsTargetPainter?? false));
    var SubsetModuleToToggle =
        SubsetModulePainter
        ?.Where(module => (module?.RampActive ?? false));
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivateAfterburnerExecute()
{
var SubsetModuleAfterburner =
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
    ?.Where(module => (module?.TooltipLast?.Value?.IsAfterburner ?? false) || (module?.TooltipLast?.Value?.IsMicroWarpDrive?? false));
    var SubsetModuleToToggle =
        SubsetModuleAfterburner
        ?.Where(module => !(module?.RampActive ?? false));

    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void StopAfterburner()
{
var SubsetModuleAfterburner =
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
    ?.Where(module => (module?.TooltipLast?.Value?.IsAfterburner ?? false) || (module?.TooltipLast?.Value?.IsMicroWarpDrive?? false));
var SubsetModuleToToggle =
        SubsetModuleAfterburner
        ?.Where(module => (module?.RampActive ?? false));

    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
    {
		ModuleToggle(Module);
	}
}

string permanentactive ;

void ActivateHardenerExecute()
{
    var SubsetModuleHardener =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => module?.TooltipLast?.Value?.IsHardener ?? false);
     var SubsetModuleToToggle =
        SubsetModuleHardener
        ?.Where(module => !(module?.RampActive ?? false));
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivatePermaExecute()
{

foreach (string permanentactive in PermanentActive)
{
    var SubsetModulePerma =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => (module?.TooltipLast?.Value?.LabelText?.Any(
		label => label?.Text?.RegexMatchSuccess(permanentactive,System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false )?? false )|| ( module?.TooltipLast?.Value?.IsHardener ?? false));
        //Host.Log("              SubsetModulePerma  " +SubsetModulePerma?.Count()+ "");
        if  (SubsetModulePerma?.Count() == 0)
        break;
    var SubsetModuleToToggle =
        SubsetModulePerma
        ?.Where(module => !(module?.RampActive ?? false));
    Host.Log("               perma   " +permanentactive+ "");
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
        Host.Delay(311);
        }

}

void ModuleToggle(Sanderling.Accumulation.IShipUiModule Module)
{
    var ToggleKey = Module?.TooltipLast?.Value?.ToggleKey;
    Host.Log("               Toggle module  '" +Module?.TooltipLast?.Value?.LabelText?.ElementAtOrDefault(1)?.Text?.RemoveXmlTag() +      "'  using " + (null == ToggleKey ? "mouse" : Module?.TooltipLast?.Value?.ToggleKeyTextLabel?.Text));
    if (null == ToggleKey)
        Sanderling.MouseClickLeft(Module);
    else
        Sanderling.KeyboardPressCombined(ToggleKey);
}
void MemoryUpdate()
{
 	RetreatUpdate();
	HpUpdate();
	Timers ();
    CheckAreDronesDamaged();
}
void CheckAreDronesDamaged()
{
  if (AreDronesDamaged() )
            {      Console.Beep(523,1200);
            Host.Delay(200);
              Console.Beep(1500, 200);
                     Host.Log("              some drones are damaged");}

}
var logoutme= false;
var logoutgame = (eveRealServerDT-DateTime.UtcNow ).TotalMinutes;
void Timers ()
{
    var CapsuleType = WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
    ?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase("Capsule") ?? false);
if (null !=CapsuleType )
    {
    reasonCapsule = true;
    Host.Log("                Reason capsule "+reasonCapsule + " ");
    }
var now = DateTime.UtcNow;
var CloseGameSession = (playSession - now).TotalMinutes;
var CloseGameDT = (eveSafeDT - now).TotalMinutes;
var LogoutGame = Math.Min(CloseGameDT,CloseGameSession);
if (playSession !=DateTime.UtcNow)
{
logoutgame = LogoutGame;
}
	if (LogoutGame < 0)
	{
	logoutme = true;
		Host.Log("               Logoutgame, time elapsed is" + logoutme + " ");
	}
}
bool MeasurementEmergencyWarpOutEnter =>
    !(Measurement?.IsDocked ?? false) && (!(EmergencyWarpOutHitpointPercent < ArmorHpPercent));
bool ReasonBumped =>
(0 < ListRatOverviewEntry?.Length) && (listOverviewEntryFriends?.Length > 0) && (listOverviewEntryFriends?.FirstOrDefault()?.DistanceMax < 50);
bool ReasonCapsuled =>
(reasonCapsule);
bool ReasonTimeElapsed =>
(logoutme);
bool ReasonDrones =>
(reasonDrones);
bool ReasonCargoFull =>
(OreHoldFilledForOffload || FullCargoMessage);
bool ReasonEnd=>
(SiteFinished);
bool ReasonDread=>
(listOverviewDreadCheck?.Length > 0);
void RetreatUpdate()
{
if ((RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal)
	|| (listOverviewEntryEnemy?.Length > 0) || tooManyOnLocal)
    {
    Sanderling.InvalidateMeasurement();
    if ((RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal)
	|| (listOverviewEntryEnemy?.Length > 0)|| tooManyOnLocal)
    RetreatReasonTemporary = " Hostiles or too many in local ! ";
    }
    else RetreatReasonTemporary = null;

    RetreatReasonPermanent = !(Measurement?.IsDocked ?? false) && (!(EmergencyWarpOutHitpointPercent < ArmorHpPercent)) ? " They messed my Armor hp!!" : null;
        RetreatReasonDrones = ReasonDrones ? " I lost my head ( Drones)!!" : null;
    RetreatReasonCargoFull = ReasonCargoFull ? " Cargo Full !!" : null;
    RetreatReasonBumped = (0 <= ListRatOverviewEntry?.Length) && (listOverviewEntryFriends?.Length > 0) && (listOverviewEntryFriends?.FirstOrDefault()?.DistanceMax < 50) ?" Retreat: I was bumped !!" : null;
        RetreatReasonCapsuled = reasonCapsule ? " Retreat: Capsuled, go home" : null;
    RetreatReasonTimeElapsed = logoutme ? " Retreat: Your session elapsed, take a break!" : null;
            if (ReasonDread)
    {
    Sanderling.InvalidateMeasurement();
    if (ReasonDread)
    RetreatReasonDread = " Retreat!! Dread on Grid!!";
    }
    else RetreatReasonDread = null;
}
void Orbit(string whatToOrbit, string distance = "30 km")
{
    var ToOrbit = Measurement?.WindowOverview?.FirstOrDefault()?.ListView?.Entry?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(whatToOrbit) ?? false)?.ToArray();
    ClickMenuEntryOnPatternMenuRoot(ToOrbit.FirstOrDefault(), "Orbit", distance);
}
void ClickMenuEntryOnPatternMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern, string SubMenuEntryRegexPattern = null)
{
    Sanderling.MouseClickRight(MenuRoot);
    var Menu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.FirstOrDefault();
    var MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
    Sanderling.MouseClickLeft(MenuEntry);
    if (SubMenuEntryRegexPattern != null)
    {
		var subMenu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.ElementAtOrDefault(1);
        var subMenuEntry = subMenu?.EntryFirstMatchingRegexPattern(SubMenuEntryRegexPattern, RegexOptions.IgnoreCase);
        Sanderling.MouseClickLeft(subMenuEntry);
    }
}
void Orbitkeyboard()
{
if (0 == ListCelestialObjects?.Length)
OrbitRats();
if (1 == ListCelestialObjects?.Length)
Sanderling.MouseClickLeft(ListCelestialObjects?.FirstOrDefault());
else
{
    Sanderling.KeyDown(orbitKeyCode);
Sanderling.MouseClickLeft(ListCelestialObjects?.FirstOrDefault(celestial => celestial?.DistanceMax > DistanceCelestial));
    //    Sanderling.MouseClickLeft(ListCelestialObjects?.Skip(1)?.FirstOrDefault());
    Sanderling.KeyUp(orbitKeyCode);
    ActivateAfterburnerExecute();
    Host.Delay(1111);
    Host.Log("               Rats Are in my range, better to Orbit arround Celestials");}
}
void OrbitRats()
{
    Sanderling.KeyDown(orbitKeyCode);
         Sanderling.MouseClickLeft(ListRatOverviewEntry?.FirstOrDefault(entry => (entry?.MainIconIsRed ?? false)));
    Sanderling.KeyUp(orbitKeyCode);
    ActivateAfterburnerExecute();
    Host.Delay(1111);
    Host.Log("               Rats are too far ... selected to Orbit them");
}

void HpUpdate()
{
if(null != RetreatReason && !Tethering && !(Measurement?.IsDocked ?? false))
{  Host.Log("               good to activate");
    ActivateRepairerExecute();
}
if (CapacitorHpPercent < StopCapacitorValue)
StopPainterExecute();
 if (ArmorRepairsCount>0)
{     if (ActivateArmorRepairer == true || ArmorHpPercent < StartArmorRepairerHitPoints)
    {
        Host.Log("               Armor integrity < "  + StartArmorRepairerHitPoints + "%");
        ActivateRepairerExecute();
    }
    if (ArmorHpPercent > StartArmorRepairerHitPoints && ActivateArmorRepairer == false  && null == RetreatReason)
    { StopRepairer(); }}
    if (ShieldBoosterCount >0)
{ if (ActivateShieldBooster == true || ShieldHpPercent < StartShieldRepairerHitPoints)
    {
        Host.Log("               Shield integrity < "  + StartShieldRepairerHitPoints + "%");
        ActivateRepairerExecute();
    }
    if (ShieldHpPercent > StartShieldRepairerHitPoints && ActivateShieldBooster == false && null == RetreatReason)
    { StopRepairer(); }
    }

}

void ReviewSettings()
{
Host.Log("                >>> Settings Review bot " + VersionScript + "");
Host.Log("                - Start (UTC) :  " + dateAndTime.ToString(" dd/MM/yyyy HH:mm:ss")+ " (-1 min); ");
Host.Log("               ⊙ Kaboonus Gift From Yesterday :  " +HocusPocusPreparatus.ToString("N0")+ "");
Host.Log("                - retreat on neutrals :  " + RetreatOnNeutralOrHostileInLocal + " ; ");
        Host.Log("                - ratting anomaly :  " + RattingAnomaly + " ; ");
            Host.Log("                - ratting anomaly :  " + RattingAnomaly + " ; ");
            Host.Log("                - Use Salvage Drones:  " + UseSalvageDrones + " ; ");
            Host.Log("                - Loot to take  :  General " + TakeLoot + " ;  Commanders  :  "  + TakeOnlyCommanderLoot + " ;  All  :  " + TakeALLLoot + " ; ");
            Host.Log("                - anomaly name to take:  " + AnomalyToTake + " ; ");
            Host.Log("                - Combat Drones:  " +    LabelNameAttackDrones + " ; ");
                Host.Log("                - next DT (Eve Time) :  " + eveRealServerDT.ToString(" dd/MM/yyyy HH:mm:ss")+ " (-1 min); ");
                    Host.Log("                - Safe DT (Eve Time) :  " + eveSafeDT.ToString(" dd/MM/yyyy HH:mm:ss")+ " (-1 min); ");
 		        	Host.Log("                - Play sesion  end (ET) :  "  +playSession.ToString(" dd/MM/yyyy HH:mm:ss")+ "" );
                    Host.Log("                - Closer logout :  " +(TimeSpan.FromMinutes(logoutgame)).ToString(@"dd") + " days and " + ((TimeSpan.FromMinutes(logoutgame) < TimeSpan.Zero) ? "-" : "") + (TimeSpan.FromMinutes(logoutgame)).ToString(@"hh\:mm\:ss")+ " ; When times up: You Dock, logout and stopbot  ; ");
                        Host.Log("                - bookmark home: " + RetreatBookmark + " ; ");
                                Host.Log("                - delay undock min(max) :  " + MinimDelayUndock + "(" + MaximDelayUndock + " ); ");
                                Host.Log("                ⊙ Kaboonus Gift in Isk :  " +HocusPocus+ "");
                                Host.Log("                ⊙ Kaboonus Gift in Loot :  " +StatusLoot+ "");
                                LogMessageToFile(" "+VersionScript+ "/" +CharName+ "/" +currentLocationName+ " # Sites : " + sitescount+ " Total: " +MagicalPrepare .ToString("N0")+ " ISK # Session: " +HocusPocusIskus+ " ISK " + (string.IsNullOrEmpty(StatusLootValue) ? "0" :  StatusLootValue)+ " LOOT ");
                                                                Host.Log("                >>> End of Review.");
}
void CheckLocation()
{
    var listSurroundingsButton = Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton;
Sanderling.MouseClickRight(listSurroundingsButton);
    var	availableMenuEntriesTexts =
        Measurement?.Menu?.FirstOrDefault()?.Entry?.Select(menuEntry => menuEntry.Text)
        ?.ToList();
Sanderling.WaitForMeasurement();


var OldSiteMenuEntry = availableMenuEntriesTexts?.Where(x => x?.Contains(messageText) ?? false)?.FirstOrDefault();
    if (null != OldSiteMenuEntry)
        {
        OldSiteExist = true;  Host.Log("                #  Old Site value  : "+OldSiteExist+ " . ");
        }
    if (null == OldSiteMenuEntry )
        {
        OldSiteExist = false;  Host.Log("                #  Old Site value  : "+OldSiteExist+ " . ");
        }
}
void SavingLocation ()
{
    if (listOverviewDreadCheck.Length > 0)
    {
    var SaveLocationWindow = Measurement?.WindowOther?.FirstOrDefault(w =>
        (w?.Caption.RegexMatchSuccessIgnoreCase("New Location") ?? false));
    Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_B});
        Host.Delay(1111);
    Sanderling.TextEntry(messageTextDread);
        Host.Delay(1111);
    Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
    }
CheckLocation();
if (OldSiteExist)
{
Host.Log("                # Already have  Old Site Bookmark : "+OldSiteExist+ " . ");
}
else
{
    var SaveLocationWindow = Measurement?.WindowOther?.FirstOrDefault(w =>
                            (w?.Caption.RegexMatchSuccessIgnoreCase("New Location") ?? false));
    Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_B});
        Host.Delay(1111);
    Sanderling.TextEntry(messageText);
        Host.Delay(1111);
    Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
            Host.Delay(1111);
    OldSiteExist = true;
        Host.Log("                >> Old Site Bookmark saved : " +OldSiteExist+ " . ");
}
}
void deleteBookmark()
{
    CheckLocation();
    if (OldSiteExist)
    {
            OldSiteExist = false;
        ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, messageText, "Remove Location");
            Host.Delay(1111);
        Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
            Host.Delay(1111);
            Host.Log("                # Old Site Bookmark removed : " +!OldSiteExist+ " . ");
    }
}
void ReturnToOldSite ()
{
    CheckLocation();
if (!OldSiteExist)
    {   OldSiteExist = false;  Host.Log("                #  Old Site value  : "+OldSiteExist+ " . ");  }
    else
   {
   OldSiteExist = true;
      Host.Log("                # Warping to Old Site : "+OldSiteExist+ " . ");
    ModuleMeasureAllTooltip();
    ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, messageText, "warp");
   }
}

string KaboonusMidget => 	Measurement?.Tooltip?.FirstOrDefault()?.LabelText?.FirstOrDefault(entry =>entry.Text.RegexMatchSuccessIgnoreCase("\\d",RegexOptions.IgnoreCase))?.Text;
string Preparatus;
int result;
int inventoryValue;

  long MagicalPrepare;
  string HocusPocus;
  string HocusPocusIskus;
  string StatusLoot;
  string StatusLootValue;
void KaboonusTalk()
{
var KaboonusWithTexturePathMatch = new Func<string, MemoryStruct.IUIElement>(texturePathRegexPattern =>
				Measurement?.Neocom?.Button?.FirstOrDefault(candidate => candidate?.TexturePath?.RegexMatchSuccess(texturePathRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false));
var KaboonusButton = KaboonusWithTexturePathMatch("wallet");

Sanderling.MouseMove(KaboonusButton); Sanderling.WaitForMeasurement();
if  (string.IsNullOrEmpty(KaboonusMidget))
  { Sanderling.MouseMove(KaboonusButton); Sanderling.WaitForMeasurement(); }
  Preparatus = Regex.Replace(KaboonusMidget, "[^0-9]+", "");
     MagicalPrepare = Convert.ToInt64(Preparatus);
 var Paracelsus = MagicalPrepare - HocusPocusPreparatus;

 HocusPocusIskus =Paracelsus.ToString("N0");
 HocusPocus = Regex.Replace(HocusPocusIskus, ",", " ");
 Sanderling.MouseMove(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton);
}
void LootValue()
{
EnsureWindowInventoryOpenActiveShip();
var ListLabeltext = 	Measurement?.WindowInventory?.FirstOrDefault()?.LabelText?.ToList();
var LootValeur= 	Measurement?.WindowInventory?.FirstOrDefault()?.LabelText?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase("ISK <color=gray>Est. price") ?? true);
int positionindex = ListLabeltext.IndexOf(LootValeur, 0);
    string KaboonusCounting = 	ListLabeltext[positionindex]?.Text;
        KaboonusCounting = Regex.Replace(KaboonusCounting, "[^0-9]+", "");
              inventoryValue = Convert.ToInt32(KaboonusCounting);
int TotalValueParSession = initial + inventoryValue;
initial = TotalValueParSession;
 StatusLoot =initial.ToString("N0");
 StatusLoot = Regex.Replace(StatusLoot, ",", " ");
 Host.Log("                # Loot value : "+StatusLoot+ " . ");

}
        int droneShield ;
        int droneArmor;
        int droneHull;
bool AreDronesDamaged()
{

if (null == WindowDrones || (Measurement?.IsDocked ?? false) )
return false;

    foreach(DroneViewEntryItem dronebool in AllDrones)
{

    if(dronebool?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase(@LabelNameAttackDrones) ?? false)
    {

            string LabelNameAttackDronesToWatch = dronebool?.LabelText?.FirstOrDefault()?.Text;

               if(LabelNameAttackDronesToWatch != null &&   LabelNameAttackDronesToWatch.Contains("</color>") )
        {
            int? droneShield = dronebool?.Hitpoints?.Shield;
            int? droneArmor= dronebool?.Hitpoints?.Armor;
            int? droneHull = dronebool?.Hitpoints?.Struct;
            if ( null != droneShield && null != droneArmor && null != droneHull && (Measurement?.IsDocked ?? false))
            {
                if (droneShield < 1000 || droneArmor < 1000 || droneHull < 1000)
                {
                     Host.Log("               Space Status Drones : Shield " +droneShield+ " ; Armor " +droneArmor+ " ; Hull " +droneHull+ "");
                return true;
                }
                else
               {// Host.Log("               Not In tethering but Drones in space seems OK ");
               return false;   }
            }
        }
        if ( Tethering)
       {
         if(LabelNameAttackDronesToWatch != null &&   !LabelNameAttackDronesToWatch.Contains("</color>") && (Measurement?.IsDocked ?? false))   //Only check when tethering 
        {
            int? droneShield = dronebool?.Hitpoints?.Shield;
            int? droneArmor= dronebool?.Hitpoints?.Armor;
            int? droneHull = dronebool?.Hitpoints?.Struct;
            if ( null != droneShield && null != droneArmor && null != droneHull)
            {
                if (droneShield < 1000 || droneArmor < 1000 || droneHull < 1000)
                {Host.Log("               Status Drones : Shield " +droneShield+ " ; Armor " +droneArmor+ " ; Hull " +droneHull+ "");
            // Console.Beep(523,1200);
                return true;
                }
                else 
               { Host.Log("               Tethering, Drones seems OK ");
               return false;   }
            }
        }
        }
    }
}

 return false;
}
public string GetTempPath()
{
 //string path = ".\\";
string path = "D:\\ISKLOG";
    if (!path.EndsWith("\\")) path += "\\";
    return path;
}

public void LogMessageToFile(string msg)
{
    System.IO.StreamWriter sw = System.IO.File.AppendText(
        GetTempPath() +CharName+".txt");
    try
    {
        string logLine = System.String.Format(
            "{0:G}: {1}.", System.DateTime.Now, msg);
        sw.WriteLine(logLine);
    }
    finally
    {
        sw.Close();
    }
}


bool AnomalySuitableGeneral(MemoryStruct.IListEntry scanResult) =>
    scanResult?.CellValueFromColumnHeader(AnomalyToTakeColumnHeader)?.RegexMatchSuccessIgnoreCase(AnomalyToTake) ?? false;
bool ActuallyAnomaly(MemoryStruct.IListEntry scanResult) =>
       scanResult?.CellValueFromColumnHeader("Distance")?.RegexMatchSuccessIgnoreCase("km") ?? false;
bool IgnoreAnomaly(MemoryStruct.IListEntry scanResult) =>
scanResult?.CellValueFromColumnHeader(IgnoreColumnheader)?.RegexMatchSuccessIgnoreCase(IgnoreAnomalyName) ?? false;
bool IsEnemyBackgroundColor(ColorORGB color) =>
    color.OMilli == 500 && color.RMilli == 750 && color.GMilli == 0 && color.BMilli == 0;
bool IsFriendBackgroundColor(ColorORGB color) =>
    (color.OMilli == 500 && color.RMilli == 0 && color.GMilli == 150 && color.BMilli == 600) || (color.OMilli == 500 && color.RMilli == 100 && color.GMilli == 600 && color.BMilli == 100);
bool IsNeutralOrEnemy(IChatParticipantEntry participantEntry) =>
   !(participantEntry?.FlagIcon?.Any(flagIcon =>
     new[] { "good standing", "excellent standing", "Pilot is in your (fleet|corporation|alliance)", "Pilot is an ally in one or more of your wars", }
     .Any(goodStandingText =>
        flagIcon?.HintText?.RegexMatchSuccessIgnoreCase(goodStandingText) ?? false)) ?? false);
