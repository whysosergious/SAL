using Godot;
using System;
using System.IO;


//**** I dislike that Godot forces you to inherit classes from Nodes
//**** making it almost impossible to create shared partial classes..
//**** it'll have to do for now..
public partial class Global : Node {

    public PackedScene loadingControl = (PackedScene)GD.Load("res://elements/Menu/LoadingControl.tscn");

    // Load menu
    // public PackedScene menu = (PackedScene)GD.Load("res://Menu.tscn");

    // current directory
    string curDir = System.AppDomain.CurrentDomain.BaseDirectory;

    // easy access standards
    public float stressTick = 9f;

    // input values
    public bool inputPressed = false;

    // variable names for elements and nodes
    public Menu menu;
    public Main main;
    public StartRoundBtn startRoundBtn;
    public Label roundTimerLabel,
        userScoreLabel, oppScoreLabel,
            userGroupLabel, oppNameLabel;
    public StaticBody2D userGroup;
    public AnimPack animPack;
    public Node2D userScoreOrigin, oppScoreOrigin, oppPolygons, simonBtn, loadingInstance, loadingScreen;
    public Area2D touchMain, touchMenu, userPolygons;

    public PromptControl promptControl;
    public QuickMenuContainer quickMenuContainer;
    public Godot.Collections.Array difficultyButtons, difficultyArrows, postButtons, postArrows, durationButtons,
        difficultyButtonsMain, difficultyArrowsMain, postButtonsMain, postArrowsMain, durationButtonsMain;
    public Vector2 difficultyButtonsStandardPos, postButtonsStandardPos,
        difficultyButtonsStandardPosMain, postButtonsStandardPosMain;

    public bool mpGame = false;

    // get name from device Later
    public string userName = "dirtyPoodle";


    public override void _Ready() {

        loadingInstance = (Node2D)loadingControl.Instance();
        loadingInstance.Position = new Vector2(0f, 0f);
        loadingInstance.ZIndex = 7;

        GetNode("/root/").CallDeferred("add_child", loadingInstance);
        loadingScreen = (Node2D)loadingInstance.GetNode("CanvasLayer/LoadingScreen");



        // binding elements and nodes
        main = (Main)GetNode("/root/Main");
        menu = (Menu)GetNode("/root/Menu");
        touchMain = (Area2D)GetNode("/root/Main/MainTouch");
        touchMenu = (Area2D)GetNode("/root/Menu/MenuTouch");
        startRoundBtn = (StartRoundBtn)GetNode("/root/Main/StartRoundBtn");
        promptControl = (PromptControl)GetNode("/root/Menu/NamePromptContainer");
        quickMenuContainer = (QuickMenuContainer)GetNode("/root/Main/QuickMenuContainer");
        userGroup = (StaticBody2D)menu.GetNode("UserGroup");
        // labels
        userGroupLabel = (Label)userGroup.GetNode("Label");
        roundTimerLabel = (Label)GetNode("/root/Main/LabelGroup/RoundTimerLbl");
        userScoreLabel = (Label)GetNode("/root/Main/LabelGroup/UserScoreLbl");
        oppScoreLabel = (Label)GetNode("/root/Main/LabelGroup/OppScoreLbl");
        userScoreOrigin = (Node2D)GetNode("/root/Main/LabelGroup/UserScoreOrigin");
        oppScoreOrigin = (Node2D)GetNode("/root/Main/LabelGroup/OppScoreOrigin");
        userPolygons = GetNode<Area2D>("/root/Main/userPolygonGroup");
        oppPolygons = GetNode<Node2D>("/root/Main/OppPolygonGroup");

        // decorative
        oppNameLabel = (Label)GetNode("/root/Main/OppNameGroup/Label");


        // animation playes
        animPack = (AnimPack)GetNode("/root/Main/AnimPack");


        // instancing variables
        stressTickCount = stressTick;

        // collections
        // settings
        // difficulty group Menu
        difficultyButtons = menu.GetNode("SettingsGroup/DifficultyGroup/Container").GetChildren();
        difficultyArrows = menu.GetNode("SettingsGroup/DifficultyGroup").GetChildren();
        difficultyButtonsStandardPos = (difficultyButtons[main.difficulty] as Node2D).Position;
        // post processing group Menu
        postButtons = menu.GetNode("SettingsGroup/GraphicsGroup/Container").GetChildren();
        postArrows = menu.GetNode("SettingsGroup/GraphicsGroup").GetChildren();
        postButtonsStandardPos = (postButtons[main.postProccessing] as Node2D).Position;
        // duration buttons Menu
        durationButtons = menu.GetNode("SettingsGroup/RoundLengthGroup").GetChildren();

        // difficulty group Main
        difficultyButtonsMain = main.GetNode("QuickMenuContainer/QuickMenu/SettingsGroup/DifficultyGroup/Container").GetChildren();
        difficultyArrowsMain = main.GetNode("QuickMenuContainer/QuickMenu/SettingsGroup/DifficultyGroup").GetChildren();
        difficultyButtonsStandardPosMain = (difficultyButtonsMain[main.difficulty] as Node2D).Position;
        // post processing group Menu
        postButtonsMain = main.GetNode("QuickMenuContainer/QuickMenu/SettingsGroup/GraphicsGroup/Container").GetChildren();
        postArrowsMain = main.GetNode("QuickMenuContainer/QuickMenu/SettingsGroup/GraphicsGroup").GetChildren();
        postButtonsStandardPosMain = (postButtonsMain[main.postProccessing] as Node2D).Position;
        // duration buttons Menu
        durationButtonsMain = main.GetNode("QuickMenuContainer/QuickMenu/SettingsGroup/RoundLengthGroup").GetChildren();


        // tween mainly for released draggable elements
        AddChild(posTween);
        AddChild(posTweenMain);

        // connecting listeneres
        touchMain.Connect("input_event", this, "TouchListener");
        touchMenu.Connect("input_event", this, "TouchListener");

        startRoundBtn.Connect("input_event", startRoundBtn, "OnButtonPress");


        LoadingTimer(loadingScreen, 1f, 0f, .5f, true);
    }


