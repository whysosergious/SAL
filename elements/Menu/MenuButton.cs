using Godot;
using System;

public class MenuButton : StaticBody2D {

    public Global g;
    public Node2D masterNode;
    public Node sprite, icon;

    // standard parameters
    public Vector2 pos;

    public override void _Ready() {

        g = (Global)GetNode("/root/Global");

        masterNode = (Node2D)GetNodeOrNull($"../../{this.Name.Replace("Button", "Group")}");

        if (this.Name == "SpButton") {
            masterNode = (Node2D)GetTree().CurrentScene;
        }

        if (masterNode == null) {
            masterNode = (Node2D)GetNode("/root/Menu/GameLabelGroup");
        } else {
            masterNode.Hide();
        }
    }





    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
            g.InputPressed(this);

            Vector2 scale = new Vector2(.03f,.03f);


            if (sprite is Godot.AnimatedSprite) {
                (sprite as AnimatedSprite).Animation = "pressed";
            }


            if (this.Name == "MpButton" && !g.main.connected && !g.main.isHost) {
                g.main.AlphaTween(g.main.loadingIndicator, 1f, .5f);
                g.main.attemptConnection = true;
                GD.Print("Start connection timer");
                g.main.ConnectionTimer();
            }


            if (masterNode.Name == "Main") {

                if (!g.main.connected && !g.main.isHost && !g.main.attemptConnection) {
                    g.loadingScreen.Show();
                    g.animPack.AlphaTween(g.loadingScreen, 1f, .5f);

                    g.LoadingTimer(g.loadingScreen, 1f, 0f, .5f, true, "playSp");

                    GD.Print("Play SP");
                } else {
                    g.main.LeaveServer();
                }
                // GD.Print("round duration: " + g.main.roundDuration);
                // GD.Print("difficulty: " + g.main.difficulty);
                // GD.Print("post proccessing: " + g.main.postProccessing);

            } else if (masterNode.Name == "SettingsGroup") {

                (icon as AnimatedSprite).Animation = "cross";

                g.menu.SwitchMenuWindow(masterNode);

                scale = new Vector2(.1f,.1f);
                g.menu.closeButton.Hide();

            } else if (!g.menu.activeWindow.Equals(masterNode)) {

                g.menu.SwitchMenuWindow(masterNode);
                g.menu.closeButton.Show();
            }


            this.Scale -= scale;

        }

    }

    public void OnRelease() {
        this.Scale = new Vector2(1f,1f);
        if(this.Name != "CloseButton") {
            (sprite as AnimatedSprite).Animation = "idle";
        } else {
            this.Hide();
            if (g.main.connected || g.main.isHost) {
                g.main.LeaveServer();

            } else if (g.main.UDP_recieving) {
                g.main.UDP_client.Close();
				g.main.UDP_recieving = false;
                g.main.attemptConnection = false;
            }
        }
    }

    // public void OnButtonPress(Viewport viewport, InputEventScreenTouch ev, int shape_idx) {
    //     // GD.Print(ev);
    //     if (ev is InputEventScreenTouch && ev.Pressed) {
    //         // GD.Print(ev);

    //         Vector2 scale = new Vector2(.03f,.03f);


    //         if (sprite is Godot.AnimatedSprite) {
    //             (sprite as AnimatedSprite).Animation = "pressed";
    //         }



    //         if (masterNode.Name == "Main") {

    //             GD.Print("Play SP");

    //         } else if (masterNode.Name == "SettingsGroup") {

    //             (icon as AnimatedSprite).Animation = "cross";

    //             g.menu.SwitchMenuWindow(masterNode);

    //             scale = new Vector2(.1f,.1f);

    //         } else if (!g.menu.activeWindow.Equals(masterNode)) {

    //             g.menu.SwitchMenuWindow(masterNode);

    //         }


    //         this.Scale -= scale;


    //     }
    // }
}
