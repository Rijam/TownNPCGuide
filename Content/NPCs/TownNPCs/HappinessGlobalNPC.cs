using Terraria;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.ModLoader;

namespace TownNPCGuide.Content.NPCs.TownNPCs
{
	public class HappinessGlobalNPC : GlobalNPC {
		public override void SetStaticDefaults() {
			
			int tutorialTownNPC = ModContent.NPCType<TutorialTownNPC>(); // Get our Town NPC's type.

			var guideHappiness = NPCHappiness.Get(NPCID.Guide); // Get the Guide's happiness.
			var goblinTinkererHappiness = NPCHappiness.Get(NPCID.GoblinTinkerer); // Get the Goblin Tinkerer's happiness.

			guideHappiness.SetNPCAffection(tutorialTownNPC, AffectionLevel.Like); // Make the Guide like our Town NPC.
			goblinTinkererHappiness.SetNPCAffection(tutorialTownNPC, AffectionLevel.Dislike); // Make the Goblin Tinkerer dislike our Town NPC.

			// Cross mod happiness

			// First check if the mod is enabled
			if (ModLoader.TryGetMod("ExampleMod", out Mod exampleMod)) {
				// Next, try to find the other Town NPC.
				// Using TryFind<>() is safer than using Find<>()
				// If the NPC we are trying to find doesn't exist, our mod will continue to work.
				if (exampleMod.TryFind<ModNPC>("ExamplePerson", out ModNPC examplePersonModNPC)) {
					// Get their happiness
					var examplePersonHappiness = NPCHappiness.Get(examplePersonModNPC.Type);
					var tutorialTownNPCHappiness = NPCHappiness.Get(tutorialTownNPC);

					// Make them love each other!
					examplePersonHappiness.SetNPCAffection(tutorialTownNPC, AffectionLevel.Love);
					tutorialTownNPCHappiness.SetNPCAffection(examplePersonModNPC.Type, AffectionLevel.Love);
				}
			}
		}
	}
}