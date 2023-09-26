#if TOOLS
using Godot;
using Godot.Collections;

namespace RebuildCsOnFocus.addons.rebuild_cs_on_focus;

[Tool]
public partial class RebuildOnFocusUi : Control
{
	public enum MenuOptions
	{
		RebuildOnFocus,
		BuildOnPlay
	}
	
	[Signal] public delegate void SettingChangedEventHandler(int option, bool enabled);
	
	PopupMenu _popupMenu = default!;
	ConfigFile _config = new ();
	string _configPath = "user://rebuild_on_focus.cfg";
	Dictionary<MenuOptions, string> _configKeys = new () {
		{MenuOptions.RebuildOnFocus, "rebuild_on_focus"}, 
		{MenuOptions.BuildOnPlay, "build_on_play"}
	};
	
	public override void _Ready()
	{
		LoadOrCreateConfigFile();
		InitPopupMenu();
	}

	void InitPopupMenu()
	{
		_popupMenu = GetNode<MenuButton>("MenuButton").GetPopup();
		InitPopupMenuItem(MenuOptions.RebuildOnFocus);
		InitPopupMenuItem(MenuOptions.BuildOnPlay);
		_popupMenu.Connect(PopupMenu.SignalName.IdPressed, new Callable(this, MethodName.MenuIdPressed));
	}

	void InitPopupMenuItem(MenuOptions option)
	{
		var enabled = LoadEnabledSetting(option);
		_popupMenu.SetItemChecked(_popupMenu.GetItemIndex((int)option), enabled);
		EmitSignal(SignalName.SettingChanged, (int)option, enabled);
	}

	void LoadOrCreateConfigFile()
	{
		if (FileAccess.FileExists(_configPath))
		{
			_config.Load(_configPath);
		}
		else
		{
			SaveEnabledSetting(MenuOptions.RebuildOnFocus, true);
			SaveEnabledSetting(MenuOptions.BuildOnPlay, true);
		}
	}

	bool LoadEnabledSetting(MenuOptions option)
	{
		return _config.GetValue(_configKeys[option], "enabled", true).AsBool();
	}

	void SaveEnabledSetting(MenuOptions option, bool enabled)
	{
		_config.SetValue(_configKeys[option], "enabled", enabled);
		_config.Save(_configPath);
	}

	void MenuIdPressed(int id)
	{
		var itemIndex = _popupMenu.GetItemIndex(id);
		_popupMenu.ToggleItemChecked(itemIndex);
		var pressed = _popupMenu.IsItemChecked(itemIndex);
		SaveEnabledSetting((MenuOptions)id, pressed);
		EmitSignal(SignalName.SettingChanged, id, pressed);
	}
}
#endif