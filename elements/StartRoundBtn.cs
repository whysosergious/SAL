using Godot;
using System;

public class StartRoundBtn : RigidBody2D {

    // elements, nodes and scripts
    public Global g;
    public AnimatedSprite sprite;
    public Label label;

    // defining original values
    public Vector2 originPosition;

    public override void _Ready() {

        // setting original values
        originPosition = this.Position;
        
        // global script
        g = (Global)GetNode("/root/Global");

        // local elements and nodes
        sprite = (AnimatedSprite)GetNode("Sprite");
        label = (Label)GetNode("Label");

    }




    // input events
    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {

        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
            g.InputPressed(this);
            sprite.Animation = "pressed";
            Vector2 scale = new Vector2(.03f,.03f);
            this.Scale -= scale;
        }
    }


    public void OnRelease() {
        this.Scale = new Vector2(1f,1f);
        sprite.Animation = "idle";
        if (g.main.roundTimer.IsStopped()) {
            if (g.mpGame) {
                g.main.Rpc("RoundStart");
            }

            g.main.RoundTimer();
            label.Text = "Stop";
        } else {
            if (g.mpGame) {
                g.main.Rpc("RoundStop");
            }

            g.main.ResetRound();
            label.Text = "Start";
        }
    }




//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta) {
//      
//  }
}
