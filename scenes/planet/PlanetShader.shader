shader_type spatial;
render_mode specular_schlick_ggx;

varying vec3 local_vertex;
uniform float min_elevation;
uniform float max_elevation;
uniform sampler2D surface_color_texture;
uniform sampler2D ocean_texture;
uniform float smoothness;

void vertex() {
	local_vertex = VERTEX;
}

float inv_lerp(float from, float to, float val) {
	return (val - from) / (to - from);
}

float lerp(float a, float b, float t)
{
	return (1f - t) * a + t * b;
}

void fragment() {
	float ocean_depth = inv_lerp(min_elevation, 0, UV.y);
	float land_elevation = inv_lerp(0, max_elevation, UV.y);
	float is_land = min(floor(ocean_depth), 1);
	vec3 land_color = texture(surface_color_texture, vec2(land_elevation, UV.x)).rgb;
	vec3 ocean_color = texture(ocean_texture, vec2(ocean_depth, 0)).rgb;
	//ALBEDO = vec3(is_land, is_land, is_land);
	ALBEDO = is_land * land_color + (1f - is_land) * ocean_color;
	// float height = inv_lerp(min_elevation, max_elevation, length(local_vertex));
	// ALBEDO = texture(surface_color_texture, vec2(height, UV.x)).rgb;
	//ALBEDO = vec3(255.0*UV.x, 255.0*UV.x, 255.0*UV.x);
	
	ROUGHNESS = is_land + (1f-is_land) * (1f-smoothness);

}

void light() {
// Output:0

}