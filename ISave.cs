using Godot.Collections;

public interface ISave
{
    Dictionary Save();
	void Load(Dictionary data);
}
