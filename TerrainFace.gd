const SurfaceGenerator = preload("res://SurfaceGenerator.gd")

var mesh: SurfaceTool
var resolution: int
var local_up: Vector3
var axis_a: Vector3
var axis_b: Vector3


func _init(mesh: SurfaceTool, resolution: int, local_up: Vector3):
	self.mesh = mesh
	self.resolution = resolution
	self.local_up = local_up
	
	axis_a = Vector3(local_up.y, local_up.z, local_up.x) 
	# axis_b should be perpendicular to local_up and axis_a
	axis_b = local_up.cross(axis_a)


func construct_mesh(surface_generator: SurfaceGenerator, offset) -> void:
	var vertices: PoolVector3Array = []

	var num_triangles = (resolution-1)*(resolution-1) * 2 * 3
	var triangles: PoolIntArray = []
	var num_verts = offset * resolution * resolution
#
	for y in range(resolution):
		for x in range(resolution):
			var percent: Vector2 = Vector2(x, y) / (resolution - 1)
			var point_on_unit_cube = local_up + (percent.x - .5) * 2 * axis_a + (percent.y - .5) * 2 * axis_b
			var point_on_unit_sphere = point_on_unit_cube.normalized()
			var point_on_sphere = surface_generator.calc_point_on_sphere(point_on_unit_sphere)
			mesh.add_vertex(point_on_sphere)
			#vertices.append(point_on_sphere)
			
			if y == resolution - 1 or x == resolution - 1:
				continue
			
			var i = x + y * resolution + num_verts
			mesh.add_index(i)
			mesh.add_index(i+resolution)
			mesh.add_index(i+resolution+1)
			
			mesh.add_index(i)
			mesh.add_index(i+1+resolution)
			mesh.add_index(i+1)
			