    ////////////////////////////////////////////////////////////////////////
    //_______________________________________________________________
    //--------------------------------------------------------
    // User preferences class and related variables
    public UserPref userPref;

	public class UserPref : Node {
		public string userName { get; set; }
        public float bestScore { get; set; }
		public float roundDur { get; set; }
		public int roundDif { get; set; }
        public int postProcess { get; set; }
        public int music { get; set; }
        public int sfx { get; set; }

		// constructor
		public UserPref(string n, float s, float dur, int dif, int pp, int m, int sfx) {
			this.userName = n;
            this.bestScore = s;
			this.roundDur = dur;
			this.roundDif = dif;
            this.postProcess = pp;
            this.music = m;
            this.sfx = sfx;
		}

        public void WriteLocalUserPref() {
            var documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            try {

                FileStream outStream = new FileStream(documents + @"/" + "userdata.set", FileMode.Create, FileAccess.Write);

                BinaryWriter writer = new BinaryWriter(outStream);

                writer.Write((string)this.userName);
                writer.Write((float)this.bestScore);
                writer.Write((float)this.roundDur);
                writer.Write((Int32)this.roundDif);
                writer.Write((Int32)this.postProcess);
                writer.Write((Int32)this.music);
                writer.Write((Int32)this.sfx);


                writer.Dispose();

                GD.Print($"preferences written successfully.. fucking well done");

            } catch (Exception e) {
                GD.Print($"Failed to save data in userdata.set: {e}");
            }
        }

        public string ReadLocalUserPref() {
            try {
                FileStream inStream = new FileStream("userdata.set", FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(inStream);

                // read data from local file
                this.userName = reader.ReadString();
                this.bestScore = reader.ReadSingle();
                this.roundDur = reader.ReadSingle();
                this.roundDif = reader.ReadInt32();
                this.postProcess = reader.ReadInt32();
                this.music= reader.ReadInt32();
                this.sfx = reader.ReadInt32();

                GD.Print($"loading user preference for: {this.userName}");
                GD.Print($"best recorded score: {this.bestScore}");
                GD.Print($"last used round duration: {this.roundDur}");
                GD.Print($"last played difficulty: {this.roundDif}");
                GD.Print($"post processing settings: {this.postProcess}");
                GD.Print($"music: {this.music}");
                GD.Print($"sound effects: {this.sfx}");

                GD.Print($"preferences loaded successfully.. fucking well done");

            reader.Dispose();

            } catch (System.IO.FileNotFoundException) {
                GD.Print("No 'userdata.set' file found in root folder");
            } catch (Exception e) {
                GD.Print($"Failed to load data from 'userdata.set': {e}");
            }

            return this.userName;
        }
	}

