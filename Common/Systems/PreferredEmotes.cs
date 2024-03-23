using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;
using TownNPCGuide.Content.NPCs.TownNPCs;

namespace TownNPCGuide.Common.Systems.PreferredEmotes
{
	public class PreferredEmotes : ModSystem // ModSystem or GlobalNPC will work.
	{
		public override void Load()
		{
			// The method we want to detour is Terraria.GameContent.UI.EmoteBubble.ProbeExceptions
			// Here we load our hook.
			// Terraria.GameContent.UI.On_EmoteBubble.ProbeExceptions += EmoteBubble_Hook_ProbeExceptions;

			// Here we load our other hook.
			Terraria.GameContent.UI.On_EmoteBubble.GetPosition += EmoteBubble_Hook_GetPosition;
		}

		/* No longer needed now that the PickEmote hook was added in GlobalNPC
		// EmoteBubble.ProbeExceptions is private and not static in vanilla, so we need to make a delegate of it.
		private delegate void orig_EmoteBubble_ProbeExceptions(EmoteBubble self);

		// Here we define our hook.
		private static void EmoteBubble_Hook_ProbeExceptions(Terraria.GameContent.UI.On_EmoteBubble.orig_ProbeExceptions orig, EmoteBubble self, List<int> list, Player plr, WorldUIAnchor other)
		{
			orig(self, list, plr, other); // Call orig first, so that the vanilla code runs before ours.

			NPC nPC = (NPC)self.anchor.entity;

			// If the NPC displaying the emotes is our Town NPC
			if (nPC.type == ModContent.NPCType<TutorialTownNPC>())
			{
				// Add some more emotes to the list.
				list.Add(EmoteID.CritterBird);
				list.Add(EmoteID.PartyPresent);

				// We can use if statements to change when an emote will be added.
				if (!Main.dayTime)
				{
					list.Add(EmoteID.EmoteSleep);
				}

				// Here this emote will only be added if our Town NPC is talking to another specific Town NPC.
				if (other != null && ((NPC)other.entity).type == NPCID.Guide)
				{
					list.Add(EmoteID.EmotionLove);
				}
			}
		}
		*/

		private delegate void orig_EmoteBubble_GetPosition(EmoteBubble self);

		private static Vector2 EmoteBubble_Hook_GetPosition(Terraria.GameContent.UI.On_EmoteBubble.orig_GetPosition orig, EmoteBubble self, out SpriteEffects effect)
		{
			// Only change for our Town NPC.
			if (self.anchor.type == WorldUIAnchor.AnchorType.Entity && self.anchor.entity is NPC npc && npc.type == ModContent.NPCType<TutorialTownNPC>())
			{
				// Flip the bubble to the opposite side of where it is normally.
				effect = ((self.anchor.entity.direction == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

				// Move it to the front of the entity and move it out more.
				float distance = 2f; // The 0.75f multiplier in the original code moves it closer (bigger numbers move it away).

				return new Vector2(self.anchor.entity.Top.X, self.anchor.entity.VisualPosition.Y) + new Vector2((float)(self.anchor.entity.direction * self.anchor.entity.width), distance) - Main.screenPosition;
				
				// Original vanilla code:
				// effect = ((anchor.entity.direction != -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
				// return new Vector2(anchor.entity.Top.X, anchor.entity.VisualPosition.Y) + new Vector2((float)(-anchor.entity.direction * anchor.entity.width) * 0.75f, 2f) - Main.screenPosition;
			}
			// Do the original code if not our Town NPC.
			return orig(self, out effect);
		}
	}
}