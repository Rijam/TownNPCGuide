using Microsoft.Xna.Framework;
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
			// NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<TutorialTravelingMerchantEmote>();

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
			return "test";
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("LegacyInterface.28"); // This is the key to the word "Shop"
			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
			{
				button2 = "Reroll shop";
			}
			else
			{
				button2 = "Advanced Shop";
			}
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop)
		{
			if (firstButton)
			{
				shop = Shop1; // Opens the shop
			}
			if (!firstButton && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
			{
				simpleShopItems.Clear();
				simpleShopItems.AddRange(SimpleShop.GenerateNewInventoryList());

				advancedShopItems.Clear();
				advancedShopItems.AddRange(AdvancedShop.GenerateNewInventoryList());
			}
			else if (!firstButton)
			{
				shop = Shop2;
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
				NPC.netSkip = -1;
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
				Main.NewText($"spawnTime {spawnTime}");
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

		public const string Shop1 = "Shop1";
		// The list of items in the traveler's shop. Saved with the world and set when the traveler spawns. Synced by the server to clients in multi player
		public readonly static List<Item> simpleShopItems = new();

		// A static instance of the declarative shop, defining all the items which can be brought. Used to create a new inventory when the NPC spawns
		public static SimpleTravelingMerchantShop SimpleShop;

		public const string Shop2 = "Shop2";
		// The list of items in the traveler's shop. Saved with the world and set when the traveler spawns. Synced by the server to clients in multi player
		public readonly static List<Item> advancedShopItems = new();

		// A static instance of the declarative shop, defining all the items which can be brought. Used to create a new inventory when the NPC spawns
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
			SimpleShop = new SimpleTravelingMerchantShop(NPC.type, Shop1)
				.Add(ItemID.CopperPickaxe)
				.Add(ItemID.TinPickaxe)
				.Add(ItemID.IronPickaxe, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.LeadPickaxe, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.SilverPickaxe, Condition.DownedEowOrBoc)
				.Add(ItemID.TungstenPickaxe, Condition.DownedEowOrBoc)
				.Add(new Item(ItemID.GoldPickaxe) { shopCustomPrice = Item.buyPrice(gold: 1) }, Condition.DownedSkeletron)
				.Add(new Item(ItemID.PlatinumPickaxe) { shopCustomPrice = Item.buyPrice(gold: 1, silver: 50) }, Condition.DownedSkeletron);
			SimpleShop.Register();

			AdvancedShop = new AdvancedTravelingMerchantShop(NPC.type, Shop2);
			AdvancedShop.Add(ItemID.CopperPickaxe);
			AdvancedShop.Add(ItemID.TinPickaxe, 1f, Condition.DownedEyeOfCthulhu);
			AdvancedShop.AddPool("Swords", 3)
				.Add(ItemID.CopperBroadsword)
				.Add(ItemID.TinBroadsword)
				.Add(ItemID.IronBroadsword, 0.5f, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.LeadBroadsword, 0.5f, Condition.DownedEyeOfCthulhu)
				.Add(ItemID.SilverBroadsword, 1f, Condition.DownedEowOrBoc)
				.Add(ItemID.TungstenBroadsword, 1f, Condition.DownedEowOrBoc)
				.Add(new Item(ItemID.GoldBroadsword) { shopCustomPrice = Item.buyPrice(gold: 1) }, 2f, Condition.DownedSkeletron)
				.Add(new Item(ItemID.PlatinumBroadsword) { shopCustomPrice = Item.buyPrice(gold: 1, silver: 50) }, 2f, Condition.DownedSkeletron)
				.Add(ItemID.FieryGreatsword, 10f);
			AdvancedShop.AddPool("Axes", 6)
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
			AdvancedShop.AddPool("Vanity", 1)
				.Add(ItemID.CopperHelmet);

			AdvancedShop.Register();
		}
		#endregion
	}

	public class SimpleTravelingMerchantShop(int npcType, string name = "Shop") : AbstractNPCShop(npcType, name)
	{
		public new record Entry(Item Item, List<Condition> Conditions) : AbstractNPCShop.Entry
		{
			IEnumerable<Condition> AbstractNPCShop.Entry.Conditions => Conditions;

			public bool ConditionsMet() => Conditions.All(c => c.IsMet());
		}

		private List<Entry> _entries = [];
		public override IEnumerable<Entry> ActiveEntries => _entries;
		public SimpleTravelingMerchantShop Add(params Entry[] entries)
		{
			_entries.AddRange(entries);
			return this;
		}

		/// <summary> Adds the specified item with the provided conditions to this shop. If all of the conditions are satisfied, the item will be available in the shop. </summary>
		public SimpleTravelingMerchantShop Add(int item, params Condition[] condition) => Add(new Entry(ContentSamples.ItemsByType[item], condition.ToList()));
		public SimpleTravelingMerchantShop Add(Item item, params Condition[] condition) => Add(new Entry(item, condition.ToList()));

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
				if (entry.ConditionsMet() && Main.rand.NextBool(3))
				{
					items.Add(entry.Item);
				}
			}
			if (items.Count == 0)
			{
				Main.NewText("Shop 1 was empty, rerolling.");
				goto Beginning;
			}
			return items;
		}
	}

	public class AdvancedTravelingMerchantShop(int npcType, string name = "Shop") : AbstractNPCShop(npcType, name)
	{

		public new record Entry(Item Item, List<Condition> Conditions, float Weight = 1f) : AbstractNPCShop.Entry
		{
			IEnumerable<Condition> AbstractNPCShop.Entry.Conditions => Conditions;

			public float Weight = Weight;

			public bool Disabled { get; private set; }

			public Entry Disable()
			{
				Disabled = true;
				return this;
			}

			public bool ConditionsMet() => Conditions.All(c => c.IsMet());
		}

		public record Pool(string Name, int Slots, List<Entry> Entries, float Weight)
		{
			public Pool Add(Item item, float weight = 1f, params Condition[] conditions)
			{
				Entries.Add(new Entry(item, conditions.ToList(), weight));
				return this;
			}

			public Pool Add<T>(float weight = 1f, params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), weight, conditions);
			public Pool Add(int item, float weight = 1f, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], weight, conditions);

			// Picks a number of items (up to Slots) from the entries list, provided conditions are met.
			public IEnumerable<Item> PickItems()
			{
				// This is not a fast way to pick items without replacement, but it's certainly easy. Be careful not to do this many many times per frame, or on huge lists of items.
				var list = Entries.Where(e => !e.Disabled && e.ConditionsMet()).ToList();
				for (int i = 0; i < Slots; i++)
				{
					if (list.Count == 0)
						break;

					float totalWeight = list.Sum(x => x.Weight);
					float roll = Main.rand.NextFloat() * totalWeight;
					float cumulative = 0f;

					for (int j = 0; j < list.Count; j++)
					{
						//Main.NewText($"Found Item {list[j].Item.Name} {list[j].Item.netID}");
						cumulative += list[j].Weight;
						if (roll <= cumulative)
						{
							// Main.NewText($"  Chose this item {list[j].Item.Name}");
							yield return list[j].Item;
						}
						list.RemoveAt(j);
					}

					/*
					foreach (var item in list)
					{
						cumulative += item.Weight;
						if (roll <= cumulative)
						{
							Main.NewText($"Chose this item {item.Item.Name}");
							yield return item.Item;
							list.Remove(item);
						}
					}
					*/

					/*
					int k = Main.rand.Next(list.Count);
					yield return list[k].Item;

					// remove the entry from the list so it can't be selected again this pick
					list.RemoveAt(k);
					*/
				}
			}
		}

		public List<Pool> Pools { get; } = new();

		public override IEnumerable<Entry> ActiveEntries => Pools.SelectMany(p => p.Entries).Where(e => !e.Disabled);

		public Pool AddPool(string name, int slots, float weight = 1f)
		{
			var pool = new Pool(name, slots, new List<Entry>(), weight);
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