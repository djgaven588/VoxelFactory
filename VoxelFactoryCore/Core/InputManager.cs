using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelFactoryCore
{
    public static class InputManager
    {
        private static KeyboardState lastState;
        private static KeyboardState currentState;
        private static MouseState lastMouseState;
        private static MouseState currentMouseState;

        private static bool mouseLockedAndHidden = false;
        private static Vector2 mouseDeltaSinceLastFrame;
        private static Vector2 mouseDelta;

        private static bool mouseMovementAccepted = true;

        public static void Initialize()
        {
            WindowManager.Inst.MouseMove += OnMouseMove;
        }

        public static void ChangeMouseState(bool lockedAndHidden)
        {
            mouseLockedAndHidden = lockedAndHidden;

            UpdateMouse();
        }

        public static void ToggleMouseState()
        {
            mouseLockedAndHidden = !mouseLockedAndHidden;

            UpdateMouse();
        }

        private static void UpdateMouse()
        {
            if (mouseLockedAndHidden)
            {
                WindowManager.Inst.CursorGrabbed = true;
            }
            else
            {
                WindowManager.Inst.CursorVisible = true;
                WindowManager.Inst.MousePosition = new Vector2(WindowManager.Inst.Size.X, WindowManager.Inst.Size.Y) / 2;
            }

            mouseMovementAccepted = false;
        }

        public static void UpdateInput(GameWindow window)
        {
            lastState = currentState;
            currentState = window.KeyboardState;

            lastMouseState = currentMouseState;
            currentMouseState = window.MouseState;

            mouseDelta = mouseDeltaSinceLastFrame;
            mouseDeltaSinceLastFrame = Vector2.Zero;
        }

        private static void OnMouseMove(MouseMoveEventArgs e)
        {
            if (mouseMovementAccepted)
            {
                mouseDeltaSinceLastFrame -= e.Delta;
            }

            mouseMovementAccepted = true;
        }

        public static int GetAxis(Keys positive, Keys negative)
        {
            int value = 0;
            if (IsKeyDown(positive))
            {
                value += 1;
            }

            if (IsKeyDown(negative))
            {
                value -= 1;
            }

            return value;
        }

        public static bool IsKeyDown(Keys key)
        {
            return currentState.IsKeyDown(key);
        }

        public static bool IsKeyNowDown(Keys key)
        {
            return currentState.IsKeyDown(key) && !lastState.IsKeyDown(key);
        }

        public static bool IsKeyNowUp(Keys key)
        {
            return !currentState.IsKeyDown(key) && lastState.IsKeyDown(key);
        }

        public static bool IsMouseDown(MouseButton button)
        {
            return currentMouseState.IsButtonDown(button);
        }

        public static bool IsMouseNowDown(MouseButton button)
        {
            return currentMouseState.IsButtonDown(button) && !lastMouseState.IsButtonDown(button);
        }

        public static bool IsMouseNowUp(MouseButton button)
        {
            return !currentMouseState.IsButtonDown(button) && lastMouseState.IsButtonDown(button);
        }

        public static Vector2 MouseDelta()
        {
            return mouseDelta;
        }

        public static Vector2 MousePosition()
        {
            return new Vector2(currentMouseState.X, currentMouseState.Y);
        }
    }
}
