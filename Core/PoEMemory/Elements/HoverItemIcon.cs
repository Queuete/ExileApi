using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets;

namespace ExileCore.PoEMemory.Elements
{
    public class HoverItemIcon : Element
    {
        private static readonly int InventPosXOffset =
            Extensions.GetOffset<NormalInventoryItemOffsets>(nameof(NormalInventoryItemOffsets.InventPosX));

        private static readonly int InventPosYOffset =
            Extensions.GetOffset<NormalInventoryItemOffsets>(nameof(NormalInventoryItemOffsets.InventPosY));

        private static readonly int InventItemTooltipOffset =
            Extensions.GetOffset<NormalInventoryItemOffsets>(nameof(NormalInventoryItemOffsets.Tooltip));

        private static readonly int InventItemOffset =
            Extensions.GetOffset<NormalInventoryItemOffsets>(nameof(NormalInventoryItemOffsets.Item));

        private static readonly int ItemsOnGroundLabelElementOffset =
            Extensions.GetOffset<IngameUElementsOffsets>(nameof(IngameUElementsOffsets.itemsOnGroundLabelRoot));


        private ToolTipType? _ToolTip;
        public Element InventoryItemTooltip => ReadObject<Element>(Address + InventItemTooltipOffset);
        public Element ItemInChatTooltip => ReadObject<Element>(Address + 0x1F0);
        public ItemOnGroundTooltip ToolTipOnGround => TheGame.IngameState.IngameUi.ItemOnGroundTooltip;
        public int InventPosX => M.Read<int>(Address + InventPosXOffset);
        public int InventPosY => M.Read<int>(Address + InventPosYOffset);

        public ToolTipType ToolTipType => (_ToolTip = _ToolTip ?? GetToolTipType()).Value;

        public new Element Tooltip
        {
            get
            {
              switch (ToolTipType)
              {
                case ToolTipType.ItemOnGround:
                  return ToolTipOnGround.Tooltip;
                case ToolTipType.InventoryItem:
                  return InventoryItemTooltip;
                case ToolTipType.ItemInChat:
                  return ItemInChatTooltip.Children[0].Children[1];
                default:
                  return null;
              }
            }
        }

        public Element ItemFrame
        {
            get
            {
              switch (ToolTipType)
              {
                case ToolTipType.ItemOnGround:
                  return ToolTipOnGround.ItemFrame;
                case ToolTipType.ItemInChat:
                  return ItemInChatTooltip.Children[0].Children[0];
                default:
                  return null;
              }
            }
        }

        public Entity Item
        {
            get
            {
                switch (ToolTipType)
                {
                    case ToolTipType.ItemOnGround:
                        var le = TheGame.IngameState.IngameUi.ReadObjectAt<ItemsOnGroundLabelElement>(
                            ItemsOnGroundLabelElementOffset);
                        var e = le?.ItemOnHover;
                        return e?.GetComponent<WorldItem>()?.ItemEntity;
                    case ToolTipType.InventoryItem:
                        return ReadObject<Entity>(Address + InventItemOffset);
                    case ToolTipType.ItemInChat:
                        return null;
                }

                return null;
            }
        }

        private ToolTipType GetToolTipType()
        {
            if (InventoryItemTooltip != null && InventoryItemTooltip.IsVisible)
            {
                return ToolTipType.InventoryItem;
            }

            if (ToolTipOnGround != null && ToolTipOnGround.Tooltip != null && ToolTipOnGround.TooltipUI != null &&
                ToolTipOnGround.TooltipUI.IsVisible)
            {
                return ToolTipType.ItemOnGround;
            }

            if (ItemInChatTooltip != null && ItemInChatTooltip.IsVisible && ItemInChatTooltip.ChildCount > 0 &&
                ItemInChatTooltip.Children[0].ChildCount > 1 && ItemInChatTooltip.Children[0].Children[0].IsVisible &&
                ItemInChatTooltip.Children[0].Children[1].IsVisible)
            {
                return ToolTipType.ItemInChat;
            }

            return ToolTipType.None;
        }
    }
}
