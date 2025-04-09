using Godot;
using System.Collections.Generic;

public partial class NationManager : Node
{
    public static NationManager Instance;
    public static Dictionary <string, Nation> Nations_Tag;
    public override void _Ready()
    {
        //base._Ready();
        Instance = this;
        Nations_Tag = [];
        InitializeNations();
    }

    private static void InitializeNations()
    {
        Tools.StartTimeCount();
        foreach (Nation nation in NodeManager.Instance.nationsNode.GetChildren()) Nations_Tag[nation.Tag] = nation;
        foreach (Nation nation in Nations_Tag.Values) nation.Initialize();
        Tools.StopTimeCount("NationManager", "Nations_Tag initialization");
    }

    public static int CalculateIncome(string nationTag, string resource) {
        Nation nation = Nations_Tag[nationTag];
        int income = 0;
        switch (resource) {
            case "All":
                break;
            case "Gold":
                foreach (State state in nation.OwnedStates) {
                    income += state.Population * GameConstants.GOLD_POP_MULTIPLIER;
                }
                break;
            case "Food":
                foreach (State state in nation.OwnedStates) {
                    income += state.FoodProduction;
                }
                break;
        }
        return income;
    }
    public static void AddIncome(string nationTag, string resource) {
        Nation nation = Nations_Tag[nationTag];
        if (resource == "All") {
            //???
        }
        else {
            int income = CalculateIncome(nationTag, resource);
            nation.AddResource(income, resource);
        }
    }
}
