using Terraria;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent;

namespace TownNPCGuide.Content.Currencies
{
	public class VirtualCurrency : CustomCurrencySingleCoin
	{
		public static VirtualCurrency VirtualCurrencySystem; // Set in the mod class
		public static int VirtualCurrencyID; // Set in the mod class

		public VirtualCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap)
		{
			this.CurrencyTextKey = CurrencyTextKey; // The name of the currency as a localization key.
			CurrencyTextColor = Color.LimeGreen; // The color that the price line will be.

			// How large the icon for the currency will draw in the "Savings" list.
			// The Savings list will show up if you have this currency in your Piggy Bank and purchase an item that costs this currency.
			// Defender Medals are set to 0.8f
			CurrencyDrawScale = 1f;
		}

		// The name of the currency is set to all lowercase by default.
		// So it'll show up as "Buy Price: 1 virtual currency" for example.
		// Here is how the line that says the buy price is set.
		// This example is the same as vanilla but without the .ToLower() to leave it the original case.
		public override void GetPriceText(string[] lines, ref int currentLine, long price)
		{
			Color lineColor = CurrencyTextColor * (Main.mouseTextColor / 255f);
			lines[currentLine++] = $"[c/{lineColor.R:X2}{lineColor.G:X2}{lineColor.B:X2}:{Lang.tip[50].Value} {price} {Language.GetTextValue(CurrencyTextKey)}]";
		}

		// Important: Needed so you can buy items without having the currency item.
		public override bool Accepts(Item item)
		{
			return true;
		}

		public override bool TryPurchasing(long price, List<Item[]> inv, List<Point> slotCoins, List<Point> slotsEmpty, List<Point> slotEmptyBank, List<Point> slotEmptyBank2, List<Point> slotEmptyBank3, List<Point> slotEmptyBank4)
		{
			// Purchasing logic.
			VirtualCurrencyPlayer modplayer = Main.LocalPlayer.GetModPlayer<VirtualCurrencyPlayer>();
			if (modplayer.VirtualCurrencyBalance >= price)
			{
				modplayer.VirtualCurrencyBalance -= price;
				return true;
			}
			return false;
		}

		// This shows up on the right side of the shop window.
		// Normally, this only shows up if you have the currency items in your Piggy Bank, Safe, Defender's Forge, or Void Bag/Vault, but this currency has no item.
		public override void DrawSavingsMoney(SpriteBatch sb, string text, float shopx, float shopy, long totalCoins, bool horizontal = false)
		{
			// How to get a texture from the currency item. Not needed for this example since this currency has no item.
			// int item = _valuePerUnit.Keys.ElementAt(0);
			// Main.instance.LoadItem(item);
			// Texture2D drawTexture = TextureAssets.Item[item].Value;
			
			totalCoins--; // We increased the actual count in CombineStacks by 1. Correct that here.

			// Here we are loading a custom texture to show in the Savings section.
			TownNPCGuide modInstance = ModContent.GetInstance<TownNPCGuide>();
			modInstance.VirtualCurrencyCoin ??= modInstance.Assets.Request<Texture2D>("Content/Currencies/VirtualCurrencyCoin");

			// Draw the texture and amount.
			int shift = ((totalCoins > 99) ? (-6) : 0);
			sb.Draw(modInstance.VirtualCurrencyCoin.Value, new Vector2(shopx + 11f, shopy + 75f), null, Color.White, 0f, modInstance.VirtualCurrencyCoin.Size() / 2f, CurrencyDrawScale, SpriteEffects.None, 0f);
			Utils.DrawBorderStringFourWay(sb, FontAssets.ItemStack.Value, totalCoins.ToString(), shopx + (float)shift, shopy + 75f, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
		}

		// Important: Needed so you can buy items without having the currency item.
		public override long CountCurrency(out bool overFlowing, Item[] inv, params int[] ignoreSlots)
		{
			overFlowing = false;
			return Main.LocalPlayer.GetModPlayer<VirtualCurrencyPlayer>().VirtualCurrencyBalance;
		}

		// Needed for DrawSavingsMoney to show up at all times.
		public override long CombineStacks(out bool overFlowing, params long[] coinCounts)
		{
			overFlowing = false;
			// Add one so it'll never be 0. If it is 0, DrawSavingsMoney doesn't show up.
			return Main.LocalPlayer.GetModPlayer<VirtualCurrencyPlayer>().VirtualCurrencyBalance + 1;
		}
	}

	// A simple ModPlayer that has our balance and saves our balance.
	// Use ModSystem for a per world currency instead of per player.
	public class VirtualCurrencyPlayer : ModPlayer
	{
		public long VirtualCurrencyBalance = 0; // Our balance for the virtual currency.

		public override void SaveData(TagCompound tag)
		{
			tag["VirtualCurrencyBalance"] = VirtualCurrencyBalance;
		}

		public override void LoadData(TagCompound tag)
		{
			VirtualCurrencyBalance = tag.GetAsLong("VirtualCurrencyBalance");
		}
	}
}