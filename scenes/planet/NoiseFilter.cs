using Godot;
using System;

[Tool]
public class NoiseFilter : Resource
{
    private OpenSimplexNoise noise = new OpenSimplexNoise();
    private OpenSimplexNoise baseNoise = new OpenSimplexNoise();

    public NoiseFilter()
    {
        baseNoise.Period = 1;
        baseNoise.Octaves = 1;
    }

    [Export]
    int Seed
    {
        get { return noise.Seed; }
        set { noise.Seed = value; EmitSignal("changed"); }
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

    [Export(PropertyHint.Range, "0, 1, or_greater")]
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

    Vector3 center = new Vector3(0, 0, 0);
    [Export]
    public Vector3 Center
    {
        get { return center; }
        set { center = value; EmitSignal("changed"); }
    }

    public enum NoiseType { Open, Centered, Ridged };
    NoiseType type = NoiseType.Open;

    [Export]
    public NoiseType Type
    {
        get { return type; }
        set { type = value; EmitSignal("changed"); }
    }

    public float Evaluate(Vector3 point)
    {
        if (type == NoiseType.Open)
        {
            return EvaluateOpen(point);
        } else if (type == NoiseType.Centered)
        {
            return EvaluateCentered(point);
        } else if (type == NoiseType.Ridged)
        {
            return EvaluateRidged(point);
        }
        throw new Exception("Noise Type Not Found");
    }

    public float EvaluateOpen(Vector3 point)
    {
        var val = (noise.GetNoise3dv(point + center) + 1) * .5f;
        //return Math.Max(0, val - subtract) * strength;
        return (val - subtract) * strength;

    }

    float ridgedLayerStrength = 1f;
    [Export(PropertyHint.Range)]
    public float RidgedLayerStrength
    {
        get { return ridgedLayerStrength; }
        set { ridgedLayerStrength = value; EmitSignal("changed"); }
    }

    public float EvaluateCentered(Vector3 point)
    {
        var roughness = Lacunarity;
        var baseRoughness = Period;
        var persistence = Persistence;
        var numLayers = noise.Octaves;
        var frequency = baseRoughness;
        var amplitude = 1f;
        float noiseValue = 0;
        baseNoise.Seed = Seed;
        for (int i = 0; i < numLayers; ++i)
        {
            var localNoise = baseNoise.GetNoise3dv((point * frequency) + center);
            noiseValue += (localNoise + 1) * .5f * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }
        //float noiseValue = (baseNoise.GetNoise3dv((point * roughness) + center) + 1) * .5f;
        //return Math.Max(0, noiseValue - subtract) * strength;
        return (noiseValue - subtract) * strength;
    }

    public float EvaluateRidged(Vector3 point)
    {
        var roughness = Lacunarity;
        var baseRoughness = Period;
        var persistence = Persistence;
        var numLayers = noise.Octaves;
        var frequency = baseRoughness;
        var amplitude = 1f;
        float noiseValue = 0;
        baseNoise.Seed = Seed;
        float weight = 1f;
        for (int i = 0; i < numLayers; ++i)
        {
            var localNoise = 1 - Math.Abs(baseNoise.GetNoise3dv((point * frequency) + center));
            localNoise *= localNoise;
            localNoise *= weight;
            weight = Mathf.Clamp(localNoise * ridgedLayerStrength, 0, 1);
            noiseValue += localNoise * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }
        //float noiseValue = (baseNoise.GetNoise3dv((point * roughness) + center) + 1) * .5f;
        //return Math.Max(0, noiseValue - subtract) * strength;
        return (noiseValue - subtract) * strength;

    }
}