    public void AssignLoadedPreferences() {
        main.roundDuration = userPref.roundDur;
        main.difficulty = userPref.roundDif;
        main.postProccessing = userPref.postProcess;
        main.music = userPref.music;
        main.sfx = userPref.sfx;

        int tgtIt = main.roundDuration == 30f ? 1 :
            main.roundDuration == 60f ? 2 :
            main.roundDuration == 90f ? 3 : 4 ;

        for (int i = 1; i <= 4; i++) {
            animPack.ScaleTween(((Node2D)durationButtons[i] as Node2D), new Vector2(1f,1f), .4f);
            animPack.ColorTween(((Node2D)durationButtons[i] as Node2D), new Color(1,1,1,1), .4f);
        }

        SetDuration((Node2D)durationButtons[tgtIt], false);
        SetDifficulty(false);
        SetPost(false);
        posTween.ResumeAll();

        // TODO!!!
        // need to created setter methods for music other audio
    }
    //________________________________________________________
    //---------------------------------------------------------------
    //////////////////////////////////////////////////////////////////////////////////





    public float scoreCatch;
    public bool initRun = true;
    public async void LoadingTimer(Node2D el, float delay, float end, float dur, bool hide = false, string tgtLoad = "init") {
        Timer t = new Timer();
        t.WaitTime = delay;
        t.OneShot = true;
        AddChild(t);
        t.Start();



        await ToSignal(t, "timeout");


        if (tgtLoad == "init") {
            initRun = false;
            if (main.highscoreList.Count == 0) {
                scoreCatch = 0;
            } else {
                scoreCatch = main.highscoreList[0].userScore;
            }

            userPref = new UserPref(userName, scoreCatch, main.roundDuration, main.difficulty, main.postProccessing, main.music, main.sfx);
            AddChild(userPref);

            GD.Print("!!!loaded score: " + userPref.bestScore);

            userName = userPref.ReadLocalUserPref();

            if (userName != "") {

                userGroupLabel.Text = main.deviceName;
                AssignLoadedPreferences();
            } else {
                promptControl.Show();
                SetDuration((Node2D)durationButtons[1], false);
            }
        } else if (tgtLoad == "playSp") {
            mpGame = false;

            oppNameLabel.Text = main.deviceName;
            try {
                oppNameLabel.Text = main.highscoreList[0].userScore + "p";
            } catch {
                GD.Print("No recorded highscores");
            }
            if (!menu.activeWindow.Equals(menu.gameLabelGroup)) {
                menu.SwitchMenuWindow(menu.gameLabelGroup);
            }
            menu.closeButton.Hide();
            menu.Hide();
            main.Show();
        } else if (tgtLoad == "playMp") {
            mpGame = true;
            oppNameLabel.Text = main.oppName;
            if (!menu.activeWindow.Equals(menu.gameLabelGroup)) {
                menu.SwitchMenuWindow(menu.gameLabelGroup);
            }
            menu.closeButton.Hide();
            menu.Hide();
            main.Show();
            (main.loadingIndicator.GetNode("Label") as Label).Text = "Finding you a friend";
            main.loadingIndicator.Modulate = new Color(1f,1f,1f,0f);
            main.mpRequestPrompt.Hide();
            if (main.isHost) {
                main.Rpc("SetPreferances", main.roundDuration, main.difficulty);
            }
        } else if (tgtLoad == "mainMenu") {
            main.HideQuickMenu();
            main.Hide();
            menu.Show();
        }



        t.Start();

        await ToSignal(t, "timeout");
        animPack.AlphaTween(el, end, dur, hide);

        t.QueueFree();

        // t.WaitTime = dur;
        // t.Start();

        // await ToSignal(t, "timeout");
        // // activate button again when Tween is finished
        // menu.spButton.InputPickable = true;
    }


    [Signal]
    public delegate void InputReleased();
    public InputEventScreenTouch inputCheck;
    // related variables
    public int stepCount = 0;
    public float stressTickCount;

