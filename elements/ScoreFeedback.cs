using Godot;
using System;

public class ScoreFeedback : Sprite {

    public Label label;
    public override void _Ready() {
        label = (Label)GetNode("Label");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta) {
//      
//  }
}
