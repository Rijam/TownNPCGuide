using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using TownNPCGuide.Content.Items;
using TownNPCGuide.EmoteBubbles;
using TownNPCGuide.Content.Currencies;
using TownNPCGuide.Common.Systems;

namespace TownNPCGuide.Content.NPCs.TownNPCs
{
	// Please do not blindly copy and paste this entire thing. Check the guide for relevant information about the sections.
	// This Town NPC has contains many examples and alternate methods that you likely do not need and/or do not fit your own Town NPC.

	[AutoloadHead]
	public class TutorialTownNPC : ModNPC
	{
		public const string Shop1 = "Shop1"; // The string we assign for the first shop. We define it here so we can use the variable without having the chance to mess up typing the string.
		public const string Shop2 = "Shop2";
		internal static int ShimmerHeadIndex; // The index of the NPC head for when the Town NPC is in its shimmered variant.
		private static Profiles.StackedNPCProfile NPCProfile; // The Town NPC Profile.
		// private static ITownNPCProfile NPCProfile; // Advanced - ITownNPCProfile()

		public override void Load() {
			// Adds our Shimmer Head to the NPCHeadLoader.
			ShimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 25; // The total amount of frames the NPC has. You may need to change this based on how many frames your sprite sheet has.

			NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs. This is the remaining frames after the walking frames.
			NPCID.Sets.AttackFrameCount[Type] = 4; // The amount of frames in the attacking animation.
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the NPC that it tries to attack enemies.
			NPCID.Sets.AttackType[Type] = 0; // The type of attack the Town NPC performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
			NPCID.Sets.AttackTime[Type] = 90; // The amount of time it takes for the NPC's attack animation to be over once it starts. Measured in ticks.
			NPCID.Sets.AttackAverageChance[Type] = 30; // The denominator for the chance for a Town NPC to attack. Lower numbers make the Town NPC appear more aggressive.
			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset. Adjust this number to change where on your NPC's head the party hat sits.
			NPCID.Sets.ShimmerTownTransform[Type] = true; // This set says that the Town NPC has a Shimmered form. Otherwise, the Town NPC will become transparent when touching Shimmer like other enemies.

			NPCID.Sets.MagicAuraColor[Type] = Color.Yellow; // Magic attacks create an aura around the Town NPC. It is white by default, but we can set it to a color here.

			// We could use this set to disallow our Town NPC from opening doors. This defaults to true, so it isn't actually needed in this case.
			// Note: This set ONLY works for Town NPC AI. This set won't do anything for any other types of AI.
			NPCID.Sets.AllowDoorInteraction[Type] = true;

			// Changing the Framing Group will change where the party hat is offset on the sprite.
			// The default is 0 and that matches the player's walking animation.
			// See the guide on the details about the other groups.
			NPCID.Sets.NPCFramingGroup[Type] = 0;

			// Connects this NPC with a custom emote.
			// This makes it when the NPC is in the world, other NPCs will "talk about them".
			// By setting this you don't have to override the PickEmote method for the emote to appear.
			NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<TutorialTownNPCEmote>();

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() {
				Velocity = 1f, // Draws the NPC in the bestiary as if it's walking +1 tiles in the x direction
				Direction = -1 // Faces left.
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			// In SetStaticDefaults(),
			// We use the NPC.Happiness hook to set our happiness. Here we can set our Town NPC's opinions on biomes and other Town NPCs.
			// We can use the chaining syntax similar to recipes to reduce the verbosity of the code.
			// Notice how we don't have any semicolons ; at the end of each line and instead a single semicolon at the very end.
			NPC.Happiness
				// All of the vanilla Town NPCs only LIKE and DISLIKE one biome, but we can set LOVE and HATE as well as many as we want.
				.SetBiomeAffection<HallowBiome>(AffectionLevel.Love) // Our Town NPC will love the Hallow.
				.SetBiomeAffection<ForestBiome>(AffectionLevel.Like) // Our Town NPC will like the Forest.
				.SetBiomeAffection<DesertBiome>(AffectionLevel.Dislike) // Our Town NPC will dislike the Desert.
				.SetBiomeAffection<UndergroundBiome>(AffectionLevel.Dislike) // Our Town NPC will hate the Underground/Caverns/Underworld.
				.SetBiomeAffection<DungeonBiome>(AffectionLevel.Love)

				.SetNPCAffection(NPCID.Guide, AffectionLevel.Love) // Our Town NPC loves living near the Guide.
				.SetNPCAffection(NPCID.PartyGirl, AffectionLevel.Like) // Our Town NPC likes living near the Party Girl.
				.SetNPCAffection(NPCID.BestiaryGirl, AffectionLevel.Like) // Our Town NPC also like living near the Zoologist. Use auto complete to find the NPCIDs.
				.SetNPCAffection(NPCID.DD2Bartender, AffectionLevel.Dislike) // Our Town NPC dislike living near the Tavernkeep. Use auto complete to find the NPCIDs.
				.SetNPCAffection(NPCID.GoblinTinkerer, AffectionLevel.Hate) // Our Town NPC hates living near the Goblin Tinkerer.
				// Use ModContent.NPCType<YourOtherTownNPC>() to set the affection towards the other Town NPCs in your mod.
				// All Town NPCs are automatically set to like the Princess.
			; // < Mind the semicolon!

			// This creates a "profile" for our Town NPC, which allows for different textures during a party and/or while the NPC is shimmered.
			NPCProfile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex, Texture + "_Shimmer_Party")
			);
			
