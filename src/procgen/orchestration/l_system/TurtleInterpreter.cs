using Godot;
using System.Collections.Generic;
using System;

public class TurtleInterpreter
{
    const float roadLength = 10f;
    const float houseSpacing = 5f;
    const float rotationAngle = 30f;

    TurtleState state;
    Stack<TurtleState> stack = new();
    Func<Vector3, float> getHeight;
    readonly int worldSeed;
    readonly bool noisy;

    public TurtleInterpreter(Func<Vector3,float> heightSampler,
                             int seed = 0, bool useHeadingNoise = true)
    {
        getHeight = heightSampler;
        worldSeed = seed;
        noisy     = useHeadingNoise;
    }

    public void Interpret(string sequence, Vector3 startPosition, Vector3 startDirection, List<Vector3> housePositions)
    {
        state = new TurtleState(startPosition, startDirection);

        foreach (char symbol in sequence)
        {
            switch (symbol)
            {
                case 'A':
                    housePositions.Add(state.Position);
                    state.Position += state.Direction * houseSpacing;
                    break;

                case 'B':
                    stack.Push(state.Clone());
                    state.Direction = state.Direction.Rotated(Vector3.Up, Mathf.DegToRad(rotationAngle));
                    break;

                case '[':
                    stack.Push(state.Clone());
                    break;

                case ']':
                    if (stack.Count > 0)
                    {
                        state = stack.Pop();
                        state.Direction = state.Direction.Rotated(Vector3.Up, Mathf.DegToRad(-rotationAngle));
                    }
                    break;

                case 'C':
                    housePositions.Add(state.Position + state.Direction.Cross(Vector3.Up) * houseSpacing);
                    housePositions.Add(state.Position - state.Direction.Cross(Vector3.Up) * houseSpacing);
                    state.Position += state.Direction * houseSpacing;
                    break;

                case 'D':
                    int sub = Mathf.CeilToInt(roadLength);  // 1-unit steps
                    for (int i = 0; i < sub; ++i)
                    {
                        if (noisy)
                            state.Direction = HeadingNoise.PerturbDirection(
                                                state.Direction, state.Position,
                                                worldSeed);

                        state.Position += state.Direction;
                    }
                    break;
            }
        }
    }
}
