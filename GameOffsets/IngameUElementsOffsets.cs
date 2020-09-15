using System.Runtime.InteropServices;

namespace GameOffsets
{
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct IngameUElementsOffsets
	{
		[FieldOffset(0x210)] public long GetQuests;
		[FieldOffset(0x238)] public long GameUI;
		[FieldOffset(0x370)] public long Mouse;
		[FieldOffset(0x378)] public long SkillBar;
		[FieldOffset(0x380)] public long HiddenSkillBar;
		[FieldOffset(0x3D8)] public long BanditDialog;
		[FieldOffset(0x480)] public long QuestTracker;
		[FieldOffset(0x4E0 /*4F0*/)] public long OpenLeftPanel;
		[FieldOffset(0x4E8 /*4F8*/)] public long OpenRightPanel;
		[FieldOffset(0x520)] public long InventoryPanel;
		[FieldOffset(0x528)] public long StashElement;
		[FieldOffset(0x550)] public long TreePanel;
		[FieldOffset(0x558)] public long AtlasPanel;
		[FieldOffset(0x588)] public long WorldMap;
		[FieldOffset(0x5A8)] public long Map;
		[FieldOffset(0x5B0)] public long itemsOnGroundLabelRoot;
		[FieldOffset(0x630)] public long NpcDialog;
		[FieldOffset(0x640)] public long QuestRewardWindow;
		[FieldOffset(0x648)] public long PurchaseWindow;
		[FieldOffset(0x650)] public long SellWindow;
		[FieldOffset(0x658)] public long TradeWindow;
		[FieldOffset(0x690)] public long MapDeviceWindow;
		[FieldOffset(0x6E8)] public long IncursionWindow;
		[FieldOffset(0x708)] public long DelveWindow;
		[FieldOffset(0x728)] public long BetrayalWindow;
		[FieldOffset(0x730)] public long ZanaMissionChoice;
		[FieldOffset(0x740)] public long CraftBenchWindow;
		[FieldOffset(0x748)] public long UnveilWindow;
		[FieldOffset(0x778)] public long SynthesisWindow;
		[FieldOffset(0x788)] public long MetamorphWindow;
		[FieldOffset(0x7D8)] public long AreaInstanceUi;
		[FieldOffset(0x918)] public long GemLvlUpPanel;
		[FieldOffset(0x8A8)] public long InvitesPanel;
		[FieldOffset(0x9C8)] public long ItemOnGroundTooltip;
		[FieldOffset(0x0/*0xF18*/)] public long MapTabWindowStartPtr;//TOFO: Fixme. Cause reading errors
	}
}
