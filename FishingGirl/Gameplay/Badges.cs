using System;
using System.Collections.Generic;

using Library.Storage;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Event arguments for when a badge is earned.
    /// </summary>
    public sealed class BadgeEventArgs : EventArgs
    {
        public Badge Badge { get; set; }
    }

    /// <summary>
    /// An acheivement.
    /// </summary>
    public abstract class Badge
    {
        /// <summary>
        /// The name of this badge.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// A description of how to acheive this badge.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// If this badge has been earned.
        /// </summary>
        public bool IsEarned { get; protected set; }

        /// <summary>
        /// Updates this badge to check if it is earned.
        /// </summary>
        /// <returns>True if this badge was earned this update; otherwise, false.</returns>
        public abstract bool Update();
    }

    public class AccumulatedMoneyBadge : Badge
    {
        public int Accumulated { get; set; }

        public AccumulatedMoneyBadge()
        {
            Name = "";
            Description = "";
            IsEarned = false;
            Accumulated = 0;
        }

        public override bool Update()
        {
            return false;
        }
    }

    /// <summary>
    /// A set of badges.
    /// </summary>
    public class Badges
    {
        /// <summary>
        /// Occurs when a new badge is earned.
        /// </summary>
        public event EventHandler<BadgeEventArgs> BadgeEarned;

        /// <summary>
        /// Loads the badges from the specified storage.
        /// </summary>
        public void Load(Storage storage)
        {
            storage.Load(_storedBadges);
            _badges = new List<Badge>(_storedBadges.Data);
        }

        /// <summary>
        /// Saves the badges to the specified storage.
        /// </summary>
        public void Save(Storage storage)
        {
            _storedBadges.Data = _badges.ToArray();
            storage.Save(_storedBadges);
        }

        /// <summary>
        /// Updates the badges, looking for new badges earned.
        /// </summary>
        /// <param name="elapsed">The elapsed time, in seconds, since the last update.</param>
        public void Update(float elapsed)
        {
            foreach (Badge badge in _badges)
            {
                if (!badge.IsEarned)
                {
                    if (badge.Update())
                    {
                        OnBadgeEarned(badge);
                    }
                }
            }
        }

        /// <summary>
        /// Notifies listeners that the specified badge has been earned.
        /// </summary>
        private void OnBadgeEarned(Badge badge)
        {
            if (BadgeEarned != null)
            {
                BadgeEventArgs args = new BadgeEventArgs();
                args.Badge = badge;

                BadgeEarned(this, args);
            }
        }

        private List<Badge> _badges;
        private readonly XmlStoreable<Badge[]> _storedBadges = new XmlStoreable<Badge[]>("Badges");
    }
}
