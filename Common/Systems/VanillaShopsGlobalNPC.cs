using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TownNPCGuide.Content.NPCs.TownNPCs;
using static Terraria.GameContent.IL_NPCInteractions.Actions;

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

		// Here we are adding a new button to an NPC.
		// See Example Mod GlobalNPCInteractions for more examples:
		// https://github.com/tModLoader/tModLoader/blob/1.4.5/ExampleMod/Common/GlobalNPCs/GobalNPCInteractions.cs
		public override void RegisterChatButtons(NPC npc, NPCInteractionList interactions)
		{
			if (npc.type == NPCID.Angler)
			{
				// This an invalid shop. It will open an empty shop.
				interactions.InsertBefore(NPCInteractions.Shop("BadShopName/Terraria/Merchant/Shop", "Shop"), NPCInteractionDatabase.CloseButton);

				// Find a button and disable it.
				// interactions.Disable(interactions.Interactions.OfType<NPCInteractions.Actions.AnglerQuest>().FirstOrDefault());
				// interactions.Disable(NPCInteractionDatabase.CloseButton);
			}
		}
	}
}