using OpenTK.Mathematics;
using System;

namespace VoxelFactoryCore
{
    public static class Mathmatics
    {
        public const double PI = 3.1415926535897931;
        public const double E = 2.7182818284590451;

        public static Matrix4 CreateViewMatrix(Camera camera)
        {
            return Matrix4.LookAt((Vector3)camera.Position, (Vector3)camera.Position + Quaternion.FromEulerAngles((Vector3)camera.Rotation * ((float)PI / 180f)) * Vector3.UnitZ, Vector3.UnitY);
            /*
            Matrix4 matrix = Matrix4.Identity;

            Vector3 negativeCameraPos = (Vector3)(-camera.Position);
            matrix *= Matrix4.CreateTranslation(negativeCameraPos);
            matrix *= Matrix4.CreateRotationY((float)ConvertToRadians(camera.Rotation.Y));
            matrix *= Matrix4.CreateRotationX((float)ConvertToRadians(camera.Rotation.X));
            matrix *= Matrix4.CreateRotationZ((float)ConvertToRadians(camera.Rotation.Z));
            return matrix;*/
        }

        /// <summary>
        /// Converts degrees into radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double ConvertToRadians(double degrees)
        {
            return (PI / 180) * degrees;
        }
    }
}
