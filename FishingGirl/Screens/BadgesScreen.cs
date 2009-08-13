using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

using Library.Extensions;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Gameplay;
using FishingGirl.Interface;
using FishingGirl.Properties;

namespace FishingGirl.Screens
{
    /// <summary>
    /// Displays the badges, earned and unearned.
    /// </summary>
    public class BadgesScreen : MenuScreen
    {
        public BadgesScreen(Badges badges, FishingGameContext context) : base(context)
        {
            _badges = badges;
            NumVisibleEntries = 3;
            Spacing = 8;
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            _entryTemplate = content.Load<SpriteDescriptorTemplate>("Sprites/BadgeMenuItem");
            _upsellSprite = new UpsellOverlay(Resources.UpsellMessageBadges, content).Sprite;
        }

        protected override void Show(bool pushed)
        {
            BuildScreen();
            base.Show(pushed);
        }

        /// <summary>
        /// Creates menu entries that reflect which badges are earned.
        /// </summary>
        private void BuildScreen()
        {
            ClearEntries();
            foreach (Badge badge in _badges.BadgeSet)
            {
                AddEntry(new BadgeMenuEntry(badge, _entryTemplate.Create()));
            }
            if (Guide.IsTrialMode)
            {
                AddDecoration(_upsellSprite);
            }
        }

        private Badges _badges;

        private SpriteDescriptorTemplate _entryTemplate;
        private Sprite _upsellSprite;
    }

    /// <summary>
    /// Displays a single badge.
    /// </summary>
    public class BadgeMenuEntry : MenuEntry
    {
        public BadgeMenuEntry(Badge badge, SpriteDescriptor descriptor) : base(descriptor.Sprite)
        {
            _descriptor = descriptor;
            _descriptor.GetSprite<TextSprite>("Name").Text = badge.Name;
            _descriptor.GetSprite<TextSprite>("Description").Text = badge.Description;
            if (badge.IsEarned)
            {
                _descriptor.GetSprite("Earned").Color = Color.White;
                _descriptor.GetSprite("NotEarned").Color = Color.TransparentWhite;
            }
            else 
            {
                _descriptor.GetSprite("Earned").Color = Color.TransparentWhite;
                _descriptor.GetSprite("NotEarned").Color = Color.White;
            }

            IsSelectable = false;
        }

        public override void OnFocusChanged(bool focused)
        {
            _descriptor.GetSprite("Background").Color = focused ? Color.White : Color.TransparentWhite;
        }

        private SpriteDescriptor _descriptor;
    }
}
