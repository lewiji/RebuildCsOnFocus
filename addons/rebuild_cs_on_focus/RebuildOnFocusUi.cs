#if TOOLS
using Godot;

namespace RebuildCsOnFocus.addons.rebuild_cs_on_focus;

[Tool]
public partial class RebuildOnFocusUi : Control
{
	[Signal] public delegate void EnabledEventHandler();
	[Signal] public delegate void DisabledEventHandler();
	CheckBox _checkBox = default!;
	public override void _Ready()
	{
		_checkBox = GetNode<CheckBox>("CheckBox");
		_checkBox.Toggled += CheckBoxOnToggled;
		CheckBoxOnToggled(_checkBox.ButtonPressed);
	}

	void CheckBoxOnToggled(bool pressed)
	{
		EmitSignal(pressed ? SignalName.Enabled : SignalName.Disabled);
	}
}
#endif