using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Library.Storage;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Per-player options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Turns sound effects on or off.
        /// </summary>
        public bool SoundEffectsToggle
        {
            get { return _options.SoundEffectsToggle; }
            set
            {
                _options.SoundEffectsToggle = value;
                SoundEffect.MasterVolume = _options.SoundEffectsToggle ? 1f : 0f;
            }
        }
        
        /// <summary>
        /// Turns music on or off.
        /// </summary>
        public bool MusicToggle
        {
            get { return _options.MusicToggle; }
            set
            {
                _options.MusicToggle = value;
                MediaPlayer.Volume = _options.MusicToggle ? 0.1f : 0f;
            }
        }

        /// <summary>
        /// Turns vibration on or off.
        /// </summary>
        public bool VibrationToggle
        {
            get { return _options.VibrationToggle; }
            set
            {
                _options.VibrationToggle = value;
                Input.VibrationEnabled = _options.VibrationToggle;
            }
        }

        /// <summary>
        /// Creates a new set of options with default values.
        /// </summary>
        public Options()
        {
            _options = new OptionsData();
            SoundEffectsToggle = true;
            MusicToggle = true;
            VibrationToggle = true;
        }

        /// <summary>
        /// Loads these options from storage.
        /// </summary>
        public void Load(Storage storage)
        {
            try
            {
                storage.Load(_storeableOptions);
                _options = _storeableOptions.Data;

                SoundEffectsToggle = _options.SoundEffectsToggle;
                MusicToggle = _options.MusicToggle;
                VibrationToggle = _options.VibrationToggle;
            }
            catch (Exception e)
            {
                // options unavailable, stick with the defaults
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Stores these options.
        /// </summary>
        public void Save(Storage storage)
        {
            try
            {
                _storeableOptions.Data = _options;
                storage.Save(_storeableOptions);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        private OptionsData _options;
        private readonly XmlStoreable<OptionsData> _storeableOptions = new XmlStoreable<OptionsData>("Options");
    }

    /// <summary>
    /// The saveable options data.
    /// </summary>
    public struct OptionsData
    {
        public bool SoundEffectsToggle;
        public bool MusicToggle;
        public bool VibrationToggle;
    }
}
