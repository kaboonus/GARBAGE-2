/*GARBAGE-2 : Gift Anomaly Ratting  salvAGE Bot with Sanderling. This bot will take only anomalies, take the loot and use salvage drones for salvaging
The child of Sanderling--ratting-bot-anomaly-or-asteroids !!
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
*/
using BotSharp.ToScript.Extension;
using Parse = Sanderling.Parse;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
//	begin of configuration section ->
string VersionScript = "GARBAGE-2v0";

var RetreatOnNeutralOrHostileInLocal =true;   // true or false :warp to RetreatBookmark when a neutral or hostile is visible in local.
var RattingAnomaly = true;	// true or false:	when this is set to true, you take anomaly
string WarpToAnomalyDistance = "Within 50 km"; // variants(just copy paste) : "Within 10 km" "Within 20 km" "Within 30 km" "Within 50 km" "Within 70 km" "Within 100 km"   "Within 0 m"
var UseSalvageDrones = true; //if this is true will launch all drones one by one
var TakeLoot = true;
string LabelNameSalvageDrones = "Salvage Drone I"; //no reason to change
string LabelNameAttackDrones = "Praetor II"; //ex:  Imperial Navy Praetor ; dunno if it work with partial name like "praetor"  or "wasp"

string salvagingTab = "colly";
string rattingTab = "combat";

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
var LimitOffloadCount = 100;// when you reach this limit, you dock
// protection distance, from this distance you forget about this rats 
/// Ranges
var shipRangerMax = 63; // in km, you orbit arround them
var maxDistanceToRats = 120;// in km, you forget about them
/////settings anomaly
string AnomalyToTakeColumnHeader = "name";  // the column header from table ex : name
string AnomalyToTake = "Forsaken Hub"; // his name , ex:  "forsaken hub" or " combat"
string IgnoreAnomalyName = "Haven|Belt|asteroid|drone|forlorn|rally|sanctum|blood hub|serpentis hub|hidden|port|den";// what anomaly to ignore : haven|Belt|asteroid|drone|forlorn|rally|sanctum|blood hub|serpentis hub|hidden|port|den
string IgnoreColumnheader = "Name";//the head  of anomaly to ignore
// you have to run from this rats:
string runFromRats = "♦|Titan|Dreadnought|Autothysian";// you run from him
//celestial to orbit
string celestialOrbit = "broken|pirate gate"; //ex: broken|pirate gate
string CelestialToAvoid = "Chemical Factory"; // ex: Chemical Factory //this one make difference between haven rock and gas
// wrecks commander etc
string commanderNameWreck = "wreck";
string CargoWreck = "cargo|wreck";
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
var EnterOffloadOreHoldFillPercent = 97;//	percentage of ore hold fill level at which to enter the offload process and warp home.
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
int x=0;
if (WarpToAnomalyDistance == "Within 0 m")
x=0;
if (WarpToAnomalyDistance == "Within 10 km")
x=1;
if (WarpToAnomalyDistance == "Within 20 km")
x=2;
if (WarpToAnomalyDistance == "Within 30 km")
x=3;
if (WarpToAnomalyDistance == "Within 50 km")
x=4;
if (WarpToAnomalyDistance == "Within 70 km")
x=5;
if (WarpToAnomalyDistance == "Within 100 km")
x=6;
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
    Host.Log("                ⊙ Kaboonus Gift From Yesterday :  " +HocusPocusPreparatus.ToString("N0")+ "");





Func<object> BotStopActivity = () => null;
Func<object> NextActivity = MainStep;
for(;;)
{
MemoryUpdate();
Host.Log(
		" >  Started at: " +  startSession.ToString(" HH:mm") +// alternative (" dd/MM/yyyy HH:mm") 
		" ;   Logout in:  "  + ((TimeSpan.FromMinutes(logoutgame) < TimeSpan.Zero) ? "-" : "") + (TimeSpan.FromMinutes(logoutgame)).ToString(@"hh\:mm\:ss")+
		" ;   ISK:  " + HocusPocus+ "" + 
		" ;   Loot:  " + StatusLoot+ "" + 
		" ;   HP: S: " + ShieldHpPercent + "% ; A: " + ArmorHpPercent + "%" +
		" ;   Hostiles: " +(chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy)-1)+ " # Msg : "  + RetreatReason + 
		"\n" +
		"                          >>  Drones (space): "  +(DronesInSpaceCount + DronesInBayCount)+ "( "+ DronesInSpaceCount +  " )"+
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
StopArmorRepairer();
}
if (Measurement?.IsDocked ?? false)
    MainStep();
