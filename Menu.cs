using Godot;
using System;
using System.Collections.Generic;

public class Menu : Node2D {

    // highscore and multipleyer row nodes
    public Control highscoreList;
    public ScrollContainer highscoreListContainer;
    public PackedScene hiRow = (PackedScene)GD.Load("res://elements/Menu/HighscoreRow.tscn");
    public Godot.Collections.Array hiRowLabels;
    public float hiRowY = 17;
    public int hiRowCount = 0;


    public StaticBody2D userGroup;


    public Global g;

    public RigidBody2D diffPrev, diffNext, postPrev, postNext,
        diffPrevMain, diffNextMain, postPrevMain, postNextMain;

    public MenuButton settingsButton, mpButton, spButton, highscoreButton, closeButton;

    public Node2D gameLabelGroup, activeWindow, settingsGroup;
    public AudioStreamPlayer input;




    public override void _Ready() {



        // binding global script
		g = (Global)GetNode("/root/Global");

        // different windows/menues
        gameLabelGroup = (Node2D)GetNode("GameLabelGroup");
        // local buttons
        userGroup = (StaticBody2D)GetNode("UserGroup");

        // lets try to instance a copy to the main node with all the bells and wistles
        settingsGroup = (Node2D)GetNode("SettingsGroup");

        activeWindow = gameLabelGroup;

        g.quickMenuContainer.Hide();
        g.main.Hide();
        activeWindow.Show();

        // sp button to lock it when the loading screen is visible
        spButton = (MenuButton)GetNode("Buttons/SpButton");

        // add, sort and defin buttons and their parameters
        foreach (Node n in this.GetNode("Buttons").GetChildren()) {
            n.Connect("input_event", n, "OnButtonPress");
            n.AddToGroup("Buttons");

            MenuButton nT = (n as MenuButton);

            nT.sprite = (n.GetNode("Sprite") as AnimatedSprite);
            nT.icon = n.GetNodeOrNull("Icon");

            if (n.Name == "CloseButton") {
                closeButton = nT;
                nT.Hide();
            }
        }


        // Settings
        // round length Menu
        foreach (Node n in this.GetNode("SettingsGroup/RoundLengthGroup").GetChildren()) {
            if (n.Name != "Label") {
                n.Connect("input_event", n, "OnButtonPress");
                n.AddToGroup("RoundDurations");
            }
        }
        // round length Main
        foreach (Node n in GetNode("/root/Main/QuickMenuContainer/QuickMenu/SettingsGroup/RoundLengthGroup").GetChildren()) {
            if (n.Name != "Label") {
                n.Connect("input_event", n, "OnButtonPress");
                n.AddToGroup("RoundDurationsMain");
            }
        }

        // difficulty Menu
        foreach (Node n in this.GetNode("SettingsGroup/DifficultyGroup/Container").GetChildren()) {
            if (n.Name != "Label") {
                n.Connect("input_event", n, "OnButtonPress");
                n.AddToGroup("Difficulty");
            }
        }
        // difficulty Main
        foreach (Node n in GetNode("/root/Main/QuickMenuContainer/QuickMenu/SettingsGroup/DifficultyGroup/Container").GetChildren()) {
            if (n.Name != "Label") {
                n.Connect("input_event", n, "OnButtonPress");
                n.AddToGroup("Difficulty");
            }
        }

        // post processing Menu
        foreach (Node n in this.GetNode("SettingsGroup/GraphicsGroup/Container").GetChildren()) {
            if (n.Name != "Label") {
                n.Connect("input_event", n, "OnButtonPress");
                n.AddToGroup("Graphics");
            }
        }
        // post processing Main
        foreach (Node n in GetNode("/root/Main/QuickMenuContainer/QuickMenu/SettingsGroup/GraphicsGroup/Container").GetChildren()) {
            if (n.Name != "Label") {
                n.Connect("input_event", n, "OnButtonPress");
                n.AddToGroup("Graphics");
            }
        }

        // arrows Menu
        diffPrev = (RigidBody2D)this.GetNode("SettingsGroup/DifficultyGroup/ArrowLeft");
        diffNext = (RigidBody2D)this.GetNode("SettingsGroup/DifficultyGroup/ArrowRight");
        postPrev = (RigidBody2D)this.GetNode("SettingsGroup/GraphicsGroup/ArrowLeft");
        postNext = (RigidBody2D)this.GetNode("SettingsGroup/GraphicsGroup/ArrowRight");
        // arrows Main
        diffPrevMain = (RigidBody2D)GetNode("/root/Main/QuickMenuContainer/QuickMenu/SettingsGroup/DifficultyGroup/ArrowLeft");
        diffNextMain = (RigidBody2D)GetNode("/root/Main/QuickMenuContainer/QuickMenu/SettingsGroup/DifficultyGroup/ArrowRight");
        postPrevMain = (RigidBody2D)GetNode("/root/Main/QuickMenuContainer/QuickMenu/SettingsGroup/GraphicsGroup/ArrowLeft");
        postNextMain = (RigidBody2D)GetNode("/root/Main/QuickMenuContainer/QuickMenu/SettingsGroup/GraphicsGroup/ArrowRight");


        // connecting input events Menu
        diffPrev.Connect("input_event", diffPrev, "OnButtonPress");
        diffNext.Connect("input_event", diffNext, "OnButtonPress");
        postPrev.Connect("input_event", postPrev, "OnButtonPress");
        postNext.Connect("input_event", postNext, "OnButtonPress");
        // input events Main
        diffPrevMain.Connect("input_event", diffPrevMain, "OnButtonPress");
        diffNextMain.Connect("input_event", diffNextMain, "OnButtonPress");
        postPrevMain.Connect("input_event", postPrevMain, "OnButtonPress");
        postNextMain.Connect("input_event", postNextMain, "OnButtonPress");


        userGroup.Connect("input_event", this, "ChangeUserName");




        // Multiplayer



        // doing the same with each multiplayer row
        // for (int i = 0; i < 3; i++) {
        //     mpRowCount++;
        //     GD.Print(mpRowCount % 2);
        //     Sprite mpRowInstance = (Sprite)mpRow.Instance();
        //     if (mpRowCount % 2 == 0) {

        //         (mpRowInstance as Sprite).SelfModulate = new Color(1f,1f,1f,0f);
        //     }
        //     mpRowInstance.Position = new Vector2(140f, mpRowY);

        //     mpList.RectMinSize = new Vector2(0f, mpRowY + 40f);

        //     mpRowLabels = mpRowInstance.GetChildren();
        //     (mpRowLabels[0] as Label).Text = mpRowCount.ToString();

        //     mpList.AddChild(mpRowInstance);
        //     mpRowY += 40f;
        // }
    }


