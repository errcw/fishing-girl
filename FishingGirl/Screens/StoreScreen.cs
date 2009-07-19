﻿using System;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Gameplay;
using FishingGirl.Properties;

namespace FishingGirl.Screens
{
    /// <summary>
    /// Displays the end-of-game narrative.
    /// </summary>
    public class StoreScreen : MenuScreen
    {
        /// <summary>
        /// The store this screen is displaying.
        /// </summary>
        public Store Store { get; set; }

        public StoreScreen(FishingGameContext context) : base(context)
        {
            ShowBeneath = true;
            TransitionOnTime = 0.25f;
            TransitionOffTime = 0.25f;
            IsRoot = true;
            ShowBackOnRoot = true;
            NumVisibleEntries = 3;
            Spacing = 8;

            context.Trial.TrialModeEnded += (o, a) => BuildScreen();
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
            
            _screenDescriptor.GetSprite<TextSprite>("TextSelect").Text = Resources.StoreBuy;
            _screenDescriptor.GetSprite<TextSprite>("TextBack").Text = Resources.StoreClose;

            _entryTemplate = content.Load<SpriteDescriptorTemplate>("Sprites/StoreItem");

            _soundNoMoney = content.Load<SoundEffect>("Sounds/ShopNoMoney");
            _soundPurchase = content.Load<SoundEffect>("Sounds/ShopPurchase");

            _nagDescriptor = content.Load<SpriteDescriptorTemplate>("Sprites/NagScreen").Create();
            _nagDescriptor.GetSprite<TextSprite>("Bubble").Text = Resources.NagBubbleStore;
            _nagDescriptor.GetSprite<TextSprite>("Time").Text = Resources.NagTime;
            _nagDescriptor.GetSprite<TextSprite>("Badges").Text = Resources.NagBadges;
            _nagDescriptor.GetSprite<TextSprite>("Lures").Text = Resources.NagLures;
            _nagDescriptor.GetSprite<TextSprite>("Fish").Text = Resources.NagFish;
        }

        /// <summary>
        /// Loads all the items in this store.
        /// </summary>
        protected override void Show(bool pushed)
        {
            BuildScreen();
            base.Show(pushed);
        }

        /// <summary>
        /// Creates the entries for each store item.
        /// </summary>
        private void BuildScreen()
        {
            ClearEntries();
            for (int i = 0; i < Store.Items.Count; i++)
            {
                StoreItem item = Store.Items[i];

                bool canPurchase = Store.CanPurchase(item) && !Guide.IsTrialMode;

                StoreItemEntry entry = new StoreItemEntry(item, _entryTemplate.Create(), canPurchase);
                entry.Selected += delegate(object entryObj, EventArgs args)
                {
                    if (canPurchase)
                    {
                        _soundPurchase.Play();
                        Store.Purchase(item);
                        Stack.Pop();
                    }
                    else
                    {
                        _soundNoMoney.Play();
                    }
                };

                AddEntry(entry);
            }
            if (Guide.IsTrialMode)
            {
                AddDecoration(_nagDescriptor.Sprite);
            }
        }

        /// <summary>
        /// Displays a store item.
        /// </summary>
        private class StoreItemEntry : MenuEntry
        {
            public StoreItemEntry(StoreItem item, SpriteDescriptor sprite, bool purchaseEnabled) : base(sprite.Sprite)
            {
                _item = item;
                _descriptor = sprite;
                _descriptor.GetSprite<CompositeSprite>("Image").Add(item.Image);
                _descriptor.GetSprite<TextSprite>("Name").Text = item.Name;
                _descriptor.GetSprite<TextSprite>("Description").Text = item.Description;
                _descriptor.GetSprite<TextSprite>("Money").Text = item.Cost.ToString(CultureInfo.InvariantCulture);
                if (!purchaseEnabled)
                {
                    _descriptor.Sprite.Color = new Color(_descriptor.Sprite.Color, 0.6f);
                }
            }

            public override void OnFocusChanged(bool focused)
            {
                _descriptor.GetSprite("Background").Color = focused ? Color.White : Color.TransparentWhite;
            }

            private StoreItem _item;
            private SpriteDescriptor _descriptor;
        }

        private SpriteDescriptorTemplate _entryTemplate;
        private SpriteDescriptor _nagDescriptor;
        private SoundEffect _soundPurchase, _soundNoMoney;
    }
}