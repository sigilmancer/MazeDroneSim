namespace MazeDroneSim;


public record Coordinate(int X, int Y, int Z);

public class Room
{
    public Coordinate Position { get; set; } = null!;
    public bool IsTreasureRoom { get; set; } = false;
    
    // Tracks if a doorway exists on each of the 6 faces
    // Index mapping matches the prompt: 0=N, 1=E, 2=Up, 3=S, 4=W, 5=Down
    public bool[] Doors { get; set; } = new bool[6];
}
