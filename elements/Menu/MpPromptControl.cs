using Godot;
using System;

public class MpPromptControl : Node2D {

    public Global g;
    public LineEdit userNameInput;
    public Node2D prompt;
    public Label promptMessage;
    public StaticBody2D playButton, mpCloseButton;

    public override void _Ready() {

        // binding global script
		g = (Global)GetNode("/root/Global");
        
        // binding local nodes
        prompt = (Node2D)GetNode("NamePrompt");
        promptMessage = (Label)prompt.GetNode("Label");
        userNameInput = (LineEdit)prompt.GetNode("UserNameInput");
        playButton = (StaticBody2D)prompt.GetNode("PlayButton");
        mpCloseButton = (StaticBody2D)prompt.GetNode("MpCloseButton");

        // connect input events
        playButton.Connect("input_event", playButton, "OnButtonPress");
        mpCloseButton.Connect("input_event", mpCloseButton, "OnButtonPress");
    }
}
