using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using FishingGirl.Gameplay;

using Library.Animation;
using Library.Sprite;
using Library.Sprite.Pipeline;

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
        /// Loads the scene content.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _bubbleTemplate = content.Load<ImageSpriteTemplate>("AirBubble1");

            _bubbles = new List<Bubble>();
            for (int i = 0; i < BubbleCount; i++)
            {
                _bubbles.Add(CreateBubble());
            }
        }

        /// <summary>
        /// Updates the scene.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            UpdateBubbles(time);

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

        /// <summary>
        /// Updates the bubbles floating in the water.
        /// </summary>
        private void UpdateBubbles(float time)
        {
            int removed = _bubbles.RemoveAll(bubble =>
            {
                bool popped = !bubble.Update(time);
                if (popped)
                {
                    _scene.Sprite.GetSprite<CompositeSprite>("Bubbles").Remove(bubble.Sprite);
                }
                return popped;
            });
            while (removed-- > 0)
            {
                _bubbles.Add(CreateBubble());
            }
        }

        /// <summary>
        /// Creates a single bubble that floats toward the surface and fades out.
        /// </summary>
        /// <returns></returns>
        private Bubble CreateBubble()
        {
            Vector2 oceanSize = _scene.Sprite.GetSprite("Water").Size;
            Vector2 start = new Vector2(
                _random.Next(0, (int)oceanSize.X),
                _random.Next(BubbleDepthMin, BubbleDepthMax));
            int height = _random.Next(BubbleTravelMin, Math.Min(BubbleTravelMax, (int)start.Y));
            Vector2 end = new Vector2(
                start.X,
                start.Y - height);

            Sprite bubbleSprite = _bubbleTemplate.Create();
            bubbleSprite.Position = start;
            bubbleSprite.Color = Color.TransparentWhite;

            _scene.Sprite.GetSprite<CompositeSprite>("Bubbles").Add(bubbleSprite);

            IAnimation animation =
                new SequentialAnimation(
                    new DelayAnimation((float)_random.NextDouble() * 10),
                    new CompositeAnimation(
                        new PositionAnimation(bubbleSprite, end, 2f, PositionInterpolation),
                        new SequentialAnimation(
                            new ColorAnimation(bubbleSprite, Color.White, 0.5f, ColorInterpolation),
                            new DelayAnimation(1f),
                            new ColorAnimation(bubbleSprite, Color.TransparentWhite, 0.5f, ColorInterpolation))));

            Bubble bubble = new Bubble();
            bubble.Sprite = bubbleSprite;
            bubble.Animation = animation;

            return bubble;
        }

        /// <summary>
        /// A single bubble.
        /// </summary>
        private class Bubble
        {
            public Sprite Sprite { get; set; }
            public IAnimation Animation { get; set; }
            public bool Update(float time)
            {
                return Animation.Update(time);
            }
        }

        private Scene _scene;
        private CameraSprite _camera;

        private List<Bubble> _bubbles;
        private ImageSpriteTemplate _bubbleTemplate;

        private Random _random = new Random();

        private const int BubbleCount = 40;
        private const int BubbleDepthMin = 20;
        private const int BubbleDepthMax = 700;
        private const int BubbleTravelMin = 20;
        private const int BubbleTravelMax = 50;

        private Interpolate<Color> ColorInterpolation = Interpolation.InterpolateColor(Easing.Uniform);
        private Interpolate<Vector2> PositionInterpolation = Interpolation.InterpolateVector2(Easing.QuadraticIn);
    }
}
