using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;
using Library.Extensions;

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

            SpriteFont font = content.Load<SpriteFont>("Fonts/Text");
            MenuScreen optionsScreen = BuildOptions(content);
            MenuScreen confirmScreen = BuildExitConfirm(content);
            
            AddEntry(BuildTextEntry(font, Resources.MenuResume, (s, a) => Stack.Pop()));
            if (Guide.IsTrialMode)
            {
                AddEntry(_purchaseEntry = BuildTextEntry(font, Resources.MenuPurchase, (s, a) => ShowPurchaseScreen()));
            }
            AddEntry(BuildTextEntry(font, Resources.MenuHelpOptions, (s, a) => Stack.Push(optionsScreen)));
            AddEntry(BuildTextEntry(font, Resources.MenuExit, (s, a) => Stack.Push(confirmScreen)));
            LayoutEntries();
        }

        /// <summary>
        /// Poll for the game switching out of trial mode to remove the purchase game menu entry.
        /// </summary>
        protected override void UpdateActive(float time)
        {
            base.UpdateActive(time);
            if (_purchaseEntry != null && !Guide.IsTrialMode)
            {
                RemoveEntry(_purchaseEntry);
                LayoutEntries();
                _purchaseEntry = null;
            }
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
            SpriteDescriptor controlsDesc = content.Load<SpriteDescriptorTemplate>(@"Sprites\Controls").Create(content);
            controlsDesc.GetSprite<TextSprite>("AText").Text = Resources.ControlsAButton;
            controlsDesc.GetSprite<TextSprite>("BText").Text = Resources.ControlsBButton;
            return BuildMenuScreen(content,
                BuildImageEntry(controlsDesc.Sprite));
        }

        /// <summary>
        /// Builds the how to play menu.
        /// </summary>
        private MenuScreen BuildSettings(ContentManager content)
        {
            SpriteFont font = content.Load<SpriteFont>("Fonts/Text");
            return BuildMenuScreen(content,
                BuildVolumeControl(content),
                BuildTextEntry(font, Resources.MenuChangeStorage, (s, a) => _context.Storage.PromptForDevice()));
        }

        /// <summary>
        /// Builds the credits menu.
        /// </summary>
        private MenuScreen BuildCredits(ContentManager content)
        {
            SpriteFont font = content.Load<SpriteFont>("Fonts/Text");
            return BuildMenuScreen(content,
                BuildImageEntry(content.Load<ImageSpriteTemplate>("FishEatFish").Create()),
                BuildTextEntry(font, Resources.CreditsDesignArt),
                BuildTextEntry(font, Resources.CreditsDevelopment));
        }

        /// <summary>
        /// Builds the exit confirmation screen.
        /// </summary>
        private MenuScreen BuildExitConfirm(ContentManager content)
        {
            SpriteFont font = content.Load<SpriteFont>("Fonts/Text");
            return BuildMenuScreen(content,
                BuildTextEntry(font, Resources.MenuExitConfirm),
                BuildTextEntry(font, " "), // blank line
                BuildTextEntry(font, Resources.MenuExitNo, (s, a) => Stack.Pop()),
                BuildTextEntry(font, Resources.MenuExitYes, (s, a) => Stack.PopAll()));
        }

        /// <summary>
        /// Builds the volume control menu entry.
        /// </summary>
        private MenuEntry BuildVolumeControl(ContentManager content)
        {
            SpriteFont font = content.Load<SpriteFont>("Fonts/Text");
            return BuildTextEntry(font, Resources.MenuVolume, (s, a) => font=null);
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
        private MenuEntry BuildImageEntry(Sprite image)
        {
            MenuEntry entry = new MenuEntry(image);
            entry.IsSelectable = false;
            return entry;
        }


        /// <summary>
        /// Shows the marketplace purchase screen if possible, or a warning otherwise.
        /// </summary>
        private void ShowPurchaseScreen()
        {
            try
            {
                PlayerIndex player = _context.Input.Controller.Value;
                if (player.CanPurchaseContent())
                {
                    Guide.ShowMarketplace(player);
                }
                else
                {
                    Guide.BeginShowMessageBox(
                        player,
                        Resources.PurchaseMBTitle,
                        Resources.PurchaseMBText,
                        new string[] { Resources.PurchaseMBOK },
                        0,
                        MessageBoxIcon.Warning,
                        r => Guide.EndShowMessageBox(r),
                        new object());
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        private MenuEntry _purchaseEntry;
    }
}
