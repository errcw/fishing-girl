using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    /// <summary>
    /// Creates the pause menu structure.
    /// </summary>
    public class PauseMenuScreenFactory
    {
        public MenuScreen Create(FishingGameContext context, ContentManager content)
        {
            _menuFont = content.Load<SpriteFont>("Fonts/Text"); // use a common font

            MenuScreen screen = new MenuScreen(context);
            screen.IsRoot = true;
            screen.LoadContent(content);

            MenuScreen badgesScreen = BuildBadges(context, content);
            MenuScreen optionsScreen = BuildOptions(context, content);
            MenuScreen confirmScreen = BuildExitConfirm(context, content);
            MenuScreen nagScreen = BuildNag(context, content);

            screen.AddEntry(BuildTextEntry(Resources.MenuResume, (s, a) => screen.Stack.Pop()));
            if (Guide.IsTrialMode)
            {
                MenuEntry purchase = BuildTextEntry(Resources.MenuPurchase, (s, a) => ShowPurchaseScreen(context));
                screen.AddEntry(purchase);
                context.Trial.TrialModeEnded += delegate(object s, EventArgs a) { screen.RemoveEntry(purchase); screen.LayoutEntries(); };
            }
            screen.AddEntry(BuildTextEntry(Resources.MenuBadges, (s, a) => screen.Stack.Push(badgesScreen)));
            screen.AddEntry(BuildTextEntry(Resources.MenuHelpOptions, (s, a) => screen.Stack.Push(optionsScreen)));
            screen.AddEntry(BuildTextEntry(Resources.MenuExit, (s, a) => screen.Stack.Push(Guide.IsTrialMode ? nagScreen : confirmScreen)));
            screen.LayoutEntries();

            return screen;
        }

        private MenuScreen BuildExitConfirm(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);
            screen.AddEntry(BuildTextEntry(Resources.MenuExitNo, (s, a) => screen.Stack.Pop()));
            screen.AddEntry(BuildTextEntry(Resources.MenuExitYes, (s, a) => screen.Stack.PopAll()));
            screen.LayoutEntries();
            return screen;
        }

        private MenuScreen BuildNag(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);

            MenuEntry purchase = BuildTextEntry(Resources.MenuPurchase, (s, a) => ShowPurchaseScreen(context));
            screen.AddEntry(purchase);
            screen.AddEntry(BuildTextEntry(Resources.MenuExitNo, (s, a) => screen.Stack.Pop()));
            screen.AddEntry(BuildTextEntry(Resources.MenuExitYes, (s, a) => screen.Stack.PopAll()));
            screen.LayoutEntries();

            context.Trial.TrialModeEnded += delegate(object s, EventArgs a) { if (screen.State == ScreenState.Active) { screen.Stack.Pop(); } };

            SpriteDescriptor nagDesc = content.Load<SpriteDescriptorTemplate>("Sprites/NagScreen").Create();
            nagDesc.GetSprite<TextSprite>("Bubble").Text = Resources.NagBubble;
            nagDesc.GetSprite<TextSprite>("Time").Text = Resources.NagTime;
            nagDesc.GetSprite<TextSprite>("Badges").Text = Resources.NagBadges;
            nagDesc.GetSprite<TextSprite>("Lures").Text = Resources.NagLures;
            nagDesc.GetSprite<TextSprite>("Fish").Text = Resources.NagFish;
            screen.AddDecoration(nagDesc.Sprite);

            return screen;
        }

        private MenuScreen BuildBadges(FishingGameContext context, ContentManager content)
        {
            BadgesScreen badgesScreen = new BadgesScreen(context);
            badgesScreen.LoadContent(content);
            return badgesScreen;
        }

        private MenuScreen BuildOptions(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);

            MenuScreen controlsScreen = BuildControls(context, content);
            MenuScreen creditsScreen = BuildCredits(context, content);
            MenuScreen settingsScreen = BuildSettings(context, content);

            screen.AddEntry(BuildTextEntry(Resources.MenuControls, (s, a) => screen.Stack.Push(controlsScreen)));
            screen.AddEntry(BuildTextEntry(Resources.MenuCredits, (s, a) => screen.Stack.Push(creditsScreen)));
            screen.AddEntry(BuildTextEntry(Resources.MenuSettings, (s, a) => screen.Stack.Push(settingsScreen)));

            screen.LayoutEntries();
            return screen;
        }

        private MenuScreen BuildControls(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);

            SpriteDescriptor controlsDesc = content.Load<SpriteDescriptorTemplate>("Sprites/Controls").Create();
            controlsDesc.GetSprite<TextSprite>("AText").Text = Resources.MenuControlsA;
            controlsDesc.GetSprite<TextSprite>("BText").Text = Resources.MenuControlsB;
            controlsDesc.GetSprite<TextSprite>("StartText").Text = Resources.MenuControlsStart;
            screen.AddEntry(BuildImageEntry(controlsDesc.Sprite));

            screen.LayoutEntries();
            return screen;
        }

        private MenuScreen BuildCredits(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);
            screen.AddEntry(BuildTextEntry(Resources.MenuCredits1));
            screen.AddEntry(BuildTextEntry(Resources.MenuCredits2));
            screen.AddEntry(BuildTextEntry(Resources.MenuCredits3));
            screen.AddEntry(BuildTextEntry(Resources.MenuCredits4));
            screen.AddEntry(BuildTextEntry(Resources.MenuCredits5));
            screen.LayoutEntries();
            return screen;
        }

        private MenuScreen BuildSettings(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);
            screen.AddEntry(BuildTextEntry(Resources.MenuChangeStorageDevice, (s, a) => context.Storage.PromptForDevice()));
            screen.LayoutEntries();
            return screen;
        }

        /// <summary>
        /// Returns a text menu entry.
        /// </summary>
        private MenuEntry BuildTextEntry(string text, EventHandler<EventArgs> e)
        {
            MenuEntry entry = new TextMenuEntry(new TextSprite(_menuFont, text));
            entry.Selected += e;
            return entry;
        }

        /// <summary>
        /// Returns a non-selectable textual menu entry.
        /// </summary>
        private MenuEntry BuildTextEntry(string text)
        {
            MenuEntry entry = new MenuEntry(new TextSprite(_menuFont, text));
            entry.IsSelectable = false;
            return entry;
        }

        /// <summary>
        /// Returns a non-selectable image entry.
        /// </summary>
        private MenuEntry BuildImageEntry(Sprite sprite)
        {
            MenuEntry entry = new MenuEntry(sprite);
            entry.IsSelectable = false;
            return entry;
        }

        /// <summary>
        /// Shows the marketplace purchase screen if possible, or a warning otherwise.
        /// </summary>
        private void ShowPurchaseScreen(FishingGameContext context)
        {
            try
            {
                PlayerIndex player = context.Input.Controller.Value;
                if (player.CanPurchaseContent())
                {
                    Guide.ShowMarketplace(player);
                }
                else
                {
                    Guide.BeginShowMessageBox(
                        player,
                        Resources.PurchaseFailedTitle,
                        Resources.PurchaseFailedText,
                        new string[] { Resources.PurchaseFailedButton },
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


        /// <summary>
        /// A plain text menu entry. 
        /// </summary>
        class TextMenuEntry : MenuEntry
        {
            /// <summary>
            /// Creates a new text menu entry.
            /// </summary>
            /// <param name="sprite">The text sprite to show.</param>
            public TextMenuEntry(TextSprite sprite) : base(sprite)
            {
            }

            /// <summary>
            /// Updates the pulsing outline.
            /// </summary>
            /// <param name="time">The elapsed time, in seconds, since the last update.</param>
            public override void Update(float time)
            {
                if (!IsSelectable)
                {
                    return;
                }
                _fadeElapsed += time;
                if (_fadeElapsed >= FadeDuration)
                {
                    _fadeElapsed = 0;
                    _fadeIn = !_fadeIn;
                }
                float p = _fadeElapsed / FadeDuration;
                float a = (_fadeIn) ? p : 1 - p;
                ((TextSprite)Sprite).OutlineColor = new Color(OutlineColor, a);
            }

            /// <summary>
            /// Sets the outline state.
            /// </summary>
            public override void OnFocusChanged(bool focused)
            {
                if (!IsSelectable)
                {
                    return;
                }
                TextSprite textSprite = (TextSprite)Sprite;
                if (focused)
                {
                    textSprite.OutlineColor = OutlineColor;
                    textSprite.OutlineWidth = 2;
                    _fadeIn = false;
                    _fadeElapsed = 0;
                }
                else
                {
                    textSprite.OutlineWidth = 0;
                }
            }

            private bool _fadeIn;
            private float _fadeElapsed;

            private readonly Color OutlineColor = new Color(207, 79, 79);
            private const float FadeDuration = 0.6f;
        }

        private SpriteFont _menuFont;
    }
}
