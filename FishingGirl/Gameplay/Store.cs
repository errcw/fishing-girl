using System;
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
            Position = InitialPosition;
        }

        /// <summary>
        /// Creates the items in this store.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            Func<string, Sprite> getStoreSprite = delegate(string spriteName)
            {
                return content.Load<ImageSpriteTemplate>(spriteName).Create();
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
            _smallLure = new StoreItem(
                (() => _fishing.Lures.Add(Lures.Small)),
                5,
                Resources.StoreLureSmall,
                Resources.StoreLureSmallDescription,
                getStoreSprite("Store" + Lures.Small.SpriteName));
            _smallUpgradedLure = new StoreItem(
                (() => _fishing.Lures.Add(Lures.SmallUpgraded)),
                75,
                Resources.StoreLureSmallUpgraded,
                Resources.StoreLureSmallUpgradedDescription,
                getStoreSprite("Store" + Lures.SmallUpgraded.SpriteName));
            _mediumLure = new StoreItem(
                (() => _fishing.Lures.Add(Lures.Medium)),
                75,
                Resources.StoreLureMedium,
                Resources.StoreLureMediumDescription,
                getStoreSprite("Store" + Lures.Medium.SpriteName));
            _largeLure = new StoreItem(
                (() => _fishing.Lures.Add(Lures.Large)),
                100,
                Resources.StoreLureLarge,
                Resources.StoreLureLargeDescription,
                getStoreSprite("Store" + Lures.Large.SpriteName));
            _largeUpgradedLure = new StoreItem(
                (() => _fishing.Lures.Add(Lures.LargeUpgraded)),
                150,
                Resources.StoreLureLargeUpgraded,
                Resources.StoreLureLargeUpgradedDescription,
                getStoreSprite("Store" + Lures.LargeUpgraded.SpriteName));

            FillStore();
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

            FillStore();
            if (Items.Count == 0)
            {
                HideStore();
            }
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
            float range = (MaxPosition - MinPosition);
            Position = (float)(_random.NextDouble() * range + MinPosition);
        }

        /// <summary>
        /// Permanently hides the store.
        /// </summary>
        private void HideStore()
        {
            Position = -1000;
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
            if (!_fishing.Lures.Contains(Lures.Small) && !_fishing.Lures.Contains(Lures.SmallUpgraded))
            {
                Items.Add(_smallLure);
            }
            if (!_fishing.Lures.Contains(Lures.SmallUpgraded))
            {
                Items.Add(_smallUpgradedLure);
            }
            if (!_fishing.Lures.Contains(Lures.Medium))
            {
                Items.Add(_mediumLure);
            }
            if (!_fishing.Lures.Contains(Lures.Large) && !_fishing.Lures.Contains(Lures.LargeUpgraded))
            {
                Items.Add(_largeLure);
            }
            if (!_fishing.Lures.Contains(Lures.LargeUpgraded))
            {
                Items.Add(_largeUpgradedLure);
            }
        }

        /// <summary>
        /// Invokes the hit event.
        /// </summary>
        private void OnHit()
        {
            if (Hit != null)
            {
                Hit(this, EventArgs.Empty);
            }
        }

        private Money _money;
        private FishingState _fishing;

        private StoreItem _silverRod, _goldRod, _legendaryRod;
        private StoreItem _smallLure, _mediumLure, _largeLure, _smallUpgradedLure, _largeUpgradedLure;

        private Random _random = new Random();

        private const float HitThreshold = 50f;

        private const float InitialPosition = 1300f;
        private const float MinPosition = 1100f;
        private const float MaxPosition = 1570f;
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
