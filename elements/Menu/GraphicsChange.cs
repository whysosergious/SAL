using Godot;
using System;

public class GraphicsChange : RigidBody2D {
    public Global g;
    


    public override void _Ready() {
        g = (Global)GetNode("/root/Global");
        
    }

    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
        

        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
            GD.Print("22");
            if (this.Name == "ArrowRight" && g.main.postProccessing != 1) {
                g.NextPost();
            } else if (this.Name == "ArrowLeft" && g.main.postProccessing != 0) {
                g.PrevPost();
            } else if (this.Name != "ArrowRight" && this.Name != "ArrowLeft") {
                g.InputPressed(this, true);
            }
            
        }
    }

    public void OnRelease(Vector2 dragTravel) {
        
        if (dragTravel.x >= 70 && g.main.postProccessing != 1) {
            g.NextPost();
        } else if (dragTravel.x <= -70 && g.main.postProccessing != 0) {
            g.PrevPost();
        } else {
            g.SetPost();
        }
        
    }


    
}
