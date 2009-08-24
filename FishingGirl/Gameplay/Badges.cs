using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

using Library.Storage;

using FishingGirl.Properties;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Event arguments for when a badge is earned.
    /// </summary>
    public sealed class BadgeEventArgs : EventArgs
    {
        public readonly Badge Badge;

        public BadgeEventArgs(Badge badge)
        {
            Badge = badge;
        }
    }

    /// <summary>
    /// An acheivement.
    /// </summary>
    [XmlInclude(typeof(SmallAccumulatedMoneyBadge))]
    [XmlInclude(typeof(LargeAccumulatedMoneyBadge))]
    [XmlInclude(typeof(TotalMoneyBadge))]
    [XmlInclude(typeof(WonGameBadge))]
    [XmlInclude(typeof(FastWonGameBadge))]
    [XmlInclude(typeof(ManyWonGameBadge))]
    [XmlInclude(typeof(CatchEveryFishBadge))]
    [XmlInclude(typeof(BuyEveryItemBadge))]
    [XmlInclude(typeof(BronzeCastBadge))]
    [XmlInclude(typeof(SilverCastBadge))]
    [XmlInclude(typeof(GoldCastBadge))]
    [XmlInclude(typeof(ChainBadge))]
    public abstract class Badge
    {
        /// <summary>
        /// The name of this badge.
        /// </summary>
        [XmlIgnore]
        public string Name { get; protected set; }

        /// <summary>
        /// A description of how to acheive this badge.
        /// </summary>
        [XmlIgnore]
        public string Description { get; protected set; }

        /// <summary>
        /// If this badge has been earned.
        /// </summary>
        public bool IsEarned { get; set; }

        /// <summary>
        /// The game context providing the badge data.
        /// </summary>
        [XmlIgnore]
        public virtual BadgeContext Context { get; set; }

        /// <summary>
        /// Updates this badge to check if it is earned.
        /// </summary>
        /// <returns>True if this badge was earned this update; otherwise, false.</returns>
        public bool Update()
        {
            IsEarned = _earned;
            return _earned;
        }

        protected bool _earned = false;
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
        /// The set of badges.
        /// </summary>
        public IEnumerable<Badge> BadgeSet
        {
            get { return _badges; }
        }

        /// <summary>
        /// The game context providing the badge data.
        /// </summary>
        public BadgeContext Context
        {
            get { return _context; }
            set { SetContext(value);  }
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
            _badges.Add(new WonGameBadge());
            _badges.Add(new FastWonGameBadge());
            _badges.Add(new ManyWonGameBadge());
            _badges.Add(new CatchEveryFishBadge());
            _badges.Add(new BuyEveryItemBadge());
            _badges.Add(new BronzeCastBadge());
            _badges.Add(new SilverCastBadge());
            _badges.Add(new GoldCastBadge());
            _badges.Add(new ChainBadge());
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
                    if (_storedBadges.Data.Length > 0)
                    {
                        _badges = new List<Badge>(_storedBadges.Data);
                        SetContext(_context); // set the context on the badges
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Saves the badges to the specified storage.
        /// </summary>
        public void Save(Storage storage)
        {
            try
            {
                _storedBadges.Data = _badges.ToArray();
                storage.Save(_storedBadges);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
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
        /// Sets the current context.
        /// </summary>
        private void SetContext(BadgeContext newContext)
        {
            _context = newContext;
            _badges.ForEach(badge => badge.Context = _context);
        }

        /// <summary>
        /// Notifies listeners that the specified badge has been earned.
        /// </summary>
        private void OnBadgeEarned(Badge badge)
        {
            if (BadgeEarned != null)
            {
                BadgeEventArgs args = new BadgeEventArgs(badge);
                BadgeEarned(this, args);
            }
        }

        private List<Badge> _badges;
        private readonly XmlStoreable<Badge[]> _storedBadges = new XmlStoreable<Badge[]>("Badges");

        private BadgeContext _context;
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

    public abstract class AccumulatedMoneyBadge : Badge
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
                        if (Accumulated > AccumulationTarget)
                        {
                            _earned = true;
                        }
                    }
                };
            }
        }

        public AccumulatedMoneyBadge(int threshold)
        {
            Name = "";
            Description = "";
            Accumulated = 0;
            AccumulationTarget = threshold;
        }

        private readonly int AccumulationTarget;
    }

    public class SmallAccumulatedMoneyBadge : AccumulatedMoneyBadge
    {
        public SmallAccumulatedMoneyBadge() : base(TargetAmount)
        {
            Name = Resources.BadgeSmallAccumulatedMoney;
            Description = string.Format(Resources.BadgeSmallAccumulatedMoneyDescription, TargetAmount);
        }
        private const int TargetAmount = 5000;
    }

    public class LargeAccumulatedMoneyBadge : AccumulatedMoneyBadge
    {
        public LargeAccumulatedMoneyBadge() : base(TargetAmount)
        {
            Name = Resources.BadgeLargeAccumulatedMoney;
            Description = string.Format(Resources.BadgeLargeAccumulatedMoneyDescription, TargetAmount);
        }
        private const int TargetAmount = 20000;
    }

    public class TotalMoneyBadge : Badge
    {
        public override BadgeContext Context
        {
            set
            {
                value.Money.AmountChanged += (o, a) =>
                    _earned = value.Money.Amount >= Total;
            }
        }

        public TotalMoneyBadge()
        {
            Name = Resources.BadgeTotalMoney;
            Description = string.Format(Resources.BadgeTotalMoneyDescription, Total);
        }

        private const int Total = 1000;
    }

    public class WonGameBadge : Badge
    {
        public override BadgeContext Context
        {
            set
            {
                value.Fishing.Event += (s, a) =>
                    _earned = (a.Event == FishingEvent.LureIsland);
            }
        }

        public WonGameBadge()
        {
            Name = Resources.BadgeWonGame;
            Description = Resources.BadgeWonGameDescription;
        }
    }

    public class FastWonGameBadge : Badge
    {
        public override BadgeContext Context
        {
            set
            {
                value.Fishing.Event += (s, a) =>
                    _earned = 
                        (a.Event == FishingEvent.LureIsland) &&
                        (value.Timer.Time > TimeInMinutes*60*60);
            }
        }

        public FastWonGameBadge()
        {
            Name = Resources.BadgeFastWonGame;
            Description = string.Format(Resources.BadgeFastWonGameDescription, TimeInMinutes);
        }

        private const float TimeInMinutes = 5;
    }

    public class ManyWonGameBadge : Badge
    {
        public int WinCount { get; set; }

        public override BadgeContext Context
        {
            set
            {
                value.Fishing.Event += (s, a) =>
                {
                    if (a.Event == FishingEvent.LureIsland)
                    {
                        WinCount += 1;
                    }
                    _earned = WinCount >= WinCountThreshold;
                };
            }
        }

        public ManyWonGameBadge()
        {
            Name = Resources.BadgeManyWonGame;
            Description = string.Format(Resources.BadgeManyWonGameDescription, WinCountThreshold);
            WinCount = 0;
        }

        private const int WinCountThreshold = 20;
    }

    public class CatchEveryFishBadge : Badge
    {
        public bool[] CaughtFish { get; set; }

        public override BadgeContext Context
        {
            set
            {
                value.Fishing.Event += (s, a) =>
                {
                    if (a.Event == FishingEvent.FishCaught)
                    {
                        CaughtFish[(int)a.Fish.Description.Size * 4 + (int)a.Fish.Description.Rarity] = true;
                        _earned = CaughtFish.All(c => c);
                    }
                };
            }
        }

        public CatchEveryFishBadge()
        {
            Name = Resources.BadgeCatchEveryFish;
            Description = Resources.BadgeCatchEveryFishDescription;
            CaughtFish = new bool[12];
        }
    }

    public class BuyEveryItemBadge : Badge
    {
        public bool[] BoughtItems { get; set; }

        public override BadgeContext Context
        {
            set
            {
                value.Store.ItemPurchased += (s, a) =>
                {
                    if (a.Item.Name == Resources.StoreRodSilver) BoughtItems[0] = true;
                    else if (a.Item.Name == Resources.StoreRodGold) BoughtItems[1] = true;
                    else if (a.Item.Name == Resources.StoreRodLegendary) BoughtItems[2] = true;
                    else if (a.Item.Name == Resources.StoreLureSmall) BoughtItems[3] = true;
                    else if (a.Item.Name == Resources.StoreLureMedium) BoughtItems[4] = true;
                    else if (a.Item.Name == Resources.StoreLureLarge) BoughtItems[5] = true;
                    else if (a.Item.Name == Resources.StoreLureSmallUpgraded) BoughtItems[6] = true;
                    else if (a.Item.Name == Resources.StoreLureLargeUpgraded) BoughtItems[7] = true;
                    _earned = BoughtItems.All(i => i);
                };
            }
        }

        public BuyEveryItemBadge()
        {
            Name = Resources.BadgeBuyEveryItem;
            Description = Resources.BadgeBuyEveryItemDescription;
            BoughtItems = new bool[8];
        }
    }

    public class CastDistanceBadge : Badge
    {
        public override BadgeContext Context
        {
            set
            {
                value.Fishing.ActionChanged += (s, a) =>
                {
                    if (a.Action == FishingAction.Reel)
                    {
                        FishingState state = (FishingState)s;
                        _earned = state.LureDistance >= Distance && state.Rod == Rod;
                    }
                };
            }
        }

        public CastDistanceBadge(RodType rod, float distance)
        {
            Rod = rod;
            Distance = distance;
        }

        private readonly RodType Rod;
        private readonly float Distance;
    }

    public class BronzeCastBadge : CastDistanceBadge
    {
        public BronzeCastBadge() : base(RodType.Bronze, Distance)
        {
            Name = Resources.BadgeBronzeCast;
            Description = string.Format(Resources.BadgeBronzeCastDescription, Distance);
        }
        private const float Distance = 14.0f;
    }

    public class SilverCastBadge : CastDistanceBadge
    {
        public SilverCastBadge() : base(RodType.Silver, Distance)
        {
            Name = Resources.BadgeSilverCast;
            Description = string.Format(Resources.BadgeSilverCastDescription, Distance);
        }
        private const float Distance = 24.0f;
    }

    public class GoldCastBadge : CastDistanceBadge
    {
        public GoldCastBadge() : base(RodType.Gold, Distance)
        {
            Name = Resources.BadgeGoldCast;
            Description = string.Format(Resources.BadgeGoldCastDescription, Distance);
        }
        private const float Distance = 38.0f;
    }

    public class ChainBadge : Badge
    {
        public override BadgeContext Context
        {
            set
            {
                bool caughtSmall = false, caughtMedium = false, caughtLarge = false;
                value.Fishing.ActionChanged += (s, a) =>
                {
                    if (a.Action == FishingAction.Idle)
                    {
                        caughtSmall = caughtMedium = caughtLarge = false;
                    }
                };
                value.Fishing.Event += (s, a) =>
                {
                    switch (a.Event)
                    {
                        case FishingEvent.FishHooked:
                            caughtSmall = (a.Fish.Description.Size == FishSize.Small);
                            break;
                        case FishingEvent.FishEaten:
                            if (a.Fish.Description.Size == FishSize.Small && caughtSmall)
                            {
                                caughtMedium = true;
                            }
                            else if (a.Fish.Description.Size == FishSize.Medium && caughtMedium)
                            {
                                caughtLarge = true;
                            }
                            break;
                        case FishingEvent.FishCaught:
                            _earned = caughtSmall && caughtMedium && caughtLarge;
                            break;
                    }
                };
            }
        }

        public ChainBadge()
        {
            Name = Resources.BadgeChain;
            Description = Resources.BadgeChainDescription;
        }
    }
}
