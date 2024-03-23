using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TownNPCGuide.Content.NPCs.TownNPCs;
using System.IO;
using TownNPCGuide.Content.Items;
using TownNPCGuide.Content.NPCs.TownNPCs.TownPets;
using TownNPCGuide.Content.Currencies;
using Terraria.GameContent.UI;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TownNPCGuide
{
	public class TownNPCGuide : Mod
	{
		internal Asset<Texture2D> VirtualCurrencyCoin; // Where our icon for the virtual currency is stored.

		public override void Load()
		{
			// Load a vanilla item as custom currency.
			VanillaItemAsCurrency.VanillaItemAsCurrencySystem = new VanillaItemAsCurrency(ItemID.Vertebrae, 9999L, "Mods.TownNPCGuide.Currency.VanillaItemAsCurrency");
			VanillaItemAsCurrency.VanillaItemAsCurrencyID = CustomCurrencyManager.RegisterCurrency(VanillaItemAsCurrency.VanillaItemAsCurrencySystem);
			
			// Load the custom currency with the item that used as currency.
			// We set the localization path to the name of the currency.
			TutorialCurrency.TutorialCurrencySystem = new TutorialCurrency(ModContent.ItemType<TutorialCurrencyCoin>(), 9999L, "Mods.TownNPCGuide.Currency.TutorialCurrency");
			TutorialCurrency.TutorialCurrencyID = CustomCurrencyManager.RegisterCurrency(TutorialCurrency.TutorialCurrencySystem);

			// Create a virtual currency. ItemID.None is set as the item because this currency has no item.
			VirtualCurrency.VirtualCurrencySystem = new VirtualCurrency(ItemID.None, 9999L, "Mods.TownNPCGuide.Currency.VirtualCurrency");
			VirtualCurrency.VirtualCurrencyID = CustomCurrencyManager.RegisterCurrency(VirtualCurrency.VirtualCurrencySystem);
		}

		public override void PostSetupContent() {
			//if (ModLoader.TryGetMod("Census", out Mod censusMod)) {
			//	censusMod.Call("TownNPCCondition", ModContent.NPCType<TutorialTownNPC>(), Language.GetTextValue($"Mods.TownNPCGuide.CrossMod.Census.TutorialTownNPC"));
			//}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			TownNPCGuideMessageType msgType = (TownNPCGuideMessageType)reader.ReadByte();

			switch (msgType)
			{
				case TownNPCGuideMessageType.TownPetUnlockOrExchange:
					// Call a custom function that we made in our License item.
					TutorialTownPetLicense.TutorialTownPetUnlockOrExchangePet(ref TownNPCGuideWorld.boughtTutorialTownPet, ModContent.NPCType<TutorialTownPet>(), "Mods.TownNPCGuide.NPCs.TutorialTownPet.UI.LicenseTutorialTownPetUse");
					break;
				default:
					break;
			}
		}
	}

	internal enum TownNPCGuideMessageType : byte
	{
		TownPetUnlockOrExchange
	}
}