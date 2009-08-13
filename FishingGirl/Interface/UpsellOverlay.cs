using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Properties;

namespace FishingGirl.Interface
{
    /// <summary>
    /// A menu overlay to sell the game.
    /// </summary>
    public class UpsellOverlay
    {
        /// <summary>
        /// The overlay sprite.
        /// </summary>
        public Sprite Sprite { get; private set; }

        public UpsellOverlay(string message, ContentManager content)
        {
            SpriteDescriptor nagDesc = content.Load<SpriteDescriptorTemplate>("Sprites/NagScreen").Create();
            nagDesc.GetSprite<TextSprite>("Bubble").Text = message;
            nagDesc.GetSprite<TextSprite>("Time").Text = Resources.UpsellTime;
            nagDesc.GetSprite<TextSprite>("Badges").Text = Resources.UpsellBadges;
            nagDesc.GetSprite<TextSprite>("Lures").Text = Resources.UpsellLures;
            nagDesc.GetSprite<TextSprite>("Fish").Text = Resources.UpsellFish;
            Sprite = nagDesc.Sprite;
        }
    }
}
