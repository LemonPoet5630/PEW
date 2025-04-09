using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    [Export] public int PlayerID { get; private set; }

    public Nation PlayerNation { get; private set; }

    public static int turn = 1;
    public static int ArmyID = 0;
    public static int CorpsID = 0;
    [Export(PropertyHint.Enum, "WW1:1900,WW2:1930")] public int startYear;
    [Export] public bool safeLaunch = false;
    [Export] public bool editScenario = false;
    public static int[] currentDate;

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        currentDate = GameConstants.START_DATE;
    }

    public void ChangePlayer(string newPlayerTag) {
        NationManager.Nations_Tag[newPlayerTag].ChangePlayerStatus(true);
        PlayerNation.ChangePlayerStatus(false);
        PlayerNation = NationManager.Nations_Tag[newPlayerTag];
    }

    public static void ProgressTime() {
        int maxDay = default;
        switch (currentDate[1])
        {
            case 1 or 3 or 5 or 7 or 8 or 10 or 12: //Has 31 days
                maxDay = 31;
                break;
            case 2: //Has 28 or 29 days
                if (currentDate[0] % 4 == 0) maxDay = 29; //Leap year
                else maxDay = 28;
                break;
            case 4 or 6 or 9 or 11: //Has 30 days
                maxDay = 30;
                break;
        }
        if (currentDate[2] == maxDay)
        {
            if (currentDate[1] == 12)
            {
                currentDate[0]++;
                currentDate[1] = 1;
            }
            else currentDate[1]++;
            currentDate[2] = 1;
        }
        else currentDate[2]++;
    }
}