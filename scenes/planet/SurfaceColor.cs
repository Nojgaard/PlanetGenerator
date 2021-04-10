using Godot;
using System;

[Tool]
public class SurfaceColor : Resource
{
    NoiseFilter noise = (NoiseFilter)GD.Load("res://scenes/planet/NoiseFilter.cs").Call("new");

    [Export]
    NoiseFilter Noise
    {
        get { return noise; }
        set { noise = value; noise.Connect("changed", this, "EmitChanged"); }
    }

    float noiseOffset = 0;
    [Export]
    float NoiseOffset
    {
        get { return noiseOffset; }
        set { noiseOffset = value; EmitChanged(); }
    }

    float noiseStrength = 1;
    [Export]
    float NoiseStrength
    {
        get { return noiseStrength; }
        set { noiseStrength = value; EmitChanged(); }
    }

    float blendAmount = 0;
    [Export(PropertyHint.Range, "0,1")]
    float BlendAmount
    {
        get { return blendAmount; }
        set { blendAmount = value; EmitChanged(); }
    }

    BiomeColor[] biomeColors;
     [Export]
     BiomeColor[] BiomeColors
     {
         get { return biomeColors; }
         set {
            biomeColors = value; 
             for (int i = 0; i < biomeColors.Length; ++i)
             {
                 if (biomeColors[i] == null)
                 {
                    biomeColors[i] = (BiomeColor)GD.Load("res://scenes/planet/BiomeColor.cs").Call("new");
                    biomeColors[i].Connect("changed", this, "EmitChanged");
                 }
             }
            EmitChanged();
         }
     }

    ImageTexture biomeTexture = new ImageTexture();
    int biomeTextureResolution = 100;

    GradientTexture oceanTexture = new GradientTexture();
    [Export]
    GradientTexture OceanTexture
    {
        get { return oceanTexture; }
        set { 
            oceanTexture = value;
            oceanTexture.Connect("changed", this, "EmitChanged");
            EmitChanged(); 
        }
    }

    public void Init()
    {
        foreach (var biome in biomeColors)
        {
            biome.Connect("changed", this, "EmitChanged");
        }
    }

    public float PointToBiomePercent(Vector3 pointOnUnitSphere)
    {
        float heightPercent = (pointOnUnitSphere.y + 1) / 2f;
        heightPercent += (noise.Evaluate(pointOnUnitSphere) - noiseOffset) * noiseStrength;
        float biomeIndex = 0;
        float blendRange = blendAmount / 2f + .0001f;
        for (int i = 0; i < biomeColors.Length; ++i)
        {
            var biome = biomeColors[i];
            float dst = heightPercent - biome.StartHeight;

            float weight = Mathf.Clamp(Mathf.InverseLerp(-blendRange, blendRange, dst), 0, 1);
            
            
            biomeIndex *= (1 - weight);
            biomeIndex += i * weight;
        }
        return biomeIndex / Mathf.Max(1f, biomeColors.Length - 1);
    }

    public void UpdateShader(ShaderMaterial mat, float minElevation, float maxElevation)
    {
        biomeTextureResolution = 50;
        Image img = new Image();
        img.Create(biomeTextureResolution, biomeColors.Length, false, Image.Format.Rgba8);
        img.Lock();
        for (int j = 0; j < biomeColors.Length; ++j)
        {
            var biome = biomeColors[j];
            for (int i = 0; i < biomeTextureResolution; i++)
            {
                float ofs = (float)i / (biomeTextureResolution - 1f);
                Color col = biome.Interpolate(ofs);
                col = (1f - biome.TintPercent) * col + biome.TintPercent * biome.Tint;
                img.SetPixel(i, j, col);
            }
        }
        img.Unlock();
        uint use_anim = (uint)ImageTexture.FlagsEnum.AnisotropicFilter;
        uint use_linear = (uint)ImageTexture.FlagsEnum.ConvertToLinear;
        uint use_filter = (uint)ImageTexture.FlagsEnum.Filter;
        uint use_repeat = (uint)ImageTexture.FlagsEnum.Repeat;
        //biomeTexture.CreateFromImage(img, use_anim + use_linear + use_filter);
        biomeTexture.CreateFromImage(img, use_linear + use_filter);
        mat.SetShaderParam("min_elevation", minElevation);
        mat.SetShaderParam("max_elevation", maxElevation);
        mat.SetShaderParam("surface_color_texture", biomeTexture);
        mat.SetShaderParam("ocean_texture", oceanTexture);

    }

    private void EmitChanged()
    {
        EmitSignal("changed");
    }

}