if(0 < RetreatReason?.Length && !(Measurement?.IsDocked ?? false))
{
    
        	Host.Log("               Tactical retreat,  reason  : " + RetreatReason + ".");
    Console.Beep(369, 125);// this beeps are  better
	StopAfterburner();
	ActivateArmorRepairerExecute();
    ActivateShieldBoosterExecute();
	 if (Measurement?.ShipUi?.Indication?.ManeuverType == ShipManeuverTypeEnum.Orbit)
	{
	 ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "align");
	}
	
    if (null !=RetreatReasonDread)
        {	
            var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
            if (probeScannerWindow == null)
                Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
            var scanActuallyAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(ActuallyAnomaly);
                Host.Log("               I'm a chicken and I'm run from dread");
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
        if ( null != RetreatReasonEndSite)
            ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
        if (!Tethering)
        ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "warp");
        else
        {
            while (HulHpPercent < 100 ||ArmorHpPercent < 100  ||ShieldHpPercent < 100 )
                {
                Host.Log("               refill hp");
                Host.Delay(5823);
                }
            if (0 < RetreatReason?.Length)
                ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
        }
        SiteFinished = false;
        K=1;
	}
	else 
		{ DroneEnsureInBay();}
    if (0 == RetreatReason?.Length)
    {  
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

int? HulHpPercent => ShipUi?.HitpointsAndEnergy?.Struct / 10;
int? ShieldHpPercent => ShipUi?.HitpointsAndEnergy?.Shield / 10;
int? ArmorHpPercent => ShipUi?.HitpointsAndEnergy?.Armor / 10;
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
string RetreatReasonEndSite = null;
string RetreatReasonDread = null;
string RetreatReason => RetreatReasonPermanent ?? RetreatReasonBumped
        ?? RetreatReasonCapsuled ?? RetreatReasonTimeElapsed
        ?? RetreatReasonTemporary ?? RetreatReasonDrones
        ?? RetreatReasonCargoFull ?? RetreatReasonEndSite
        ??  RetreatReasonDread ?? null;

int? LastCheckOreHoldFillPercent = null;
int OffloadCount = 0;
bool OreHoldFilledForOffload => Math.Max(0, Math.Min(100, EnterOffloadOreHoldFillPercent)) <= OreHoldFillPercent;

Func<object> MainStep()
{   

         if (Measurement?.IsDocked ?? false)
    {    
        Host.Delay(4111);
        FullCargoMessage = false;
        while ( K>0)
            {
                KaboonusTalk ();
                ReviewSettings();
                K--;
                return MainStep;
            }
        if ( OffloadCount > LimitOffloadCount || ReasonTimeElapsed || ReasonCapsuled ||ReasonDrones)
        {
            if (ReasonDrones) 
			{ Host.Log("                Until you refill your drones = bot stop");
				
                return BotStopActivity;
			}
            else
            { 
                Host.Log("               Times up , too many anomalies or you are naked = bot stop");
        	    Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LMENU, VirtualKeyCode.SHIFT, VirtualKeyCode.VK_Q});
		        Host.Delay(3111);
               
		        return BotStopActivity;
            } 
		}
    while (RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal)
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
    LootValue();
    Sanderling.WaitForMeasurement(); 
    InInventoryUnloadItems();
    Sanderling.WaitForMeasurement(); 
    StackAll ();
    Sanderling.WaitForMeasurement(); 

    while (hostileOrNeutralsInLocal && RetreatOnNeutralOrHostileInLocal ) 
    {   Host.Log("               I feel a great disturbance in the Force ... taking a nap into station until hostiles go from this system");      
                       Random rnd = new Random();
    int DelayTime = rnd.Next(MinimDelayUndock, MaximDelayUndock);
        Host.Log("               next check in :  " + DelayTime+ " s ");
        Host.Delay( DelayTime*1000);}
    if ((!hostileOrNeutralsInLocal && RetreatOnNeutralOrHostileInLocal ) || (!RetreatOnNeutralOrHostileInLocal))
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
        if (Tethering)
        {while (HulHpPercent < 100 ||ArmorHpPercent < 100  ||ShieldHpPercent < 100 || !Tethering )
        {
        Host.Log("               Luke > I try Master Yoda, ... I try ... to refill my HP !");
        Host.Delay(5823);

        }}
        if (0 < DronesInSpaceCount  &&  NoRatsOnGrid)
        DroneEnsureInBay(); 
        ModuleMeasureAllTooltip();
                if (ActivateHardener)
	    	ActivateHardenerExecute();
    	if (ActivateOmni)	
	    	ActivateOmniExecute();
        if (ActivateSensorBoost)
            ActivateSensorBoostExecute();
               Host.Log("               Refreshing news: I'm ready for rats");
        if (0 == DronesInSpaceCount  &&  NoRatsOnGrid)
        {         
            if ( !Tethering)
            return InBeltMineStep;

        ReturnToOldSite ();
            if (RattingAnomaly)
            {
                    Host.Log("               I would like to spin around rocks");
                return TakeAnomaly;
            }           
        }
    }
	if (ActivateHardener)
		ActivateHardenerExecute();
	if (ActivateOmni)	
		ActivateOmniExecute();
    if (ActivateSensorBoost)
        ActivateSensorBoostExecute();
	return InBeltMineStep;
}
var FullCargoMessage = false;
void CloseModalUIElement()
{

    var ConnectionLost = Measurement?.WindowOther?.FirstOrDefault()?.LabelText?.FirstOrDefault(text => (text?.Text.RegexMatchSuccessIgnoreCase("Connection") ?? false));
    
    var NotEnoughCargo = Sanderling?.MemoryMeasurementParsed?.Value?.WindowOther?.FirstOrDefault()?.LabelText?.FirstOrDefault(text => (text?.Text.RegexMatchSuccessIgnoreCase("Not enough cargo space") ?? false));
    var ButtonClose =
        ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("close|no|ok"));
    var ButtonQuit =
        ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("quit|close|ok"));

    if (NotEnoughCargo != null)
    { 
        var OkyButton = Sanderling?.MemoryMeasurementParsed?.Value?.WindowOther?.FirstOrDefault()?.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("ok"));
        if (OkyButton != null)
            Sanderling.MouseClickLeft(OkyButton);
                    Host.Delay(3500);
        FullCargoMessage = true;
        StopAfterburner();
        ActivateArmorRepairerExecute();
        ActivateShieldBoosterExecute();

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
    Host.Log("               Launching my Vipers");
    Sanderling.MouseClickRight(DronesInBayListEntry);
    Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("launch", RegexOptions.IgnoreCase));
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
    //Sanderling.MouseClickRight(DronesInSpaceListEntry);
    //Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("return.*bay", RegexOptions.IgnoreCase));
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


        while (HulHpPercent < 100 ||ArmorHpPercent < 100  ||ShieldHpPercent < 100 || !Tethering )
        {
        Host.Log("               Luke > I try Master Yoda, ... I try!");
        Host.Delay(5823);
        }
        if (Sanderling?.MemoryMeasurementParsed?.Value?.ShipUi?.SpeedMilli>2000)
           Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
           RetreatUpdate();
        ModuleMeasureAllTooltip();
        CheckLocation();
            while (hostileOrNeutralsInLocal && RetreatOnNeutralOrHostileInLocal ) 
        {                        Random rnd = new Random();
        int DelayTime = rnd.Next(MinimDelayUndock, MaximDelayUndock);
            Host.Log("               next check hostiles in :  " + DelayTime+ " s ");
            Host.Delay( DelayTime*1000);
        }
        ReturnToOldSite ();  
        
    }
   
}

