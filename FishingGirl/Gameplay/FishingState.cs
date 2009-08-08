using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Library.Sprite;
using Library.Sprite.Pipeline;
using Library.Animation;

using FishingGirl.Screens;
using FishingGirl.Properties;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Describes the actions involved in fishing.
    /// </summary>
    public enum FishingAction
    {
        Idle,
        Swing,
        Cast,
        Reel
    }

    /// <summary>
    /// Describes a fishing event.
    /// </summary>
    public enum FishingEvent
    {
        FishHooked,
        FishEaten,
        FishCaught,
        LureBroke,
        LureChanged,
        LureIsland
    }

    /// <summary>
    /// Describes the fishing mechanics.
    /// </summary>
    public class FishingState
    {
        /// <summary>
        /// Occurs when the current fishing action changes.
        /// </summary>
        public event EventHandler<FishingActionEventArgs> ActionChanged;

        /// <summary>
        /// Occurs when a lure event occurs.
        /// </summary>
        public event EventHandler<FishingEventArgs> Event;

        /// <summary>
        /// The type of lure used to fish.
        /// </summary>
        public Lure Lure { get { return Lures[_lureIndex]; } }

        /// <summary>
        /// The position of the lure.
        /// </summary>
        public Vector2 LurePosition { get { return _lurePosition; } }

        /// <summary>
        /// The lures available to to fish with.
        /// </summary>
        public List<Lure> Lures { get; private set; }

        /// <summary>
        /// The type of rod used to fish.
        /// </summary>
        public RodType Rod { get; set; }

        /// <summary>
        /// The rotation of the rod.
        /// </summary>
        public float RodRotation { get; private set; }

        /// <summary>
        /// The sprites to draw this state.
        /// </summary>
        public Dictionary<RodType, Sprite> RodSprites { get; private set; }
        public Dictionary<Lure, Sprite> LureSprites { get; private set; }
        public Sprite LineSprite { get; private set; }

        /// <summary>
        /// The length of the fishing line.
        /// </summary>
        public float MaxCastDistance
        {
            get { return ((GetSwingSweep(Rod) - SwingInitialRotation) / CastingMaxSweep) * CastingMaxDistance; }
        }

        /// <summary>
        /// Creates a new fishing state.
        /// </summary>
        /// <param name="game">The game context.</param>
        /// <param name="scene">The scene context of the state.</param>
        public FishingState(GameplayScreen game, Scene scene)
        {
            _game = game;
            _scene = scene;

            Rod = RodType.Bronze;
            RodRotation = SwingInitialRotation;

            Lures = new List<Lure>();
            Lures.Add(FishingGirl.Gameplay.Lures.Basic);
            _lureIndex = 0;

            _lineLength = IdleLineLength;

            EnterIdleState();
        }

        /// <summary>
        /// Loads the sprites necessary to display this state.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            RodSprites = new Dictionary<RodType, Sprite>();
            for (RodType rod = RodType.Bronze; rod <= RodType.Legendary; rod++)
            {
                Sprite rodSprite = content.Load<SpriteDescriptorTemplate>("Sprites/Fishing/Rod" + rod.ToString()).Create().Sprite;
                rodSprite.Position += _scene.PlayerPosition;
                RodSprites.Add(rod, rodSprite);
            }

            LureSprites = new Dictionary<Lure, Sprite>();
            for (int i = 0; i < FishingGirl.Gameplay.Lures.AllLures.Length; i++)
            {
                Lure lure = FishingGirl.Gameplay.Lures.AllLures[i];
                Sprite lureSprite = content.Load<SpriteDescriptorTemplate>("Sprites/Fishing/" + lure.SpriteName).Create().Sprite;
                LureSprites.Add(lure, lureSprite);
            }

            LineSprite = content.Load<SpriteDescriptorTemplate>("Sprites/Fishing/Line").Create().Sprite;

            // set up the lure now that we know what the rod looks like
            _lurePosition = GetRodTipPosition() + new Vector2(5f, 15f);
        }

        /// <summary>
        /// Updates the state of this fishing rod.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        /// <param name="input">The current input state.</param>
        public void Update(float time, Input input)
        {
            _stateTick(time, input);
            UpdateLure(time);
        }

        /// <summary>
        /// Returns true if the given fish should chase the lure; otherwise, false.
        /// </summary>
        public bool IsAttractedToLure(Fish fish)
        {
            if (_lureBroken || _lurePosition.Y <= _scene.WaterLevel)
            {
                return false;
            }
            return Lure.IsAttractedTo(fish, _hookedFish);
        }

        /// <summary>
        /// Bites the lure with the given fish.
        /// </summary>
        /// <param name="fish">The biting fish.</param>
        /// <returns>True if the bite hooked the fish; otherwise, false.</returns>
        public bool BiteLure(Fish fish)
        {
            bool hooked = Lure.BiteLure(fish, _hookedFish);
            if (hooked)
            {
                if (_hookedFish == null)
                {
                    _hookedFish = fish;
                    OnFishingEvent(FishingEvent.FishHooked);
                }
                else
                {
                    _hookedFish.OnEaten();
                    OnFishingEvent(FishingEvent.FishEaten);
                    _hookedFish = fish;
                }
            }
            else
            {
                if (_hookedFish != null)
                {
                    _hookedFish.OnEaten();
                    OnFishingEvent(FishingEvent.FishEaten);
                    _hookedFish = null;
                }
                _lureBroken = true;
                OnFishingEvent(FishingEvent.LureBroke);
            }
            return hooked;
        }

        /// <summary>
        /// Enters the idle state.
        /// </summary>
        private void EnterIdleState()
        {
            OnActionChanged(FishingAction.Idle);

            _lureFriction = AirFriction;
            _lureAcceleration = AirGravity;
            _lureBroken = false;

            _stateTick = delegate(float elapsed, Input input)
            {
                if (RodRotation > SwingInitialRotation)
                {
                    RodRotation -= IdleSpeed * elapsed;
                    if (RodRotation < SwingInitialRotation)
                    {
                        RodRotation = SwingInitialRotation;
                    }
                }
                if (input.Action.Pressed && RodRotation <= SwingInitialRotation)
                {
                    EnterSwingingState();
                }

                if (input.AltAction.PressedRepeat)
                {
                    _lureIndex = (_lureIndex + 1) % Lures.Count;
                    OnFishingEvent(FishingEvent.LureChanged);
                }
            };
        }

        /// <summary>
        /// Enters the swinging state.
        /// </summary>
        private void EnterSwingingState()
        {
            OnActionChanged(FishingAction.Swing);

            float swingPosition = 0f;
            float swingSweep = GetSwingSweep(Rod);

            _stateTick = delegate(float elapsed, Input input)
            {
                if (input.Action.Down)
                {
                    swingPosition += SwingSpeed * elapsed;

                    float rotation = swingPosition;
                    if (swingPosition > swingSweep)
                    {
                        rotation = 2 * swingSweep - swingPosition;
                    }

                    RodRotation = rotation + SwingInitialRotation;
                    if (RodRotation <= SwingInitialRotation)
                    {
                        RodRotation = SwingInitialRotation;
                        EnterIdleState();
                    }
                }
                else
                {
                    EnterCastingState();
                }
            };
        }

        /// <summary>
        /// Enters the casting state.
        /// </summary>
        private void EnterCastingState()
        {
            OnActionChanged(FishingAction.Cast);

            float power = (RodRotation - SwingInitialRotation) / CastingMaxSweep;
            _lineLength = Math.Max(power * CastingMaxDistance, CastingMinDistance);
            _lureVelocity = CastingMaxVelocity * power;

            _stateTick = delegate(float elapsed, Input input)
            {
                if (_lurePosition.X > _scene.FarShore.X && _lurePosition.Y > _scene.FarShore.Y)
                {
                    EnterIslandState();
                }
                else if (_lurePosition.Y > _scene.WaterLevel)
                {
                    _lineLength = Vector2.Distance(_lurePosition, GetRodTipPosition()); // set the length
                    EnterReelingState();
                }
                if (RodRotation > SwingInitialRotation)
                {
                    RodRotation -= CastingSpeed * elapsed;
                }
            };
        }

        /// <summary>
        /// Enters the reeling state.
        /// </summary>
        private void EnterReelingState()
        {
            OnActionChanged(FishingAction.Reel);

            _lureFriction = WaterFriction;
            _lureAcceleration = WaterGravity;
            _reelSpeed = ReelSpeed;

            _stateTick = delegate(float elapsed, Input input)
            {
                if (input.Action.Down)
                {
                    _reelSpeed += GetReelAcceleration() * elapsed;
                    _lineLength -= _reelSpeed * elapsed;
                    if (_lineLength <= IdleLineLength)
                    {
                        if (_hookedFish != null)
                        {
                            _hookedFish.OnCaught();
                            OnFishingEvent(FishingEvent.FishCaught);
                            _hookedFish = null;
                        }
                        _lineLength = IdleLineLength;
                        EnterIdleState();
                    }
                }
                else
                {
                    _reelSpeed = ReelSpeed;
                }
            };
        }

        /// <summary>
        /// Enters the island state.
        /// </summary>
        private void EnterIslandState()
        {
            OnFishingEvent(FishingEvent.LureIsland);

            _lureVelocity = Vector2.Zero;
            _lureAcceleration = Vector2.Zero;
            _lureFriction = 0f;

            Vector2 offset = _lurePosition - _scene.FarShore;
            
            _stateTick = delegate(float elapsed, Input input)
            {
                // have the lure follow the moving island
                _lurePosition = _scene.FarShore + offset;
            };
        }

        /// <summary>
        /// Updates the position of the lure.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateLure(float time)
        {
            Vector2 lurePreviousPos = _lurePosition;

            // update the position of the lure
            _lureVelocity = _lureVelocity * _lureFriction + _lureAcceleration * time;
            _lurePosition = _lurePosition + _lureVelocity * time;

            // constrain the lure to the length of the line
            Vector2 tipPosition = GetRodTipPosition();
            Vector2 tipToLure = _lurePosition - tipPosition;
            if (tipToLure.LengthSquared() > _lineLength * _lineLength)
            {
                float scaler = _lineLength / tipToLure.Length();
                _lurePosition = tipPosition + tipToLure * scaler;
            }

            // track the velocity (in pixels per second)
            _lureVelocity = (_lurePosition - lurePreviousPos) / time;
        }

        /// <summary>
        /// Returns the position of the tip of the fishing rod.
        /// </summary>
        private Vector2 GetRodTipPosition()
        {
            Sprite rod = RodSprites[Rod];
            Vector2 tip = new Vector2(199, 10) - new Vector2(16, 11);
            tip = Vector2.Transform(tip, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -RodRotation));
            tip = tip + rod.Position;
            return tip;
        }

        /// <summary>
        /// Returns the maximum swing angle for the specified type of rod.
        /// </summary>
        private float GetSwingSweep(RodType rod)
        {
            switch (rod)
            {
                case RodType.Bronze: return MathHelper.Pi / 3f;
                case RodType.Silver: return MathHelper.Pi / 2f;
                case RodType.Gold: return MathHelper.Pi / 1.4f;
                case RodType.Legendary: return MathHelper.Pi / 1.25f;
                default: return 0f;
            }
        }

        /// <summary>
        /// Returns the acceleration of the line being reeled in.
        /// </summary>
        private float GetReelAcceleration()
        {
            return (_hookedFish != null) ? 0f // no acceleration when hooked
                    : (_lureBroken)
                        ? ReelAccelerationBroken
                        : ReelAccelerationLure;
        }

        /// <summary>
        /// Sets the current action.
        /// </summary>
        /// <param name="action">The new action.</param>
        private void OnActionChanged(FishingAction action)
        {
            _action = action;
            if (ActionChanged != null)
            {
                ActionChanged(this, new FishingActionEventArgs(_action));
            }
        }

        /// <summary>
        /// Indicates a lure event occurred.
        /// </summary>
        /// <param name="evt">The event type.</param>
        private void OnFishingEvent(FishingEvent evt)
        {
            if (Event != null)
            {
                Event(this, new FishingEventArgs(evt, _hookedFish));
            }
        }

        /// <summary>
        /// Updates the fishing rod for the current frame. 
        /// </summary>
        /// <param name="elapsed">The elapsed time, in seconds, since the last tick.</param>
        /// <param name="input">The input state for the current frame.</param>
        private delegate void StateTick(float elapsed, Input input);


        private StateTick _stateTick;
        private FishingAction _action;

        private float _lineLength;

        private int _lureIndex;
        private Vector2 _lurePosition;
        private Vector2 _lureVelocity;
        private float _lureFriction;
        private Vector2 _lureAcceleration;
        private bool _lureBroken;
        private float _reelSpeed;

        private Fish _hookedFish;

        private GameplayScreen _game;
        private Scene _scene;

        private const float IdleSpeed = 1.0f;
        private const float IdleLineLength = 30f;
        private const float SwingSpeed = 2f;
        private const float SwingInitialRotation = (float)(Math.PI / 8);
        private const float CastingSpeed = 2f;
        private const float CastingMinDistance = 200f;
        private const float CastingMaxDistance = 2000f;
        private const float CastingMaxSweep = MathHelper.Pi / 1.25f;
        private readonly Vector2 CastingMaxVelocity = new Vector2(1100, -600);
        private const float ReelSpeed = 150f;
        private const float ReelAccelerationLure = 100f;
        private const float ReelAccelerationBroken = 250f;

        private const float AirFriction = 1f;
        private readonly Vector2 AirGravity = new Vector2(0, 500);
        private const float WaterFriction = 0.9f;
        private readonly Vector2 WaterGravity = new Vector2(0, 500);
    }

    /// <summary>
    /// Event data for the action changed event.
    /// </summary>
    public sealed class FishingActionEventArgs : EventArgs
    {
        /// <summary>
        /// The new action.
        /// </summary>
        public readonly FishingAction Action;

        /// <summary>
        /// Creates a new action event.
        /// </summary>
        public FishingActionEventArgs(FishingAction action)
        {
            Action = action;
        }
    }

    /// <summary>
    /// Event data for the fishing event.
    /// </summary>
    public sealed class FishingEventArgs : EventArgs
    {
        /// <summary>
        /// The event.
        /// </summary>
        public readonly FishingEvent Event;

        /// <summary>
        /// The fish.
        /// </summary>
        public readonly Fish Fish;

        /// <summary>
        /// Creates a new fish event.
        /// </summary>
        /// <param name="evt">The event type.</param>
        /// <param name="fish">The fish.</param>
        public FishingEventArgs(FishingEvent evt, Fish fish)
        {
            Event = evt;
            Fish = fish;
        }
    }
}