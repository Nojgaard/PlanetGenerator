tool
extends Spatial

var has_mesh_changed = false
var has_color_changed = false

var resolution = 10 setget set_resolution
var shape_radius = 1.0 setget set_radius
var color_surface = Color(1,0,0,1) setget set_color_surface

export(Array, Resource) var noise_filters
export(GradientTexture) var color_settings: GradientTexture

	
func add_group(props, group_name, group_hint):
	props.append({
		name = group_name,
		type = TYPE_NIL,
		hint_string = group_hint + "_",
		usage = PROPERTY_USAGE_GROUP | PROPERTY_USAGE_SCRIPT_VARIABLE
	})
	
func add_property(props, name, type, hint = PROPERTY_HINT_NONE, hint_string = ""):
	props.append({
		hint = hint,
		hint_string = hint_string,
		usage = PROPERTY_USAGE_DEFAULT,
		name = name,
		type = type
	})
	
func _get_property_list():
	var properties = []
	add_property(properties, "resolution", TYPE_INT, PROPERTY_HINT_RANGE, "2, 200, 1")
	add_group(properties, "Shape", "shape")
	add_property(properties, "shape_radius", TYPE_REAL)
	add_group(properties, "Color", "color")
	add_property(properties, "color_surface", TYPE_COLOR)
	
	return properties

func set_resolution(new_resolution):
	resolution = new_resolution
	if is_inside_tree():
		$Surface.generate_mesh()
	
func set_radius(new_radius):
	shape_radius = new_radius
	if is_inside_tree():
		$Surface.generate_mesh()
	
	
func set_color_surface(new_color):
	color_surface = new_color
		

func _on_mesh_changed():
	has_mesh_changed = true
	
func _on_color_changed():
	print("IM CONNECTED")
	

# Called when the node enters the scene tree for the first time.
func _ready():
	for nf in noise_filters:
		nf.connect("changed", self, "_on_mesh_changed")
	color_settings.connect("changed", self, "_on_color_changed")
	$Surface.generate_mesh()
	



# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if Engine.editor_hint and has_mesh_changed:
		has_mesh_changed = false
		var start = OS.get_ticks_msec()
		$Surface.generate_mesh()
		var end = OS.get_ticks_msec()
		print("generate_mesh: %f" % (end - start))
	if Engine.editor_hint and has_color_changed:
		has_color_changed = false
		$Surface.update_color()
