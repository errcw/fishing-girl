using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FishingGirl.Gameplay;

using Library.Sprite;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Draws scenes.
    /// </summary>
    public class SceneView
    {
        /// <summary>
        /// Creates a new scene view.
        /// </summary>
        /// <param name="scene">The scene to draw.</param>
        /// <param name="camera">The camera viewing the scene.</param>
        public SceneView(Scene scene, CameraSprite camera)
        {
            _scene = scene;
            _camera = camera;
        }

        /// <summary>
        /// Updates the scene.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            // parallax effect
            _scene.Sprite.GetSprite("Sky").Position = _camera.Position;
            _scene.Sprite.GetSprite("FarHills").Position = _camera.Position - (_camera.Position / 10);
            _scene.Sprite.GetSprite("NearHills").Position = _camera.Position - (_camera.Position / 5);
        }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // draw sections separately to preserve layers
            _scene.Sprite.GetSprite("Background").Draw(spriteBatch);
            _scene.Sprite.GetSprite("Water").Draw(spriteBatch);
            _scene.Sprite.GetSprite("Islands").Draw(spriteBatch);
        }

        private Scene _scene;
        private CameraSprite _camera;
    }
}
