using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using TownNPCGuide.Content.Items;

namespace TownNPCGuide.Content.NPCs.TownNPCs.TutorialSkeletonMerchant
{
	public class TutorialSkeletonMerchant : ModNPC
	{
		private static Profiles.StackedNPCProfile NPCProfile; // The Town NPC Profile.

		public override void Load() {
			base.Load();
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 23; // The total amount of frames the NPC has. You may need to change this based on how many frames your sprite sheet has.
			NPCID.Sets.ExtraFramesCount[Type] = 7; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs. This is the remaining frames after the walking frames.
			NPCID.Sets.AttackFrameCount[Type] = 2;  // The amount of frames in the attacking animation.
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
			NPCID.Sets.PrettySafe[Type] = 300;
			NPCID.Sets.AttackType[Type] = 2; // The type of attack the Town NPC performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
			NPCID.Sets.AttackTime[Type] = 60; // The amount of time it takes for the NPC's attack animation to be over once it starts.
			NPCID.Sets.AttackAverageChance[Type] = 10; // The denominator for the chance for a Town NPC to attack. Lower numbers make the Town NPC appear more aggressive. Vanilla Skeleton Merchant is 30.
			NPCID.Sets.HatOffsetY[Type] = 3; // For when a party is active, the party hat spawns at a Y offset.
			NPCID.Sets.NPCFramingGroup[Type] = 3; // Change where the party hat is offset on the sprite. 3 matches the Truffle. Use 8 for pre-determined no offsets.
			NPCID.Sets.ShimmerTownTransform[Type] = true; // This set says that the Town NPC has a Shimmered form. Otherwise, the Town NPC will become transparent when touching Shimmer like other enemies.

			NPCID.Sets.MagicAuraColor[Type] = Color.LightBlue; // Magic attacks create an aura around the Town NPC. It is white by default, but we can set it to a color here.

			// This sets entry is the most important part of this NPC. Since it is true, it tells the game that we want this NPC to act like a town NPC without ACTUALLY being one.
			// What that means is: the NPC will have the AI of a town NPC, will attack like a town NPC, and have a shop (or any other additional functionality if you wish) like a town NPC.
			// However, the NPC will not have their head displayed on the map, will de-spawn when no players are nearby or the world is closed, and will spawn like any other NPC.
			NPCID.Sets.ActsLikeTownNPC[Type] = true;

			// This prevents the happiness button
			NPCID.Sets.NoTownNPCHappiness[Type] = true;

			// To reiterate, since this NPC isn't technically a town NPC, we need to tell the game that we still want this NPC to have a custom/randomized name when they spawn.
			// In order to do this, we simply make this hook return true, which will make the game call the TownNPCName method when spawning the NPC to determine the NPC's name.
			NPCID.Sets.SpawnsWithCustomName[Type] = true;

			// Connects this NPC with a custom emote.
			// This makes it when the NPC is in the world, other NPCs will "talk about him".
			// NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleBoneMerchantEmote>();

			// The vanilla Skeleton Merchant cannot interact with doors (open or close them, specifically), but if you want your NPC to be able to interact with them despite this,
			// uncomment this line below.
			// NPCID.Sets.AllowDoorInteraction[Type] = true;

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
				Direction = -1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, -1, Texture + "_Party"),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", -1, Texture + "_Shimmer_Party")
			);

