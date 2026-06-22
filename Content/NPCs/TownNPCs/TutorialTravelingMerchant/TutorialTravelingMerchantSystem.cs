
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TownNPCGuide.Content.NPCs.TownNPCs.TutorialTravelingMerchant
{
	public class TutorialTravelingMerchantSystem : ModSystem
	{
		public override void PreUpdateWorld()
		{
			TutorialTravelingMerchant.UpdateTravelingMerchant();
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["simpleShopItems"] = TutorialTravelingMerchant.simpleShopItems;
			tag["advancedShopItems"] = TutorialTravelingMerchant.advancedShopItems;
			if (TutorialTravelingMerchant.spawnTime != double.MaxValue)
			{
				tag["spawnTime"] = TutorialTravelingMerchant.spawnTime;
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			TutorialTravelingMerchant.simpleShopItems.Clear();
			TutorialTravelingMerchant.simpleShopItems.AddRange(tag.Get<List<Item>>("simpleShopItems"));
			TutorialTravelingMerchant.advancedShopItems.Clear();
			TutorialTravelingMerchant.advancedShopItems.AddRange(tag.Get<List<Item>>("advancedShopItems"));
			if (!tag.TryGet("spawnTime", out TutorialTravelingMerchant.spawnTime))
			{
				TutorialTravelingMerchant.spawnTime = double.MaxValue;
			}
		}

		public override void ClearWorld()
		{
			TutorialTravelingMerchant.simpleShopItems.Clear();
			TutorialTravelingMerchant.advancedShopItems.Clear();
			TutorialTravelingMerchant.spawnTime = double.MaxValue;
		}

		public override void NetSend(BinaryWriter writer)
		{
			// Note that NetSend is called whenever WorldData packet is sent.
			// We use this so that shop items can easily be synced to joining players
			// We recommend modders avoid sending WorldData too often, or filling it with too much data, lest too much bandwidth be consumed sending redundant data repeatedly
			// Consider sending a custom packet instead of WorldData if you have a significant amount of data to synchronise

			writer.Write(TutorialTravelingMerchant.simpleShopItems.Count);
			foreach (Item item in TutorialTravelingMerchant.simpleShopItems)
			{
				ItemIO.Send(item, writer, writeStack: true);
			}

			writer.Write(TutorialTravelingMerchant.advancedShopItems.Count);
			foreach (Item item in TutorialTravelingMerchant.advancedShopItems)
			{
				ItemIO.Send(item, writer, writeStack: true);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			TutorialTravelingMerchant.simpleShopItems.Clear();
			int countSimple = reader.ReadInt32();
			for (int i = 0; i < countSimple; i++)
			{
				TutorialTravelingMerchant.simpleShopItems.Add(ItemIO.Receive(reader, readStack: true));
			}

			TutorialTravelingMerchant.advancedShopItems.Clear();
			int countAdvanced = reader.ReadInt32();
			for (int i = 0; i < countAdvanced; i++)
			{
				TutorialTravelingMerchant.advancedShopItems.Add(ItemIO.Receive(reader, readStack: true));
			}
		}
	}
}