			// Advanced - ITownNPCProfile()
			// NPCProfile = new TutorialTownNPCProfile();
		}
		public override void SetDefaults() {
			NPC.townNPC = true; // Sets NPC to be a Town NPC
			NPC.friendly = true; // The NPC Will not attack player
			NPC.width = 18; // The width of the hitbox (hurtbox)
			NPC.height = 40; // The height of the hitbox (hurtbox)
			NPC.aiStyle = NPCAIStyleID.Passive; // Copies the AI of passive NPCs. This is AI Style 7.
			NPC.damage = 10; // This is the amount of damage the NPC will deal as contact damage. This is NOT the damage dealt by the Town NPC's attack.
			NPC.defense = 15; // All vanilla Town NPCs have a base defense of 15. This will increases as more bosses are defeated.
			NPC.lifeMax = 250; // All vanilla Town NPCs have 250 HP.
			NPC.HitSound = SoundID.NPCHit1; // The sound that is played when the NPC takes damage.
			NPC.DeathSound = SoundID.NPCDeath1; // The sound that is played with the NPC dies.
			NPC.knockBackResist = 0.5f; // All vanilla Town NPCs have 50% knockback resistances. Think of this more as knockback susceptibility. 1f = 0% resistance, 0f = 100% resistance.

			AnimationType = NPCID.Guide; // Sets the animation style to follow the animation of your chosen vanilla Town NPC.
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// The first line is for the background. Auto complete is recommended to see the available options.
				// Generally, this is set to the background of the biome that the Town NPC most loves/likes, but it is not automatic.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
				// Examples for how to modify the background
				// BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Visuals.Blizzard,
				// BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,

				// This line is for the description of the entry. We are accessing a localization key here.
				new FlavorTextBestiaryInfoElement("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Bestiary")
				// You can add several descriptions just by adding another new FlavorTextBestiaryInfoElement
			});
		}

		public override void HitEffect(NPC.HitInfo hitInfo) {
			// Create gore when the NPC is killed.
			// HitEffect() is called every time the NPC takes damage.
			// We need to check that the gore is not spawned on a server and that the NPC is actually dead.
			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
				// Retrieve the gore types. This NPC has shimmer and party variants for head, arm, and leg gore. (8 total gores)
				// The way this is set up is that in the Gores folder, our gore sprites are named "TutorialTownNPC_Gore_[Shimmer]_[Party]_<BodyPart>".
				// For example, the normal head gore is called "TutorialTownNPC_Gore_Head".
				// The shimmered party head gore is called "TutorialTownNPC_Gore_Shimmer_Party_Head".
				// Your naming system does not need to match this, but it is convenient because this the following code will work for all of your Town NPCs.

				string shimmer = ""; // Create an empty string.
				string party = ""; // Create an empty string.
				if (NPC.IsShimmerVariant) {
					shimmer += "_Shimmer"; // If the Town NPC is shimmered, add "_Shimmer" to the file path.
				}
				if (NPC.altTexture == 1) {
					party += "_Party";  // If the Town NPC has a different texture for parties, add "_Party" to the file path.
				}
				int hatGore = NPC.GetPartyHatGore(); // Get the party hat gore for the party hat that the Town NPC is currently wearing.
				int headGore = Mod.Find<ModGore>($"{Name}_Gore{shimmer}{party}_Head").Type; // Find the correct gore.
				int armGore = Mod.Find<ModGore>($"{Name}_Gore{shimmer}_Arm").Type; // {Name} will be replaced with the class name of the Town NPC.
				int legGore = Mod.Find<ModGore>($"{Name}_Gore{shimmer}_Leg").Type; // {shimmer} and {party} will add the extra bits of the string if it exists.

				// Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
				if (hatGore > 0) {
					Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, hatGore); // Spawn the party hat gore if there is one.
				}
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore, 1f);
				// Remember, positive Y values go down.
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
			}
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) {

			// If our Town NPC hasn't been rescued, don't arrive.
			if (!TownNPCGuideWorld.rescuedTutorialTownNPC) {
				return false;
			}

			// Search through all of the players
			for (int k = 0; k < Main.maxPlayers; k++) {
				Player player = Main.player[k];
				// If the player is not active (disconnected/doesn't exist), continue to the next player.
				if (!player.active) {
					continue;
				}

				// Player has at least 100 maximum mana, return true.
				// statManaMax2 includes the "temporary" mana. This includes accessories/potions/etc. that give extra mana.
				if (player.statManaMax2 >= 100) {
					return true;
				}
			}
			return false;
		}

		public override bool CheckConditions(int left, int right, int top, int bottom) {
			// This code is very similar to how the Truffle checks if it is a surface mushroom biome. In this example, we are checking it is a surface Hallow biome instead.

			// If the bottom is below the surface height, return false. Remember, positive Y values go down.
			if (bottom > Main.worldSurface) {
				return false;
			}

			// We call this function to get the bounds of the "biome". We are going to count how many Hallow blocks are in the area and see if it is enough to count as a Hallow biome.
			WorldGen.Housing_GetTestedRoomBounds(out int startX, out int endX, out int startY, out int endY);
			int score = 0;
			for (int i = startX + 1; i < endX; i++) {
				for (int j = startY + 2; j < endY + 2; j++) {
					Tile tile = Main.tile[i, j];
					// If the tile we are searching is Hallowed Grass, Pearlstone, Pearlsand, Pearlsandstone, Hardened Pearlsand, or Pink Ice, increase the score.
					if (tile.HasTile && (tile.TileType == TileID.HallowedGrass || tile.TileType == TileID.Pearlstone || tile.TileType == TileID.Pearlsand || tile.TileType == TileID.HallowSandstone || tile.TileType == TileID.HallowHardenedSand || tile.TileType == TileID.HallowedIce)) {
						score++;
					}
				}
			}

			// If the score matches the threshold for the biome, return true. In this case, 125 tiles are needed to count as a Hallow biome.
			if (score >= SceneMetrics.HallowTileThreshold) {
				return true;
			}

			return false;
		}

		public override ITownNPCProfile TownNPCProfile() {
			return NPCProfile; // Set the Town NPC Profile to the profile we defined in SetStaticDefaults().
		}

		public override List<string> SetNPCNameList() {
			// Return a list of strings for our random name.
			return new List<string>() {
				"blushiemagic",
				"Chicken Bones",
				"jopojelly",
				"Jofairden",
				"Mirsario",
				"Solxan"
			};
		}

		public override string GetChat() {
			// WeightedRandom<string> is an easy and convenient way to add chat. We don't have to deal with making our own randomness or deal with large switch/if else statements.
			WeightedRandom<string> chat = new();

			// Here we have 4 standard messages that the Town NPC can say
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.StandardDialogue1"));
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.StandardDialogue2"));
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.StandardDialogue3"));
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.StandardDialogue4"));
			
			// If the local player has a Terra Toilet in their inventory.
			// We can use Main.LocalPlayer here because GetChat() runs client side. So, the only player who can see the chat is the one chatting with the Town NPC.
			if (Main.LocalPlayer.HasItem(ItemID.TerraToilet)) {
				// This message is 2 times as common.
				chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.PlayerHasTerraToilet"), 2);
			}
			// If the local player is located in the desert.
			// As mentioned briefly before, only players know which biome they are in. Since the player is standing right next to the Town NPC to chat, checking the player is not a big deal.
			if (Main.LocalPlayer.ZoneDesert) {
				// This message is 2 times as rare.
				chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.DesertDialogue"), 0.5);
			}
			// We check that an Angler is present in the world. NPC.FindFirstNPC() returns the index of the NPC in Main.npc[]
			int angler = NPC.FindFirstNPC(NPCID.Angler);
			if (angler >= 0) { // If the Angler is not present, NPC.FindFirstNPC() will return -1.
				// We've set up our localization key value pair in such a way that {0} will be replaced with the name of the Angler.
				// You can use Main.npc[angler].FullName instead if you want it to say "Adam the Angler" instead of just "Adam".
				chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.AnglerDialogue", Main.npc[angler].GivenName));
			}
			// If Moon Lord has been defeated in this world.
			if (NPC.downedMoonlord) {
				// We've set this message up so that {0} will be replaced the name of the player and {1} will be replaced with the name of the world.
				chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.DownedMoonLord", Main.LocalPlayer.name, Main.worldName));
			}

			// Cross mod dialogue

			// First check if the mod is enabled
			if (ModLoader.TryGetMod("ExampleMod", out Mod exampleMod)) {
				// Next, try to find the other Town NPC.
				// Using TryFind<>() is safer than using Find<>()
				// If the NPC we are trying to find doesn't exist, our mod will continue to work.
				if (exampleMod.TryFind<ModNPC>("ExamplePerson", out ModNPC examplePersonModNPC)) {
					int examplePerson = NPC.FindFirstNPC(examplePersonModNPC.Type);
					if (examplePerson >= 0) {
						// FullName will give the given name and their "profession". For example: Blocky the Example Person
						// GivenName will give just the given name of the Town NPC: For example: Blocky
						chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.ExamplePerson", Main.npc[examplePerson].FullName), 2);
					}
				}
			}

			// Tell the player their Virtual Currency Balance and increase it when they talk to the Tutorial Town NPC.
			VirtualCurrencyPlayer virtualCurrencyPlayer = Main.LocalPlayer.GetModPlayer<VirtualCurrencyPlayer>();
			virtualCurrencyPlayer.VirtualCurrencyBalance++;
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.VirtualCurrencyBalance", virtualCurrencyPlayer.VirtualCurrencyBalance));

			return chat;
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			button = Language.GetTextValue("LegacyInterface.28"); // This will automatically be translated to say "Shop".
			
			// We can add a second button to our Town NPC by assigning button2. In this case, the second button will only appear during the day time.
			if (Main.dayTime) {
				button2 = Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.UI.SecondButton");
			}
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			// If the first button, the shop button, was clicked, open the shop.
			if (firstButton) {
				shop = Shop1;
			}
			// If the button that was clicked wasn't the first button, aka it was the second button, do something else.
			// In this case, we are setting the text to something else and opening the second shop.
			// (The Close or Happiness buttons are not considered here.)
			if (!firstButton) {
				// Main.npcChatText = Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.SecondButtonChat");
				shop = Shop2;
			}
		}

		public override void AddShops() {
			// First, create our new shop.
			// The "Type" parameter assigns the shop to our Town NPC.
			// The "Shop1" parameter is a string that refers to which shop to add. We defined this variable at the beginning of our ModNPC.
			// Most Town NPCs only have one shop, but adding multiple shops is easy with this method.
			var npcShop = new NPCShop(Type, Shop1);

			// Here we set the first slot to an Iron Pickaxe.
			// By default, the prices set to the value of the item which is 5 times the sell price.
			npcShop.Add(ItemID.IronPickaxe);
			// Add an item to the next slot.
			npcShop.Add(ItemID.Starfury);
			// Here is how you would set a modded item from your mod.
			npcShop.Add(ModContent.ItemType<TutorialItem>());

			// If want to change the price of an item, you can set the shopCustomPrice value.
			// To do this, we need to create a new Item object with a constructor setting the shopCustomPrice variable.
			// 1 = 1 copper coin, 100 = 1 silver coin, 10000 = 1 gold coin, and 1000000 = 1 platinum coin.
			// In this example, our Stone Blocks will cost 10 copper.
			npcShop.Add(new Item(ItemID.StoneBlock) { shopCustomPrice = 10 });
			// An easier way to set the price is to use Item.buyPrice()
			npcShop.Add(new Item(ItemID.DirtBlock) { shopCustomPrice = Item.buyPrice(silver: 5) });

			// We can use the Condition class to change the availability of items.
			// We put the condition after our item separated by a comma.
			// Here, our item will be available after any Mechanical Boss has been defeated.
			npcShop.Add(ItemID.HallowedBar, Condition.DownedMechBossAny);
			// Here our item will be available during Full Moons and New Moons.
			npcShop.Add(ItemID.MoonCharm, Condition.MoonPhases04);
			// Here our item will be available in the Hallow biome AND while below the surface.
			// Adding multiple conditions will require that ALL conditions are met for the item to appear.
			npcShop.Add(ItemID.CrystalShard, Condition.InHallow, Condition.InBelowSurface);

			// We can create our own conditions if the default ones don't work for us.
			// For this example, we are saying the Boomstick is available if in the Jungle OR Queen Bee has been defeated.
			// Conditions take two arguments: a string and a Func<bool>

			// The string is the availability written in plain language.
			// In our localization file, we have "When in the Jungle or after Queen Bee has been defeated"

			// The Func<bool> is a fancy form of a boolean expression.
			// We can write any expression as long as it will be evaluate as a boolean expression.
			// We can use existing Conditions with IsMet() to get their boolean values.
			// This is convenient because those Conditions are already written and we know they are correct.
			// Alternately, we could write "() => Main.LocalPlayer.ZoneJungle || NPC.downedQueenBee" if we don't want to use the existing Conditions.
			npcShop.Add(ItemID.Boomstick, new Condition(Language.GetTextValue("Mods.TownNPCGuide.Conditions.JungleOrDownedQueenBee"), () => Condition.InJungle.IsMet() || Condition.DownedQueenBee.IsMet()));

			// If we are to use a custom condition that we wrote several times, it might be wise to create our own class.
			// That way we can just define the Condition once instead of every time we need it.
			// This condition is defined in a different class that we created called CustomCondtions.
			npcShop.Add(ItemID.BirdieRattle, CustomConditions.TravelingMerchantPresentOrHardmode);

			// You may be temped to use Main.rand to set the availability of your item, this may not do what you want it do to.
			// In this example, there is a 50% chance of our item being in the shop.
			// However, this will chance will run every time the shop is opened. So, players can just close and reopen the shop to roll the chance again.
			npcShop.Add(ItemID.BoneTorch, new Condition("Not recommended, this won't work", () => Main.rand.NextBool(2)));

			// Finally, we register or shop. If you forget this, nothing will shop up in your shop!
			npcShop.Register();

			// Advanced

			// We can use the chaining syntax on our shops too.
			// Notice how there are no semi colons until the end.
			var npcShop2 = new NPCShop(Type, Shop2)
				.Add(ItemID.Starfury)
				.Add(ItemID.TerraBlade, Condition.DownedPlantera, Condition.Eclipse)
				.Add(ItemID.RainbowRod, Condition.DownedMechBossAll)
				.Add(ItemID.Terragrim, Condition.PlayerCarriesItem(ItemID.EnchantedSword))
				.Add(ItemID.Megashark, Condition.NpcIsPresent(NPCID.ArmsDealer));

			// This will hide one of the items in the shop. It will throw an exception if the item is not found, though.
			npcShop2.GetEntry(ItemID.RainbowRod).Disable();

			// TryGetEntry will not throw an exception if it fails.
			if (npcShop2.TryGetEntry(ItemID.Megashark, out var entry)) {
				// We can add additional conditions.
				entry.AddCondition(Condition.DownedMechBossAny);
			}

			npcShop2.Add(ItemID.LightsBane, Condition.DownedEowOrBoc);
			// The Space Gun will be added to the shop before the Light's Bane
			npcShop2.InsertBefore(ItemID.LightsBane, ItemID.SpaceGun);
			// The Influx Waver will be added after the Terra Blade.
			npcShop2.InsertAfter(ItemID.TerraBlade, ItemID.InfluxWaver, Condition.DownedMartians);

			npcShop2.Add(ItemID.DD2BallistraTowerT1Popper, Condition.DownedOldOnesArmyAny);
			npcShop2.GetEntry(ItemID.DD2BallistraTowerT1Popper).ReserveSlot(); // We reserve this slot it always appear here.

			// Cross mod shop items

			// First check if the mod is enabled
			if (ModLoader.TryGetMod("ExampleMod", out Mod exampleMod)) {
				// Next, try to find the item.
				// Using TryFind<>() is safer than using Find<>()
				// If the item we are trying to find doesn't exist, our mod will continue to work.
				if (exampleMod.TryFind<ModItem>("ExampleJoustingLance", out ModItem exampleJoustingLance)) {
					npcShop2.Add(exampleJoustingLance.Type);
				}
			}

			// Actually, we don't even need to check to see if the mod is enabled.
			// We can specify the mod name when we try to find the item. This is still safe because the item won't get added if TryFind fails.
			if (ModContent.TryFind<ModItem>("ExampleMod/ExamplePaperAirplane", out ModItem examplePaperAirplane)) {
				npcShop2.Add(examplePaperAirplane.Type);
			}

			// Custom Currency Examples
			npcShop2.Add(new Item(ItemID.IronPickaxe) { shopSpecialCurrency = TutorialCurrency.TutorialCurrencyID, shopCustomPrice = 1});
			npcShop2.Add(new Item(ItemID.IronShortsword) { shopSpecialCurrency = TutorialCurrency.TutorialCurrencyID, shopCustomPrice = 25});
			npcShop2.Add(new Item(ItemID.IronBroadsword) { shopSpecialCurrency = TutorialCurrency.TutorialCurrencyID, shopCustomPrice = 50});

			npcShop2.Add(new Item(ItemID.Leather) { shopSpecialCurrency = VanillaItemAsCurrency.VanillaItemAsCurrencyID, shopCustomPrice = 5 });

			npcShop2.Add(new Item(ItemID.EchoBlock) { shopSpecialCurrency = VirtualCurrency.VirtualCurrencyID, shopCustomPrice = 3 });

			npcShop2.Register();
		}

		public override void ModifyActiveShop(string shopName, Item[] items) {
			// ModifyActiveShop() can modify the shop each time the shop is opened (as opposed to once during mod load).
			// This hook should mostly be used for modifying items already in the shop instead of adding new ones.

			// Here is an example using a foreach loop
			foreach (Item item in items) {
				// If the item in the list isn't a real item, continue to the next item in the list.
				if (item is null)
				{
					continue;
				}
				// If the item is a Terragrim, set it's price to double it's value.
				if (item.type == ItemID.Terragrim) {
					item.shopCustomPrice = item.value * 2;
				}
				// If the item is a Starfury in the second shop, set its modifier to Legendary.
				if (item.type == ItemID.Starfury && shopName == NPCShopDatabase.GetShopName(Type, Shop2)) {
					item.Prefix(PrefixID.Legendary);
				}
				// If the player has the Discount Card equipped, then make the prices even cheaper for our Town NPC.
				if (Main.LocalPlayer.discountAvailable) {
					// We want to discount the shopCustomPrice number if it exists. If it doesn't, then we discount the value instead.
					// Item.GetStoreValue() will return the shopCustomPrice if it exists, otherwise it'll return the value.
					// (It is the same as writing `item.shopCustomPrice ?? item.value` if you prefer that.)
					// Changing the price like this is multiplicative with the discount from the Discount Card. (So 0.8f * 0.8f == 0.64f or 36% discount)
					item.shopCustomPrice = (int?)(item.GetStoreValue() * 0.8f);
				}
			}

			// Here is an example using a for loop
			for (int i = 0; i < items.Length; i++) {
				// Here we find the first item that doesn't exist and set it to the Universal Pylon, then we break out of the loop to stop it.
				if (items[i] is null) {
					items[i] = new Item(ItemID.TeleportationPylonVictory);
					break;
				}
			}

			// Here is an example adding an item to the last slot of the shop.
			items[^1] = new Item(ItemID.PoopBlock) { shopCustomPrice = Item.buyPrice(platinum: 1) };
		}

		public override bool CanGoToStatue(bool toKingStatue) {
			// Can teleport to a King Statue
			return toKingStatue;
			// Return `!toKingStatue` for Queen Statues
			// Return `true` for both
		}

		public override void OnGoToStatue(bool toKingStatue) {
			// Display "Woah!" in chat. The message comes from our localization file.
			ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Mods.TownNPCGuide.NPCs.TutorialTownNPC.Dialogue.OnTeleportToStatue"), Color.White);
		}

		public override bool UsesPartyHat() {
			// Is set to true by default. This is not needed unless you want to set it to false.
			return true;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			// Our Town NPC will drop an item only if it is named a specific name.
			// See the Basic NPC Drops and Loot guide for more information about NPC loot.
			LeadingConditionRule specificName = new LeadingConditionRule(new Conditions.NamedNPC("blushiemagic"));
			specificName.OnSuccess(ItemDropRule.Common(ModContent.ItemType<TutorialItem>()));
			npcLoot.Add(specificName);
		}

		public override int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) {
			// Add some more emotes to the list.
			emoteList.Add(EmoteID.CritterBird);
			emoteList.Add(EmoteID.PartyPresent);

			// We can use if statements to change when an emote will be added.
			if (!Main.dayTime) {
				emoteList.Add(EmoteID.EmoteSleep);
			}

			// If the nearest player is in the snow biome, display a blizzard emote.
			// The NPC doesn't know what biome it is in -- it is all based on the player's biome.
			// The player is probably standing pretty close to the Town NPC so it doesn't matter that much.
			if (closestPlayer.ZoneSnow) {
				emoteList.Add(EmoteID.WeatherSnowstorm);
			}

			// Here this emote will only be added if our Town NPC is talking to another specific Town NPC.
			//   The `otherAnchor?.entity is NPC entityNPC` checks that the other thing that our Town NPC is
			//   trying to talk to actually exists and is an NPC. We assign a variable to that and then check
			//   if the NPC type is the Town NPC we want.
			if (otherAnchor?.entity is NPC entityNPC && entityNPC.type == NPCID.Guide) {
				emoteList.Add(EmoteID.EmotionLove);
			}

			// Here this emote will only be added if our Town NPC is talking to a player.
			// You might try to do something like `otherAnchor?.entity is Player` but that will never be true.
			// If the Town NPC is speaking to the player, the otherAnchor is never set, so we'll never know if it is for the player.
			// However, we can look at the NPC AI to find out that ai[0] == 7 and ai[0] == 19 is when the Town NPC will speak to the player.
			if (NPC.ai[0] == 7f || NPC.ai[0] == 19f) {
				// For this example, we clear the list so that it becomes empty.
				// We then add our ModEmote to the list so that is the only thing that our Town NPC can say. 
				emoteList.Clear();
				emoteList.Add(ModContent.EmoteBubbleType<TutorialTownNPCEmote>());
			}

			// Make sure you return base.PickEmote here. Otherwise you'll override all other NPCs and all other emotes.
			return base.PickEmote(closestPlayer, emoteList, otherAnchor);
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
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			// For throwing, shooting, and magic attacks.

			
			// Throwing
			projType = ProjectileID.Shuriken; // Set the type of projectile the Town NPC will attack with.
			attackDelay = 10; // This is the amount of time, in ticks, before the projectile will actually be spawned after the attack animation has started.
			

			/*
			// Shooting
			projType = ProjectileID.Bullet; // Set the type of projectile the Town NPC will attack with.
			attackDelay = 1; // This is the amount of time, in ticks, before the projectile will actually be spawned after the attack animation has started.

			// If the world is Hardmode, change the projectile to something else.
			if (Main.hardMode) {
				projType = ProjectileID.CursedBullet;
			}
			*/

			/*
			// Magic
			projType = ProjectileID.MagicMissile; // Set the type of projectile the Town NPC will attack with.
			attackDelay = 1; // This is the amount of time, in ticks, before the projectile will actually be spawned after the attack animation has started.
			*/
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			// For throwing, shooting, and magic attacks.

			
			// Throwing
			multiplier = 12f; // multiplier is similar to shootSpeed. It determines how fast the projectile will move.
			gravityCorrection = 2f; // This will affect how high the Town NPC will aim to correct for gravity.
			randomOffset = 1f; // This will affect the speed of the projectile (which also affects how accurate it will be).
			

			/*
			// Shooting
			multiplier = 16f; // multiplier is similar to shootSpeed. It determines how fast the projectile will move.
			randomOffset = 0.1f; // This will affect the speed of the projectile (which also affects how accurate it will be).
			*/

			/*
			// Magic
			multiplier = 16f; // multiplier is similar to shootSpeed. It determines how fast the projectile will move.
			randomOffset = 5f; // This will affect the speed of the projectile (which also affects how accurate it will be).
			*/
		}

		public override void TownNPCAttackShoot(ref bool inBetweenShots) {
			// Only used for shooting attacks.

			// If this is true, it means that the Town NPC has already created a projectile and will continue to create projectiles as part of the same attack.
			// This is like how the Steampunker shoots a three round burst with her Clockwork Assault Rifle.
			// inBetweenShots = false;
		}

		public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) {
			// Only used for shooting attacks.

			// Here is an example on how we would change which weapon is displayed. Omit this part if only want one weapon.
			// In Pre-Hardmode, display the first gun.
			if (!Main.hardMode) {
				// This hook takes a Texture2D instead of an int for the item. That means the weapon our Town NPC uses doesn't need to be an existing item.
				// But, that also means we need to load the texture ourselves. Luckily, GetItemDrawFrame() can do the work for us.
				// The first parameter is what you set as the item.
				// Then, there are two "out" parameters. We can use those out parameters.
				Main.GetItemDrawFrame(ItemID.FlintlockPistol, out Texture2D itemTexture, out Rectangle itemRectangle);

				// Set the item texture to the item texture.
				item = itemTexture;

				// This is the source rectangle for the texture that will be drawn.
				// In this case, it is just the entire bounds of the texture because it has only one frame.
				// You could change this if your texture has multiple frames to be animated.
				itemFrame = itemRectangle;

				scale = 1f; // How large the item is drawn.
				horizontalHoldoutOffset = 12; // How close it is drawn to the Town NPC. Adjust this if the item isn't in the Town NPC's hand.

				return; // Return early so the Hardmode code doesn't run.
			}

			// If the world is in Hardmode, change the item to something else.
			Main.GetItemDrawFrame(ItemID.VenusMagnum, out Texture2D itemTexture2, out Rectangle itemRectangle2);
			item = itemTexture2;
			itemFrame = itemRectangle2;
			scale = 0.75f;
			horizontalHoldoutOffset = 15;
		}

		public override void TownNPCAttackMagic(ref float auraLightMultiplier) {
			// Only used for magic attacks.

			// auraLightMultiplier = 1f; // How strong the light is from the magic attack. 1f is the default.
		}

		public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) {
			// Only used for melee attacks.

			/*
			// This is the hitbox of the melee swing. It recommended to set this to the resolution of the sprite you want to use.
			// Below, we've set the Exotic Scimitar as the weapon which has a resolution of 40x48.
			itemWidth = 40;
			itemHeight = 48;
			*/
		}

		public override void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) {
			// Only used for melee attacks.

			/*
			// This hook takes a Texture2D instead of an int for the item. That means the weapon our Town NPC uses doesn't need to be an existing item.
			// But, that also means we need to load the texture ourselves. Luckily, GetItemDrawFrame() can do the work for us.
			// The first parameter is what you set as the item.
			// Then, there are two "out" parameters. We can use those out parameters.
			Main.GetItemDrawFrame(ItemID.DyeTradersScimitar, out Texture2D itemTexture, out Rectangle itemRectangle);

			// To retrieve a sprite from your mod.
			// Texture2D itemTexture2 = ModContent.Request<Texture2D>("TownNPCGuide/Content/NPCs/TownNPCs/SwordWithNoItem").Value;

			// Set the item texture to the item texture.
			item = itemTexture;

			// This is the source rectangle for the texture that will be drawn.
			// In this case, it is just the entire bounds of the texture because it has only one frame.
			// You could change this if your texture has multiple frames to be animated.
			itemFrame = itemRectangle;

			// Set the size of the item to the size of one of the dimensions. This will always be a square, but it doesn't matter that much.
			// itemSize is only used to determine how far into the swing animation it should be.
			itemSize = itemRectangle.Width;

			// The scale affects how far the arc of the swing is from the Town NPC.
			// This is not how large the item will be drawn on the screen.
			// A scale of 0 will draw the swing directly on the Town NPC.
			// We set it to 0.15f so it the arc is slightly in front of the Town NPC.
			scale = 0.15f;

			// offset will change the position of the item.
			// Change this to match with the location of the Town NPC's hand.
			// Remember, positive Y values go down.
			offset = new Vector2(0, 12f);

			// It is also recommended to change the following sets in SetStaticDefaults() to better match the behavior of melee attacks.
			// NPCID.Sets.DangerDetectRange[Type] = 60; // The amount of pixels away from the center of the NPC that it tries to attack enemies.
			// NPCID.Sets.AttackType[Type] = 3; // The type of attack the Town NPC performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
			// NPCID.Sets.AttackTime[Type] = 15; // The amount of time it takes for the NPC's attack animation to be over once it starts. Measured in ticks.
			// NPCID.Sets.AttackAverageChance[Type] = 1; // The denominator for the chance for a Town NPC to attack. Lower numbers make the Town NPC appear more aggressive.

			// And then change the numbers in TownNPCAttackCooldown()
			// cooldown = 12;
			// randExtraCooldown = 6;
			*/
		}

		public override void PostAI() {
			// Main.NewText("NPC.ai[0] " + NPC.ai[0] + " NPC.ai[1] " + NPC.ai[1] + " NPC.ai[2] " + NPC.ai[2] + " NPC.ai[3] " + NPC.ai[3]);
			// Main.NewText("NPC.localAI[0] " + NPC.localAI[0] + " NPC.localAI[1] " + NPC.localAI[1] + " NPC.localAI[2] " + NPC.localAI[2] + " NPC.localAI[3] " + NPC.localAI[3]);

			// NPC.ai[0] = current action
			//	0 == no action
			//  1 == walking
			//  2 == 
			//  3-4 == chatting
			//  5 == sitting
			//  7 == Talking to player
			//  8 == Decide to run from enemy? Immediately set to 1.
			//  9 == Go sit in a chair?
			//  10 == attacking
			//  12-15 == attacking related
			//	16 == player rock paper scissors
			//  19 == Talking to player (responding to the player)
			//  24 == magic attack aura
			//  25 == change to a different action

			// NPC.ai[1] = time before changing actions
			//  299 when talking to a player

			// NPC.ai[2] = The player index that the Town NPC is speaking to? (Player.whoAmI) 
			//  Reset to 0 most of the time

			// NPC.ai[3] =
			//  Used by the Old Man

			// NPC.localAI[0] is open
			//  This is used by the Mechanic to know to stay still while her boomerang is returning to her.
			// NPC.localAI[1] = Attacking cooldown
			//  0 most of the time
			// NPC.localAI[2] =
			//  1 most of the time
			// NPC.localAI[3] = Attacking animation time
			//  -1 or 0 most of the time when not attacking
			//  -99 when talking to a player

			// If just started to attack.
			if (NPC.ai[0] == 10 && NPC.localAI[3] == 1) {
				EmoteBubble.NewBubble(EmoteID.ItemSword, new WorldUIAnchor(NPC), 120); // Display an emote above their head.
			}
			// Standing still about to do something else and very hurt.
			if (NPC.ai[0] == 0 && NPC.ai[1] == 10 && NPC.life < NPC.lifeMax * 0.25f) {
				EmoteBubble.NewBubble(EmoteID.ItemTombstone, new WorldUIAnchor(NPC), 120); // Display an emote above their head.
			}
			// If talking to the player.
			if (NPC.ai[0] == 7 || NPC.ai[0] == 19) {
				NPC.AddBuff(BuffID.Lovestruck, 2); // Give them a buff.
			}
		}

		// Load our textures
		private readonly Asset<Texture2D> glowMask = ModContent.Request<Texture2D>("TownNPCGuide/Content/NPCs/TownNPCs/Advanced/TutorialTownNPC_GlowMask");
		private readonly Asset<Texture2D> glowMaskAttacking = ModContent.Request<Texture2D>("TownNPCGuide/Content/NPCs/TownNPCs/Advanced/TutorialTownNPC_GlowMaskAttacking");
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			// Flip the glow mask to match which direction the Town NPC is facing.
			SpriteEffects spriteEffects = NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			// Draw our glow mask in full bright.
			Color color = Color.White;

			// Move the position up by 4 pixels plus the gfxOffY value (that is for climbing half blocks).
			// Main.NPCAddHeight() makes it so if the Town NPC is sitting, it also moves the glow mask up by 4 more pixels.
			Vector2 verticalOffset = new(0, -4 + NPC.gfxOffY + Main.NPCAddHeight(NPC));

			// Draw our glow mask
			spriteBatch.Draw(glowMask.Value, NPC.Center - screenPos + verticalOffset, NPC.frame, color, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, spriteEffects, 1f);

			// Only draw our extra attacking glow mask while attacking, which are frames 21+
			if (NPC.frame.Y > 20 * NPC.frame.Height) { 
				spriteBatch.Draw(glowMaskAttacking.Value, NPC.Center - screenPos + verticalOffset, NPC.frame, color, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, spriteEffects, 1f);
			}
		}
	}

	// Defining our own Town NPC Profile gives us a little more freedom, but is rarely needed.
	public class TutorialTownNPCProfile : ITownNPCProfile {
		public int RollVariation() => 0; // Used if our NPC has different variants like the Town Pets do.

		// If your Town NPC has no random names, return null here.
		public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

		// Load of all our textures. Doing it this way will only load them once during mod load time.
		private readonly Asset<Texture2D> bestiaryTexture = ModContent.Request<Texture2D>("TownNPCGuide/Content/NPCs/TownNPCs/Advanced/TutorialTownNPC_BestiaryExample");
		private readonly Asset<Texture2D> shimmeredAndParty = ModContent.Request<Texture2D>("TownNPCGuide/Content/NPCs/TownNPCs/TutorialTownNPC_Shimmer_Party");
		private readonly Asset<Texture2D> shimmeredAndNotParty = ModContent.Request<Texture2D>("TownNPCGuide/Content/NPCs/TownNPCs/TutorialTownNPC_Shimmer");
		private readonly Asset<Texture2D> notShimmeredAndParty = ModContent.Request<Texture2D>("TownNPCGuide/Content/NPCs/TownNPCs/TutorialTownNPC_Party");
		private readonly Asset<Texture2D> notShimmeredAndNotParty = ModContent.Request<Texture2D>("TownNPCGuide/Content/NPCs/TownNPCs/TutorialTownNPC");
		private readonly int headSlot = ModContent.GetModHeadSlot("TownNPCGuide/Content/NPCs/TownNPCs/TutorialTownNPC_Head");

		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) {
			// Here we can give our Town NPC a bunch of different textures if we wanted.
			// For example, we can make the texture in the Bestiary different.
			if (npc.IsABestiaryIconDummy && !npc.ForcePartyHatOn) {
				return bestiaryTexture;
			}

			// Shimmered and party
			if (npc.IsShimmerVariant && npc.altTexture == 1) {
				return shimmeredAndParty;
			}
			// Shimmered and no party
			if (npc.IsShimmerVariant && npc.altTexture != 1) {
				return shimmeredAndNotParty;
			}
			// Not shimmered and party
			if (!npc.IsShimmerVariant && npc.altTexture == 1) {
				return notShimmeredAndParty;
			}
			// Not shimmered and no party
			return notShimmeredAndNotParty;
		}

		public int GetHeadTextureIndex(NPC npc) {
			if (npc.IsShimmerVariant) {
				return TutorialTownNPC.ShimmerHeadIndex;
			}
			return headSlot;
		}

		// In our Town NPC's class, we'll need to change a few things:
		// internal static int ShimmerHeadIndex; // Change private to internal so we can access it here.
		// private static ITownNPCProfile NPCProfile; // Change Profiles.StackedNPCProfile to ITownNPCProfile.

		// In SetStaticDefaults(), change
		//	NPCProfile = new Profiles.StackedNPCProfile(
		//		new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
		//		new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex, Texture + "_Shimmer_Party")
		//	);
		// To:
		// NPCProfile = new TutorialTownNPCProfile();
	}
}