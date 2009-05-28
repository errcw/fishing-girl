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
        /// Purchases an item.
        /// </summary>
        /// <param name="fishing">The fishing state to apply the purchase to.</param>
        public delegate void PurchaseItem(FishingState fishing);

        public Store(Money money, FishingState fishing)
        {
            _money = money;
            _fishing = fishing;
            _fishing.ActionChanged += OnActionChanged;
        }

        /// <summary>
        /// Updates the state of this store.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
        }

        private void OnActionChanged(object stateObj, FishingActionEventArgs e)
        {
            if (e.Action == FishingAction.Reel)
            {
                // check position for hit
                float distance = Math.Abs(_position - _fishing.LurePosition.X);
                if (distance <= HitThreshold)
                {
                    //TODO purchased item
                }
            }
        }

        /// <summary>
        /// An item in the store.
        /// </summary>
        private class StoreItem
        {
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

        private float _position;

        private Money _money;
        private FishingState _fishing;

        private const float HitThreshold = 30f;
    }
}
