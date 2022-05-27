using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Experiments.EndlessHallway.Managers;
using Experiments.Global.Managers;
using Experiments.Global.Audio;
using Experiments.Global.IO;
using System;

namespace Experiments.EndlessHallway.Managers
{
    public class SettingsManager : Manager<SettingsManager>
    {
        [Serializable]
        public class SliderSetting
        {
            public string Name;
            public float Value;
            public float MinValue;
            public float MaxValue;
        }
        [Serializable]
        public class DropdownSetting
        {
            public string Name;
            public int Value;
            public List<string> Options;
        }
        [Serializable]
        public class ToggleSetting
        {
            public string Name;
            public bool Value;
        }
        [Serializable]
        public class InputFieldSetting
        {
            public string Name;
            public int Value;
        }

        [Serializable]
        public class Settings : SaveFile
        {
            [Space]
            public SliderSetting[] SliderSettings;
            public DropdownSetting[] DropdownSettings;
            public ToggleSetting[] ToggleSettings;
            public InputFieldSetting[] InputFieldSettings;
        }
        public Settings settings;
        public float[] CoinSpawnRates;
        [HideInInspector]
        public bool HideMouse;
        bool SettingsLoadedSuccessfuly;
        Resolution[] Resolutions;
        int PrevResIndex;

        // Start is called before the first frame update
        void Awake()
        {
            // Initialize Manager Object
            Init(this);

            // Load The Players Settings
            Settings LoadedSettings = Saver.Load(settings) as Settings;
            SettingsLoadedSuccessfuly = LoadedSettings != null;
            if(SettingsLoadedSuccessfuly) { settings = LoadedSettings; }

            // Initialize Resolutions Option
            InitRes();
        }
        void InitRes()
        {
            // Store The Resolution Setting In A Variable
            DropdownSetting ResSetting = FindDropdownSetting("Resolution");
            // Clear All Previous Options From The Resolution Setting.
            // We Want The Options To Automatically Be All Of The Available Screen Resolutions.
            ResSetting.Options.Clear();
            // Get All Available Screen Resolutions.
            Resolutions = Screen.resolutions;
            // The Index Of The Current Screen Resolution On The Resolutions Array.
            int CurResIndex = 0;
            for (int R = 0; R < Resolutions.Length; R++)
            {
                // The Screen Resolution From The Resolutions Array At The Index R.
                Resolution Res = Resolutions[R];
                // The Current Screen Resolution.
                Resolution CurRes = Screen.currentResolution;

                // Convert The Resolution To A String Variable.
                string Res2String = Res.width + "x" + Res.height + "@" + Res.refreshRate + "Hz";
                // Add The Converted String Variable To The Options List.
                ResSetting.Options.Add(Res2String);

                // If The Resolution At Index R Is The Current Screen Resolution,
                // Set The Current Screen Resolution Index To R.
                if(Res.width == CurRes.width && Res.height == CurRes.height && Res.refreshRate == CurRes.refreshRate)
                {
                    CurResIndex = R;
                }
            }

            // If The Player Has Not Set A Resolution Of Their Own,
            // Set The Value Of The Resolution Setting To The Index Of The Current Screen Resolution On The Resolutions Array.
            if(!SettingsLoadedSuccessfuly) { ResSetting.Value = CurResIndex; }
            PrevResIndex = -PrevResIndex;
        }

        // Update is called once per frame
        void Update()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            // Apply Video Settings
            Screen.fullScreen = FindToggleSetting("Fullscreen").Value;
            Application.runInBackground = FindToggleSetting("Run In Background").Value;
            HideMouse = FindToggleSetting("Hide Mouse").Value;
            HallwayManager.Instance.CamRange = FindSliderSetting("Render Distance").Value;
            QualitySettings.SetQualityLevel(FindDropdownSetting("Quality").Value);
            // Applying Resolution Is A Little Bit Harder
            int ResIndex = FindDropdownSetting("Resolution").Value;
            if(ResIndex != PrevResIndex)
            {
                PrevResIndex = ResIndex;
                Resolution Res = Resolutions[ResIndex];
                Screen.SetResolution(Res.width, Res.height, Screen.fullScreen, Res.refreshRate);
            }

            // Apply Volume Settings
            AudioManager.Instance.SetChannelVolume("Master", FindSliderSetting("Master Volume").Value);
            AudioManager.Instance.SetChannelVolume("Music", FindSliderSetting("Music Volume").Value);
            AudioManager.Instance.SetChannelVolume("SFX", FindSliderSetting("SFX Volume").Value);

            // Apply Input Settings
            GameManager.Instance.Player.InputReciever.InvertMouse = FindToggleSetting("Invert Mouse").Value;
            GameManager.Instance.Player.Sens = FindSliderSetting("Sensitivity").Value;

            // Apply Game Settings
            HallwayManager.Instance.GameSeed = FindInputFieldSetting("Game Seed").Value;
            GameManager.Instance.IsExplorer = FindToggleSetting("Explorer Mode").Value;
            GameManager.Instance.NewSeedOnRetry = FindToggleSetting("New Seed On Retry").Value;
            DropdownSetting TimeDropdownSetting = FindDropdownSetting("Time Limit");
            GameManager.Instance.StartTime = Convert.ToInt32(TimeDropdownSetting.Options[TimeDropdownSetting.Value]);
            GameManager.Instance.CoinSpawnPercent = CoinSpawnRates[FindDropdownSetting("Coin Spawn Rate").Value];

            // Update Stats Menu
            for (int S = 0; S < GameManager.Instance.CurStats.stats.Length; S++)
            {
                GameManager.Stats.Stat stat = GameManager.Instance.CurStats.stats[S];
                UIManager.Instance.SettingsCategories[4].Settings[S].Label = stat.Name + ":" + stat.Value;
            }

            // Save Settings
            settings.Save();
        }

        public SliderSetting FindSliderSetting(string Name)
        {
            return Array.Find(settings.SliderSettings, SliderSetting => SliderSetting.Name == Name);
        }
        public DropdownSetting FindDropdownSetting(string Name)
        {
            return Array.Find(settings.DropdownSettings, DropdownSetting => DropdownSetting.Name == Name);
        }
        public ToggleSetting FindToggleSetting(string Name)
        {
            return Array.Find(settings.ToggleSettings, ToggleSetting => ToggleSetting.Name == Name);
        }
        public InputFieldSetting FindInputFieldSetting(string Name)
        {
            return Array.Find(settings.InputFieldSettings, InputFieldSetting => InputFieldSetting.Name == Name);
        }
    }
}