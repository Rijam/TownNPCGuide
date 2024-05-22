using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace TownNPCGuide.Content.NPCs.TownNPCs.TownPets
{
	[AutoloadHead]
	public class TutorialTownPet : ModNPC
	{
		// Where our additional head sprites will be stored.
		internal static int HeadIndex1;
		internal static int HeadIndex2;
		internal static int HeadIndex3;
		internal static int HeadIndex4;

		private static ITownNPCProfile NPCProfile;

		public override void Load()
		{
			// Adds our variant heads to the NPCHeadLoader.
			HeadIndex1 = Mod.AddNPCHeadTexture(Type, Texture + "_1_Head");
			HeadIndex2 = Mod.AddNPCHeadTexture(Type, Texture + "_2_Head");
			HeadIndex3 = Mod.AddNPCHeadTexture(Type, Texture + "_3_Head");
			HeadIndex4 = Mod.AddNPCHeadTexture(Type, Texture + "_4_Head");
		}

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 28;
			NPCID.Sets.ExtraFramesCount[Type] = 18; // The number of frames after the walking frames.
			NPCID.Sets.AttackFrameCount[Type] = 0; // Town Pets don't have any attacking frames.
			NPCID.Sets.DangerDetectRange[Type] = 250; // How far away the NPC will detect danger. Measured in pixels.
			NPCID.Sets.AttackType[Type] = -1;
			NPCID.Sets.AttackTime[Type] = -1;
			NPCID.Sets.AttackAverageChance[Type] = 1;
			NPCID.Sets.HatOffsetY[Type] = 0; // An offset for where the party hat sits on the sprite.
			NPCID.Sets.ShimmerTownTransform[Type] = false; // Town Pets don't have a Shimmer variant.
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true; // But they are still immune to Shimmer.
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true; // And Confused.
			NPCID.Sets.ExtraTextureCount[Type] = 0; // Even though we have several variation textures, we don't use this set.
			NPCID.Sets.NPCFramingGroup[Type] = 4; // How the party hat is animated to match the walking animation. Town Cat = 4, Town Dog = 5, Town Bunny = 6, Town Slimes = 7

			NPCID.Sets.IsTownPet[Type] = true; // Our NPC is a Town Pet
			NPCID.Sets.CannotSitOnFurniture[Type] = false; // True by default, but Town Cats can sit on furniture.
			NPCID.Sets.TownNPCBestiaryPriority.Add(Type); // Puts our NPC with all of the other Town NPCs.

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
			{
				Velocity = 0.25f, // Draws the NPC in the bestiary as if its walking +0.25 tiles in the x direction
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new TutorialTownPetProfile(); // Assign our profile.
		}
		public override void SetDefaults()
		{
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 20;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0.5f;
			NPC.housingCategory = 1; // This means it can share a house with a normal Town NPC.
			AnimationType = NPCID.TownCat; // This example matches the animations of the Town Cat.
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.TownNPCGuide.NPCs.TutorialTownPet.Bestiary")
			});
		}

		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			// If we've used the License, our Town Pet can freely respawn.
			if (TownNPCGuideWorld.boughtTutorialTownPet)
			{
				return true;
			}
			return false;
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			// Vanilla Town Pets use Profiles.VariantNPCProfile() to set the variants, but that doesn't work for us because
			// it uses Main.Assets.Request<>() which won't find mod assets (ModContent.Request<>() is needed instead).
			// So, we make our own NPCProfile. (See Below)
			return NPCProfile;
		}

		// Create a bunch of lists for our names. Each variant gets its own list of names.
		public readonly List<string> NameList0 = new ()
		{
			"Tutorial", "Guide", "Example"
		};
		public readonly List<string> NameList1 = new()
		{
			"Bob", "Bill", "Billy-bob"
		};
		public readonly List<string> NameList2 = new()
		{
			"John", "Jane"
		};
		public readonly List<string> NameList3 = new()
		{
			"Sir Fluffykins of the Dungeon", "Nugget", "Snausages", "big butts and I cannot lie", "Skadoodles"
		};
		public readonly List<string> NameList4 = new()
		{
			"Blinky", "Pinky", "Inky", "Clyde"
		};

		public override List<string> SetNPCNameList()
		{
			return NPC.townNpcVariationIndex switch // Change the name based on the variation.
			{
				0 => NameList0,
				1 => NameList1, // Will be the Shimmered variant if your NPC has a shimmer variant.
				2 => NameList2,
				3 => NameList3,
				4 => NameList4,
				_ => NameList0
			};
		}

		public override string GetChat()
		{
			WeightedRandom<string> chat = new();

			chat.Add("*Tutorial Town Pet noises*");

			return chat;
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("UI.PetTheAnimal"); // Pet
		}

		public override bool CanGoToStatue(bool toKingStatue)
		{
			return false; // Don't go to King or Queen statues. (Default is false so this technically isn't needed.)
		}
	}

	public class TutorialTownPetProfile : ITownNPCProfile
	{
		private static readonly string filePath = "TownNPCGuide/Content/NPCs/TownNPCs/TownPets/TutorialTownPet";

		// Load all of our textures only one time during mod load time.
		private readonly Asset<Texture2D> variant0 = ModContent.Request<Texture2D>(filePath);
		private readonly Asset<Texture2D> variant1 = ModContent.Request<Texture2D>($"{filePath}_1");
		private readonly Asset<Texture2D> variant2 = ModContent.Request<Texture2D>($"{filePath}_2");
		private readonly Asset<Texture2D> variant3 = ModContent.Request<Texture2D>($"{filePath}_3");
		private readonly Asset<Texture2D> variant4 = ModContent.Request<Texture2D>($"{filePath}_4");
		private readonly int headIndex0 = ModContent.GetModHeadSlot($"{filePath}_Head");


		public int RollVariation()
		{
			int random = Main.rand.Next(5); // 5 variants; 0 through 4.

			// If your Town Pet has a shimmer variant:
			// townNpcVariationIndex of 1 makes it shimmered, even when it isn't.
			// if (random == 1) {
			//	random = 5; // So variation 1 becomes number 5.
			// }

			return random;
		}

		public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc)
		{
			return npc.townNpcVariationIndex switch
			{
				0 => variant0,
				1 => variant1, // Will be the Shimmered variant if your NPC has a shimmer variant.
				2 => variant2,
				3 => variant3,
				4 => variant4,
				_ => variant0
			};
		}

		public int GetHeadTextureIndex(NPC npc)
		{
			return npc.townNpcVariationIndex switch
			{
				0 => headIndex0,
				1 => TutorialTownPet.HeadIndex1, // Will be the Shimmered variant if your NPC has a shimmer variant.
				2 => TutorialTownPet.HeadIndex2,
				3 => TutorialTownPet.HeadIndex3,
				4 => TutorialTownPet.HeadIndex4,
				_ => headIndex0
			};
		}
	}

	public class PetDetour : ModSystem
	{
		// The position of the player and the angle of their arm when petting is hardcoded.
		// We can use a Detour to change it for our Town Pet.

		public override void Load()
		{
			Terraria.On_Player.GetPettingInfo += Player_Hook_GetPettingInfo; // Load and apply our Detour.
			Terraria.On_Main.NPCAddHeight += Main_Hook_NPCAddHeight;
		}

		private delegate void orig_Player_GetPettingInfo(Player self); // Create a delegate because the original method is private.

		// Create our Detour
		private static void Player_Hook_GetPettingInfo(Terraria.On_Player.orig_GetPettingInfo orig, Player self, int animalNpcIndex, out int targetDirection, out Vector2 playerPositionWhenPetting, out bool isPetSmall)
		{
			// Run the vanilla code first.
			orig(self, animalNpcIndex, out targetDirection, out playerPositionWhenPetting, out isPetSmall);

			// Vanilla code
			/*
			int num = 36; // <--- distance multiplier
			isPetSmall = false;
			switch (nPC.type)
			{
				case 637: // <--- Town Cat
					isPetSmall = true;
					num = 28;
					break;
				case 656:
					isPetSmall = true;
					num = 24;
					break;
				case 670:
				case 678:
				case 679:
				case 680:
				case 681:
				case 683:
					isPetSmall = true;
					num = 26;
					break;
				case 682:
					isPetSmall = true;
					num = 22;
					break;
				case 684:
					isPetSmall = true;
					num = 20;
					break;
			}

			playerPositionWhenPetting = nPC.Bottom + new Vector2(-targetDirection * num, 0f);
			*/

			// If the NPC type is our Town Pet
			if (Main.npc[animalNpcIndex].type == ModContent.NPCType<TutorialTownPet>())
			{
				// Since the default distance multiplier is 36 and our Town Pet mimics the Town Cat which as a multiplier of 28,
				// we will move the position by 8 to match.
				// The position also depends on the width of the NPC's hitbox.
				playerPositionWhenPetting += new Vector2(targetDirection * 8f, 0f);

				// isPetSmall will make the arm angled down. Otherwise it is angled slightly up.
				isPetSmall = true;
			}
		}

		// Since our Town Pet can sit in chairs, we want to them to be moved up visually to match the height of the chair.
		private static float Main_Hook_NPCAddHeight(Terraria.On_Main.orig_NPCAddHeight orig, NPC theNPC)
		{
			// The NPC is our Town Pet and the Town Pet is sitting.
			if (theNPC.type == ModContent.NPCType<TutorialTownPet>() && theNPC.ai[0] == 5f)
			{
				// Move 14 pixels up. This matches the Town Cat.
				return -14f * theNPC.scale;
			}

			// Otherwise, run the vanilla code.
			return orig(theNPC);
		}

		// Still have other issues:
		// Chat bubble when hovering over the NPC isn't shifted up when sitting. Main.DrawNPCChatBubble()
		// Party hat isn't in the correct position. Main.DrawNPCExtras()
	}
}