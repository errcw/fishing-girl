using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Library.Sprite;
using Library.Sprite.Pipeline;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// The background scene.
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// The water level in the scene.
        /// </summary>
        public float WaterLevel
        {
            get
            {
                return _scene.GetSprite("Water").Position.Y + 25f;
            }
        }

        /// <summary>
        /// The X coordinate of the shore.
        /// </summary>
        public float ShoreX
        {
            get
            {
                Sprite cliff = _scene.GetSprite("Cliff");
                return cliff.Position.X + cliff.Size.X + _scene.GetSprite("LeftIsland").Position.X;
            }
        }

        /// <summary>
        /// The X coordinate of the far shore.
        /// </summary>
        public float FarShoreX
        {
            get
            {
                return _scene.GetSprite("FarCliff").Position.X + _scene.GetSprite("RightIsland").Position.X;
            }
        }

        /// <summary>
        /// The (fixed) positon of the player character.
        /// </summary>
        public Vector2 PlayerPosition
        {
            get
            {
                return _scene.GetSprite("Girl").Position + _scene.GetSprite("LeftIsland").Position;
            }
        }

        /// <summary>
        /// Creates a new scene.
        /// </summary>
        /// <param name="camera">The camera viewing this scene.</param>
        public Scene(CameraSprite camera)
        {
            _camera = camera;
        }

        /// <summary>
        /// Loads the content for this scene.
        /// </summary>
        /// <param name="content">The content manager to load.</param>
        public void LoadContent(ContentManager content)
        {
            _scene = content.Load<SpriteDescriptorTemplate>("Sprites/Scene").Create(content);
        }

        /// <summary>
        /// Updates the scene.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            // parallax effect
            _scene.GetSprite("Sky").Position = _camera.Position;
            _scene.GetSprite("FarHills").Position = _camera.Position - (_camera.Position / 10);
            _scene.GetSprite("NearHills").Position = _camera.Position - (_camera.Position / 5);
            
            // idle animations
            _scene.GetAnimation("Waves").Update(time);
            _scene.GetAnimation("BoyJumping").Update(time);
            
        }

        /// <summary>
        /// Updates the story animation.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void UpdateStory(float time)
        {
            _scene.GetAnimation("Story").Update(time);
        }

        /// <summary>
        /// Resets the positions of the scene items for gameplay.
        /// </summary>
        public void EndStory()
        {
            _scene.GetAnimation("EndStory").Update(1f);
        }

        /// <summary>
        /// Draws this scene.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // draw sections separately to preserve layers
            _scene.GetSprite("Background").Draw(spriteBatch);
            _scene.GetSprite("Water").Draw(spriteBatch);
            _scene.GetSprite("Islands").Draw(spriteBatch);
        }

        private SpriteDescriptor _scene;

        private CameraSprite _camera;
    }
}