using UnityEngine.Events;
using EasyPlayerController;
using Experiments.Global.Audio;
using Experiments.Global.Managers;
using Experiments.Global.IO;
using System;
using UnityEngine;

namespace Experiments.EndlessHallway.Managers
{
    public class GameManager : Manager<GameManager>
    {
        public enum GameStates
        {
            Init,
            Menu,
            Intro,
            Paused,
            Playing,
            GameOver
        }
        [Serializable]
        public class Stats : SaveFile
        {
            [Serializable]
            public class Stat
            {
                public string Name;
                public int Value;
            }
            [Space]
            public Stat[] stats;

            public int GetStat(string StatName)
            {
                return Array.Find(stats, Stat => Stat.Name == StatName).Value;
            }
            public void SetStat(string StatName, int NewValue)
            {
                Array.Find(stats, Stat => Stat.Name == StatName).Value = NewValue;
            }
            public void ChangeStat(string StatName, int Amount)
            {
                SetStat(StatName, GetStat(StatName) + Amount);
            }

            public void ResetStats()
            {
                foreach (Stat S in stats)
                {
                    S.Value = 0;
                }
            }
        }
        [Space]
        public GameStates GameState;
        public Stats CurStats;
        public CCPlayerController Player;
        public Transform StartingPoint;
        [HideInInspector]
        public PlayerInputReciever playerInputReciever;
        int IntroState;
        float IntroCountdown;
        float PrevIntroCountdown;
        Color[] IntroCountdownColors = new Color[]
        {
            Color.green,
            Color.yellow,
            new Color(1f, 0.5f, 0f),
            Color.red
        };
        [HideInInspector]
        public bool IntroAnim;
        int IntroTime;
        [HideInInspector]
        public int CurScore;
        int HIScore;
        bool NewHI;
        public float TimeLeft;
        float GameOverTime;
        Vector3 PrevPlayerVelocity;
        [Range(0f, 100f)]
        public float CoinSpawnPercent;
        [HideInInspector]
        public bool IsExplorer;
        [HideInInspector]
        public float StartTime;
        [HideInInspector]
        public bool NewSeedOnRetry;
        bool EnableGameUI;
        int Dist;
        int TravelDist;
        int PrevTotalDist;
        int TotalDist;
        int HIDist;

        [Header("Sky")]
        public Material SkyMat;
        public Color StartSkyColor;
        public Texture2D SkyTex;

        // Start is called before the first frame update
        void Start()
        {
            Init(this);

            Stats LoadedStats = Saver.Load(CurStats) as Stats;
            if(LoadedStats != null) { CurStats = LoadedStats; }
            LoadStats();

            playerInputReciever = Player.InputReciever;
        }
        void LoadStats()
        {
            HIScore = CurStats.GetStat("High Score");
            TotalDist = CurStats.GetStat("Total Distance Traveled");
            PrevTotalDist = TotalDist;
            HIDist = CurStats.GetStat("Best Distance Traveled");
        }
        public void AltStart()
        {
            if(GameState == GameStates.Init)
            {
                SkyMat.mainTexture = null;
                SkyMat.SetColor("_Tint", StartSkyColor);

                StartCoroutine(UIManager.Instance.FadeInOut(3f, 0.5f, new UnityAction(InitializeGame), new UnityAction(delegate { AudioManager.Instance.SetMusicTrack("Menu"); }), new System.Func<bool>(InitializationFinished), true));
            }
        }
        void InitializeGame()
        {
            SkyMat.mainTexture = SkyTex;
            SkyMat.SetColor("_Tint", Color.white);

            HallwayManager.Instance.RandomizeSeed();
            HallwayManager.Instance.Generate();

            UIManager.Instance.SetupMenus();

            GameState = GameStates.Menu;
        }

