using Godot;
using System;

public class QuickMenuContainer : Node2D {

    public Global g;

    public Node2D quickMenu;
    public StaticBody2D mainMenuButton, closeButton;

    public override void _Ready() {

        // binding global script
		g = (Global)GetNode("/root/Global");

        quickMenu = (Node2D)GetNode("QuickMenu");
        mainMenuButton = (StaticBody2D)quickMenu.GetNode("MainMenuButton");


        // connect input events
        mainMenuButton.Connect("input_event", mainMenuButton, "OnButtonPress");
        
    }
}
