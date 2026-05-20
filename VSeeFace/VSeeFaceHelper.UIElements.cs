using UnityEngine;

namespace rfPlugin.VSeeFace.UI;

/***
      Structure of classes does not 100% match the hierarchy of VSeeFace objects!
      Only grabbed things that were of interest to me for now.
      Wrote all this darn junk by hand, I don't use AIs :3
***/

public class LaunchUI
{
    public GameObject startButton;
    public StartingText StartingText;
    public GameObject selectionUI;
    public GameObject localization;
    public GameObject avatarSelectButton;
    public GameObject avatarCancelButton;
    public GameObject avatarRemoveButton;
    public GameObject avatarInformationUI;
    public GameObject avatarScrollView;
    public GameObject vroidHub;
    public GameObject vroidInformationUI;
    public GameObject avatarSourceToggle;

    public LaunchUI()
    {
        startButton = GameObject.Find("VSeeFace/Canvas/UI/StartUI/StartButton");
        RfPlugin.LogGameObject("UI LaunchUI startButton found:", startButton);

        StartingText = new();

        selectionUI = GameObject.Find("VSeeFace/Canvas/UI/StartUI/SelectionUI");
        RfPlugin.LogGameObject("UI LaunchUI selectionUI found:", selectionUI);

        localization = GameObject.Find("VSeeFace/Canvas/UI/StartUI/Localization");
        RfPlugin.LogGameObject("UI LaunchUI localization found:", localization);

        avatarSelectButton = GameObject.Find("VSeeFace/Canvas/UI/AvatarSelection/SelectButton");
        RfPlugin.LogGameObject("UI LaunchUI avatarSelectButton found:", avatarSelectButton);

        avatarCancelButton = GameObject.Find("VSeeFace/Canvas/UI/AvatarSelection/CancelButton");
        RfPlugin.LogGameObject("UI LaunchUI avatarCancelButton found:", avatarCancelButton);

        avatarRemoveButton = GameObject.Find("VSeeFace/Canvas/UI/AvatarSelection/RemoveAvatarButton");
        RfPlugin.LogGameObject("UI LaunchUI avatarRemoveButton found:", avatarRemoveButton);

        avatarInformationUI = GameObject.Find("VSeeFace/Canvas/UI/AvatarSelection/InformationUI");
        RfPlugin.LogGameObject("UI LaunchUI avatarInformationUI found:", avatarInformationUI);

        avatarScrollView = GameObject.Find("VSeeFace/Canvas/UI/AvatarSelection/AvatarScrollView");
        RfPlugin.LogGameObject("UI LaunchUI avatarScrollView found:", avatarScrollView);

        vroidHub = GameObject.Find("VSeeFace/Canvas/UI/AvatarSelection/VRoidHub");
        RfPlugin.LogGameObject("UI LaunchUI vroidHub found:", vroidHub);

        vroidInformationUI = GameObject.Find("VSeeFace/Canvas/UI/AvatarSelection/VRoidInformationUI");
        RfPlugin.LogGameObject("UI LaunchUI vroidInformationUI found:", vroidInformationUI);

        avatarSourceToggle = GameObject.Find("VSeeFace/Canvas/UI/AvatarSelection/AvatarSourceToggle");
        RfPlugin.LogGameObject("UI LaunchUI avatarSourceToggle found:", avatarSourceToggle);

    }
}

// member of LaunchUI
public class StartingText
{
    public GameObject startingText;
    public GameObject logo;
    public GameObject title;
    public GameObject modelCredits;
    public GameObject credits;

    public StartingText()
    {
        startingText = GameObject.Find("VSeeFace/Canvas/UI/StartingText");
        RfPlugin.LogGameObject("UI StartingText startingText found:", startingText);

        logo = GameObject.Find("VSeeFace/Canvas/UI/StartingText/Logo");
        RfPlugin.LogGameObject("UI StartingText logo found:", logo);

        title = GameObject.Find("VSeeFace/Canvas/UI/StartingText/Title (TMP)");
        RfPlugin.LogGameObject("UI StartingText title found:", title);

        modelCredits = GameObject.Find("VSeeFace/Canvas/UI/StartingText/Model credits (TMP)");
        RfPlugin.LogGameObject("UI StartingText modelCredits found:", modelCredits);

        credits = GameObject.Find("VSeeFace/Canvas/UI/StartingText/Credits (TMP)");
        RfPlugin.LogGameObject("UI StartingText credits found:", credits);
    }
}

