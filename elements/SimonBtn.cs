using Godot;
using System;

public class SimonBtn : RigidBody2D {


    // ---- temp variables
    private int clickCount = 0;
    private Label clickCountLabel;
    // ---- end temp    ----


    // standard values
    public float buttonStateInterval = 1f;
    public string currTiming = "idle";
    private int timingScore = -1;
    private int btnNr;

    public string[] perfFeedback = { "Perfect", "Great", "Super", "Awesome" };
    public string[] goodFeedback = { "Good", "Okay", "Decent", "Ok" };
    public string[] badFeedback = { "Bad", "Late", "Missed", "Nope" };
    public string[] pointSprite = { "Point1", "Point2", "Point3" };

    // variable names for elements, nodes and scripts
    private Random r = new Random();
    public Global g;
    public AnimatedSprite sprite, ps;
    
    AnimationPlayer player, cameraPlayer;




    public override void _Ready() {



        



        // ---- temp assignments
        clickCountLabel = (Label)GetNode("ClickCountLbl");
        // ---- end temp    ----


        // binding global script
        g = (Global)GetNode("/root/Global");

        // binding local elements and nodes
        sprite = (AnimatedSprite)GetNode("Sprite");
        player = (AnimationPlayer)GetNode("SimonPlayer");
        player.RootNode = this.GetPath();
        cameraPlayer = (AnimationPlayer)GetNode("/root/Main/MainCamera/Player");


        // getting standard values
        btnNr = Convert.ToInt32(this.Name.Substr(8,10));


        // adding this button to the button list in Main
        Main.SimonButton newEl = new Main.SimonButton(this);
        g.main.simonButtonList.Add(newEl);


        // timers and everything related
        //-- round timer
        buttonStateTimer = new Timer();
        buttonStateTimer.WaitTime = buttonStateInterval;
        buttonStateTimer.OneShot = true;
        AddChild(buttonStateTimer);

        // connect timer to method
        buttonStateTimer.Connect("timeout", this, nameof(NextState));


        // event connections
        this.Connect("input_event", this, "OnButtonPress");
        




        

        // testsing
        //-- getting the name of this node from list in Main script by getting name number from 'this'
        //-- checking this way that everything works as I expect
        // GD.Print(g.main.simonButtonList[Convert.ToInt32(this.Name.Substr(8,20)) - 1].el.Name + " " + g.main.test4);

    }





    





    // timer signals
    [Signal]
    public delegate void MySignal(string e);

    //****
    //____ Timers, timer methods and everything related ____
    //_____________________________________________________________
    //
    // defining timer
    public Timer buttonStateTimer;
    // timer method
    public void ButtonStateTimer() {

        buttonStateTimer.Start();

        timingScore = 2;
        currTiming = "perfect";
        sprite.Animation = currTiming;
        // animate        
        
        // player.Stop(true);
        player.Play("SmoothScaleUp");

    }


    public void NextState() {

        if (!g.main.roundTimer.IsStopped()) {

            if (currTiming == "perfect") {

                buttonStateTimer.Start();

                timingScore = 1;
                currTiming = "good";
                sprite.Animation = currTiming;
                // animate
                player.Play("SmoothScaleDown");

                
            } else if (currTiming == "good") {
                ResetState();
            }
        } else {
            // ignored timeout
        }
    }


    // stop and reset local timer
    public void ResetState() {

        buttonStateTimer.Stop();

        // set state to idle only if its not.. well, idle
        if (currTiming != "idle") {
            g.main.idleButtonList.Add(btnNr);

            timingScore = -1;
            currTiming = "idle";
            sprite.Animation = currTiming;

        }

        g.animPack.SyncScaleTween(this, 1f, 1f, .8f);

        if (g.main.simonSaysWait && !g.main.roundTimer.IsStopped()) {
            g.main.simonSaysWait = false;
            g.main.SimonSaysTimer();
        }

    }




    // input events
    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {

        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {

             // ---- temp actions
            clickCount++;
            clickCountLabel.Text = clickCount.ToString();
            // ---- end temp    ----


            this.Scale = new Vector2(.8f,.8f);
            // input control
            g.InputPressed();

            // simon button states
            // ***think I will change this to method calls.. depends on timer state changes***
            if (currTiming != "idle") {
                AddStep();

                // g.animPack.FeedbackBlast("Simon", this.Name);
                sprite.Animation = "pressed";
            } else {
                AddStep();
                g.animPack.PolygonScale(g.userPolygons, -.001f, 05f);
                sprite.Animation = "red";

                // shake camera
                cameraPlayer.Stop(true);
                cameraPlayer.Play("CameraShake");
            }
        }
    }


    public void OnRelease() {

    }

    // local tween methods
    


    // register current steps to list in Main
    public void AddStep() {
        if (g.mpGame) {
            g.main.Rpc("OpponentStep", timingScore);
        }
        ScoreFeedback scoreInstance = (ScoreFeedback)g.main.scoreFeedback.Instance();
        ScoreFeedback feedbackInstance = (ScoreFeedback)g.main.scoreFeedback.Instance();

        g.userScoreOrigin.AddChild(scoreInstance);
        g.userScoreOrigin.AddChild(feedbackInstance);

        scoreInstance.label.Text = timingScore > 0 ? "+" + timingScore : timingScore.ToString();
        feedbackInstance.label.Text = timingScore == 2 ? perfFeedback[r.Next(perfFeedback.Length)] : timingScore == 1 ? goodFeedback[r.Next(goodFeedback.Length)] : badFeedback[r.Next(badFeedback.Length)];

        g.animPack.ScorePathTween(scoreInstance, timingScore);
        g.animPack.ScorePathTween(feedbackInstance, timingScore, .1f);
        

        if (timingScore > 0) {
            for (int i = 0; i < timingScore * 2; i++) {
                Point pointInstance = (Point)g.main.pointNode.Instance();

                int angle = r.Next(0, 360);

                double x = this.Position.x + 40 * Math.Cos(angle * (Math.PI / 180));
                double y = this.Position.y + 40 * Math.Sin(angle * (Math.PI / 180));
                

                pointInstance.Position = new Vector2((float)x, (float)y);
                
                ps = (AnimatedSprite)pointInstance.GetNode("Sprite");
                ps.Animation = pointSprite[r.Next(pointSprite.Length)];
                g.main.AddChild(pointInstance);

                // pointInstance.RotationDegrees = (float)r.Next(360);
                
                pointInstance.Modulate = new Color((float)r.Next(7,10)/10,(float)r.Next(7,10)/10,(float)r.Next(7,10)/10,1);

                // pointInstance.Modulate = new Color(.2f,1,1,1);

                pointInstance.PointAnimate(pointInstance);
            }
        }
        




        g.main.userScore += timingScore < 0 && g.main.userScore < 1 ? 0 : timingScore;

        Main.ActionStep newStep = new Main.ActionStep(g.main.roundTimer.TimeLeft, timingScore);
        g.main.currSteps.Add(newStep);

        
        // animation
        g.animPack.ScalePop("UserScore");
        

        ResetState();
    }



    // // Called every frame. 'delta' is the elapsed time since the previous frame.
    // public override void _Process(float delta) {
    // }
}
