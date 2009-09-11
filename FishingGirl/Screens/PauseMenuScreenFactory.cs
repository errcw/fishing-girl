using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;
using Library.Extensions;

using FishingGirl.Gameplay;
using FishingGirl.Interface;
using FishingGirl.Properties;

namespace FishingGirl.Screens
{
    /// <summary>
    /// Creates the pause menu structure.
    /// </summary>
    public class PauseMenuScreenFactory
    {
        public MenuScreen Create(Options options, Badges badges, FishingGameContext context, ContentManager content)
        {
            _menuFont = content.Load<SpriteFont>("Fonts/Text"); // use a common font
            _menuSound = content.Load<SoundEffect>("Sounds/MenuSelect"); // use a common selected sound

            MenuScreen screen = new MenuScreen(context);
            screen.IsRoot = true;
            screen.LoadContent(content);

            MenuScreen badgesScreen = BuildBadges(badges, context, content);
            MenuScreen optionsScreen = BuildOptions(options, context, content);
            MenuScreen confirmScreen = BuildExitConfirm(context, content);
            MenuScreen upsellScreen = BuildExitUpsell(context, content);

            screen.AddEntry(BuildTextEntry(Resources.MenuResume, (s, a) => screen.Stack.Pop()));
            if (Guide.IsTrialMode)
            {
                MenuEntry purchase = BuildTextEntry(Resources.MenuPurchase, (s, a) => ShowPurchaseScreen(context));
                screen.AddEntry(purchase);
                context.Trial.TrialModeEnded += delegate(object s, EventArgs a) { screen.RemoveEntry(purchase); screen.LayoutEntries(); };
            }
            screen.AddEntry(BuildTextEntry(Resources.MenuBadges, (s, a) => screen.Stack.Push(badgesScreen)));
            screen.AddEntry(BuildTextEntry(Resources.MenuHelpOptions, (s, a) => screen.Stack.Push(optionsScreen)));
            screen.AddEntry(BuildTextEntry(Resources.MenuExit, (s, a) => screen.Stack.Push(Guide.IsTrialMode ? upsellScreen : confirmScreen)));
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

        private MenuScreen BuildExitUpsell(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);

            MenuEntry purchase = BuildTextEntry(Resources.MenuPurchase, (s, a) => ShowPurchaseScreen(context));
            screen.AddEntry(purchase);
            screen.AddEntry(BuildTextEntry(Resources.MenuExitNo, (s, a) => screen.Stack.Pop()));
            screen.AddEntry(BuildTextEntry(Resources.MenuExitYes, (s, a) => screen.Stack.PopAll()));
            screen.LayoutEntries();

            context.Trial.TrialModeEnded += delegate(object s, EventArgs a) { if (screen.State == ScreenState.Active) { screen.Stack.Pop(); } };

            UpsellOverlay upsell = new UpsellOverlay(Resources.UpsellMessageExit, content);
            screen.AddDecoration(upsell.Sprite);

            return screen;
        }

        private MenuScreen BuildBadges(Badges badges, FishingGameContext context, ContentManager content)
        {
            BadgesScreen badgesScreen = new BadgesScreen(badges, context);
            badgesScreen.LoadContent(content);
            return badgesScreen;
        }

        private MenuScreen BuildOptions(Options options, FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);

            MenuScreen controlsScreen = BuildControls(context, content);
            MenuScreen creditsScreen = BuildCredits(context, content);
            MenuScreen settingsScreen = BuildSettings(options, context, content);

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

