using Godot;

public static class IX
{
    public static void CheckAndAddAction(string action, KeyList key, bool ctrl = false, bool alt = false, bool shift = false, bool cmd = false)
    {
        if (!InputMap.HasAction(action))
        {
            //Tell user that input was added
            string message = action + " not found in Input Map. Action was added and set to ";
            if (ctrl || alt || shift || cmd) message += (ctrl ? "ctrl + " : "") + (alt ? "alt + " : "") + (shift ? "shift + " : "") + (cmd ? "cmd + " : "");
            message += key.ToString();
            GD.Print(message);

            //Add the action
            InputMap.AddAction(action);

            //Create the event
            InputEventWithModifiers inputEventWithModifiers = new InputEventKey
            {
                Scancode = (uint)key
            };
            inputEventWithModifiers.Control = ctrl;
            inputEventWithModifiers.Alt = alt;
            inputEventWithModifiers.Shift = shift;
            inputEventWithModifiers.Command = cmd;

            //Add event to action
            InputMap.ActionAddEvent(action, inputEventWithModifiers);
        }
    }
}