    // check first press of userName input
    public bool firstPress = true;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {


        // singleplayer button block
        if (main.connected || main.attemptConnection || main.isHost) {
            menu.spButton.Modulate = new Color(.4f,.4f,.4f,1f);
        } else if (!main.connected && !main.attemptConnection && !main.isHost) {
            menu.spButton.Modulate = new Color(1f,1f,1f,1f);
        }


        if (promptControl.Visible && promptControl.userNameInput.HasFocus() && firstPress) {
            firstPress = false;
            promptControl.userNameInput.Text = "";
        }


        // if round is active
        if (!main.roundTimer.IsStopped()) {
            // display time left
            roundTimerLabel.Text = main.roundTimer.TimeLeft.ToString("f1") + "s";
            userScoreLabel.Text = main.userScore.ToString();

            if (!mpGame) {
                // check for and play saved score
                if (main.roundDuration == 30f && main.savedSteps30.Count >= 1 && main.scoreSaved30) {

                    // take saved steps at consecutive timeStamps
                    while (main.roundTimer.TimeLeft <= main.savedSteps30[stepCount].timeStamp) {

                        int oppPoints = main.savedSteps30[stepCount].stepPoints;


                        ScoreDisplayAndAnimate(oppPoints);


                        // if there are more steps, loop
                        if (stepCount < main.savedSteps30.Count) {
                            stepCount++;
                            continue;
                        } else {
                            return;
                        }
                    }
                }

                if (main.roundDuration == 60f && main.savedSteps60.Count >= 1 && main.scoreSaved60) {

                    // take saved steps at consecutive timeStamps
                    while (main.roundTimer.TimeLeft <= main.savedSteps60[stepCount].timeStamp) {

                        int oppPoints = main.savedSteps60[stepCount].stepPoints;


                        ScoreDisplayAndAnimate(oppPoints);


                        // if there are more steps, loop
                        if (stepCount < main.savedSteps60.Count) {
                            stepCount++;
                            continue;
                        } else {
                            return;
                        }
                    }
                }

                if (main.roundDuration == 90f && main.savedSteps90.Count >= 1 && main.scoreSaved90) {

                    // take saved steps at consecutive timeStamps
                    while (main.roundTimer.TimeLeft <= main.savedSteps90[stepCount].timeStamp) {

                        int oppPoints = main.savedSteps90[stepCount].stepPoints;


                        ScoreDisplayAndAnimate(oppPoints);


                        // if there are more steps, loop
                        if (stepCount < main.savedSteps90.Count) {
                            stepCount++;
                            continue;
                        } else {
                            return;
                        }
                    }
                }

                if (main.roundDuration == 120f && main.savedSteps120.Count >= 1 && main.scoreSaved120) {

                    // take saved steps at consecutive timeStamps
                    while (main.roundTimer.TimeLeft <= main.savedSteps120[stepCount].timeStamp) {

                        int oppPoints = main.savedSteps120[stepCount].stepPoints;


                        ScoreDisplayAndAnimate(oppPoints);


                        // if there are more steps, loop
                        if (stepCount < main.savedSteps120.Count) {
                            stepCount++;
                            continue;
                        } else {
                            return;
                        }
                    }
                }
            }




            // countdown stressTick animation
            while(main.roundTimer.TimeLeft <= stressTickCount + .04f) {

                // animation
                animPack.ScalePopMin("RoundTimer");

                // do this until zero
                if (stressTickCount >= 0) {
                    stressTickCount--;
                    continue;
                } else {
                    return;
                }
            }
        }

        // GD.Print(tp);


        // ON_RELEASE actions if inputPressed is true
        // ***I should split this, so it does not loop through simon buttons unnecessarily***
        // if (inputPressed && !Input.IsActionPressed("on_pressed")) {


        //     // check and change button state for all simon buttons
        //
        // }
    }

    public void ScoreDisplayAndAnimate(int oppPoints) {
        main.oppScore += oppPoints;

        // display opponents score
        oppScoreLabel.Text = main.oppScore.ToString();


        // animation
        animPack.ScalePop("OppScore");

        ScoreFeedback scoreInstance = (ScoreFeedback)main.scoreFeedback.Instance();

        oppScoreOrigin.AddChild(scoreInstance);

        scoreInstance.label.Text = oppPoints > 0 ? "+" + oppPoints : oppPoints.ToString();

        animPack.ScorePathTween(scoreInstance);

        animPack.PolygonScale(oppPolygons, (float)oppPoints/200, 1f);
        oppPolygons.Modulate = oppPolygons.Modulate - new Color(0,0,0,oppPoints / 2000);
    }



