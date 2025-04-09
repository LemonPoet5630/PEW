using Godot;
using Godot.Collections;
using System;

public partial class MainCamera : Camera2D
{
    /*
    [Export] public float zoomSpeed = 0.1f;
    [Export] public float panSpeed = 1.0f;
    [Export] public float rotationSpeed = 1.0f;

    [Export] public bool canPan;
    [Export] public bool canZoom;
    [Export] public bool canRotate;

    Dictionary touchPoints = [];
    float startDistance;
    float startZoom;
    float startAngle;
    float currentAngle;


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch) {
            handleTouch((InputEventScreenTouch)@event);
        }
        else if (@event is InputEventScreenDrag) {
            handleDrag((InputEventScreenDrag)@event);
        }
    }

    public void handleTouch (InputEventScreenTouch @event) {
        if (@event.IsPressed()) {
            touchPoints[@event.Index] = @event.Position;
        }
        else {
            touchPoints.Remove(@event.Index);
        }

        if (touchPoints.Count == 2) {
            var touchPointPositions = touchPoints.Values;
            //startDistance = touchPointPositions[0];
        }
    }

    public void handleDrag (InputEventScreenDrag @event) {
        touchPoints[@event.Index] = @event.Position;
        if (touchPoints.Count == 1) {
            if (canPan) {
                Offset -= @event.Relative * panSpeed;
            }
        }
    }
    */
    
    [Export] public float defaultZoomValue;
    [Export] public float zoomMinValue;
    [Export] public float zoomMaxValue;
    [Export(PropertyHint.Range, "0.01, 0.1, 0.01")] public float zoomSpeedValue;
    [Export] public int panSpeedValue;

    [Export] public RichTextLabel testText;

    private Vector2 zoomMin;
    private Vector2 zoomMax;
    private Vector2 zoomSpeed;
    private float panSpeed;

    public override void _Ready()
    {
        base._Ready();

        zoomMin = new Vector2(zoomMinValue, zoomMinValue);
        zoomMax = new Vector2(zoomMaxValue, zoomMaxValue);
        zoomSpeed = new Vector2(zoomSpeedValue, zoomSpeedValue);
        panSpeed = panSpeedValue;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (Input.IsActionJustPressed("Scroll_Out")) {
            if (Zoom > zoomMin) {
                Zoom -= zoomSpeed;
            }
        }
        if (Input.IsActionJustPressed("Scroll_In")) {
            if (Zoom < zoomMax) {
                Zoom += zoomSpeed;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        float tempPanSpeed = panSpeed * (defaultZoomValue / Zoom.X);
        
        if (Input.IsActionPressed("Up"))
            Position -= new Vector2(0, tempPanSpeed);
        if (Input.IsActionPressed("Down"))
            Position += new Vector2(0, tempPanSpeed);
        if (Input.IsActionPressed("Right"))
            Position += new Vector2(tempPanSpeed, 0);
        if (Input.IsActionPressed("Left"))
            Position -= new Vector2(tempPanSpeed, 0);
    }
    
}
