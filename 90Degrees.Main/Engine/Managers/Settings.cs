using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Text;
using IndependentResolutionRendering;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using MP3Player;
using raycaster;

namespace Degrees.Main.Engine.Managers
{
    internal class Settings
    {
        private static readonly Configuration mConfig;

        static Settings()
        {
            mConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
        public static void Load()
        {
            if (mConfig.AppSettings.Settings.Count == 0)
            {
                MouseSensitivity = 0.50f;
                Fullscreen = false;
                KeyboardOnly = true;
                MusicVolume = 100;
                SfxVolume = 100;
                CreateConfiguration();
            }
            else
            {
                Fullscreen = bool.Parse(mConfig.AppSettings.Settings["Fullscreen"].Value);
                KeyboardOnly = bool.Parse(mConfig.AppSettings.Settings["KeyboardOnly"].Value);
                MouseSensitivity = float.Parse(mConfig.AppSettings.Settings["MouseSensitivity"].Value, CultureInfo.InvariantCulture);
                MusicVolume = int.Parse(mConfig.AppSettings.Settings["MusicVolume"].Value);
                SfxVolume = int.Parse(mConfig.AppSettings.Settings["SfxVolume"].Value);
            }
        }

        private static void CreateConfiguration()
        {
            mConfig.AppSettings.Settings.Add("Fullscreen", Fullscreen.ToString());
            mConfig.AppSettings.Settings.Add("KeyboardOnly", KeyboardOnly.ToString());
            mConfig.AppSettings.Settings.Add("MouseSensitivity", MouseSensitivity.ToString(CultureInfo.InvariantCulture));
            mConfig.AppSettings.Settings.Add("MusicVolume", MusicVolume.ToString(CultureInfo.InvariantCulture));
            mConfig.AppSettings.Settings.Add("SfxVolume", SfxVolume.ToString(CultureInfo.InvariantCulture));

            mConfig.Save(ConfigurationSaveMode.Full);
        }

        public static void SaveConfiguration()
        {
            if (mConfig.AppSettings.Settings.Count == 0)
            {
                CreateConfiguration();
                return;
            }

            mConfig.AppSettings.Settings["Fullscreen"].Value = Fullscreen.ToString();
            mConfig.AppSettings.Settings["KeyboardOnly"].Value = KeyboardOnly.ToString();
            mConfig.AppSettings.Settings["MouseSensitivity"].Value = MouseSensitivity.ToString(CultureInfo.InvariantCulture);
            mConfig.AppSettings.Settings["MusicVolume"].Value = MusicVolume.ToString(CultureInfo.InvariantCulture);
            mConfig.AppSettings.Settings["SfxVolume"].Value = SfxVolume.ToString(CultureInfo.InvariantCulture);
            mConfig.Save(ConfigurationSaveMode.Full);
        }

        public static int SfxVolume
        {
            get => (int)(SoundEffect.MasterVolume * 100);
            set => SoundEffect.MasterVolume = value / 100.0f;
        }

        public static int MusicVolume
        {
            get => (int) (MediaPlayer.Volume * 100);
            set => MediaPlayer.Volume = value / 100.0f;
        }

        public static bool Fullscreen { get; set; }

        public static float MouseSensitivity { get; set; }
        public static bool KeyboardOnly { get; set; }
    }
}
