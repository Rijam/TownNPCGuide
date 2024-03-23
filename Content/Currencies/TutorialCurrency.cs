using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Localization;

namespace TownNPCGuide.Content.Currencies
{
	public class TutorialCurrency : CustomCurrencySingleCoin
	{
		public static TutorialCurrency TutorialCurrencySystem; // Set in the mod class
		public static int TutorialCurrencyID; // Set in the mod class

		public TutorialCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap)
		{
			this.CurrencyTextKey = CurrencyTextKey; // The name of the currency as a localization key.
			CurrencyTextColor = Color.LightBlue; // The color that the price line will be.

			// How large the icon for the currency will draw in the "Savings" list.
			// The Savings list will show up if you have this currency in your Piggy Bank and purchase an item that costs this currency.
			// Defender Medals are set to 0.8f
			CurrencyDrawScale = 1f; 
		}
		
		// The name of the currency is set to all lowercase by default.
		// So it'll show up as "Buy Price: 1 tutorial currency coin" for example.
		// Here is how the line that says the buy price is set.
		// This example is the same as vanilla but without the .ToLower() to leave it the original case.
		public override void GetPriceText(string[] lines, ref int currentLine, long price)
		{
			Color lineColor = CurrencyTextColor * (Main.mouseTextColor / 255f);
			lines[currentLine++] = $"[c/{lineColor.R:X2}{lineColor.G:X2}{lineColor.B:X2}:{Lang.tip[50].Value} {price} {Language.GetTextValue(CurrencyTextKey)}]";
		}

		// There are several other overrides that you can use for currencies as well, but you are unlikely to need them.
		// Accepts(Item item)
		//		Can decide whether or not you can use that currency to buy things.
		//		By default you can if the item is currency.
		// CombineStacks(out bool overFlowing, params long[] coinCounts)
		//		Used for combining stacks of coins into higher denominations.
		//		Doesn't actually do the stacking, just returns a number of what the stack is.
		// CountCurrency(out bool overFlowing, Item[] inv, params int[] ignoreSlots)
		//		Counts how much of the currency you have only in your inventory.
		// DrawSavingsMoney(SpriteBatch sb, string text, float shopx, float shopy, long totalCoins, bool horizontal = false)
		//		When purchasing and item that costs this currency and the currency is in your Piggy Bank, a "Savings" section will aprear to the right of the shop window.
		//		This show how many of this currency you have in your Piggy Bank.
		//		You can easily change the scale of the sprite by changing CurrencyDrawScale in the constructor.
		// GetItemExpectedPrice(Item item, out long calcForSelling, out long calcForBuying)
		//		Calls item.GetStoreValue(); and returns that value for both out parameters.
		// TryPurchasing(long price, List<Item[]> inv, List<Point> slotCoins, List<Point> slotsEmpty, List<Point> slotEmptyBank, List<Point> slotEmptyBank2, List<Point> slotEmptyBank3, List<Point> slotEmptyBank4)
		//		A complex method that tries to buy the shop item if you have the currency in your inventory, Piggy Bank, Safe, Defender's Forge, or Void Bag/Vault.
	}
}