        // Update is called once per frame
        void Update()
        {
            // Player
            playerInputReciever = Player.InputReciever;
            playerInputReciever.LockMouse = PressedPlay() && SettingsManager.Instance.HideMouse;
            Player.CanMove = GameState == GameStates.Playing;

            if(GameState == GameStates.Init)
            {
                return;
            }

            // Intro
            if(GameState == GameStates.Intro)
            {
                // Rotate The Player Away From The Window
                if(IntroAnim)
                {
                    // If The StartingPoint In Facing Away From The Window, Stop Rotating It And
                    // Begin The Countdown
                    if(StartingPoint.rotation.y <= 0f)
                    {
                        IntroCountdown = 3f;
                        IntroTime = 3;
                        PrevIntroCountdown = 2f;
                        AudioManager.Instance.InteractWithSFX("Countdown", SoundEffectBehaviour.Play);
                        IntroAnim = false;
                    }

                    UIManager.IntroCountdownText.Label = "";
                    StartingPoint.Rotate(Vector3.up, -180f * Time.deltaTime);
                }
                // If We Finished Rotating The Player,
                // Start Counting Down From 3 To 0 And Display A Countdown On Screen,
                // Then Start The Game
                else
                {
                    // Set The Starting Point To Face Directly Forwards
                    StartingPoint.rotation = Quaternion.identity;

                    // Count Down
                    IntroCountdown -= Time.deltaTime;
                    // Converting The Intro Time An Integer
                    // So We Could Assign Colors To The Countdown
                    if(IntroCountdown < PrevIntroCountdown)
                    {
                        PrevIntroCountdown--;
                        IntroTime--;
                    }

                    // If The Countdown Is Above 0, Display The Countdown (Integer Form) On Screen
                    if(IntroTime > 0)
                    {
                        UIManager.IntroCountdownText.Label = IntroTime.ToString("0");
                    }
                    // If The Countdown Is Exactly 0, Display "Go!" On Screen
                    else if(IntroTime == 0)
                    {
                        UIManager.IntroCountdownText.Label = "Go!";
                    }
                    // If The Countdown Is Less Than 0, Start The Game
                    else if(IntroTime < 0)
                    {
                        UIManager.IntroCountdownText.Label = "";
                        AudioManager.Instance.InteractWithSFX("Drums", SoundEffectBehaviour.Stop);
                        if(!IsExplorer) { AudioManager.Instance.SetMusicTrack("MainTheme"); }
                        GameState = GameStates.Playing;
                    }

                    // If We Are Still Counting Down, Update The Countdown Texts Color
                    if(IntroTime >= 0) { UIManager.IntroCountdownText.color = IntroCountdownColors[IntroTime]; }
                }
            }
            // Enable/Disable The Countdown Text
            UIManager.IntroCountdownText.Object.SetActive(GameState == GameStates.Intro);

            // Menu
            if(GameState == GameStates.Menu || GameState == GameStates.Intro)
            {
                Player.transform.position = StartingPoint.position;
                Player.transform.rotation = StartingPoint.rotation;
                TimeLeft = StartTime;
                if(GameState != GameStates.Intro)
                {
                    StartingPoint.rotation = Quaternion.Euler(0f, 180f, 0f);
                    UIManager.IntroCountdownText.Label = "";
                }
            }

            // Game Loop
            // Game UI
            EnableGameUI = ((int)GameState > 2 && !IsExplorer);
            if(CurScore > HIScore)
            {
                HIScore = CurScore;
                if(!NewHI)
                {
                    NewHI = true;
                    AudioManager.Instance.InteractWithSFX("New HI Score", SoundEffectBehaviour.Play);
                }
            }
            UIManager.ScoreText.Label = "Score:" + CurScore.ToString("000");
            UIManager.ScoreText.Object.SetActive(EnableGameUI);
            UIManager.HIScoreText.Label = "High Score:" + HIScore.ToString("000");
            UIManager.HIScoreText.Object.SetActive(EnableGameUI);
            // New HI Text
            UIManager.NewHIText.Flash(0.125f, Color.yellow);
            UIManager.NewHIText.Object.SetActive(NewHI);
            // Time Limit
            if(GameState == GameStates.Playing && !IsExplorer) { TimeLeft -= Time.deltaTime; }
            UIManager.TimeText.Label = "Time Left:" + TimeLeft.ToString("000");
            UIManager.TimeText.Object.SetActive(EnableGameUI);
            if(TimeLeft <= 0f && GameState != GameStates.GameOver)
            {
                Player.DampVelocity();
                UIManager.TimeText.StopFlash();
                CurStats.ChangeStat("Game Overs", 1);
                GameState = GameStates.GameOver;
                AudioManager.Instance.MuteMusic();
                AudioManager.Instance.InteractWithSFX("Die", SoundEffectBehaviour.Play);
                AudioManager.Instance.InteractWithSFX("Warning", SoundEffectBehaviour.Stop);
            }
            // Time text Animations
            if(TimeLeft < 10f && GameState != GameStates.GameOver) { UIManager.TimeText.Flash(0.5f, Color.red); AudioManager.Instance.InteractWithSFXOneShot("Warning", SoundEffectBehaviour.Play); }
            else if(TimeLeft <= 0f && GameState == GameStates.GameOver) { UIManager.TimeText.color = Color.red; }
            else if(TimeLeft > 10f && GameState != GameStates.GameOver) { UIManager.TimeText.StopFlash(); }
            // Distance
            Dist = Mathf.RoundToInt(Vector3.Distance(Player.transform.position, StartingPoint.position));
            if(Dist > TravelDist) { TravelDist = Dist; }
            if(TravelDist > HIDist) { HIDist = TravelDist; }
            TotalDist = PrevTotalDist + TravelDist;

            // Player Menu
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                switch (GameState)
                {
                    case GameStates.Playing:
                    {
                        PlaySelectSound();
                        GameState = GameStates.Paused;
                        break;
                    }
                    case GameStates.Paused:
                    {
                        PlaySelectSound();
                        GameState = GameStates.Playing;
                        break;
                    }
                }
            }
            UIManager.PauseMenuBG.Object.SetActive(MiniMenuOn());
            // Audio Pause
            bool IsPaused = (GameState == GameStates.Paused);
            SoundEffectBehaviour soundEffectBehaviour = (IsPaused) ? SoundEffectBehaviour.Pause : SoundEffectBehaviour.Resume;
            AudioManager.Instance.InteractWithAllSFXOneShot(soundEffectBehaviour);
            AudioManager.Instance.InteractWithMusic(soundEffectBehaviour);

