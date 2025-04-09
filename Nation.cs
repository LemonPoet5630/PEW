using System.Collections.Generic;
using Godot;

public partial class Nation : Node
{

    [Export] public bool IsPlayer { get; private set; }
    public void ChangePlayerStatus(bool isPlayer) => IsPlayer = isPlayer;

    //Debug
    [Export] public bool isActive = false;

    //Basic Info
    [Export] public string Tag { get; private set; }
    [Export] public string NationName { get; private set;}
    public void ChangeNationName(string name) => NationName = name;
    [Export] public string Description { get; private set; }
    [Export] public Color NationalColor { get; private set; }
    [Export] public Texture2D Flag { get; private set; }

    //Stuff
    public List<int> OwnedArmyList;

    //Territory
    public List<State> ControlledStates { get; private set; }
    public List<State> OwnedStates { get; private set; }

    //Resources
    [Export] public int InitialGold { get; private set; }
    [Export] public int InitialFood { get; private set; }

    //Diplomacy
    [Export] public string[] InitialAtWar { get; private set; }
    //public List<string> AtWar { get; private set; }

    //Basic Statistics
    public int Population { get; private set; }
    
    public Diplomacy Diplomacy { get; private set; }
    public Economy Economy { get; private set; }
    public Stockpile Stockpile { get; private set; }
    public Technology Technology { get; private set; }

    public void Initialize() {
        ResetAll();
        Economy.gold = InitialGold;
        Economy.food = InitialFood;
    }

    public void ResetAll() {
        
        //Diplomacy
        ResetDiplomacy();
        ResetEconomy();
        //Owned and Controlled States
        OwnedStates = [];
        ControlledStates = [];
        //Army
        OwnedArmyList = [];
    }

    public void ResetDiplomacy() {
        Diplomacy = new();
        foreach (string nationTag in NationManager.Nations_Tag.Keys) {
            
            //Relations
            Diplomacy.relations[nationTag] = 0;

            //AtWar
            Diplomacy.atWar[nationTag] = false;
            InitialAtWar ??= [];
            foreach (string atWarNationTag in InitialAtWar) {
                Diplomacy.atWar[atWarNationTag] = true;
            }
        }
    }
    public void ResetEconomy() {
        Economy = new();
    }

    public void AddResource(int amount, string resource) {
        switch (resource) {
            case "Gold":
                Economy.gold += amount;
                break;
            case "Food":
                Economy.food += amount;
                break;
        }
    }
    public void GainState(State state) => OwnedStates.Add(state);
    public void GainControlState(State state) => ControlledStates.Add(state);
    public void LoseState(State state) => OwnedStates.Remove(state);
    public void LoseControlState(State state) => ControlledStates.Remove(state);
}