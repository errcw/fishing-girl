using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
        }

        /// <summary>
        /// Updates the store view.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
        }

        /// <summary>
        /// Draws the store.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
        }

        private Store _store;
    }
}
