using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Promotinator.UI;

public static class Input {
    public static Vector2 MousePosition => new(_mouseState.X, _mouseState.Y);

    private static KeyboardState _keyboardState;
    private static KeyboardState _lastKeyboardState;
    private static MouseState _mouseState;
    private static MouseState _lastMouseState;

    public static void Update() {
        _lastKeyboardState = _keyboardState;
        _keyboardState = Keyboard.GetState();

        _lastMouseState = _mouseState;
        _mouseState = Mouse.GetState();
    }

    public static bool IsKeyDown(Keys key) {
        return _keyboardState.IsKeyDown(key);
    }

    public static bool IsKeyPressedOnce(Keys key) {
        return IsKeyDown(key) && !_lastKeyboardState.IsKeyDown(key);
    }

    public static bool IsLeftMouseButtonClick() {
        return _mouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Released;
    }

    public static bool IsLeftMouseButtonReleased() {
        return _mouseState.LeftButton == ButtonState.Released && _lastMouseState.LeftButton == ButtonState.Pressed;
    }
}