using System;
using System.Collections.Generic;
using Godot;

public partial class State: Area2D
{
    //Mapping
    public List<CollisionPolygon2D> StateCollisionPolygon;
    public List<Polygon2D> StatePolygon;
    public Label ArmyNumLabel;

    // INITIAL VALUES==========================================================================
    [ExportGroup("1936")]
    [Export] public string StateName { get; private set; }
    [Export] public string OwnerTag { get; private set; }
    [Export] public string ControllerTag { get; private set; }
    [Export] public bool IsCapitalState { get; private set; }
    [Export] public string CoresTag { get; private set; }
    [Export] public string ClaimsTag { get; private set; }
    [Export] public int Population { get; private set; }
    [Export(PropertyHint.Range, "0,100,")] public int Development { get; private set; }
    [Export(PropertyHint.Range, "0,100,")] public int Happiness { get; private set; }
    [ExportSubgroup("Resources")]
    [Export] public int FoodProduction { get; private set; }

    [ExportGroup("Absolute State Values")]
    [Export] public bool Passable { get; private set; }
    public int ID { get; private set; }
    //=============================================================================================
    //     STATE VALUES============================================================================
    public float ExertPressure { get; private set; } 
    public float AbsorbPressre { get; private set; }
    public List<int> stationedCorpsList;
    //=============================================================================================

    public bool hover = false;

    //SIGNALS=============================================================
    public void OnMouseEnter() => hover = true; //Mouse enters the state boundary
    public void OnMouseExit() => hover = false; //Mouse exits the state boundary

    public override void _UnhandledInput(InputEvent @event) {
        base._UnhandledInput(@event);
        
        if (Input.IsActionJustPressed("Left_Mouse_Click")) {
            Color controllerColor = NationManager.Nations_Tag[ControllerTag].NationalColor;
            if (hover) { //State is clicked
                GD.Print("Nation: " + ControllerTag + ", " + "State: " + Name + ", " + "ID: " + ID);
                MapManager.Instance.ShowNationBorders(ControllerTag);
                //MapManager.CreateStateBorderLine(Name);
                /*
                if (StatePolygon[0].Color == GameConstants.STATE_CLICK_COLOR) {
                    ChangeColor(controllerColor);
                }
                else if (StatePolygon[0].Color == controllerColor) {
                    ChangeColor(GameConstants.STATE_CLICK_COLOR);
                    if (ConnectionManager.Instance.connectionMode) ConnectionManager.Instance.CreateConnection(Name);
                }
                */
            }
            else if (!hover) {
                if (StatePolygon[0].Color == GameConstants.STATE_CLICK_COLOR) {
                    ChangeColor(controllerColor);
                }
            }
        }
        else if (Input.IsActionJustPressed("Right_Mouse_Click")) {
            if (!ConnectionManager.Instance.connectionMode) return;
            Color controllerColor = NationManager.Nations_Tag[ControllerTag].NationalColor;
            if (hover) { //This state is clicked
                if (StatePolygon[0].Color == GameConstants.STATE_LOCK_COLOR) {
                    ConnectionManager.Instance.UnlockState();
                    ChangeColor(controllerColor);
                }
                else if (StatePolygon[0].Color == controllerColor || StatePolygon[0].Color == GameConstants.STATE_CLICK_COLOR) {
                    ConnectionManager.Instance.LockState(this);
                    ChangeColor(GameConstants.STATE_LOCK_COLOR);
                }
            }
            else if (!hover) { //This state was not clicked
                if (StatePolygon[0].Color == GameConstants.STATE_LOCK_COLOR || StatePolygon[0].Color == GameConstants.STATE_CLICK_COLOR) {
                    ChangeColor(controllerColor);
                }
            }
        }
    }
    //====================================================================
    
    public void DebugSetID (int ID) => this.ID = ID;
    public void DebugSetOwner (string tag) => OwnerTag = tag;
    public void DebugSetController (string tag) => ControllerTag = tag;
    public void ChangeName (string newName) => StateName = newName;
    public void ChangeOwner(Nation newNation) {
        Nation oldNation = NationManager.Nations_Tag[OwnerTag];

        OwnerTag = newNation.Tag;
        ControllerTag = newNation.Tag;
        ChangeColor(newNation.NationalColor);

        oldNation.LoseState(this);
        oldNation.LoseControlState(this);
        newNation.GainState(this);
        newNation.GainControlState(this);
    }
    public void ChangeController (Nation newNation) {
        Nation oldNation = NationManager.Nations_Tag[ControllerTag];

        ControllerTag = newNation.Tag;
        ChangeColor(newNation.NationalColor);

        oldNation.LoseControlState(this);
        newNation.GainControlState(this);
    }
    public void ChangeColor(Color color) {
        foreach (Polygon2D polygon in StatePolygon)
            polygon.Color = color;
    }
    public void PopGrowth () {
        //Population = (int)Math.Floor(Population * 1.1f);
    }
}