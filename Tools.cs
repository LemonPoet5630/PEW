using System.Collections.Generic;
using Godot;

//PLACEHOLDER CLASS FOR TOOLS FOR LATER USE
public static class Tools 
{
    public static System.Diagnostics.Stopwatch stopwatch;
    
    public static List<State> GetOwnedStates (string tag) {
        List<State> ownedStates = [];
        foreach (State states in StateManager.States_Name.Values) {
            if (states.OwnerTag.Equals(tag))
                ownedStates.Add(states);
        }
        return ownedStates;
    }
    public static List<State> GetControlledStates (string tag) {
        List<State> controlledStates = [];
        foreach (State states in StateManager.States_Name.Values) {
            if (states.ControllerTag.Equals(tag))
                controlledStates.Add(states);
        }
        return controlledStates;
    }
    public static (List<State>, List<State>) GetBorderStates (string tag) {
        //Finds controlled states that border another nation through connections
        List<State> borderStates = [];
        List<State> borderingStates = [];
        Nation nation = NationManager.Nations_Tag[tag];
        foreach (State state in nation.ControlledStates) {
            if (ConnectionManager.Connections[state.Name].Count == 0) continue;
            List<string> connections = ConnectionManager.Connections[state.Name];
            foreach (string connection in connections) {
                State otherState = StateManager.States_Name[connection];
                if (otherState.ControllerTag != state.ControllerTag) {
                    if (!borderStates.Contains(state)) borderStates.Add(state);
                    if (!borderingStates.Contains(otherState)) borderingStates.Add(otherState);
                }
            }
        }
        return (borderStates, borderingStates);
    }
    public static (List<State>, List<State>) GetBorderStates (string selfTag, List<string> otherTagList) {
        List<State> borderStates = [];
        List<State> borderingStates = [];
        Nation selfNation = NationManager.Nations_Tag[selfTag];
        foreach (State state in selfNation.ControlledStates) {
            if (ConnectionManager.Connections[state.Name].Count == 0) continue; //State has no connections
            List<string> connections = ConnectionManager.Connections[state.Name];
            foreach (string connection in connections) {
                State otherState = StateManager.States_Name[connection];
                if (otherTagList.Contains(otherState.ControllerTag)) {
                    if (!borderStates.Contains(state)) borderStates.Add(state);
                    if (!borderingStates.Contains(otherState)) borderingStates.Add(otherState);
                }
            }
        }
        return (borderStates, borderingStates);
    }
    public static Vector2 FindCentroid(State state) {
        float xCoorSum = 0, yCoorSum = 0;
        int xCoorCount = 0, yCoorCount = 0;
        foreach (Polygon2D polygon in state.StatePolygon) {
            xCoorCount += polygon.Polygon.Length;
            yCoorCount += polygon.Polygon.Length;
            foreach (Vector2 coor in polygon.Polygon) {
                xCoorSum += coor.X + state.Position.X;
                yCoorSum += coor.Y + state.Position.Y;
            }
        }
        float xCoor = xCoorSum / xCoorCount;
        float yCoor = yCoorSum / yCoorCount;
        return new Vector2(xCoor, yCoor);
    }

    public static string CenterString (string text) {
        return "[center]" + text + "[/center]";
    }

    public static void StartTimeCount () {
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
    }
    public static void StopTimeCount(string scriptName, string action) {
        stopwatch.Stop();
        GD.Print(scriptName + ": " + action + " took " + stopwatch.ElapsedMilliseconds + "ms.");
        stopwatch.Reset();
    }
    public static int GetArmyID() {
        int ID = GameManager.ArmyID;
        GameManager.ArmyID++;
        return ID;
    }
    public static int GetCorpsID() {
        int ID = GameManager.CorpsID;
        GameManager.CorpsID++;
        return ID;
    }

    public static Stack<Stack<int>> GetAvailableArmyCorps (string nationTag) {
        Stack<Stack<int>> availableArmyCorps = [];
        
        foreach (int armyID in NationManager.Nations_Tag[nationTag].OwnedArmyList) {
            Stack<int> corpsList = [];
            foreach (int corpsID in ArmyManager.Armies_ID[armyID].CorpsList) {
                corpsList.Push(corpsID);
            }
            availableArmyCorps.Push(corpsList);
        }
        return availableArmyCorps;
    }
    public static Vector2 Snap(Vector2 vector, float epsilon = 0.001f) {
        return new Vector2(Mathf.Round(vector.X / epsilon) * epsilon, Mathf.Round(vector.Y / epsilon) * epsilon);
    }
}