Func<object> DefenseStep()
{
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
     if (ActivateArmorRepairer == true || ArmorHpPercent < StartArmorRepairerHitPoints)
    {
        Host.Log("               Armor integrity < "  + StartArmorRepairerHitPoints + "%");
        ActivateArmorRepairerExecute();
    }
    if (ArmorHpPercent > StartArmorRepairerHitPoints && ActivateArmorRepairer == false)
    { StopArmorRepairer(); }
        if (ActivateShieldBooster == true || ShieldHpPercent < StartShieldRepairerHitPoints)
    { 
    	if (ShieldBoosterCount >0)
        Host.Log("               Armor integrity < "  + StartShieldRepairerHitPoints + "%");
        ActivateShieldBoosterExecute();
    }
    if (ShieldHpPercent > StartShieldRepairerHitPoints && ActivateShieldBooster == false)
    { StopShieldBooster(); }
    if ( (DronesInSpaceCount + DronesInBayCount ) < DroneNumber)
	{
	reasonDrones = true;
	}
    if (Measurement?.ShipUi?.Indication?.ManeuverType != ShipManeuverTypeEnum.Orbit)
		return InBeltMineStep;	
    if (null == targetSelected)
        LockTarget();
    if (0 < DronesInBayCount && DronesInSpaceCount < DroneNumber)
       {    if (!UseSalvageDrones)
            DroneLaunch();
            else 
            LaunchDronesByLabelName(LabelNameAttackDrones);

       }
    if (null != targetSelected)
    {
        if (shouldAttackTarget)
        {
		if (droneInLocalSpaceIdle && (Measurement?.Target?.Length > 0))
			{
				Sanderling.KeyboardPress(attackDrones);
				Host.Log("               Vipers message: Sir! Yes Sir! We engage the target");			
            }
        }
        else
            UnlockTarget();
    }
    if (Measurement?.Target?.Length < TargetCountMax && 1 < ListRatOverviewEntry?.Count())
        LockTarget();
    if (droneInLocalSpaceIdle && ListRatOverviewEntry?.FirstOrDefault()?.DistanceMax > ShipRangeMax)
        OrbitRats();
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
    if (DefenseExit)
    {
	StopAfterburner();
	DroneEnsureInBay();
        Console.Beep(523, 125);
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
var startsalvaging = false;
Sanderling.Accumulation.IShipUiModule[] SetModuleTractorActive	 =>
	SetModuleTractorBeam?.Where(module => module?.RampActive ?? false)?.ToArray();	

Func<object> InBeltMineStep()
{
        if (!ReadyForManeuver)
      return MainStep;
      if (Tethering)
{
return MainStep;
}
       var droneListView = Measurement?.WindowDroneView?.FirstOrDefault()?.ListView;
    var droneGroupWithNameMatchingPattern = new Func<string, DroneViewEntryGroup>(namePattern =>
        droneListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(group => group?.LabelTextLargest()?.Text?.RegexMatchSuccessIgnoreCase(namePattern) ?? false));
    var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
        var droneGroupInLocalSpace = droneGroupWithNameMatchingPattern("local space");
    var setDroneInLocalSpace = droneListView?.Entry?.OfType<DroneViewEntryItem>()
        ?.Where(drone => droneGroupInLocalSpace?.RegionCenter()?.B < drone?.RegionCenter()?.B)
        ?.ToArray();
    var droneInLocalSpaceSetStatus =
        setDroneInLocalSpace?.Select(drone => drone?.LabelText?.Select(label => label?.Text?.StatusStringFromDroneEntryText()))?.ConcatNullable()?.WhereNotDefault()?.Distinct()?.ToArray();
    var droneInLocalSpaceIdle =
        droneInLocalSpaceSetStatus?.Any(droneStatus => droneStatus.RegexMatchSuccessIgnoreCase("idle")) ?? false;

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
		ActivateArmorRepairerExecute();//to be sure I stay alive, rats can target me
        //deleteBookmark();
        return TakeAnomaly;
		}
	}
    if (ReadyToBattle && (Measurement?.ShipUi?.Indication?.ManeuverType != ShipManeuverTypeEnum.Orbit))
    {

        if (!OldSiteExist)
            SavingLocation ();
        Orbitkeyboard();
        if (DefenseEnter) 
        {
            if (combatTab != OverviewTabActive)
            { 
            Sanderling.MouseClickLeft(combatTab);
            Host.Delay(1111);
            }
            return DefenseStep;
        }
    }
    EnsureWindowInventoryOpen();
 if (TakeLoot)
 {
             if (salvageTab != OverviewTabActive)
            { 
            Sanderling.MouseClickLeft(salvageTab);
                Host.Delay(1111);
            }
    if ((!OreHoldFilledForOffload
     &&  0 < listOverviewCommanderAll.Length)
     && (LookingAtStars || ShipIsSleeping ))
    {
        StopAfterburner ();
    if (0 == listOverviewCommanderWreck?.Length)
    DroneEnsureInBay();
    while (0 < DronesInBayCount && DronesInSpaceCount < DroneNumber && 0 < listOverviewCommanderWreck?.Length)
    {LaunchDronesByLabelName(LabelNameSalvageDrones);}

    var moduleTractorInactive = SetModuleTractorInactive?.FirstOrDefault();
    var moduleTractorActive = SetModuleTractorActive?.FirstOrDefault();
    Host.Log("               looting  :))");

    if(listOverviewCommanderAll?.FirstOrDefault()?.DistanceMax < 20000)
    { 
    Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});    
    Sanderling.KeyDown(lockTargetKeyCode);
    Sanderling.MouseClickLeft(listOverviewCommanderAll?.FirstOrDefault());
    Sanderling.KeyUp(lockTargetKeyCode);
    if (SetModuleTractorInactive?.Length >0)
    ModuleToggle(moduleTractorInactive);
    if(listOverviewCommanderAll?.FirstOrDefault()?.DistanceMax < 2000)
        {   
        WreckLoot();  
        LootingCargo();

        }
    }
    else 
        {   
        WreckLoot();  
        LootingCargo();

        }

    if (droneInLocalSpaceIdle && 0 < listOverviewCommanderWreck?.Length)
    Sanderling.KeyboardPress(attackDrones);
    if (0 == listOverviewCommanderWreck?.Length)
    DroneEnsureInBay();
    }
    if (( OreHoldFilledForOffload || 0 == listOverviewCommanderAll?.Length ) 
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
        if ( 0 == listOverviewCommanderAll?.Length ) 
        {deleteBookmark ();
        DroneEnsureInBay(); 
        Host.Log("                Im coolest! Site finished! "); 
		SiteFinished = true;	
        
        return TakeAnomaly;
        }
	}
 }
 else 
 {
deleteBookmark ();
        DroneEnsureInBay(); 
        Host.Log("                I don't love the loot! Site finished! "); 
		SiteFinished = true;	//this is just for show, unused
        
        return TakeAnomaly;

 }

    return InBeltMineStep;
}

