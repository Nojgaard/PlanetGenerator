using Godot;
using System;

[Tool]
public class BiomeColor : Gradient
{
    
    Color tint;
    [Export]
    public Color Tint
    {
        get { return tint; }
        set { tint = value; EmitChanged(); }
    }

    float tintPercent;
    [Export(PropertyHint.Range, "0,1")]
    public float TintPercent
    {
        get { return tintPercent; }
        set { tintPercent = value; EmitChanged(); }
    }

    float startHeight;
    [Export(PropertyHint.Range, "0,1")]
    public float StartHeight
    {
        get { return startHeight; }
        set { startHeight = value; EmitChanged(); }
    }

    private void EmitChanged()
    {
        EmitSignal("changed");
    }

}
