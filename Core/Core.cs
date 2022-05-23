using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore.PoEMemory;
using ExileCore.RenderQ;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.VersionChecker;
using ExileCore.Threads;
using ImGuiNET;
using JM.LinqFaster;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SharpDX.Windows;
using Color = SharpDX.Color;

namespace ExileCore
{
    public class Core : IDisposable
    {
        private const int JOB_TIMEOUT_MS = 1000 / 5;
        private const int TICKS_BEFORE_SLEEP = 4;
        public static object SyncLocker = new object();
        private readonly DebugInformation _allPluginsDebugInformation;
        private readonly DebugInformation _coreDebugInformation;
        private readonly CoreSettings _coreSettings;
        private readonly DebugInformation _coroutineTickDebugInformation;
        private readonly DebugWindow _debugWindow;
        private readonly DebugInformation _deltaTimeDebugInformation;
        private readonly DX11 _dx11;
        private readonly RenderForm _form;
        private readonly DebugInformation _fpsCounterDebugInformation;
        private readonly DebugInformation _gcTickDebugInformation;
        private readonly WaitTime _mainControl = new WaitTime(2000);
        private readonly WaitTime _mainControl2 = new WaitTime(250);
        private readonly MenuWindow _mainMenu;
        private readonly DebugInformation _menuDebugInformation;
        private readonly DebugInformation _parallelCoroutineTickDebugInformation;
        private readonly SettingsContainer _settings;
        private readonly SoundController _soundController;
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private readonly DebugInformation _totalDebugInformation;
        private readonly List<(PluginWrapper plugin, Job job)> WaitingJobs = new List<(PluginWrapper, Job)>(20);
        private double _elTime = 1000 / 20f;
        private double _endParallelCoroutineTimer;
        private Memory _memory;
        private bool _memoryValid = true;
        private float _minimalFpsTime;
        private double _startParallelCoroutineTimer;
        private double _targetParallelFpsTime;
        private double _tickEnd;
        private double _tickStart;
        private double _tickStartCore;
        private double _timeSec;
        private int frameCounter;
        private Rectangle lastClientBound;
        private double lastCounterTime;
        private double NextCoroutineTime;
        private double NextRender;
        private int ticks;
        private double _targetPcFrameTime;
        private double _deltaTargetPcFrameTime;

        private void UpdateLogLevel(string level)
        {
            if (Enum.TryParse(level, false, out LogEventLevel logLevel))
            {
                DebugWindow.LogMsg($"Changing level to {level}");
                LoggingLevel.MinimumLevel = logLevel;
                ExileCore.Logger.LoggingLevel.MinimumLevel = logLevel;
            }
        }

        public Core(RenderForm form)
        {
            try
            {
                form.Load += (sender, args) =>
                {
                    var f = (RenderForm) sender;
                    WinApi.EnableTransparent(f.Handle);
                    WinApi.SetTransparent(f.Handle);
                };

                _coreDebugInformation = new DebugInformation("Core");
                _menuDebugInformation = new DebugInformation("Menu+Debug");
                _allPluginsDebugInformation = new DebugInformation("All plugins");
                _gcTickDebugInformation = new DebugInformation("GameController Tick");
                _coroutineTickDebugInformation = new DebugInformation("Coroutine Tick");
                _parallelCoroutineTickDebugInformation = new DebugInformation("Parallel Coroutine Tick");
                _fpsCounterDebugInformation = new DebugInformation("Fps counter", false);
                _deltaTimeDebugInformation = new DebugInformation("Delta Time", false);
                _totalDebugInformation = new DebugInformation("Total", "Total waste time");
                _form = form;
                FormHandle = _form.Handle;
                _settings = new SettingsContainer();
                _coreSettings = _settings.CoreSettings;

                this.UpdateLogLevel(this._coreSettings.LoggingLevel.Value);

                this._coreSettings.LoggingLevel.OnValueSelected += this.UpdateLogLevel;

                _coreSettings.Threads = new RangeNode<int>(_coreSettings.Threads.Value, 0, Environment.ProcessorCount);
                CoroutineRunner = new Runner("Main Coroutine");
                CoroutineRunnerParallel = new Runner("Parallel Coroutine");

                VersionChecker versionChecker;
                using (new PerformanceTimer("Check version"))
                {
                    versionChecker = new VersionChecker();
                    // check every ~100s for an update
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            if (versionChecker.AutoUpdate.IsDownloading)
                            {
                                DebugWindow.LogDebug("Core -> Currently downloading update, dont check again..."); 
                                Thread.Sleep(10 * 1000);
                                continue;
                            }
                            DebugWindow.LogDebug("Core -> Checking for update...");
                            versionChecker.CheckVersionAndPrepareUpdate(_coreSettings.AutoPrepareUpdate);
                            Thread.Sleep(100 * 1000);
                        }
                    });
                }

