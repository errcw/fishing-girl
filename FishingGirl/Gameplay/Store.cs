using System;

using Microsoft.Xna.Framework;

using Library.Sprite;

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
        public StoreItem Items { get; private set; }

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
            MoveStore();
        }

        /// <summary>
        /// Updates the state of this store.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
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
