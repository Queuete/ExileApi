using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using JM.LinqFaster;
using SharpDX;

namespace ExileCore.Shared
{
    public class PluginWrapper
    {
        private static readonly Stopwatch sw = Stopwatch.StartNew();
        private double startTick;
        public DateTime LastWrite { get; set; } = DateTime.MinValue;
        public PluginWrapper(IPlugin plugin)
        {
            Plugin = plugin;
            TickDebugInformation = new DebugInformation($"{Name} [P]", "plugin");
            RenderDebugInformation = new DebugInformation($"{Name} [R]", "plugin");
            
        }

        public double LoadedTime { get; set; }
        public double InitialiseTime { get; private set; }
        public bool Force => Plugin.Force;
        public string Name => Plugin.Name;
        public int Order => Plugin.Order;
        public IPlugin Plugin { get; private set; }
        public bool CanRender { get; set; }
        public bool CanBeMultiThreading => Plugin.CanUseMultiThreading;
        public DebugInformation TickDebugInformation { get; }
        public DebugInformation RenderDebugInformation { get; }
        public bool IsEnable => Plugin._Settings.Enable;

        public void CorrectThisTick(float val)
        {
            TickDebugInformation.CorrectAfterTick(val);
        }

        public void Onload()
        {
            try
            {
                Plugin.OnLoad();
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public void Initialise(GameController _gameController)
        {
            try
            {
                if (Plugin._Settings == null)
                {
                  throw new NullReferenceException($"Cant load plugin ({Plugin.Name}) because settings is null.");
                }

                if (Plugin.Initialized)
                {
                    throw new InvalidOperationException($"Already initialized.");
                }

                if (Plugin._Settings.Enable)
                {
                    Plugin.Initialized = PluginInitialise();
                    if (!Plugin.Initialized)
                    {
                        Plugin._Settings.Enable.Value = false;
                    }
                }

                Plugin._Settings.Enable.OnValueChanged += (obj, isEnabled) =>
                {
                    try
                    {
                        if (Plugin.Initialized)
                        {
                            var coroutines = Core.MainRunner.Coroutines.Concat(Core.ParallelRunner.Coroutines)
                                .Where(x => x.OwnerName == Plugin.InternalName).ToList();

                            if (isEnabled)
                            {
                                foreach (var coroutine in coroutines)
                                {
                                    coroutine.Resume();
                                }
                            }
                            else
                            {
                                foreach (var coroutine in coroutines)
                                {
                                    coroutine.Pause();
                                }
                            }
                        }
                        else if (isEnabled)
                        {
                            Plugin.Initialized = PluginInitialise();
                            if (Plugin.Initialized)
                            {
                                Plugin.AreaChange(_gameController.Area.CurrentArea);
                            }
                            else
                            {
                                Plugin._Settings.Enable.Value = false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                    }
                };
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        private bool PluginInitialise()
        {
            var sw = Stopwatch.StartNew();
            var initialise = Plugin.Initialise();
            sw.Stop();
            if (initialise)
            {
                var elapsedTotalMilliseconds = sw.Elapsed.TotalMilliseconds;
                InitialiseTime = elapsedTotalMilliseconds;
                DebugWindow.LogDebug($"{Name} -> Initialise time: {elapsedTotalMilliseconds} ms.");
            }

            return initialise;

        }
        
        public void TurnOnOffPlugin(bool state)
        {
            Plugin._Settings.Enable.Value = state;
        }

        public void AreaChange(AreaInstance area)
        {
            try
            {
                Plugin.AreaChange(area);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public Job PerfomanceTick()
        {
            try
            {
                startTick = sw.Elapsed.TotalMilliseconds;
                var tick = Plugin.Tick();
                TickDebugInformation.Tick = sw.Elapsed.TotalMilliseconds - startTick;
                return tick;
            }
            catch (Exception e)
            {
                LogError(e);
                return null;
            }
        }

        public Job Tick()
        {
            try
            {
                return Plugin.Tick();
            }
            catch (Exception e)
            {
                LogError(e);
                return null;
            }
        }

        public void PerfomanceRender()
        {
            try
            {
                startTick = sw.Elapsed.TotalMilliseconds;
                Plugin.Render();
                RenderDebugInformation.Tick = sw.Elapsed.TotalMilliseconds - startTick;
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public void Render()
        {
            try
            {
                Plugin.Render();
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        private void LogError(Exception e, [CallerMemberName] string methodName = null)
        {
            var msg = $"{Plugin.Name}, {methodName} -> {e}";
            DebugWindow.LogError(msg, 3);
        }

        public void EntityIgnored(Entity entity)
        {
            try
            {
                Plugin.EntityIgnored(entity);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public void EntityAddedAny(Entity entity)
        {
            try
            {
                Plugin.EntityAddedAny(entity);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public void EntityAdded(Entity entity)
        {
            try
            {
                Plugin.EntityAdded(entity);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public void EntityRemoved(Entity entity)
        {
            try
            {
                Plugin.EntityRemoved(entity);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public void ReceiveEvent(string eventId, object args)
        {
            try
            {
                Plugin.ReceiveEvent(eventId, args);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public void SetApi(GameController gameController, Graphics graphics, PluginManager pluginManager)
        {
            Plugin.SetApi(gameController, graphics, pluginManager);
        }

        public void LoadSettings()
        {
            Plugin._LoadSettings();
        }

        public void Close()
        {
            try
            {
                Plugin._SaveSettings();
                Plugin.OnClose();
                Plugin.OnUnload();
                Plugin.Dispose();
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public void DrawSettings()
        {
            try
            {
                Plugin.DrawSettings();
            }
            catch (Exception e)
            {
                DebugWindow.LogError($"PluginWrapper -> DrawSettings for {Plugin?.Name} failed");
                DebugWindow.LogError($"PluginWrapper -> {e.Message}");
            }
        }

        public void ReloadPlugin(IPlugin plugin, GameController gameController, Graphics graphics, PluginManager pluginManager)
        {
            var mainRunner = Core.MainRunner;
            var parallelRunner = Core.ParallelRunner;
            var coroutines = mainRunner.Coroutines.Where(x=>x.Owner==Plugin).Concat(parallelRunner.Coroutines.Where(x=>x.Owner==Plugin)).ToList();
            foreach (var coroutine in coroutines)
            {
                coroutine.Done();
            }

            Close();
            plugin.SetApi(gameController,graphics, pluginManager);
            plugin.DirectoryName = Plugin.DirectoryName;
            plugin.DirectoryFullName = Plugin.DirectoryFullName;
            plugin._LoadSettings();
            Plugin.OnPluginDestroyForHotReload();
            Plugin = plugin;
            Onload();
            Initialise(gameController);

            foreach (var gameControllerEntity in gameController.Entities)
            {
                EntityAdded(gameControllerEntity);
            }
        }

        public override string ToString()
        {
            return $"{Name} [{Order}]";
        }
    }
}
