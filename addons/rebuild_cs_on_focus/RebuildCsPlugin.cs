#if TOOLS
using System.Linq;
using Godot;
namespace RebuildCsOnFocus.addons.rebuild_cs_on_focus;

[Tool]
public partial class RebuildCsPlugin : EditorPlugin
{
	RebuildOnFocusUi _rebuildOnFocusUi = default!;
	Callable? _buildCallable;
	bool _enabled;
	bool _scanning;
	public override void _EnterTree()
	{
		AddUiControl();
		FindEditorBuildShortcut();
		ConnectSignals();
	}

	void ConnectSignals()
	{
		_rebuildOnFocusUi.Enabled += RebuildOnFocusUiOnEnabled;
		_rebuildOnFocusUi.Disabled += RebuildOnFocusUiOnDisabled;
		GetTree().Root.FocusEntered += RootOnFocusEntered;
		GetEditorInterface().GetResourceFilesystem().ResourcesReload += OnResourcesReload;
	}
	
	void DisconnectSignals()
	{
		_rebuildOnFocusUi.Enabled -= RebuildOnFocusUiOnEnabled;
		_rebuildOnFocusUi.Disabled -= RebuildOnFocusUiOnDisabled;
		GetTree().Root.FocusEntered -= RootOnFocusEntered;
		GetEditorInterface().GetResourceFilesystem().ResourcesReload -= OnResourcesReload;
	}

	void AddUiControl()
	{
		var dir = GetScript().As<CSharpScript>().ResourcePath.GetBaseDir();
		_rebuildOnFocusUi = GD.Load<PackedScene>($"{dir}/rebuild_on_focus.tscn").Instantiate<RebuildOnFocusUi>();
		AddControlToContainer(CustomControlContainer.Toolbar, _rebuildOnFocusUi);
	}

	void OnResourcesReload(string[] resources)
	{
		if (_scanning && resources.Any(res => res.GetFile().GetExtension() == "cs"))
		{
			_buildCallable?.Call();
		}
	}

	void RootOnFocusEntered()
	{
		if (!_enabled) return;
		_scanning = true;
		GetEditorInterface().GetResourceFilesystem().ScanSources();
	}

	void RebuildOnFocusUiOnEnabled()
	{
		_enabled = true;
	}
	
	void RebuildOnFocusUiOnDisabled()
	{
		_enabled = false;
	}

	// Hacky way to fetch build button shortcut, since API access to build system or editor shortcuts isn't exposed
	void FindEditorBuildShortcut()
	{
		var toolbarNodes = _rebuildOnFocusUi.GetParent().GetChildren();
		foreach (var toolbarNode in toolbarNodes)
		{
			if (toolbarNode is not Button {Visible: true} button) continue;
			
			var signalConnectionList = button.GetSignalConnectionList(BaseButton.SignalName.Pressed);

			foreach (var dictionary in signalConnectionList)
			{
				var connection = dictionary["callable"].AsCallable();
				if (connection.Delegate.Method.Name != "BuildProjectPressed") continue;
				_buildCallable = connection;
				break;
			}
		}
	}

	public override void _ExitTree()
	{
		_buildCallable = null;
		_enabled = false;
		_scanning = false;
		DisconnectSignals();
		RemoveControlFromContainer(CustomControlContainer.Toolbar, _rebuildOnFocusUi);
	}
}
#endif
