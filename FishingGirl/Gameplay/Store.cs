﻿using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Properties;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// A store for purchasing items.
    /// </summary>
    public class Store
    {
        /// <summary>
        /// Occurs when this store is hit by a lure.
        /// </summary>
        public event EventHandler<EventArgs> Hit;

        /// <summary>
        /// The position of this store; distance in pixels from the left shore.
        /// </summary>
        public float Position { get; private set; }

        /// <summary>
        /// The set of items available for purchase in the store.
        /// </summary>
        public List<StoreItem> Items { get; private set; }

        /// <summary>
        /// Creates a new store.
        /// </summary>
        /// <param name="money"></param>
        /// <param name="fishing"></param>
        public Store(Money money, FishingState fishing)
        {
            _money = money;
            _fishing = fishing;
            _fishing.ActionChanged += OnActionChanged;
            Items = new List<StoreItem>();
            MoveStore();
        }

        /// <summary>
        /// Creates the items in this store.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            // creates a centered sprite
            Func<string, Sprite> getStoreSprite = delegate(string spriteName)
            {
                Sprite sprite = content.Load<ImageSpriteTemplate>(spriteName).Create();
                return sprite;
            };

            _silverRod = new StoreItem(
                (() => _fishing.Rod = RodType.Silver),
                100,
                Resources.StoreRodSilver,
                Resources.StoreRodSilverDescription,
                getStoreSprite("StoreRodSilver"));
            _goldRod = new StoreItem(
                (() => _fishing.Rod = RodType.Gold),
                300,
                Resources.StoreRodGold,
                Resources.StoreRodGoldDescription,
                getStoreSprite("StoreRodGold"));
            _legendaryRod = new StoreItem(
                (() => _fishing.Rod = RodType.Legendary),
                500,
                Resources.StoreRodLegendary,
                Resources.StoreRodLegendaryDescription,
                getStoreSprite("StoreRodLegendary"));
            _mediumLure = new StoreItem(
                (() => _fishing.AddLure(LureType.Medium)),
                50,
                Resources.StoreLureMedium,
                Resources.StoreLureMediumDescription,
                getStoreSprite("StoreLureMedium"));
            _largeLure = new StoreItem(
                (() => _fishing.AddLure(LureType.Large)),
                150,
                Resources.StoreLureLarge,
                Resources.StoreLureLargeDescription,
                getStoreSprite("StoreLureLarge"));
            _bombLure = new StoreItem(
                (() => _fishing.AddLure(LureType.Bomb)),
                125,
                Resources.StoreLureBomb,
                Resources.StoreLureBombDescription,
                getStoreSprite("StoreLureBomb"));
        }

        /// <summary>
        /// Updates the state of this store.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
        }

        /// <summary>
        /// Returns whether the specified item can be purchased.
        /// </summary>
        public bool CanPurchase(StoreItem item)
        {
            return item.Cost <= _money.Amount;
        }

        /// <summary>
        /// Purchases the specified item from the store.
        /// </summary>
        public void Purchase(StoreItem item)
        {
            item.Purchase();
            _money.Amount -= item.Cost;
        }

        /// <summary>
        /// Checks for a hit when the lure enters the water.
        /// </summary>
        private void OnActionChanged(object stateObj, FishingActionEventArgs e)
        {
            if (e.Action == FishingAction.Reel)
            {
                float distance = Math.Abs(Position - _fishing.LurePosition.X);
                if (distance <= HitThreshold)
                {
                    OnHit();
                    MoveStore();
                }
            }
        }

        /// <summary>
        /// Moves this store to a new position.
        /// </summary>
        private void MoveStore()
        {
            float max = _fishing.MaxCastDistance * 0.9f; // avoid putting the store too far
            Position = (float)(_random.NextDouble() * max + StoreMinX);
        }

        /// <summary>
        /// Adds the available items to the store.
        /// </summary>
        private void FillStore()
        {
            Items.Clear();
            // add the next type of rod
            if (_fishing.Rod == RodType.Bronze)
            {
                Items.Add(_silverRod);
            }
            else if (_fishing.Rod == RodType.Silver)
            {
                Items.Add(_goldRod);
            }
            else if (_fishing.Rod == RodType.Gold)
            {
                Items.Add(_legendaryRod);
            }
            // add lures
            if (!_fishing.HasLure(LureType.Medium))
            {
                Items.Add(_mediumLure);
            }
            if (!_fishing.HasLure(LureType.Large))
            {
                Items.Add(_largeLure);
            }
            if (!_fishing.HasLure(LureType.Bomb))
            {
                Items.Add(_bombLure);
            }
        }

        /// <summary>
        /// Invokes the hit event.
        /// </summary>
        private void OnHit()
        {
            FillStore();
            if (Hit != null)
            {
                Hit(this, EventArgs.Empty);
            }
        }

        private Money _money;
        private FishingState _fishing;

        private StoreItem _silverRod, _goldRod, _legendaryRod;
        private StoreItem _mediumLure, _largeLure, _bombLure;

        private Random _random = new Random();

        private const float HitThreshold = 45f;
        private const float StoreMinX = 1100f;
    }

    /// <summary>
    /// An item in the store.
    /// </summary>
    public class StoreItem
    {
        /// <summary>
        /// Purchases an item.
        /// </summary>
        /// <param name="fishing">The fishing state to apply the purchase to.</param>
        public delegate void PurchaseItem();

        /// <summary>
        /// The cost of the item.
        /// </summary>
        public readonly int Cost;

        /// <summary>
        /// The short name of the item.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// A longer description of the item.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// A thumbnail image of the item.
        /// </summary>
        public readonly Sprite Image;

        /// <summary>
        /// The purchase delegate.
        /// </summary>
        public readonly PurchaseItem Purchase;

        /// <summary>
        /// Creates a new item description.
        /// </summary>
        public StoreItem(PurchaseItem purchase, int cost, string name, string description, Sprite image)
        {
            Purchase = purchase;
            Cost = cost;
            Name = name;
            Description = description;
            Image = image;
        }
    }
}