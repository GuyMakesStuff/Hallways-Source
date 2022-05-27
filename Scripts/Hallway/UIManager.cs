using Experiments.EndlessHallway.Visuals;
using Experiments.Global.Managers;
using Experiments.Global.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Experiments.EndlessHallway.Managers
{
    // The Main Menu And All Other UI Elements Construct Themselfs.
    // It Is Not Ideal To Create A Main menu Like This, However This Is An
    // Expiremental Project So It Doesnt Matter.
    public class UIManager : Manager<UIManager>
    {
        [Header("General")]
        public RectTransform canvas;
        public RectTransform MenuAnchor;
        public GameObject MenuRefrence;
        public MenuItem[] Templates;
        public TMP_FontAsset RegularFont;
        public TMP_FontAsset SmallFont;

        [Header("Settings Menu")]
        public Sprite SettingsMenuBGSprite;
        RectTransform SettingsScrollContent;
        public GameObject[] RefrenceSettings;
        public SettingsCategory[] SettingsCategories;

        public static class Utils
        {
            public static Menu NewMenu(string MenuName, float MenuYPos)
            {
                Menu menu = new Menu(MenuName, MenuYPos);
                return menu;
            }

            public static MenuItem NewMenuItem(string ItemName, string label, GameObject RefObject, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Menu menu)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.RefrenceObject = RefObject;
                menuItem.Construct(ItemName, label, position, size, anchorMin, anchorMax, menu);
                return menuItem;
            }
            public static MenuImage NewMenuImage(string ItemName, string label, GameObject RefObject, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Menu menu)
            {
                MenuImage menuImage = new MenuImage();
                menuImage.RefrenceObject = RefObject;
                menuImage.Construct(ItemName, label, position, size, anchorMin, anchorMax, menu);
                return menuImage;
            }
            public static MenuText NewMenuText(string ItemName, string label, GameObject RefObject, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Menu menu)
            {
                MenuText menuText = new MenuText();
                menuText.RefrenceObject = RefObject;
                menuText.Construct(ItemName, label, position, size, anchorMin, anchorMax, menu);
                return menuText;
            }
            public static MenuButton NewMenuButton(string ItemName, string label, GameObject RefObject, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Menu menu, UnityAction[] OnClick)
            {
                MenuButton menuButton = new MenuButton();
                menuButton.RefrenceObject = RefObject;
                menuButton.Construct(ItemName, label, position, size, anchorMin, anchorMax, menu);
                menuButton.OnClickEvents = OnClick;
                return menuButton;
            }
        }

        public class Menu
        {
            public string Name;
            public GameObject Object;
            public float MenuYPos;
            [HideInInspector]
            public RectTransform rectTransform;
            public List<MenuItem> Items;
            [HideInInspector]
            public bool FinishedSetup;

            public Menu(string name, float YPos)
            {
                Name = name;
                Items = new List<MenuItem>();
                Object = Instantiate(UIManager.Instance.MenuRefrence);
                Object.name = Name;
                rectTransform = Object.GetComponent<RectTransform>();
                rectTransform.sizeDelta = UIManager.Instance.GetScreenSize();
                rectTransform.SetParent(UIManager.Instance.MenuAnchor);
                rectTransform.anchoredPosition = Vector2.down * YPos;
                MenuYPos = YPos;
                rectTransform.ForceUpdateRectTransforms();
            }

            public void AddItem(MenuItem item)
            {
                item.rectTransform.SetParent(rectTransform);
                Items.Add(item);
            }

            public void UpdateMenu()
            {
                foreach (MenuItem MI in Items)
                {
                    MI.ItemUpdate();
                }
            }
        }
        [System.Serializable]
        public class MenuItem
        {
            public string Name;
            [HideInInspector]
            public string Label;
            public GameObject RefrenceObject;
            [HideInInspector]
            public RectTransform rectTransform;
            [HideInInspector]
            public GameObject Object;
            [HideInInspector]
            public Vector2 Position;
            [HideInInspector]
            public Vector2 Size;
            [HideInInspector]
            public Vector2 AnchorMin;
            [HideInInspector]
            public Vector2 AnchorMax;

            public void Construct(string ItemName, string label, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Menu menu)
            {
                Name = ItemName;
                Label = label;
                Position = position;
                Size = size;
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;

                Object = Instantiate(RefrenceObject);
                Object.name = Name;
                Object.SetActive(true);
                rectTransform = Object.GetComponent<RectTransform>();
                rectTransform.pivot = Vector2.one / 2f;
                rectTransform.sizeDelta = size;
                rectTransform.anchorMin = AnchorMin;
                rectTransform.anchorMax = AnchorMax;
                if(menu != null) { menu.AddItem(this); }
                rectTransform.anchoredPosition = Position;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.ForceUpdateRectTransforms();

                ItemInit();
            }

            public virtual void ItemInit()
            {

            }

            public virtual void ItemUpdate()
            {
                
            }
        }
        public class MenuButton : MenuItem
        {
            Button button;
            TMP_Text text;
            public float FontSize;
            public TextAlignmentOptions Alignment;
            public UnityAction[] OnClickEvents;
            public float Transparency;

            public override void ItemInit()
            {
                base.ItemInit();
                button = Object.GetComponent<Button>();
                text = Object.GetComponent<TextMeshProUGUI>();
            }

            public override void ItemUpdate()
            {
                base.ItemUpdate();
                text.text = Label;
                text.fontSize = FontSize;
                text.alignment = Alignment;
                text.alpha = Mathf.Clamp01(Transparency);
                button.interactable = Transparency > 0.01f;
            }

            public void AssignEvents()
            {
                button.onClick.RemoveAllListeners();
                foreach (UnityAction A in OnClickEvents)
                {
                    button.onClick.AddListener(A);
                }
            }
        }
        public class MenuText : MenuItem
        {
            TMP_Text text;
            public float FontSize;
            public TextAlignmentOptions Alignment;
            public TMP_FontAsset Font;
            public Color color;
            [HideInInspector]
            public Color PrevColor;
            float FlashTime;
            float PrevFlashTime;
            bool FlashOn;

            public override void ItemInit()
            {
                base.ItemInit();
                text = Object.GetComponent<TextMeshProUGUI>();
            }

            public override void ItemUpdate()
            {
                base.ItemUpdate();
                text.text = Label;
                text.fontSize = FontSize;
                text.alignment = Alignment;
                text.font = Font;
                text.color = color;
                FlashTime += Time.deltaTime;
            }

            public void Flash(float FlashIntervals, Color FlashColor)
            {
                if(FlashTime >= PrevFlashTime)
                {
                    PrevFlashTime += FlashIntervals;
                    FlashOn = !FlashOn;
                }
                color = (FlashOn) ? FlashColor : PrevColor;
            }
            public void StopFlash()
            {
                PrevFlashTime = FlashTime;
                color = PrevColor;
                FlashOn = false;
            }
        }
        public class MenuImage : MenuItem
        {
            [HideInInspector]
            public Image image;
            public Sprite Image;
            public Color color;

            public override void ItemInit()
            {
                base.ItemInit();
                image = Object.GetComponent<Image>();
            }
            public override void ItemUpdate()
            {
                base.ItemUpdate();
                image.sprite = Image;
                image.color = color;
            }
        }
        [System.Serializable]
        public class Setting
        {
            public string Label;
            public enum SettingType { Slider, Dropdown, Toggle, InputField, Label }
            public SettingType type;
            public string PropertyName;
            [HideInInspector]
            public GameObject Object;
            [HideInInspector]
            public RectTransform rectTransform;

            public void InitSetting()
            {
                switch (type)
                {
                    case SettingType.Slider:
                    {
                        Slider slider = Object.transform.Find("ValueSlider").GetComponent<Slider>();
                        slider.minValue = SettingsManager.Instance.FindSliderSetting(PropertyName).MinValue;
                        slider.maxValue = SettingsManager.Instance.FindSliderSetting(PropertyName).MaxValue;
                        slider.value = SettingsManager.Instance.FindSliderSetting(PropertyName).Value;
                        break;
                    }
                    case SettingType.Dropdown:
                    {
                        TMP_Dropdown dropdown = Object.transform.Find("ValueDropdown").GetComponent<TMP_Dropdown>();
                        dropdown.ClearOptions();
                        dropdown.AddOptions(SettingsManager.Instance.FindDropdownSetting(PropertyName).Options);
                        dropdown.value = SettingsManager.Instance.FindDropdownSetting(PropertyName).Value;
                        dropdown.RefreshShownValue();
                        dropdown.onValueChanged.AddListener(new UnityAction<int>(delegate { AudioManager.Instance.PlaySelectSound(); }));
                        break;
                    }
                    case SettingType.Toggle:
                    {
                        Toggle toggle = Object.transform.Find("ValueToggle").GetComponent<Toggle>();
                        toggle.isOn = SettingsManager.Instance.FindToggleSetting(PropertyName).Value;
                        toggle.onValueChanged.AddListener(new UnityAction<bool>(delegate { AudioManager.Instance.PlaySelectSound(); }));
                        break;
                    }
                    case SettingType.InputField:
                    {
                        TMP_InputField inputField = Object.transform.Find("ValueInputField").GetComponent<TMP_InputField>();
                        inputField.text = SettingsManager.Instance.FindInputFieldSetting(PropertyName).Value.ToString("0");
                        inputField.onSelect.AddListener(new UnityAction<string>(delegate { AudioManager.Instance.PlaySelectSound(); }));
                        break;
                    }
                }
            }

            public void UpdateSetting()
            {
                rectTransform.sizeDelta = Vector2.up * 50f;

                rectTransform.Find("SettingLabel").GetComponent<TMP_Text>().text = " " + Label;

                switch (type)
                {
                    case SettingType.Slider:
                    {
                        Slider slider = Object.transform.Find("ValueSlider").GetComponent<Slider>();
                        SettingsManager.Instance.FindSliderSetting(PropertyName).Value = slider.value;
                        break;
                    }
                    case SettingType.Dropdown:
                    {
                        TMP_Dropdown dropdown = Object.transform.Find("ValueDropdown").GetComponent<TMP_Dropdown>();
                        SettingsManager.Instance.FindDropdownSetting(PropertyName).Value = dropdown.value;
                        break;
                    }
                    case SettingType.Toggle:
                    {
                        Toggle toggle = Object.transform.Find("ValueToggle").GetComponent<Toggle>();
                        SettingsManager.Instance.FindToggleSetting(PropertyName).Value = toggle.isOn;
                        break;
                    }
                    case SettingType.InputField:
                    {
                        TMP_InputField inputField = Object.transform.Find("ValueInputField").GetComponent<TMP_InputField>();
                        string InputValue = inputField.text;
                        if(InputValue == string.Empty || InputValue == "-") { SettingsManager.Instance.FindInputFieldSetting(PropertyName).Value = 0; break; }
                        SettingsManager.Instance.FindInputFieldSetting(PropertyName).Value = System.Convert.ToInt32(InputValue);
                        break;
                    }
                }
            }
        }
        [System.Serializable]
        public class SettingsCategory
        {
            public string Name;
            public Setting[] Settings;
            [HideInInspector]
            public RectTransform CategoryObject;

            public void Construct(RectTransform SettingsScroll)
            {
                // Category Object
                CategoryObject = new GameObject(Name + "Settings").AddComponent<RectTransform>();
                CategoryObject.SetParent(SettingsScroll);
                CategoryObject.anchorMin = Vector2.up;
                CategoryObject.anchorMax = Vector2.one;
                CategoryObject.anchoredPosition = Vector2.down * 150f;
                CategoryObject.sizeDelta = new Vector2(0f, 300f);

                // Settings Objects
                float YPos = 15f;
                for (int S = 0; S < Settings.Length; S++)
                {
                    GameObject SettingsObject = Instantiate(UIManager.Instance.RefrenceSettings[(int)Settings[S].type], Vector3.zero, Quaternion.identity, CategoryObject);
                    SettingsObject.SetActive(true);
                    RectTransform rectTransform = SettingsObject.GetComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.up;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.pivot = Vector2.up;
                    rectTransform.anchoredPosition = Vector2.down * YPos;
                    YPos += rectTransform.rect.height + 15f;

                    Settings[S].Object = SettingsObject;
                    Settings[S].rectTransform = rectTransform;
                    Settings[S].InitSetting();
                }
                CategoryObject.sizeDelta = new Vector2(0f, YPos);
                CategoryObject.anchoredPosition = Vector2.down * (CategoryObject.sizeDelta.y / 2f);
            }

            public void UpdateSettings()
            {
                foreach (Setting S in Settings)
                {
                    S.UpdateSetting();
                }
            }
        }

        List<Menu> Menus;
        float AnimTime;
        float PrevAnimTime;
        Menu CurMenu;
        Menu PrevMenu;
        MenuImage Fade;
        float FadeRate;
        float FadeSpeed;
        bool IsFaded;
        [HideInInspector]
        public int PrevGameSeed;
        string CurSettingsCategory;
        List<GameObject> SettingsButtons;
        List<SettingsCategory> SettingsCategoryObjects;
        MenuButton SettingsBackButton;
        MenuText MouseTestText;
        MenuImage SettingsBG;
        MenuCamera menuCamera;
        float ResetTimer;
        enum ResetStates { None, Holding, Done }
        ResetStates ResetState;

        // Static Refrences
        public static MenuImage PauseMenuBG;
        public static MenuButton RetryButton;
        public static MenuButton menuButton;
        public static MenuText IntroCountdownText;
        public static MenuText MiniMenuText;
        public static MenuText ScoreText;
        public static MenuText HIScoreText;
        public static MenuText NewHIText;
        public static MenuText TimeText;

        // Start is called before the first frame update
        void Start()
        {
            Init(this);

            menuCamera = FindObjectOfType<MenuCamera>();

            CreateFader();
            GameManager.Instance.AltStart();
        }
        void CreateFader()
        {
            Fade = NewImage("Fade", "Fader", null, Vector2.zero, MenuAnchor.sizeDelta, Vector2.zero, Vector2.one, null, new Color(1f, 1f, 1f, 0f));
            Fade.rectTransform.SetParent(canvas);
            Fade.rectTransform.sizeDelta = MenuAnchor.sizeDelta;
            Fade.rectTransform.anchoredPosition = MenuAnchor.anchoredPosition;
        }
        public void SetupMenus()
        {
            // Initialize Menus List
            Menus = new List<Menu>(3);

            // Create The Main Menu
            SetupMainMenu();
            // Create The Play Menu
            SetupPlayMenu();
            // Create The Settings menu
            SetupSettingsMenu();
        }
        void SetupMainMenu()
        {
            // Main Menu Object
            Menus.Add(Utils.NewMenu("Main", 0f));
            CurMenu = GetMenu("Main");
            PrevMenu = GetMenu("Main");

            // Game Title Text
            NewText("Title", "Hallways", GetMenu("Main"), Vector2.up * 80, new Vector2(800, 160), Vector2.one * 0.5f, Vector2.one * 0.5f, TextAlignmentOptions.Center, 200f, RegularFont, Color.white);
            // Main Menu Buttons
            string[] MenuButtons = new string[3]{ "Play", "Settings", "Quit"};
            UnityAction[] MenuButtonEvents = new UnityAction[]
            {
                new UnityAction(Play),
                new UnityAction(delegate { SetMenu(GetMenu("Settings")); }),
                new UnityAction(QuitGame)
            };
            for (int MB = 0; MB < MenuButtons.Length; MB++)
            {
                Vector2 Pos = new Vector2(-305f + (305f * MB), -60f);
                UnityAction[] actions = new UnityAction[]
                {
                    MenuButtonEvents[MB],
                    new UnityAction(PlaySelectSound)
                };
                NewButton(MenuButtons[MB], MenuButtons[MB], GetMenu("Main"), Pos, new Vector2(300f, 75f), Vector2.one * 0.5f, Vector2.one * 0.5f, TextAlignmentOptions.Center, 75f, actions, 1f);
            }

            GetMenu("Main").FinishedSetup = true;
        }
        void SetupPlayMenu()
        {
            // Play Menu Object
            Menus.Add(Utils.NewMenu("Play", -Screen.height));

            // Pause/Game Over Mini Menu
            // Pause/Game Over Menu Background Image
            PauseMenuBG = NewImage("BG", "Background", GetMenu("Play"), Vector2.zero, GetScreenSize(), Vector2.zero, Vector2.one, null, Color.black / 2f);
            // Mini Menu Text
            MiniMenuText = NewText("MiniMenu", "Pause", GetMenu("Play"), Vector2.up * 80f, new Vector2(780f, 115f), Vector2.one / 2f, Vector2.one / 2f, TextAlignmentOptions.Center, 150f, RegularFont, Color.white);
            MiniMenuText.rectTransform.SetParent(PauseMenuBG.rectTransform);
            // Retry Button
            UnityAction[] actions = new UnityAction[]
            {
                new UnityAction(delegate
                { 
                    StartCoroutine(FadeInOut(0.5f, 2f, new UnityAction(delegate { GameManager.Instance.Retry(); }), new UnityAction(delegate { GameManager.Instance.ResetPlayer(); GameManager.Instance.PlayIntro(); }), new System.Func<bool>(HallwayManager.Instance.ReadyFunc), false));
                ; }),
                new UnityAction(PlaySelectSound)
            };
            RetryButton = NewButton("Retry", "Retry", GetMenu("Play"), Vector2.down * 50f, new Vector2(280, 75f), Vector2.one / 2f, Vector2.one / 2f, TextAlignmentOptions.Center, 80f, actions, 1f);
            RetryButton.rectTransform.SetParent(PauseMenuBG.rectTransform);
            // Menu Button
            actions[0] = new UnityAction(delegate
            { 
                StartCoroutine(FadeInOut(0.5f, 2f, new UnityAction(delegate
                { 
                    SetMenu(GetMenu("Main"));
                    GameManager.Instance.Menu();
                }), new UnityAction(delegate { AudioManager.Instance.SetMusicTrack("Menu"); }), new System.Func<bool>(HallwayManager.Instance.ReadyFunc), false));
            });
            menuButton = NewButton("Menu", "Menu", GetMenu("Play"), Vector2.down * 140f, new Vector2(280, 75f), Vector2.one / 2f, Vector2.one / 2f, TextAlignmentOptions.Center, 80f, actions, 1f);
            menuButton.rectTransform.SetParent(PauseMenuBG.rectTransform);

            // Intro Countdown Text
            IntroCountdownText = NewText("Countdown", "3", GetMenu("Play"), Vector2.zero, new Vector2(400, 160), Vector2.one * 0.5f, Vector2.one * 0.5f, TextAlignmentOptions.Center, 175f, RegularFont, Color.white);

            // Time Text
            TimeText = NewText("Time", "Time Left:", GetMenu("Play"), new Vector2(210, -30), new Vector2(405f, 50f), Vector2.up, Vector2.up, TextAlignmentOptions.Left, 50f, SmallFont, Color.white);
            // Score Text
            ScoreText = NewText("Score", "Score:", GetMenu("Play"), new Vector2(210, 65), new Vector2(405f, 50f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, 50f, SmallFont, Color.white);
            // High Score Text
            HIScoreText = NewText("HIScore", "High Score:", GetMenu("Play"), new Vector2(212, 25), new Vector2(405f, 27f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, 35f, SmallFont, Color.white);
            // New HI Text
            NewHIText = NewText("NewHI", "New High Score!", GetMenu("Play"), new Vector2(152f, 110f), new Vector2(285f, 35f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, 40f, SmallFont, Color.white);

            GetMenu("Play").FinishedSetup = true;
        }
        void SetupSettingsMenu()
        {
            // Settings Menu Object
            Menus.Add(Utils.NewMenu("Settings", Screen.height));

            // Settings
            // Settings Background
            SettingsBG = NewImage("SettingsBG", "Settings Background", GetMenu("Settings"), Vector2.zero, new Vector2(1000f, 800f), Vector2.zero, Vector2.one, SettingsMenuBGSprite, Color.white);
            // Settings Scroller
            MenuItem SettingsScroller = Utils.NewMenuItem("Settings", "Settings Scroll View", FindItemRef("Settings Scroll"), Vector2.zero, new Vector2(975f, 785f), Vector2.zero, Vector2.one, GetMenu("Settings"));
            SettingsScroller.rectTransform.SetParent(SettingsBG.rectTransform);
            SettingsScrollContent = SettingsScroller.Object.GetComponent<ScrollRect>().content;
            // Settings Categories, Buttons And Objects
            int SettingsCategoryCount = SettingsCategories.Length;
            SettingsCategoryObjects = new List<SettingsCategory>(SettingsCategoryCount);
            float StartButtonXPos = 0f - ((240f / 2f) * (SettingsCategoryCount - 1));
            Vector2 ButtonAnchor = new Vector2(0.5f, 1f);
            SettingsButtons = new List<GameObject>(SettingsCategoryCount);
            for (int SC = 0; SC < SettingsCategoryCount; SC++)
            {
                float ButtonXPos = StartButtonXPos + (240f * SC);
                int CategoryIndex = SC;
                SettingsButtons.Add(NewButton(SettingsCategories[SC].Name + "Category", SettingsCategories[SC].Name, GetMenu("Settings"), new Vector2(ButtonXPos, -87.5f), new Vector2(230f, 75f), ButtonAnchor, ButtonAnchor, TextAlignmentOptions.Center, 80f, new UnityAction[]
                {
                    new UnityAction(delegate { SetSettingsCategory(CategoryIndex); }),
                    new UnityAction(PlaySelectSound)
                }, 1f).Object);
                CreateSettingCategory(SC);
            }
            SetSettingsCategory(0);

            // Mouse Testing Text
            MouseTestText = NewText("MouseTest", "Press F To Stop Testing Mouse", GetMenu("Settings"), new Vector2(348.568f, 35f), new Vector2(682.1359f, 50f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, 50f, SmallFont, Color.white);

            // Back Button
            SettingsBackButton = NewButton("Back", "Back", GetMenu("Settings"), Vector2.up * 65f, new Vector2(195f, 75f), Vector2.right / 2f, Vector2.right / 2f, TextAlignmentOptions.Center, 75f, new UnityAction[] { new UnityAction(BackFromSettings) }, 1f);

            GetMenu("Settings").FinishedSetup = true;
        }
        void BackFromSettings()
        {
            // Apply Game Seed Setting
            // If The Player Has Entered An Invalid Seed, Set The Game Seed To 0.
            TMP_InputField SeedInput = SettingsCategories[3].Settings[0].Object.transform.Find("ValueInputField").GetComponent<TMP_InputField>();
            if(SeedInput.text == string.Empty || SeedInput.text == "-")
            {
                FixGameSeed(0);
                SettingsManager.Instance.ApplySettings();
            }
            // Regenerate The Map Only If The Seed Setting Has Changed
            if(SettingsManager.Instance.FindInputFieldSetting("Game Seed").Value != PrevGameSeed)
            {
                PrevGameSeed = SettingsManager.Instance.FindInputFieldSetting("Game Seed").Value;
                HallwayManager.Instance.Generate();
            }
            // Return To The Main Menu
            SetMenu(GetMenu("Main"));
            // Play Button Click Sound
            PlaySelectSound();
        }
        void CreateSettingCategory(int Index)
        {
            SettingsCategory settingsCategory = SettingsCategories[Index];
            settingsCategory.Construct(SettingsScrollContent);
            SettingsCategoryObjects.Add(settingsCategory);
        }
        void SetSettingsCategory(int Index)
        {
            CurSettingsCategory = SettingsCategoryObjects[Index].Name;
            for (int SC = 0; SC < SettingsCategoryObjects.Count; SC++)
            {
                SettingsCategoryObjects[SC].CategoryObject.gameObject.SetActive(SC == Index);
                if(SC == Index)
                {
                    SettingsScrollContent.anchoredPosition = SettingsCategoryObjects[SC].CategoryObject.anchoredPosition;
                    SettingsScrollContent.sizeDelta = SettingsCategoryObjects[SC].CategoryObject.sizeDelta;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            // We Should Only Update The Actual Menus If
            // We Have Finished Initializing
            if(GameManager.Instance.GameState != GameManager.GameStates.Init)
            {
                // Update Menu Items
                foreach (Menu M in Menus)
                {
                    M.UpdateMenu();
                }
                // Update Settings
                foreach (SettingsCategory SC in SettingsCategoryObjects)
                {
                    SC.UpdateSettings();
                }

                // Animate Menu Transitions
                AnimTime += Time.deltaTime;
                float MenuAnchorYPos = (AnimTime - PrevAnimTime) * 2f;
                MenuAnchor.anchoredPosition = new Vector3(0f, Mathf.Lerp(PrevMenu.MenuYPos, CurMenu.MenuYPos, MenuAnchorYPos), 0f);

                SpecialMenuFunctions();
            }

            // Update Fader.
            // The Fader Is Seperate From All Of the Menus So
            // It Will Be Updated Seperately.
            Fade.ItemUpdate();
            // Animate Fader
            FadeRate += ((IsFaded) ? FadeSpeed : -FadeSpeed) * Time.deltaTime;
            FadeRate = Mathf.Clamp(FadeRate, 0f, 1f);
            Fade.color = new Color(1f, 1f, 1f, FadeRate);
            Fade.image.raycastTarget = (FadeRate > 0.001f);
        }
        void SpecialMenuFunctions()
        {
            // Input Menu Mouse Testing
            if(GameManager.Instance.GameState == GameManager.GameStates.Menu && CurMenu.Name == "Settings" && CurSettingsCategory == "Input")
            {
                if(Input.GetKeyDown("f")) { menuCamera.IsTestingMouse = !menuCamera.IsTestingMouse; PlaySelectSound(); }
            }
            SettingsBG.Object.SetActive(!menuCamera.IsTestingMouse);
            foreach (GameObject SBOBJ in SettingsButtons) { SBOBJ.SetActive(!menuCamera.IsTestingMouse); }
            SettingsBackButton.Object.SetActive(!menuCamera.IsTestingMouse);
            MouseTestText.Object.SetActive(menuCamera.IsTestingMouse);

            // Stats Resetting
            if(GameManager.Instance.GameState == GameManager.GameStates.Menu && CurMenu.Name == "Settings" && CurSettingsCategory == "Stats")
            {
                if(Input.GetKeyDown("r")) { ResetState = ResetStates.Holding; }
                else if(Input.GetKeyUp("r")) { ResetState = ResetStates.None; }
            }
            else
            {
                ResetState = ResetStates.None;
            }

            switch(ResetState)
            {
                case ResetStates.None:
                {
                    ResetTimer = 0f;
                    break;
                }
                case ResetStates.Holding:
                {
                    ResetTimer += Time.deltaTime;
                    if(ResetTimer >= 5f)
                    {
                        GameManager.Instance.ResetAllStats();
                        ResetState = ResetStates.Done;
                    }
                    break;
                }
                case ResetStates.Done:
                {
                    ResetTimer = -1f;
                    break;
                }
            }
        }

        MenuImage NewImage(string Name, string Label, Menu ImageMenu, Vector2 Position, Vector2 Size, Vector2 AnchorMin, Vector2 AnchorMax, Sprite ImageSprite, Color ImageColor)
        {
            MenuImage IMG = Utils.NewMenuImage(Name, Label, FindItemRef("Image"), Position, Size, AnchorMin, AnchorMax, ImageMenu);
            IMG.Image = ImageSprite;
            IMG.color = ImageColor;
            return IMG;
        }
        MenuText NewText(string Name, string Text, Menu TextMenu, Vector2 Position, Vector2 Size, Vector2 AnchorMin, Vector2 AnchorMax, TextAlignmentOptions alignment, float FontSize, TMP_FontAsset fontAsset, Color TextColor)
        {
            MenuText text = Utils.NewMenuText(Name + "Text", Text, FindItemRef("Text"), Position, Size, AnchorMin, AnchorMax, TextMenu);
            text.Alignment = alignment;
            text.FontSize = FontSize;
            text.Font = fontAsset;
            text.color = TextColor;
            text.PrevColor = TextColor;
            return text;
        }
        MenuButton NewButton(string Name, string Text, Menu ButtonMenu, Vector2 Position, Vector2 Size, Vector2 AnchorMin, Vector2 AnchorMax, TextAlignmentOptions alignment, float FontSize, UnityAction[] Actions, float Transparency)
        {
            MenuButton button = Utils.NewMenuButton(Name + "Button", Text, FindItemRef("Button"), Position, Size, AnchorMin, AnchorMax, ButtonMenu, Actions);
            button.Alignment = alignment;
            button.FontSize = FontSize;
            button.Transparency = Transparency;
            button.AssignEvents();
            return button;
        }
        GameObject FindItemRef(string ItemName)
        {
            MenuItem item = System.Array.Find(Templates, MenuItem => MenuItem.Name == ItemName);
            return item.RefrenceObject;
        }
        Vector2 GetScreenSize()
        {
            return new Vector2(Screen.width, Screen.height);
        }
        Menu GetMenu(string MenuName)
        {
            return Menus.Find(Menu => Menu.Name == MenuName);
        }
        public bool AllMenusReady()
        {
            foreach (Menu M in Menus)
            {
                if(!M.FinishedSetup) { return false; }
            }

            return true;
        }
        public void FixGameSeed(int NewSeed)
        {
            if(GameManager.Instance.GameState != GameManager.GameStates.Init)
            {
                SettingsCategories[3].Settings[0].Object.transform.Find("ValueInputField").GetComponent<TMP_InputField>().text = NewSeed.ToString("0");
            }
        }

        public IEnumerator FadeInOut(float FadeDelay, float fadeSpeed, UnityAction FirstCallback, UnityAction LastCallback, System.Func<bool> CallbackHold, bool StartupFade)
        {
            FadeSpeed = fadeSpeed;
            IsFaded = true;
            AudioManager.Instance.InteractWithSFX((StartupFade) ? "Startup" : "Fade In", SoundEffectBehaviour.Play);
            yield return new WaitForSeconds(1f / FadeSpeed);
            yield return new WaitForSeconds(FadeDelay);
            FirstCallback();
            yield return null;
            yield return new WaitUntil(CallbackHold);
            if(LastCallback != null) { LastCallback(); }
            AudioManager.Instance.InteractWithSFX((StartupFade) ? "After Startup" : "Fade Out", SoundEffectBehaviour.Play);
            IsFaded = false;
        }
        public void Play()
        {
            StartCoroutine(EnterGame());
        }
        IEnumerator EnterGame()
        {
            SetMenu(GetMenu("Play"));
            yield return new WaitForSeconds(0.5f);
            GameManager.Instance.PlayIntro();
        }
        public void SetMenu(Menu menu)
        {
            PrevMenu = CurMenu;
            CurMenu = menu;
            PrevAnimTime = AnimTime;
        }
        public void QuitGame()
        {
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.ExitPlaymode();
            #endif
        }
    }
}