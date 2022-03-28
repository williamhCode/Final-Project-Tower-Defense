using System;
using Microsoft.Xna.Framework;

namespace TowerDefense.Camera
{
    public class Camera3D
    {
        public Vector3 forward = new Vector3(0, 0, -1);
        public Vector3 up = new Vector3(0, 1, 0);

        public Vector3 position;
        public Vector3 orientation;
        public Matrix projectionMatrix;

        public Camera3D(Vector3 position)
        {
            this.position = position;
            this.orientation = new Vector3(0, 0, 0);
            this.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
        }

        public void Rotate(float pitch, float yaw, float roll)
        {
            orientation.X += pitch;
            orientation.Y += yaw;
            orientation.Z += roll;
        }

        public void Translate(float x, float y, float z)
        {
            position.X += x;
            position.Y += y;
            position.Z += z;
        }

        public Matrix GetViewMatrix()
        {
            var rotationMatrix = Matrix.CreateFromYawPitchRoll(orientation.Y, orientation.X, orientation.Z);
            forward = Vector3.Transform(forward, rotationMatrix);
            up = Vector3.Transform(up, rotationMatrix);
            return Matrix.CreateLookAt(position, position + forward, up);
        }

        public Matrix GetProjectionMatrix()
        {
            return projectionMatrix;
        }
    }

    public class FPS_Camera : Camera3D
    {
        public FPS_Camera(Vector3 position) : base(position)
        {
        }

        public void Rotate(float horizontal, float vertical)
        {
            base.Rotate(vertical, horizontal, 0);
            orientation.X = MathHelper.Clamp(orientation.X, -MathHelper.PiOver2, MathHelper.PiOver2);
        }

        public void Move(float forwards, float sideways, float vertical)
        {
            var rotation = Matrix.CreateRotationY(orientation.Y);
            var translationVector = new Vector3(sideways, vertical, forwards);
            var moveDirection = Vector3.Transform(translationVector, rotation);
            Translate(moveDirection.X, moveDirection.Y, moveDirection.Z);
        }
    }
}