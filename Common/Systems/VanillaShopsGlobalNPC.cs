using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TownNPCGuide.Common.Systems
{
	public class VanillaShopsGlobalNPC : GlobalNPC
	{
		// We can add items to existing shops here.
		public override void ModifyShop(NPCShop shop) {
			// Check the NPC Type on the shop, then add the item.
			if (shop.NpcType == NPCID.Merchant) {
				shop.Add(ItemID.TinPickaxe);
			}

			if (shop.NpcType == NPCID.Painter) {
				// The Painter for example has two shops.
				// If we don't want our item to go item to show in both shops, we need to specify which shop to add the item to.
				if (shop.Name == "Decor") {
					shop.Add(ItemID.SparkyPainting);
				}
				// All default shop names are "Shop".
				if (shop.Name == "Shop") { // or shop.Name != "Decor"
					shop.Add(ItemID.EchoCoating);
				}
			}
		}

		// Traveling Merchant uses a different system for his shop.
		// If you were familiar with the Pre-1.4.4 shop system, it is still using that.
		public override void SetupTravelShop(int[] shop, ref int nextSlot) {
			if (NPC.downedBoss1 && Main.rand.NextBool(5)) {
				shop[nextSlot++] = ItemID.EmeraldStaff;
			}
			if (Condition.NpcIsPresent(NPCID.Nurse).IsMet() && Main.rand.NextBool(2)) {
				shop[nextSlot++] = ItemID.HealingPotion;
			}
		}
	}
}