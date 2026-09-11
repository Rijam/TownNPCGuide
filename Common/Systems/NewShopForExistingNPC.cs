using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TownNPCGuide.Content.Items;
using TownNPCGuide.Content.NPCs.TownNPCs;

namespace TownNPCGuide.Common.Systems
{
	// Here is an example of registering a new shop for existing Town NPCs.
	public class NewShopForExistingNPC : GlobalNPC
	{
		// Here we are detouring two methods so we can register new shops at the correct time.

		private delegate void orig_NPCShopDatabase_RegisterVanillaNPCShops();
		private static Hook Hook_NPCShopDatabase_RegisterVanillaNPCShops;

		private delegate void orig_NPCLoader_AddShops(int type);
		private static Hook Hook_NPCLoader_AddShops;

		public override void Load()
		{
			// Create a new detour hook for NPCShopDatabase.RegisterVanillaNPCShops for vanilla Town NPCs
			MethodInfo NPCShopDatabase_RegisterVanillaNPCShops = typeof(NPCShopDatabase).GetMethod("RegisterVanillaNPCShops", BindingFlags.Static | BindingFlags.NonPublic);
			Hook_NPCShopDatabase_RegisterVanillaNPCShops = new Hook(NPCShopDatabase_RegisterVanillaNPCShops, On_NPCShopDatabase_RegisterVanillaNPCShops);

			// Create a new detour hook for NPCLoader.AddShops for Modded Town NPCs
			MethodInfo NPCLoader_AddShops = typeof(NPCLoader).GetMethod("AddShops", BindingFlags.Static | BindingFlags.Public);
			Hook_NPCLoader_AddShops = new Hook(NPCLoader_AddShops, On_NPCLoader_AddShops);
		}

		public override void Unload()
		{
			Hook_NPCShopDatabase_RegisterVanillaNPCShops.Undo();
			Hook_NPCLoader_AddShops.Undo();
		}

		// Create a shop for the Angler
		private void On_NPCShopDatabase_RegisterVanillaNPCShops(orig_NPCShopDatabase_RegisterVanillaNPCShops orig)
		{
			orig(); // Run the original code of NPCShopDatabase.RegisterVanillaNPCShops first, then add our new shop.
			new NPCShop(NPCID.Angler, "ModdedShop") // The name here must match the name for the button.
				// Add your items here.
				.Add(ItemID.ApprenticeBait)
				.Add(ItemID.JourneymanBait)
				.Add(ItemID.MasterBait, Condition.AnglerQuestsFinishedOver(1))
				.Register();
		}

		// Create a shop for the Tutorial Town NPC
		private void On_NPCLoader_AddShops(orig_NPCLoader_AddShops orig, int type)
		{
			orig(type); // Run the original code of NNPCLoader.AddShops first, then add our new shop.

			if (type == ModContent.NPCType<TutorialTownNPC>())
			{
				new NPCShop(type, "ModdedShop") // The name here must match the name for the button.
					// Add your items here.
					.Add(ModContent.ItemType<TutorialItem>())
					.Register();
			}
		}

		// Register a chat button for the shop.
		public override void RegisterChatButtons(NPC npc, NPCInteractionList interactions)
		{
			if (npc.type == NPCID.Angler)
			{
				NPCInteraction questButton = interactions.Interactions.OfType<NPCInteractions.Actions.AnglerQuest>().FirstOrDefault();
				interactions.InsertAfter(NPCInteractions.Shop("ModdedShop"), questButton); // Place it after the quest button.
			}
			
			if (npc.type == ModContent.NPCType<TutorialTownNPC>())
			{
				interactions.Append(NPCInteractions.Shop("ModdedShop", "Elsewhere Shop")); // Place it at the end.
			}
		}
	}
}
