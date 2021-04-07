#ifndef SURFACE_GENERATOR_H
#define SURFACE_GENERATOR_H

#include <Godot.hpp>
#include <Object.hpp>
#include <SurfaceTool.hpp>
#include <Vector3.hpp>
#include <Variant.hpp>

namespace godot {

class SurfaceGenerator : public Object {
	GODOT_CLASS(SurfaceGenerator, Object)

public:
    int resolution;
    double radius;
public:
    static void _register_methods();

    void _init(); // our initializer called by Godot
    void generate_mesh(SurfaceTool *st);

private:
    Vector3 calc_point_on_sphere(const Vector3& point_on_unit_sphere);

    void generate_face(SurfaceTool& st, const Vector3& local_up, int offset);
};

}


#endif // !PLANET_SURFACE_H
