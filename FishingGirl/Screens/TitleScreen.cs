﻿using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Library.Input;
using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl;
using FishingGirl.Properties;

namespace FishingGirl.Screens
{
    /// <summary>
    /// The game title screen.
    /// </summary>
    public class TitleScreen : Screen
    {
        /// <summary>
        /// Creates a new title screen.
        /// </summary>
        public TitleScreen(FishingGameContext context)
        {
            _context = context;
            ShowBeneath = true;
            TransitionOnTime = 0f;
            TransitionOffTime = 0.5f;
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _screenDescriptor = content.Load<SpriteDescriptorTemplate>("Sprites/TitleScreen").Create(content);
            _screenDescriptor.GetSprite<TextSprite>("Text").Text = Resources.TitlePrompt;
        }

        /// <summary>
        /// Draws this screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            _screenDescriptor.Sprite.Draw(spriteBatch);
            spriteBatch.End();
        }

        /// <summary>
        /// Finds the active controller.
        /// </summary>
        protected override void UpdateActive(float time)
        {
            if (_context.Input.FindActiveController())
            {
                Stack.Pop();
            }
            _screenDescriptor.GetAnimation("HighlightText").Update(time);
        }

        /// <summary>
        /// Fades the title screen out.
        /// </summary>
        protected override void UpdateTransitionOff(float time, float progress, bool popped)
        {
            _screenDescriptor.Sprite.Color = new Color(_screenDescriptor.Sprite.Color, 1 - progress);
        }

        private SpriteDescriptor _screenDescriptor;
        private FishingGameContext _context;
    }
}