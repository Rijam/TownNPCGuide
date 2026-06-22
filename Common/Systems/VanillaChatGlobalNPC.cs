using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using TownNPCGuide.Content.NPCs.TownNPCs;

namespace TownNPCGuide.Common.Systems
{
	public class VanillaChatGlobalNPC : GlobalNPC
	{
		public override void GetChat(NPC npc, ref string chat) {
			// We check to see if the NPC is the type of NPC we want to add chat to.
			if (npc.type == NPCID.Guide) {
				// Add a random chance for it to be chosen. Otherwise, this line will override all the other lines.
				if (Main.rand.NextBool(4)) {
					// Set the chat variable to our new chat message from our localization.
					// The localization keys don't necessarily need to follow this pattern. This is just an example.
					chat = Language.GetTextValue("Mods.TownNPCGuide.NPCs.Guide.Dialogue.ModdedDialogue");
				}
			}

			// We could use a switch statement instead, too.
			switch (npc.type) {
				case NPCID.Nurse:
					if (Main.rand.NextBool(4)) {
						chat = Language.GetTextValue("Mods.TownNPCGuide.NPCs.Nurse.Dialogue.ModdedDialogue");
					}
					break;
				case NPCID.ArmsDealer:
					// We can add any other conditions just like normal.

					// FindFirstNPC will return the index of the NPC in the Main.npc[] array. Or -1 if nothing is found.
					int tutorialNPC = NPC.FindFirstNPC(ModContent.NPCType<TutorialTownNPC>());
					if (Main.rand.NextBool(6) && tutorialNPC > 0) {
						chat = Language.GetTextValue("Mods.TownNPCGuide.NPCs.ArmsDealer.Dialogue.TutorialTownNPCPresent", Main.npc[tutorialNPC].FullName);
					}
					break;
				case NPCID.Dryad:
					if (Main.rand.NextBool(8) && Condition.IsNpcShimmered.IsMet() && WorldGen.tEvil > 0 && WorldGen.tBlood > 0) {
						chat = Language.GetTextValue("Mods.TownNPCGuide.NPCs.Dryad.Dialogue.Shimmered", Main.worldName, WorldGen.tEvil, WorldGen.tBlood);
					}
					break;
				case NPCID.Steampunker:
					if (Main.rand.NextBool(3)) {
						// We could use a Weighted Random string, too.
						WeightedRandom<string> lines = new();

						lines.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.Steampunker.Dialogue.Standard1"), 1.2);
						lines.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.Steampunker.Dialogue.Standard2"), 0.6);
						lines.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.Steampunker.Dialogue.Standard3"), 2);
						chat = lines; // lines is implicitly cast to a string.
					}
					break;
			}
		}

		// This hook lets you do things before the chat buttons are clicked.
		// See Example Mod GlobalNPCInteractions for more examples:
		// https://github.com/tModLoader/tModLoader/blob/1.4.5/ExampleMod/Common/GlobalNPCs/GobalNPCInteractions.cs
		public override bool PreChatButtonClicked(NPC npc, NPCInteraction interaction)
		{
			// Main.NewText($"PreChatButtonClicked {interaction.GetText()} was clicked from the GlobalNPC!");
			// if (interaction is NPCInteractions.Actions.CloseChat)
			// {
				// Main.NewText($"PreChatButtonClicked returning false!!");
				// return false;
			// }
			return base.PreChatButtonClicked(npc, interaction);
		}

		// This hook lets you do things when chat buttons are clicked.
		public override void OnChatButtonClicked(NPC npc, NPCInteraction interaction)
		{
			// Main.NewText($"Button {interaction.GetText()} was clicked from the GlobalNPC!");
		}
	}
}