var reasonDrones = false;
int L=3;
Func<object> TakeAnomaly()
{
    Host.Log("               take Anomaly");
    ModuleMeasureAllTooltip();
	if ( OreHoldFillPercent > 0)
    {
        Host.Log("               You won't start a new anomaly with the cargo at : " +OreHoldFillPercent+ " %  . Go to unload !");
        ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");        
        return MainStep;
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
  if  (FullCargoMessage == false && listOverviewCommanderAll?.FirstOrDefault()?.DistanceMax > 80)
 ClickMenuEntryOnMenuRoot(listOverviewCommanderAll?.FirstOrDefault(), "open cargo");
}
void LootingCargo ()
{
    var LootButton = Measurement?.WindowInventory?[0]?.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("Loot All")); 
        Host.Log("               Teleporting some loot!");
        Sanderling.MouseClickLeft(LootButton);
    EnsureWindowInventoryOpenActiveShip();
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

var ShipHasHold = inventoryActiveShipEntry?.TreeEntryFromCargoSpaceType(ShipCargoSpaceTypeEnum.General) != null;
var hasHold = ShipHasHold;
ITreeViewEntry InventoryActiveShipContainer
{
    get
    {
        var hasHold = ShipHasHold;
        return
        WindowInventory?.ActiveShipEntry?.TreeEntryFromCargoSpaceType( hasHold ? ShipCargoSpaceTypeEnum.OreHold : ShipCargoSpaceTypeEnum.General);
    }
}
IInventoryCapacityGauge OreHoldCapacityMilli =>
    (InventoryActiveShipContainer?.IsSelected ?? false) ? WindowInventory?.SelectedRightInventoryCapacityMilli : null;
int? OreHoldFillPercent => OreHoldCapacityMilli?.Max > 0 ? ((int?)((OreHoldCapacityMilli?.Used * 100) / OreHoldCapacityMilli?.Max )) : 0 ;
var reasonCapsule  = false;
Sanderling.Accumulation.IShipUiModule[] SetModuleWeapon =>
	Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? false)?.ToArray();
