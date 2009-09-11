using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Library.Audio;
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
        /// The coordinate of the near shore.
        /// </summary>
        public Vector2 NearShore { get; private set; }

        /// <summary>
        /// The coordinate of the far shore.
        /// </summary>
        public Vector2 FarShore { get; private set; }

        /// <summary>
        /// The (fixed) positon of the player character.
        /// </summary>
        public Vector2 PlayerPosition { get; private set; }

        /// <summary>
        /// The position of the non-player character.
        /// </summary>
        public Vector2 NPCPosition { get; private set; }

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
            Sprite = content.Load<SpriteDescriptorTemplate>("Sprites/Scene").Create();
            UpdatePositions();
        }

        /// <summary>
        /// Updates the scene.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            Sprite.GetAnimation("Waves").Update(time);
            Sprite.GetAnimation("Clouds").Update(time);
            Sprite.GetAnimation("Blink").Update(time);
            Sprite.GetAnimation("Bird").Update(time);
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
        /// Updates the ending animation.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        /// <returns>True if the animation is still running; otherwise, false.</returns>
        public bool UpdateEnding(float time)
        {
            bool running = Sprite.GetAnimation("StoryWin").Update(time);
            Sprite.GetAnimation("Waves").Update(time);
            UpdatePositions();
            return running;
        }

        /// <summary>
        /// Resets the positions of the scene items for the story.
        /// </summary>
        public void StartStory()
        {
            Sprite.GetAnimation("Story").Start();
        }

        /// <summary>
        /// Resets the positions of the scene items for gameplay.
        /// </summary>
        public void EndStory()
        {
            Sprite.GetAnimation("EndStory").Start();
            Sprite.GetAnimation("EndStory").Update(1f);
            Sprite.GetAnimation<AudioAnimation>("RumbleSound").Stop();
        }

        /// <summary>
        /// Resets the positions of the scene items for the ending.
        /// </summary>
        public void StartEnding()
        {
            Sprite.GetAnimation("StoryWin").Start();
        }

        /// <summary>
        /// Moves the right island towards the left island the specified distance.
        /// </summary>
        /// <param name="distance">The distance to move the island.</param>
        public void MoveFarShore(float distance)
        {
            Sprite rightIsland= Sprite.GetSprite("RightIsland");
            rightIsland.Position = new Vector2(rightIsland.Position.X - distance, rightIsland.Position.Y);
            UpdatePositions();
        }

        /// <summary>
        /// Updates the positions of the scene items.
        /// </summary>
        private void UpdatePositions()
        {
            WaterLevel = Sprite.GetSprite("Water").Position.Y + 25f;

            Vector2 leftIslandPos = Sprite.GetSprite("LeftIsland").Position;
            Sprite cliff = Sprite.GetSprite("Cliff");
            NearShore = cliff.Position + cliff.Size + leftIslandPos;

            Vector2 farCliffPos = Sprite.GetSprite("FarCliff").Position;
            Vector2 rightIslandPos = Sprite.GetSprite("RightIsland").Position;
            FarShore = farCliffPos + rightIslandPos;

            PlayerPosition = Sprite.GetSprite("Girl").Position + leftIslandPos;
            NPCPosition = Sprite.GetSprite("Boy").Position + rightIslandPos;
        }
    }
}