                DebugWindow.LogMsg($"Core -> Windows 10 is the only supported system!");

                using (new PerformanceTimer("DX11 Load"))
                {
                    _dx11 = new DX11(form, _coreSettings);
                }

                try
                {
                    _soundController = new SoundController("Sounds");
                    _coreSettings.Volume.OnValueChanged += (sender, i) => { _soundController.SetVolume(i / 100f); };
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"Core -> Loading SoundController failed.");
                    DebugWindow.LogError($"Core -> {e}");
                }

                _coreSettings.VSync.OnValueChanged += (obj, b) => { _dx11.VSync = _coreSettings.VSync.Value; };
                Graphics = new Graphics(_dx11, _coreSettings);

                MainRunner = CoroutineRunner;
                ParallelRunner = CoroutineRunnerParallel;

                // Task.Run(ParallelCoroutineRunner);
                var th = new Thread(ParallelCoroutineManualThread) {Name = "Parallel Coroutine", IsBackground = true};
                th.Start();
                _mainMenu = new MenuWindow(this, _settings, _dx11.ImGuiRender.fonts, ref versionChecker);
                _debugWindow = new DebugWindow(Graphics, _coreSettings);

                MultiThreadManager = new MultiThreadManager(_coreSettings.Threads);
                CoroutineRunner.MultiThreadManager = MultiThreadManager;

                _coreSettings.Threads.OnValueChanged += (sender, i) =>
                {
                    if (MultiThreadManager == null)
                        MultiThreadManager = new MultiThreadManager(i);
                    else
                    {
                        var coroutine1 =
                            new Coroutine(() => { MultiThreadManager.ChangeNumberThreads(_coreSettings.Threads); },
                                new WaitTime(2000), null, "Change Threads Number", false) {SyncModWork = true};

                        ParallelRunner.Run(coroutine1);
                    }
                };

                TargetPcFrameTime = 1000f / _coreSettings.TargetFps;
                _targetParallelFpsTime = 1000f / _coreSettings.TargetParallelFPS;
                _coreSettings.TargetFps.OnValueChanged += (sender, i) => { TargetPcFrameTime = 1000f / i; };
                _coreSettings.TargetParallelFPS.OnValueChanged += (sender, i) => { _targetParallelFpsTime = 1000f / i; };
                _minimalFpsTime = 1000f / _coreSettings.MinimalFpsForDynamic;
                _coreSettings.MinimalFpsForDynamic.OnValueChanged += (sender, i) => { _minimalFpsTime = 1000f / i; };

                _coreSettings.DynamicFPS.OnValueChanged += (sender, b) =>
                {
                    if (!b) TargetPcFrameTime = 1000f / _coreSettings.TargetFps;
                };

                if (_memory == null) _memory = FindPoe();

                if (GameController == null && _memory != null) Inject();

                var coroutine = new Coroutine(MainControl(), null, "Render control") { Priority = CoroutinePriority.Critical };

                CoroutineRunnerParallel.Run(coroutine);
                NextCoroutineTime = Time.TotalMilliseconds;
                NextRender = Time.TotalMilliseconds;
                if (pluginManager?.Plugins.Count == 0)
                {
                    _coreSettings.Enable.Value = true;
                }

                Graphics.InitImage("missing_texture.png");
            }
            catch (Exception e)
            {
                DebugWindow.LogMsg($"Core constructor -> {e}");
                MessageBox.Show($"Error in Core constructor -> {e}", "Oops... Program fail to launch");
            }
        }

        // Initialized in Loader.
        public static LoggingLevelSwitch LoggingLevel { get; set; }

        // Initialized in Loader.
        public static ILogger Logger { get; set; }

        public static Runner MainRunner { get; set; }
        public static Runner ParallelRunner { get; set; }
        public static uint FramesCount { get; private set; }

        public double TargetPcFrameTime
        {
            get => _targetPcFrameTime;
            private set
            {
                _targetPcFrameTime = value;
                _deltaTargetPcFrameTime =  value / 1000f;
            }
        }

        public MultiThreadManager MultiThreadManager { get; private set; }
        public static ObservableCollection<DebugInformation> DebugInformations { get; } =
            new ObservableCollection<DebugInformation>();
        public PluginManager pluginManager { get; private set; }
        private IntPtr FormHandle { get; }
        public Runner CoroutineRunner { get; set; }
        public Runner CoroutineRunnerParallel { get; set; }
        public GameController GameController { get; private set; }
        public Graphics Graphics { get; }
        public bool IsForeground => WinApi.IsForegroundWindow(_memory.Process.MainWindowHandle)
            || WinApi.IsForegroundWindow(FormHandle)
            || _coreSettings.ForceForeground;


        public void Dispose()
        {
            _memory?.Dispose();
            _mainMenu?.Dispose();
            GameController?.Dispose();
            _dx11?.Dispose();
            pluginManager?.CloseAllPlugins();
        }

        private IEnumerator MainControl()
        {
            while (true)
            {
                if (_memory == null)
                {
                    _memory = FindPoe();
                    if (_memory == null) yield return _mainControl;
                    continue;
                }

                if (GameController == null && _memory != null)
                {
                    Inject();
                    if (GameController == null) yield return _mainControl;
                    continue;
                }

                var clientRectangle = WinApi.GetClientRectangle(_memory.Process.MainWindowHandle);

                if (lastClientBound != clientRectangle && _form.Bounds != clientRectangle &&
                    clientRectangle.Width > 2 &&
                    clientRectangle.Height > 2)
                {
                    DebugWindow.LogDebug($"Resize from: {lastClientBound} to {clientRectangle}");
                    lastClientBound = clientRectangle;
                    _form.Invoke(new Action(() => { _form.Bounds = clientRectangle; }));
                }

                _memoryValid = !_memory.IsInvalid();

                if (!_memoryValid)
                {
                    GameController.Dispose();
                    GameController = null;
                    _memory = null;
                }
                else
                {
                    GameController.IsForeGroundCache = IsForeground;
                }

                yield return _mainControl2;
            }
        }

        public static Memory FindPoe()
        {
            var pid = FindPoeProcess();

            if (!pid.HasValue || pid.Value.process.Id == 0)
                DebugWindow.LogError("Game not found");
            else
                return new Memory(pid.Value);

            return null;
        }

        private void Inject()
        {
            try
            {
                if (_memory != null)
                {
                    GameController = new GameController(_memory, _soundController, _settings, MultiThreadManager);
                    lastClientBound = _form.Bounds;

                    using (new PerformanceTimer("Plugin loader"))
                    {
                        pluginManager = new PluginManager(
                            GameController, 
                            Graphics, 
                            MultiThreadManager,
                            _settings
                        );
                    }
                }
            }
            catch (Exception e)
            {
                DebugWindow.LogError($"Inject -> {e}");
            }
        }

        public ThreadManager ThreadManager { get; } = new ThreadManager();

        public void Tick()
        {
            try
            {
                Input.Update(FormHandle);
                _tickStartCore = _sw.Elapsed.TotalMilliseconds;
                FramesCount++;

                if (!IsForeground) return;

                try
                {
                    _debugWindow.Render();
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"DebugWindow Tick -> {e}");
                }

                try
                {
                    _mainMenu.Render(GameController, pluginManager?.Plugins);
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"Core Tick Menu -> {e}");
                }

                _tickEnd = _sw.Elapsed.TotalMilliseconds;
                _menuDebugInformation.Tick = _tickEnd - _tickStartCore;

                if (GameController == null || pluginManager == null || !pluginManager.AllPluginsLoaded)
                {
                    _coreDebugInformation.Tick = (float) (_sw.Elapsed.TotalMilliseconds - _tickStart);
                    return;
                }

                _timeSec += GameController.DeltaTime;

                if (_timeSec >= 1000)
                {
                    _timeSec = 0;

                    if (_coreSettings.DynamicFPS)
                    {
                        var fpsArray = GameController.IngameState.FPSRectangle.DiagnosticArrayValues;

                        var dynamicFps = 1000f / (fpsArray.SkipF((int) (fpsArray.Length * 0.75f)).AverageF() *
                                                  (_coreSettings.DynamicPercent / 100f));

                        TargetPcFrameTime = Math.Min(_minimalFpsTime, dynamicFps);
                    }
                }

                _tickStart = _sw.Elapsed.TotalMilliseconds;
                GameController.Tick();
                _tickEnd = _sw.Elapsed.TotalMilliseconds;
                _gcTickDebugInformation.Tick = (float) (_tickEnd - _tickStart);

                _tickStart = _sw.Elapsed.TotalMilliseconds;

                if (pluginManager != null)
                {
                    ThreadManager.AbortLongRunningThreads();
                    var pluginTickJobs = new List<Job>();

                    // Plugin Tick
                    foreach (var plugin in pluginManager?.Plugins)
                    {
                        if (!plugin.IsEnable) continue;
                        if (!GameController.InGame && !plugin.Force) continue;
                        plugin.CanRender = true;

                        var job = (_coreSettings.CollectDebugInformation?.Value == true) 
                            ? plugin.PerfomanceTick() 
                            : plugin.Tick();

                        if (job == null) continue;
                        pluginTickJobs.Add(job);
                        ThreadManager.AddOrUpdateJob($"Plugin_Tick_{plugin.Name}", job);
                    }

                    SpinWait.SpinUntil(() => pluginTickJobs.AllF(job => job.IsCompleted), JOB_TIMEOUT_MS);

                    // Plugin Render
                    foreach (var plugin in pluginManager?.Plugins)
                    {
                        if (!plugin.IsEnable) continue;
                        if (!plugin.CanRender) continue;
                        if (!GameController.InGame && !plugin.Force) continue;

                        if (_coreSettings.CollectDebugInformation?.Value == true)
                        {
                            plugin.PerfomanceRender();
                        }
                        else
                        {
                            plugin.Render();
                        }
                    }
                }

                _tickEnd = _sw.Elapsed.TotalMilliseconds;
                _allPluginsDebugInformation.Tick = (float) (_tickEnd - _tickStart);
                _coreDebugInformation.Tick = (float) (_tickEnd - _tickStartCore);
            }
            catch (Exception ex)
            {
                DebugWindow.LogError($"Core tick -> {ex}");
            }
        }

        private static int ChooseSingleProcess(List<(Process, Offsets)> clients)
        {
            var o1 =
                $"Yes - process #{clients[0].Item1.Id}, started at {clients[0].Item1.StartTime.ToLongTimeString()}";

            var o2 = $"No - process #{clients[1].Item1.Id}, started at {clients[1].Item1.StartTime.ToLongTimeString()}";
            const string o3 = "Cancel - quit this application";

            var answer = MessageBox.Show(null, string.Join(Environment.NewLine, o1, o2, o3),
                "Choose a PoE instance to attach to",
                MessageBoxButtons.YesNoCancel);

            return answer == DialogResult.Cancel ? -1 : answer == DialogResult.Yes ? 0 : 1;
        }

        private static (Process process, Offsets offsets)? FindPoeProcess()
        {
            var clients = Process.GetProcessesByName(Offsets.Regular.ExeName).Select(x => (x, Offsets.Regular))
                .ToList();

            clients.AddRange(Process.GetProcessesByName(Offsets.Korean.ExeName).Select(p => (p, Offsets.Korean)));
            var ixChosen = clients.Count > 1 ? ChooseSingleProcess(clients) : 0;

            if (clients.Count > 0)
                return clients[ixChosen];

            return null;
        }

        private void ParallelCoroutineManualThread()
        {
            try
            {
                while (true)
                {
                    MultiThreadManager?.Process(this);
                    _startParallelCoroutineTimer = _sw.Elapsed.TotalMilliseconds;

                    if (CoroutineRunnerParallel.IsRunning)
                    {
                        try
                        {
                            for (var i = 0; i < CoroutineRunnerParallel.IterationPerFrame; i++)
                            {
                                CoroutineRunnerParallel.Update();
                            }
                        }
                        catch (Exception e)
                        {
                            DebugWindow.LogError($"Coroutine Parallel error: {e.Message}");
                        }
                    }
                    else
                        Thread.Sleep(10);

                    _endParallelCoroutineTimer = _sw.Elapsed.TotalMilliseconds;
                    _elTime = _endParallelCoroutineTimer - _startParallelCoroutineTimer;

                    _parallelCoroutineTickDebugInformation.Tick = _elTime;

                    if (_elTime < _targetParallelFpsTime)
                    {
                        var millisecondsDelay = _targetParallelFpsTime - _elTime;
                        Thread.Sleep((int) millisecondsDelay);
                    }
                }
            }
            catch (Exception e)
            {
                DebugWindow.LogError($"Coroutine Parallel error: {e.Message}");
                throw;
            }
        }

        public void Render()
        {
            var startTime = Time.TotalMilliseconds;
            _tickStart = _sw.Elapsed.TotalMilliseconds;

            ticks++;

            if (NextCoroutineTime <= Time.TotalMilliseconds)
            {
                NextCoroutineTime += _targetParallelFpsTime;

                if (CoroutineRunner.IsRunning)
                {
                    ThreadManager.AddOrUpdateJob(new Job("CoroutineRunner", CoroutineRunner.Update));
                    //if (_coreSettings.CoroutineMultiThreading)
                    //    CoroutineRunner.ParallelUpdate();
                    //else
                    //    CoroutineRunner.Update();
                }

                _tickEnd = _sw.Elapsed.TotalMilliseconds;
                _coroutineTickDebugInformation.Tick = (float) (_tickEnd - _tickStart);
            }


            if (NextRender <= Time.TotalMilliseconds)
            {
                _dx11.ImGuiRender.InputUpdate(_totalDebugInformation.Tick*_deltaTargetPcFrameTime);
                _dx11.Render(TargetPcFrameTime, this);
                NextRender += TargetPcFrameTime;
                frameCounter++;
                WaitRender.Frame();

                if (Time.TotalMilliseconds - lastCounterTime > 1000)
                {
                    _fpsCounterDebugInformation.Tick = frameCounter;
                    _deltaTimeDebugInformation.Tick = 1000f / frameCounter;
                    lastCounterTime = Time.TotalMilliseconds;
                    frameCounter = 0;
                }

                _totalDebugInformation.Tick = Time.TotalMilliseconds - startTime;
            }
            else
            {
                if (ticks >= TICKS_BEFORE_SLEEP)
                {
                    Thread.Sleep(1);
                    ticks = 0;
                }
            }

            var elTime = Time.TotalMilliseconds - startTime;
        }

        public void FixImGui()
        {
            WinApi.SetNoTransparent(_form.Handle);
            ImGui.CaptureMouseFromApp();
            ImGui.CaptureKeyboardFromApp();
        }
    }
}
