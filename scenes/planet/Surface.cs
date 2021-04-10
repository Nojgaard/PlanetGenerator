using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class Surface : MeshInstance
{
    private int resolution = 10;
    [Export]
    public int Resolution
    {
        get { return resolution; }
        set { resolution = value; onMeshChanged(); }
    }
    private NoiseFilter[] noiseFilters;
    private float radius = 1;
    private float minElevation, maxElevation;

    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] normals;
    private Vector2[] uvs;

    public bool materialChanged = false;
    public bool meshChanged = false;

    
    [Export]
    SurfaceColor surfaceColor = (SurfaceColor)GD.Load("res://scenes/planet/SurfaceColor.cs").Call("new");

    private Vector3[] GetDirections()
    {
        return new Vector3[] {
            new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0), new Vector3(0, -1, 0),
            new Vector3(0, 0, 1), new Vector3(0, 0, -1)
        };
    }
    private float CalcUnscaledElevation(Vector3 point)
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

    private float CalcElevation(float unscaledElevation)
    {
        var unScaledElevation = Math.Max(0, unscaledElevation);
        var elevation = (1 + unScaledElevation) * radius;
        return elevation;
    }

    private void GenerateNormals(int vFrom, int vTo, int tFrom, int tTo)
    {
        for (int i = tFrom; i < tTo; i += 3)
        {
            Vector3 normal = new Plane(
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]
                ).Normal;
            for (int vi = 0; vi < 3; ++vi)
            {
                normals[triangles[i + vi]] += normal;
            }
        }
        for (int i = vFrom; i < vTo; ++i)
        {
            normals[i] = normals[i].Normalized();
        }
    }

    private void GenerateFace(Vector3 localUp, int vertexOffset, int triangleOffset)
    {
        var axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        var axisB = localUp.Cross(axisA);
        int vFrom = vertexOffset;
        int tFrom = triangleOffset;
        for (int y = 0; y < resolution; ++y)
        {
            for (int x = 0; x < resolution; ++x)
            {
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                var pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                var pointOnUnitSphere = pointOnUnitCube.Normalized();
                //var point = CalcPointOnSphere(pointOnUnitSphere);
                var unscaledElevation = CalcUnscaledElevation(pointOnUnitSphere);
                vertices[vertexOffset] = pointOnUnitSphere * CalcElevation(unscaledElevation);
                uvs[vertexOffset].y = unscaledElevation;


                vertexOffset += 1;


                if (y == resolution - 1 | x == resolution - 1) { continue; }

                //var i = x + y * resolution + indexOffset;
                var i = vertexOffset - 1;
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = i + resolution;
                triangles[triangleOffset + 2] = i + resolution + 1;
                 
                triangles[triangleOffset + 3] = i;
                triangles[triangleOffset + 4] = i + 1 + resolution;
                triangles[triangleOffset + 5] = i + 1;
                triangleOffset += 6;
            }
        }
        GenerateNormals(vFrom, vertexOffset, tFrom, triangleOffset);
    }

    private void GenerateUvsFace(Vector3 localUp, int vertexOffset)
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
                uvs[vertexOffset].x = surfaceColor.PointToBiomePercent(pointOnUnitSphere);
                vertexOffset += 1;
            }
        }
    }

    public void GenerateUvs()
    {
        var directions = GetDirections();
        /*for (int i = 0; i < directions.Length; ++i)
        {
            int vertexOffset = resolution * resolution * i;
            GenerateUvsFace(directions[i], vertexOffset);
        }*/
        System.Threading.Tasks.Parallel.For(0, directions.Length, i =>
        {
            int vertexOffset = resolution * resolution * i;
            GenerateUvsFace(directions[i], vertexOffset);
        });
    }

    private void UpdateShader()
    {
        var shader = ResourceLoader.Load("res://scenes/planet/PlanetShader.shader") as Shader;
        //ShaderMaterial mat = GetSurfaceMaterial(0) as ShaderMaterial;
        ShaderMaterial mat = MaterialOverride as ShaderMaterial;
        if (mat == null)
        {
            mat = new ShaderMaterial();
            mat.Shader = shader;

            MaterialOverride = mat;
            //SetSurfaceMaterial(0, mat);
        }
        surfaceColor.UpdateShader(mat, minElevation, maxElevation);
    }

    private ArrayMesh GetMeshArray()
    {
        return (ArrayMesh)Mesh;
    }

    public void GenerateMesh()
    {
        if (resolution == 0) { return; }
        minElevation = int.MaxValue;
        maxElevation = int.MinValue;
        ///resolution = (int)GetParent().Get("resolution");
        radius = (float)GetParent().Get("shape_radius");
        var noiseResources = (Godot.Collections.Array)GetParent().Get("noise_filters");
        noiseFilters = new NoiseFilter[noiseResources.Count];
        noiseResources.CopyTo(noiseFilters, 0);

        var directions = GetDirections();

        var numVertices = resolution * resolution;
        var numTriangles = (resolution - 1) * (resolution - 1) * 6;
        vertices = new Vector3[numVertices * directions.Length];
        normals = new Vector3[numVertices * directions.Length];
        triangles = new int[numTriangles * directions.Length];
        uvs = new Vector2[numVertices * directions.Length];
        //seamMap = new Dictionary<Vector3, List<int>>();

        System.Threading.Tasks.Parallel.For(0, directions.Length, i =>
        {
            GenerateFace(directions[i], numVertices * i, numTriangles * i);
        });
        /*for (int i = 0; i < directions.Length; ++i)
        {
            GenerateFace(directions[i], numVertices * i, numTriangles * i);
        }*/

        //GenerateNormals(0, numVertices * directions.Length, 0, numTriangles * directions.Length);
    }

    public void CommitSurface()
    {
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = triangles;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals;
        arrays[(int)ArrayMesh.ArrayType.TexUv] = uvs;
        //ArrayMesh arrayMesh = new ArrayMesh();
        //arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        if (Mesh.GetSurfaceCount() > 0)
        {
            GetMeshArray().SurfaceRemove(0);
        }
        ((ArrayMesh)Mesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        UpdateShader();
    }

    private void onMaterialChanged()
    {
        materialChanged = true;
    }

    private void onMeshChanged()
    {
        meshChanged = true;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Mesh = new ArrayMesh();
        surfaceColor.Connect("changed", this, "onMaterialChanged");
        GenerateMesh();
        GenerateUvs();
        CommitSurface();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Engine.EditorHint && (materialChanged || meshChanged))
        {
            GD.Print("Updating Surface");
            var start = OS.GetTicksMsec();
            if (meshChanged)
            {
                GenerateMesh();
            }
            if (meshChanged || materialChanged)
            {
                GenerateUvs();
            }
            materialChanged = false;
            meshChanged = false;
            CommitSurface();
            var end = OS.GetTicksMsec();
            GD.Print("Update Time: " + (end - start));
        }      
    }
}
