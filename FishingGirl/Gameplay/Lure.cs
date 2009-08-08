using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// A lure for catching fish.
    /// </summary>
    public interface Lure
    {
        /// <summary>
        /// The name of the lure sprite.
        /// </summary>
        string SpriteName { get; }

        /// <summary>
        /// Returns true if the specified fish should chase this lure; otherwise, false.
        /// </summary>
        bool IsAttractedTo(Fish fish, Fish hookedFish);

        /// <summary>
        /// Returns true if the specified fish should be hooked on this lure; false if the lure should break.
        /// </summary>
        bool BiteLure(Fish fish, Fish hookedFish);
    }

    /// <summary>
    /// The lures.
    /// </summary>
    public class Lures
    {
        /// <summary>
        /// The default lure. Attracts all fish but can only hook small ones.
        /// </summary>
        public static readonly Lure Basic = new LureImpl(
            "LureBasic",
            (fish, hooked) =>
                (hooked == null) || (hooked.Description.Size < fish.Description.Size),
            (fish, hooked) =>
                (fish.Description.Size == FishSize.Small)
        );

        /// <summary>
        /// The small lure. A chaining lure starting with small fish.
        /// </summary>
        public static readonly Lure Small = new LureImpl(
            "LureSmall",
            (fish, hooked) =>
                (hooked == null || hooked.Description.Size < fish.Description.Size),
            (fish, hooked) =>
                (hooked == null)
                    ? (fish.Description.Size == FishSize.Small)
                    : (fish.Description.Size == hooked.Description.Size + 1)
        );

        /// <summary>
        /// The medium lure. A chaining lure starting with medium fish.
        /// </summary>
        public static readonly Lure Medium = new LureImpl(
            "LureMedium",
            (fish, hooked) =>
                (hooked == null)
                    ? (fish.Description.Size >= FishSize.Medium)
                    : (fish.Description.Size > hooked.Description.Size),
            (fish, hooked) =>
                (hooked == null)
                    ? (fish.Description.Size == FishSize.Medium)
                    : (fish.Description.Size == hooked.Description.Size + 1)
        );

        /// <summary>
        /// The large lure. A lure for large fish that damages the fish it catches.
        /// </summary>
        public static readonly Lure Large = new LureImpl(
            "LureLarge",
            (fish, hooked) =>
                (hooked == null && fish.Description.Size >= FishSize.Large) ||
                (hooked.Description.Size < fish.Description.Size),
            delegate(Fish fish, Fish hookedFish)
            {
                bool hooked = (hookedFish == null && fish.Description.Size == FishSize.Large);
                if (hooked)
                {
                    fish.Description.Modifier = FishModifier.Ugly;
                }
                return hooked;
            }
        );

        /// <summary>
        /// The upgraded small lure. A chaining lure starting with small fish that
        /// always adds a bonus to hooked fish.
        /// </summary>
        public static readonly Lure SmallUpgraded = new LureImpl(
            "LureSmallUpgraded",
            Small.IsAttractedTo,
            delegate(Fish fish, Fish hookedFish)
            {
                bool hooked = Small.BiteLure(fish, hookedFish);
                if (hooked)
                {
                    fish.Description.Modifier = FishModifier.Beautiful;
                }
                return hooked;
            }
        );

        /// <summary>
        /// The upgraded large lure. A lure for large fish that imposes no penalties.
        /// </summary>
        public static readonly Lure LargeUpgraded = new LureImpl(
            "LureLargeUpgraded",
            Large.IsAttractedTo,
            (fish, hooked) =>
                (hooked == null && fish.Description.Size == FishSize.Large)
        );

        /// <summary>
        /// The set of all lures.
        /// </summary>
        public static readonly Lure[] AllLures = { Basic, Small, SmallUpgraded, Medium, Large, LargeUpgraded };

        /// <summary>
        /// A basic implementation of a lure.
        /// </summary>
        private class LureImpl : Lure
        {
            public LureImpl(string name, Func<Fish, Fish, bool> attracted, Func<Fish, Fish, bool> bite)
            {
                _spriteName = name;
                _isAttractedToFunc = attracted;
                _biteLureFunc = bite;
            }

            public string SpriteName
            {
                get { return _spriteName; }
            }

            public bool IsAttractedTo(Fish fish, Fish hookedFish)
            {
                return _isAttractedToFunc(fish, hookedFish);
            }

            public bool BiteLure(Fish fish, Fish hookedFish)
            {
                return _biteLureFunc(fish, hookedFish);
            }

            private string _spriteName;
            private Func<Fish, Fish, bool> _isAttractedToFunc;
            private Func<Fish, Fish, bool> _biteLureFunc;
        }
    }
}
