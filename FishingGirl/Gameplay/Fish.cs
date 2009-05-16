using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Sprite;
using Library.Sprite.Pipeline;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// The state of a fish.
    /// </summary>
    public enum FishState
    {
        Swimming,
        ChasingLure,
        Hooked,
        Eaten,
        Caught
    }

    /// <summary>
    /// A fish.
    /// </summary>
    public class Fish
    {
        /// <summary>
        /// A description of this fish.
        /// </summary>
        public FishDescription Description { get { return _description; } }

        /// <summary>
        /// The movement of this fish.
        /// </summary>
        public FishMovement Movement { get { return _movement; } }

        /// <summary>
        /// The position of this fish.
        /// </summary>
        public Vector2 Position { get { return _position; } }

        /// <summary>
        /// The velocity of this fish.
        /// </summary>
        public Vector2 Velocity { get { return _velocity; } }

        /// <summary>
        /// The current action of this fish.
        /// </summary>
        public FishState State { get { return _state; } }

        /// <summary>
        /// The sprite used to draw this fish.
        /// </summary>
        public SpriteDescriptor Sprite { get { return _fishDescriptor; } }

        /// <summary>
        /// The fishing state to which this fish is bound.
        /// </summary>
        internal FishingState Fishing { get { return _fishing; } }

        /// <summary>
        /// Creates a new fish.
        /// </summary>
        public Fish(FishDescription description, FishMovement movement, FishBehavior behavior, FishingState fishing)
        {
            _description = description;
            _movement = movement;
            _behavior = behavior;
            _fishing = fishing;

            _position = new Vector2(700f, _movement.Range.Y);
            _velocity = _targetVelocity = new Vector2(_movement.Speed, 0f);
        }

        /// <summary>
        /// Loads all the content for this fish.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _fishDescriptor = content.Load<SpriteDescriptorTemplate>(_description.DescriptorName).Create();
            _mouthOffset = _fishDescriptor.GetSprite("Body").Size.X / 2f;
        }

        /// <summary>
        /// Updates this fish.
        /// </summary>
        /// <param name="time">The time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            switch (_state)
            {
                case FishState.Swimming:
                    UpdateSwim(time);
                    break;

                case FishState.ChasingLure:
                    UpdateChase(time);
                    break;

                case FishState.Eaten: goto case FishState.Hooked;
                case FishState.Caught: goto case FishState.Hooked;
                case FishState.Hooked:
                    UpdateHooked(time);
                    break;
            }
        }

        /// <summary>
        /// Occurs when this fish is eaten.
        /// </summary>
        public void OnEaten()
        {
            _state = FishState.Eaten;
        }

        /// <summary>
        /// Occurs when this fish is caught.
        /// </summary>
        public void OnCaught()
        {
            _state = FishState.Caught;
        }

        /// <summary>
        /// Updates the acceleration, velocity, and position of this fish.
        /// </summary>
        private void UpdatePosition(float time)
        {
            if (_velocity != _targetVelocity)
            {
                Vector2 diff = _targetVelocity - _velocity;
                diff = new Vector2(Math.Sign(diff.X), Math.Sign(diff.Y));

                _velocity += _movement.Acceleration * time * diff;
                if ((_velocity.X > _targetVelocity.X && diff.X > 0) ||
                    (_velocity.X < _targetVelocity.X && diff.X < 0))
                {
                    _velocity.X = _targetVelocity.X;
                }
                if ((_velocity.Y > _targetVelocity.Y && diff.Y > 0) ||
                    (_velocity.Y < _targetVelocity.Y && diff.Y < 0))
                {
                    _velocity.Y = _targetVelocity.Y;
                }
            }

            _position += _velocity * time;
        }

        /// <summary>
        /// Updates this fish when no lure is in sight.
        /// </summary>
        private void UpdateSwim(float time)
        {
            UpdatePosition(time);

            if (IsLureVisible(_fishing.LurePosition))
            {
                _state = FishState.ChasingLure;
                return;
            }

            Vector2 direction = new Vector2(_velocity.X > 0 ? 1f : -1f, 0f);
            if (_position.X < _movement.Range.X)
            {
                direction.X = 1f;
            }
            else if (_position.X > _movement.Range.Z)
            {
                direction.X = -1f;
            }
            if (_position.Y < _movement.Range.Y)
            {
                direction.Y = 1f;
            }
            else if (_position.Y > _movement.Range.W)
            {
                direction.Y = -1f;
            }
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                _targetVelocity = direction * _movement.Speed;
            }
        }

        /// <summary>
        /// Updates this fish when it is chasing a lure.
        /// </summary>
        private void UpdateChase(float time)
        {
            UpdatePosition(time);

            if (!IsLureVisible(_fishing.LurePosition))
            {
                _state = FishState.Swimming;
                return;
            }

            Vector2 toLure = _fishing.LurePosition - GetMouthPosition();
            if (toLure.Length() <= LureBiteDistance)
            {
                if (_fishing.BiteLure(this))
                {
                    _state = FishState.Hooked;
                    return;
                }
            }
            else
            {
                Vector2 v = Vector2.Normalize(_fishing.LurePosition - _position);
                v *= (_movement.Speed * _behavior.LungeSpeedMultiplier);
                _targetVelocity = v;
            }
        }

        /// <summary>
        /// Updates this fish when it is hooked on a lure.
        /// </summary>
        private void UpdateHooked(float time)
        {
            _position = _fishing.LurePosition;
        }

        /// <summary>
        /// Checks if the lure is visible to this fish.
        /// </summary>
        /// <param name="lurePosition">The current position of the lure.</param>
        private bool IsLureVisible(Vector2 lurePosition)
        {
            if (_fishing.IsAttractedToLure(this))
            {
                Vector2 toLure = lurePosition - GetMouthPosition();
                if (toLure.LengthSquared() <= _behavior.LureSightDistance * _behavior.LureSightDistance)
                {
                    float dot = Math.Min(Vector2.Dot(GetFacing(), Vector2.Normalize(toLure)), 1f);
                    float angle = (float)Math.Acos(dot);
                    if (angle <= _behavior.LureSightAngle)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the position of the mouth of this fish.
        /// </summary>
        private Vector2 GetMouthPosition()
        {
            Vector2 facing = GetFacing();
            Vector2 mouthPosition = _position + _mouthOffset * facing;
            return mouthPosition;
        }

        /// <summary>
        /// Returns a unit vector pointing in the direction this fish is facing.
        /// </summary>
        private Vector2 GetFacing()
        {
            return Vector2.Normalize(_velocity);
        }

        private FishState _state;

        private Vector2 _position;
        private Vector2 _velocity;
        private Vector2 _targetVelocity;

        private SpriteDescriptor _fishDescriptor;
        private float _mouthOffset;

        private FishDescription _description;
        private FishMovement _movement;
        private FishBehavior _behavior;

        private FishingState _fishing;

        private const float LureBiteDistance = 5f;
    }
}
