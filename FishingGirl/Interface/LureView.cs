using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Gameplay;
using FishingGirl.Properties;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Displays the lures owned by the player.
    /// </summary>
    public class LureView
    {
        /// <summary>
        /// Creates a new lure view.
        /// </summary>
        /// <param name="state">The fishing state with the current lure.</param>
        /// <param name="store">The store providing lures.</param>
        public LureView(FishingState state, Store store)
        {
            state.Event += OnFishingEvent;
            store.ItemPurchased += OnItemPurchased;
            _state = state;
        }

        /// <summary>
        /// Loads the lure sprite.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _sprite = content.Load<SpriteDescriptorTemplate>("Sprites/LureView").Create();
        }

        /// <summary>
        /// Updates the lure animation.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            if (_animation != null)
            {
                if (!_animation.Update(time))
                {
                    _animation = null;
                }
            }
        }

        /// <summary>
        /// Draws the money.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Notifies this view that a fishing event (maybe a lure change) occured.
        /// </summary>
        private void OnFishingEvent(object fishing, FishingEventArgs args)
        {
            if (args.Event == FishingEvent.LureChanged)
            {
                float width = _sprite.Sprite.Size.X;
                TextSprite lureName = _sprite.GetSprite<TextSprite>("LureName");
                lureName.Text = GetLureName(_state.Lure);
                lureName.Position = new Vector2((width - lureName.Size.X) / 2, lureName.Position.Y);

                _animation = _sprite.GetAnimation("ShowName");
                _animation.Start();

                Sprite selector = _sprite.GetSprite("Selector");
                selector.Position = GetLureSprite(_state.Lure).Position;
            }
        }

        /// <summary>
        /// Notifies this view that an item (maybe a lure) was purchased.
        /// </summary>
        private void OnItemPurchased(object store, ItemPurchasedEventArgs args)
        {
            StoreItem item = args.Item;
            Sprite lureSprite = null;

            if (item.Name == Resources.StoreLureSmall) lureSprite = _sprite.GetSprite("Small");
            else if (item.Name == Resources.StoreLureSmallUpgraded) lureSprite = _sprite.GetSprite("SmallUpgraded");
            else if (item.Name == Resources.StoreLureMedium) lureSprite = _sprite.GetSprite("Medium");
            else if (item.Name == Resources.StoreLureLarge) lureSprite = _sprite.GetSprite("Large");
            else if (item.Name == Resources.StoreLureLargeUpgraded) lureSprite = _sprite.GetSprite("LargeUpgraded");
            else return; // must have bought something other than a lure

            lureSprite.Position = _lastLurePosition + LureSpacing;
            lureSprite.Color = Color.White;
            _lastLurePosition = lureSprite.Position;

            if (_state.Lures.Count == 2)
            {
                // show the selector now that we have lures to select
                _animation = _sprite.GetAnimation("Show");
            }
        }

        /// <summary>
        /// Gets the sprite of the specified lure.
        /// </summary>
        private Sprite GetLureSprite(Lure lure)
        {
            if (lure == Lures.Basic) return _sprite.GetSprite("Basic");
            if (lure == Lures.Small) return _sprite.GetSprite("Small");
            if (lure == Lures.SmallUpgraded) return _sprite.GetSprite("SmallUpgraded");
            if (lure == Lures.Medium) return _sprite.GetSprite("Medium");
            if (lure == Lures.Large) return _sprite.GetSprite("Large");
            if (lure == Lures.LargeUpgraded) return _sprite.GetSprite("LargeUpgraded");
            return null;
        }

        /// <summary>
        /// Gets the localized name of the specified lure.
        /// </summary>
        private string GetLureName(Lure lure)
        {
            if (lure == Lures.Basic) return Resources.StoreLureBasic;
            if (lure == Lures.Small) return Resources.StoreLureSmall;
            if (lure == Lures.SmallUpgraded) return Resources.StoreLureSmallUpgraded;
            if (lure == Lures.Medium) return Resources.StoreLureMedium;
            if (lure == Lures.Large) return Resources.StoreLureLarge;
            if (lure == Lures.LargeUpgraded) return Resources.StoreLureLargeUpgraded;
            return String.Empty;
        }

        private FishingState _state;

        private SpriteDescriptor _sprite;
        private IAnimation _animation;

        private Vector2 _lastLurePosition = new Vector2(12, 6);
        private readonly Vector2 LureSpacing = new Vector2(40, 0);
    }
}