            // Mini Menu Animations
            // Game Over Animations
            if(GameState == GameStates.GameOver)
            {
                // Animate Game Over Screen
                // Increase Game over Animation Time
                GameOverTime += Time.deltaTime;
                // Mini Menu Background
                float BGTransculancy = Mathf.Lerp(0f, 0.5f, Mathf.Clamp01(GameOverTime));
                UIManager.PauseMenuBG.color = new Color(1f, 0f, 0f, BGTransculancy);
                // Mini menu Label
                float PauseTextTransparency = Mathf.Clamp01(GameOverTime - 1f);
                UIManager.MiniMenuText.Label = "Game Over!";
                UIManager.MiniMenuText.color = new Color(1f, 0f, 0f, PauseTextTransparency);
                // Mini Menu Buttons
                float ButtonTransculancy = Mathf.Clamp01(GameOverTime - 2f);
                UIManager.RetryButton.Transparency = ButtonTransculancy;
                UIManager.menuButton.Transparency = ButtonTransculancy;
            }
            // Pause Menu "Animations"
            else
            {
                UIManager.PauseMenuBG.color = Color.black / 2f;
                UIManager.MiniMenuText.color = Color.white;
                UIManager.MiniMenuText.Label = "Pause";
            }

            // Save The Players Stats
            SaveStats();
        }
        void SaveStats()
        {
            CurStats.SetStat("High Score", HIScore);
            CurStats.SetStat("Total Distance Traveled", TotalDist);
            CurStats.SetStat("Best Distance Traveled", HIDist);

            CurStats.Save();
        }

        public void CollectToken()
        {
            AudioManager.Instance.InteractWithSFX("Coin Collect", SoundEffectBehaviour.Play);
            CurScore += 10;
            CurStats.ChangeStat("Total Coins Collected", 1);
        }
        
        void ResetDist()
        {
            PrevTotalDist = TotalDist;
            TravelDist = 0;
        }

        public void ResetPlayer()
        {
            GameOverTime = 0f;
            TimeLeft = StartTime;
            CurScore = 0;
            NewHI = false;

            Player.transform.position = StartingPoint.position;
            Player.transform.rotation = StartingPoint.rotation;
        }
        public void RegenerateMap()
        {
            HallwayManager.Instance.RandomizeSeed();
            HallwayManager.Instance.Generate();
        }
        public void PlayIntro()
        {
            IntroAnim = true;
            AudioManager.Instance.MuteMusic();
            AudioManager.Instance.InteractWithSFX("Drums", SoundEffectBehaviour.Play);
            GameState = GameStates.Intro;
        }


        public void Retry()
        {
            ResetDist();
            if(NewSeedOnRetry)
            {
                RegenerateMap();
            }
            else
            {
                HallwayManager.Instance.Generate();
            }
        }
        public void Menu()
        {
            ResetPlayer();
            ResetDist();
            RegenerateMap();
            GameState = GameStates.Menu;
            AudioManager.Instance.MuteMusic();
        }

        public bool InitializationFinished()
        {
            return HallwayManager.Instance.ReadyFunc() && UIManager.Instance.AllMenusReady();
        }
        public bool PressedPlay()
        {
            return (GameState == GameStates.Playing || GameState == GameStates.Intro);
        }
        bool MiniMenuOn()
        {
            return (GameState == GameStates.Paused || GameState == GameStates.GameOver);
        }

        public void ResetAllStats()
        {
            HIScore = 0;
            PrevTotalDist = 0;
            TotalDist = 0;
            HIDist = 0;
            CurStats.ResetStats();

            AudioManager.Instance.InteractWithSFX("Reset Stats", SoundEffectBehaviour.Play);
        }
    }
}