using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using TownNPCGuide.Content.EmoteBubbles;

namespace TownNPCGuide.Content.NPCs.TownNPCs.TutorialTravelingMerchant
{
	[AutoloadHead]
	public class TutorialTravelingMerchant : ModNPC
	{
		private static Profiles.StackedNPCProfile NPCProfile; // The Town NPC Profile.
		internal static int ShimmerHeadIndex; // The index of the NPC head for when the Town NPC is in its shimmered variant.

		public override void Load()
		{
			// Adds our Shimmer Head to the NPCHeadLoader.
			ShimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		}

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 25; // The total amount of frames the NPC has. You may need to change this based on how many frames your sprite sheet has.
			NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs. This is the remaining frames after the walking frames.
			NPCID.Sets.AttackFrameCount[Type] = 4;  // The amount of frames in the attacking animation.
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
			NPCID.Sets.PrettySafe[Type] = 300;
			NPCID.Sets.AttackType[Type] = 0; // The type of attack the Town NPC performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
			NPCID.Sets.AttackTime[Type] = 60; // The amount of time it takes for the NPC's attack animation to be over once it starts.
			NPCID.Sets.AttackAverageChance[Type] = 10; // The denominator for the chance for a Town NPC to attack. Lower numbers make the Town NPC appear more aggressive. Vanilla Skeleton Merchant is 30.
			NPCID.Sets.HatOffsetY[Type] = 3; // For when a party is active, the party hat spawns at a Y offset.
			NPCID.Sets.NPCFramingGroup[Type] = 0; // Change where the party hat is offset on the sprite. 3 matches the Truffle. Use 8 for pre-determined no offsets.
			NPCID.Sets.ShimmerTownTransform[Type] = true; // This set says that the Town NPC has a Shimmered form. Otherwise, the Town NPC will become transparent when touching Shimmer like other enemies.

			// This prevents the happiness button
			NPCID.Sets.NoTownNPCHappiness[Type] = true;

