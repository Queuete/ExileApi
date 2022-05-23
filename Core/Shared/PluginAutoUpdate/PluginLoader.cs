using ExileCore.Shared.Interfaces;
using JM.LinqFaster;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExileCore.Shared.PluginAutoUpdate
{
    public class PluginLoader
    {
        private GameController GameController { get; }
        private Graphics Graphics { get; }
        private PluginManager PluginManager { get; }
        private ConcurrentDictionary<string, Stopwatch> PluginLoadTime { get; }

        public PluginLoader(GameController gameController, Graphics graphics, PluginManager pluginManager)
        {
            GameController = gameController ?? throw new ArgumentNullException(nameof(gameController));
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            PluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
            PluginLoadTime = new ConcurrentDictionary<string, Stopwatch>();
        }


        public List<PluginWrapper> Load(DirectoryInfo info)
        {
            if (info == null) return null;
            var assembly =  LoadAssembly(info);
            if (assembly == null) return null;
            PluginLoadTime.TryAdd(info.FullName, Stopwatch.StartNew());

            return TryLoadPlugin(assembly, info);
        }

        private Assembly LoadAssembly(DirectoryInfo dir)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(dir.FullName);
                if (!directoryInfo.Exists)
                {
                    DebugWindow.LogError($"Directory - {dir} not found.");
                    return null;
                }

                var dll = directoryInfo.GetFiles($"{directoryInfo.Name}*.dll", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();

                if (dll == null)
                {
                    dll = directoryInfo.GetFiles("*.dll", SearchOption.TopDirectoryOnly).SingleOrDefault();
                }

                if (dll == null)
                {
                    DebugWindow.LogError($"PluginLoader -> No found plugin dll in \"{dir.FullName}\".");
                    DebugWindow.LogError("PluginLoader -> Dll should be named like folder or only dll in folder.");                    
                    return null;
                }
                
                var asm = Assembly.LoadFrom(dll.FullName);
                return asm;
            }
            catch (Exception e)
            {
                DebugWindow.LogDebug($"{nameof(LoadAssembly)} -> Failed to load \"{dir.FullName}\".");
                DebugWindow.LogError($"{nameof(LoadAssembly)}() -> {e}");
                return null;
            }
        }

        private List<PluginWrapper> TryLoadPlugin(Assembly assembly, DirectoryInfo directoryInfo)
        {
            var pluginWrappers = new List<PluginWrapper>();
            try
            {
                var dir = assembly.ManifestModule.ScopeName.Replace(".dll", "");

                var fullPath = directoryInfo.FullName;

                var types = assembly.GetTypes();
                if (types.Length == 0)
                {
                    DebugWindow.LogError($"Not found any types in plugin {fullPath}");
                    return null;
                }

                var classesWithIPlugin = types.WhereF(type => typeof(IPlugin).IsAssignableFrom(type));
                var settings = types.FirstOrDefaultF(type => typeof(ISettings).IsAssignableFrom(type));

                if (settings == null)
                {
                    DebugWindow.LogError("Not found setting class");
                    return null;
                }


                foreach (var type in classesWithIPlugin)
                {
                    if (type.IsAbstract) continue;

                    if (Activator.CreateInstance(type) is IPlugin instance)
                    {
                        instance.DirectoryName = dir;
                        instance.DirectoryFullName = fullPath;
                        var pluginWrapper = new PluginWrapper(instance);
                        pluginWrapper.SetApi(GameController, Graphics, PluginManager);
                        pluginWrapper.LoadSettings();
                        pluginWrapper.Onload();
                        var sw = PluginLoadTime[directoryInfo.FullName];
                        sw.Stop();
                        var elapsedTotalMilliseconds = sw.Elapsed.TotalMilliseconds;
                        pluginWrapper.LoadedTime = elapsedTotalMilliseconds;
                        DebugWindow.LogDebug($"{pluginWrapper.Name} loaded in {elapsedTotalMilliseconds} ms.", 1);

                        pluginWrappers.Add(pluginWrapper);
                    }
                }
                return pluginWrappers;
            }
            catch (Exception e)
            {
                DebugWindow.LogError($"Error when load plugin ({assembly.ManifestModule.ScopeName}): {e})");
                return null;
            }
        }
    }
}