			// In vanilla, Skeleton enemies can't harm the Skeleton Merchant.
			// You might try to add this set so Skeletons can't harm your NPC, but it won't work like that.
			// Adding this set just means your NPC can't harm the Skeleton Merchant.
			// NPCID.Sets.Skeletons[Type] = true;
		}
		public override void SetDefaults() {
			NPC.friendly = true; // NPC Will not attack player
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 30; // Skeleton Merchant has some extra defense to help them survive. (Other Town NPCs have 15 defense).
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			NPC.npcSlots = 7f; // Takes up extra enemy spawning spots. That means fewer enemies spawn when this NPC is around.
			NPC.rarity = 1; // Shows up on the Lifeform Analyzer. 1 is the lowest priority.

			AnimationType = NPCID.Guide;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the preferred biomes of this town NPC listed in the bestiary.
				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				// Sets your NPC's flavor text in the bestiary.
				new FlavorTextBestiaryInfoElement("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Bestiary")
			});
			bestiaryEntry.Info.RemoveAll((IBestiaryInfoElement x) => x is NPCKillCounterInfoElement); // Hides the kill count in the Bestiary.
		}

		public override void HitEffect(NPC.HitInfo hit) {
			// Create gore when the NPC is killed.
			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
				// Retrieve the gore types. 
				string variant = "";
				if (NPC.IsShimmerVariant) {
					variant += "_Shimmer"; // If the Town NPC is shimmered, add "_Shimmer" to the file path.
				}
				if (NPC.altTexture == 1) {
					variant += "_Party";  // If the Town NPC has a different texture for parties, add "_Party" to the file path.
				}
				int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
				int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
				int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;
				int hatGore = Mod.Find<ModGore>($"{Name}_Gore_Hat").Type;
				int partyHatGore = NPC.GetPartyHatGore(); // Get the party hat gore for the party hat that the Town NPC is currently wearing.

				if (partyHatGore > 0) {
					Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, partyHatGore); // Spawn the party hat gore if there is one.
				}
				else {
					Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * 3f, hatGore);
				}

				// Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
			}
		}

		public override ITownNPCProfile TownNPCProfile() {
			return NPCProfile;
		}

		public override List<string> SetNPCNameList() {
			// Automatically get the list of possible names from the localization file.
			// A benefit of doing it this way is you can add/remove/change names in the localization file without having to edit the code here.
			// Another benefit of this approach is a separate mod can add a Mods.TownNPCGuide.NPCs.TutorialSkeletonMerchant.Names key and it will automatically be used as an name option.
			return Language.FindAll(Lang.CreateDialogFilter(this.GetLocalizationKey("Names"))).Select(x => x.Value).ToList();
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// If any player is the caverns layer and doesn't already exist, the Tutorial Skeleton Merchant will have a slight chance to spawn.
			if (spawnInfo.Player.ZoneRockLayerHeight && NPC.CountNPCS(Type) == 0) {
				return 0.14f;
			}

			// Else, the Tutorial Skeleton Merchant will not spawn if the above conditions are not met.
			return 0f;
		}

		// Make sure to allow your NPC to chat, since being "like a town NPC" doesn't automatically allow for chatting.
		public override bool CanChat() {
			return true;
		}

		public override string GetChat() {
			WeightedRandom<string> chat = new WeightedRandom<string>();

			// These are things that the NPC has a chance of telling you when you talk to it.
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialSkeletonMerchant.Dialogue.StandardDialogue1"));
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialSkeletonMerchant.Dialogue.StandardDialogue2"));
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialSkeletonMerchant.Dialogue.StandardDialogue3"));
			chat.Add(Language.GetTextValue(this.GetLocalizationKey("Dialogue.StandardDialogue4"))); // this.GetLocalizationKey("") Will automatically get the "Mods.ModName.Category.ContentType.ContentName" part.
			return chat; // chat is implicitly cast to a string.
		}

		public override void SetChatButtons(ref string button, ref string button2) { // What the chat buttons are when you open up the chat UI
			button = Language.GetTextValue("LegacyInterface.28"); //This is the key to the word "Shop"
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				shop = "Shop";
			}
		}

		public override void AddShops() {
			// Some custom conditions for the shop items. These are copied from the Skeleton Merchant, thus the localization keys already defined in tModLoader.
			Condition boneTorchCondition = new Condition("Conditions.Periodically", () => Main.time % 60 <= 30);
			Condition torchCondition = new Condition("Conditions.Periodically", () => Main.time % 60 > 30);
			Condition artisanCondition = new Condition("Conditions.NoAteLoaf", () => !Main.LocalPlayer.ateArtisanBread);

			new NPCShop(Type) // Default name for the shop is "Shop".
				.Add<TutorialItem>()
				.Add(ItemID.Bone)
				.Add(ItemID.BoneTorch, boneTorchCondition)
				.Add(ItemID.Torch, torchCondition)
				.Add(ItemID.ArtisanLoaf, artisanCondition, Condition.MoonPhasesNearNew)
				.Register();
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			// The amount of base damage the attack will do.
			// This is NOT the same as NPC.damage (that is for contact damage).
			// Remember, the damage will increase as more bosses are defeated.
			damage = 20;
			// The amount of knockback the attack will deal.
			// This value does not scale like damage does.
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			// How long, in ticks, the Town NPC must wait before they can attack again.
			// The actual length will be: cooldown <= length < (cooldown + randExtraCooldown)
			cooldown = 10;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			// For throwing, shooting, and magic attacks.

			// Magic
			projType = ProjectileID.MagicMissile; // Set the type of projectile the Town NPC will attack with.
			attackDelay = 1; // This is the amount of time, in ticks, before the projectile will actually be spawned after the attack animation has started.
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			// For throwing, shooting, and magic attacks.

			// Magic
			multiplier = 16f; // multiplier is similar to shootSpeed. It determines how fast the projectile will move.
			randomOffset = 5f; // This will affect the speed of the projectile (which also affects how accurate it will be).
		}

		public override void TownNPCAttackMagic(ref float auraLightMultiplier) {
			// Only used for magic attacks.
			auraLightMultiplier = 1.5f; // How strong the light is from the magic attack. 1f is the default.
		}

		// You might try to add UsesPartyHat so your NPC can wear a party hat during parties. Unfortunately, it isn't that simple.
		// Party hat eligibility is only checked for real Town NPCs (NPC.townNPC), which our NPC here is not.
		// public override bool UsesPartyHat() {
		// 	return true;
		// }
	}
}