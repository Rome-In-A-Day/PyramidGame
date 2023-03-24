using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class MenuManager : VisualElement
{
    PyramidGame game;
    #region Screens
    //Main Menu
    VisualElement m_MainScreen;
    VisualElement b_Start;
    VisualElement b_Options;
    VisualElement b_Exit;
    // In Game
    VisualElement b_Menu;
    VisualElement b_Back;
    VisualElement b_InGameExit;
    VisualElement b_Restart;
    // End Game
    TextElement t_EndGameTxt;
    VisualElement b_ToMainMenu;    
    #endregion

    #region UI Titles
    // Main Menu Elements
    string Main_TitleScreen = "TitleScreen";
    string Main_Start_Btn = "Start";
    string Main_Options_Btn = "Options";
    string Main_Exit_Btn = "Exit";
    // In Game Elements
    string InGame_Screen = "GameScreen";
    string InGame_MenuBtn = "MenuButton";
    string InGame_Menu = "InGameMenu";
    string InGame_Menu_Back = "Back";
    string InGame_Menu_InGameRestart = "InGameRestart";
    string InGame_Menu_Exit = "InGameExit";
    // End Game Elements
    string End_Screen = "EndScreen";
    string End_Text = "EndGameTxt";
    string End_ToMMBtn = "ToMainMenuButton";
    #endregion

    public new class UxmlFactory : UxmlFactory<MenuManager, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits { }


    public MenuManager() {
        game = PyramidGame.FindObjectOfType<PyramidGame>();
        game.GameEndEvent.AddListener(this.GameEnd);
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        this.pickingMode = PickingMode.Ignore; // Prevents the High lvl container from stoping clicks        
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        DisableAll();
        this.Q(Main_TitleScreen).style.display = DisplayStyle.Flex;
        this.Q(InGame_Menu).style.display = DisplayStyle.None;

        this.Q(InGame_Screen).pickingMode = PickingMode.Ignore;
        // Assign Main Screen Elements
        m_MainScreen = this.Q(Main_TitleScreen);
        b_Start = this.Q(Main_Start_Btn);
        b_Options = this.Q(Main_Options_Btn);
        b_Exit = this.Q(Main_Exit_Btn);
        //In Game Elements
        b_Menu = this.Q(InGame_MenuBtn);
        b_Back = this.Q(InGame_Menu_Back);
        b_InGameExit = this.Q(InGame_Menu_Exit);
        b_Restart = this.Q(InGame_Menu_InGameRestart);
        // End Game Elements
        b_ToMainMenu = this.Q(End_ToMMBtn);
        t_EndGameTxt = this.Q<TextElement>(End_Text);


        //Events for Title Screen
        b_Start?.RegisterCallback<ClickEvent>(ev => StartClicked());
        m_MainScreen?.RegisterCallback<TransitionEndEvent>(ev => MainScreenTransition());
        b_Exit?.RegisterCallback<ClickEvent>(ev => ExitGame());
        b_Options.SetEnabled(false);
        b_Options.RegisterCallback<MouseEnterEvent>((evt) => {
            evt.PreventDefault();
            evt.StopImmediatePropagation();
            b_Options.RemoveFromClassList("titleButton:hover");
        }, TrickleDown.TrickleDown);


        // In Game Events
        b_Menu?.RegisterCallback<ClickEvent>(ev => ToInGameMenu());
        b_Back?.RegisterCallback<ClickEvent>(ev => InGameMenuBack());
        b_InGameExit?.RegisterCallback<ClickEvent>(ev => InGameMenuExit());
        b_Restart?.RegisterCallback<ClickEvent>(ev => RestartGame());

        // End Game Events
        b_ToMainMenu?.RegisterCallback<ClickEvent>(ev => ToMainMenu());
    }

    private void RestartGame()
    {
        game.Reset();
        this.Q(InGame_Screen).pickingMode = PickingMode.Ignore;
        this.Q(InGame_Menu).style.display = DisplayStyle.None;
    }

    private void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    private void GameEnd(string endGameTxt)
    {
        DisableAll();
        this.Q(End_Screen).style.display = DisplayStyle.Flex;
        t_EndGameTxt.text = endGameTxt;

    }

    private void InGameMenuExit()
    {
        DisableAll();
        game.Reset();
        this.Q(InGame_Screen).pickingMode = PickingMode.Ignore;
        this.Q(InGame_Menu).style.display = DisplayStyle.None;
        this.Q(Main_TitleScreen).style.display = DisplayStyle.Flex;
        
    }

    private void InGameMenuBack()
    {
        DisableAll();
        this.Q(InGame_Screen).pickingMode = PickingMode.Ignore;
        this.Q(InGame_Menu).style.display = DisplayStyle.None;
        this.Q(InGame_Screen).style.display= DisplayStyle.Flex;        
    }

    private void ToInGameMenu()
    {
        DisableAll();
        this.Q(InGame_Screen).pickingMode = PickingMode.Position;
        this.Q(InGame_Screen).style.display = DisplayStyle.Flex;
        this.Q(InGame_Menu).style.display = DisplayStyle.Flex;
    }

    // Main Menu Handeling
    private void StartClicked()
    {        
        DisableAll();
        this.Q(Main_TitleScreen).style.display = DisplayStyle.Flex;
        this.Q(Main_TitleScreen).AddToClassList("fadeOut");
    }
    private void MainScreenTransition()
    {
        this.Q(Main_TitleScreen).style.display = DisplayStyle.None;
        this.Q(InGame_Screen).style.display = DisplayStyle.Flex;
        this.Q(Main_TitleScreen).RemoveFromClassList("fadeOut");
    }

    private void OptionsClicked()
    {

    }
    
    private void DisableAll()
    {
        this.Q(Main_TitleScreen).style.display = DisplayStyle.None;
        this.Q(InGame_Screen).style.display = DisplayStyle.None;
        this.Q(End_Screen).style.display = DisplayStyle.None;
    }

    private void ToMainMenu()
    {
        DisableAll();        
        this.Q(Main_TitleScreen).style.display = DisplayStyle.Flex;
        game.Reset();
    }

}
