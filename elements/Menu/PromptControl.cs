using Godot;
using System;

public class PromptControl : Node2D {

    public Global g;
    public LineEdit userNameInput;
    public Node2D prompt;
    public Label promptMessage;
    public StaticBody2D okButton, closeButton;

    public override void _Ready() {

        // binding global script
		g = (Global)GetNode("/root/Global");
        
        // binding local nodes
        prompt = (Node2D)GetNode("NamePrompt");
        promptMessage = (Label)prompt.GetNode("Label");
        userNameInput = (LineEdit)prompt.GetNode("UserNameInput");
        okButton = (StaticBody2D)prompt.GetNode("OkButton");
        closeButton = (StaticBody2D)prompt.GetNode("CloseButton");

        closeButton.Hide();

        // connect input events
        okButton.Connect("input_event", okButton, "OnButtonPress");
        closeButton.Connect("input_event", closeButton, "OnButtonPress");
    }
}
