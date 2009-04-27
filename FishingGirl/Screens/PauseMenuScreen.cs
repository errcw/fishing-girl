using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Properties;

namespace FishingGirl.Screens
{
    public class PauseMenuScreen : MenuScreen
    {
        public PauseMenuScreen(FishingGameContext context) : base(context)
        {
            IsRoot = true;
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            MenuScreen optionsScreen = BuildOptions(content);

            SpriteFont font = content.Load<SpriteFont>("Fonts/Text");
            AddEntry(BuildTextEntry(font, Resources.MenuResume, (s, a) => Stack.Pop()));
            if (Guide.IsTrialMode)
            {
                AddEntry(BuildTextEntry(font, Resources.MenuPurchase, (s, a) => Guide.ShowMarketplace(PlayerIndex.One)));
            }
            AddEntry(BuildTextEntry(font, Resources.MenuHelpOptions, (s, a) => Stack.Push(optionsScreen)));
            AddEntry(BuildTextEntry(font, Resources.MenuExit, (s, a) => Stack.PopAll()));
            LayoutEntries();
        }

        /// <summary>
        /// Builds the options menu.
        /// </summary>
        private MenuScreen BuildOptions(ContentManager content)
        {
            SpriteFont font = content.Load<SpriteFont>("Fonts/Text");
            MenuScreen controlsScreen = BuildControls(content);
            MenuScreen settingsScreen = BuildSettings(content);
            MenuScreen creditsScreen = BuildCredits(content);
            return BuildMenuScreen(content,
                BuildTextEntry(font, Resources.MenuControls, (s, a) => Stack.Push(controlsScreen)),
                BuildTextEntry(font, Resources.MenuSettings, (s, a) => Stack.Push(settingsScreen)),
                BuildTextEntry(font, Resources.MenuCredits, (s, a) => Stack.Push(creditsScreen)));
        }

        /// <summary>
        /// Builds the controls menu.
        /// </summary>
        private MenuScreen BuildControls(ContentManager content)
        {
            return BuildMenuScreen(content, BuildImageEntry(content.Load<ImageSpriteTemplate>("Controls").Create()));
        }

        /// <summary>
        /// Builds the how to play menu.
        /// </summary>
        private MenuScreen BuildSettings(ContentManager content)
        {
            return BuildMenuScreen(content, BuildImageEntry(content.Load<ImageSpriteTemplate>("Controls").Create()));
        }


        /// <summary>
        /// Builds the credits menu.
        /// </summary>
        private MenuScreen BuildCredits(ContentManager content)
        {
            SpriteFont font = content.Load<SpriteFont>("Fonts/Text");
            return BuildMenuScreen(content,
                BuildTextEntry(font, Resources.CreditsTitle),
                BuildTextEntry(font, Resources.CreditsDesignArt),
                BuildTextEntry(font, Resources.CreditsDevelopment));
        }

        /// <summary>
        /// Builds and configures a menu screen.
        /// </summary>
        private MenuScreen BuildMenuScreen(ContentManager content, params MenuEntry[] entries)
        {
            MenuScreen menu = new MenuScreen(_context);
            menu.LoadContent(content);
            foreach (MenuEntry entry in entries)
            {
                menu.AddEntry(entry);
            }
            menu.LayoutEntries();
            return menu;
        }

        /// <summary>
        /// Returns a text menu entry.
        /// </summary>
        private MenuEntry BuildTextEntry(SpriteFont font, string text, EventHandler<EventArgs> e)
        {
            MenuEntry entry = new TextMenuEntry(new TextSprite(font, text));
            entry.Selected += e;
            return entry;
        }

        /// <summary>
        /// Returns a text menu entry.
        /// </summary>
        private MenuEntry BuildTextEntry(SpriteFont font, string text)
        {
            MenuEntry entry = new TextMenuEntry(new TextSprite(font, text));
            entry.IsSelectable = false;
            return entry;
        }

        /// <summary>
        /// Returns an image menu entry.
        /// </summary>
        private MenuEntry BuildImageEntry(ImageSprite image)
        {
            MenuEntry entry = new MenuEntry(image);
            entry.IsSelectable = false;
            return entry;
        }
    }
}
