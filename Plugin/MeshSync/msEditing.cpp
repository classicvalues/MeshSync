#include "pch.h"
#include "msEditing.h"
#include "MeshUtils/ampmath.h"

//#define msEnableProfiling

using ns = uint64_t;
static inline ns now()
{
    using namespace std::chrono;
    return duration_cast<nanoseconds>(steady_clock::now().time_since_epoch()).count();
}


namespace ms {

void ProjectNormals(ms::Mesh& dst, ms::Mesh& src, EditFlags flags)
{
    dst.flags.has_normals = 1;
    dst.refine_settings.flags.gen_normals_with_smooth_angle = 0;

    // just copy normals if topology is identical
    if (src.indices == dst.indices) {
        ms::MeshRefineSettings rs;
        rs.flags.gen_normals_with_smooth_angle = 1;
        rs.smooth_angle = src.refine_settings.smooth_angle;
        src.refine(rs);

        size_t n = src.normals.size();
        dst.normals.resize(n);
        for (size_t i = 0; i < n; ++i) {
            dst.normals[i] = src.normals[i] * -1.0f;
        }
        return;
    }

    bool use_gpu = false;
#ifdef _MSC_VER
    use_gpu = am::device_available() && flags & msEF_PreferGPU;
#endif

    int num_triangles = (int)src.indices.size() / 3;
    int num_rays = (int)dst.points.size();
    RawVector<float> soa[9];

    concurrency::parallel_invoke(
        [&]() {
            ms::MeshRefineSettings rs;
            rs.flags.triangulate = 1;
            rs.flags.gen_normals_with_smooth_angle = 1;
            rs.smooth_angle = src.refine_settings.smooth_angle;
            src.refine(rs);

            if (!use_gpu) {
                // make SoAnized triangles data
                for (int i = 0; i < 9; ++i) {
                    soa[i].resize(num_triangles);
                }
                for (int ti = 0; ti < num_triangles; ++ti) {
                    for (int i = 0; i < 3; ++i) {
                        ms::float3 p = src.points[src.indices[ti * 3 + i]];
                        soa[i * 3 + 0][ti] = p.x;
                        soa[i * 3 + 1][ti] = p.y;
                        soa[i * 3 + 2][ti] = p.z;
                    }
                }
            }
        },
        [&]() {
            ms::MeshRefineSettings rs;
            rs.flags.no_reindexing = 1;
            rs.flags.gen_normals = 1;
            dst.refine(rs);
        }
    );

#ifdef msEnableProfiling
    auto tbegin = now();
    static int s_count;
    if (++s_count % 2 == 0) { (int&)flags &= ~msEF_PreferGPU; }
#endif

#ifdef _MSC_VER
    if (use_gpu) {
        using namespace am;

        array_view<const float_3> vpoints((int)src.points.size(), (const float_3*)src.points.data());
        array_view<const float_3> vnormals((int)src.normals.size(), (const float_3*)src.normals.data());
        array_view<const int_3> vindices(num_triangles, (const int_3*)src.indices.data());
        array_view<const float_3> vrpos((int)dst.points.size(), (const float_3*)dst.points.data());
        //array_view<const int> vrindices(num_triangles, (const int*)dst.indices.data());
        array_view<float_3> vresult(num_rays, (float_3*)dst.normals.data());

        parallel_for_each(vresult.extent, [=](index<1> ri) restrict(amp)
        {
            //float_3 rpos = vrpos[vrindices[ri]];
            float_3 rpos = vrpos[ri];
            float_3 rdir = vresult[ri];
            float distance = FLT_MAX;
            int hit = 0;

            for (int ti = 0; ti < num_triangles; ++ti) {
                int_3 idx = vindices[ti];
                float d;
                if (am::ray_triangle_intersection(rpos, rdir,
                    vpoints[idx.x], vpoints[idx.y], vpoints[idx.z], d))
                {
                    if (d < distance) {
                        distance = d;
                        hit = ti;
                    }
                }
            }
            if (distance < FLT_MAX) {
                int_3 idx = vindices[hit];
                vresult[ri] = -am::triangle_interpolation(
                    rpos + rdir * distance,
                    vpoints[idx.x], vpoints[idx.y], vpoints[idx.z],
                    vnormals[idx.x], vnormals[idx.y], vnormals[idx.z]);
                ;
            }
        });
        vresult.synchronize();
    }
    else
#endif
    {
        concurrency::parallel_for(0, num_rays, [&](int ri) {
            ms::float3 rpos = dst.points[ri];
            ms::float3 rdir = dst.normals[ri];
            int ti;
            float distance;
            int num_hit = ms::RayTrianglesIntersection(rpos, rdir,
                soa[0].data(), soa[1].data(), soa[2].data(),
                soa[3].data(), soa[4].data(), soa[5].data(),
                soa[6].data(), soa[7].data(), soa[8].data(),
                num_triangles, ti, distance);

            if (num_hit > 0) {
                dst.normals[ri] = -triangle_interpolation(
                    rpos + rdir * distance,
                    { soa[0][ti], soa[1][ti], soa[2][ti] },
                    { soa[3][ti], soa[4][ti], soa[5][ti] },
                    { soa[6][ti], soa[7][ti], soa[8][ti] },
                    src.normals[src.indices[ti * 3 + 0]],
                    src.normals[src.indices[ti * 3 + 1]],
                    src.normals[src.indices[ti * 3 + 2]]);
            }
        });
    }

#ifdef msEnableProfiling
    auto tend = now();
    char buf[1024];
    sprintf(buf,
        "ProjectNormals (%s): %d rays, %d triangles %.2fms\n",
        use_gpu ? "GPU" : "CPU",
        num_rays, num_triangles, float(tend - tbegin) / 1000000.0f
    );
    ::OutputDebugStringA(buf);
#endif
}

} // namespace ms
