using System;

using Microsoft.Xna.Framework.Input;

using Library.Input;

namespace FishingGirl
{
    /// <summary>
    /// The current input state.
    /// </summary>
    public class Input : Library.Input.Input
    {
        /// A terrible hack to get global vibration enable/disable
        private static Library.Input.Input _instance;
        public static bool VibrationEnabled
        {
            get { return _instance.VibrationEnabled; }
            set { _instance.VibrationEnabled = value; }
        }

        public readonly ControlState Action = new ControlState();
        public readonly ControlState AltAction = new ControlState();
        public readonly ControlState Cancel = new ControlState();
        public readonly ControlState Up = new ControlState();
        public readonly ControlState Down = new ControlState();
        public readonly ControlState Start = new ControlState();

        /// <summary>
        /// Registers the actions.
        /// </summary>
        public Input(FishingGame game) : base(game)
        {
            Register(Action, Polling.Any(Polling.One(Buttons.A), Polling.One(Buttons.Start)));
            Register(AltAction, Polling.One(Buttons.B));
            Register(Cancel, Polling.Any(Polling.One(Buttons.B), Polling.One(Buttons.Back)));
            Register(Up, Polling.Any(Polling.One(Buttons.DPadUp), Polling.One(Buttons.LeftThumbstickUp)));
            Register(Down, Polling.Any(Polling.One(Buttons.DPadDown), Polling.One(Buttons.LeftThumbstickDown)));
            Register(Start, Polling.One(Buttons.Start));

            _instance = this;
        }

        /// <summary>
        /// Polls for the controller with the A button pressed.
        /// </summary>
        /// <returns>True if a controller was found; otherwise, false.</returns>
        public bool FindActiveController()
        {
            return FindActiveController(Polling.One(Buttons.A));
        }
    }
}
