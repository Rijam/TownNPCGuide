using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace TownNPCGuide.Content.NPCs.TownNPCs
{
	public static class CustomConditions {

		public static Condition TravelingMerchantPresentOrHardmode = new(Language.GetTextValue("Mods.TownNPCGuide.Conditions.TravelingMerchantPresentOrHardmode"),
			() => Condition.NpcIsPresent(NPCID.TravellingMerchant).IsMet() || Main.hardMode);
	}
}