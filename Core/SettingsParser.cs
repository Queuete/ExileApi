using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using JM.LinqFaster;
using MoreLinq;

namespace ExileCore
{
    public static class SettingsParser
    {
        public static bool ButtonPopupVisible { get; private set; } = false;

        public static Type ListOfWhat(object list)
        {
            return !(list is IList) ? null : (Type) ListOfWhat2((dynamic) list);
        }

        private static Type ListOfWhat2<T>(IList<T> list)
        {
            return typeof(T);
        }

        public static void Parse(ISettings settings, List<ISettingsHolder> draws, int id = -1)
        {
            var nextAvailableKey = -2;

            Parse(settings, draws, id, ref nextAvailableKey);
        }

        private static void Parse(ISettings settings, List<ISettingsHolder> draws, int id, ref int nextAvailableKey)
        {
            if (settings == null)
            {
                DebugWindow.LogError("Cant parse null settings.");
                return;
            }

            var props = settings.GetType().GetProperties();

            foreach (var property in props)
            {
                if (property.GetCustomAttribute<IgnoreMenuAttribute>() != null) continue;

                var menuAttribute = property.GetCustomAttribute<MenuAttribute>();

                if (property.Name == "Enable" && menuAttribute == null) continue;

                menuAttribute ??= new MenuAttribute(Regex.Replace(property.Name, "(\\B[A-Z])", " $1"));

                var holder = new SettingsHolder
                {
                    Name = menuAttribute.MenuName,
                    Tooltip = menuAttribute.Tooltip,
                    ID = menuAttribute.index == -1 ? nextAvailableKey-- : menuAttribute.index
                };


                if (property.PropertyType.GetInterfaces().ContainsF(typeof(ISettings)))
                {
                    var innerSettings = (ISettings) property.GetValue(settings);

                    if (menuAttribute.index == -1)
                    {
                        Parse(innerSettings, draws, id, ref nextAvailableKey);
                        continue;
                    }

                    holder.Type = HolderChildType.Tab;
                    draws.Add(holder);
                    Parse(innerSettings, draws, menuAttribute.index, ref nextAvailableKey);
                    var parent = GetAllDrawers(draws).Find(x => x.ID == menuAttribute.parentIndex);
                    parent?.Children.Add(holder);
                    continue;
                }

                if (IsISettingsList(property, settings))
                {
                    if (!(property.GetValue(settings) is IList list)) continue;

                    foreach (var item in list)
                    {
                        Parse(item as ISettings, draws, id, ref nextAvailableKey);
                    }

                    continue;
                }

                if (menuAttribute.parentIndex != -1)
                {
                    var parent = GetAllDrawers(draws).Find(x => x.ID == menuAttribute.parentIndex);
                    if (parent != null)
                    {
                        // TODO - Check if the new setting index collides with any children.
                        parent.Children.Add(holder);
                    }
                    else
                    {
                        DebugWindow.LogDebug(
                            $"SettingsParser => ParentIndex used before created. [Menu(\"{menuAttribute.MenuName}\", ..., {menuAttribute.parentIndex})] added as a top-level setting.");
                        draws.Add(holder);
                    }
                }
                else if (id != -1)
                {
                    var parent = GetAllDrawers(draws).Find(x => x.ID == id);
                    if (parent != null)
                    {
                        // debug log spam during startup due to HealthBars, temporarly disabled for now
                        //DebugWindow.LogDebug(
                        //    $"SettingsParser => Index collision. '[Menu(\"{menuAttribute.MenuName}\", ..., {id}, ...)] added as sub-setting of \"{parent.Name}\".");
                        parent.Children.Add(holder);
                    }
                    else
                    {
                        draws.Add(holder);
                    }
                }
                else
                {
                    draws.Add(holder);
                }

                var type = property.GetValue(settings);
                HandleType(holder, type, property.ToString());
            }
        }

