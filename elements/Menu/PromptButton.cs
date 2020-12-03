using Godot;
using System;

public class PromptButton : StaticBody2D {

    public Global g;
    public AnimationPlayer cameraPlayer;

    public override void _Ready() {
        
        // binding global script
		g = (Global)GetNode("/root/Global");
        
        cameraPlayer = (AnimationPlayer)GetNode("/root/Main/MainCamera/Player");
    }

    public void OnButtonPress(Viewport viewport, InputEvent ev, int shape_idx) {
        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
            if (this.Name == "OkButton") {
                string userEntry = g.promptControl.userNameInput.Text.Replace(" ", "");
                if (userEntry != "") {
                    g.userName = userEntry;
                    g.userPref.userName = g.userName;
                    g.userPref.WriteLocalUserPref();
                    g.userGroupLabel.Text = g.userName;
                    g.animPack.AlphaTween(g.promptControl, 0f, .5f, true);
                    g.menu.highscoreListContainer.MouseFilter = Godot.Control.MouseFilterEnum.Pass;
                    // g.menu.mpListContainer.MouseFilter = Godot.Control.MouseFilterEnum.Pass;
                } else {
                    cameraPlayer.Stop(true);
                    cameraPlayer.Play("CameraShake");
                }
            } else if (this.Name == "CloseButton") {
                g.animPack.AlphaTween(g.promptControl, 0f, .5f, true);
            } else if (this.Name == "PlayButton") {
                g.animPack.AlphaTween(g.main.mpRequestPrompt, 0f, .5f, true);
                if (!g.main.deniedCheck) {
                    g.main.RpcId(g.main.oppID, "LoadGameScene");
                    g.main.LoadGameScene();
                } else {
                    g.main.deniedCheck = false;
                }
            } else if (this.Name == "MpCloseButton") {
                g.animPack.AlphaTween(g.main.mpRequestPrompt, 0f, .5f, true);
                if (!g.main.deniedCheck) {
                    g.main.RpcId(g.main.oppID, "RequestDenied", g.main.playerName);
                } else {
                    g.main.deniedCheck = false;
                }
            }
        }
        
    }
}
