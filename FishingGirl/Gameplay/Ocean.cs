﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Sprite;
using Library.Sprite.Pipeline;
using Library.Animation;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Models an ocean full of fish.
    /// </summary>
    public class Ocean
    {
        /// <summary>
        /// Creates a new ocean.
        /// </summary>
        /// <param name="fishing">The fishing state.</param>
        public Ocean(FishingState fishing)
        {
            _fishing = fishing;
            _fishing.Event += OnFishingEvent;
            CreateFishes();
        }

        /// <summary>
        /// Loads all the contents of this ocean.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _content = content;
            _fishes.ForEach(fish => fish.LoadContent(content));

            // run the simulation for a few ticks to put the fish into position
            for (int i = 0; i < 60 * 30; i++)
            {
                _fishes.ForEach(fish => fish.Update(1f / 60f));
            }
        }

        /// <summary>
        /// Updates all the fish in this ocean.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            _fishes.ForEach(fish => fish.Update(time));

            List<RespawningFish> respawning = _respawningFishes.Where(fish => fish.Update(time)).ToList();
            foreach (RespawningFish fish in respawning)
            {
                _fishes.Add(fish.Fish);
                _respawningFishes.Remove(fish);
            }
        }

        /// <summary>
        /// Draws all the fish in this ocean.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _fishes.ForEach(fish => fish.Draw(spriteBatch));
        }

        /// <summary>
        /// Creates all the fish in this ocean.
        /// </summary>
        private void CreateFishes()
        {
            _fishes.Add(CreateFish(FishSize.Small, 900f, 1200f, 800f));
            _fishes.Add(CreateFish(FishSize.Small, 1250f, 1400f, 820f));
            _fishes.Add(CreateFish(FishSize.Medium, 920f, 1250f, 900f));
            _fishes.Add(CreateFish(FishSize.Small, 1000f, 1400f, 950f));
            _fishes.Add(CreateFish(FishSize.Small, 1450f, 1600f, 940f));
            _fishes.Add(CreateFish(FishSize.Small, 1100f, 1400f, 1200f));
            _fishes.Add(CreateFish(FishSize.Medium, 900f, 1600f, 1100f));
            _fishes.Add(CreateFish(FishSize.Medium, 1450f, 1650f, 810f));
            _fishes.Add(CreateFish(FishSize.Large, 1600f, 2000f, 1180f));
            _fishes.Add(CreateFish(FishSize.Small, 1750f, 1900f, 820f));
            _fishes.Add(CreateFish(FishSize.Medium, 1950f, 2250f, 800f));
            _fishes.Add(CreateFish(FishSize.Large, 1800f, 2200f, 900f));
            _fishes.Add(CreateFish(FishSize.Medium, 1950f, 2250f, 1100f));
            _fishes.Add(CreateFish(FishSize.Medium, 1500f, 1680f, 1000f));
            _fishes.Add(CreateFish(FishSize.Small, 1720f, 1900f, 1000f));
            _fishes.Add(CreateFish(FishSize.Small, 1800f, 2000f, 1050f));
            _fishes.Add(CreateFish(FishSize.Large, 930f, 1300f, 1400f));
            _fishes.Add(CreateFish(FishSize.Medium, 1350f, 1500f, 1300f));
            _fishes.Add(CreateFish(FishSize.Small, 1550f, 1650f, 1250f));
            _fishes.Add(CreateFish(FishSize.VeryLarge, 1100, 1500f, 1700f));
            _fishes.Add(CreateFish(FishSize.Small, 2275f, 2400f, 790f));
            _fishes.Add(CreateFish(FishSize.Small, 2600f, 2700f, 875f));
            _fishes.Add(CreateFish(FishSize.Small, 2250f, 2750f, 910f));
            _fishes.Add(CreateFish(FishSize.Medium, 2300f, 2520f, 960f));
            _fishes.Add(CreateFish(FishSize.Small, 2500f, 2725f, 1000f));
            _fishes.Add(CreateFish(FishSize.Medium, 2400f, 2650f, 1075f));
            _fishes.Add(CreateFish(FishSize.Small, 2220f, 2500f, 1120f));
            _fishes.Add(CreateFish(FishSize.Large, 2300f, 2720f, 1250f));
            _fishes.Add(CreateFish(FishSize.Small, 1750f, 2000f, 1300f));
            _fishes.Add(CreateFish(FishSize.Medium, 1600f, 1900f, 1400f));
            _fishes.Add(CreateFish(FishSize.Medium, 2400f, 2750f, 1375f));
            _fishes.Add(CreateFish(FishSize.VeryLarge, 2000f, 2300f, 1500f));
        }

        /// <summary>
        /// Creates a semi-ranom fish. The fish's rarity, modifier, and speed are chosen randomly.
        /// </summary>
        /// <param name="size">The size of the fish.</param>
        /// <param name="left">The left side of fish's path.</param>
        /// <param name="right">The right edge of the fish's path.</param>
        /// <param name="depth">The depth at which the fish swims.</param>
        private Fish CreateFish(FishSize size, float left, float right, float depth)
        {
            FishDescription description = new FishDescription(
                size,
                GetRandomRarity(),
                GetRandomModifier());

            FishMovement movement = new FishMovement(
                GetRandomSpeed(description),
                300f,
                new Vector4(left, depth - 5f, right, depth + 5f));

            FishBehavior behavior = new FishBehavior(
                150f,
                (float)(Math.PI / 6),
                2f);

            return new Fish(description, movement, behavior, _fishing);
        }

        /// <summary>
        /// Schedules a replacement for the given fish.
        /// </summary>
        /// <param name="fish">The fish to respawn.</param>
        private void AddFishToRespawn(Fish fish)
        {
            Fish respawn = CreateFish(
                fish.Description.Size,
                fish.Movement.Range.X,
                fish.Movement.Range.Z,
                fish.Movement.Range.Y + 5f);
            respawn.LoadContent(_content);
            _respawningFishes.Add(new RespawningFish(
                respawn,
                GetRandomRespawnTime(respawn.Description)));
        }

        /// <summary>
        /// Gets a random fish size.
        /// </summary>
        private FishSize GetRandomSize()
        {
            return SelectRandom(
                new WeightedChoice<FishSize>(FishSize.Small, 40),
                new WeightedChoice<FishSize>(FishSize.Medium, 40),
                new WeightedChoice<FishSize>(FishSize.Large, 19),
                new WeightedChoice<FishSize>(FishSize.VeryLarge, 1));
        }

        /// <summary>
        /// Gets a random fish rarity.
        /// </summary>
        private FishRarity GetRandomRarity()
        {
            return SelectRandom(
                new WeightedChoice<FishRarity>(FishRarity.Common, 50),
                new WeightedChoice<FishRarity>(FishRarity.Uncommon, 35),
                new WeightedChoice<FishRarity>(FishRarity.Rare, 10),
                new WeightedChoice<FishRarity>(FishRarity.VeryRare, 5));
        }

        /// <summary>
        /// Gets a random fish modifier.
        /// </summary>
        private FishModifier GetRandomModifier()
        {
            return SelectRandom(
                new WeightedChoice<FishModifier>(FishModifier.Typical, 55),
                new WeightedChoice<FishModifier>(FishModifier.Ancient, 10),
                new WeightedChoice<FishModifier>(FishModifier.Young, 5),
                new WeightedChoice<FishModifier>(FishModifier.Beautiful, 10),
                new WeightedChoice<FishModifier>(FishModifier.Ugly, 5),
                new WeightedChoice<FishModifier>(FishModifier.Fat, 10),
                new WeightedChoice<FishModifier>(FishModifier.Thin, 5));
        }

        /// <summary>
        /// Returns a random speed.
        /// </summary>
        private float GetRandomSpeed(FishDescription description)
        {
            float speed = _random.Next(25, 100);
            switch (description.Size)
            {
                case FishSize.Small: speed *= 1.1f; break;
                case FishSize.Medium: speed *= 1f; break;
                case FishSize.Large: speed *= 0.9f; break;
                case FishSize.VeryLarge: speed *= 0.8f; break;
            }
            return speed;
        }

        /// <summary>
        /// Returns a random respawn wait time, in seconds.
        /// </summary>
        private float GetRandomRespawnTime(FishDescription description)
        {
            float time = _random.Next(45, 120);
            switch (description.Size)
            {
                case FishSize.Small: time *= 0.75f; break;
                case FishSize.Medium: time *= 0.9f; break;
                case FishSize.Large: time *= 1f; break;
                case FishSize.VeryLarge: time *= 1f; break;
            }
            return time;
        }

        /// <summary>
        /// Selects a random element from a weighted list of choices.
        /// </summary>
        /// <typeparam name="T">The type of item to choose.</typeparam>
        /// <param name="weights">The choices and their weights.</param>
        /// <returns>A random element.</returns>
        private T SelectRandom<T>(params WeightedChoice<T>[] tuples)
        {
            int totalWeight = tuples.Sum(t => t.Weight);
            int random = _random.Next(totalWeight);
            foreach (WeightedChoice<T> tuple in tuples)
            {
                if (random < tuple.Weight)
                {
                    return tuple.Item;
                }
                random -= tuple.Weight;
            }
            return tuples[0].Item;
        }

        /// <summary>
        /// Removes a fish from the ocean when it is caught or eaten.
        /// </summary>
        private void OnFishingEvent(object stateObj, FishingEventArgs e)
        {
            if (e.Event == FishingEvent.FishCaught ||
                e.Event == FishingEvent.FishEaten)
            {
                _fishes.Remove(e.Fish);
                AddFishToRespawn(e.Fish);
            }
        }

        /// <summary>
        /// A choice and its relative weight.
        /// </summary>
        private class WeightedChoice<T>
        {
            public readonly T Item;
            public readonly int Weight;

            public WeightedChoice(T item, int weight)
            {
                Item = item;
                Weight = weight;
            }
        }

        /// <summary>
        /// A fish waiting to be respawned.
        /// </summary>
        private class RespawningFish
        {
            public readonly Fish Fish;

            public RespawningFish(Fish fish, float timeRemaining)
            {
                Fish = fish;
                _timeRemaining = timeRemaining;
            }

            public bool Update(float time)
            {
                _timeRemaining -= time;
                return (_timeRemaining <= 0);
            }

            private float _timeRemaining;
        }

        private List<Fish> _fishes = new List<Fish>();
        private List<RespawningFish> _respawningFishes = new List<RespawningFish>();

        private FishingState _fishing;
        private ContentManager _content;

        private Random _random = new Random();
    }
}

