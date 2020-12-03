using Godot;
using System;

public class AnimPack : Node {
    
    // variable names for elements, nodes and scripts
    public Global g;
    public Random r = new Random();




    public override void _Ready() {
        
        // binding global script
        g = (Global)GetNode("/root/Global");
        

    }




    // animation methods
    public void ScalePop(string node) {
        AnimationPlayer label = (AnimationPlayer)GetNode($"/root/Main/AnimPack/{node}Player");
        label.RootNode = $"/root/Main/LabelGroup/{node}Lbl";
        label.Stop(true);
        label.Play("ScalePop");
    }

    public void ScalePopMin(string node) {
        AnimationPlayer label = (AnimationPlayer)GetNode($"/root/Main/AnimPack/{node}Player");
        label.RootNode = $"/root/Main/LabelGroup/{node}Lbl";
        label.Stop(true);
        label.Play("ScalePopMin");
    }


    public void FallAndFade(string node) {
        AnimationPlayer label = (AnimationPlayer)GetNode($"/root/Main/AnimPack/{node}Player");
        label.RootNode = $"/root/Main/LabelGroup/{node}Parent/{node}Lbl";
        label.Stop(true);
        label.Play("FallAndFade");
    }

    
    // simonButtons animation and states
    public void SmoothScaleUp(string node, string name = default(string)) {
        string[] nodePath = NodeCheck(node, name);

        AnimationPlayer label = (AnimationPlayer)GetNode(nodePath[0]);
        label.RootNode = nodePath[1];
        label.Stop(true);
        label.Play("SmoothScaleUp");
    }

    public void SmoothScaleDown(string node, string name = default(string)) {
        string[] nodePath = NodeCheck(node, name);

        AnimationPlayer label = (AnimationPlayer)GetNode(nodePath[0]);
        label.RootNode = nodePath[1];
        label.Stop(true);
        label.Play("SmoothScaleDown");
    }

    public void SmoothScaleDownMin(string node, string name = default(string)) {
        string[] nodePath = NodeCheck(node, name);

        AnimationPlayer label = (AnimationPlayer)GetNode(nodePath[0]);
        label.RootNode = nodePath[1];
        label.Stop(true);
        label.Play("SmoothScaleDownMin");
    }


    // simon feedback blast
    public void FeedbackBlast(string node, string name = default(string)) {
        string[] nodePath = NodeCheck(node, name);

        AnimationPlayer label = (AnimationPlayer)GetNode(nodePath[0]);
        label.RootNode = nodePath[1];
        label.Stop(true);
        label.Play("FeedbackBlast");
    }


    public string[] NodeCheck(string node, string name = default(string)) {

        string[] result = { "p", "b" };

        if (node == "Simon") {
            result[0] = $"/root/Main/SimonGroup/{name}/{node}Player";
            result[1] = $"/root/Main/SimonGroup/{name}";
        }
        
        return result;
    }



    
    

    // Tweens
    public async void PositionTween(Node2D el, Vector2 end, float duration) {
        Tween pos = new Tween();
        AddChild(pos);

        pos.InterpolateProperty(el, "position", el.Position, end, duration, Tween.TransitionType.Quint, Tween.EaseType.Out);
        pos.Start();

        await ToSignal(pos, "tween_completed");

        pos.QueueFree();
    }


    public async void ScaleTween(Node2D el, Vector2 end, float duration) {
        Tween scale = new Tween();
        AddChild(scale);

        scale.InterpolateProperty(el, "scale", el.Scale, end, duration, Tween.TransitionType.Quint, Tween.EaseType.Out);
        scale.Start();

        await ToSignal(scale, "tween_completed");

        scale.QueueFree();
    }


    public async void ColorTween(Node2D el, Color end, float duration, bool hideAfter = false) {
        Tween color = new Tween();
        AddChild(color);

        color.InterpolateProperty(el, "modulate", el.Modulate, end, duration, Tween.TransitionType.Quint, Tween.EaseType.Out);
        color.Start();

        await ToSignal(color, "tween_completed");

        if (hideAfter) { el.Hide(); }

        color.QueueFree();
    }



    public async void AlphaTween(Node2D el, float end, float duration, bool hideAfter = false) {
        Tween alpha = new Tween();
        AddChild(alpha);

        alpha.InterpolateProperty(el, "modulate", el.Modulate, new Color(1, 1, 1, end), duration, Tween.TransitionType.Quint, Tween.EaseType.Out);
        alpha.Start();

        await ToSignal(alpha, "tween_completed");

        if (hideAfter) { el.Hide(); }

        alpha.QueueFree();
    }