    public void UpdateHighscores() {
        // Highscores
        highscoreListContainer = (ScrollContainer)GetNode("/root/Menu/HighscoreGroup/ScrollContainer");
        highscoreList = (Control)highscoreListContainer.GetNode("List");

        GD.Print("hsCount: " + g.main.highscoreList.Count);

        g.main.highscoreList.Sort((a, b) => -a.userScore.CompareTo(b.userScore));

        // adding each hiscore row
        for (int i = 0; i < g.main.highscoreList.Count; i++) {
            hiRowCount++;

            Sprite hiRowInstance = (Sprite)hiRow.Instance();
            if (hiRowCount % 2 == 0) {
                hiRowInstance.SelfModulate = new Color(1f,1f,1f,0f);
            }

            hiRowInstance.Position = new Vector2(140f, hiRowY);

            highscoreList.RectMinSize = new Vector2(0f, hiRowY + 40f);

            hiRowLabels = hiRowInstance.GetChildren();
            (hiRowLabels[0] as Label).Text = g.main.highscoreList[i].userScore + "p";
            (hiRowLabels[1] as Label).Text = g.main.highscoreList[i].userName;
            (hiRowLabels[2] as Label).Text = g.main.highscoreList[i].roundDur + "s";
            (hiRowLabels[3] as Label).Text = g.main.highscoreList[i].roundDifficulty.ToString();

            highscoreList.AddChild(hiRowInstance);
            hiRowY += 40f;
        }


    }

