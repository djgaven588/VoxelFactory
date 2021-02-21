using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelFactoryCore
{
    public class Camera
    {
        public Vector3d Position = new Vector3d(0, 0, 0);
        public Vector3d Rotation = new Vector3d(0, 0, 0);
        public Matrix4 ProjectionMatrix { get { RecreateProjectionMatrix(); return projectionMatrix; } }
        public float FOV = 60f;
        public float NearPlane = 0.01f;
        public float FarPlane = 1000f;

        private Matrix4 projectionMatrix;

        public void RecreateProjectionMatrix()
        {
            (int width, int height) = WindowManager.Inst.Size;
            float aspectRatio = ((float)width) / height;
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Mathmatics.ConvertToRadians(FOV), aspectRatio, NearPlane, FarPlane);
        }

        public void MoveDirectionBased(Vector3d movement)
        {
            Vector3d toAdd = (Quaternion.FromEulerAngles(0, (float)Mathmatics.ConvertToRadians(-Rotation.Y), 0) * (Vector3)movement);
            Position += toAdd;
        }

        public void Update(float timeDelta)
        {
            if (InputManager.IsKeyNowDown(Keys.Escape))
                InputManager.ToggleMouseState();

            Vector3 movement = new Vector3(InputManager.GetAxis(Keys.D, Keys.A), InputManager.GetAxis(Keys.Space, Keys.LeftShift), InputManager.GetAxis(Keys.W, Keys.S));

            if (movement.Length > 0)
            {
                movement = (movement / movement.Length) * timeDelta * 16;
            }

            var ang = InputManager.MouseDelta().Yx * timeDelta * 5;
            Rotation += new Vector3(ang.X, ang.Y, 0);

            Rotation.X = Math.Clamp(Rotation.X, -90, 90);

            Position += movement;
        }
    }
}
