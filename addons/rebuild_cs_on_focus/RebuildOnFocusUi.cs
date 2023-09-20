#if TOOLS
using Godot;

namespace RebuildCsOnFocus.addons.rebuild_cs_on_focus;

[Tool]
public partial class RebuildOnFocusUi : Control
{
	[Signal] public delegate void EnabledEventHandler();
	[Signal] public delegate void DisabledEventHandler();
	
	CheckBox _checkBox = default!;
	ConfigFile _config = new ();
	string _configPath = "user://rebuild_on_focus.cfg";
	
	public override void _Ready()
	{
		LoadOrCreateConfigFile();
		SetupCheckBox();
	}

	void SetupCheckBox()
	{
		_checkBox = GetNode<CheckBox>("CheckBox");
		_checkBox.ButtonPressed = _config.GetValue("rebuild_on_focus", "enabled").AsBool();
		_checkBox.Connect(BaseButton.SignalName.Toggled, new Callable(this, MethodName.CheckBoxOnToggled));
		CheckBoxOnToggled(_checkBox.ButtonPressed);
	}

	void LoadOrCreateConfigFile()
	{
		if (FileAccess.FileExists(_configPath))
		{
			_config.Load(_configPath);
		}
		else
		{
			SaveEnabledSetting(true);
		}
	}

	void SaveEnabledSetting(bool enabled)
	{
		_config.SetValue("rebuild_on_focus", "enabled", enabled);
		_config.Save(_configPath);
	}

	void CheckBoxOnToggled(bool pressed)
	{
		EmitSignal(pressed ? SignalName.Enabled : SignalName.Disabled);
		SaveEnabledSetting(pressed);
	}
}
#endif