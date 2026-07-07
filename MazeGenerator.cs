namespace MazeDroneSim;

public class MazeGenerator
{
    private readonly Random _random = new();
    public Dictionary<Coordinate, Room> Rooms { get; private set; } = new();
    public Coordinate TreasurePosition { get; private set; } = default!;
    public Coordinate DroneStartPosition { get; private set; } = default!;

    public void Generate3x3x3Maze()
    {
        Rooms.Clear();

        // 1. Initialize the raw cube shell rooms
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    var coord = new Coordinate(x, y, z);
                    Rooms[coord] = new Room { Position = coord };
                }
            }
        }

        // 2. Link rooms together naturally with adjacent doors
        // 0=North(+Y), 1=East(+X), 2=Up(+Z), 3=South(-Y), 4=West(-X), 5=Down(-Z)
        foreach (var room in Rooms.Values)
        {
            var pos = room.Position;

            // Look North (+Y)
            if (pos.Y < 2 && _random.Next(0, 2) == 1) ConnectRooms(pos, new Coordinate(pos.X, pos.Y + 1, pos.Z), 0, 3);
            // Look East (+X)
            if (pos.X < 2 && _random.Next(0, 2) == 1) ConnectRooms(pos, new Coordinate(pos.X + 1, pos.Y, pos.Z), 1, 4);
            // Look Up (+Z)
            if (pos.Z < 2 && _random.Next(0, 2) == 1) ConnectRooms(pos, new Coordinate(pos.X, pos.Y, pos.Z + 1), 2, 5);
        }

        // 3. Guarantee Interview Rule: No isolated rooms! Every room must have at least one door.
        foreach (var room in Rooms.Values)
        {
            if (!room.Doors.Contains(true))
            {
                CarveForcedDoor(room);
            }
        }

        // 4. Set the Secret Treasure Room location
        var coordinatesList = Rooms.Keys.ToList();
        TreasurePosition = coordinatesList[_random.Next(coordinatesList.Count)];
        Rooms[TreasurePosition].IsTreasureRoom = true;

        // 5. Select a completely different starting location for the drone
        do
        {
            DroneStartPosition = coordinatesList[_random.Next(coordinatesList.Count)];
        } while (DroneStartPosition == TreasurePosition);
    }

    private void ConnectRooms(Coordinate current, Coordinate target, int dir, int oppositeDir)
    {
        if (Rooms.ContainsKey(target))
        {
            Rooms[current].Doors[dir] = true;
            Rooms[target].Doors[oppositeDir] = true;
        }
    }

    private void CarveForcedDoor(Room room)
    {
        var pos = room.Position;
        // Try to connect to any valid adjacent grid cell
        if (pos.Y < 2) ConnectRooms(pos, new Coordinate(pos.X, pos.Y + 1, pos.Z), 0, 3);
        else if (pos.X < 2) ConnectRooms(pos, new Coordinate(pos.X + 1, pos.Y, pos.Z), 1, 4);
        else if (pos.Z < 2) ConnectRooms(pos, new Coordinate(pos.X, pos.Y, pos.Z + 1), 2, 5);
        else if (pos.Y > 0) ConnectRooms(pos, new Coordinate(pos.X, pos.Y - 1, pos.Z), 3, 0);
        else if (pos.X > 0) ConnectRooms(pos, new Coordinate(pos.X - 1, pos.Y, pos.Z), 4, 1);
        else if (pos.Z > 0) ConnectRooms(pos, new Coordinate(pos.X, pos.Y, pos.Z - 1), 5, 2);
    }
}
