using UnityEngine;
using BluePrinceArchipelago.Utils;
using System.Collections.Generic;
using static BluePrinceArchipelago.UI.UIManager;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Il2CppInterop.Runtime.InteropTypes.Arrays;







#if Bep
using TMPro;
using BluePrince.Inputs;
using BluePrince.UI;
using SummertimeMadness.Localization;
#endif
#if ML
using Il2Cpp;
using Il2CppTMPro;
using Il2CppBluePrince.Inputs;
using Il2CppBluePrince.UI;
using Il2CppSummertimeMadness.Localization;
#endif

namespace BluePrinceArchipelago.UI
{
    /// <summary>
    ///     A Manager that handles operations related to changing or adding in Game UI.
    /// </summary>
    public static class UIManager
    {

        public static GameObject ToggleTemplate { get; set; } = null;

        public static GameObject SliderTemplate { get; set; } = null;

        public static GameObject TextInputTemplate { get; set; } = null;
        public static void InitializeMainMenu()
        {
            GameObject MainMenu = GameObject.Find("MAIN MENU");
            GameObject Layout = MainMenu.transform.Find("MAINMENU LAYOUT").gameObject;
            GameObject NewGame = Layout.transform.Find("New Game Click").gameObject;
            GameObject Continue = Layout.transform.Find("Continue Click").gameObject;
            GameObject Profiles = Layout.transform.Find("Profiles Click").gameObject;
            GameObject Settings = Layout.transform.Find("Settings Click").gameObject;
            GameObject Quit = Layout.transform.Find("Quit Click").gameObject;
            //float MainMenuSpacingY = 0.07f; //The default spacing for the main menu. Continue and new game are at the same position.
            //float MaxY = 1.1072f;
            //float CenterY = 0.9572f;
            //float MinY = 0.8072f;
            // -8 -> 6
            // Center -1.16
            ControllerSelector LayoutSelector = Layout.GetComponent<ControllerSelector>();
            
            GameObject ApSettings = GameObject.Instantiate(Settings, new Vector3(-0.2591f, 0.8822f, -9.2206f), Settings.transform.rotation, Settings.transform.parent);
            ApSettings.SetName("AP Settings Click");
            
            GameObject ApSettingsText = ApSettings.transform.Find("SETTINGS TEXT").gameObject;
            ApSettingsText.SetName("AP SETTINGS TEXT");
            TextMeshPro APSettingsTMPro = ApSettingsText.GetComponent<TextMeshPro>();
            APSettingsTMPro.text = "AP Settings";

            GetTemplates();
        }
        public static void GetTemplates() {
            GameObject ASM = GameObjectExtensions.GetRootGameObject("ACCESSIBILITY SUBMENU");
            GameObject ASMs = ASM.transform.FindChild("ACCESSIBILITY SUBMENUS").gameObject;
            GameObject MAS = ASMs.transform.Find("MOTION ASSIST SETTINGS").gameObject;
            GameObject Toggle = MAS.transform.Find("Toggle (Smooth_look)").gameObject;
            ToggleTemplate = GameObject.Instantiate(Toggle, ModInstance.Instance.transform);
            ToggleTemplate.name = "Toggle";
            ToggleTemplate.transform.Find("Toggle Label").Find("Headline Text 1").GetComponent<TextMeshPro>().text = "Default";
            ToggleTemplate.transform.Find("Toggle (Smooth_look)").gameObject.name = "Toggle";
            //TODO Edit the FSMs

            GameObject Slider = MAS.transform.Find("Slider (Look_sensitivity)").gameObject;
            SliderTemplate = GameObject.Instantiate(Slider, ModInstance.Instance.transform);
            SliderTemplate.name = "Slider";
            SliderTemplate.transform.Find("Sens label new").Find("Headline Text 1").GetComponent<TextMeshPro>().text = "Default";
            SliderTemplate.transform.Find("Sens label new").gameObject.name = "Slider Label";
            SliderTemplate.transform.Find("Look sens Slider").gameObject.name = "Slider";
            //TODO Edit the FSMs

            TextInputTemplate = CreateTextInputTemplate();
            TextInputTemplate.transform.parent = ModInstance.Instance.transform;

            CreateOptionsPage("Test",
                new Page("Test", "Test",
                    [
                        [new PageOption("Test Checkbox", new ToggleOptions())],
                        [new PageOption("Test TextInput", new TextInputOptions())],
                   ]
                ));
        }