    // Preference setter methods
    public bool durChanged = false;
    public void SetDuration(Node2D tgtEl, bool save = true) {
        int a = 0;

        GD.Print(tgtEl.Name);

        int tgtIt = tgtEl.Name == "Dur1" ? 1 :
                tgtEl.Name == "Dur2" ? 2 :
                tgtEl.Name == "Dur3" ? 3 : 4 ;

        a = main.roundDuration == 30 ? 1 :
            main.roundDuration == 60 ? 2 :
            main.roundDuration == 90 ? 3 : 4 ;

        for (int i = 1; i <= 4; i++) {
            if (i != tgtIt) {
                // reset active button
                animPack.ScaleTween((durationButtons[i] as Node2D), new Vector2(1f,1f), .4f);
                animPack.ColorTween((durationButtons[i] as Node2D), new Color(1,1,1,1), .4f);
                animPack.ScaleTween((durationButtonsMain[i] as Node2D), new Vector2(1f,1f), .4f);
                animPack.ColorTween((durationButtonsMain[i] as Node2D), new Color(1,1,1,1), .4f);
            } else {
                // mark pressed
                animPack.ScaleTween((durationButtons[tgtIt] as Node2D), new Vector2(1.1f,1.1f), .4f);
                animPack.ColorTween((durationButtons[tgtIt] as Node2D), new Color(1,1,0,1), .4f);
                animPack.ScaleTween((durationButtonsMain[tgtIt] as Node2D), new Vector2(1.1f,1.1f), .4f);
                animPack.ColorTween((durationButtonsMain[tgtIt] as Node2D), new Color(1,1,0,1), .4f);
            }
        }

        main.roundDuration = tgtEl.Name == "Dur1" ? 30f :
                tgtEl.Name == "Dur2" ? 60f :
                tgtEl.Name == "Dur3" ? 90f : 120f ;

        main.simonSaysDelaySpan[1] = main.difficulty == 0 ? 1200 :
            main.difficulty == 1 ? 1000 :
            main.difficulty == 2 ? 800 : 600 ;

        roundTimerLabel.Text = main.roundDuration.ToString("f1") + "s";


        // save user data when needed
        if (save) {
            userPref.roundDur = main.roundDuration;
            userPref.WriteLocalUserPref();
            if (mpGame && durChanged) {
                durChanged = false;
                main.Rpc("ChangeDuration", main.roundDuration);
            }
        }
    }

    public Tween posTween = new Tween();
    public Tween posTweenMain = new Tween();
    public void NextDifficulty() {
        main.difficulty = main.difficulty == 0 ? 1 :
            main.difficulty == 1 ? 2 : 3 ;
        SetDifficulty();
    }

    public void PrevDifficulty() {
        main.difficulty = main.difficulty == 3 ? 2 :
            main.difficulty == 2 ? 1 : 0 ;
        SetDifficulty();
    }

