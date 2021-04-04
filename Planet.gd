tool
extends Spatial

const TerrainFace = preload("res://TerrainFace.gd")
const SurfaceGenerator = preload("res://SurfaceGenerator.gd")

export(int, 2, 256) var resolution = 10 setget set_resolution

var terrain_faces = [] 
var mesh_instance = MeshInstance.new()


var shape_radius = 1.0 setget set_radius
var color_planet = Color(1,0,0,1) setget set_color_planet
func _get_property_list():
	var properties = []
	properties.append({
		name = "Shape",
		type = TYPE_NIL,
		hint_string = "shape_",
		usage = PROPERTY_USAGE_GROUP | PROPERTY_USAGE_SCRIPT_VARIABLE
	})
	properties.append({
		name = "shape_radius",
		type = TYPE_REAL,
		value = shape_radius
	})
	properties.append({
		name = "Color",
		type = TYPE_NIL,
		hint_string = "color_",
		usage = PROPERTY_USAGE_GROUP | PROPERTY_USAGE_SCRIPT_VARIABLE
	})
	properties.append({
		name = "color_planet",
		type = TYPE_COLOR,
		value = color_planet
	})
	
	return properties


func set_radius(new_radius):
	shape_radius = new_radius
	construct_mesh()

func set_color_planet(new_color_planet):
	color_planet = new_color_planet
	var mat = SpatialMaterial.new()    
	mat.albedo_color = color_planet
	mesh_instance.set_surface_material(0, mat)

func _ready():
	add_child(mesh_instance)
	construct_mesh()
	
func construct_mesh():
	terrain_faces = []
	var surface_generator = SurfaceGenerator.new(shape_radius)
	var st = SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	
	var t = get_transform()
	var directions = [
		Vector3(1,0,0),
		Vector3(-1,0,0),
		Vector3(0,1,0),
		Vector3(0,-1,0),
		Vector3(0,0,1),
		Vector3(0,0,-1)
		]
	for dir in directions:
		terrain_faces.append(TerrainFace.new(st, resolution, dir))
		
	for face in terrain_faces:
		face.construct_mesh(surface_generator)
	
	st.index()
	st.generate_normals()
	
	var material = SpatialMaterial.new()    
	material.albedo_color = color_planet
	st.set_material(material)
	mesh_instance.mesh = st.commit()

func set_resolution(new_resolution):
	if resolution != new_resolution:
		resolution = new_resolution
		construct_mesh()