    public bool touchHold = false;
    public InputEventScreenTouch touchEvents;

    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {

        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
            g.InputPressed(this);
            input.Play();
        }

    }

    // public void OnButtonPress(Viewport viewport, InputEventScreenTouch ev, int shape_idx) {
    //     touchEvents = ev;
    //     g.InputPressed(this);

    //     if (ev is InputEventScreenTouch) {
    //         if (ev.Pressed) {
    //             touchHold = true;
    //         }

    //     }
    // }


    public override void _Process(float delta) {
    }




    public async void SwitchMenuWindow(Node2D tgtWindow) {
        Timer t = new Timer();
        t.OneShot = true;
        t.WaitTime = .2f;
        AddChild(t);
        t.Start();

        if (activeWindow.Name == "SettingsGroup") {
            (GetNode("Buttons/SettingsButton/Icon") as AnimatedSprite).Animation = "burger";
            closeButton.Hide();
        }

        if (activeWindow.Equals(tgtWindow)) {
            tgtWindow = gameLabelGroup;
            closeButton.Hide();
        }




        g.animPack.AlphaTween(activeWindow, 0f, .3f, true);


        activeWindow = tgtWindow;

        await ToSignal(t, "timeout");
        t.WaitTime = .3f;
        t.Start();

        activeWindow.Show();

        g.animPack.AlphaTween(tgtWindow, 1f, .4f);


        await ToSignal(t, "timeout");


        t.QueueFree();
    }



    public void ChangeUserName(Viewport viewport, InputEvent ev, int shape_idx) {
        // if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
        //     highscoreListContainer.MouseFilter = Godot.Control.MouseFilterEnum.Ignore;
        //     g.main.mpListContainer.MouseFilter = Godot.Control.MouseFilterEnum.Ignore;
        //     g.firstPress = true;
        //     g.promptControl.userNameInput.Text = g.userName;
        //     g.promptControl.Show();
        //     g.promptControl.closeButton.Show();
        //     g.promptControl.promptMessage.Text = "Change your\nname already?";
        //     g.animPack.AlphaTween(g.promptControl, 1f, .5f);
        // }
    }






}











  // public void OnCloseButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
    //     if (ev.IsActionPressed("on_pressed")) {
    //     }
    // }

    // public void OnSettingsButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
    //     if (ev.IsActionPressed("on_pressed")) {
    //         g.inputPressed = true;
    //         (settingsButton.GetNode("Sprite") as AnimatedSprite).Animation = "pressed";

    //         if (!activeWindow.Equals(settingsGroup)) {
    //             (settingsButton.GetNode("Icon") as AnimatedSprite).Animation = "cross";
    //             g.animPack.AlphaTween(activeWindow, 0f, .3f);

    //             activeWindow = settingsGroup;

    //             QuickTimer(settingsGroup, 1f, .1f);

    //         } else {
    //             (settingsButton.GetNode("Icon") as AnimatedSprite).Animation = "burger";
    //             g.animPack.AlphaTween(settingsGroup, 0f, .3f);


    //             activeWindow = gameLabelGroup;

    //             QuickTimer(gameLabelGroup, 1f, .1f);

    //         }
    //     }
    // }



// public void OnMpButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
    //     if (ev.IsActionPressed("on_pressed")) {
    //         g.inputPressed = true;
    //         (mpButton.GetNode("Sprite") as AnimatedSprite).Animation = "pressed";

    //         if (!activeWindow.Equals(mpGroup)) {
    //             (settingsButton.GetNode("Icon") as AnimatedSprite).Animation = "burger";
    //             g.animPack.AlphaTween(activeWindow, 0f, .3f);

    //             activeWindow = mpGroup;

    //             QuickTimer(mpGroup, 1f, .1f);

    //         }
    //     }
    // }


    // public void OnSpButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
    //     if (ev.IsActionPressed("on_pressed")) {
    //         g.inputPressed = true;
    //         (spButton.GetNode("Sprite") as AnimatedSprite).Animation = "pressed";
    //     }
    // }


    // public void OnHighscoreButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
    //     if (ev.IsActionPressed("on_pressed")) {
    //         g.inputPressed = true;
    //         (highscoreButton.GetNode("Sprite") as AnimatedSprite).Animation = "pressed";

    //         if (!activeWindow.Equals(highscoreGroup)) {
    //             (settingsButton.GetNode("Icon") as AnimatedSprite).Animation = "burger";
    //             g.animPack.AlphaTween(activeWindow, 0f, .3f);

    //             activeWindow = highscoreGroup;

    //             QuickTimer(highscoreGroup, 1f, .1f);

    //         }
    //     }
    // }



    // public void OnUserNamePress(Viewport viewport, InputEvent ev, int shape_idx) {
    //     if (ev.IsActionPressed("on_pressed")) {
    //         g.inputPressed = true;
    //     }
    // }








// Messy but there´s something here___________________________''


        // Sprite newRow = (Sprite)hiRow.Instance();
        // newRow.Name = "Row2";
        // highscoreList.AddChild(newRow);
        // newRow.Position = newRow.Position + new Vector2(0f, (float)(newRow.Texture.GetSize().y * .24) + 4f);
        // newRow.SelfModulate = new Color(1,1,1,0);
        // Godot.Collections.Array ss = newRow.GetChildren();

        // (ss[0] as Label).Text = "20p";
        // (ss[1] as Label).Text = "new Name";
        // (ss[2] as Label).Text = "20s";
        // (ss[3] as Label).Text = "VERYHARD";

        //_____________________________________________________________