tool
extends MeshInstance

const PlanetShader = preload("res://scenes/planet/PlanetShader.shader")
var resolution = 10
var radius = 1
var min_elevation = INF
var max_elevation = 0

func calc_elevation(point: Vector3):
	var elevation = 0
	var first_layer = -1
	for nf in get_parent().noise_filters:
		if first_layer == -1:
			first_layer = nf.Evaluate(point)
			if nf.IsEnabled:
				elevation = first_layer
		else:
			if not nf.IsEnabled: continue
			var mask = first_layer if nf.UseMask else 1
			elevation += nf.Evaluate(point) * mask
			
	return elevation


func calc_point_on_sphere(point_on_unit_sphere):
	#var elevation = calc_elevation(point_on_unit_sphere)
	#var elevation = get_parent().noise_filters[0].evaluate(point_on_unit_sphere)
	var elevation = (1+calc_elevation(point_on_unit_sphere)) * radius
	min_elevation = min(elevation, min_elevation)
	max_elevation = max(elevation, max_elevation)
	return point_on_unit_sphere * elevation

func generate_face(st: SurfaceTool, local_up: Vector3, index_offset: int):
	var axis_a = Vector3(local_up.y, local_up.z, local_up.x) 
	# axis_b should be perpendicular to local_up and axis_a
	var axis_b = local_up.cross(axis_a)
	for y in resolution:
		for x in resolution:
			var percent: Vector2 = Vector2(x, y) / (resolution - 1)
			var point_on_unit_cube = local_up + (percent.x - .5) * 2 * axis_a + (percent.y - .5) * 2 * axis_b
			var point_on_unit_sphere = point_on_unit_cube.normalized()
			var point_on_sphere = calc_point_on_sphere(point_on_unit_sphere)
			st.add_vertex(point_on_sphere)
			
			if y == resolution - 1 or x == resolution - 1:
				continue
			
			var i = x + y * resolution + index_offset
			st.add_index(i)
			st.add_index(i+resolution)
			st.add_index(i+resolution+1)
			
			st.add_index(i)
			st.add_index(i+1+resolution)
			st.add_index(i+1)
			
func generate_mesh():
	min_elevation = INF
	max_elevation = 0
	print("generating mesh")
	resolution = get_parent().resolution
	radius = get_parent().shape_radius
	var st = SurfaceTool.new()
	var directions = [
		Vector3(1,0,0),
		Vector3(-1,0,0),
		Vector3(0,1,0),
		Vector3(0,-1,0),
		Vector3(0,0,1),
		Vector3(0,0,-1)
		]

	var start = OS.get_ticks_msec()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	st.add_smooth_group(get_parent().shape_smooth)
	var offset = 0
	for local_up in directions:
		generate_face(st, local_up, offset)
		offset += resolution * resolution
	var end = OS.get_ticks_msec()
	print("generateing faces: %f" % (end - start))
	start = OS.get_ticks_msec()
	st.generate_normals()
	mesh = st.commit()
	end = OS.get_ticks_msec()
	print("generateing normals and commiting: %f" % (end - start))
	#update_color()
	update_shader()


func update_shader():
	var mat = get_surface_material(0)
	if mat == null:
		set_surface_material(0, ShaderMaterial.new())
		mat = get_surface_material(0)
		mat.shader = PlanetShader
	print(min_elevation, ", ", max_elevation)
	mat.set_shader_param("min_elevation", min_elevation)
	mat.set_shader_param("max_elevation", max_elevation)
	mat.set_shader_param("surface_color_texture", get_parent().color_settings)
	
func update_color():
	var mat = SpatialMaterial.new()    
	mat.albedo_color = get_parent().color_surface
	#mesh.surface_set_material(0, mat)
	set_surface_material(0, mat)

# Called when the node enters the scene tree for the first time.
func _ready():
	pass


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
