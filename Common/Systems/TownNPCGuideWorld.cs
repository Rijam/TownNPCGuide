using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria;

namespace TownNPCGuide.Common.Systems
{
	// Here is where we save that our Town NPC has been rescued to the world.
	public class TownNPCGuideWorld : ModSystem
	{
		public static bool rescuedTutorialTownNPC = false;
		public static bool boughtTutorialTownPet = false;

		public override void ClearWorld() {
			rescuedTutorialTownNPC = false;
			boughtTutorialTownPet = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			if (rescuedTutorialTownNPC) {
				tag["rescuedTutorialTownNPC"] = true;
			}
			if (boughtTutorialTownPet) {
				tag["boughtTutorialTownPet"] = true;
			}
		}

		public override void LoadWorldData(TagCompound tag) {
			rescuedTutorialTownNPC = tag.ContainsKey("rescuedTutorialTownNPC");
			boughtTutorialTownPet = tag.ContainsKey("boughtTutorialTownPet");
		}

		public override void NetSend(BinaryWriter writer) {
			BitsByte flags = new BitsByte();
			flags[0] = rescuedTutorialTownNPC;
			flags[1] = boughtTutorialTownPet;
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader) {
			BitsByte flags = reader.ReadByte();
			rescuedTutorialTownNPC = flags[0];
			boughtTutorialTownPet = flags[1];
		}
	}
}