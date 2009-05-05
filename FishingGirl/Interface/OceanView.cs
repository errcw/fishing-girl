using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Library.Sprite;
using Library.Extensions;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Draws oceans.
    /// </summary>
    public class OceanView
    {
        /// <summary>
        /// Creates a new ocean view.
        /// </summary>
        /// <param name="ocean">The ocean to draw.</param>
        public OceanView(Ocean ocean)
        {
            _ocean = ocean;
            _ocean.FishAdded += (o, e) => _fishes.Add(e.Fish, new FishView(e.Fish));
            _ocean.FishRemoved += (o, e) => _fishes.Remove(e.Fish);

            _fishes = new Dictionary<Fish, FishView>();
            _ocean.Fish.ForEach(fish => _fishes.Add(fish, new FishView(fish)));
        }

        /// <summary>
        /// Updates the view of the ocean.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            _fishes.Values.ForEach(view => view.Update(time));
        }

        /// <summary>
        /// Draws the ocean.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _fishes.Values.ForEach(view => view.Draw(spriteBatch));
        }

        private Ocean _ocean;
        private IDictionary<Fish, FishView> _fishes;
    }
}