        private MenuScreen BuildSettings(Options options, FishingGameContext context, ContentManager content)
        {
            Func<bool,string> getStateString = (b) => b ? Resources.MenuOn : Resources.MenuOff;

            var effectsEntry = BuildOptionEntry(Resources.MenuSoundEffects, getStateString(options.SoundEffectsToggle),
                (s, a) =>
                {
                    options.SoundEffectsToggle = !options.SoundEffectsToggle;
                    ((TextMenuEntry)s).TextSprite.Text = getStateString(options.SoundEffectsToggle);
                });
            var musicEntry = BuildOptionEntry(Resources.MenuMusic, getStateString(options.MusicToggle),
                (s, a) =>
                {
                    options.MusicToggle = !options.MusicToggle;
                    ((TextMenuEntry)s).TextSprite.Text = getStateString(options.MusicToggle);
                });
            var vibrationEntry = BuildOptionEntry(Resources.MenuVibration, getStateString(options.VibrationToggle),
                (s, a) =>
                {
                    options.VibrationToggle = !options.VibrationToggle;
                    ((TextMenuEntry)s).TextSprite.Text = getStateString(options.VibrationToggle);
                });
            var timerEntry = BuildOptionEntry(Resources.MenuTimer, getStateString(options.TimerToggle),
                (s, a) =>
                {
                    options.TimerToggle = !options.TimerToggle;
                    ((TextMenuEntry)s).TextSprite.Text = getStateString(options.TimerToggle);
                });

            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);
            screen.AddEntry(effectsEntry);
            screen.AddEntry(musicEntry);
            screen.AddEntry(vibrationEntry);
            screen.AddEntry(timerEntry);
            screen.LayoutEntries();

            // update the text when the screen is displayed because the storage device might have changed
            screen.StateChanged += (s, a) =>
            {
                Screen scr = (Screen)s;
                if (scr.State == ScreenState.TransitionOn || scr.State == ScreenState.Active)
                {
                    effectsEntry.TextSprite.Text = getStateString(options.SoundEffectsToggle);
                    musicEntry.TextSprite.Text = getStateString(options.MusicToggle);
                    vibrationEntry.TextSprite.Text = getStateString(options.VibrationToggle);
                    timerEntry.TextSprite.Text = getStateString(options.TimerToggle);
                }
            };

            return screen;
        }

        /// <summary>
        /// Returns a labelled text menu entry.
        /// </summary>
        private TextMenuEntry BuildOptionEntry(string label, string text, EventHandler<EventArgs> e)
        {
            TextMenuEntry entry = new TextMenuEntry(
                new TextSprite(_menuFont, label),
                new TextSprite(_menuFont, text));
            entry.Selected += e;
            entry.Selected += (s, a) => _menuSound.Play();
            entry.SelectText = Resources.MenuToggle;
            return entry;
        }

        /// <summary>
        /// Returns a text menu entry.
        /// </summary>
        private TextMenuEntry BuildTextEntry(string text, EventHandler<EventArgs> e)
        {
            TextMenuEntry entry = new TextMenuEntry(new TextSprite(_menuFont, text));
            entry.Selected += e;
            entry.Selected += (s, a) => _menuSound.Play();
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
            context.Input.Controller.Value.PurchaseContent();
        }

        /// <summary>
        /// A plain text menu entry. 
        /// </summary>
        class TextMenuEntry : MenuEntry
        {
            public TextSprite LabelSprite { get; protected set; }
            public TextSprite TextSprite { get; protected set; }

            /// <summary>
            /// Creates a new text menu entry.
            /// </summary>
            /// <param name="text">The text sprite to show.</param>
            public TextMenuEntry(TextSprite text) : base(text)
            {
                TextSprite = text;
            }

            /// <summary>
            /// Creates a new text menu entry.
            /// </summary>
            /// <param name="label">The unhighlighted label to show.</param>
            /// <param name="text">The text to show.</param>
            public TextMenuEntry(TextSprite label, TextSprite text) : base(null)
            {
                LabelSprite = label;
                TextSprite = text;

                CompositeSprite composite = new CompositeSprite(LabelSprite, TextSprite);
                LabelSprite.Position = Vector2.Zero;
                TextSprite.Position = new Vector2(LabelSprite.Size.X + LabelSprite.Font.MeasureString(" ").X, 0);
                Sprite = composite;
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
                TextSprite.OutlineColor = new Color(OutlineColor, a);
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
                if (focused)
                {
                    TextSprite.OutlineColor = OutlineColor;
                    TextSprite.OutlineWidth = 2;
                    _fadeIn = false;
                    _fadeElapsed = 0;
                }
                else
                {
                    TextSprite.OutlineWidth = 0;
                }
            }

            private bool _fadeIn;
            private float _fadeElapsed;

            private readonly Color OutlineColor = new Color(207, 115, 115);
            private const float FadeDuration = 0.6f;
        }

        private SpriteFont _menuFont;
        private SoundEffect _menuSound;
    }
}
