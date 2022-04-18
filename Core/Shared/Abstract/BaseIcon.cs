using System;
using System.Collections.Generic;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using SharpDX;

namespace ExileCore.Shared.Abstract
{
    public abstract class BaseIcon
    {
        protected static readonly Dictionary<string, Size2> strongboxesUV = new Dictionary<string, Size2>
        {
            {"Metadata/Chests/StrongBoxes/Large", new Size2(7, 7)},
            {"Metadata/Chests/StrongBoxes/Strongbox", new Size2(1, 2)},
            {"Metadata/Chests/StrongBoxes/Armory", new Size2(2, 1)},
            {"Metadata/Chests/StrongBoxes/Arsenal", new Size2(4, 1)},
            {"Metadata/Chests/StrongBoxes/Artisan", new Size2(3, 1)},
            {"Metadata/Chests/StrongBoxes/Jeweller", new Size2(1, 1)},
            {"Metadata/Chests/StrongBoxes/Cartographer", new Size2(5, 1)},
            {"Metadata/Chests/StrongBoxes/CartographerLowMaps", new Size2(5, 1)},
            {"Metadata/Chests/StrongBoxes/CartographerMidMaps", new Size2(5, 1)},
            {"Metadata/Chests/StrongBoxes/CartographerHighMaps", new Size2(5, 1)},
            {"Metadata/Chests/StrongBoxes/Ornate", new Size2(7, 7)},
            {"Metadata/Chests/StrongBoxes/Arcanist", new Size2(1, 8)},
            {"Metadata/Chests/StrongBoxes/Gemcutter", new Size2(6, 1)},
            {"Metadata/Chests/StrongBoxes/StrongboxDivination", new Size2(7, 1)},
            {"Metadata/Chests/AbyssChest", new Size2(7, 7)}
        };

        protected static readonly Dictionary<string, Color> FossilRarity = new Dictionary<string, Color>
        {
            {"Fractured", Color.Aquamarine},
            {"Faceted", Color.Aquamarine},
            {"Glyphic", Color.Aquamarine},
            {"Hollow", Color.Aquamarine},
            {"Shuddering", Color.Aquamarine},
            {"Bloodstained", Color.Aquamarine},
            {"Tangled", Color.OrangeRed},
            {"Dense", Color.OrangeRed},
            {"Gilded", Color.OrangeRed},
            {"Sanctified", Color.Aquamarine},
            {"Encrusted", Color.Yellow},
            {"Aetheric", Color.Orange},
            {"Enchanted", Color.Orange},
            {"Pristine", Color.Orange},
            {"Prismatic", Color.Orange},
            {"Corroded", Color.Yellow},
            {"Perfect", Color.Orange},
            {"Jagged", Color.Yellow},
            {"Serrated", Color.Yellow},
            {"Bound", Color.Yellow},
            {"Lucent", Color.Yellow},
            {"Metallic", Color.Yellow},
            {"Scorched", Color.Yellow},
            {"Aberrant", Color.Yellow},
            {"Frigid", Color.Yellow}
        };

        private readonly ISettings _settings;
        protected bool _HasIngameIcon;

        public BaseIcon(Entity entity, ISettings settings)
        {
            _settings = settings;
            Entity = entity;

            if (_settings == null || Entity == null)
            {
                return;
            }

            Rarity = Entity.Rarity;

            switch (Rarity)
            {
                case MonsterRarity.White:
                    Priority = IconPriority.Low;
                    break;
                case MonsterRarity.Magic:
                    Priority = IconPriority.Medium;
                    break;
                case MonsterRarity.Rare:
                    Priority = IconPriority.High;
                    break;
                case MonsterRarity.Unique:
                    Priority = IconPriority.Critical;
                    break;
                default:
                    Priority = IconPriority.Critical;
                    break;
            }

            Show = () => Entity.IsValid;
            Hidden = () => entity.IsHidden;
            GridPosition = () => Entity.GridPos;

            if (Entity.HasComponent<MinimapIcon>())
            {
                var name = Entity.GetComponent<MinimapIcon>().Name;

                if (!string.IsNullOrEmpty(name))
                {
                    var iconIndexByName = Extensions.IconIndexByName(name);

                    if (iconIndexByName != MapIconsIndex.MyPlayer)
                    {
                        MainTexture = new HudTexture("Icons.png") {UV = SpriteHelper.GetUV(iconIndexByName), Size = 16};
                        _HasIngameIcon = true;
                    }

                    if (Entity.HasComponent<Portal>() && Entity.HasComponent<Transitionable>())
                    {
                        var transitionable = Entity.GetComponent<Transitionable>();
                        Text = RenderName;
                        Show = () => Entity.IsValid && transitionable.Flag1 == 2;
                    }
                    else if (Entity.Path.StartsWith("Metadata/Terrain/Labyrinth/Objects/Puzzle_Parts/Switch", StringComparison.Ordinal))
                    {
                        Show = () =>
                        {
                            var transitionable = Entity.GetComponent<Transitionable>();
                            var minimapIcon = Entity.GetComponent<MinimapIcon>();
                            return Entity.IsValid && minimapIcon.IsVisible && minimapIcon.IsHide == false && transitionable.Flag1 != 2;
                        };
                    }
                    else if (Entity.Path.StartsWith("Metadata/MiscellaneousObjects/Abyss/Abyss"))
                    {
                        Show = () =>
                        {
                            var minimapIcon = Entity.GetComponent<MinimapIcon>();

                            return Entity.IsValid && minimapIcon.IsVisible &&
                                   (minimapIcon.IsHide == false || Entity.GetComponent<Transitionable>().Flag1 == 1);
                        };
                    }
                    else if (entity.Path.Contains("Metadata/Terrain/Leagues/Delve/Objects/DelveMineral"))
                    {
                        Priority = IconPriority.High;
                        MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.DelveMineralVein);
                        Text = "Sulphite";
                        Show = () => entity.IsValid && entity.IsTargetable;
                    }
                    else
                    {
                        Show = () =>
                        {
                            var c = Entity.GetComponent<MinimapIcon>();
                            return c != null && c.IsVisible && c.IsHide == false;
                        };
                    }
                }
            }
        }

        public bool HasIngameIcon => _HasIngameIcon;
        public Entity Entity { get; }
        public Func<Vector2> GridPosition { get; set; }
        public RectangleF DrawRect { get; set; }
        public Func<bool> Show { get; set; }
        public Func<bool> Hidden { get; protected set; } = () => false;
        public HudTexture MainTexture { get; protected set; }
        public IconPriority Priority { get; protected set; }
        public MonsterRarity Rarity { get; protected set; }
        public string Text { get; protected set; }
        public string RenderName => Entity.RenderName;

        protected bool PathCheck(Entity path, params string[] check)
        {
            foreach (var s in check)
            {
                if (path.Path.Equals(s, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