int?		WeaponRange => SetModuleWeapon?.Select(module =>
	module?.TooltipLast?.Value?.RangeOptimal ?? module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();
string OverviewTypeSelectionName =>
    WindowOverview?.Caption?.RegexMatchIfSuccess(@"\(([^\)]*)\)")?.Groups?[1]?.Value;
Parse.IOverviewEntry[] ListRatOverviewEntry => WindowOverview?.ListView?.Entry?.Where(entry =>
    (entry?.MainIconIsRed ?? false))
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"battery|tower|sentry|web|strain|splinter|render|raider|friar|reaver")) //Frigate
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"coreli|centi|alvi|pithi|corpii|gistii|cleric|engraver")) //Frigate
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelior|centior|alvior|pithior|corpior|gistior")) //Destroyer
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelum|centum|alvum|pithum|corpum|gistum|prophet")) //Cruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelatis|centatis|alvatis|pithatis|corpatis|gistatis|apostle")) //Battlecruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"core\s|centus|alvus|pith\s|corpus|gist\s")) //Battleship
    ?.ThenBy(entry => entry?.DistanceMax ?? int.MaxValue)
    ?.ToArray();

Parse.IOverviewEntry[] listOverviewCommanderWreck =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(commanderNameWreck) ?? true)
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"Commander|Dark Blood|true|Shadow Serpentis|Dread Gurista|Domination Saint|Gurista Distributor|Sentient|Overseer|Spearhead|Dread Guristas|Estamel|Vepas|Thon|Kaikka|True Sansha|Chelm|Vizan|Selynne|Brokara|Dark Blood|Draclira|Ahremen|Raysere|Tairei|Cormack|Setele|Tuvan|Brynn|Domination|Tobias|Gotan|Hakim|Mizuro")) //Battleship
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"core\s|corpatis|centus|alvus|pith\s|corpus|gist\s")) //Battleship
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelatis|centatis|alvatis|pithatis|corpatis|gistatis|apostle")) //Battlecruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelum|centum|alvum|pithum|corpum|gistum|prophet")) //Cruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelior|centior|alvior|pithior|corpior|gistior")) //Destroyer
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"coreli|centi|alvi|pithi|corpii|gistii|cleric|engraver")) //Frigate
    .ToArray(); 
