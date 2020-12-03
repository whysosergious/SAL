using Godot;
using System;

public class DifficultyChange : RigidBody2D {
    public Global g;


    public override void _Ready() {
        g = (Global)GetNode("/root/Global");
    }

    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
            // this.InputPickable = false;
            if (this.Name == "ArrowRight" && g.main.difficulty != 3) {
                g.difChanged = true;
                g.NextDifficulty();
            } else if (this.Name == "ArrowLeft" && g.main.difficulty != 0) {
                g.difChanged = true;
                g.PrevDifficulty();
            } else if (this.Name != "ArrowRight" && this.Name != "ArrowLeft") {
                g.InputPressed(this, true);
            }
        }
    }

    public async void WaitTimer() {
        GD.Print("checkTiming");
        Timer t = new Timer();
		t.OneShot = true;
		t.WaitTime = 1f;
        AddChild(t);
		t.Start();
        
        await ToSignal(t, "timeout");
        this.InputPickable = true;
    }

    public void OnRelease(Vector2 dragTravel) {
        
        if (dragTravel.x >= 100 && g.main.difficulty != 3) {
            g.difChanged = true;
            g.NextDifficulty();
        } else if (dragTravel.x <= -100 && g.main.difficulty != 0) {
            g.difChanged = true;
            g.PrevDifficulty();
        } else {
            g.SetDifficulty(false);
        }
        
    }


    
}
