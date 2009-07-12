using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Draws stores.
    /// </summary>
    public class StoreView
    {
        /// <summary>
        /// Creates a new store view.
        /// </summary>
        /// <param name="store">The store to draw.</param>
        public StoreView(Store store)
        {
            _store = store;
            _store.Hit += OnStoreHit;
        }

        /// <summary>
        /// Loads the store sprite.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _storeDescriptor = content.Load<SpriteDescriptorTemplate>("Sprites/StoreContainer").Create();
            _storeDescriptor.Sprite.Position = new Vector2(_store.Position, ContainerY);
            _storeVisAnimation = _storeDescriptor.GetAnimation("Show");
            _storeVisAnimation.Start();
        }

        /// <summary>
        /// Updates the store view.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            _storeDescriptor.GetAnimation("Bob").Update(time);
            if (_storeVisAnimation != null)
            {
                if (!_storeVisAnimation.Update(time))
                {
                    if (_storeDescriptor.Sprite.Color.A == 0)
                    {
                        // the store is hidden, move it to a new position
                        _storeDescriptor.Sprite.Position = new Vector2(_store.Position, ContainerY);
                        _storeVisAnimation = _storeDescriptor.GetAnimation("Show");
                        _storeVisAnimation.Start();
                    }
                    else
                    {
                        // store is visible
                        _storeVisAnimation = null;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the store.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _storeDescriptor.Sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Hides the store temporarily when it is hit.
        /// </summary>
        private void OnStoreHit(object store, EventArgs args)
        {
            _storeVisAnimation = _storeDescriptor.GetAnimation("Hide");
            _storeVisAnimation.Start();
        }

        private Store _store;

        private SpriteDescriptor _storeDescriptor;
        private IAnimation _storeVisAnimation;

        private const float ContainerY = 690f;
    }
}
