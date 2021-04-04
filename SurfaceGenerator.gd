var radius = 0

func _init(radius):
	self.radius = radius
	
func calc_point_on_sphere(point_on_unit_spere: Vector3):
	return point_on_unit_spere * radius
