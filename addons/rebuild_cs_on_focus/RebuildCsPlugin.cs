#if TOOLS
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using Godot;
namespace RebuildCsOnFocus.addons.rebuild_cs_on_focus;

[Tool]
public partial class RebuildCsPlugin : EditorPlugin
{
	RebuildOnFocusUi _rebuildOnFocusUi = default!;
	Callable _buildCallable;
	bool _enabled;
	bool _scanning;
	public override void _EnterTree()
	{
		AddUiControl();
		FindEditorBuildShortcut();
		ConnectEditorSignals();
	}
	
	public override void _ExitTree()
	{
		_enabled = false;
		_scanning = false;
		RemoveControlFromContainer(CustomControlContainer.Toolbar, _rebuildOnFocusUi);
		_rebuildOnFocusUi.QueueFree();
	}

	void ConnectEditorSignals()
	{
		GetTree().Root.Connect(Window.SignalName.FocusEntered, new Callable(this, MethodName.RootOnFocusEntered));
		GetEditorInterface().GetResourceFilesystem().Connect(EditorFileSystem.SignalName.ResourcesReload, new Callable(this, MethodName.OnResourcesReload));
	}

	void AddUiControl()
	{
		var dir = GetScript().As<CSharpScript>().ResourcePath.GetBaseDir();
		_rebuildOnFocusUi = GD.Load<PackedScene>($"{dir}/rebuild_on_focus.tscn").Instantiate<RebuildOnFocusUi>();
		_rebuildOnFocusUi.Connect(RebuildOnFocusUi.SignalName.Enabled, new Callable(this, MethodName.RebuildOnFocusUiOnEnabled));
		_rebuildOnFocusUi.Connect(RebuildOnFocusUi.SignalName.Disabled, new Callable(this, MethodName.RebuildOnFocusUiOnDisabled));
		AddControlToContainer(CustomControlContainer.Toolbar, _rebuildOnFocusUi);
	}

	void OnResourcesReload(string[] resources)
	{
		if (_scanning && resources.Any(res => res.GetFile().GetExtension() == "cs"))
		{
			_buildCallable.Call();
		}
	}

	void RootOnFocusEntered()
	{
		if (!_enabled) return;
		_scanning = true;
		GetEditorInterface().GetResourceFilesystem().ScanSources();
	}

	public override void _Process(double delta)
	{
		if (!_scanning) return;
		if (GetEditorInterface().GetResourceFilesystem().IsScanning()) return;
		_scanning = false;
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
}
#endif