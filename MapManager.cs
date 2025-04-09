using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapManager: Node2D
{
    public static MapManager Instance;
    [Export] public Node StateParentNode;
    [Export] public Node2D EditScenarioNode;
    [Export] public Node NationButtonContainerNode;

    [Export] public Node2D ConnectionLinesParent;
    [Export] public PackedScene ConnectionLinePrefab;
    [Export] public Node2D StateBordersParent;
    [Export] public Node2D NationBordersParent;
    private string currentlyShownNationTag;
    [Export] public Node2D StateNamesParent;
    [Export] public PackedScene StateNamePrefab;
    

    public int setupScenario = 0;
    public string chosenNationTag;
    private const float SNAP_EPSILON = 0.001f; // Optional: tiny snap during input

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        currentlyShownNationTag = "";

        Tools.StartTimeCount();
        foreach (string nationTag in NationManager.Nations_Tag.Keys) {
            if (NationManager.Nations_Tag[nationTag].ControlledStates.Count == 0)
                continue;
            Node2D nationNode = new();
            nationNode.Name = nationTag;
            NationBordersParent.AddChild(nationNode);
            CreateNationBorderLine(nationTag);
            nationNode.Visible = false;
        }
        Tools.StopTimeCount("MapManager", "Creating Nation Borders");

        if (!GameManager.Instance.editScenario) {
            EditScenarioNode.Visible = false;
        }
        else {
            List<string> tempList = [];
            foreach (string nationTag in NationManager.Nations_Tag.Keys) {
                tempList.Add(nationTag);
            }
            tempList.Sort();
            foreach (string nationTag in tempList) {
                Button nationButton = new();
                nationButton.Name = nationTag;
                nationButton.CustomMinimumSize = new Vector2(0, 30);
                nationButton.Text = nationTag;
                nationButton.Pressed += () => ChangeNation(nationTag);
                NationButtonContainerNode.AddChild(nationButton);
            }
        }

    }

    //Edit Scenario====================================================
    public void ScenarioChosen (int index) {
        setupScenario = index;
    }
    public void SaveSetup () {
        //StateManager.SaveSetup();
    }
    public void ChangeNation(string nationTag) {
        chosenNationTag = nationTag;
        GD.Print(chosenNationTag);
    }
    //=================================================================
    public void ToggleConnections()
    {
        ConnectionLinesParent.Visible = !ConnectionLinesParent.Visible;
    }

    public void CreateConnectionLine(string state1Name, string state2Name)
    {
        Vector2 coor1 = Tools.FindCentroid(StateManager.States_Name[state1Name]);
        Vector2 coor2 = Tools.FindCentroid(StateManager.States_Name[state2Name]);
        var line = ConnectionLinePrefab.Instantiate();
        line.Name = state1Name + "-" + state2Name + "," + state2Name + "-" + state1Name;
        Line2D lineProperty = (Line2D)line;
        lineProperty.AddPoint(coor1);
        lineProperty.AddPoint(coor2);
        ConnectionLinesParent.AddChild(line);
    }

    public void DeleteConnectionLine(string state1Name)
    {
        foreach (Line2D line in ConnectionLinesParent.GetChildren())
        {
            if (line.Name.ToString().Contains(state1Name))
            {
                line.QueueFree();
            }
        }
    }

    public void ShowNationBorders (string nationTag) {
        if (currentlyShownNationTag != "") {
            NationBordersParent.GetNode<Node2D>(currentlyShownNationTag).Visible = false;
        }
        NationBordersParent.GetNode<Node2D>(nationTag).Visible = true;
        currentlyShownNationTag = nationTag;
    }

    public static void CreateStateBorderLine(string stateName) {
        State state = StateManager.States_Name[stateName];
        List<List<Vector2>> polygons = [];

        foreach (Polygon2D polygon in state.StatePolygon){
            polygons.Add([.. polygon.Polygon]);
        }
        foreach (List<Vector2> path in polygons)
        {
            Line2D line = new Line2D();
            line.Points = [.. path];
            line.Closed = true;
            line.Width = GameConstants.STATE_BORDER_WIDTH;
            line.DefaultColor = new Color(0.1f, 0.1f, 0.1f, 0.1f);
            NodeManager.Instance.borderLineNode.AddChild(line);
        }

    }
    public void CreateNationBorderLine(string nationTag)
    {
        List<List<Vector2>> polygons = [];
        foreach (State state in NationManager.Nations_Tag[nationTag].ControlledStates) {
            //CreateStateBorderLine(state.Name);
            foreach (Polygon2D polygon in state.StatePolygon) {
                polygons.Add([.. polygon.Polygon]);
            }
        }

        List<List<Vector2>> outlines = FindNationOutline(polygons);

        foreach (List<Vector2> path in outlines)
        {
            Line2D line = new Line2D();
            line.Points = [.. path];
            line.Closed = true;
            line.Width = GameConstants.STATE_BORDER_WIDTH;
            line.DefaultColor = GameConstants.STATE_BORDER_COLOR;
            NationBordersParent.GetNode<Node2D>(nationTag).AddChild(line);
        }
    }

    private struct CanonicalEdge
    {
        public Vector2 A;
        public Vector2 B;

        public CanonicalEdge(Vector2 a, Vector2 b)
        {
            if (a < b)
            {
                A = a;
                B = b;
            }
            else
            {
                A = b;
                B = a;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is CanonicalEdge other && A == other.A && B == other.B;
        }

        public override int GetHashCode()
        {
            return 1;
        }
    }
    private static Vector2 Snap(Vector2 v)
    {
        return new Vector2(
            Mathf.Round(v.X / SNAP_EPSILON) * SNAP_EPSILON,
            Mathf.Round(v.Y / SNAP_EPSILON) * SNAP_EPSILON
        );
    }
    private static List<List<Vector2>> FindNationOutline(List<List<Vector2>> polygons)
    {
        Dictionary<CanonicalEdge, int> edgeCounts = new Dictionary<CanonicalEdge, int>();

        foreach (List<Vector2> polygon in polygons)
        {
            int count = polygon.Count;
            for (int i = 0; i < count; i++)
            {
                Vector2 start = Snap(polygon[i]);
                Vector2 end = Snap(polygon[(i + 1) % count]);

                CanonicalEdge edge = new CanonicalEdge(start, end);

                if (edgeCounts.ContainsKey(edge))
                    edgeCounts[edge]++;
                else
                    edgeCounts[edge] = 1;
            }
        }

        List<CanonicalEdge> boundaryEdges = edgeCounts
            .Where(kv => kv.Value == 1) // Only keep non-shared edges
            .Select(kv => kv.Key)
            .ToList();

        return StitchEdgesIntoPaths(boundaryEdges);
    }
    private static List<List<Vector2>> StitchEdgesIntoPaths(List<CanonicalEdge> edges)
    {
        Dictionary<Vector2, List<Vector2>> adjacency = new Dictionary<Vector2, List<Vector2>>();

        foreach (var edge in edges)
        {
            if (!adjacency.ContainsKey(edge.A))
                adjacency[edge.A] = new List<Vector2>();
            if (!adjacency.ContainsKey(edge.B))
                adjacency[edge.B] = new List<Vector2>();

            adjacency[edge.A].Add(edge.B);
            adjacency[edge.B].Add(edge.A);
        }

        List<List<Vector2>> outlines = new List<List<Vector2>>();

        while (adjacency.Count > 0)
        {
            Vector2 start = adjacency.Keys.First();
            List<Vector2> path = new List<Vector2>();
            path.Add(start);

            Vector2 current = start;
            while (true)
            {
                if (!adjacency.ContainsKey(current) || adjacency[current].Count == 0)
                    break;

                Vector2 next = adjacency[current][0];
                path.Add(next);

                // Remove current -> next
                adjacency[current].Remove(next);
                if (adjacency[current].Count == 0)
                    adjacency.Remove(current);

                // Remove next -> current
                if (adjacency.ContainsKey(next))
                {
                    adjacency[next].Remove(current);
                    if (adjacency[next].Count == 0)
                        adjacency.Remove(next);
                }

                current = next;
            }

            outlines.Add(path);
        }

        return outlines;
    }
}