        public static void ChangeMenuLayout() {
            
            Logging.LogWarning("Attempting to Change Menu Layout");
            GameObject MainMenu = GameObject.Find("MAIN MENU");
            GameObject Layout = MainMenu.transform.Find("MAINMENU LAYOUT").gameObject;
            GameObject NewGame = Layout.transform.Find("New Game Click").gameObject;
            GameObject Continue = Layout.transform.Find("Continue Click").gameObject;
            GameObject Profiles = Layout.transform.Find("Profiles Click").gameObject;
            GameObject Settings = Layout.transform.Find("Settings Click").gameObject;
            GameObject Quit = Layout.transform.Find("Quit Click").gameObject;
            NewGame.transform.position = new Vector3(-0.2591f, 1.0922f, -9.2206f);
            Continue.transform.position = new Vector3(-0.2591f, 1.0922f, -9.2206f);
            Profiles.transform.position = new Vector3(-0.2591f, 1.0222f, -9.2206f);
            Settings.transform.position = new Vector3(-0.2591f, 0.9522f, -9.2206f);
            Quit.transform.position = new Vector3(-0.2591f, 0.8122f, -9.2206f);
            ControllerSelector LayoutSelector = Layout.GetComponent<ControllerSelector>();

            GameObject APSettings = Layout.transform.Find("AP Settings Click").gameObject;
            PlayMakerFSM APSettingsFSM = APSettings.GetComponent<PlayMakerFSM>();
            ControllerSelectable APSettingsCS = APSettings.GetComponent<ControllerSelectable>();
            LayoutSelector.Selectables = LayoutSelector.Selectables.InsertIfUnique(APSettingsCS, 4);

            APSettingsCS.TargetFSM = APSettingsFSM;

            
        }

        public static GameObject CreateOptionsPage(string PageName, Page page)
        {
            // Create a New Page and set it to be inactive
            GameObject Page = new GameObject(PageName);
            // Create the Page Title and set the Title Position.
            GameObject PageTitle = CreateTMProText("Page Title", page.Title);
               
            PageTitle.name = "Page Title";
            PageTitle.transform.position = new Vector3(0.01f, -996.4f, 9.01f);
            PageTitle.transform.SetParent(Page.transform);
            // Create the Selector Object and set it's parent as the Page
            GameObject Selector = new GameObject($"{PageName} Selector");
            Selector.transform.parent = Page.transform;

            // Add the ControllerSelector component, so Controller and arrow key Navigation works.
            ControllerSelector SelectorCS = Selector.AddComponent<ControllerSelector>();
            SelectorCS.Selectables = new Il2CppReferenceArray<ControllerSelectable>(0);
            // Find a Black Background and copy it to the page.
            GameObject Graphics = GameObjectExtensions.GetRootGameObject("GRAPHICS");
            GameObject BlackFade = GameObject.Instantiate(Graphics.transform.Find("BLACK FADE").gameObject, Page.transform);
            BlackFade.name = "Black Fade";
            // Iterate through the rows of page options
            float heightStep = 1.0f;
            int rownum = 0;
            foreach (List<PageOption> PageOption in page.Layout) { 

                int numOptions = PageOption.Count;
                float start = -2.0f;
                float step = 0.0f;

                // Currently a Max of 3 per row;
                if (numOptions < 3)
                {

                    if (numOptions == 3)
                    {
                        start = -6.0f;
                        step = 4.0f;
                    }
                    else if (numOptions == 2)
                    {
                        start = -3f;
                        step = 4.0f;
                    }
                    int optionNum = 0;

                    // Foreach option in the row, create it parent it to the Page, set it's relative position, and then add a controller Selectable component.
                    foreach (PageOption Option in PageOption)
                    {
                        if (Option.InputType == "Slider")
                        {
                            GameObject Slider = CreateSlider(Option.Label, Option.Label, Option.SliderOptions);
                            Slider.transform.parent = Page.transform;
                            Slider.transform.position = new Vector3(start + step*optionNum, -997 -(heightStep * rownum), 9.00f);
                            SelectorCS.RegisterSelectable(Slider.transform.Find("Slider").GetComponent<ControllerSelectable>());
                        }
                        else if (Option.InputType == "Toggle") {
                            GameObject Toggle = CreateToggle(Option.Label, Option.Label, Option.ToggleOptions);
                            Toggle.transform.parent = Page.transform;
                            Toggle.transform.position = new Vector3(start + step * optionNum, -997 - (heightStep * rownum), 9.00f);
                            SelectorCS.RegisterSelectable(Toggle.transform.Find("Toggle").GetComponent<ControllerSelectable>());
                        }
                        else if (Option.InputType == "TextInput")
                        {
                            GameObject TextInput = CreateTextInput(Option.Label, Option.Label, Option.TextInputOptions);
                            TextInput.transform.parent = Page.transform;
                            TextInput.transform.position = new Vector3(start + step * optionNum, -997 - (heightStep * rownum), 9.00f);
                            SelectorCS.RegisterSelectable(TextInput.transform.Find("TextInput").GetComponent<ControllerSelectable>());
                        }
                        optionNum++;
                    }
                }
                rownum++;
                
            }
            GameObject.Instantiate(BlackFade, Page.transform);
            return Page;
        }

