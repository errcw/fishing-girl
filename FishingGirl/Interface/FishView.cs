using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Draws fish.
    /// </summary>
    public class FishView
    {
        /// <summary>
        /// Creates a new fish view.
        /// </summary>
        /// <param name="fish">The fish to draw.</param>
        public FishView(Fish fish)
        {
            _fish = fish;
            _sprite = _fish.Sprite.Sprite;
            _swimAnimation = _fish.Sprite.GetAnimation("Swim");
        }

        /// <summary>
        /// Updates the view of the fish.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        /// <returns>True if this fish is still visible; otherwise, false.</returns>
        public void Update(float time)
        {
            _sprite.Position = _fish.Position;
            switch (_fish.State)
            {
                case FishState.ChasingLure: goto case FishState.Swimming;
                case FishState.Swimming:
                    UpdateSwimming(time);
                    break;

                case FishState.Hooked:
                    UpdateHooked(time);
                    break;
            }

            if (_fish.State == FishState.ChasingLure && _previousState == FishState.Swimming)
            {
                OnChaseStart();
            }
            else if (_fish.State == FishState.Swimming && _previousState == FishState.ChasingLure)
            {
                OnChaseEnd();
            }
            else if (_fish.State == FishState.Hooked && _previousState != FishState.Hooked)
            {
                OnHook();
            }
            _previousState = _fish.State;

            UpdateAnimations(time);
        }

        /// <summary>
        /// Draws the fish.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Updates the fish when it is swiming or chasing.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateSwimming(float time)
        {
            _sprite.Rotation = GetRotation();
            _sprite.Scale = GetScale();
        }

        /// <summary>
        /// Updates the fish when it is hooked.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateHooked(float time)
        {
            if (_sprite.Origin.X != 0f)
            {
                float prevOriginX = _sprite.Origin.X;

                float rot = HookedFallSpeed * time;
                _sprite.Rotation -= rot;
                _sprite.Origin = Vector2.Transform(_sprite.Origin,
                    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (_sprite.Scale.X > 0) ? rot : -rot));

                if (_sprite.Origin.X > 0f && prevOriginX < 0f ||
                    _sprite.Origin.X < 0f && prevOriginX > 0f)
                {
                    _sprite.Origin = new Vector2(0f, _sprite.Origin.Y);
                }
            }

        }

        /// <summary>
        /// Updates the animations managed by this fish.
        /// </summary>
        private void UpdateAnimations(float time)
        {
            if (_fish.State == FishState.Swimming || _fish.State == FishState.ChasingLure)
            {
                _swimAnimation.Update(_fish.State == FishState.ChasingLure ? time * 2 : time);
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
        /// Returns a rotation that indicates the current velocity of the fish.
        /// </summary>
        private float GetRotation()
        {
            Vector2 facing = Vector2.Normalize(_fish.Velocity);
            double angle = Math.Acos(Math.Abs(facing.X));
            if (facing.Y < 0)
            {
                angle = -angle;
            }
            return (float)angle;
        }

        /// <summary>
        /// Returns a scale that indicates the current velocity of the fish.
        /// </summary>
        private Vector2 GetScale()
        {
            float scaleX = 1f;
            float speed = _fish.Velocity.Length();
            if (speed < _fish.Movement.Speed)
            {
                scaleX = Math.Max(speed / _fish.Movement.Speed, MinScale);
            }
            if (_fish.Velocity.X > 0)
            {
                scaleX = -scaleX;
            }
            return new Vector2(scaleX, 1f);
        }

        /// <summary>
        /// Occurs when the fish starts chasing the lure.
        /// </summary>
        private void OnChaseStart()
        {
            OpenMouth();
        }

        /// <summary>
        /// Occurs when the fish stops chasing the lure.
        /// </summary>
        private void OnChaseEnd()
        {
            CloseMouth();
        }

        /// <summary>
        /// Occurs when the fish is hooked on the lure.
        /// </summary>
        private void OnHook()
        {
            _sprite.Scale = new Vector2(Math.Sign(_sprite.Scale.X), 1f);
            _sprite.Origin = _sprite.Position - _fish.Fishing.LurePosition;
            _sprite.Position = _fish.Fishing.LurePosition;
            CloseMouth();
        }

        /// <summary>
        /// Starts the mouth open animation.
        /// </summary>
        private void OpenMouth()
        {
            _mouthAnimation = _fish.Sprite.GetAnimation("MouthOpen");
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
            IAnimation closeAnimation = _fish.Sprite.GetAnimation("MouthClose");
            if (closeAnimation != null)
            {
                if (_mouthAnimation != null)
                {
                    // let the mouth open before closing it again
                    _mouthAnimation = new SequentialAnimation(_mouthAnimation, closeAnimation);
                }
                else
                {
                    _mouthAnimation = closeAnimation;
                    _mouthAnimation.Start();
                }
            }
        }

        private Fish _fish;
        private FishState _previousState;

        private Sprite _sprite;
        private IAnimation _swimAnimation;
        private IAnimation _mouthAnimation;

        private const float MinScale = 0.2f;
        private const float HookedFallSpeed = 3f;
    }
}
