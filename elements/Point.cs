using Godot;
using System;

public class Point : RigidBody2D {

    [Signal]
	public delegate void Collided();

	AnimationPlayer cameraPlayer;



	public PackedScene partyEnd = (PackedScene)GD.Load("res://elements/PartyEnd.tscn");


    public Global g;
	public Particles2D party;

    public override void _Ready() {
        // binding global script
		g = (Global)GetNode("/root/Global");
		cameraPlayer = (AnimationPlayer)GetNode("/root/Main/MainCamera/Player");

    }

    public void _on_Point_Collided(PhysicsBody2D el) {
		if (!g.main.roundTimer.IsStopped()) {
			g.animPack.PolygonScale(g.userPolygons, .003f, 1f);
			g.userPolygons.Modulate = g.userPolygons.Modulate - new Color(0,0,0,.0012f);
		}

		Particles2D partyEndInstance = (Particles2D)partyEnd.Instance();
		partyEndInstance.OneShot = true;
		partyEndInstance.GlobalPosition = el.GlobalPosition;
		partyEndInstance.GlobalRotation = el.GlobalRotation;

		g.main.QuickTimer(partyEndInstance);

		party = (Particles2D)this.GetNode("Party");
		party.SpeedScale = 100f;

		// shake camera
		cameraPlayer.Stop(true);
		cameraPlayer.Play("CameraShake");

		EmitSignal("Collided");
		el.QueueFree();
		return;
    }

    public bool move = true;
	float tempVel = -200f;
	public async void PointAnimate(RigidBody2D el) {
		move = true;
		tempVel = -400f;

		Timer t = new Timer();
		AddChild(t);
		t.OneShot = true;
		t.WaitTime = .3f;

		Vector2 tgt = g.userScoreOrigin.Position;

		double radians = Math.Atan2(el.Position.y - g.userScoreOrigin.Position.y, el.Position.x - g.userScoreOrigin.Position.x);

		float angle = (float)(radians * (180/Math.PI));
		// el.AngularVelocity = 7f;

		double gap;
		double origX = el.Position.x;
		double origY = el.Position.y;

		while (move) {



			t.Start();


			double deltaX = Math.Pow((tgt.x - el.GlobalPosition.x), 2);
			double deltaY = Math.Pow((tgt.y - el.GlobalPosition.y), 2);

			double distance = Math.Sqrt(deltaX + deltaY);



			// x = 0 + el.Position.x * Math.Cos(angle * (Math.PI / 180));
            // y = 0 + el.Position.y * Math.Sin(angle * (Math.PI / 180));

            // origX = (float)x / 3;
            // origY = (float)y / 3;

			radians = Math.Atan2(el.Position.y - tgt.y, el.Position.x - tgt.x) - 90;



			if ((origX - el.Position.x) + (origY - el.Position.y) > 100 || (origX - el.Position.x) + (origY - el.Position.y) < -100) {
				el.Rotation = (float)radians;

				el.Rotation += (float)((el.Rotation - radians) / 2 - el.AngularVelocity + .1f);

			}
			if ((origX - el.Position.x) + (origY - el.Position.y) < 100 || (origX - el.Position.x) + (origY - el.Position.y) > -100) {
				el.Rotation += (float)el.Position.x >= 180 ? -.05f : .05f;
			}



			gap = radians - el.Rotation;

			el.Rotation = (float)radians;

			el.Rotation += (float)((el.Rotation - radians) / 2 - el.AngularVelocity + .3f);

			el.LinearVelocity = new Vector2(0f, tempVel).Rotated(el.Rotation);
			await ToSignal(t, "timeout");
			t.WaitTime = .08f;

			tempVel -= tempVel < -300 ? -5f : 15f;

			continue;
		}
		t.QueueFree();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta) {
//     // GD.Print(this.GetCollidingBodies());
//  }
}