        private static bool IsISettingsList(PropertyInfo propertyInfo, ISettings settings)
        {
            try
            {
                var value = propertyInfo.GetValue(settings);

                if (!(value is IList)) return false;

                var enumerator = (value as IList).GetEnumerator();
                try
                {
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current.GetType().GetInterfaces().ContainsF(typeof(ISettings));
                    }
                }
                finally
                {
                    var disposable = enumerator as IDisposable;
                    disposable?.Dispose();
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private static void HandleType(SettingsHolder holder, object type, string propertyInfo)
        {
            switch (type)
            {
                case ButtonNode buttonNode:
                    holder.DrawDelegate = () =>
                    {
                        if (ImGui.Button(holder.Unique))
                        {
                            buttonNode.OnPressed();
                        }
                    };
                    return;
                case null:
                case EmptyNode _:
                    holder.DrawDelegate = () => { };
                    return;
                case ToggleHotkeyNode toggleHotkeyNode:
                    holder.DrawDelegate = () =>
                    {

                        ImGui.Columns(2);
                        ImGui.SetColumnWidth(0, 35);
                        var isChecked = toggleHotkeyNode.Enabled;
                        ImGui.Checkbox("  " + holder.Unique, ref isChecked);
                        toggleHotkeyNode.Enabled = isChecked;
                        ImGui.NextColumn();

                        var str = $"{holder.Name} {toggleHotkeyNode.Value}##{toggleHotkeyNode.Value}";
                        var popupOpened = true;

                        if (ImGui.Button(str))
                        {
                            ImGui.OpenPopup(str);
                            popupOpened = true;
                        }
                        ImGui.NextColumn();

                        if (!ImGui.BeginPopupModal(str, ref popupOpened,
                            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
                            return;

                        if (Input.GetKeyState(Keys.Escape))
                        {
                            ImGui.CloseCurrentPopup();
                            ImGui.EndPopup();
                            return;
                        }

                        foreach (var key in Enum.GetValues(typeof(Keys)))
                        {
                            if (!Input.GetKeyState((Keys)key)) continue;
                            toggleHotkeyNode.Value = (Keys)key;
                            ImGui.CloseCurrentPopup();
                            break;
                        }

                        ImGui.Text($"Press new key to change '{toggleHotkeyNode.Value}' or Esc for exit.");
                        ImGui.EndPopup();
                    };
                    return;
                case HotkeyNode hotkeyNode:
                    holder.DrawDelegate = () =>
                    {
                        var str = $"{holder.Name} {hotkeyNode.Value}##{hotkeyNode.Value}";

                        if (ImGui.Button(str))
                        {
                            // Clear async buffer state
                            Input.ClearAsyncBuffer();

                            // Begin pop up
                            ImGui.OpenPopup(str);
                        }

                        // Create modal
                        var popupOpened = true;
                        if (ImGui.BeginPopupModal(str, ref popupOpened, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
                        {
                            ButtonPopupVisible = true;
                        }
                        else
                        {
                            return;
                        }

                        // Close popup if escape was pressed
                        if (Input.GetKeyState(Keys.Escape))
                        {
                            ImGui.CloseCurrentPopup();
                            ImGui.EndPopup();
                            ButtonPopupVisible = false;
                            return;
                        }

                        // Get prssed keys
                        Keys key = Input.GetPressedKeys(true);

                        // If a key was pressed, set value and close popup
                        if ((key & Keys.KeyCode) != Keys.None)
                        {
                            // Set the node's value to the keys that were pressed and close the popup
                            hotkeyNode.Value = key;
                            ImGui.CloseCurrentPopup();
                            ButtonPopupVisible = false;
                        }

                        // End popup
                        ImGui.Text($"Press new key to change '{hotkeyNode.Value}' or Esc for exit.");
                        ImGui.EndPopup();
                    };
                    return;
                case ToggleNode toggleNode:
                    holder.DrawDelegate = () =>
                    {
                        var isChecked = toggleNode.Value;
                        ImGui.Checkbox(holder.Unique, ref isChecked);
                        toggleNode.Value = isChecked;
                    };
                    return;
                case ColorNode colorNode:
                    holder.DrawDelegate = () =>
                    {
                        var color = colorNode.Value.ToVector4().ToVector4Num();
                        if (ImGui.ColorEdit4(holder.Unique, ref color,
                            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar |
                            ImGuiColorEditFlags.AlphaPreviewHalf))
                        {
                            colorNode.Value = color.ToSharpColor();
                        }
                    };
                    return;
                case ListNode listNode:
                    holder.DrawDelegate = () =>
                    {
                        if (!ImGui.BeginCombo(holder.Unique, listNode.Value)) return;

                        foreach (var value in listNode.Values)
                        {
                            if (!ImGui.Selectable(value)) continue;
                            listNode.Value = value;
                            break;
                        }

                        ImGui.EndCombo();
                    };
                    return;
                case FileNode fileNode:
                    holder.DrawDelegate = () =>
                    {
                        if (!ImGui.TreeNode(holder.Unique)) return;

                        var value = fileNode.Value;
                        if (ImGui.BeginChildFrame(1, new Vector2(0f, 300f)))
                        {
                            var directoryInfo = new DirectoryInfo("config");
                            if (directoryInfo.Exists)
                            {
                                var files = directoryInfo.GetFiles();
                                foreach (var fileInfo in files)
                                {
                                    if (ImGui.Selectable(fileInfo.Name, value == fileInfo.FullName))
                                    {
                                        fileNode.Value = fileInfo.FullName;
                                    }
                                }
                            }

                            ImGui.EndChildFrame();
                        }

                        ImGui.TreePop();
                    };
                    return;

                case RangeNode<int> iRangeNode:
                    holder.DrawDelegate = () =>
                    {
                        var value = iRangeNode.Value;
                        ImGui.SliderInt(holder.Unique, ref value, iRangeNode.Min, iRangeNode.Max);
                        iRangeNode.Value = value;
                    };
                    return;

                case RangeNode<float> fRangeNode:
                    holder.DrawDelegate = () =>
                    {
                        var value = fRangeNode.Value;
                        ImGui.SliderFloat(holder.Unique, ref value, fRangeNode.Min, fRangeNode.Max);
                        fRangeNode.Value = value;
                    };
                    return;

                case RangeNode<long> lRangeNode:
                    holder.DrawDelegate = () =>
                    {
                        var value = (int) lRangeNode.Value;
                        ImGui.SliderInt(holder.Unique, ref value, (int) lRangeNode.Min, (int) lRangeNode.Max);
                        lRangeNode.Value = value;
                    };
                    return;

                case RangeNode<Vector2> vRangeNode:
                    holder.DrawDelegate = () =>
                    {
                        var value = vRangeNode.Value;
                        ImGui.SliderFloat2(holder.Unique, ref value, vRangeNode.Min.X, vRangeNode.Max.X);
                        vRangeNode.Value = value;
                    };
                    return;

                case TextNode textNode:
                    holder.DrawDelegate = () =>
                    {
                        var value = textNode.Value;
                        ImGui.InputText(holder.Unique, ref value, 200);
                        textNode.Value = value;
                    };
                    return;
            }

            DebugWindow.LogDebug(
                $"SettingsParser => DrawDelegate not auto-generated for '{propertyInfo}'.");
        }

        private static List<ISettingsHolder> GetAllDrawers(List<ISettingsHolder> settingPropertyDrawers)
        {
            var settingsHolder = new List<ISettingsHolder>();
            GetDrawersRecurs(settingPropertyDrawers, settingsHolder);
            return settingsHolder;
        }

        private static void GetDrawersRecurs(IList<ISettingsHolder> drawers, IList<ISettingsHolder> settingsHolder)
        {
            foreach (var drawer in drawers)
            {
                if (settingsHolder.Contains(drawer))
                {
                    DebugWindow.LogError(
                        $"SettingsParser => Possible overflow or duplicate drawers detected when generating menu. Name: {drawer.Name}, Id: {drawer.ID}",
                        5);
                }
                else
                {
                    settingsHolder.Add(drawer);
                }
            }

            drawers.ForEach(x => GetDrawersRecurs(x.Children, settingsHolder));
        }
    }

    public enum HolderChildType
    {
        Tab,
        Border
    }

    public class SettingsHolder : ISettingsHolder
    {
        public SettingsHolder()
        {
            Tooltip = "";
        }

        public HolderChildType Type { get; set; } = HolderChildType.Border;
        public string Name { get; set; } = "";
        public string Tooltip { get; set; }
        public string Unique => $"{Name}##{ID}";
        public int ID { get; set; } = -1;
        public Action DrawDelegate { get; set; }
        public IList<ISettingsHolder> Children { get; } = new List<ISettingsHolder>();

        public void Draw()
        {
            var font = ImGui.GetFont();

            if (Children.Count <= 0)
            {
                DrawDelegate?.Invoke();

                if (Tooltip?.Length > 0)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled("(?)");
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
                    {
                        ImGui.SetTooltip(Tooltip);
                    }
                }

                return;
            }

            for (var i = 0; i < 5; i++)
            {
                ImGui.Spacing();
            }

            ImGui.BeginGroup();
            var contentRegionAvail = ImGui.GetContentRegionAvail();

            var firstCursorPos = ImGui.GetCursorPos().Translate(10, font.FontSize * -0.66f);

            ImGui.BeginChild(Unique, new Vector2(contentRegionAvail.X, font.FontSize * 2 * (Children.Count + 0.2f)),
                true);

            foreach (var child in Children)
            {
                child.Draw();
            }

            var secondCursorPos = ImGui.GetCursorPos().Translate(0, font.FontSize);
            ImGui.EndChild();
            ImGui.SetCursorPos(firstCursorPos);
            ImGui.Text(Name);

            if (Tooltip?.Length > 0)
            {
                ImGui.SameLine();
                ImGui.TextDisabled("(?)");
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
                {
                    ImGui.SetTooltip(Tooltip);
                }
            }

            ImGui.SetCursorPos(secondCursorPos);
            ImGui.EndGroup();

            DrawDelegate?.Invoke();
        }
    }
}