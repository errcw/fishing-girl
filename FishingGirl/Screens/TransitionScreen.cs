using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Sprite;
using Library.Sprite.Pipeline;
using Library.Screen;

namespace FishingGirl.Screens
{
    /// <summary>
    /// Fades in to a solid colour then back out, then removes itself.
    /// </summary>
    public class TransitionScreen : Screen
    {
        /// <summary>
        /// The colour to fade to.
        /// </summary>
        public Color FadeColor { get; set; }

        public TransitionScreen()
        {
            FadeColor = Color.White;
            TransitionOnTime = 1.5f;
            TransitionOffTime = 1.5f;
            ShowBeneath = true;
        }

        public void LoadContent(ContentManager content)
        {
            _background = content.Load<ImageSpriteTemplate>("Colourable").Create();
            _background.Scale = new Vector2(1280, 720);
            _background.Color = Color.TransparentWhite;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            _background.Draw(spriteBatch);
            spriteBatch.End();
        }

        protected override void UpdateTransitionOn(float time, float progress, bool pushed)
        {
            _background.Color = new Color(FadeColor, progress); // fade in
        }

        protected override void UpdateActive(float time)
        {
            Stack.Pop(); // pop ourselves off right away
        }

        protected override void UpdateTransitionOff(float time, float progress, bool popped)
        {
            _background.Color = new Color(FadeColor, 1 - progress); // fade out
        }

        private Sprite _background;
    }
}
