#include "SurfaceGenerator.h"
#include <vector>

using namespace godot;

void SurfaceGenerator::_register_methods() {
    register_method("generate_mesh", &SurfaceGenerator::generate_mesh);
    register_property<SurfaceGenerator, int>("resolution", &SurfaceGenerator::resolution, 10);
    register_property<SurfaceGenerator, double>("radius", &SurfaceGenerator::radius, 1.0);
}

void SurfaceGenerator::_init() {
}

void godot::SurfaceGenerator::generate_mesh(SurfaceTool *st) {
    std::vector<Vector3> directions = {
        Vector3(1, 0, 0), Vector3(-1, 0, 0),
        Vector3(0, 1, 0), Vector3(0, -1, 0),
        Vector3(0, 0, 1), Vector3(0, 0, -1)
    };

    int offset = 0;
    for (const Vector3& local_up : directions) {
        generate_face(*st, local_up, offset);
        offset += 1;
    }
    //st->index();
    st->generate_normals();
}

Vector3 godot::SurfaceGenerator::calc_point_on_sphere(const Vector3& point_on_unit_sphere) {
    return point_on_unit_sphere * radius;
}

void godot::SurfaceGenerator::generate_face(SurfaceTool& st, const Vector3& local_up, int offset) {
    PoolVector3Array vertices;

    Vector3 axis_a(local_up.y, local_up.z, local_up.x);
    Vector3 axis_b = local_up.cross(axis_a);

    int num_verts = resolution * resolution * offset;
    

    for (int y = 0; y < resolution; ++y) {
        for (int x = 0; x < resolution; ++x) {
            Vector2 percent = Vector2(static_cast<real_t>(x), static_cast<real_t>(y)) / static_cast<real_t>(resolution - 1);
            Vector3 point_on_unit_sphere = local_up + (percent.x - .5) * 2 * axis_a + (percent.y - .5) * 2 * axis_b;
            point_on_unit_sphere.normalize();
            st.add_vertex(calc_point_on_sphere(point_on_unit_sphere));

            if (x == resolution - 1 || y == resolution - 1) { continue; }
            int i = x + y * resolution + num_verts;
            st.add_index(i);
            st.add_index(i + resolution);
            st.add_index(i + resolution + 1);

            st.add_index(i);
            st.add_index(i + 1 + resolution);
            st.add_index(i + 1);
 
        }
    }
}
