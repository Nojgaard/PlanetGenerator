extends GradientTexture

class_name AA_SurfaceColor

export(int) var test_ting


func _changed():
	print("WOOW")

func _set(name, val):
	print("IM HERE!!")
	print(name)

# Declare member variables here. Examples:
# var a = 2
# var b = "text"

func _init():
	print("UOUOOU")

# Called when the node enters the scene tree for the first time.
func _ready():
	print("LOADED SURFACE_COLaaORaaaaaa")


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
