using System;
using System.Numerics;
using System.Reflection;
using System.Xml;

namespace Engine.Rendering
{
    public struct CullingFrustum
    {
        public float near_right;
        public float near_top;
        public float near_plane;
        public float far_plane;
    };

    public struct AABB
    {
        // Two different corners of the AABB bounding box
        public Vector3 min;
        public Vector3 max;
    };

    struct Obb
    {
        public Vector3 Center;

        public Vector3 Extents;

        // Orthonormal basis
        public Vector3[] Axes;
    };

    public class FrustrumCulling
    {
        public static bool SAT_visibility_test(AABB aabb, Matrix4x4 vsTransform, Matrix4x4 cam_modelview_proj_mat)
        {
            var tan_fov =  (float)Math.Tan(Camera.MainCamera.GetFOV());
            CullingFrustum frustum = new CullingFrustum(){
                near_right = Camera.MainCamera.AspectRatio * Camera.MainCamera.NearPlane * tan_fov,
                near_top = Camera.MainCamera.NearPlane * tan_fov,
                near_plane = -Camera.MainCamera.NearPlane,
                far_plane = -Camera.MainCamera.FarPlane,
            };

            return test_using_separating_axis_theorem(frustum, vsTransform, aabb);
        }

        public static bool ObbInFrustum(AABB aabb, Matrix4x4 obj_transform_mat, Matrix4x4 cam_modelview_proj_mat)
        {

            Vector3 Min = aabb.min;
            Vector3 Max = aabb.max;
            //transform all 8 box points to clip space
            ////clip space because we easily can test points outside required unit cube
            //NOTE: for DirectX we should test z coordinate from 0 to w (-w..w - for OpenGL), look for transformations / clipping box differences
            //matrix to transfrom points to clip space
            Matrix4x4 to_clip_space_mat = cam_modelview_proj_mat * obj_transform_mat;
            //transform all 8 box points to clip space
            Vector4[] obb_points = new Vector4[8];
            obb_points[0] = Vector4.Transform(new Vector4(Min.X, Max.Y, Min.Z, 1.0f) , to_clip_space_mat);
            obb_points[1] = Vector4.Transform(new Vector4(Min.X, Max.Y, Max.Z, 1.0f), to_clip_space_mat);
            obb_points[2] = Vector4.Transform(new Vector4(Max.X, Max.Y, Max.Z, 1.0f), to_clip_space_mat);
            obb_points[3] = Vector4.Transform(new Vector4(Max.X, Max.Y, Min.Z, 1.0f), to_clip_space_mat);
            obb_points[4] = Vector4.Transform(new Vector4(Max.X, Min.Y, Min.Z, 1.0f), to_clip_space_mat);
            obb_points[5] = Vector4.Transform(new Vector4(Max.X, Min.Y, Max.Z, 1.0f), to_clip_space_mat);
            obb_points[6] = Vector4.Transform(new Vector4(Min.X, Min.Y, Max.Z, 1.0f), to_clip_space_mat);
            obb_points[7] = Vector4.Transform(new Vector4(Min.X, Min.Y, Min.Z, 1.0f), to_clip_space_mat);

            bool outside;
            //we have 6 frustum planes, which in clip space is unit cube (for GL) with -1..1 range for (int i = 0; i < 3; i++)
            //3 because we test positive & negative plane at once
            {
                //if all 8 points outside one of the plane
                //actually it is vertex normalization xyz / w, then compare if all 8points coordinates < -1 or > 1
                bool outsidePositivePlane = CompareXYZtoPosW(obb_points[0]) && CompareXYZtoPosW(obb_points[1]) &&
                                            CompareXYZtoPosW(obb_points[2]) && CompareXYZtoPosW(obb_points[3]) &&
                                            CompareXYZtoPosW(obb_points[4]) && CompareXYZtoPosW(obb_points[5]) &&
                                            CompareXYZtoPosW(obb_points[6]) && CompareXYZtoPosW(obb_points[7]);
                
                bool outsideNegativePlane = CompareXYZtoNegW(obb_points[0]) && CompareXYZtoNegW(obb_points[1]) &&
                                            CompareXYZtoNegW(obb_points[2]) && CompareXYZtoNegW(obb_points[3]) &&
                                            CompareXYZtoNegW(obb_points[4]) && CompareXYZtoNegW(obb_points[5]) &&
                                            CompareXYZtoNegW(obb_points[6]) && CompareXYZtoNegW(obb_points[7]);
                
                outside = outsidePositivePlane || outsideNegativePlane;
            } 
            return !outside;
        }