Parse.IOverviewEntry[] listOverviewCommanderAll =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(CargoWreck) ?? true)
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"cargo|Commander|Dark Blood|true|Shadow Serpentis|Dread Gurista|Domination Saint|Gurista Distributor|Sentient|Overseer|Spearhead|Dread Guristas|Estamel|Vepas|Thon|Kaikka|True Sansha|Chelm|Vizan|Selynne|Brokara|Dark Blood|Draclira|Ahremen|Raysere|Tairei|Cormack|Setele|Tuvan|Brynn|Domination|Tobias|Gotan|Hakim|Mizuro")) //Battleship
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"core\s|corpatis|centus|alvus|pith\s|corpus|gist\s")) //Battleship
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelatis|centatis|alvatis|pithatis|corpatis|gistatis|apostle")) //Battlecruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelum|centum|alvum|pithum|corpum|gistum|prophet")) //Cruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelior|centior|alvior|pithior|corpior|gistior")) //Destroyer
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"coreli|centi|alvi|pithi|corpii|gistii|cleric|engraver")) //Frigate

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
    ?.Where(entry => (entry?.Name?.RegexMatchSuccess(runFromRats) ?? true))// || (entry?.Type?.RegexMatchSuccess(runFromRats) ?? true))
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
	?.ToArray(); 
