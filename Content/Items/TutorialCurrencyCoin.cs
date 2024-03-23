using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TownNPCGuide.Content.Items
{
	public class TutorialCurrencyCoin : ModItem
	{
		public Asset<Texture2D> TutorialCurrencyCoinAnimated; // Where our animated texture is stored.

		public override void SetStaticDefaults()
		{
			// If you want your item to always be animated you can set these two.
			// Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 8));
			// ItemID.Sets.AnimatesAsSoul[Type] = true;

			ItemID.Sets.CoinLuckValue[Type] = 1000; // How much coin luck is created when throwing this item into Shimmer.
			Item.ResearchUnlockCount = 100; // How many items we need to research before it is unlocked for duplication.
		}
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 0;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = Item.CommonMaxStack;

			/* Copper Coin
			width = 10;
			height = 10;
			maxStack = 100;
			value = 5; // Silver Coin: 500
			ammo = AmmoID.Coin;
			shoot = 158;
			notAmmo = true;
			damage = 25;
			shootSpeed = 1f;
			ranged = true;
			useStyle = 1;
			useTurn = true;
			useAnimation = 15;
			useTime = 10;
			autoReuse = true;
			consumable = true;
			createTile = 330;
			noMelee = true;
			*/

			/* Defender Medal
			width = 12;
			height = 12;
			maxStack = CommonMaxStack;
			value = 0;
			rare = 3;
			*/
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			// Hide the "No Value" line when looking in shops.
			TooltipLine tooltipLine = tooltips.FirstOrDefault(x => x.Name == "Price" && x.Mod == "Terraria");
			tooltipLine?.Hide();
		}

		// Used for the animation when thrown in the world.
		private int frame = 0; // Current frame.
		private int frameCounter = 0; // Counter for the frame rate.
		private readonly int frameRate = 6; // 6 ticks per frame; matches vanilla coins.
		private readonly int frameCount = 8; // 8 total frames in our animation.
		
		// Our item is a still frame, but we want it to be animated when we throw it on the ground.
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			// Increase the frame every frameRate ticks and then set it back to 0 if it goes above the frameCount.
			if (frameCounter++ >= frameRate)
			{
				frameCounter = 0;
				frame = ++frame % frameCount;
			}

			// Get the animated texture. Save the result so we only have to load it once.
			TutorialCurrencyCoinAnimated ??= Mod.Assets.Request<Texture2D>("Content/Items/" + Name + "_Animated");

			// Get the frame of the animation.
			Rectangle sourceRectangle = TutorialCurrencyCoinAnimated.Frame(1, 8, frameY: frame);

			// Get the position of the item.
			Vector2 position = Item.position - Main.screenPosition;

			// Draw the item in the world.
			spriteBatch.Draw(TutorialCurrencyCoinAnimated.Value, position, sourceRectangle, lightColor, rotation, default, scale, SpriteEffects.None, 0f);

			return false; // Return false so the original sprite doesn't draw as well.
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			// Spawn some dusts on the coin when thrown in the world.
			if (!Main.gamePaused && Main.instance.IsActive && lightColor.R > 60 && Main.rand.Next(500) - (Math.Abs(Item.velocity.X) + Math.Abs(Item.velocity.Y)) * 10f < (lightColor.R / 50f))
			{
				int sparkleDust = Dust.NewDust(Item.position, Item.width, Item.height, DustID.PlatinumCoin, 0f, 0f, 0, Color.White, 1f);
				Main.dust[sparkleDust].velocity *= 0f;
			}
		}
	}
}