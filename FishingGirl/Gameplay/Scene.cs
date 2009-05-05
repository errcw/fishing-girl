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
        public float WaterLevel { get; private set; }

        /// <summary>
        /// The X coordinate of the shore.
        /// </summary>
        public float ShoreX { get; private set; }

        /// <summary>
        /// The X coordinate of the far shore.
        /// </summary>
        public float FarShoreX { get; private set; }

        /// <summary>
        /// The (fixed) positon of the player character.
        /// </summary>
        public Vector2 PlayerPosition { get; private set; }

        /// <summary>
        /// The scene sprite.
        /// </summary>
        public SpriteDescriptor Sprite { get; private set; }

        /// <summary>
        /// Loads the content for this scene.
        /// </summary>
        /// <param name="content">The content manager to load.</param>
        public void LoadContent(ContentManager content)
        {
            Sprite = content.Load<SpriteDescriptorTemplate>("Sprites/Scene").Create(content);
            WaterLevel = Sprite.GetSprite("Water").Position.Y + 25f;
            ShoreX = Sprite.GetSprite("Cliff").Position.X + Sprite.GetSprite("Cliff").Size.X + Sprite.GetSprite("LeftIsland").Position.X;
            FarShoreX = Sprite.GetSprite("FarCliff").Position.X + Sprite.GetSprite("RightIsland").Position.X;
            PlayerPosition = Sprite.GetSprite("Girl").Position + Sprite.GetSprite("LeftIsland").Position;
        }

        /// <summary>
        /// Updates the scene.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            Sprite.GetAnimation("Waves").Update(time);
            Sprite.GetAnimation("BoyJumping").Update(time);
        }

        /// <summary>
        /// Updates the story animation.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void UpdateStory(float time)
        {
            Sprite.GetAnimation("Story").Update(time);
            Sprite.GetAnimation("Waves").Update(time);
        }

        /// <summary>
        /// Resets the positions of the scene items for gameplay.
        /// </summary>
        public void EndStory()
        {
            Sprite.GetAnimation("EndStory").Update(1f);
        }
    }
}