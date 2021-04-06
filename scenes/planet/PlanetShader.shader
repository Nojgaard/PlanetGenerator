shader_type spatial;
render_mode specular_schlick_ggx;

varying vec3 local_vertex;
uniform float min_elevation;
uniform float max_elevation;
uniform sampler2D surface_color_texture;

void vertex() {
	local_vertex = VERTEX;
}

float inv_lerp(float from, float to, float val) {
	return (val - from) / (to - from);
}

void fragment() {
	float height = inv_lerp(min_elevation, max_elevation, length(local_vertex));
	//ALBEDO = height * vec3(10, 10, 10);
	ALBEDO = texture(surface_color_texture, vec2(height, 1)).rgb;

}

void light() {
// Output:0

}