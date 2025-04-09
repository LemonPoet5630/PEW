using Godot;

public partial class NodeManager : Node
{

    public static NodeManager Instance;

    [Export] public Node nationsNode;
    [Export] public Node statesNode;
    [Export] public Node2D borderLineNode;
    [Export] public Node2D armyTextNode;
    [Export] public Node2D statePolygonNode;
    public override void _Ready()
    {
        base._Ready();

        Instance = this;

    }

    public void NodeVisibilityControl (Control targetNode, bool toggle) {
        if (targetNode == null) {
            GD.Print("TARGET NODE IS NULL!");
            return;
        }
        targetNode.Visible = toggle;
    }

}
