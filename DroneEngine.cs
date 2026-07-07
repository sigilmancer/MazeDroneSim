namespace MazeDroneSim;

public class DroneEngine
{
    // Direction constants from the interview sheet
    public const int North = 0;
    public const int East = 1;
    public const int Up = 2;
    public const int South = 3;
    public const int West = 4;
    public const int Down = 5;

    private readonly MazeGenerator _maze;
    
    // The drone's relative mapping tracking memory
    public Coordinate CurrentRelativePosition { get; private set; } = new Coordinate(0, 0, 0);
    
    // Track visited paths using our relative tuple map keys
    public HashSet<(int X, int Y, int Z)> VisitedMemory { get; } = new();

    // Actual underlying tracker used to control the physical sandbox layout loop
    public Coordinate ActualPhysicalPosition { get; private set; }

    // Action loop callback to refresh the Blazor UI smoothly during steps
    private readonly Func<Task> _onStateChanged;

    public bool IsComplete { get; private set; } = false;

    public DroneEngine(MazeGenerator maze, Func<Task> onStateChanged)
    {
        _maze = maze;
        _onStateChanged = onStateChanged;
        ActualPhysicalPosition = _maze.DroneStartPosition;
    }

    // --- Interview Sheet Driver Implementations ---

    public bool IsTreasureRoom()
    {
        return _maze.Rooms[ActualPhysicalPosition].IsTreasureRoom;
    }

    public bool IsDoorway(int direction)
    {
        return _maze.Rooms[ActualPhysicalPosition].Doors[direction];
    }

    public void Move(int direction)
    {
        // 1. Calculate next target node coordinate in the physical space
        ActualPhysicalPosition = GetNextPhysicalCoordinate(direction);

        // 2. Track our internal relative position mapping
        CurrentRelativePosition = direction switch
        {
            North => CurrentRelativePosition with { Y = CurrentRelativePosition.Y + 1 },
            East  => CurrentRelativePosition with { X = CurrentRelativePosition.X + 1 },
            Up    => CurrentRelativePosition with { Z = CurrentRelativePosition.Z + 1 },
            South => CurrentRelativePosition with { Y = CurrentRelativePosition.Y - 1 },
            West  => CurrentRelativePosition with { X = CurrentRelativePosition.X - 1 },
            Down  => CurrentRelativePosition with { Z = CurrentRelativePosition.Z - 1 },
            _ => throw new ArgumentException("Unknown direction")
        };
    }

    // --- The Core Search Algorithm Execution Loop ---

    public async Task FindTreasure()
    {
        IsComplete = false;
        VisitedMemory.Clear();
        CurrentRelativePosition = new Coordinate(0, 0, 0);
        ActualPhysicalPosition = _maze.DroneStartPosition;

        await ExploreRoomDFS();
        IsComplete = true;
        await _onStateChanged();
    }

    private async Task<bool> ExploreRoomDFS()
    {
        // Give the UI thread a moment to breathe so we can watch the animation happen!
        await Task.Delay(300);
        await _onStateChanged();

        if (IsTreasureRoom()) return true;

        var relativeKey = (CurrentRelativePosition.X, CurrentRelativePosition.Y, CurrentRelativePosition.Z);
        VisitedMemory.Add(relativeKey);

        for (int dir = 0; dir <= 5; dir++)
        {
            if (IsDoorway(dir))
            {
                var targetRelative = GetNextRelativeCoordinate(dir);

                if (!VisitedMemory.Contains(targetRelative))
                {
                    // Move forward step
                    Move(dir);

                    // Recursively explore deeper down the branch path
                    if (await ExploreRoomDFS()) return true;

                    // Backtrack step: Undo layout position modifications if it was a dead end
                    await Task.Delay(200);
                    Move(GetOppositeDirection(dir));
                    await _onStateChanged();
                }
            }
        }

        return false;
    }

    // --- Core Coordinate Translation Calculations ---

    private Coordinate GetNextPhysicalCoordinate(int direction) => direction switch
    {
        North => ActualPhysicalPosition with { Y = ActualPhysicalPosition.Y + 1 },
        East  => ActualPhysicalPosition with { X = ActualPhysicalPosition.X + 1 },
        Up    => ActualPhysicalPosition with { Z = ActualPhysicalPosition.Z + 1 },
        South => ActualPhysicalPosition with { Y = ActualPhysicalPosition.Y - 1 },
        West  => ActualPhysicalPosition with { X = ActualPhysicalPosition.X - 1 },
        Down  => ActualPhysicalPosition with { Z = ActualPhysicalPosition.Z - 1 },
        _ => throw new ArgumentException()
    };

    private (int, int, int) GetNextRelativeCoordinate(int direction) => direction switch
    {
        North => (CurrentRelativePosition.X, CurrentRelativePosition.Y + 1, CurrentRelativePosition.Z),
        East  => (CurrentRelativePosition.X + 1, CurrentRelativePosition.Y, CurrentRelativePosition.Z),
        Up    => (CurrentRelativePosition.X, CurrentRelativePosition.Y, CurrentRelativePosition.Z + 1),
        South => (CurrentRelativePosition.X, CurrentRelativePosition.Y - 1, CurrentRelativePosition.Z),
        West  => (CurrentRelativePosition.X - 1, CurrentRelativePosition.Y, CurrentRelativePosition.Z),
        Down  => (CurrentRelativePosition.X, CurrentRelativePosition.Y, CurrentRelativePosition.Z - 1),
        _ => throw new ArgumentException()
    };

    private int GetOppositeDirection(int direction) => direction switch
    {
        North => South, South => North,
        East => West, West => East,
        Up => Down, Down => Up,
        _ => throw new ArgumentException()
    };
}
