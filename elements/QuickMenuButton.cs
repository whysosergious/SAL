using Godot;
using System;

public class QuickMenuButton : StaticBody2D {

    public Global g;

    public override void _Ready() {

        // binding global script
		g = (Global)GetNode("/root/Global");
        
    }


    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
            if (this.Name == "MainMenuButton") {
                g.loadingScreen.Show();
                g.animPack.AlphaTween(g.loadingScreen, 1f, .5f);

                if (g.mpGame) {
                    g.main.LeaveServer();
                }
                
                
                g.LoadingTimer(g.loadingScreen, 1f, 0f, .5f, true, "mainMenu");
                g.animPack.AlphaTween(g.quickMenuContainer, 0f, .5f, true);
            }
        }
    }
}
