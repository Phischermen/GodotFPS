using Godot;
using System.Collections.Generic;

public class GroundSound : Node
{
    private static SortedList<int, string> Priorities = new SortedList<int, string>();
    private static SortedList<string, int> UniqueKeys = new SortedList<string, int>();
    private static int UniqueKey = -1;

    [Export]
    public string Klass { get; private set; }
    [Export]
    public int Priority { get; private set; }
    [Export]
    public AudioStream StepSound;
    [Export]
    public AudioStream JumpSound;
    [Export]
    public AudioStream LandSound;

    public override void _EnterTree()
    {
        base._EnterTree();
        //Ensure that GroundSound has unique priority per klass
        if (!Priorities.ContainsKey(Priority))
        {
            Priorities.Add(Priority, Klass);
        }
        else if (Klass != Priorities[Priority])
        {
                if (!UniqueKeys.ContainsKey(Klass))
                {
                    GD.PushWarning("Two ground sounds were found with same priority. The Klasses are: " + Klass + " & " + Priorities[Priority]);
                    UniqueKeys.Add(Klass, UniqueKey);
                    Priorities.Add(UniqueKey, Klass);
                    UniqueKey--;
                }
                Priority = UniqueKeys[Klass];
        }
    }
}
