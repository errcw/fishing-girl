using System;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Describes how a fish behaves with a lure.
    /// </summary>
    public class FishBehavior
    {
        /// <summary>
        /// The distance at which the fish sees the lure.
        /// </summary>
        public readonly float LureSightDistance;

        /// <summary>
        /// The angle at which the fish sees the lure.
        /// </summary>
        public readonly float LureSightAngle;

        /// <summary>
        /// The multiplier applied to the fish's speed when lunging at the lure.
        /// </summary>
        public readonly float LungeSpeedMultiplier;

        /// <summary>
        /// Creates a new set of fish behavior.
        /// </summary>
        public FishBehavior(float lureSightDistance, float lureSightAngle, float lungeSpeedMultiplier)
        {
            LureSightDistance = lureSightDistance;
            LureSightAngle = lureSightAngle;
            LungeSpeedMultiplier = lungeSpeedMultiplier;
        }
    }
}
