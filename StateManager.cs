using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class StateManager: Node
{

    public static Dictionary<string, State> States_Name;
    public static Dictionary<int, State> States_ID;
    public static Dictionary<string, State> States_Capital;
    public static Dictionary<string, string[]> Setup { get; private set; }
    public override void _Ready()
    {
        base._Ready();
        Tools.StartTimeCount();

        States_Name = [];
        States_ID = [];
        States_Capital = [];

        Setup = [];
        if (!GameManager.Instance.safeLaunch)
            LoadSetup();
        InitializeStates();
        Tools.StopTimeCount("StateManager", "Initializing States");
    }

    public static void TransferState (State state, Nation newOwner) => state.ChangeOwner(newOwner);
    public static void OccupyState (State state, Nation newController) => state.ChangeController(newController);
    private static void InitializeStates () {
        int stateID = 0;
        foreach (Node continentNode in NodeManager.Instance.statesNode.GetChildren())
        {
            foreach (Node regionNode in continentNode.GetChildren())
            {
                for (int i = 0; i < regionNode.GetChildren().Count; i++)
                {
                    State state = regionNode.GetChildren()[i] as State;
                    if (!GameManager.Instance.safeLaunch && !GameManager.Instance.editScenario) {
                        state.DebugSetOwner(Setup[state.Name][0]);
                        state.DebugSetController(Setup[state.Name][1]);
                    }
                    InitializeStateArea(state);
                    States_ID[stateID] = state;
                    States_Name[state.Name] = state;
                    if (state.IsCapitalState) States_Capital[state.OwnerTag] = state;
                    state.DebugSetID(stateID);
                    stateID++;
                    state.stationedCorpsList = [];

                    NationManager.Nations_Tag[state.OwnerTag].GainState(state);
                    NationManager.Nations_Tag[state.ControllerTag].GainControlState(state);
                }
            }
        }
    }
    private static void InitializeStateArea(State state) {
        state.StateCollisionPolygon = [];
        state.StatePolygon = [];
        foreach (CollisionPolygon2D collisionPolygon in state.GetChildren()) {
            Vector2[] tempPolygon = new Vector2[collisionPolygon.Polygon.Length];
            for (int i = 0; i < collisionPolygon.Polygon.Length; i++) {
                tempPolygon[i] = collisionPolygon.Polygon[i] + state.Position;
            }
            state.StateCollisionPolygon.Add(collisionPolygon);
            //Create Polygon2D Node
            Polygon2D statePolygon = new();
            NodeManager.Instance.statePolygonNode.AddChild(statePolygon);
            statePolygon.Polygon = tempPolygon;
            try {
                statePolygon.Color = NationManager.Nations_Tag[state.ControllerTag].NationalColor;
            }
            catch {
                statePolygon.Color = GameConstants.NULL_COLOR;
            }
            state.StatePolygon.Add(statePolygon);
        }
    }
    private static void LoadSetup () {
        FileAccess stateFile = default;
        if (GameManager.Instance.editScenario) {
            return;
        }
        switch (GameManager.Instance.startYear) {
            case 1900:
                stateFile = FileAccess.Open("res://Data/1900.json", FileAccess.ModeFlags.Read);
                break;
            case 1930:
                stateFile = FileAccess.Open("res://Data/1930.json", FileAccess.ModeFlags.Read);
                break;
        }
        string json = stateFile.GetAsText();
        Setup = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
        stateFile.Close();
        GD.Print("Setup Loaded!");
    }
    public static void SaveSetup () {
        FileAccess stateFile = default;
        if (GameManager.Instance.editScenario) {
            stateFile = FileAccess.Open("res://Data/" + MapManager.Instance.setupScenario + ".json", FileAccess.ModeFlags.Write);
        }
        else {
            switch (GameManager.Instance.startYear) {
                case 1900:
                    stateFile = FileAccess.Open("res://Data/1900.json", FileAccess.ModeFlags.Write);
                    break;
                case 1930:
                    stateFile = FileAccess.Open("res://Data/1930.json", FileAccess.ModeFlags.Write);
                    break;
            }
        }
        foreach (State state in States_Name.Values) Setup[state.Name] = [state.OwnerTag, state.ControllerTag];
        string json = JsonSerializer.Serialize(Setup);
        stateFile.StoreString(json);
        stateFile.Close();
        GD.Print("Setup Saved!");
    }

}
