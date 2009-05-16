using System;
using System.Globalization;

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
    /// Shows the distance of each cast.
    /// </summary>
    public class DistanceView
    {
        /// <summary>
        /// Creates a new distance marker.
        /// </summary>
        /// <param name="scene">The scene from which to get the cliff position.</param>
        /// <param name="state">The state from which to get the lure position.</param>
        /// <param name="context">The context from which to load the content.</param>
        public DistanceView(Scene scene, FishingState state)
        {
            state.ActionChanged += (stateObj, e) =>
            {
                if (e.Action == FishingAction.Reel)
                {
                    ShowMarker(state.LurePosition, (state.LurePosition.X - scene.ShoreX) * PixelsToMetres);
                }
            };
        }

        /// <summary>
        /// Loads the marker sprite.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _marker = content.Load<SpriteDescriptorTemplate>("Sprites/DistanceMarker").Create();
            _marker.GetSprite<TextSprite>("BestText").Text = Resources.BestDistance;
        }

        /// <summary>
        /// Updates this marker for the current frame.
        /// </summary>
        public void Update(float time)
        {
            if (_markerAnimation != null)
            {
                if (!_markerAnimation.Update(time))
                {
                    _markerAnimation = null;
                }
            }
        }

        /// <summary>
        /// Draws this distance marker.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _marker.Sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Shows the marker at the given position.
        /// </summary>
        /// <param name="position">The position of the marker.</param>
        /// <param name="distance">The distance to show on the marker.</param>
        private void ShowMarker(Vector2 position, float distance)
        {
            if (distance < 0f)
            {
                return; // just ignore casts behind the player
            }

            _marker.Sprite.Position = position;

            string distanceString = distance.ToString("F1", CultureInfo.CurrentCulture);
            string markerString = String.Format(Resources.Distance, distanceString);

            TextSprite text = _marker.GetSprite<TextSprite>("DistanceText");
            text.Text = markerString;
            text.Position = new Vector2((_marker.Sprite.Size.X - text.Size.X) / 2 - 1, text.Position.Y);

            if (distance > _bestDistance)
            {
                _markerAnimation = _marker.GetAnimation(_bestDistance > 0 ? "ShowBest" : "Show");
                _bestDistance = distance;
            }
            else
            {
                _markerAnimation = _marker.GetAnimation("Show");
            }
            _markerAnimation.Start();
        }

        private SpriteDescriptor _marker;
        private IAnimation _markerAnimation;

        private float _bestDistance = 0f;

        private const float PixelsToMetres = 1 / 50f;
    }
}
