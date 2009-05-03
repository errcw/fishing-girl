using System;

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
        PullBack,
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
        LureBroke
    }

    /// <summary>
    /// The type of lure.
    /// </summary>
    public enum LureType
    {
        Small,
        Medium,
        Large,
        Bomb
    }

    /// <summary>
    /// The type of rod.
    /// </summary>
    public enum RodType
    {
        Bronze,
        Silver,
        Gold,
        Legendary
    }

    /// <summary>
    /// Event data for the action changed event.
    /// </summary>
    public class FishingActionEventArgs : EventArgs
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
    public class FishingEventArgs : EventArgs
    {
        /// <summary>
        /// The event.
        /// </summary>
        public readonly FishingEvent Event;

        /// <summary>
        /// The hooked fish on the lure.
        /// </summary>
        public readonly Fish Fish;

        /// <summary>
        /// Creates a new lure event.
        /// </summary>
        /// <param name="evt">The event type.</param>
        /// <param name="fish">The hooked fish.</param>
        public FishingEventArgs(FishingEvent evt, Fish fish)
        {
            Event = evt;
            Fish = fish;
        }
    }

    /// <summary>
    /// Describes the fishing game mechanic.
    /// </summary>
    public class FishingState
    {
        /// <summary>
        /// An event that fires when the current fishing action changes.
        /// </summary>
        public event EventHandler<FishingActionEventArgs> ActionChanged;

        /// <summary>
        /// An event that fires when a lure event occurs.
        /// </summary>
        public event EventHandler<FishingEventArgs> Event;

        /// <summary>
        /// The current fishing action.
        /// </summary>
        public FishingAction Action
        {
            get
            {
                return _action;
            }
        }

        /// <summary>
        /// The position of the lure.
        /// </summary>
        public Vector2 LurePosition
        {
            get
            {
                return _lure.Position;
            }
        }

        /// <summary>
        /// The type of rod used to fish.
        /// </summary>
        public RodType Rod
        {
            set
            {
                _rodType = value;
            }
        }

        /// <summary>
        /// Creates a new fishing state.
        /// </summary>
        /// <param name="game">The game context.</param>
        /// <param name="scene">The context of the state.</param>
        public FishingState(GameplayScreen game, Scene scene)
        {
            _game = game;
            _scene = scene;
        }

        /// <summary>
        /// Loads the content for the fishing rod.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _rods = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                RodType rod = (RodType)i;
                _rods[i] = content.Load<ImageSpriteTemplate>("Rod" + rod.ToString()).Create();
                _rods[i].Origin = new Vector2(16, 11);
                _rods[i].Position = _scene.PlayerPosition + new Vector2(35f, 18f);
                _rods[i].Rotation = SwingInitialRotation;
                _rods[i].Layer = 0.55f;
            }
            _rod = _rods[0];

            _lure = content.Load<ImageSpriteTemplate>("LureSmall").Create();
            _lure.Position = GetRodTipPosition() + new Vector2(5f, 15f);
            _lure.Origin = new Vector2(16f, 16f);
            _lure.Layer = 0.56f;

            _line = content.Load<ImageSpriteTemplate>("Colourable").Create();
            _line.Color = new Color(69, 77, 89);
            _line.Layer = 0.55f;

            // once the rod is fully initialized we can set the inital state
            EnterIdleState();
        }

        /// <summary>
        /// Updates the state of this fishing rod.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        /// <param name="input">The current input state.</param>
        public void Update(float time, Input input)
        {
            _input = input; // needed to set feedback
            _stateTick(time, input);
            UpdateLure(time);
            UpdateLine();
        }

        /// <summary>
        /// Draws this fishing rod.
        /// </summary>
        /// <param name="spriteBatch">The batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _rod.Draw(spriteBatch);
            _line.Draw(spriteBatch);
            _lure.Draw(spriteBatch);
        }

        /// <summary>
        /// Returns true if the given fish should chase the lure; otherwise, false.
        /// </summary>
        public bool IsAttractedToLure(Fish fish)
        {
            if (_lureBroken || _lure.Position.Y <= _scene.WaterLevel)
            {
                return false;
            }
            return (_hookedFish == null) || (fish.Description.Size > _hookedFish.Description.Size);
        }

        /// <summary>
        /// Bites the lure with the given fish.
        /// </summary>
        /// <param name="fish">The biting fish.</param>
        /// <returns>True if the bite hooked the fish; otherwise, false.</returns>
        public bool BiteLure(Fish fish)
        {
            //_input.SetVibration(GetLureFeedback(fish), 0.35f);
            if (_hookedFish == null)
            {
                if ((int)fish.Description.Size <= (int)_lureType)
                {
                    _hookedFish = fish;
                    OnFishingEvent(FishingEvent.FishHooked);
                    return true;
                }
                else
                {
                    SetLureBroken(true);
                    OnFishingEvent(FishingEvent.LureBroke);
                }
            }
            else
            {
                if (fish.Description.Size == _hookedFish.Description.Size + 1 &&
                    fish.Description.Size < FishSize.VeryLarge)
                {
                    OnFishingEvent(FishingEvent.FishEaten);
                    _hookedFish = fish;
                    return true;
                }
                else
                {
                    OnFishingEvent(FishingEvent.FishEaten);
                    _hookedFish = null;

                    SetLureBroken(true);
                    OnFishingEvent(FishingEvent.LureBroke);
                }
            }
            return false;
        }

        /// <summary>
        /// Enters the idle state.
        /// </summary>
        private void EnterIdleState()
        {
            OnActionChanged(FishingAction.Idle);

            _lureFriction = AirFriction;
            _lureAcceleration = AirGravity;
            SetLureBroken(false);

            _rod = _rods[(int)_rodType];

            _stateTick = delegate(float elapsed, Input input)
            {
                if (_rod.Rotation > SwingInitialRotation)
                {
                    _rod.Rotation -= IdleSpeed * elapsed;
                    if (_rod.Rotation < SwingInitialRotation)
                    {
                        _rod.Rotation = SwingInitialRotation;
                    }
                }
                if (input.Action.Pressed && _rod.Rotation <= SwingInitialRotation)
                {
                    EnterSwingingState();
                }
            };
        }

        /// <summary>
        /// Enters the swinging state.
        /// </summary>
        private void EnterSwingingState()
        {
            float swingPosition = 0f;

            OnActionChanged(FishingAction.PullBack);

            _stateTick = delegate(float elapsed, Input input)
            {
                if (input.Action.Down)
                {
                    swingPosition += SwingSpeed * elapsed;
                    if (swingPosition > Math.PI / 2f)
                    {
                        OnActionChanged(FishingAction.Swing);
                    }
                    _rod.Rotation = (float)(Math.Sin(swingPosition) * SwingRotationSweep + SwingInitialRotation);
                    if (_rod.Rotation <= SwingInitialRotation)
                    {
                        _rod.Rotation = SwingInitialRotation;
                        EnterIdleState();
                    }
                }
                else
                {
                    if (swingPosition > Math.PI / 2f)
                    {
                        EnterCastingState();
                    }
                    else
                    {
                        EnterIdleState();
                    }
                }
            };
        }

        /// <summary>
        /// Enters the casting state.
        /// </summary>
        private void EnterCastingState()
        {
            OnActionChanged(FishingAction.Cast);

            _lineLength = float.MaxValue; // let the line spool out
            _lureVelocity *= GetCastingVelocityScale(_rodType);

            _stateTick = delegate(float elapsed, Input input)
            {
                if (_lure.Position.X > _scene.FarShoreX)
                {
                    //TODO _game.EndGame(GameEndReason.CaughtBoy);
                }
                else if (_lure.Position.Y > _scene.WaterLevel)
                {
                    _lineLength = Vector2.Distance(_lure.Position, GetRodTipPosition()); // set the length
                    EnterReelingState();
                }
                if (_rod.Rotation > SwingInitialRotation)
                {
                    _rod.Rotation -= CastingSpeed * elapsed;
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

            _stateTick = delegate(float elapsed, Input input)
            {
                if (input.Action.Down)
                {
                    _lineLength -= ReelSpeed * elapsed;
                    if (_lineLength <= IdleLineLength)
                    {
                        if (_hookedFish != null)
                        {
                            OnFishingEvent(FishingEvent.FishCaught);
                            _hookedFish = null;
                        }
                        _lineLength = IdleLineLength;
                        EnterIdleState();
                    }
                }
            };
        }

        /// <summary>
        /// Updates the position of the lure.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateLure(float time)
        {
            Vector2 lurePreviousPos = _lure.Position;

            // update the position of the lure
            _lureVelocity = _lureVelocity * _lureFriction + _lureAcceleration * time;
            _lure.Position = _lure.Position + _lureVelocity * time;

            // constrain the lure to the length of the line
            Vector2 tipPosition = GetRodTipPosition();
            Vector2 tipToLure = _lure.Position - tipPosition;
            if (tipToLure.LengthSquared() > _lineLength * _lineLength)
            {
                float scaler = _lineLength / tipToLure.Length();
                _lure.Position = tipPosition + tipToLure * scaler;
            }

            // track the velocity (in pixels per second)
            _lureVelocity = (_lure.Position - lurePreviousPos) / time;

            // update the lure animation
            if (_lureAnimation != null)
            {
                if (!_lureAnimation.Update(time))
                {
                    _lureAnimation = null;
                }
            }
        }

        /// <summary>
        /// Sets the line sprite attributes such that it covers the distance.
        /// </summary>
        private void UpdateLine()
        {
            Vector2 tipPosition = GetRodTipPosition();
            Vector2 lurePosition = _lure.Position;
            float angle = (float)Math.Atan2(lurePosition.Y - tipPosition.Y, lurePosition.X - tipPosition.X);
            _line.Position = tipPosition;
            _line.Rotation = -angle;
            _line.Scale = new Vector2(Vector2.Distance(tipPosition, lurePosition), 1f);
        }

        /// <summary>
        /// Returns the position of the tip of the fishing rod.
        /// </summary>
        private Vector2 GetRodTipPosition()
        {
            Vector2 tip = new Vector2(199, 10) - _rod.Origin;
            tip = Vector2.Transform(tip, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -_rod.Rotation));
            tip = tip + _rod.Position;
            return tip;
        }

        /// <summary>
        /// Sets the visibility of the lure, fading it in or out as appropriate.
        /// </summary>
        /// <param name="broken">If the lure is broken.</param>
        private void SetLureBroken(bool broken)
        {
            _lureBroken = broken;
            _lureAnimation = new ColorAnimation(_lure, broken ? Color.TransparentWhite : Color.White, 0.5f, Interpolation.InterpolateColor(Easing.Uniform));
        }

        /// <summary>
        /// Returns the casting velocity scale factor applied by the given rod type.
        /// </summary>
        private float GetCastingVelocityScale(RodType rod)
        {
            switch (rod)
            {
                case RodType.Bronze: return 0.8f;
                case RodType.Silver: return 1f;
                case RodType.Gold: return 1.2f;
                case RodType.Legendary: return 1.35f;
                default: return 1f;
            }
        }

        /// <summary>
        /// Returns the strength of the feedback for a fish biting the lure.
        /// </summary>
        /// <param name="fish"></param>
        private Vector2 GetLureFeedback(Fish fish)
        {
            switch (fish.Description.Size)
            {
                case FishSize.Small: return new Vector2(0f, 0.2f);
                case FishSize.Medium: return new Vector2(0.15f, 0.2f);
                case FishSize.Large: return new Vector2(0.5f, 0.2f);
                case FishSize.VeryLarge: return new Vector2(0.75f, 0.5f);
            }
            return Vector2.Zero;
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

        private float _lineLength = IdleLineLength;
        private Vector2 _lureVelocity = Vector2.Zero;
        private float _lureFriction = 1f;
        private Vector2 _lureAcceleration = new Vector2(0f, 500f);
        private LureType _lureType = LureType.Small;
        private bool _lureBroken;
        private IAnimation _lureAnimation;

        private RodType _rodType = RodType.Bronze;

        private Fish _hookedFish;

        private Sprite _rod;
        private Sprite _lure;
        private Sprite _line;

        private Sprite[] _rods;

        private GameplayScreen _game;
        private Scene _scene;
        private Input _input;

        private const float IdleSpeed = 1.0f;
        private const float IdleLineLength = 30f;
        private const float SwingSpeed = 2.1f;
        private const float SwingInitialRotation = (float)(Math.PI / 8);
        private const float SwingRotationSweep = (float)(Math.PI / 1.5);
        private const float CastingSpeed = 3f;
        private const float CastingVelocityScale = 0.8f;
        private const float ReelSpeed = 150f;

        private const float AirFriction = 1f;
        private readonly Vector2 AirGravity = new Vector2(0, 500);
        private const float WaterFriction = 0.9f;
        private readonly Vector2 WaterGravity = new Vector2(0, 500);
    }
}