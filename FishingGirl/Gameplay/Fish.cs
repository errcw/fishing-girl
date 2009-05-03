using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Sprite;
using Library.Sprite.Pipeline;
using Library.Animation;
using Library.Extensions;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// A fish.
    /// </summary>
    public class Fish
    {
        /// <summary>
        /// A description of this fish.
        /// </summary>
        public FishDescription Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// The movement of this fish.
        /// </summary>
        public FishMovement Movement
        {
            get
            {
                return _movement;
            }
        }

        /// <summary>
        /// The sprite used to draw this fish.
        /// </summary>
        public Sprite Sprite
        {
            get
            {
                return _sprite;
            }
        }

        /// <summary>
        /// Creates a new fish.
        /// </summary>
        public Fish(FishDescription description, FishMovement movement, FishBehavior behavior, FishingState fishing)
        {
            _description = description;
            _movement = movement;
            _behavior = behavior;
            _fishing = fishing;
        }

        /// <summary>
        /// Loads all the content for this fish.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _fishDescriptor = content.Load<SpriteDescriptorTemplate>(_description.DescriptorName).Create(content);
            _sprite = (CompositeSprite)_fishDescriptor.Sprite;

            _mouthOffset = _fishDescriptor.GetSprite("Body").Size.X / 2f;
            _swimAnimation = _fishDescriptor.GetAnimation("Swim");

            _sprite.Position = new Vector2(700f, _movement.Range.Y);
            _velocity = _targetVelocity = new Vector2(_movement.Speed, 0f);
        }

        /// <summary>
        /// Updates this fish.
        /// </summary>
        /// <param name="time">The time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            UpdateAnimations(time);
            if (_hooked)
            {
                UpdateHooked(time);
            }
            else
            {
                bool wasChasingLure = _chasingLure;
                _chasingLure = IsLureVisible(_fishing.LurePosition);
                if (_chasingLure)
                {
                    if (!wasChasingLure)
                    {
                        OnChaseStart();
                    }
                    UpdateChase(time);
                }
                else
                {
                    if (wasChasingLure)
                    {
                        OnChaseEnd();
                    }
                    UpdateIdle(time);
                }

                UpdatePosition(time);
                _sprite.Rotation = GetRotation();
                _sprite.Scale = GetScale();
            }
        }

        /// <summary>
        /// Draws this fish.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Updates the animations managed by this fish.
        /// </summary>
        private void UpdateAnimations(float time)
        {
            if (!_hooked)
            {
                _swimAnimation.Update(_chasingLure ? time * 2 : time);
            }
            if (_mouthAnimation != null)
            {
                if (!_mouthAnimation.Update(time))
                {
                    _mouthAnimation = null;
                }
            }
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

            _sprite.Position += _velocity * time;
        }

        /// <summary>
        /// Updates this fish when no lure is in sight.
        /// </summary>
        private void UpdateIdle(float time)
        {
            Vector2 direction = new Vector2(_velocity.X > 0 ? 1f : -1f, 0f);
            if (_sprite.Position.X < _movement.Range.X)
            {
                direction.X = 1f;
            }
            else if (_sprite.Position.X > _movement.Range.Z)
            {
                direction.X = -1f;
            }
            if (_sprite.Position.Y < _movement.Range.Y)
            {
                direction.Y = 1f;
            }
            else if (_sprite.Position.Y > _movement.Range.W)
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
            _chasingLure = true;

            Vector2 toLure = _fishing.LurePosition - GetMouthPosition();
            if (toLure.Length() <= LureBiteDistance)
            {
                if (_fishing.BiteLure(this))
                {
                    OnHooked();
                }
            }
            else
            {
                Vector2 v = Vector2.Normalize(_fishing.LurePosition - _sprite.Position);
                v *= (_movement.Speed * _behavior.LungeSpeedMultiplier);
                _targetVelocity = v;
            }
        }

        /// <summary>
        /// Updates this fish when it is hooked on a lure.
        /// </summary>
        private void UpdateHooked(float time)
        {
            _sprite.Position = _fishing.LurePosition;

            if (!MathHelperExtensions.EpsilonEquals(_sprite.Origin.X, 0f, 2f))
            {
                float rot = HookedFallSpeed * time;
                _sprite.Rotation -= rot;
                _sprite.Origin = Vector2.Transform(_sprite.Origin,
                    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (_sprite.Scale.X > 0) ? rot : -rot));
            }
        }

        /// <summary>
        /// Indicates that this fish has started chasing a lure.
        /// </summary>
        private void OnChaseStart()
        {
            OpenMouth();
        }

        /// <summary>
        /// Indicates this fish has stopped chasing a lure.
        /// </summary>
        private void OnChaseEnd()
        {
            CloseMouth();
        }

        /// <summary>
        /// Indicates this fish has been hooked by a lure.
        /// </summary>
        private void OnHooked()
        {
            _hooked = true;

            _sprite.Scale = new Vector2(Math.Sign(_sprite.Scale.X), 1f);
            _sprite.Origin = _sprite.Position - _fishing.LurePosition;
            _sprite.Position = _fishing.LurePosition;

            CloseMouth();
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
            Vector2 mouthPosition = _sprite.Position + _mouthOffset * facing;
            return mouthPosition;
        }

        /// <summary>
        /// Returns a unit vector pointing in the direction this fish is facing.
        /// </summary>
        private Vector2 GetFacing()
        {
            return Vector2.Normalize(_velocity);
        }

        /// <summary>
        /// Returns a rotation that indicates the current velocity of this fish.
        /// </summary>
        private float GetRotation()
        {
            Vector2 facing = Vector2.Normalize(_velocity);
            double angle = Math.Acos(Math.Abs(facing.X));
            if (facing.Y < 0)
            {
                angle = -angle;
            }
            return (float)angle;
        }

        /// <summary>
        /// Returns a scale that indicates the current velocity of this fish.
        /// </summary>
        private Vector2 GetScale()
        {
            float scaleX = 1f;
            float speed = _velocity.Length();
            if (speed < _movement.Speed)
            {
                scaleX = Math.Max(speed / _movement.Speed, MinScale);
            }
            if (_velocity.X > 0)
            {
                scaleX = -scaleX;
            }
            return new Vector2(scaleX, 1f);
        }

        /// <summary>
        /// Starts the mouth open animation.
        /// </summary>
        private void OpenMouth()
        {
            _mouthAnimation = _fishDescriptor.GetAnimation("MouthOpen");
            if (_mouthAnimation != null)
            {
                _mouthAnimation.Start();
            }
        }

        /// <summary>
        /// Starts the mouth closed animation.
        /// </summary>
        private void CloseMouth()
        {
            IAnimation closeAnimation = _fishDescriptor.GetAnimation("MouthClose");
            if (closeAnimation != null)
            {
                closeAnimation.Start();
                if (_mouthAnimation != null)
                {
                    _mouthAnimation = new SequentialAnimation(_mouthAnimation, closeAnimation);
                }
                else
                {
                    _mouthAnimation = closeAnimation;
                }
            }
        }

        private Vector2 _velocity;
        private Vector2 _targetVelocity;

        private bool _chasingLure;
        private bool _hooked;

        private SpriteDescriptor _fishDescriptor;
        private Sprite _sprite;

        private float _mouthOffset;
        private IAnimation _swimAnimation;
        private IAnimation _mouthAnimation;

        private FishDescription _description;
        private FishMovement _movement;
        private FishBehavior _behavior;

        private FishingState _fishing;

        private const float LureSightDistance = 150f;
        private const float LureSightAngle = (float)(Math.PI / 6);
        private const float LureBiteDistance = 5f;

        private const float HookedFallSpeed = 3f;
        private const float MinScale = 0.2f;
    }
}
