using Godot;
using System;

public class DurationChange : RigidBody2D {

    public Global g;
    

    public override void _Ready() {
        g = (Global)GetNode("/root/Global");
    }

    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
            g.durChanged = true;

            g.InputPressed();

            g.SetDuration(this);
        }
    }
}
