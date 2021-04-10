using Godot;
using System;

[Tool]
public class SurfaceShape : Resource
{

    float radius = 1;
    [Export]
    public float Radius
    {
        get { return radius; }
        set { radius = value; EmitChanged(); }
    }

    private NoiseFilter[] noiseFilters;
    [Export]
    NoiseFilter[] NoiseFilters
    {
        get { return noiseFilters; }
        set
        {
            noiseFilters = value;
            for (int i = 0; i < noiseFilters.Length; ++i)
            {
                if (noiseFilters[i] == null)
                {
                    noiseFilters[i] = (NoiseFilter)GD.Load("res://scenes/planet/NoiseFilter.cs").Call("new");
                    noiseFilters[i].Connect("changed", this, "EmitChanged");
                }
            }
            EmitChanged();
        }
    }

    float minElevation;
    public float MinElevation { get { return minElevation; } }

    float maxElevation;
    public float MaxElevation { get { return maxElevation; } }

    private void EmitChanged()
    {
        EmitSignal("changed");
    }

    public void Init()
    {
        minElevation = int.MaxValue;
        maxElevation = int.MinValue;
    }

    public void InitConnections()
    {
        foreach (var nf in noiseFilters)
        {
            if (!nf.IsConnected("changed", this, "EmitChanged"))
            {
                nf.Connect("changed", this, "EmitChanged");
            }
        }
    }

    public float CalcUnscaledElevation(Vector3 point)
    {
        if (noiseFilters.Length == 0) { return 0; }

        float elevation = 0;
        float firstLayer = noiseFilters[0].Evaluate(point);
        if (noiseFilters[0].IsEnabled) { elevation = firstLayer; }
        for (int i = 1; i < noiseFilters.Length; ++i)
        {
            if (!noiseFilters[i].IsEnabled) { continue; }
            float mask = (noiseFilters[i].UseMask) ? firstLayer : 1;
            elevation += noiseFilters[i].Evaluate(point) * mask;
        }
        minElevation = Math.Min(minElevation, elevation);
        maxElevation = Math.Max(maxElevation, elevation);
        return elevation;
    }

    public float CalcElevation(float unscaledElevation)
    {
        var unScaledElevation = Math.Max(0, unscaledElevation);
        var elevation = (1 + unScaledElevation) * radius;
        return elevation;
    }
}