    public async void PolygonScale(Node2D el, float end, float duration, bool reset = false) {
        Tween scale = new Tween();
        AddChild(scale);

        

        scale.InterpolateProperty(el, "scale", el.Scale, el.Scale + new Vector2(end, end), duration, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        scale.Start();

        await ToSignal(scale, "tween_completed");

        
        
        if(reset) {
            scale.ResetAll();
            scale.InterpolateProperty(el, "scale", el.Scale, new Vector2(1f, 1f), duration, Tween.TransitionType.Elastic, Tween.EaseType.Out);
            scale.Start();
            await ToSignal(scale, "tween_completed");
        }
        scale.QueueFree();
    }


    public async void SyncScaleTween(SimonBtn el, float duration, float end, float start = default(float)) {

        Tween scaleEl = new Tween();
        AddChild(scaleEl);

        if (start == 0) {
            start = el.Scale.x;
        }

        scaleEl.InterpolateProperty(el, "scale", new Vector2(start, start), new Vector2(end, end), duration, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        scaleEl.Start();
        

        await ToSignal(scaleEl, "tween_completed");
        scaleEl.QueueFree();

    }

    public async void ScorePathTween(ScoreFeedback el, int timing = -1, float delay = 0f) {

        Timer t = new Timer();
        t.OneShot = true;
        AddChild(t);

        Tween move = new Tween();
        AddChild(move);
        Tween scale = new Tween();
        AddChild(scale);
        Tween alpha = new Tween();
        AddChild(alpha);


        // setting original and target color depending on user score
        // randomize the brightness to give depth
        Godot.Color color = new Color(1f,1f,1f,1f);
        
        if (timing == 2) {
            float rb = (float)r.Next(70, 100) / 100;
            color.r = rb;
            color.g = rb;
            color.b = 0f;
        } else if (timing == 1) {
            color.r = 0f;
            color.g = (float)r.Next(70, 100) / 100;
            color.b = 0f;
        } else {
            color.r = (float)r.Next(70, 100) / 100;
            color.g = 0f;
            color.b = 0f;
        }

        Godot.Color tgtColor = color;
        tgtColor.a = 0f;

        Tween.TransitionType transType = Tween.TransitionType.Circ;
        Tween.EaseType easeType = Tween.EaseType.Out;

        float duration = r.Next(10, 15);
        duration = duration / 10;

        int angle = r.Next(0, 360);

        double x = 0 + 70 * Math.Cos(angle * (Math.PI / 180));
        double y = 0 + 70 * Math.Sin(angle * (Math.PI / 180));
        float origX = (float)x / 4;
        float origY = (float)y / 4;

        // set to transparent
        el.Modulate = tgtColor;


        if (delay > 0) {
            t.WaitTime = delay;
            t.Start();
            
            

            x = 0 + 100 * Math.Cos(angle * (Math.PI / 180));
            y = 0 + 100 * Math.Sin(angle * (Math.PI / 180));

            origX = (float)x / 3;
            origY = (float)y / 3;

            transType = Tween.TransitionType.Quad;
            easeType = Tween.EaseType.Out;

            el.Position = new Vector2(origX, origY);

            await ToSignal(t, "timeout");
        }

        t.WaitTime = duration - .8f;

        t.Start();
        
        
        
        // set color of instance
        el.Modulate = color;

        move.InterpolateProperty(el, "position", new Vector2(origX, origY), new Vector2((float)x, (float)y), duration, transType, easeType);
        move.Start();

        

        scale.InterpolateProperty(el, "scale", new Vector2(1f, 1f), new Vector2(1.4f, 1.4f), duration, Tween.TransitionType.Quart, Tween.EaseType.Out);
        scale.Start();


        await ToSignal(t, "timeout");

        alpha.InterpolateProperty(el, "modulate", el.Modulate, new Color(tgtColor.r, tgtColor.g, tgtColor.b, tgtColor.a), .6f, Tween.TransitionType.Quint, Tween.EaseType.Out);
        alpha.Start();


        t.WaitTime = .5f;
        t.Start();

        await ToSignal(t, "timeout");

        t.QueueFree();
        move.QueueFree();
        scale.QueueFree();
        alpha.QueueFree();
        el.Free();

    }


//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta) {
//      
//  }
}
