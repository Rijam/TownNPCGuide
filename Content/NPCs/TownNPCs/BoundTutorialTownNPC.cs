using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TownNPCGuide.Common.Systems;

namespace TownNPCGuide.Content.NPCs.TownNPCs
{
	// This is how to make a rescuable Town NPC.
	public class BoundTutorialTownNPC : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 1;

			// Hide this NPC from the bestiary.
			NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new() {
				Hide = true 
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
		}

		public override void SetDefaults() {
			// Notice NPC.townNPC is not set.
			NPC.friendly = true;
			NPC.width = 28;
			NPC.height = 32;
			NPC.aiStyle = 0; // aiStyle of 0 is used. The NPC will not move.
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			NPC.rarity = 1; // To make our NPC will show up on the Lifeform Analyzer.
		}

		public override bool CanChat()
		{
			// Make it so our NPC can be talked to.
			return true;
		}

		public override void AI() {
			// Using aiStyle 0 will make it so the NPC will always turn to face the player.
			// If you don't want that, you can set the spriteDirection to -1 (left) or 1 (right) so they always appear to face one way.
			// NPC.spriteDirection = 1;

			// This is where we check to see if a player has clicked on our NPC.
			// First, don't run this code if it is a multiplayer client.
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				// Loop through every player on the server.
				for (int i = 0; i < Main.maxPlayers; i++) {
					// If the player is active (on the server) and are talking to this NPC...
					if (Main.player[i].active && Main.player[i].talkNPC == NPC.whoAmI) {
						NPC.Transform(ModContent.NPCType<TutorialTownNPC>()); // Transform to our real Town NPC.																  
						Main.BestiaryTracker.Chats.RegisterChatStartWith(NPC); // Unlock the Town NPC in the Bestiary.																  
						Main.player[i].SetTalkNPC(NPC.whoAmI);  // Change who the player is talking to to the new Town NPC. 
						TownNPCGuideWorld.rescuedTutorialTownNPC = true; // Set our rescue bool to true.

						// We need to sync these changes in multiplayer.
						if (Main.netMode == NetmodeID.Server) {
							NetMessage.SendData(MessageID.SyncTalkNPC, -1, -1, null, i);
							NetMessage.SendData(MessageID.WorldData);
						}
					}
				}
			}
		}

		public override string GetChat() {
			// Make the Town NPC say something unique when first rescued.
			return Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.OnRescue");
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			// In this example, the bound NPC can spawn at the surface on grass, dirt, or hallowed grass.
			// We also make sure not spawn the bound NPC if it has already spawned or if the NPC has already been rescued.
			if (spawnInfo.Player.ZoneOverworldHeight && !TownNPCGuideWorld.rescuedTutorialTownNPC && !NPC.AnyNPCs(ModContent.NPCType<BoundTutorialTownNPC>()) && !NPC.AnyNPCs(ModContent.NPCType<TutorialTownNPC>())) {
				if (spawnInfo.SpawnTileType == TileID.Grass || spawnInfo.SpawnTileType == TileID.Dirt || spawnInfo.SpawnTileType == TileID.HallowedGrass) {
					return 0.75f;
				}
			}
			return 0f;
		}

		public override void HitEffect(NPC.HitInfo hit) {
			// Create gore when the NPC is killed.
			// HitEffect() is called every time the NPC takes damage.
			// We need to check that the gore is not spawned on a server and that the NPC is actually dead.
			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
				// Spawn a piece of gore in the Gores folder with the name "TutorialTownNPC_Gore_Head".
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("TutorialTownNPC_Gore_Head").Type);
				// Spawn two pieces named "TutorialTownNPC_Gore_Arm".
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, NPC.height / 2f), NPC.velocity, Mod.Find<ModGore>("TutorialTownNPC_Gore_Arm").Type);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, NPC.height / 2f), NPC.velocity, Mod.Find<ModGore>("TutorialTownNPC_Gore_Arm").Type);
				// Spawn two pieces named "TutorialTownNPC_Gore_Leg".
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, NPC.height), NPC.velocity, Mod.Find<ModGore>("TutorialTownNPC_Gore_Leg").Type);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, NPC.height), NPC.velocity, Mod.Find<ModGore>("TutorialTownNPC_Gore_Leg").Type);
			}
		}
	}
}