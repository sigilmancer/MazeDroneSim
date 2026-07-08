# The 3D Maze Drone: Algorithmic Exploration

A technical diary entry logging a dual-language (C# .NET) solution to a classic 3-dimensional graph traversal challenge. 

## The Core Algorithmic Engineering Problem
The challenge involves programming an autonomous drone dropped into a random entry room within an enclosed, finite 3D maze of identical cube-shaped rooms. The maze size is completely unknown. The drone possesses ample internal memory, can query for paths using 6-directional sensor gates (`North`, `East`, `Up`, `South`, `West`, `Down`), and must reliably discover a single special room containing treasure. 

A critical architectural restraint is that **the drone has no tracking connection to the external matrix grid coordinates.** It does not know where it is in the world space. 

## Architectural Strategy: Dual-Coordinate Mapping
To decouple the simulation engine from the drone's logic, this architecture tracks two distinct positional states:

1.  **Actual Physical Position:** Used exclusively by the sandbox layout engine to track the true matrix coordinate inside the generated environment block.
2.  **Current Relative Position:** The drone's self-contained relative coordinate layout memory. It instantiates at `(0, 0, 0)`. Every physical movement translates a relative shifting offset inside a localised tuple `HashSet` tracker.
3.  **Blazor WebAssembly UI:** The engine is coupled to a non-blocking interactive web interface using multi-layered CSS grids to visually render the three separate 3D matrix floors (Z = 0, 1, 2) simultaneously.

                  ┌──────────────────────────────┐
                  │   3D Matrix Environment      │
                  │  (True Physical Coordinates) │
                  └──────────────┬───────────────┘
                                 │
                     [Move(dir) Sensor Event]
                                 │
                                 ▼
                  ┌──────────────────────────────┐
                  │    Drone Relative Memory     │
                  │ (Local (X,Y,Z) Graph Ledger) │
                  └──────────────────────────────┘

## Recursive Backtracking Implementation
The navigation layout relies on an asynchronous **Depth-First Search (DFS)** graph engine. 

*   **Loop Phase:** The system queries for active doors, validates if the relative offset path has been logged in the `VisitedMemory` collection, and commits a execution step.
*   **Backtrack Phase:** If a specific branch path returns a dead end, the recursive call frame pops, calculating the explicit bitwise opposite index direction (`GetOppositeDirection`) to safely rewind the system positioning without data drift.
*   **Asynchronous Execution:** Built intentionally using non-blocking asynchronous state loops (`Task.Delay`), allowing modern UI frameworks to visually track the state traversal step-by-step.

## Engineering Reflection & Language Nuances
This entry serves as a comparative study on object mapping structure styles:
*   The **C# Engine** utilises strongly-typed modern `record` structs with value-equality features optimised for predictable enterprise runtimes:
```csharp
public record Coordinate(int X, int Y, int Z);
```

## 🎮 Live Simulation Controls
* **🔄 Generate New Maze Layout**: Procedurally builds a perfect 3x3x3 grid ensuring no isolated sectors exist.
* **⚡ Launch Autonomous Drone Pathfinding**: Triggers the async DFS solver with a localised 300ms execution pacing window so human eyes can watch the drone (`🛸`) search and backtrack.

