﻿using System;

using Microsoft.Xna.Framework.Input;

using Library.Input;

namespace FishingGirl
{
    /// <summary>
    /// The current input state.
    /// </summary>
    public class Input : Library.Input.Input
    {
        public readonly ControlState Action = new ControlState();

        public readonly ControlState Cancel = new ControlState();

        public readonly ControlState Up = new ControlState();

        public readonly ControlState Down = new ControlState();

        /// <summary>
        /// Registers the actions.
        /// </summary>
        public Input()
        {
            Register(Action, Polling.Any(Polling.One(Buttons.A), Polling.One(Buttons.Start)));
            Register(Cancel, Polling.Any(Polling.One(Buttons.B), Polling.One(Buttons.Back)));
            Register(Up, Polling.Any(Polling.One(Buttons.DPadUp), Polling.One(Buttons.LeftThumbstickUp)));
            Register(Down, Polling.Any(Polling.One(Buttons.DPadDown), Polling.One(Buttons.LeftThumbstickDown)));
        }
    }
}
