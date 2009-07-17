using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Extensions;
using Library.Sprite;
using Library.Sprite.Pipeline;

namespace FishingGirl.Screens
{
    public class BadgesScreen : MenuScreen
    {
        public BadgesScreen(FishingGameContext context) : base(context)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            SpriteFont font = content.Load<SpriteFont>(@"Fonts/Text");
            AddEntry(new BadgeMenuEntry(new TextSprite(font, "A")));
            AddEntry(new BadgeMenuEntry(new TextSprite(font, "B")));
            AddEntry(new BadgeMenuEntry(new TextSprite(font, "C")));
            AddEntry(new BadgeMenuEntry(new TextSprite(font, "D")));
            AddEntry(new BadgeMenuEntry(new TextSprite(font, "E")));
            AddEntry(new BadgeMenuEntry(new TextSprite(font, "F")));
            AddEntry(new BadgeMenuEntry(new TextSprite(font, "G")));
            AddEntry(new BadgeMenuEntry(new TextSprite(font, "H")));
            LayoutEntries();
        }
    }

    public class BadgeMenuEntry : MenuEntry
    {
        public BadgeMenuEntry(Sprite s) : base(s)
        {
        }

        public override void OnFocusChanged(bool focused)
        {
            ((TextSprite)Sprite).Color = focused ? Color.Red : Color.White;
        }
    }
}
