using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TownNPCGuide.Common.Systems;

namespace TownNPCGuide.Content.Items
{
	public class TutorialItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.value = 10000;
			Item.maxStack = Item.CommonMaxStack;
			Item.useStyle = ItemUseStyleID.RaiseLamp;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.autoReuse = true;
		}

		// This is just to reset the bool for testing purposes.
		public override bool? UseItem(Player player) {
			TownNPCGuideWorld.rescuedTutorialTownNPC = false;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.WorldData);
			}
			return true;
		}
		public override bool CanRightClick() {
			return true;
		}
		public override void RightClick(Player player) {
			TownNPCGuideWorld.rescuedTutorialTownNPC = true;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.WorldData);
			}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			string currentState = TownNPCGuideWorld.rescuedTutorialTownNPC.ToString();
			tooltips.Add(new TooltipLine(Mod, "CurrentState", "rescuedTutorialTownNPC == " + currentState));
		}
	}
}