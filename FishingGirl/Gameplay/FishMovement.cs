using System;

using Microsoft.Xna.Framework;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Describes the movement of a fish.
    /// </summary>
    public class FishMovement
    {
        /// <summary>
        /// The (maximum) speed to swim at.
        /// </summary>
        public readonly float Speed;

        /// <summary>
        /// A constant acceleration.
        /// </summary>
        public readonly float Acceleration;

        /// <summary>
        /// The rectangular area to swim around in.
        /// </summary>
        public readonly Vector4 Range;

        /// <summary>
        /// Creates a new fish path.
        /// </summary>
        public FishMovement(float speed, float acceleration, Vector4 range)
        {
            Speed = speed;
            Acceleration = acceleration;
            Range = range;
        }
    }
}
