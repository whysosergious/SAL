using Godot;
using System;

public class userPolygonGroup : Area2D {
    
    [Signal]
    public delegate void Collided();
    public override void _Ready() {
        
    }

    public void _on_Point_Collided(PhysicsBody2D el) {
        EmitSignal("Collided");
        GD.Print("rec");
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        // GD.Print(this.GetCollidingBodies());
    }
}