        // TODO Edit FSM appropriately. Add Hooks for when options change.
        private static GameObject CreateSlider(string name, string label, SliderOptions options) {
            GameObject NewSlider = GameObject.Instantiate(SliderTemplate);
            NewSlider.name = name;
            NewSlider.transform.Find("Slider Label").Find("Headline Text 1").GetComponent<TextMeshPro>().text = label;
            GameObject.Destroy(NewSlider.transform.Find("Slider Label").Find("Headline Text 1").GetComponent<LocalizedText>());
            UISlider SliderComponent = NewSlider.transform.Find("Slider").gameObject.GetComponent<UISlider>();
            SliderComponent.minimum = options.Min;
            SliderComponent.maximum = options.Max;
            SliderComponent.NavigationStepIncrement = options.Step;
            SliderComponent.currentValue = options.InitValue;
            SliderComponent.FormatValue = options.Format;
            return NewSlider;
        }

        // TODO Edit FSM Appropriately. Add Hooks for when options change.
        private static GameObject CreateToggle(string name, string label, ToggleOptions options) { 
            GameObject NewToggle = GameObject.Instantiate(ToggleTemplate);
            NewToggle.name = name;
            NewToggle.transform.Find("Toggle Label").Find("Headline Text 1").GetComponent<TextMeshPro>().text = label;
            GameObject.Destroy(NewToggle.transform.Find("Toggle Label").Find("Headline Text 1").GetComponent<LocalizedText>());
            UIToggle ToggleComponent = NewToggle.transform.Find("Toggle").gameObject.GetComponent<UIToggle>();
            ToggleComponent.IsOn = options.InitValue;
            return NewToggle;
        }

        // TODO Add Component Initialization. Add Custom Fsms or Actions. Add Hooks for when Values change.
        private static GameObject CreateTextInput(string name, string label, TextInputOptions options) {
            GameObject NewTextInput = GameObject.Instantiate(TextInputTemplate);
            NewTextInput.name = name;
            NewTextInput.transform.Find("TextInput Label").Find("Headline Text 1").GetComponent<TextMeshPro>().text = label;
            GameObject.Destroy(NewTextInput.transform.Find("TextInput Label").Find("Headline Text 1").GetComponent<LocalizedText>());
            TMP_InputField TextInputField = NewTextInput.transform.Find("TextInput Box").GetComponent<TMP_InputField>();
            TextInputField.text = options.InitValue;
            TextInputField.caretColor = Color.white;
            TextMeshProUGUI Placeholder = TextInputField.placeholder.GetComponent<TextMeshProUGUI>();
            Placeholder.text = options.InitValue;
            Placeholder.color = Color.white;
            return NewTextInput;
        }

        private static GameObject CreateTextInputTemplate() {
            GameObject TextInput = new GameObject("TextInput");
            TextInput.AddComponent<RectTransform>();
            TextInput.AddComponent<MeshRenderer>();
            GameObject TextInputText = new GameObject("TextInput Label");
            TextInputText.transform.parent = TextInput.transform;
            TextInputText.AddComponent<RectTransform>();
            GameObject TextInputHeadlineText1 = GameObject.Instantiate(ToggleTemplate.transform.Find("Toggle Label").Find("Headline Text 1").gameObject, TextInputText.transform);
            TextInputHeadlineText1.name = "Headline Text 1";
            GameObject TextInputField = new GameObject("TextInput");
            TextInputField.transform.parent = TextInput.transform;
            ControllerSelectable TextInputFieldCS = TextInputField.AddComponent<ControllerSelectable>();
            UITextInput TextInputUI = TextInputField.AddComponent<UITextInput>();
            
            GameObject TextInputBox = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            TextInputBox.name = "TextInput Box";
            TextInputBox.transform.parent = TextInput.transform;
            TextInputBox.transform.localPosition += new Vector3(2, 0);
            TextInputUI.InputField = TextInputBox.GetComponent<TextMeshPro>();
            TMP_InputField TextInputTMP = TextInputBox.GetComponent<TMP_InputField>();
            TextInputTMP.placeholder.GetComponent<TextMeshProUGUI>().text = "Placeholder";
            TextInputTMP.placeholder.GetComponent<TextMeshProUGUI>().fontSize = 18;
            TextInputTMP.placeholder.GetComponent<TextMeshProUGUI>().fontSize = 18;
            TextInputTMP.placeholder.GetComponent<RectTransform>().anchorMax = new(1, 1.4f);
            TextInputTMP.textComponent.GetComponent<RectTransform>().anchorMax = new(1, 1.4f);
            TextInputTMP.enabled = false;
            TextInputTMP.enabled = true;
            return TextInput;
        }