        static bool CompareXYZtoPosW(Vector4 vec4)
        {
            return vec4.X > vec4.W && vec4.Y > vec4.W && vec4.Z > vec4.W;
        }
        static bool CompareXYZtoNegW(Vector4 vec4) 
        {
            return vec4.X < -vec4.W && vec4.Y < -vec4.W && vec4.Z < -vec4.W; 
        }
        
        static bool test_using_separating_axis_theorem(CullingFrustum frustum, Matrix4x4 vs_transform, AABB aabb)
        {
            // Near, far
            float z_near = frustum.near_plane;
            float z_far = frustum.far_plane;
            // half width, half height
            float x_near = frustum.near_right;
            float y_near = frustum.near_top;

            // So first thing we need to do is obtain the normal directions of our OBB by transforming 4 of our AABB vertices
            Vector3[] corners = new []{
                new Vector3(aabb.min.X, aabb.min.Y, aabb.min.Z),
                new Vector3(aabb.max.X, aabb.min.Y, aabb.min.Z),
                new Vector3(aabb.min.X, aabb.max.Y, aabb.min.Z),
                new Vector3(aabb.min.X, aabb.min.Y, aabb.max.Z),
            };

            // Transform corners
            // This only translates to our OBB if our transform is affine
            for (int corner_idx = 0; corner_idx < corners.Length; corner_idx++)
            {
                corners[corner_idx] = Vector3.Transform(corners[corner_idx], vs_transform);
            }

            Obb obb = new Obb(){
                Axes = new []{
                corners[1] - corners[0],
                corners[2] - corners[0],
                corners[3] - corners[0]
                },
            };
            obb.Center = corners[0] + 0.5f * (obb.Axes[0] + obb.Axes[1] + obb.Axes[2]);
            obb.Extents = new Vector3( obb.Axes[0].Length(), obb.Axes[1].Length(), obb.Axes[2].Length() );
            obb.Axes[0] = obb.Axes[0] / obb.Extents.X;
            obb.Axes[1] = obb.Axes[1] / obb.Extents.Y;
            obb.Axes[2] = obb.Axes[2] / obb.Extents.Z;
            obb.Extents *= 0.5f;

            {
                Vector3 M = new Vector3( 0, 0, 1 );
                float MoX = 0.0f;
                float MoY = 0.0f;
                float MoZ = 1.0f;

                // Projected center of our OBB
                float MoC = obb.Center.Z;
                // Projected size of OBB
                float radius = 0.0f;
                for (int i = 0; i < 3; i++) {
                    // dot(M, axes[i]) == axes[i].Z;

                    if (i == 0)
                    {
                        radius += Math.Abs(obb.Axes[i].Z) * obb.Extents.X;
                    }
                    else if(i ==1)
                    {
                        radius += Math.Abs(obb.Axes[i].Z) * obb.Extents.Y;
                    }
                    else if (i == 2)
                    {
                        radius += Math.Abs(obb.Axes[i].Z) * obb.Extents.Z;
                    }
                }
                float obb_min = MoC - radius;
                float obb_max = MoC + radius;

                float tau_0 = z_far; // Since z is negative, far is smaller than near
                float tau_1 = z_near;

                if (obb_min > tau_1 || obb_max < tau_0) {
                    return false;
                }
            }

            {
                Vector3[] M = new []{
                    new Vector3(z_near, 0.0f, x_near) , // Left Plane
                    new Vector3(-z_near, 0.0f, x_near ), // Right plane
                    new Vector3(0.0f, -z_near, y_near ), // Top plane
                    new Vector3( 0.0f, z_near, y_near ), // Bottom plane
                };
                
                for (int m = 0; m < M.Length; m++) {
                    float MoX = Math.Abs(M[m].X);
                    float MoY = Math.Abs(M[m].Y);
                    float MoZ = M[m].Z;
                    float MoC = Vector3.Dot(M[m], obb.Center);

                    float obb_radius = 0.0f;
                    for (int i = 0; i < 3; i++) {
                        
                        if (i == 0)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M[m], obb.Axes[i])) * obb.Extents.X;
                        }
                        else if (i == 1)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M[m], obb.Axes[i])) * obb.Extents.Y;
                        }
                        else if (i == 2)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M[m], obb.Axes[i])) * obb.Extents.Z;
                        }

                    }
                    float obb_min = MoC - obb_radius;
                    float obb_max = MoC + obb_radius;

                    float p = x_near * MoX + y_near * MoY;

                    float tau_0 = z_near * MoZ - p;
                    float tau_1 = z_near * MoZ + p;

                    if (tau_0 < 0.0f) {
                        tau_0 *= z_far / z_near;
                    }
                    if (tau_1 > 0.0f) {
                        tau_1 *= z_far / z_near;
                    }

                    if (obb_min > tau_1 || obb_max < tau_0) {
                        return false;
                    }
                }
            }

            // OBB Axes
            {
                for (int m = 0; m < obb.Axes.Length; m++) {
                    Vector3 M = obb.Axes[m];
                    float MoX = Math.Abs(M.X);
                    float MoY = Math.Abs(M.Y);
                    float MoZ = M.Z;
                    float MoC = Vector3.Dot(M, obb.Center);

                    float obb_radius = 0.0f;
                    if (m == 0)
                    {
                        obb_radius = obb.Extents.X;
                    }
                    else if (m == 1)
                    {
                        obb_radius = obb.Extents.Y;
                    }
                    else if (m == 2)
                    {
                        obb_radius = obb.Extents.Z;
                    }

                    float obb_min = MoC - obb_radius;
                    float obb_max = MoC + obb_radius;

                    // Frustum projection
                    float p = x_near * MoX + y_near * MoY;
                    float tau_0 = z_near * MoZ - p;
                    float tau_1 = z_near * MoZ + p;
                    if (tau_0 < 0.0f) {
                        tau_0 *= z_far / z_near;
                    }
                    if (tau_1 > 0.0f) {
                        tau_1 *= z_far / z_near;
                    }

                    if (obb_min > tau_1 || obb_max < tau_0) {
                        return false;
                    }
                }
            }

            // Now let's perform each of the cross products between the edges
            // First R x A_i
            {
                for (int m = 0; m < obb.Axes.Length; m++) {
                    Vector3 M = new Vector3( 0.0f, -obb.Axes[m].Z, obb.Axes[m].Y );
                    float MoX = 0.0f;
                    float MoY = Math.Abs(M.Y);
                    float MoZ = M.Z;
                    float MoC = M.Y * obb.Center.Y + M.Z * obb.Center.Z;

                    float obb_radius = 0.0f;
                    for (int i = 0; i < 3; i++) {
                        
                        if (i == 0)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M, obb.Axes[i])) * obb.Extents.X;
                        }
                        else if (i == 1)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M, obb.Axes[i])) * obb.Extents.Y;
                        }
                        else if (i == 2)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M, obb.Axes[i])) * obb.Extents.Z;
                        }
                    }

                    float obb_min = MoC - obb_radius;
                    float obb_max = MoC + obb_radius;

                    // Frustum projection
                    float p = x_near * MoX + y_near * MoY;
                    float tau_0 = z_near * MoZ - p;
                    float tau_1 = z_near * MoZ + p;
                    if (tau_0 < 0.0f) {
                        tau_0 *= z_far / z_near;
                    }
                    if (tau_1 > 0.0f) {
                        tau_1 *= z_far / z_near;
                    }

                    if (obb_min > tau_1 || obb_max < tau_0) {
                        return false;
                    }
                }
            }

            // U x A_i
            {
                for (int m = 0; m < obb.Axes.Length; m++) {
                    Vector3 M = new Vector3( obb.Axes[m].Z, 0.0f, -obb.Axes[m].X );
                    float MoX = Math.Abs(M.X);
                    float MoY = 0.0f;
                    float MoZ = M.Z;
                    float MoC = M.X * obb.Center.X + M.Z * obb.Center.Z;

                    float obb_radius = 0.0f;
                    for (int i = 0; i < 3; i++) {
                        
                        if (i == 0)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M, obb.Axes[i])) * obb.Extents.X;
                        }
                        else if (i == 1)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M, obb.Axes[i])) * obb.Extents.Y;
                        }
                        else if (i == 2)
                        {
                            obb_radius += Math.Abs(Vector3.Dot(M, obb.Axes[i])) * obb.Extents.Z;
                        }
                    }

                    float obb_min = MoC - obb_radius;
                    float obb_max = MoC + obb_radius;

                    // Frustum projection
                    float p = x_near * MoX + y_near * MoY;
                    float tau_0 = z_near * MoZ - p;
                    float tau_1 = z_near * MoZ + p;
                    if (tau_0 < 0.0f) {
                        tau_0 *= z_far / z_near;
                    }
                    if (tau_1 > 0.0f) {
                        tau_1 *= z_far / z_near;
                    }

                    if (obb_min > tau_1 || obb_max < tau_0) {
                        return false;
                    }
                }
            }

            // Frustum Edges X Ai
            {
                for (int obb_edge_idx = 0; obb_edge_idx < obb.Axes.Length; obb_edge_idx++) {
                    Vector3[] M = new []{
                        Vector3.Cross(new Vector3(-x_near, 0.0f, z_near), obb.Axes[obb_edge_idx]), // Left Plane
                        Vector3.Cross(new Vector3( x_near, 0.0f, z_near), obb.Axes[obb_edge_idx]), // Right plane
                        Vector3.Cross(new Vector3( 0.0f, y_near, z_near), obb.Axes[obb_edge_idx]), // Top plane
                        Vector3.Cross(new Vector3( 0.0f, -y_near, z_near ), obb.Axes[obb_edge_idx]) // Bottom plane
                    };

                    for (int m = 0; m < M.Length; m++) {
                        float MoX = Math.Abs(M[m].X);
                        float MoY = Math.Abs(M[m].Y);
                        float MoZ = M[m].Z;

                        float epsilon = 1e-4f;
                        if (MoX < epsilon && MoY < epsilon && Math.Abs(MoZ) < epsilon) continue;

                        float MoC = Vector3.Dot(M[m], obb.Center);

                        float obb_radius = 0.0f;
                        for (int i = 0; i < 3; i++) {
                            
                            if (i == 0)
                            {
                                obb_radius += Math.Abs(Vector3.Dot(M[m], obb.Axes[i])) * obb.Extents.X;
                            }
                            else if (i == 0)
                            {
                                obb_radius += Math.Abs(Vector3.Dot(M[m], obb.Axes[i])) * obb.Extents.Y;
                            }
                            else if (i == 2)
                            {
                                obb_radius += Math.Abs(Vector3.Dot(M[m], obb.Axes[i])) * obb.Extents.Z;
                            }
                        }

                        float obb_min = MoC - obb_radius;
                        float obb_max = MoC + obb_radius;

                        // Frustum projection
                        float p = x_near * MoX + y_near * MoY;
                        float tau_0 = z_near * MoZ - p;
                        float tau_1 = z_near * MoZ + p;
                        if (tau_0 < 0.0f) {
                            tau_0 *= z_far / z_near;
                        }
                        if (tau_1 > 0.0f) {
                            tau_1 *= z_far / z_near;
                        }

                        if (obb_min > tau_1 || obb_max < tau_0) {
                            return false;
                        }
                    }
                }
            }

            // No intersections detected
            return true;
        }


    }
}