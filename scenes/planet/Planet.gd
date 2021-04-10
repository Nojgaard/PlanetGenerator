tool
extends Spatial

var SurfaceColor2 = preload("res://scenes/planet/SurfaceColor.cs")
var BiomeColor = preload("res://scenes/planet/BiomeColor.cs")
var NoiseFilter = preload("res://scenes/planet/NoiseFilter.cs")
var Surface = preload("res://scenes/planet/Surface.cs")
var has_mesh_changed = false
var has_color_changed = false

var resolution = 10 setget set_resolution
var shape_radius = 1.0 setget set_radius
var color_surface = Color(1,0,0,1) setget set_color_surface
var shape_smooth = true setget set_shape_smooth

export(Array, Resource) var noise_filters setget set_noise_filters
export(GradientTexture) var color_settings: GradientTexture
export(GradientTexture) var biome_test

	
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
	add_property(properties, "resolution", TYPE_INT, PROPERTY_HINT_RANGE, "2, 256, 1")
	add_group(properties, "Shape", "shape")
	add_property(properties, "shape_radius", TYPE_REAL)
	add_property(properties, "shape_smooth", TYPE_BOOL)
	add_group(properties, "Color", "color")
	add_property(properties, "color_surface", TYPE_COLOR)
	
	return properties

func set_resolution(new_resolution):
	resolution = new_resolution
	_on_mesh_changed()
	
func set_radius(new_radius):
	shape_radius = new_radius
	_on_mesh_changed()
	
	
func set_color_surface(new_color):
	color_surface = new_color
	
func set_shape_smooth(val):
	shape_smooth = val
	_on_mesh_changed()

func set_noise_filters(val):
	noise_filters = val
	for i in noise_filters.size():
		var nf = NoiseFilter.new()
		if noise_filters[i] == null:
			noise_filters[i] = nf
			noise_filters[i].connect("changed", self, "_on_mesh_changed")
	print("changed")

func _on_mesh_changed():
	has_mesh_changed = true
	$Surface.meshChanged = true
	
func _on_color_changed():
	print("IM CONNECTED")

func _on_noise_changed():
	print("Noise Changed")
	
func _on_update_surface():
	$Surface.materialChanged = true
	

# Called when the node enters the scene tree for the first time.
func _ready():
	if biome_test == null:
		biome_test = SurfaceColor2.new()
	#test.connect("changed", self, "_on_noise_changed")
	
	for nf in noise_filters:
		nf.connect("changed", self, "_on_mesh_changed")
	
	biome_test.connect("changed", self, "_on_update_surface")
	color_settings.connect("changed", self, "_on_color_changed")
	



# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	return
	if Engine.editor_hint and has_mesh_changed:
		has_mesh_changed = false
		var start = OS.get_ticks_msec()
		$Surface.generate_mesh()
		var end = OS.get_ticks_msec()
		print("generate_mesh: %f" % (end - start))
	if Engine.editor_hint and has_color_changed:
		has_color_changed = false
		$Surface.update_color()