    public bool difChanged = false;
    public void SetDifficulty(bool save = true) {
        GD.Print("checking diff setter: " + main.difficulty);
        GD.Print("strd posX: " + difficultyButtonsStandardPos.x);


        // dimming arrows
        if (main.difficulty == 0) {
            (difficultyArrows[1] as DifficultyChange).Modulate = new Color(1f,1f,1f,.33f);
            (difficultyArrows[2] as DifficultyChange).Modulate = new Color(1f,1f,1f,1f);
            (difficultyArrowsMain[1] as DifficultyChange).Modulate = new Color(1f,1f,1f,.33f);
            (difficultyArrowsMain[2] as DifficultyChange).Modulate = new Color(1f,1f,1f,1f);
        } else if (main.difficulty == 3) {
            (difficultyArrows[1] as DifficultyChange).Modulate = new Color(1f,1f,1f,1f);
            (difficultyArrows[2] as DifficultyChange).Modulate = new Color(1f,1f,1f,.33f);
            (difficultyArrowsMain[1] as DifficultyChange).Modulate = new Color(1f,1f,1f,1f);
            (difficultyArrowsMain[2] as DifficultyChange).Modulate = new Color(1f,1f,1f,.33f);
        } else {
            (difficultyArrows[1] as DifficultyChange).Modulate = new Color(1f,1f,1f,1f);
            (difficultyArrows[2] as DifficultyChange).Modulate = new Color(1f,1f,1f,1f);
            (difficultyArrowsMain[1] as DifficultyChange).Modulate = new Color(1f,1f,1f,1f);
            (difficultyArrowsMain[2] as DifficultyChange).Modulate = new Color(1f,1f,1f,1f);
        }


        // centering current button Menu
        posTween.StopAll();
        posTween.InterpolateProperty((difficultyButtons[main.difficulty] as Node2D), "position", (difficultyButtons[main.difficulty] as Node2D).Position, difficultyButtonsStandardPos, 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
        posTween.Start();
        // --||-- Main
        posTweenMain.StopAll();
        posTweenMain.InterpolateProperty((difficultyButtonsMain[main.difficulty] as Node2D), "position", (difficultyButtonsMain[main.difficulty] as Node2D).Position, difficultyButtonsStandardPosMain, 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
        posTweenMain.Start();


        // hide inactive preceding buttons Menu
        for (int i = 0; i < main.difficulty; i++) {
            posTween.InterpolateProperty((difficultyButtons[i] as Node2D), "position", (difficultyButtons[i] as Node2D).Position, new Vector2((difficultyButtonsStandardPos.x - 300f), difficultyButtonsStandardPos.y), 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
            posTween.Start();
        }
        // same in Main
        for (int i = 0; i < main.difficulty; i++) {
            posTweenMain.InterpolateProperty((difficultyButtonsMain[i] as Node2D), "position", (difficultyButtonsMain[i] as Node2D).Position, new Vector2((difficultyButtonsStandardPosMain.x - 300f), difficultyButtonsStandardPosMain.y), 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
            posTweenMain.Start();
        }


        // hiding succeeding elements Menu
        for (int i = main.difficulty + 1; i <= 3; i++) {
            posTween.InterpolateProperty((difficultyButtons[i] as Node2D), "position", (difficultyButtons[i] as Node2D).Position, new Vector2((difficultyButtonsStandardPos.x + 300f), difficultyButtonsStandardPos.y), 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
            posTween.Start();
        }
        // and same in Main
        for (int i = main.difficulty + 1; i <= 3; i++) {
            posTweenMain.InterpolateProperty((difficultyButtonsMain[i] as Node2D), "position", (difficultyButtonsMain[i] as Node2D).Position, new Vector2((difficultyButtonsStandardPosMain.x + 300f), difficultyButtonsStandardPosMain.y), 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
            posTweenMain.Start();
        }


        // save user data when needed
        if (save) {
            userPref.roundDif = main.difficulty;
            userPref.WriteLocalUserPref();
            if (mpGame && difChanged) {
                difChanged = false;
                main.Rpc("ChangeDifficulty", main.difficulty);
            }
        }
    }


    // post processing
    public void NextPost() {
        main.postProccessing = 1;
        SetPost();
    }

    public void PrevPost() {
        main.postProccessing = 0;
        SetPost();
    }

    public void SetPost(bool save = true) {

        // hide inactive button in Menu
        posTween.StopAll();
        if (main.postProccessing == 0) {
            (postArrows[1] as GraphicsChange).Modulate = new Color(1f,1f,1f,.33f);
            (postArrows[2] as GraphicsChange).Modulate = new Color(1f,1f,1f,1f);
            posTween.InterpolateProperty((postButtons[1] as Node2D), "position", (postButtons[1] as Node2D).Position, new Vector2((postButtonsStandardPos.x + 300), postButtonsStandardPos.y), 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
            posTween.Start();
        } else if (main.postProccessing == 1) {
            (postArrows[1] as GraphicsChange).Modulate = new Color(1f,1f,1f,1f);
            (postArrows[2] as GraphicsChange).Modulate = new Color(1f,1f,1f,.33f);
            posTween.InterpolateProperty((postButtons[0] as Node2D), "position", (postButtons[0] as Node2D).Position, new Vector2((postButtonsStandardPos.x - 300), postButtonsStandardPos.y), 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
            posTween.Start();
        }
        // same in Main
        posTweenMain.StopAll();
        if (main.postProccessing == 0) {
            (postArrowsMain[1] as GraphicsChange).Modulate = new Color(1f,1f,1f,.33f);
            (postArrowsMain[2] as GraphicsChange).Modulate = new Color(1f,1f,1f,1f);
            posTweenMain.InterpolateProperty((postButtonsMain[1] as Node2D), "position", (postButtonsMain[1] as Node2D).Position, new Vector2((postButtonsStandardPosMain.x + 300), postButtonsStandardPosMain.y), 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
            posTweenMain.Start();
        } else if (main.postProccessing == 1) {
            (postArrowsMain[1] as GraphicsChange).Modulate = new Color(1f,1f,1f,1f);
            (postArrowsMain[2] as GraphicsChange).Modulate = new Color(1f,1f,1f,.33f);
            posTweenMain.InterpolateProperty((postButtonsMain[0] as Node2D), "position", (postButtonsMain[0] as Node2D).Position, new Vector2((postButtonsStandardPosMain.x - 300), postButtonsStandardPosMain.y), 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
            posTweenMain.Start();
        }


        // center the active button Menu
        posTween.InterpolateProperty((postButtons[main.postProccessing] as Node2D), "position", (postButtons[main.postProccessing] as Node2D).Position, postButtonsStandardPos, 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
        posTween.Start();
        // same in Main
        posTweenMain.InterpolateProperty((postButtonsMain[main.postProccessing] as Node2D), "position", (postButtonsMain[main.postProccessing] as Node2D).Position, postButtonsStandardPosMain, 1f, Tween.TransitionType.Quint, Tween.EaseType.Out);
        posTweenMain.Start();

        if (main.postProccessing == 1) {
            GetViewport().ShadowAtlasSize = 4096;
            GetViewport().Msaa = Godot.Viewport.MSAA.Msaa16x;
            // (GetNode("/root/Main/MainCamera") as Camera).Environment.SsaoEnabled = true;
        } else {
            GetViewport().ShadowAtlasSize = 1024;
            GetViewport().Msaa = Godot.Viewport.MSAA.Disabled;
            // (GetNode("/root/Main/MainCamera") as Camera).Environment.SsaoEnabled = false;
        }

        // Save user data if necessary
        if (save) {
            userPref.postProcess = main.postProccessing;
            userPref.WriteLocalUserPref();
        }
    }




    // Touch listener... mainly for release and drag events
    public void TouchListener(Viewport viewport, InputEvent ev, int shape_idx) {

        if (ev is InputEventScreenTouch press && (ev as InputEventScreenTouch).Pressed) {
            if (dragEl != null) {
                // posTween.StopAll();
                // posTweenMain.StopAll();
                pressPos = press.Position;
                origElPos = dragEl.Position;
                relPressPos = origElPos - pressPos;

                GD.Print((pressPos));
            }

        }

        if (ev is InputEventScreenTouch  && !(ev as InputEventScreenTouch).Pressed && inputPressed) {
            EmitSignal("InputReleased");
            dragEl = null;
        }

        if (ev is InputEventScreenTouch  && !(ev as InputEventScreenTouch).Pressed) {

            foreach (Main.SimonButton simonButtonList in main.simonButtonList) {

                if (simonButtonList.el.sprite.Animation == "red" || simonButtonList.el.sprite.Animation == "pressed") {
                    simonButtonList.el.sprite.Animation = "idle";
                    animPack.SyncScaleTween(simonButtonList.el, 1f, 1f);

                }
            }
        }

        if (ev is InputEventScreenDrag drag && dragEl != null) {
            dragEl.Position = new Vector2(origElPos.x - (pressPos.x - drag.Position.x), dragEl.Position.y);
            dragTravel.x = pressPos.x - drag.Position.x;
        }
    }

    Node2D dragEl;
    Vector2 pressPos, relPressPos, origElPos, dragTravel;
    public async void InputPressed(object n = null, bool draggable = false) {

        inputPressed = true;



        if (draggable) {
            dragEl = (Node2D)n;
            posTween.StopAll();
            posTweenMain.StopAll();
        }

        await ToSignal(this, "InputReleased");
        inputPressed = false;

        if (n != null) {
            if (n is MenuButton m) {
                m.OnRelease();
            } else if (n is StartRoundBtn st) {
                GD.Print("true");
                st.OnRelease();
            } else if (n is DifficultyChange d) {
                d.OnRelease(dragTravel);
            } else if (n is GraphicsChange gr) {
                gr.OnRelease(dragTravel);
            } else if ((n as Node2D).Name == "SettingsButton") {
                main.QuickMenuButtonRelease();
            }
        }
    }
}
