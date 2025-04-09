using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ArmyManager : Node
{
    public static Dictionary<int, Army> Armies_ID; // ID - Army Instantiation
    public static Dictionary<int, Corps> Corps_ID; // ID - Corps Instantiation
    public override void _Ready()
    {
        base._Ready();
        Armies_ID = [];
        Corps_ID = [];
    }
    public static void CreateArmy (string OwnerTag, List<(string, int)> location_amount) {
        Army army = new(OwnerTag);
        Armies_ID[army.ID] = army;
        NationManager.Nations_Tag[OwnerTag].OwnedArmyList.Add(army.ID);
        foreach ((string, int) loc_am in location_amount) {
            CreateCorps(army.ID, loc_am.Item1, loc_am.Item2);
        }
    }
    public static void DeleteArmy (int armyID) {
        NationManager.Nations_Tag[Armies_ID[armyID].OwnerTag].OwnedArmyList.Remove(armyID);
        Armies_ID.Remove(armyID);
    }
    public static void CreateCorps (int armyID, string location, int amount) {
        Corps corps = new(armyID, location, amount);
        Corps_ID[corps.ID] = corps;
        Armies_ID[armyID].CorpsList.Add(corps.ID);
        StateManager.States_Name[location].stationedCorpsList.Add(corps.ID);
    }
    public static void DeleteCorps (int corpsID) {

        //Remove corps from Army.CorpsList
        int armyID = Corps_ID[corpsID].ArmyID;
        Armies_ID[armyID].CorpsList.Remove(corpsID);
        if (Armies_ID[armyID].CorpsList.Count == 0) { //Army has no more corps
            DeleteArmy(armyID); //Delete army
        }
        //Remove corpsID from State.stationedCorpsID
        StateManager.States_Name[Corps_ID[corpsID].Location].stationedCorpsList.Remove(corpsID);

        //Final
        Corps_ID[corpsID].DestroyLabel();
        Corps_ID.Remove(corpsID);

    }
    public static void CorpsMoveAction (int thisCorpsID, string destinationStateName) {
        
        //Some reference pointers for code clarity
        Nation thisNation = NationManager.Nations_Tag[Armies_ID[GetArmyIDfromCorpsID(thisCorpsID)].OwnerTag];
        //State thisState = StateManager.States_Name[Corps_ID[thisCorpsID].Location];
        Nation otherNation = NationManager.Nations_Tag[StateManager.States_Name[destinationStateName].ControllerTag];
        State otherState = StateManager.States_Name[destinationStateName];
        //GD.Print(thisNation.NationName + ", " + otherNation.NationName + ", " + otherState.Name);
        if (thisNation.Diplomacy.atWar[otherNation.Tag] == true) { //thisNation and otherNation are at war
            List<int> hostileCorpsList = GetHostileCorpsList(thisCorpsID, otherState);
            if (hostileCorpsList.Count == 0) { //If the state does not have any hostile corps
                MoveCorps(thisCorpsID, destinationStateName);
            }
            else { //The state has 1 or more hostile corps stationed there
                do {
                    bool attackerWon = SimulateWarfare(thisCorpsID, hostileCorpsList.Last());
                    if (attackerWon == false) { //Attacker's army is destroyed and battle is concluded as the defender's victory
                        break;
                    }
                    else { //Attacker won
                        hostileCorpsList.RemoveAt(hostileCorpsList.Count - 1); //Remove defeated defending corps
                        if (hostileCorpsList.Count == 0) { //If there are no more hostile corps to defend
                            MoveCorps(thisCorpsID, destinationStateName);
                            break;
                        }
                        else continue; //There are still defending corps
                    }
                } while (true);
            }
        }
        else { //We are not at war with that nation
            MoveCorps(thisCorpsID, destinationStateName);
        }
    }
    public static void MoveCorps(int corpsID, string stateName) {

        Nation thisNation = NationManager.Nations_Tag[Armies_ID[GetArmyIDfromCorpsID(corpsID)].OwnerTag];
        State thisState = StateManager.States_Name[Corps_ID[corpsID].Location];
        //Nation otherNation = NationManager.Nations_Tag[StateManager.States_Name[stateName].ControllerTag];
        State otherState = StateManager.States_Name[stateName];

        otherState.stationedCorpsList.Add(corpsID); //Add corps to destination state
        thisState.stationedCorpsList.Remove(corpsID); //Remove corps from previous state
        Corps_ID[corpsID].Location = stateName; //Update the corps' location
        Corps_ID[corpsID].UpdateLabel();
        StateManager.TransferState(otherState, thisNation); //Transfer state

    }
    public static bool SimulateWarfare(int attackingCorpsID, int defendingCorpsID) { //True if attack successful
        //SIMPLE WARFARE FOR NOW
        //Only handles warfare and casualty/label updates
        int casualty = Math.Min(Corps_ID[attackingCorpsID].amount, Corps_ID[defendingCorpsID].amount);
        Corps_ID[attackingCorpsID].amount -= casualty;
        Corps_ID[defendingCorpsID].amount -= casualty;
        bool attackingDestroyed = Corps_ID[attackingCorpsID].amount == 0;
        bool defendingDestroyed = Corps_ID[defendingCorpsID].amount == 0;
        if (!attackingDestroyed && defendingDestroyed) { //Attacking survives while defending destroyed
            Corps_ID[attackingCorpsID].UpdateLabel();
            DeleteCorps(defendingCorpsID);
            return true;
        }
        else if (attackingDestroyed && !defendingDestroyed) { //Attacking army is destroyed while defending survives
            DeleteCorps(attackingCorpsID);
            Corps_ID[defendingCorpsID].UpdateLabel();
            return false;
        }
        else { //Both are destroyed
            DeleteCorps(attackingCorpsID);
            DeleteCorps(defendingCorpsID);
            return false;
        }
    }
    public static int GetArmyIDfromCorpsID(int corpsID) => Corps_ID[corpsID].ArmyID;

    private static List<int> GetHostileCorpsList(int refCorpsID, State state) {
        List<int> hostileCorpsList = [];
        List<int> corpsList = state.stationedCorpsList;
        Nation thisNation = NationManager.Nations_Tag[Armies_ID[Corps_ID[refCorpsID].ArmyID].OwnerTag];
        foreach (int corpsID in corpsList) {
            Nation otherNation = NationManager.Nations_Tag[Armies_ID[Corps_ID[corpsID].ArmyID].OwnerTag];
            if (thisNation.Diplomacy.atWar[otherNation.Tag]) {
                hostileCorpsList.Add(corpsID);
            }
        }
        return hostileCorpsList;
    }
}
