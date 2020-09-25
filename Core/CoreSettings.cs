using System.Collections.Generic;
using System.Windows.Forms;
using ExileCore.RenderQ;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace ExileCore
{
    public class CoreSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);


        #region main
        [Menu("Main", 1000)]
        public EmptyNode EmptyMain { get; set; } = new EmptyNode();
        [Menu("Refresh area", 1, 1000)]
        public ButtonNode RefreshArea { get; set; } = new ButtonNode();
        [Menu("List profiles", "Currently not works. Soon.", 2, 1000)]
        public ListNode Profiles { get; set; } = new ListNode { Values = new List<string> { "global" }, Value = "global" };
        [Menu("Menu Key Toggle", 3, 1000)]
        public HotkeyNode MainMenuKeyToggle { get; set; } = Keys.F12;
        [Menu("Force Foreground", 4, 1000)]
        public ToggleNode ForceForeground { get; set; } = new ToggleNode(false);
        [Menu("Automatically Download Update", 5, 1000)]
        public ToggleNode AutoPrepareUpdate { get; set; } = new ToggleNode(true);
        #endregion

        #region messages
        [Menu("Messages", 2000)]
        public EmptyNode EmptyMessages { get; set; } = new EmptyNode();
        [Menu("Show Information Messages", "Will always show in log window", 1, 2000)]
        public ToggleNode ShowInformationMessages { get; set; } = new ToggleNode(true);
        [Menu("Show Error Messages", "Will always show in log window", 2, 2000)]
        public ToggleNode ShowErrorMessages { get; set; } = new ToggleNode(true);
        [Menu("Show Debug Messages", "(De)activates entries in log window aswell", 3, 2000)]
        public ToggleNode ShowDebugMessages { get; set; } = new ToggleNode(false);
        [Menu("Show Log Window", 4, 2000)]
        public ToggleNode ShowDebugLog { get; set; } = new ToggleNode(false);
        [Menu("Show Debug Window", 5, 2000)]
        public ToggleNode ShowDebugWindow { get; set; } = new ToggleNode(false);
        [Menu("Debug Information", "With this option you can check how much every plugin works.", 6, 2000)]
        public ToggleNode CollectDebugInformation { get; set; } = new ToggleNode(false);
        [Menu("Log Read Memory Errors", 7, 2000)]
        public ToggleNode LogReadMemoryError { get; set; } = new ToggleNode(false);
        #endregion

        #region performance
        [Menu("Performance", 3000)]
        public EmptyNode EmptyPerfomance { get; set; } = new EmptyNode();
        [Menu("Threads count", 1, 3000)]
        public RangeNode<int> Threads { get; set; } = new RangeNode<int>(1, 0, 4);
        [Menu("HUD VSync", 2, 3000)]
        public ToggleNode VSync { get; set; } = new ToggleNode(false);
        [Menu("Dynamic FPS", "Hud FPS like FPS game", 3, 3000)]

        public ToggleNode DynamicFPS { get; set; } = new ToggleNode(false);
        [Menu("Percent from game FPS", 4, 3000)]
        public RangeNode<int> DynamicPercent { get; set; } = new RangeNode<int>(100, 1, 150);
        [Menu("Minimal FPS when dynamic", 5, 3000)]
        public RangeNode<int> MinimalFpsForDynamic { get; set; } = new RangeNode<int>(60, 10, 150);

        [Menu("Target FPS", 6, 3000)]
        public RangeNode<int> TargetFps { get; set; } = new RangeNode<int>(60, 5, 200);
        [Menu("Target Parallel Coroutine Fps", 7, 3000)]
        public RangeNode<int> TargetParallelFPS { get; set; } = new RangeNode<int>(60, 30, 500);
        [Menu("Entites FPS", "How need often update entities. You can see in DebugWindow->Coroutines time what spent for that work.", 8, 3000)]
        public RangeNode<int> EntitiesUpdate { get; set; } = new RangeNode<int>(60, 5, 200);
        [Menu("Parse server entities", 10, 3000)]
        public ToggleNode ParseServerEntities { get; set; } = new ToggleNode(false);
        [Menu("Collect entities in parallel when more than X", 11, 3000)]
        public ToggleNode CollectEntitiesInParallelWhenMoreThanX { get; set; } = new ToggleNode(false);
        [Menu("Limit draw plot in ms",
            "Don't put small value, because plot need a lot triangles and DebugWindow with a lot plot will be broke.", 12, 3000)]
        public RangeNode<float> LimitDrawPlot { get; set; } = new RangeNode<float>(0.2f, 0.05f, 20f);
        [Menu("Message if plugin render work more than X ms", 13, 3000)]
        public RangeNode<int> CriticalTimeForPlugin { get; set; } = new RangeNode<int>(100, 1, 2000);
        #endregion

        #region multithread
        [Menu("Multithread", 4000)]
        public EmptyNode EmptyMultithread { get; set; } = new EmptyNode();
        [Menu("Load Plugins Multithread", 1, 4000)]
        public ToggleNode MultiThreadLoadPlugins { get; set; } = new ToggleNode(false);
        [Menu("Coroutine Multithread", "", 2, 4000)]
        public ToggleNode CoroutineMultiThreading { get; set; } = new ToggleNode(true);
        [Menu("Added Entities Multithread", 3, 4000)]
        public ToggleNode AddedMultiThread { get; set; } = new ToggleNode(false);
        [Menu("Parse Entities Multithread", 4, 4000)]
        public ToggleNode ParseEntitiesInMultiThread { get; set; } = new ToggleNode(false);
        #endregion

        #region miscellaneous
        [Menu("Miscellaneous", 5000)]
        public EmptyNode EmptyMiscellaneous { get; set; } = new EmptyNode();
        [Menu("Font", 1, 5000)]
        public ListNode Font { get; set; } = new ListNode {Values = new List<string> {"Not found"}};
        [IgnoreMenu] // "Currently not works. Because this option broke calculate how much pixels needs for render."
        public RangeNode<int> FontSize { get; set; } = new RangeNode<int>(13, 7, 36);
        [Menu("Font Spacing", 2, 5000)]
        public RangeNode<float> FontSpacing { get; set; } = new RangeNode<float>(1, 0.5f, 2);
        [Menu("Volume", 3, 5000)]
        public RangeNode<int> Volume { get; set; } = new RangeNode<int>(100, 0, 100);
        #endregion
    }
}