			// Connects this NPC with a custom emote.
			// This makes it when the NPC is in the world, other NPCs will "talk about him".
			NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<TutorialTravelingMerchantEmote>();

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
			{
				Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
				Direction = -1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture)),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex)
			);

			// Here we define which portrait to use for the Town NPC when the portrait style setting is set to detailed.
			NPCID.Sets.NPCPortraits.Add(Type, NPCID.Sets.PrioritizedPortrait()
				.With(NPCID.Sets.ShimmeredPortraitCondition, NPCID.Sets.BasicPortrait($"{Texture}_Shimmer_Portrait"))
				.Default(NPCID.Sets.BasicPortrait($"{Texture}_Portrait")));
			NPCID.Sets.NPCPortraitsCloseUpOffsets.Add(Type, new Vector2(0f, 0f)); // Here we can change the offsets of Town NPC when the portrait style setting is set to profile.
			NPCID.Sets.NPCPortraitsFullBodyRetroOffsets.Add(Type, new Vector2(0f, 0f)); // Here we can change the offsets of Town NPC when the portrait style setting is set to retro.
		}

		public override void SetDefaults()
		{
			NPC.townNPC = true; // This NPC is Town NPC
			NPC.friendly = true; // NPC Will not attack player
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			AnimationType = NPCID.DD2Bartender;
			TownNPCStayingHomeless = true; // This Town NPC doesn't move into homes.
		}

		#region Normal Town NPC Things

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange([
				// Sets the preferred biomes of this town NPC listed in the bestiary.
				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				// Sets your NPC's flavor text in the bestiary.
				new FlavorTextBestiaryInfoElement("Mods.TownNPCGuide.NPCs.TutorialTravelingMerchant.Bestiary")
			]);
		}

		public override void HitEffect(NPC.HitInfo hitInfo)
		{
			// Create gore when the NPC is killed.
			// HitEffect() is called every time the NPC takes damage.
			// We need to check that the gore is not spawned on a server and that the NPC is actually dead.
			if (Main.netMode != NetmodeID.Server && NPC.life <= 0)
			{
				// Retrieve the gore types. This NPC has shimmer and party variants for head, arm, and leg gore. (8 total gores)
				// The way this is set up is that in the Gores folder, our gore sprites are named "TutorialTownNPC_Gore_[Shimmer]_[Party]_<BodyPart>".
				// For example, the normal head gore is called "TutorialTownNPC_Gore_Head".
				// The shimmered party head gore is called "TutorialTownNPC_Gore_Shimmer_Party_Head".
				// Your naming system does not need to match this, but it is convenient because this the following code will work for all of your Town NPCs.

				string shimmer = ""; // Create an empty string.
				if (NPC.IsShimmerVariant)
				{
					shimmer += "_Shimmer"; // If the Town NPC is shimmered, add "_Shimmer" to the file path.
				}
				int hatGore = NPC.GetPartyHatGore(); // Get the party hat gore for the party hat that the Town NPC is currently wearing.
				int headGore = Mod.Find<ModGore>($"{Name}_Gore{shimmer}_Head").Type; // Find the correct gore.
				int armGore = Mod.Find<ModGore>($"{Name}_Gore{shimmer}_Arm").Type; // {Name} will be replaced with the class name of the Town NPC.
				int legGore = Mod.Find<ModGore>($"{Name}_Gore{shimmer}_Leg").Type; // {shimmer} and {party} will add the extra bits of the string if it exists.

				// Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
				if (hatGore > 0)
				{
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

		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			return false; // This should always be false, because we spawn in the Traveling Merchant manually
		}

		public override ITownNPCProfile TownNPCProfile()
		{
			return NPCProfile;
		}

		public override List<string> SetNPCNameList()
		{
			return [
				"Traveler",
				"Journeyer",
				"Hiker",
				"Tourist",
				"Adventurer",
				"Commuter",
				"Voyager",
				"Wanderer"
			];
		}

		public override string GetChat()
		{
			WeightedRandom<string> chat = new WeightedRandom<string>();

			// These are things that the NPC has a chance of telling you when you talk to it.
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTravelingMerchant.Dialogue.StandardDialogue1"));
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTravelingMerchant.Dialogue.StandardDialogue2"));
			chat.Add(Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTravelingMerchant.Dialogue.StandardDialogue3"));
			chat.Add(Language.GetTextValue(this.GetLocalizationKey("Dialogue.StandardDialogue4"))); // this.GetLocalizationKey("") Will automatically get the "Mods.ModName.Category.ContentType.ContentName" part.
			return chat; // chat is implicitly cast to a string.
		}

		// This hooks is where we register which buttons will show up when interacting the Town NPC.
		// The "Close", "Happiness", and "Housing" buttons are automatically registered first.
		// This NPC isn't affected by happiness, so the "Happiness" and "Housing" buttons won't show up.
		public override void RegisterChatButtons(NPCInteractionList interactions)
		{
			// Here is one way to assign a Shop button to our NPC.
			// In this example, we are assigning the button to be at the beginning of the list.
			// The shop name we pass in NPCInteractions.Shop() needs to be the same name as what we use to register the NPCShop.
			interactions.InsertBefore(NPCInteractions.Shop(Shop1Simple, "Mods.TownNPCGuide.NPCs.TutorialTravelingMerchant.UI.SimpleShop"), NPCInteractionDatabase.CloseButton);
			interactions.InsertBefore(NPCInteractions.Shop(Shop2Advanced, "Mods.TownNPCGuide.NPCs.TutorialTravelingMerchant.UI.AdvancedShop"), NPCInteractionDatabase.CloseButton);
			interactions.InsertAfter(new RerollShopsButton(), NPCInteractionDatabase.CloseButton);
		}

		// This is a custom button to reroll the shops.
		public class RerollShopsButton : NPCInteraction
		{
			public override string GetText() => Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTravelingMerchant.UI.RerollShops");
			public override bool Condition() => true;
			public override void Interact()
			{
				Main.npcChatText = Language.GetTextValue("Mods.TownNPCGuide.NPCs.TutorialTravelingMerchant.Dialogue.RerollShopDialogue");
				Main.DoNPCPortraitHop();

				simpleShopItems.Clear();
				simpleShopItems.AddRange(SimpleShop.GenerateNewInventoryList());

				advancedShopItems.Clear();
				advancedShopItems.AddRange(AdvancedShop.GenerateNewInventoryList());
			}
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			// The amount of base damage the attack will do.
			// This is NOT the same as NPC.damage (that is for contact damage).
			// Remember, the damage will increase as more bosses are defeated.
			damage = 20;
			// The amount of knockback the attack will deal.
			// This value does not scale like damage does.
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			// How long, in ticks, the Town NPC must wait before they can attack again.
			// The actual length will be: cooldown <= length < (cooldown + randExtraCooldown)
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			// Throwing
			projType = ProjectileID.Shuriken; // Set the type of projectile the Town NPC will attack with.
			attackDelay = 10; // This is the amount of time, in ticks, before the projectile will actually be spawned after the attack animation has started.
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			// Throwing
			multiplier = 12f; // multiplier is similar to shootSpeed. It determines how fast the projectile will move.
			gravityCorrection = 2f; // This will affect how high the Town NPC will aim to correct for gravity.
			randomOffset = 1f; // This will affect the speed of the projectile (which also affects how accurate it will be).
		}

		#endregion

		#region Traveling Merchant Specific Things

		// Time of day for traveler to leave (6PM)
		public const double despawnTime = 48600.0;

		// the time of day the traveler will spawn (double.MaxValue for no spawn). Saved and loaded with the world in TravelingMerchantSystem
		public static double spawnTime = double.MaxValue;

		public override bool PreAI()
		{
			if ((!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) // If it's past the despawn time and the NPC isn't onscreen
			{
				// Here we despawn the NPC and send a message stating that the NPC has despawned
				// LegacyMisc.35 is {0) has departed!
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.NewText(Language.GetTextValue("LegacyMisc.35", NPC.FullName), 50, 125, 255);
				}
				else
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.35", NPC.GetFullNetName()), new Color(50, 125, 255));
				}
				NPC.active = false;
				NPC.life = 0;
				return false;
			}

			return true;
		}

		public override void AI()
		{
			NPC.homeless = true; // Make sure it stays homeless
		}

		public static void UpdateTravelingMerchant()
		{
			bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<TutorialTravelingMerchant>()) != -1); // Find a Merchant if there's one spawned in the world

			// Main.time is set to 0 each morning, and only for one update. Sundialling will never skip past time 0 so this is the place for 'on new day' code
			if (Main.dayTime && Main.time == 0)
			{
				// insert code here to change the spawn chance based on other conditions (say, NPCs which have arrived, or milestones the player has passed)
				// You can also add a day counter here to prevent the merchant from possibly spawning multiple days in a row.

				// NPC won't spawn today if it stayed all night
				if (!travelerIsThere && Main.rand.NextBool(1))
				{ // 4 = 25% Chance
				  // Here we can make it so the NPC doesn't spawn at the EXACT same time every time it does spawn
					spawnTime = GetRandomSpawnTime(5400, 8100); // minTime = 6:00am, maxTime = 7:30am
				}
				else
				{
					spawnTime = double.MaxValue; // no spawn today
				}
				// Main.NewText($"spawnTime {spawnTime}");
			}

			// Spawn the traveler if the spawn conditions are met (time of day, no events, no sundial)
			if (!travelerIsThere && CanSpawnNow())
			{
				int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<TutorialTravelingMerchant>(), 1); // Spawning at the world spawn
				NPC traveler = Main.npc[newTraveler];
				traveler.homeless = true;
				traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
				traveler.netUpdate = true;

				// Prevents the traveler from spawning again the same day
				spawnTime = double.MaxValue;

				// Announce that the traveler has spawned in!
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.NewText(Language.GetTextValue("Announcement.HasArrived", traveler.FullName), 50, 125, 255);
				}
				else
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", traveler.GetFullNetName()), new Color(50, 125, 255));
				}
			}
		}

		private static bool CanSpawnNow()
		{
			// can't spawn if any events are running
			if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
				return false;

			// can't spawn if the sundial is active
			if (Main.IsFastForwardingTime())
				return false;

			// can spawn if daytime, and between the spawn and despawn times
			return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
		}

		private static bool IsNpcOnscreen(Vector2 center)
		{
			int w = NPC.sWidth + NPC.safeRangeX * 2;
			int h = NPC.sHeight + NPC.safeRangeY * 2;
			Rectangle npcScreenRect = new((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
			foreach (Player player in Main.ActivePlayers)
			{
				// If any player is close enough to the traveling merchant, it will prevent the npc from despawning
				if (player.getRect().Intersects(npcScreenRect))
				{
					return true;
				}
			}
			return false;
		}

		public static double GetRandomSpawnTime(double minTime, double maxTime)
		{
			// A simple formula to get a random time between two chosen times
			return (maxTime - minTime) * Main.rand.NextDouble() + minTime;
		}

		public const string Shop1Simple = "Shop1Simple";
		/// <summary>
		/// The list of items in the traveler's shop. Saved with the world and set when the traveler spawns. Synced by the server to clients in multi player
		/// </summary>
		public readonly static List<Item> simpleShopItems = new();

		/// <summary>
		/// A static instance of the declarative shop, defining all the items which can be brought. Used to create a new inventory when the NPC spawns
		/// </summary>
		public static SimpleTravelingMerchantShop SimpleShop;

		public const string Shop2Advanced = "Shop2Advanced";
		/// <summary>
		/// The list of items in the traveler's shop. Saved with the world and set when the traveler spawns. Synced by the server to clients in multi player
		/// </summary>
		public readonly static List<Item> advancedShopItems = new();

		/// <summary>
		/// A static instance of the declarative shop, defining all the items which can be brought. Used to create a new inventory when the NPC spawns
		/// </summary>
		public static AdvancedTravelingMerchantShop AdvancedShop;

		public override void OnSpawn(IEntitySource source)
		{
			simpleShopItems.Clear();
			simpleShopItems.AddRange(SimpleShop.GenerateNewInventoryList());

			advancedShopItems.Clear();
			advancedShopItems.AddRange(AdvancedShop.GenerateNewInventoryList());

			// In multi player, ensure the shop items are synced with clients (see TravelingMerchantSystem.cs)
			if (Main.netMode == NetmodeID.Server)
			{
				// We recommend modders avoid sending WorldData too often, or filling it with too much data, lest too much bandwidth be consumed sending redundant data repeatedly
				// Consider sending a custom packet instead of WorldData if you have a significant amount of data to synchronize
				NetMessage.SendData(MessageID.WorldData);
			}
		}

		public override void AddShops()
		{
			// The Simple Shop contains a bunch of items that are randomly selected to be shown with equal weight. The number of items chosen is not guaranteed.
			SimpleShop = new SimpleTravelingMerchantShop(Type, Shop1Simple)
				.Add(ItemID.CopperPickaxe)
				.Add(ItemID.TinPickaxe)
				.Add(ItemID.IronPickaxe, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.LeadPickaxe, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.SilverPickaxe, Condition.DownedEowOrBoc)
				.Add(ItemID.TungstenPickaxe, Condition.DownedEowOrBoc)
				.Add(new Item(ItemID.GoldPickaxe) { shopCustomPrice = Item.buyPrice(gold: 1) }, Condition.DownedSkeletron)
				.Add(new Item(ItemID.PlatinumPickaxe) { shopCustomPrice = Item.buyPrice(gold: 1, silver: 50) }, Condition.DownedSkeletron);
			SimpleShop.Register();

			// The Advanced Shop contains several categories of items (we've called pools) with a certain number of items from each pool being chosen and each item in the pool having weights.
			AdvancedShop = new AdvancedTravelingMerchantShop(Type, Shop2Advanced);

			AdvancedShop.Add(ItemID.CopperPickaxe); // This item is not in a pool, so it is guaranteed to show up.
			AdvancedShop.Add(ItemID.TinPickaxe, conditions: Condition.DownedEyeOfCthulhu);

			// Add a pool of items called "Swords" with 3 slots. 3 items from this pool will be chosen.
			AdvancedShop.AddPool("Swords", slots: 3)
				.Add(ItemID.CopperBroadsword) // Weight of 1. Bigger values means the item is more likely to be chosen.
				.Add(ItemID.TinBroadsword)
				.Add(ItemID.IronBroadsword, 0.5f, Condition.DownedEyeOfCthulhu) // Weight of 0.5f
				.Add(ItemID.LeadBroadsword, 0.5f, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.SilverBroadsword, 1f, Condition.DownedEowOrBoc)
				.Add(ItemID.TungstenBroadsword, 1f, Condition.DownedEowOrBoc)
				.Add(new Item(ItemID.GoldBroadsword) { shopCustomPrice = Item.buyPrice(gold: 1) }, 2f, Condition.DownedSkeletron)
				.Add(new Item(ItemID.PlatinumBroadsword) { shopCustomPrice = Item.buyPrice(gold: 1, silver: 50) }, 2f, Condition.DownedSkeletron)
				.Add(ItemID.FieryGreatsword, 10f);
			// Add a poll of items called "Axes" with 6 slots. 6 items from this pool will be chosen.
			AdvancedShop.AddPool("Axes", slots: 6)
				.Add(ItemID.CopperAxe, 5f)
				.Add(ItemID.TinAxe, 3f)
				.Add(ItemID.IronAxe, 2.5f, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.LeadAxe, 1.5f, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.SilverAxe, 1f, Condition.DownedEowOrBoc)
				.Add(ItemID.TungstenAxe, 0.75f, Condition.DownedEowOrBoc)
				.Add(ItemID.GoldAxe, 0.25f, Condition.DownedSkeletron)
				.Add(ItemID.PlatinumAxe, 0.15f, Condition.DownedSkeletron)
				.Add(ItemID.WarAxeoftheNight, 0.05f, Condition.Hardmode)
				.Add(ItemID.BloodLustCluster, 0.025f, Condition.Hardmode)
				.Add(ItemID.LunarHamaxeSolar, 25f, Condition.DownedMoonLord);
			// Add a poll of items called "Helmets" with 1 slot. 1 item from this pool will be chosen.
			AdvancedShop.AddPool("Helmets", slots: 1)
				.Add(ItemID.CopperHelmet, 3f)
				.Add(ItemID.TinHelmet, 3f)
				.Add(ItemID.IronHelmet, 2f)
				.Add(ItemID.LeadHelmet, 2f)
				.Add(ItemID.SilverHelmet)
				.Add(ItemID.TungstenHelmet)
				.Add(ItemID.GoldHelmet, 0.5f)
				.Add(ItemID.PlatinumHelmet, 0.5f);

			// This pool says there should be 5 items chosen from it, but we've only defined 2 possible items. Those 2 items will always be chosen.
			AdvancedShop.AddPool("TooFewItems", slots: 5)
				.Add(ItemID.CopperChainmail)
				.Add(ItemID.TinChainmail);
			// This pool has items who's conditions are always false. Nothing will show up.
			AdvancedShop.AddPool("NoAvailableItems", slots: 2)
				.Add(ItemID.CopperGreaves, conditions: new Condition("Not available", () => false))
				.Add(ItemID.TinGreaves, conditions: new Condition("Not available", () => false));

			// This pool will have more items based on a condition.
			// In this case it is if the player is facing left or right for demonstration. This wouldn't work in multiplayer.
			// Another more useful example for Hardmode: new Func<int>(() => Main.hardMode ? 5 : 2)
			AdvancedShop.AddPool("MoreItemsAfterCondition", new Func<int>(() => Main.LocalPlayer.direction == -1 ? 5 : 2))
				.Add(ItemID.DirtBlock, 5f)
				.Add(ItemID.ClayBlock, 4f)
				.Add(ItemID.MudBlock, 2f)
				.Add(ItemID.StoneBlock, 1f)
				.Add(ItemID.SiltBlock, 0.5f)
				.Add(ItemID.AshBlock, 0.25f);

			AdvancedShop.Register();
		}
		#endregion
	}

	/// <summary>
	/// A simple shop that inherits AbstractNPCShop. This shop unintelligently randomly selects items from the list of available items.
	/// </summary>
	public class SimpleTravelingMerchantShop(int npcType, string name = "Shop") : AbstractNPCShop(npcType, name)
	{
		private List<Entry> _entries = [];
		protected override IEnumerable<Entry> AllEntries => _entries;
		public SimpleTravelingMerchantShop Add(params Entry[] entries)
		{
			_entries.AddRange(entries);
			return this;
		}


		// Here are the two methods that are used in AddShops() to add items.
		/// <summary> Adds the specified item with the provided conditions to this shop. If all of the conditions are satisfied, the item will be available in the shop. </summary>
		public SimpleTravelingMerchantShop Add(int item, params Condition[] condition) => Add(new AbstractNPCShop.Entry(ContentSamples.ItemsByType[item], condition));
		/// <summary> Adds the specified item with the provided conditions to this shop. If all of the conditions are satisfied, the item will be available in the shop. </summary>
		public SimpleTravelingMerchantShop Add(Item item, params Condition[] condition) => Add(new AbstractNPCShop.Entry(item, condition));

		public override void FillShop(ICollection<Item> items, NPC npc)
		{
			// use the items which were selected when the NPC spawned.
			foreach (var item in TutorialTravelingMerchant.simpleShopItems)
			{
				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
				items.Add(item.Clone());
			}
		}

		public override void FillShop(Item[] items, NPC npc, out bool overflow)
		{
			overflow = false;
			int i = 0;
			// use the items which were selected when the NPC spawned.
			foreach (var item in TutorialTravelingMerchant.simpleShopItems)
			{

				if (i == items.Length - 1)
				{
					// leave the last slot empty for selling
					overflow = true;
					return;
				}

				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
				items[i++] = item.Clone();
			}
		}

		// Here is where we actually 'roll' the contents of the shop
		public List<Item> GenerateNewInventoryList()
		{
			Beginning:
			var items = new List<Item>();
			foreach (var entry in _entries)
			{
				// Unintelligently rolls if the item should be in the shop.
				// This means the shop could end up with all possible items or 0 items.
				if (entry.ConditionsMet() && Main.rand.NextBool(3)) 
				{
					items.Add(entry.Item);
				}
			}
			if (items.Count == 0) // If the shop was empty, lets roll again so we can get at least one item.
			{
				Main.NewText("Shop 1 was empty, rerolling.");
				goto Beginning; // Warning: If no item could be rolled because of their condition, this will get stuck in an infinite loop.
			}
			return items;
		}
	}

	/// <summary>
	/// This shop system is a more complex.
	/// <br/> It allows for normal items that are always available.
	/// <br/> It allows selecting a number of items from a pools of items that weighted.
	/// </summary>
	/// <param name="npcType"></param>
	/// <param name="name"></param>
	public class AdvancedTravelingMerchantShop(int npcType, string name = "Shop") : AbstractNPCShop(npcType, name)
	{
		/// <summary>
		/// Similar to AbstractNPCShop.Entry, but with the weight added on.
		/// </summary>
		public class WeightedEntry(Item item, List<Condition> condition, float weight = 1f) : AbstractNPCShop.Entry(item, condition.ToArray())
		{
			/// <summary> The weight of the item. Bigger the number the more likely it is to be chosen. </summary>
			public float Weight = weight;

			// The following is already implemented by AbstractNPCShop.Entry; here is what it does:
			/*
			public Item Item { get; } = item;

			private readonly List<Condition> _conditions = condition;
			IEnumerable <Condition> Conditions => _conditions;

			public bool Disabled { get; private set; }

			public void Disable() => Disabled = true;

			public void AddCondition(Condition condition)
			{
				ArgumentNullException.ThrowIfNull(condition, nameof(condition));
				_conditions.Add(condition);
			}

			public bool ConditionsMet()
			{
				foreach (var c in _conditions) {
					if (!c.IsMet())
						return false;
				}

				return true;
			}
			*/
		}

		/// <summary>
		/// Creates a pool of items.
		/// </summary>
		/// <param name="Name">The name of the pool.</param>
		/// <param name="Slots">The number of items from the pool to choose. Func&lt;int&gt; to allow for dynamic sizes.</param>
		/// <param name="Entries">The item with condition and weight.</param>
		public record Pool(string Name, Func<int> Slots, List<WeightedEntry> Entries)
		{
			public Pool Add(Item item, float weight = 1f, params Condition[] conditions)
			{
				Entries.Add(new WeightedEntry(item, conditions.ToList(), weight));
				return this;
			}

			// Here are the two methods that are used in AddShops() to add items.
			/// <summary> Adds the specified item with the provided conditions to this pool. If all of the conditions are satisfied, the item will be available in the shop. </summary>
			public Pool Add<T>(float weight = 1f, params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), weight, conditions);
			/// <summary> Adds the specified item with the provided conditions to this pool. If all of the conditions are satisfied, the item will be available in the shop. </summary>
			public Pool Add(int item, float weight = 1f, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], weight, conditions);

			// Picks a number of items (up to Slots) from the entries list, provided conditions are met.
			public IEnumerable<Item> PickItems()
			{
				// This is not a fast way to pick items without replacement, but it's certainly easy. Be careful not to do this many many times per frame, or on huge lists of items.
				List<WeightedEntry> list = Entries.Where(e => !e.Disabled && e.ConditionsMet()).ToList();

				// The order of the items in the list can have an effect on the outcome.
				// For extra randomness, shuffle the list.
				// list = list.Shuffle().ToList();

				// Each pool has the number of slots it should choose
				for (int i = 0; i < Slots.Invoke(); i++)
				{
					// If there are no items to choose, then don't choose any items.
					if (list.Count == 0)
					{
						break;
					}

					float totalWeight = list.Sum(x => x.Weight); // Sum up the total weight of all of the items in the pool.
					float roll = Main.rand.NextFloat() * totalWeight; // Generate a random number from 0 to 1 and multiply it by the total weight.
					float cumulative = 0f; // Keep track of the running total weight of each item (used below).

					// For each item in the pool
					for (int j = 0; j < list.Count; j++)
					{
						// Main.NewText($"{i} {Slots} {j} {list.Count} Found Item {list[j].Item.Name} {list[j].Item}");
						cumulative += list[j].Weight; // Add the item's weight to the running total

						// If the roll was less than or equal to the running total, add the item to the shop.
						if (roll <= cumulative) 
						{
							// Main.NewText($"  Chose this item {list[j].Item.Name}");
							yield return list[j].Item; // Return the item that was selected.
							list.RemoveAt(j); // Remove the item from the list so it can't be selected again.
							break; // Stop trying to roll for items in this slot because we already selected an item.
						}
						// If an item was not selected, try the next item in the list.
					}
				}
				/* Big example:
					The Swords pool is like this:
						Choose 3 items from this list
						|		Item		|	Weight	|
						CopperBroadsword		1f
						TinBroadsword			1f
						IronBroadsword			0.5f
						LeadBroadsword			0.5f
						SilverBroadsword		1f
						TungstenBroadsword		1f
						GoldBroadsword			2f
						PlatinumBroadsword		2f
						FieryGreatsword			10f

					For slot number 1:
						Add up all of the weights of the items:							totalWeight = 19f
						Generate a random number:										roll = (0.554 * 19) = 10.526
						Keep track of the running total weights, which right now is 0:	cumulative = 0f
						
						For each item in the pool:
							The first item is the CopperBroadsword with a weight of 1
								Add it's weight to the running total:	cumulative += 1 == 1
								Is the roll <= to the cumulative?
									roll == 10.526 and cumulative == 1, so no.
									Move onto the next item.
							The next item is the TinBroadsword with a weight of 1
								Add it's weight to the running total:	cumulative += 1 == 2
								Is the roll <= to the cumulative?
									roll == 10.526 and cumulative == 2, so no.
									Move onto the next item.
							...
							Nothing has been chosen yet until we reach the FieryGreatsword with a weight of 10
								Add it's weight to the running total:	cumulative += 10 == 19
								Is the roll <= to the cumulative?
									roll == 10.526 and cumulative == 19, so yes!
									Add the FieryGreatsword
									Remove it from the list so it cannot be chosen again.
						Now we've selected the first item out of the three, so lets choose the next item.

					For slot number 2:
						Add up all of the weights of the items:							totalWeight = 9f
																							Remember that the FieryGreatsword is no longer in the list, so the total weight is less.
						Generate a random number:										roll = (0.2206 * 9) = 1.986
						Keep track of the running total weights, which right now is 0:	cumulative = 0f

						For each item in the pool:
							The first item is the CopperBroadsword with a weight of 1
								Add it's weight to the running total:	cumulative += 1 == 1
								Is the roll <= to the cumulative?
									roll == 1.986 and cumulative == 1, so no.
									Move onto the next item.
							The next item is the TinBroadsword with a weight of 1
								Add it's weight to the running total:	cumulative += 1 == 2
								Is the roll <= to the cumulative?
									roll == 1.986 and cumulative == 2, so yes!
									Add the TinBroadsword
									Remove it from the list so it cannot be chosen again.
						Now we've selected the second item out of the three, so lets choose the next item.

					For slot number 3:
						Add up all of the weights of the items:							totalWeight = 8f
						Generate a random number:										roll = (0.221 * 8) = 1.768
						Keep track of the running total weights, which right now is 0:	cumulative = 0f
					
						For each item in the pool:
							The first item is the CopperBroadsword with a weight of 1
								Add it's weight to the running total:	cumulative += 1 == 1
								Is the roll <= to the cumulative?
									roll == 1.768 and cumulative == 1, so no.
									Move onto the next item.
							The next item is the IronBroadsword (Not the TinBroadsword because that was already selected) with a weight of 0.5
								Add it's weight to the running total:	cumulative += 0.5 == 1.5
								Is the roll <= to the cumulative?
									roll == 1.986 and cumulative == 1.5, so no.
									Move onto the next item.
							The next item is the LeadBroadsword with a weight of 0.5
								Add it's weight to the running total:	cumulative += 0.5 == 2
								Is the roll <= to the cumulative?
									roll == 1.986 and cumulative == 2, so yes!
									Add the LeadBroadsword
									Remove it from the list so it cannot be chosen again.
						Now we've selected all three items, so we are done!

					The order of the items in the list can have an effect on the outcome.
					For extra randomness, we could shuffle the list first. list = list.Shuffle().ToList();
				*/
			}
		}

		public List<Pool> Pools { get; } = new();

		public IEnumerable<WeightedEntry> WeightedEntries => Pools.SelectMany(p => p.Entries).Where(e => !e.Disabled);

		protected override IEnumerable<Entry> AllEntries => Pools.SelectMany(p => p.Entries);

		public override void RefreshItems(bool onlyIfVariantChanged = true)
		{
			foreach (var entry in WeightedEntries)
			{
				entry.Item.Refresh(onlyIfVariantChanged);
			}
		}

		public Pool AddPool(string name, int slots)
		{
			var pool = new Pool(name, new Func<int>( () => slots ), new List<WeightedEntry>());
			Pools.Add(pool);
			return pool;
		}

		public Pool AddPool(string name, Func<int> slots)
		{
			var pool = new Pool(name, slots, new List<WeightedEntry>());
			Pools.Add(pool);
			return pool;
		}

		// Some methods to add a pool with a single item
		public void Add(Item item, float weight = 1f, params Condition[] conditions) => AddPool(item.ModItem?.FullName ?? $"Terraria/{item.type}", slots: 1).Add(item, weight, conditions);
		public void Add<T>(float weight = 1f, params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), weight, conditions);
		public void Add(int item, float weight = 1, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], weight, conditions);

		// Here is where we actually 'roll' the contents of the shop
		public List<Item> GenerateNewInventoryList()
		{
			var items = new List<Item>();
			foreach (var pool in Pools)
			{
				items.AddRange(pool.PickItems());
			}
			return items;
		}

		public override void FillShop(ICollection<Item> items, NPC npc)
		{
			// use the items which were selected when the NPC spawned.
			foreach (var item in TutorialTravelingMerchant.advancedShopItems)
			{
				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
				items.Add(item.Clone());
			}
		}

		public override void FillShop(Item[] items, NPC npc, out bool overflow)
		{
			overflow = false;
			int i = 0;
			// use the items which were selected when the NPC spawned.
			foreach (var item in TutorialTravelingMerchant.advancedShopItems)
			{

				if (i == items.Length - 1)
				{
					// leave the last slot empty for selling
					overflow = true;
					return;
				}

				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
				items[i++] = item.Clone();
			}
		}
	}
}
