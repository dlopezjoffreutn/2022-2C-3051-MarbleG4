using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP
{
    internal class FollowCamera : Camera
    {
        private readonly bool lockMouse;

        private readonly Point screenCenter;

        private Vector2 pastMousePosition;
        private float pastMouseWheel;

        private Vector3 PlayerPosition = Vector3.Zero;

        float YaxisAngle = 0;
        float Yhigh = 0;
        float Xdistance = 30;

        private Vector3 AnglePosition;

        // Angles

        public FollowCamera(float aspectRatio, Vector3 position, Point screenCenter) : this(aspectRatio, position)
        {
            lockMouse = true;
            this.screenCenter = screenCenter;
            UpdateCameraVectors();
        }

        public FollowCamera(float aspectRatio, Vector3 position) : base(aspectRatio)
        {
            AnglePosition = new Vector3(-Xdistance, 0, 0);
            Position = position + AnglePosition;
            pastMousePosition = Mouse.GetState().Position.ToVector2();
            UpdateCameraVectors();
            CalculateView();
        }

        public float MovementSpeed { get; set; } = 100f;
        public float MouseSensitivity { get; set; } = 5f;

        private void CalculateView()
        {
            View = Matrix.CreateLookAt(Position, PlayerPosition, UpDirection);
        }

        public override float YAxisAngle()
        {
            return YaxisAngle;
        }

        public override void UpdatePlayerPosition(Vector3 position)
        {
            PlayerPosition = position;
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ProcessMouseMovement(elapsedTime);

            Position = AnglePosition + PlayerPosition;
            Position = new Vector3(Position.X, Math.Max(0, Position.Y), Position.Z);

            CalculateView();
        }

        private void ProcessMouseMovement(float elapsedTime)
        {
            bool notRightClick = true;
            MouseState mouseState = Mouse.GetState();
            if (mouseState.RightButton.Equals(ButtonState.Pressed) || notRightClick)
            {
                Vector2 mouseDelta = mouseState.Position.ToVector2() - pastMousePosition;
                mouseDelta *= MouseSensitivity * elapsedTime;

                float mouseWheelDelta = (Mouse.GetState().ScrollWheelValue - pastMouseWheel) * 0.01f;

                //Position = new Vector3(Position.X - mouseDelta.X, Position.Y, Position.Z);
                float error = 0.001f;
                YaxisAngle -= mouseDelta.X * 0.1f;
                Yhigh = Math.Clamp(Yhigh - mouseDelta.Y * 0.1f, error, 1 - error);
                Xdistance = Math.Clamp(Xdistance - mouseWheelDelta, 15, 50);
                float curve = 1 + MathF.Sqrt(-MathF.Pow(Yhigh, 2) + 1);
                float Odistance = (1 - curve) * Xdistance;

                AnglePosition = new Vector3(Odistance * MathF.Cos(YaxisAngle), Yhigh * Xdistance, Odistance * MathF.Sin(YaxisAngle));

                UpdateCameraVectors();

                if (lockMouse || notRightClick)
                {
                    Mouse.SetPosition(screenCenter.X, screenCenter.Y);
                    Mouse.SetCursor(MouseCursor.Crosshair);
                }
                else
                {
                    Mouse.SetCursor(MouseCursor.Arrow);
                }
            }

            pastMousePosition = Mouse.GetState().Position.ToVector2();
            pastMouseWheel = Mouse.GetState().ScrollWheelValue;
        }

        public void UpdateCameraVectors()
        {
            // Calculate the new Front vector
            Vector3 tempFront;
            tempFront.X = PlayerPosition.X - Position.X;
            tempFront.Y = 0;
            tempFront.Z = PlayerPosition.Z - Position.Z;

            FrontDirection = Vector3.Normalize(tempFront);

            // Also re-calculate the Right and Up vector
            // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            RightDirection = Vector3.Normalize(Vector3.Cross(FrontDirection, Vector3.Up));
            UpDirection = Vector3.Normalize(Vector3.Cross(RightDirection, FrontDirection));
        }
    }
}