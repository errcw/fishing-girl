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
    public class PauseMenuScreenFactory
    {
        public MenuScreen Create(FishingGameContext context, ContentManager content)
        {
            // use a common font
            _menuFont = content.Load<SpriteFont>("Fonts/Text");

            MenuScreen screen = new MenuScreen(context);
            screen.IsRoot = true;
            screen.LoadContent(content);

            MenuScreen confirmScreen = BuildExitConfirm(context, content);
            MenuScreen nagScreen = BuildNag(context, content);

            screen.AddEntry(BuildTextEntry(Resources.MenuResume, (s, a) => screen.Stack.Pop()));
            screen.AddEntry(BuildTextEntry(Resources.MenuBadges, (s, a) => screen.Stack.Pop()));
            if (Guide.IsTrialMode)
            {
                MenuEntry purchase = BuildTextEntry(Resources.MenuPurchase, (s, a) => ShowPurchaseScreen(context));
                screen.AddEntry(purchase);
                context.Trial.TrialModeEnded += delegate(object s, EventArgs a) { screen.RemoveEntry(purchase); screen.LayoutEntries(); };
            }
            screen.AddEntry(BuildTextEntry(Resources.MenuExit, (s, a) => screen.Stack.Push(Guide.IsTrialMode ? nagScreen : confirmScreen)));
            screen.LayoutEntries();

            return screen;
        }

        /// <summary>
        /// Builds the exit confirmation screen.
        /// </summary>
        private MenuScreen BuildExitConfirm(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);
            screen.AddEntry(BuildTextEntry(Resources.MenuExitNo, (s, a) => screen.Stack.Pop()));
            screen.AddEntry(BuildTextEntry(Resources.MenuExitYes, (s, a) => screen.Stack.PopAll()));
            screen.LayoutEntries();
            return screen;
        }

        /// <summary>
        /// Builds the buy-me nag screen.
        /// </summary>
        private MenuScreen BuildNag(FishingGameContext context, ContentManager content)
        {
            MenuScreen screen = new MenuScreen(context);
            screen.LoadContent(content);

            MenuEntry purchase = BuildTextEntry(Resources.MenuPurchase, (s, a) => ShowPurchaseScreen(context));
            screen.AddEntry(purchase);
            screen.AddEntry(BuildTextEntry(Resources.MenuExitNo, (s, a) => screen.Stack.Pop()));
            screen.AddEntry(BuildTextEntry(Resources.MenuExitYes, (s, a) => screen.Stack.PopAll()));
            screen.LayoutEntries();

            context.Trial.TrialModeEnded += delegate(object s, EventArgs a) { screen.Stack.Pop(); };

            SpriteDescriptor nagDesc = content.Load<SpriteDescriptorTemplate>(@"Sprites\NagScreen").Create(content);
            nagDesc.GetSprite<TextSprite>("Bubble").Text = Resources.NagBubble;
            nagDesc.GetSprite<TextSprite>("Time").Text = Resources.NagTime;
            nagDesc.GetSprite<TextSprite>("Badges").Text = Resources.NagBadges;
            nagDesc.GetSprite<TextSprite>("Lures").Text = Resources.NagLures;
            nagDesc.GetSprite<TextSprite>("Fish").Text = Resources.NagFish;
            screen.AddDecoration(nagDesc.Sprite);

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

        private SpriteFont _menuFont;
    }
}
