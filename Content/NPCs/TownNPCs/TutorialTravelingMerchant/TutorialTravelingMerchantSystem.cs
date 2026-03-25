
using Terraria.ModLoader;

namespace TownNPCGuide.Content.NPCs.TownNPCs.TutorialTravelingMerchant
{
	public class TutorialTravelingMerchantSystem : ModSystem
	{
		public override void PreUpdateWorld()
		{
			TutorialTravelingMerchant.UpdateTravelingMerchant();
		}
	}
}