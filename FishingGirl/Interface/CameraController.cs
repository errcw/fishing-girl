﻿using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Library.Sprite;
using Library.Animation;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Controls the camera.
    /// </summary>
    public class CameraController
    {
        /// <summary>
        /// Creates a new camera controller.
        /// </summary>
        /// <param name="camera">The camera to control.</param>
        /// <param name="fishing">The fishing state to monitor.</param>
        public CameraController(CameraSprite camera, FishingState fishing)
        {
            _camera = camera;
            _camera.Position = InitialPosition;
            SetCameraFocus(InitialFocus);

            _fishing = fishing;
            _fishing.ActionChanged += OnActionChanged;
            _fishing.Event += OnFishingEvent;
        }

        /// <summary>
        /// Updates position of the camera.
        /// </summary>
        public void Update(float time)
        {
            if (_followLure)
            {
                SetCameraFocus(_fishing.LurePosition);
            }
            if (_cameraAnimation != null)
            {
                if (!_cameraAnimation.Update(time))
                {
                    _cameraAnimation = null;
                }
            }
            UpdateDebugControls();
        }

        /// <summary>
        /// Centers the camera on the given position.
        /// </summary>
        private void SetCameraFocus(Vector2 position)
        {
            _cameraAnimation = new PositionAnimation(
                _camera,
                GetFocusPosition(position),
                1f,
                Interpolation.InterpolateVector2(Easing.QuadraticOut));
        }

        /// <summary>
        /// Gets the camera position necessary to focus on the given centre.
        /// </summary
        private Vector2 GetFocusPosition(Vector2 centrePosition)
        {
            return new Vector2(
               centrePosition.X - _camera.Size.X / 2,
               centrePosition.Y - _camera.Size.Y / 2);
        }

        /// <summary>
        /// Checks the action for the lure hitting the water.
        /// </summary>
        private void OnActionChanged(object stateObj, FishingActionEventArgs e)
        {
            if (e.Action == FishingAction.Idle)
            {
                _followLure = false;
            }
            else if (e.Action == FishingAction.PullBack)
            {
                if (_camera.Scale != Vector2.One)
                {
                    // undo the caught transform if necessary
                    _cameraAnimation = new CompositeAnimation(
                        new PositionAnimation(_camera, GetFocusPosition(SwingFocus), CatchScaleTime, Interpolation.InterpolateVector2(Easing.QuadraticOut)),
                        new ScaleAnimation(_camera, Vector2.One, CatchScaleTime, Interpolation.InterpolateVector2(Easing.QuadraticOut)));
                }
            }
            else if (e.Action == FishingAction.Cast)
            {
                _followLure = true;
            }
        }

        /// <summary>
        /// Zooms in on caught fish.
        /// </summary>
        private void OnFishingEvent(object stateObj, FishingEventArgs e)
        {
            if (e.Event == FishingEvent.FishCaught)
            {
                FishingState fishingState = (FishingState)stateObj;
                _cameraAnimation = new SequentialAnimation(
                    new CompositeAnimation(
                        new PositionAnimation(_camera, GetFocusPosition(CatchFocus), CatchScaleTime, Interpolation.InterpolateVector2(Easing.QuadraticOut)),
                        new ScaleAnimation(_camera, CatchScale, CatchScaleTime, Interpolation.InterpolateVector2(Easing.QuadraticOut))),
                    new DelayAnimation(CatchDelayTime),
                    new CompositeAnimation(
                        new PositionAnimation(_camera, GetFocusPosition(SwingFocus), CatchScaleTime, Interpolation.InterpolateVector2(Easing.QuadraticOut)),
                        new ScaleAnimation(_camera, Vector2.One, CatchScaleTime, Interpolation.InterpolateVector2(Easing.QuadraticOut))));
            }
        }

        /// <summary>
        /// Implements debug camera controls.
        /// </summary>
        private void UpdateDebugControls()
        {
#if DEBUG
            const float CAMINC = 15f;
            KeyboardState keys = Keyboard.GetState();
            GamePadState pad = GamePad.GetState(PlayerIndex.One);
            if (keys.IsKeyDown(Keys.Right) || pad.DPad.Right == ButtonState.Pressed)
            {
                _camera.Position = new Vector2(_camera.Position.X + CAMINC, _camera.Position.Y);
            }
            else if (keys.IsKeyDown(Keys.Left) || pad.DPad.Left == ButtonState.Pressed)
            {
                _camera.Position = new Vector2(_camera.Position.X - CAMINC, _camera.Position.Y);
            }
            if (keys.IsKeyDown(Keys.Up) || pad.DPad.Up == ButtonState.Pressed)
            {
                _camera.Position = new Vector2(_camera.Position.X, _camera.Position.Y - CAMINC);
            }
            else if (keys.IsKeyDown(Keys.Down) || pad.DPad.Down == ButtonState.Pressed)
            {
                _camera.Position = new Vector2(_camera.Position.X, _camera.Position.Y + CAMINC);
            }
#endif
        }

        private FishingState _fishing;

        private CameraSprite _camera;
        private IAnimation _cameraAnimation;
        private bool _followLure;

        private readonly Vector2 InitialPosition = new Vector2(100, 150);
        private readonly Vector2 InitialFocus = new Vector2(740, 510);
        private readonly Vector2 SwingFocus = new Vector2(900, 540);
        private readonly Vector2 CatchFocus = new Vector2(1200, 750);
        private readonly Vector2 CatchScale = new Vector2(1.5f, 1.5f);
        private const float CatchScaleTime = 1f;
        private const float CatchDelayTime = 3f;
    }
}
