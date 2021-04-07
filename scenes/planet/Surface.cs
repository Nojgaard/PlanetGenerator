using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class Surface : MeshInstance
{
    private NoiseFilter[] noiseFilters;
    private int resolution = 10;
    private float radius = 1;
    private float minElevation, maxElevation;

    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] normals;
    Dictionary<Vector3, List<int>> seamMap;

    private float CalcElevation(Vector3 point)
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
        return elevation;
    }

    private Vector3 CalcPointOnSphere(Vector3 pointOnUnitSphere)
    {
        var elevation = (1 + CalcElevation(pointOnUnitSphere)) * radius;
        minElevation = Math.Min(minElevation, elevation);
        maxElevation = Math.Max(maxElevation, elevation);
        return pointOnUnitSphere * elevation;
    }

    private void GenerateNormals()
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 normal = new Plane(
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]
                ).Normal;
            for (int vi = 0; vi < 3; ++vi)
            {
                var index = triangles[i + vi] % (resolution * resolution);
                var x = index % resolution;
                var y = index / resolution;
                if (y == resolution | x == resolution | y == 0 | x == 0)
                {
                    List<int> seamIndices;
                    seamMap.TryGetValue(vertices[triangles[i + vi]], out seamIndices);
                    foreach (int idx in seamIndices) { normals[idx] += normal;  }
                } else
                {
                    normals[triangles[i + vi]] += normal;
                }
            }
        }
        for (int i = 0; i < triangles.Length; ++i)
        {
            normals[i] = normals[i].Normalized();
        }
    }

    private void GenerateFace(Vector3 localUp, ref int vertexOffset, ref int triangleOffset)
    {
        var axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        var axisB = localUp.Cross(axisA);
        for (int y = 0; y < resolution; ++y)
        {
            for (int x = 0; x < resolution; ++x)
            {
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                var pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                var pointOnUnitSphere = pointOnUnitCube.Normalized();
                var point = CalcPointOnSphere(pointOnUnitSphere);
                vertices[vertexOffset] = point;

                if (y == resolution |x == resolution | y == 0 | x == 0)
                {
                    List<int> seamIndices;
                    if (!seamMap.TryGetValue(point, out seamIndices))
                    {
                        seamIndices = new List<int>();
                        seamMap.Add(point, seamIndices);
                    }
                    seamIndices.Add(vertexOffset);
                }

                vertexOffset += 1;


                if (y == resolution - 1 | x == resolution - 1) { continue; }

                //var i = x + y * resolution + indexOffset;
                var i = vertexOffset-1;
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = i + resolution;
                triangles[triangleOffset + 2] = i + resolution + 1;

                triangles[triangleOffset + 3] = i;
                triangles[triangleOffset + 4] = i + 1 + resolution;
                triangles[triangleOffset + 5] = i + 1;
                triangleOffset += 6;
            }
        }
    }

    private void UpdateShader()
    {
        var shader = ResourceLoader.Load("res://scenes/planet/PlanetShader.shader") as Shader;
        ShaderMaterial mat = GetSurfaceMaterial(0) as ShaderMaterial;
        if (mat == null)
        {
            mat = new ShaderMaterial();
            mat.Shader = shader;
           
            SetSurfaceMaterial(0, mat);
        }
        mat.SetShaderParam("min_elevation", minElevation);
        mat.SetShaderParam("max_elevation", maxElevation);
        mat.SetShaderParam("surface_color_texture", GetParent().Get("color_settings"));
    }

    public void generate_mesh()
    {
        GD.Print("Im Now here Here");
        minElevation = int.MaxValue;
        maxElevation = int.MinValue;
        resolution = (int)GetParent().Get("resolution");
        radius = (float)GetParent().Get("shape_radius");
        var noiseResources = (Godot.Collections.Array)GetParent().Get("noise_filters");
        noiseFilters = new NoiseFilter[noiseResources.Count];
        noiseResources.CopyTo(noiseFilters, 0);

        var directions = new Vector3[] { 
            new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0), new Vector3(0, -1, 0),
            new Vector3(0, 0, 1), new Vector3(0, 0, -1)
        };

        vertices = new Vector3[resolution * resolution * directions.Length * directions.Length];
        normals = new Vector3[resolution * resolution * directions.Length * directions.Length];
        triangles = new int[(resolution - 1) * (resolution - 1) * 6 * directions.Length];
        seamMap = new Dictionary<Vector3, List<int>>();

        int vertexOffset = 0;
        int triangleOffset = 0;
        foreach (var dir in directions)
        {
            GenerateFace(dir, ref vertexOffset, ref triangleOffset);
        }

        GenerateNormals();

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = triangles;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals;
        ArrayMesh arrayMesh = new ArrayMesh();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        Mesh = arrayMesh;

        UpdateShader();

    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