DroneViewEntryGroup DronesInBayListEntry =>
    WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in bay", RegexOptions.IgnoreCase));
DroneViewEntryGroup DronesInSpaceListEntry =>
    WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in Local Space", RegexOptions.IgnoreCase));
int? DronesInSpaceCount => DronesInSpaceListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();
int? DronesInBayCount => DronesInBayListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();

public bool Tethering =>
    Measurement?.ShipUi?.EWarElement?.Any(EwarElement => (EwarElement?.EWarType).RegexMatchSuccess("tethering")) ?? false;
public bool ReadyForManeuverNot =>
    Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("warp|docking")) ?? false;
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
	Host.Log("               Starbuck : I'm searching my 'fumerellos' ....");
    ////salvager
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
		if(Sanderling.MemoryMeasurementParsed?.Value?.IsDocked ?? false)
			break;
		foreach(var NextModule in Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule)
		{
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

void ActivateShieldBoosterExecute()
{
var SubsetModuleShieldBooster =
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
    ?.Where(module => module?.TooltipLast?.Value?.IsShieldBooster ?? false);
    var SubsetModuleToToggle =
        SubsetModuleShieldBooster
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleShieldBooster.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void StopShieldBooster()
{
var SubsetModuleShieldBooster =
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
    ?.Where(module => module?.TooltipLast?.Value?.IsShieldBooster ?? false);
var SubsetModuleToToggle =
        SubsetModuleShieldBooster
        ?.Where(module => (module?.RampActive ?? false));
    if ( SubsetModuleShieldBooster.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivateHardenerExecute()
{
    var SubsetModuleHardener =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule        
        ?.Where(module => module?.TooltipLast?.Value?.IsHardener ?? false);
     var SubsetModuleToToggle =
        SubsetModuleHardener
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleHardener.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}

void ActivateArmorRepairerExecute()
{
var SubsetModuleArmorRepairer =
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
    ?.Where(module => module?.TooltipLast?.Value?.IsArmorRepairer ?? false);
    var SubsetModuleToToggle =
        SubsetModuleArmorRepairer
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleArmorRepairer.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void StopArmorRepairer()
{
var SubsetModuleArmorRepairer =
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
    ?.Where(module => module?.TooltipLast?.Value?.IsArmorRepairer ?? false);
    var SubsetModuleToToggle =
        SubsetModuleArmorRepairer
        ?.Where(module => (module?.RampActive ?? false));
    if ( SubsetModuleArmorRepairer.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivateWeaponExecute()
{
    var SubsetModuleWeapon =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? false);
    var SubsetModuleToToggle =
        SubsetModuleWeapon
        ?.Where(module => !(module?.RampActive ?? false));
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
    if ( SubsetModuleAfterburner.Count()>0)
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
    if ( SubsetModuleAfterburner.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
    {
		ModuleToggle(Module); 
	}
}
void ActivateOmniExecute()
{
    var SubsetModuleOmni =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.LabelText?.Any(
		label => label?.Text?.RegexMatchSuccess(OmniSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
    var SubsetModuleToToggle =
        SubsetModuleOmni
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleOmni.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivateSensorBoostExecute()
{
    var SubsetModuleSensorBoost =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.LabelText?.Any(
		label => label?.Text?.RegexMatchSuccess(SensorSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
    var SubsetModuleToToggle =
        SubsetModuleSensorBoost
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleSensorBoost.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
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
	OffloadCountUpdate();
	Timers (); 
}

var logoutme= false;
var logoutgame = (eveRealServerDT-DateTime.UtcNow ).TotalMinutes;
void Timers ()
{
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
	|| (listOverviewEntryEnemy?.Length > 0))
    { 
    Sanderling.InvalidateMeasurement();
    if ((RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal)
	|| (listOverviewEntryEnemy?.Length > 0))
    RetreatReasonTemporary = " Hostiles in local ! ";
    }
    else RetreatReasonTemporary = null;
        RetreatReasonEndSite = ReasonEnd ? " Site Finished": null;
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
    Sanderling.KeyDown(orbitKeyCode);
    if (AnomalyToTake == "haven"|| AnomalyToTake == "Haven")
        Sanderling.MouseClickLeft(ListCelestialObjects?.FirstOrDefault());
    else
        Sanderling.MouseClickLeft(ListCelestialObjects?.Skip(1)?.FirstOrDefault());
    Sanderling.KeyUp(orbitKeyCode);
    ActivateAfterburnerExecute();
    Host.Delay(1111);
    Host.Log("               Rats Are in my range, better to Orbit arround Celestials");
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
void OffloadCountUpdate()
{
    var CapsuleType = WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
            ?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase("Capsule") ?? false);							
    if (null !=CapsuleType )
    {
    reasonCapsule = true;
    Host.Log("                Reason capsule "+reasonCapsule + " ");
    }
    var OreHoldFillPercentSynced = OreHoldFillPercent;
    if (!OreHoldFillPercentSynced.HasValue)
        return;
    if (0 == OreHoldFillPercentSynced && OreHoldFillPercentSynced < LastCheckOreHoldFillPercent)
        ++OffloadCount;
    LastCheckOreHoldFillPercent = OreHoldFillPercentSynced;
}

void ReviewSettings()
{ 
Host.Log("                >>> Settings Review bot " + VersionScript + "");
Host.Log("                - Start (UTC) :  " + dateAndTime.ToString(" dd/MM/yyyy HH:mm:ss")+ " (-1 min); ");
Host.Log("               ⊙ Kaboonus Gift From Yesterday :  " +HocusPocusPreparatus.ToString("N0")+ "");
Host.Log("                - retreat on neutrals :  " + RetreatOnNeutralOrHostileInLocal + " ; ");
        Host.Log("                - ratting anomaly :  " + RattingAnomaly + " ; ");
            Host.Log("                - anomaly name to take:  " + AnomalyToTake + " ; ");
                Host.Log("                - next DT (Eve Time) :  " + eveRealServerDT.ToString(" dd/MM/yyyy HH:mm:ss")+ " (-1 min); ");
                    Host.Log("                - Safe DT (Eve Time) :  " + eveSafeDT.ToString(" dd/MM/yyyy HH:mm:ss")+ " (-1 min); ");
 		        	Host.Log("                - Play sesion  end (ET) :  "  +playSession.ToString(" dd/MM/yyyy HH:mm:ss")+ "" );
                    Host.Log("                - Closer logout :  " +(TimeSpan.FromMinutes(logoutgame)).ToString(@"dd") + " days and " + ((TimeSpan.FromMinutes(logoutgame) < TimeSpan.Zero) ? "-" : "") + (TimeSpan.FromMinutes(logoutgame)).ToString(@"hh\:mm\:ss")+ " ; When times up: You Dock, logout and stopbot  ; ");
                        Host.Log("                - bookmark home: " + RetreatBookmark + " ; ");
                                Host.Log("                - delay undock min(max) :  " + MinimDelayUndock + "(" + MaximDelayUndock + " ); ");
                                Host.Log("                ⊙ Kaboonus Gift in Isk :  " +HocusPocus+ "");
                                Host.Log("                ⊙ Kaboonus Gift in Loot :  " +StatusLoot+ "");
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
Sanderling.InvalidateMeasurement(); 
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
  string StatusLoot;
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

 HocusPocus =Paracelsus.ToString("N0");
 HocusPocus = Regex.Replace(HocusPocus, ",", " ");
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