        private static GameObject CreateTMProText(string objName, string text, TextAlignmentOptions alignment = TextAlignmentOptions.Center) {
            GameObject TextGO = TMP_DefaultControls.CreateText(new TMP_DefaultControls.Resources());
            TextMeshProUGUI GOTextMeshPro = TextGO.GetComponent<TextMeshProUGUI>();

            TextMeshPro TMPro = TextGO.AddComponent<TextMeshPro>();
            TextGO.name = objName;
            GOTextMeshPro.text = text;
            GOTextMeshPro.alignment = alignment;
            return TextGO;
        }


        public struct Page {
            public string Name = "";
            public string Title = "";

            // A 2D array that handles What options will be on the same row, and the order of options on the page.
            public List<List<PageOption>> Layout;

            public Page(string name, string title = "", List<List<PageOption>> layout = null) {
                if (layout == null)
                {
                    Layout = new List<List<PageOption>>();
                }
                else {
                    Layout = layout;
                }
                if (title == "")
                {
                    Title = name;
                }
                else {
                    Title = title;
                }
                Name = name;
            }
        }
        public struct PageOption {

            // The Label for the option.
            public string Label;

            // The MonoBehavior for the input of the option
            public SliderOptions SliderOptions;

            public ToggleOptions ToggleOptions;

            public TextInputOptions TextInputOptions;

            // The type of the input so it can be properly cast to the correct type.
            public string InputType;

            public PageOption(string label, SliderOptions sliderOptions) { 
                Label = label;
                InputType = "Slider";
                SliderOptions = sliderOptions;
            }
            public PageOption(string label, ToggleOptions toggleOptions) {
                Label = label;
                InputType = "Toggle";
                ToggleOptions = toggleOptions;
            }

            public PageOption(string label, TextInputOptions textInputOptions) {
                Label = label;
                InputType = "TextInput";
                TextInputOptions = textInputOptions;
            }

        }

        public struct SliderOptions {
            public float Min;
            public float Max;
            public float Step;
            public float InitValue;
            public string Format;
            public SliderOptions(float min, float max, float step, float initValue, string format) {
                Min = min;
                Max = max;
                Step = step;
                InitValue = initValue;
                Format = format;
            }
        }

        public struct ToggleOptions {
            public bool InitValue = false;

            public ToggleOptions(bool initValue) {
                InitValue = initValue;
            }
        }

        public struct TextInputOptions {
            public string InitValue;
            public TextInputOptions(string initValue)
            {
                InitValue = initValue;
            }
        }
    }

    public class UITextInput() : MonoBehaviour
    {

        public delegate void StringDelegate(string s);

        public TextMeshPro InputField;

        public StringDelegate OnStringChanged;

        public Collider ClickArea;

        private UISelectableGraphics[] _childGraphics;

        private bool _isSelected;

        private string _lastValue;

        private string currentValue;

        public string Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                if (InputField != null)
                {
                    InputField.text = value;
                }
            }
        }

        private void Awake()
        {
            _childGraphics = GetComponentsInChildren<UISelectableGraphics>();
        }
        private void Update() {
            if (InputField != null)
            {
                if (currentValue != InputField.text)
                {
                    OnParametersChange();
                    OnStringChanged?.Invoke(currentValue);
                }
            }
        }

        private void OnEnable()
        {
            OnParametersChange();
        }
        private void OnParametersChange()
        {
            _lastValue = currentValue;
            if (InputField != null)
            {
                currentValue = InputField.text;
            }
        }
        public void Select()
        {
            _isSelected = true;
            if (_childGraphics != null)
            {
                UISelectableGraphics[] childGraphics = _childGraphics;
                foreach (UISelectableGraphics uISelectableGraphics in childGraphics)
                {
                    if (uISelectableGraphics != null)
                    {
                        uISelectableGraphics.SetHover();
                    }
                }
            }
        }

        public void Unselect()
        {
            _isSelected = false;
            if (_childGraphics != null)
            {
                UISelectableGraphics[] childGraphics = _childGraphics;
                foreach (UISelectableGraphics uISelectableGraphics in childGraphics)
                {
                    if (uISelectableGraphics != null)
                    {
                        uISelectableGraphics.SetNormal();
                    }
                }
            }
        }
    }
}