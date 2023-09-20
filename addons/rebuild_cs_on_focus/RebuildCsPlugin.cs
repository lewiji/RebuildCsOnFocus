#if TOOLS
using System.Linq;
using Godot;
namespace RebuildCsOnFocus.addons.rebuild_cs_on_focus;

[Tool]
public partial class RebuildCsPlugin : EditorPlugin
{
   RebuildOnFocusUi _rebuildOnFocusUi = default!;
   Callable _buildCallable;
   bool _enabled;
   bool _scanning;
   readonly StringName _msBuildPanelBuildMethod = new ("BuildProject");

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

      GetEditorInterface()
         .GetResourceFilesystem()
         .Connect(EditorFileSystem.SignalName.ResourcesReload, new Callable(this, MethodName.OnResourcesReload));
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
         GD.Print("Rebuilt .NET project");
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
      var node = new Control();
      AddControlToBottomPanel(node, "");
      var bottomBar = node.GetParent();
      RemoveControlFromBottomPanel(node);
      node.QueueFree();

      var msBuildPanel = bottomBar.GetChildren()
         .FirstOrDefault(c => c is VBoxContainer && c.HasMethod(_msBuildPanelBuildMethod));

      if (msBuildPanel != null)
      {
         _buildCallable = new Callable(msBuildPanel, _msBuildPanelBuildMethod);
      }
   }
}
#endif