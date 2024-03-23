using Terraria;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework;

namespace TownNPCGuide.Content.Currencies
{
	public class VanillaItemAsCurrency : CustomCurrencySingleCoin
	{
		// The item we use for this currency is set in the mod class.

		public static VanillaItemAsCurrency VanillaItemAsCurrencySystem; // Set in the mod class
		public static int VanillaItemAsCurrencyID; // Set in the mod class

		public VanillaItemAsCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap)
		{
			this.CurrencyTextKey = CurrencyTextKey; // The name of the currency as a localization key.
			CurrencyTextColor = Color.Salmon; // The color that the price line will be.

			// How large the icon for the currency will draw in the "Savings" list.
			// The Savings list will show up if you have this currency in your Piggy Bank and purchase an item that costs this currency.
			// Defender Medals are set to 0.8f
			CurrencyDrawScale = 1f;
		}

		// The name of the currency is set to all lowercase by default.
		// See TutorialCurrceny on how to change that.
	}
}