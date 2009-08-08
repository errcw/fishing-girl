using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using Library.Storage;

using FishingGirl.Properties;

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
        /// The game context providing the badge data.
        /// </summary>
        [XmlIgnore]
        public virtual BadgeContext Context { get; set; }

        /// <summary>
        /// Updates this badge to check if it is earned.
        /// </summary>
        /// <returns>True if this badge was earned this update; otherwise, false.</returns>
        public virtual bool Update() { return false; }
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
        /// The game context providing the badge data.
        /// </summary>
        public BadgeContext Context
        {
            set { _badges.ForEach(badge => badge.Context = value); }
        }

        /// <summary>
        /// Creates a new set of badges.
        /// </summary>
        public Badges()
        {
            _badges = new List<Badge>();
            _badges.Add(new SmallAccumulatedMoneyBadge());
            _badges.Add(new LargeAccumulatedMoneyBadge());
            _badges.Add(new TotalMoneyBadge());
        }

        /// <summary>
        /// Loads the badges from the specified storage.
        /// </summary>
        public void Load(Storage storage)
        {
            try
            {
                if (storage.Exists(_storedBadges))
                {
                    storage.Load(_storedBadges);
                    _badges = new List<Badge>(_storedBadges.Data);
                }
            }
            // nothing to do on failure except use the existing badges
            catch (IOException) { }
            catch (InvalidOperationException) { }
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

    /// <summary>
    /// All the game systems.
    /// </summary>
    public class BadgeContext
    {
        public FishingState Fishing { get; set; }
        public Timer Timer { get; set; }
        public Money Money { get; set; }
        public Store Store { get; set; }
    }

    /// <summary>
    /// A badge earned when the player accumulates some number of coins.
    /// </summary>
    public class AccumulatedMoneyBadge : Badge
    {
        public int Accumulated { get; set; }

        public override BadgeContext Context {
            set
            {
                value.Money.AmountChanged += (o, a) =>
                {
                    if (a.ChangeInAmount > 0)
                    {
                        Accumulated += a.ChangeInAmount;
                        if (Accumulated > Threshold)
                        {
                            IsEarned = true;
                        }
                    }
                };
            }
        }

        public AccumulatedMoneyBadge(int threshold)
        {
            Name = "";
            Description = "";
            IsEarned = false;
            Accumulated = 0;
            Threshold = threshold;
        }

        public override bool Update()
        {
            return IsEarned;
        }

        private readonly int Threshold;
    }

    /// <summary>
    /// A badge earned when the player has accumulated over 5,000 coins.
    /// </summary>
    public class SmallAccumulatedMoneyBadge : AccumulatedMoneyBadge
    {
        public SmallAccumulatedMoneyBadge() : base(5000)
        {
            Name = "";
            Description = "";
        }
    }

    /// <summary>
    /// A badge earned when the player has accumulated over 100,000 coins.
    /// </summary>
    public class LargeAccumulatedMoneyBadge : AccumulatedMoneyBadge
    {
        public LargeAccumulatedMoneyBadge() : base(10000)
        {
            Name = "";
            Description = "";
        }
    }

    /// <summary>
    /// A badge earned when the player has over 1,000 coins at any point.
    /// </summary>
    public class TotalMoneyBadge : Badge
    {
        public TotalMoneyBadge()
        {
            Name = Resources.BadgeTotalMoney;
            Description = string.Format(Resources.BadgeTotalMoneyDescription, Total);
        }

        public override bool Update()
        {
            IsEarned = Context.Money.Amount >= Total;
            return IsEarned;
        }

        private const int Total = 5;
    }
}