public class MainUI
{
    public GameObject mainUI;
    public GameObject title;
    public GameObject menuRight;
    public Settings Settings;
    public GameObject propsWindow;
    public GameObject propsWindowScrollView;
    public GameObject propsWindowScrollViewContent;
    public GameObject propSettings;
    public GameObject propDragText;
    public GameObject perfMonitor;
    public GameObject pointsView;
    public GameObject help;
    public GameObject hideUI;

    public MainUI()
    {
        mainUI = GameObject.Find("VSeeFace/Canvas/UI/MainUI");
        RfPlugin.LogGameObject("UI MainUI mainUI found:", mainUI);

        title = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Title (TMP)");
        RfPlugin.LogGameObject("UI MainUI title found:", title);

        menuRight = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Menu right");
        RfPlugin.LogGameObject("UI MainUI menuRight found:", menuRight);

        Settings = new();
        
        propsWindow = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/Props Window");
        RfPlugin.LogGameObject("UI MainUI propsWindow found:", propsWindow);
        
        propsWindowScrollView = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/Props Window/Scroll View");
        RfPlugin.LogGameObject("UI MainUI propsWindowScrollView found:", propsWindowScrollView);
        
        propsWindowScrollViewContent = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/Props Window/Scroll View/Viewport/Content");
        RfPlugin.LogGameObject("UI MainUI propsWindowScrollViewContent found:", propsWindowScrollViewContent);

        propSettings = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/Prop Settings");
        RfPlugin.LogGameObject("UI MainUI propSettings found:", propSettings);

        propDragText = GameObject.Find("VSeeFace/Canvas/UI/MainUI/PropDragText");
        RfPlugin.LogGameObject("UI MainUI propDragText found:", propDragText);

        perfMonitor = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Perf monitor");
        RfPlugin.LogGameObject("UI MainUI perfMonitor found:", perfMonitor);

        pointsView = GameObject.Find("VSeeFace/Canvas/UI/MainUI/PointsView");
        RfPlugin.LogGameObject("UI MainUI pointsView found:", pointsView);

        help = GameObject.Find("VSeeFace/Canvas/UI/Help");
        RfPlugin.LogGameObject("UI MainUI help found:", help);

        hideUI = GameObject.Find("VSeeFace/Canvas/UI/HideUI");
        RfPlugin.LogGameObject("UI MainUI hideUI found:", hideUI);

    }
}

// member of MainUI
public class Settings
{
    public GameObject settings;
    public GameObject general;
    public GameObject generalBtn;
    public GameObject expression;
    public GameObject expressionBtn;
    public GameObject light;
    public GameObject lightBtn;
    public GameObject effects;
    public GameObject effectsBtn;
    public GameObject leapMotion;
    public GameObject leapMotionBtn;

    public Settings()
    {
        settings = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings");
        RfPlugin.LogGameObject("UI Settings settings found:", settings);

        general = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/General settings");
        RfPlugin.LogGameObject("UI Settings general found:", general);

        expression = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/Expression settings");
        RfPlugin.LogGameObject("UI Settings expression found:", expression);

        light = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/Light settings");
        RfPlugin.LogGameObject("UI Settings light found:", light);

        effects = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/Effect settings");
        RfPlugin.LogGameObject("UI Settings effects found:", effects);

        leapMotion = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Settings/Leap Motion settings");
        RfPlugin.LogGameObject("UI Settings leapMotion found:", leapMotion);


        generalBtn = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Menu right/Settings/Hide settings/General settings");
        RfPlugin.LogGameObject("UI Settings generalBtn found:", generalBtn);

        expressionBtn = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Menu right/Settings/Hide settings/Expression settings");
        RfPlugin.LogGameObject("UI Settings expressionBtn found:", expressionBtn);

        lightBtn = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Menu right/Settings/Hide settings/Light settings");
        RfPlugin.LogGameObject("UI Settings lightBtn found:", lightBtn);

        effectsBtn = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Menu right/Settings/Hide settings/Effect settings");
        RfPlugin.LogGameObject("UI Settings effectsBtn found:", effectsBtn);

        leapMotionBtn = GameObject.Find("VSeeFace/Canvas/UI/MainUI/Menu right/Settings/Hide settings/Leap Motion settings");
        RfPlugin.LogGameObject("UI Settings leapMotionBtn found:", leapMotionBtn);
    }
}
