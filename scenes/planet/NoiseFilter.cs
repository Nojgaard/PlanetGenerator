using Godot;
using System;

[Tool]
public class NoiseFilter : Resource
{
    private OpenSimplexNoise noise = new OpenSimplexNoise();

    [Export]
    int Seed
    {
        get { return noise.Seed; }
        set { noise.Seed = value; EmitSignal("changed");  }
    }

    [Export(PropertyHint.Range, "1, 9")]
    int Octaves
    {
        get { return noise.Octaves; }
        set { noise.Octaves = value; EmitSignal("changed"); }
    }

    [Export(PropertyHint.Range, "0, 10, or_greater")]
    float Lacunarity
    {
        get { return noise.Lacunarity; }
        set { noise.Lacunarity = value; EmitSignal("changed"); }
    }

    [Export(PropertyHint.Range, "0, 5, or_greater")]
    float Period
    {
        get { return noise.Period; }
        set { noise.Period = value; EmitSignal("changed"); }
    }

    [Export(PropertyHint.Range, "0, 10, or_greater")]
    float Persistence
    {
        get { return noise.Persistence; }
        set { noise.Persistence = value; EmitSignal("changed"); }
    }

    bool isEnabled = true;
    [Export]
    public bool IsEnabled
    {
        get { return isEnabled; }
        set { isEnabled = value; EmitSignal("changed"); }
    }

    bool useMask = false;
    [Export]
    public bool UseMask
    {
        get { return useMask; }
        set { useMask = value; EmitSignal("changed"); }
    }

    float strength = 1;
    [Export]
    public float Strength
    {
        get { return strength; }
        set { strength = value; EmitSignal("changed"); }
    }

    float subtract = 0;
    [Export(PropertyHint.Range, "0, 2")]
    public float Subtract
    {
        get { return subtract; }
        set { subtract = value; EmitSignal("changed"); }
    }

    public float Evaluate(Vector3 point)
    {
        // noise returns a value from [-1, 1] so we transform to [0,1]
        var val = (noise.GetNoise3dv(point) + 1) * .5f;
        return Math.Max(0, val - subtract) * strength;
    }
}
