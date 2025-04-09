using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class ConnectionManager : Node
{

    [Export] public Label lockedStateText;

    public string lockedStateName = GameConstants.DEFAULT_STRING;

    public static Dictionary<string, List<string>> Connections;

    public static ConnectionManager Instance;
    public static AStar2D AStar { get; private set; }

    [Export] public bool connectionMode;

    public override void _Ready()
    {
        base._Ready();

        Instance = this;
        
        Connections = [];
        AStar = new();
        
        LoadConnections();
        InitializeAStar();
        foreach (string state1Name in Connections.Keys) {
            if (!StateManager.States_Name.ContainsKey(state1Name)) {
                Connections.Remove(state1Name);
                foreach (string tempStateName in Connections.Keys) Connections[tempStateName].Remove(state1Name);
                continue;
            }
            foreach (string state2Name in Connections[state1Name]) {
                MapManager.Instance.CreateConnectionLine(state1Name, state2Name);
            }
        }
    }
    //====================================================================
    public void LockState(State state) {
        UnlockState();
        lockedStateName = state.Name;
        lockedStateText.Text = state.StateName;
    }
    public void UnlockState() {
        lockedStateName = GameConstants.DEFAULT_STRING;
        lockedStateText.Text = "";
    }
    //====================================================================

    public void CreateConnection(string stateName) {
        if (!connectionMode) return;

        if (lockedStateName.Equals(GameConstants.DEFAULT_STRING)) return;
        if (CheckConnectionExists(lockedStateName, stateName)) {
            GD.Print("ConnectionManager: Connection between " + lockedStateName + " and " + stateName + " already exists!");
        }
        else { //Create connection
            Connections[lockedStateName].Add(stateName);
            Connections[stateName].Add(lockedStateName);
            MapManager.Instance.CreateConnectionLine(lockedStateName, stateName);
            GD.Print("ConnectionManager: Connection between " + lockedStateName + " and " + stateName + " established!");
        }
    }
    public void ResetConnection() {
        if (!connectionMode) return;
        try {
            foreach (string connectedName in Connections[lockedStateName]) Connections[connectedName].Remove(lockedStateName);
            Connections[lockedStateName].Clear();
            MapManager.Instance.DeleteConnectionLine(lockedStateName);
            GD.Print("ConnectionManager: " + lockedStateName + "'s Connections removed!");
        }
        catch {
            GD.Print("ConnectionManager: ResetConnection() failed!");
        }
    }
    public bool CheckConnectionExists(string state1Name, string state2Name) {
        if (Connections[state1Name].Contains(state2Name) || Connections[state2Name].Contains(state1Name)) {
            return true;
        }
        return false;
    }

    public void SaveConnections() {
        if (!connectionMode) return;

        FileAccess connectionFile = FileAccess.Open("res://Data/Connections.json", FileAccess.ModeFlags.Write);
        string json = JsonSerializer.Serialize(Connections);
        connectionFile.StoreString(json);
        connectionFile.Close();

        GD.Print("ConnectionManager: Connections saved!");
    }
    public void LoadConnections() {
        FileAccess connectionFile = FileAccess.Open("res://Data/Connections.json", FileAccess.ModeFlags.Read);
        if (connectionFile == null) {
            GD.Print("ConnectionManager: Cannot find file!");
            return;
        }
        Tools.StartTimeCount();
        string json = connectionFile.GetAsText();
        Connections = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
        connectionFile.Close();

        foreach (State state in StateManager.States_Name.Values) if (!Connections.ContainsKey(state.Name)) Connections[state.Name] = []; //If state that exists in the game does not in Connections
        Tools.StopTimeCount("ConnectionManager", "Loading Connections");
    }

    public override void _Input(InputEvent @event)
    {
        if (!connectionMode) return;

        base._Input(@event);

        if (Input.IsActionJustPressed("Q")) { //Save
            if (!connectionMode) return;
            SaveConnections();
        }
        else if (Input.IsActionJustPressed("R")) { //Reset
            if (!connectionMode) return;
            ResetConnection();
        }
        else if (Input.IsActionJustPressed("T")) { //Toggle connection
            MapManager.Instance.ToggleConnections();
        }
    }

    private static void InitializeAStar() {
        Tools.StartTimeCount();
        AStar.Clear();
        foreach (State state in StateManager.States_Name.Values) { //First create all the points
            AStar.AddPoint(state.ID, Tools.FindCentroid(state), 1);
        }
        foreach (State state1 in StateManager.States_Name.Values) {
            foreach (string state2Name in Connections[state1.Name]) {
                int ID1 = state1.ID;
                int ID2 = StateManager.States_Name[state2Name].ID;
                if (ID1 != ID2) AStar.ConnectPoints(ID1, ID2, false);
            }
        }
        ResetAStar();
        Tools.StopTimeCount("ConnectionManager", "AStar Initialization");
    }

    public static void ResetAStar() {
        foreach (int pointID in AStar.GetPointIds()) {
            AStar.SetPointDisabled(pointID, false);
        }

    }
    
    public static void UpdateAStar(string refNationTag) {
        ResetAStar();
        foreach (int pointID in AStar.GetPointIds()) {
            Nation refNation = NationManager.Nations_Tag[refNationTag];
            Nation pointOwner = NationManager.Nations_Tag[StateManager.States_ID[pointID].OwnerTag];
            
            AStar.SetPointDisabled(pointID, true);

            if (refNationTag == pointOwner.Tag) //Own nation
                AStar.SetPointDisabled(pointID, false);
            if (refNation.Diplomacy.atWar[pointOwner.Tag] == true) { //At war
                AStar.SetPointDisabled(pointID, false);
            }
        }
    }

}
