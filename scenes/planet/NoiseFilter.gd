tool
extends Resource

class_name AA_NoiseFilter

var noise = OpenSimplexNoise.new()
export var is_enabled = true setget set_is_enabled
export var use_mask = false setget set_use_mask
export var noise_seed = 0 setget set_noise_seed
export(int, 1, 9) var octaves = 3 setget set_octaves
export(float, 0, 5) var period = 0.5 setget set_period
export(float, 0, 10) var persistence = 0.3 setget set_persistence
export(float, 0, 10) var lacunarity = 2.0 setget set_lacunarity
export(float) var strength = 1.0 setget set_strength
export(float, 0, 2) var subtract = 0.0 setget set_subtract

func evaluate(point: Vector3):
	var val = (noise.get_noise_3dv(point) + 1) * 0.5
	return max(0, val - subtract) * strength

func update():
	noise.seed = noise_seed
	noise.period = period
	noise.octaves = octaves
	noise.persistence = persistence
	noise.lacunarity = lacunarity
	emit_signal("changed")

func set_is_enabled(val):
	is_enabled = val
	update()

func set_use_mask(val):
	use_mask = val
	update()

func set_noise_seed(val):
	noise_seed = val
	update()
	
func set_period(val):
	period = val
	update()

func set_octaves(val):
	octaves = val
	update()
	
func set_persistence(val):
	persistence = val
	update()
	
func set_lacunarity(val):
	lacunarity = val
	update()
	
func set_strength(val):
	strength = val
	update()

func set_subtract(val):
	subtract = val
	update()

func _ready():
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
