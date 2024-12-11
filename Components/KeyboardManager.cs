using Microsoft.Xna.Framework.Input;


// https://community.monogame.net/t/one-shot-key-press/11669

// Semi-automatic attack
public class KeyboardManager
{
    private static KeyboardState _currentKeyState;
    private static KeyboardState _previousKeyState;

    public static void Update()
    {
        _previousKeyState = _currentKeyState;
        _currentKeyState = Keyboard.GetState();
    }

    public static bool IsPressed(Keys key)
    {
        return _currentKeyState.IsKeyDown(key);
    }

    public static bool HasBeenPressed(Keys key)
    {
        return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
    }
}
