using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


using System.Text;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

public partial class Main : Node2D {

	// multiplayer
	public bool isHost = false;
	public bool connected = false;
	public bool attemptConnection = false;

	public string deviceName = Dns.GetHostName();
	public Array hostIp = Array.FindAll(Dns.GetHostEntry(Dns.GetHostName()).AddressList,
   	a => a.AddressFamily == AddressFamily.InterNetwork);

	private readonly int defaultPort = 35444;

	private Dictionary<int, string> Players = new Dictionary<int, string>();

	public StaticBody2D hostButton, joinButton;
	public Sprite hostButtonCheck;
	public string playerName = "";


	// multiplayer list nodes
	public ScrollContainer mpListContainer;
	public Control mpList;
    public PackedScene mpRow = (PackedScene)GD.Load("res://elements/Menu/MpRow.tscn");
    public Godot.Collections.Array mpRowLabels;
    public float mpRowY = 17;
    public int mpRowCount = 0;
	public Node2D loadingIndicator, mpRequestPrompt;





	private Random rndm = new Random();
	public PackedScene scoreFeedback = (PackedScene)GD.Load("res://elements/ScoreFeedback.tscn");
	public PackedScene pointNode = (PackedScene)GD.Load("res://elements/Point.tscn");




	// timer settings
	//-- round timer duration in seconds
	//-- (to use decimals, 'f' at the end of the given number is obligatory[i.e. .7f || 44.44f])
	public float roundDuration = 30f;
	public int difficulty = 0;
	public int postProccessing = 0;
	public int music = 0;
	public int sfx = 0;
	//-- delay span for simon says timer in ms
	public int[] simonSaysDelaySpan = { 50, 1200 };


	// standard variables
	public int userScore = 0;
	public int oppScore = 0;

	// variable names for elements, nodes and scripts
	public Global g;

	public StaticBody2D quickSettingsButton;
	public AnimatedSprite quickSettingsSprite, quickSettingsButtonIcon;
	// NOTE::: Leave button is the Return to Menu in quickMenu


	public async void QuickTimer(Particles2D el) {
		Timer t = new Timer();
		AddChild(t);
		t.OneShot = true;
		t.WaitTime = .4f;
		t.Start();

		AddChild(el);
		el.Visible = true;



		double radians = Math.Atan2(el.Position.y - g.userScoreOrigin.Position.y, el.Position.x - g.userScoreOrigin.Position.x);
		el.Rotation = (float)radians -90;

		el.OneShot = true;
		el.OneShot = false;
		el.Restart();
		el.Emitting = true;

		await ToSignal(t, "timeout");
		el.OneShot = true;
		t.Start();

		await ToSignal(t, "timeout");
		el.QueueFree();
	}

























	public int oppID;
	public string oppName = "";
	// multiplayer methods
	public void UpdatePlayerList(string playerName, int id, float score) {
        // Highscores
        mpListContainer = (ScrollContainer)GetNode("/root/Menu/MpGroup/ScrollContainer");
        mpList = (Control)mpListContainer.GetNode("List");

		mpRowCount++;

		Sprite mpRowInstance = (Sprite)mpRow.Instance();
		mpRowInstance.Position = new Vector2(140f, mpRowY);

		if (mpRowCount % 2 == 0) {
			mpRowInstance.SelfModulate = new Color(1f,1f,1f,0f);
		}

		mpList.RectMinSize = new Vector2(0f, mpRowY + 40f);
		mpRowLabels = mpRowInstance.GetChildren();
		(mpRowLabels[0] as Label).Text = playerName;
		(mpRowLabels[2] as Label).Text = score.ToString() + "p";

		loadingIndicator.Modulate = new Color(1f,1f,1f,0f);
		AlphaTween(loadingIndicator, 0f, .5f);

		mpRowInstance.Modulate = new Color(1f,1f,1f,0f);
		mpList.AddChild(mpRowInstance);
		AlphaTween(mpRowInstance, 1f, 1f);
		mpRowInstance.GetNode("Button").Connect("input_event", this, "PlayMultiplayer");
		mpRowInstance.Name = id.ToString();
		mpRowY += 44f;
	}



	public PacketPeerUDP UDP_host, UDP_client;
	public int UDP_port = 35666;

	public Timer bt = new Timer();
	public async void BroadcastTimer() {
		bt.OneShot = true;
		bt.WaitTime = 1f;
		bt.Start();

		string hostData = "";

		foreach (object newIp in hostIp) {
			hostData += $"{newIp},";
		}
		hostData += $"||{deviceName}";

		byte[] hostPacketString = Encoding.ASCII.GetBytes(hostData);


		// collect broadcast ips for log
		string broadIpLog = null;

		// iterate thgough all host IPs
		foreach (object ip in hostIp) {

			string[] split = ip.ToString().Split(".");
			string broadcastIp = $"{split[0]}.{split[1]}.{split[2]}.255";
			broadIpLog = $"{ broadIpLog }| { broadcastIp } |";

			UDP_host.SetDestAddress(broadcastIp, UDP_port);

			var err = UDP_host.PutPacket(hostPacketString);

			if (err.ToString() != "Ok") {
				GD.Print($"Failed to send to {broadcastIp}");
			}
		}

		GD.Print(broadIpLog);

		await ToSignal(bt, "timeout");
		if(isHost && !connected) {
			BroadcastTimer();
		}
	}


	public void HostServer(Viewport viewport, InputEvent ev, int shape_idx) {
		if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
			if (!isHost) {
				if (!connected) {
					attemptConnection = false;
					AlphaTween(loadingIndicator, 1f, .5f);
					isHost = true;
					hostButtonCheck.Show();
					GD.Print("Is Host");

					UDP_host = new PacketPeerUDP();
					UDP_host.SetBroadcastEnabled(true);

					UDP_client.Close();
					UDP_recieving = false;

					BroadcastTimer();

					// display hostID & port
					(loadingIndicator.GetNode("Label") as Label).Text = $"Finding you a friend";

					try {
						var peer = new NetworkedMultiplayerENet();

						var a = peer.CreateServer(defaultPort, 32);
						GetTree().NetworkPeer = peer;
					} catch (Exception e) {
						GD.Print($"Could not create a new server: {e}");
					}
				} else {
					GD.Print("Disconnect from server before hosting");
				}
			} else {
				if (!connected) {
					isHost = false;
					hostButtonCheck.Hide();
					GD.Print("Is Client");
					AlphaTween(loadingIndicator, 0f, .5f);

					// revert message
					(loadingIndicator.GetNode("Label") as Label).Text = "Finding you a friend";
					try {
						((NetworkedMultiplayerENet)GetTree().NetworkPeer).CloseConnection();
						GetTree().NetworkPeer = null;
						UDP_host.SetBroadcastEnabled(false);
					} catch (Exception e) {
						GD.Print($"Cought exception while removing ENet: {e}");
					}
				}
			}
		}
	}

	// timer that attempts to connect to the host within small intervals
	public Timer ct = new Timer();
	public async void ConnectionTimer() {
		ct.OneShot = true;
		ct.WaitTime = 1f;
		ct.Start();



		// JoinServer();
		ListenForHost();

		await ToSignal(ct, "timeout");
		if(attemptConnection) {
			ConnectionTimer();
		}
	}


	public Boolean UDP_recieving = false;
	public Godot.Error UPD_listening;

	public void ListenForHost() {

		if (!UDP_recieving) {
			UDP_client = new PacketPeerUDP();
			UPD_listening = UDP_client.Listen(UDP_port);

			if (UPD_listening.ToString() == "Ok") {
				GD.Print($"Listening to UPD_port {UDP_port}");
				UDP_recieving = true;
			} else {
				GD.Print($"Failed to listen to port {UDP_port} retrying");
			}
		}

		if (UDP_client.GetAvailablePacketCount() > 0) {
			var UDP_bytesArray = UDP_client.GetPacket();
			var UDP_packetString = System.Text.Encoding.ASCII.GetString(UDP_bytesArray);

			var UDP_data = UDP_packetString.Split(",||");
			var hostName = UDP_data[1];
			var UDP_ipArray = UDP_data[0].Split(",");
			// var newServerName = UDP_array[1];
			// var newServerPort = UDP_array[2];

			GD.Print("SUCCESS");
			oppName = hostName;

			JoinServer(UDP_ipArray);
			if (ipCount < UDP_ipArray.Length) ipCount++;
			// var newServerPlayers = UDP_array[3];
			// var newServerMaxPlayers = UDP_array[4];
		}
	}


	public int ipCount = 0;
	public void JoinServer(string[] UDP_ipArray) {

		var clientPeer = new NetworkedMultiplayerENet();


		GD.Print($"Trying to connect to server with address {UDP_ipArray[ipCount]}");
		clientPeer.CreateClient(UDP_ipArray[ipCount], defaultPort);
		GetTree().NetworkPeer = clientPeer;
	}



	public void LeaveServer() {
		ipCount = 0;

		GD.Print("Leaving server");

		foreach (var player in Players) {
			GetNode($"/root/Menu/MpGroup/ScrollContainer/List/{player.Key.ToString()}").QueueFree();
		}

		mpRowY -= 44f;
		mpRowCount--;

		Players.Clear();

		Rpc("RemovePlayer", GetTree().GetNetworkUniqueId());

		try {
			((NetworkedMultiplayerENet)GetTree().NetworkPeer).CloseConnection();
			GetTree().NetworkPeer = null;
		} catch (Exception e) {
			GD.Print($"Cought exception while disconnecting: {e}");
		}


		if (isHost || connected) {
			connected = false;
			isHost = false;
			hostButtonCheck.Hide();
		}
	}

	private void PlayerConnected(int id) {
		ipCount = 0;

		connected = true;

		playerName = deviceName;

		GD.Print($"Greet with name:  {playerName}");
		// communicate with connected player by sending an rpc back with user name
		RpcId(id, "RegisterPlayer", playerName, "0x0");
	}

	public void PlayerDisconnected(int id) {
		ipCount = 0;

		connected = false;
		GD.Print("Player disconnected");

		RemovePlayer(id);

		if (isHost) {
			AlphaTween(loadingIndicator, 1f, .5f);
		}
		if (g.mpGame) {
			g.mpGame = false;
			ExitGameScene();
		}
	}

	private void ConnectedToServer() {
		ipCount = 0;

		connected = true;

		GD.Print("Server connection succeeded");
		attemptConnection = false;
	}

	private void ConnectedFailed() {
		ipCount = 0;

		connected = false;
		GetTree().NetworkPeer = null;

		GD.Print("Failed to connect");
	}

	private void ServerDisconnected() {
		ipCount = 0;

		connected = false;
		GD.Print("Disconnected from server");
		foreach (var player in Players) {
			GetNode($"/root/Menu/MpGroup/ScrollContainer/List/{player.Key.ToString()}").QueueFree();
		}
		Players.Clear();
		mpRowY -= 44f;
		mpRowCount--;

		if (g.mpGame) {
			g.mpGame = false;
			ExitGameScene();
		}
	}

	[Remote]
	private void RegisterPlayer(string playerName, float score) {
		var id = GetTree().GetRpcSenderId();
		oppID = id;
		oppName = playerName;
		Players.Add(id, playerName);

		GD.Print($"{playerName} added with ID: {id}");
		UpdatePlayerList(playerName, id, score);

		// opponent added, load her into main
		LoadPlayer(id, playerName);
	}

	[Remote]
	private void StartMultiplayerGame() {
		// load user

		LoadPlayer(GetTree().GetNetworkUniqueId(), playerName);
	}

	private void LoadPlayer(int id, string playerName) {
		// load players and scene
		//ex: menu.Hide();	LoadingControl.Tween;
		// var playerScene = (PackedScene)GD.Load("res://Main.tscn");

		//ex: bgString = playerName;
		var playerNode = (Node2D)GetNode("/root/Main/userPolygonGroup/PlayerNode");
		// playerNode.Name = id.ToString();
		playerNode.SetNetworkMaster(id);

		// playerNode.SetPlayerName(GetTree().GetNetworkUniqueId() == id ? playerName : playerName);
	}

	[Remote]
	private void RemovePlayer(int id) {
		if (Players.ContainsKey(id)) {
			Players.Remove(id);

			GetNode($"/root/Menu/MpGroup/ScrollContainer/List/{id.ToString()}").QueueFree();
			mpRowY -= 44f;
			mpRowCount--;
		}
	}


	public void PlayMultiplayer(Viewport viewport, InputEvent ev, int shape_idx) {
		if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
			GD.Print("Sending request");
			(loadingIndicator.GetNode("Label") as Label).Text = "Match request sent";
			AlphaTween(loadingIndicator, 1f, .5f);
			RpcId(oppID, "RequestGame", playerName);
		}
	}

	[Remote]
	public void RequestGame(string playerName) {
		(mpRequestPrompt.GetNode("NamePrompt/Label") as Label).Text = $"{playerName} wants\nto play with you";

		mpRequestPrompt.Show();
		AlphaTween(mpRequestPrompt, 1f, .5f);
	}

	[Remote]
	public void LoadGameScene() {
		g.loadingScreen.Show();
		g.animPack.AlphaTween(g.loadingScreen, 1f, .5f);

		g.LoadingTimer(g.loadingScreen, 1f, 0f, .5f, true, "playMp");
	}

	[Remote]
	public void ExitGameScene() {
		if (isHost) {
			try {
				((NetworkedMultiplayerENet)GetTree().NetworkPeer).CloseConnection();
				GetTree().NetworkPeer = null;
			} catch (Exception e) {
				GD.Print($"Cought exception while removing ENet: {e}");
			}

		}
		isHost = false;
		hostButtonCheck.Hide();

		g.loadingScreen.Show();
		g.animPack.AlphaTween(g.loadingScreen, 1f, .5f);

		g.LoadingTimer(g.loadingScreen, 1f, 0f, .5f, true, "mainMenu");
	}

	[Remote]
	public void SetPreferances(float dur, int dif) {
		roundDuration = dur;
		difficulty = dif;

		int tgtIt = roundDuration == 30f ? 1 :
            roundDuration == 60f ? 2 :
            roundDuration == 90f ? 3 : 4 ;

		GD.Print(dur);
		GD.Print(dif);

		g.SetDuration((Node2D)g.durationButtons[tgtIt]);
        g.SetDifficulty();
	}

	[Remote]
	public void ChangeDuration(float dur) {
		roundDuration = dur;

		int tgtIt = roundDuration == 30f ? 1 :
            roundDuration == 60f ? 2 :
            roundDuration == 90f ? 3 : 4 ;

		g.SetDuration((Node2D)g.durationButtons[tgtIt]);
	}

	[Remote]
	public void ChangeDifficulty(int dif) {
		difficulty = dif;

		g.SetDifficulty();
	}

	public bool deniedCheck = false;
	[Remote]
	public void RequestDenied(string playerName) {
		deniedCheck = true;
		(mpRequestPrompt.GetNode("NamePrompt/Label") as Label).Text = $"{playerName}\ndenied your request";

		mpRequestPrompt.Show();
		AlphaTween(mpRequestPrompt, 1f, .5f);
		loadingIndicator.Modulate = new Color(1f,1f,1f,0f);
		(loadingIndicator.GetNode("Label") as Label).Text = "Finding you a friend";
	}

	[Remote]
	public void OpponentStep(int timingScore) {
		g.ScoreDisplayAndAnimate(timingScore);
	}

	[Remote]
	public void RoundStart() {
		RoundTimer();
	}
	[Remote]
	public void RoundStop() {
		ResetRound();
	}

	[Remote]
	public void RoundFinished(int finalScore) {
		g.oppScoreLabel.Text = finalScore.ToString();
	}











	//___________________________________________________________________________








	public override void _Ready() {
		if (deviceName == "localhost") deviceName = "pollyCat" ;
		AddChild(alphaTween);


		// binding global script
		g = (Global)GetNode("/root/Global");





		// everything multiplayer
		// broadcast timer
		AddChild(bt);
		// connection timer
		AddChild(ct);
		// nodes
		hostButton = (StaticBody2D)GetNode("/root/Menu/MpGroup/HostButton");
		hostButtonCheck = (Sprite)hostButton.GetNode("Check");


		GetTree().Connect("network_peer_connected", this, "PlayerConnected");
		GetTree().Connect("network_peer_disconnected", this, "PlayerDisconnected");
		GetTree().Connect("connected_to_server", this, "ConnectedToServer");
		GetTree().Connect("connection_failed", this, "ConnectionFailed");
		GetTree().Connect("server_disconnected", this, "ServerDisconnected");

		hostButton.Connect("input_event", this, "HostServer");


		mpListContainer = (ScrollContainer)GetNode("/root/Menu/MpGroup/ScrollContainer");
        mpList = (Control)mpListContainer.GetNode("List");

		loadingIndicator = (Node2D)GetNode("/root/Menu/MpGroup/Loading");

		mpRequestPrompt = (Node2D)GetNode("/root/Menu/MpRequestPrompt");
		mpRequestPrompt.Hide();
		mpRequestPrompt.Modulate = new Color(1f,1f,1f,0f);





		// quickmenu
		quickSettingsButton = (StaticBody2D)GetNode("SettingsButton");
		quickSettingsSprite = (AnimatedSprite)quickSettingsButton.GetNode("Sprite");
		quickSettingsButtonIcon = (AnimatedSprite)quickSettingsButton.GetNode("Icon");
        quickSettingsButton.Connect("input_event", this, "ToggleQuickSettings");

		// timers and everything related
		//-- round timer
		roundTimer = new Timer();
		roundTimer.OneShot = true;
		AddChild(roundTimer);
		roundTimer.Connect("timeout", this, nameof(RoundTimeout));
		//-- timer for simon buttons with random delay(time span set in method)
		simonSaysTimer = new Timer();
		simonSaysTimer.OneShot = true;
		AddChild(simonSaysTimer);
		//-- creating dynamic
		simonSaysTimer.Connect("timeout", this, nameof(SimonSaysTimeout));


		// bind value thats referenced more than once
		int simonButtonCount = simonButtonList.Count;
		// add individual button count to array
		//-- this is used as a reset
		idleButtons = new int[simonButtonCount];
		for (int i = 1; i <= simonButtonCount; i++) {
			idleButtons[i-1] = i;
		}
		idleButtonList.AddRange(idleButtons);


		// try to load saved scores or catch error
		// highscores
		// try {
		// 	// scoreSaved60 = true;

		// 	FileStream inStream = new FileStream("userdatahs.set", FileMode.Open, FileAccess.Read);
		// 	BinaryReader reader = new BinaryReader(inStream);

		// 	int dataScoresCount = reader.ReadInt32();

		// 	GD.Print($"scores count: {dataScoresCount}");

		// 	savedSteps60.Clear();

		// 	for (int i = 0; i < dataScoresCount; i++) {
		// 		int userScore = reader.ReadInt32();
		// 		string userName = reader.ReadString();
		// 		float roundDur = reader.ReadSingle();
		// 		string roundDifficulty = reader.ReadString();
		// 		int roundDiff = roundDifficulty == "Easy" ? 0 :
		// 			roundDifficulty == "Normal" ? 1 :
		// 			roundDifficulty == "Hard" ? 2 : 3 ;

		// 		// GD.Print($"loaded: {i} name: {userName} with {userScore}p");

		// 		HighscoreEntry newEntry = new HighscoreEntry(userScore, userName, roundDur, roundDiff);

		// 		highscoreList.Add(newEntry);
		// 	}

		// 	reader.Dispose();
		// 	// g.menu.UpdateHighscores();
		// } catch (System.IO.FileNotFoundException) {
		// 	GD.Print("No user data found at userdatahs");
		// } catch (Exception e) {
		// 	GD.Print($"Failed to load data from userdatahs: {e}");
		// }



		// // different durations
		// try {
		// 	scoreSaved30 = true;

		// 	FileStream inStream = new FileStream("userdata30.set", FileMode.Open, FileAccess.Read);
		// 	BinaryReader reader = new BinaryReader(inStream);

		// 	int dataStepsCount = reader.ReadInt32();

		// 	GD.Print($"steps count: {dataStepsCount}");

		// 	savedSteps30.Clear();

		// 	for (int i = 0; i < dataStepsCount; i++) {
		// 		float dataTimeStamp = reader.ReadSingle();
		// 		int dataStepPoints = reader.ReadInt32();

		// 		// GD.Print($"loaded: {dataStepPoints} at {dataTimeStamp}s");

		// 		ActionStep saveStep = new ActionStep(dataTimeStamp, dataStepPoints);

		// 		savedSteps30.Add(saveStep);
		// 	}

		// 	reader.Dispose();

		// } catch (System.IO.FileNotFoundException) {
		// 	GD.Print("No user data found for userdata30");
		// } catch (Exception e) {
		// 	GD.Print($"Failed to load data from userdata30: {e}");
		// }

		// try {
		// 	scoreSaved60 = true;

		// 	FileStream inStream = new FileStream("userdata60.set", FileMode.Open, FileAccess.Read);
		// 	BinaryReader reader = new BinaryReader(inStream);

		// 	int dataStepsCount = reader.ReadInt32();

		// 	GD.Print($"steps count: {dataStepsCount}");

		// 	savedSteps60.Clear();

		// 	for (int i = 0; i < dataStepsCount; i++) {
		// 		float dataTimeStamp = reader.ReadSingle();
		// 		int dataStepPoints = reader.ReadInt32();

		// 		// GD.Print($"loaded: {dataStepPoints} at {dataTimeStamp}s");

		// 		ActionStep saveStep = new ActionStep(dataTimeStamp, dataStepPoints);

		// 		savedSteps60.Add(saveStep);
		// 	}

		// 	reader.Dispose();

		// } catch (System.IO.FileNotFoundException) {
		// 	GD.Print("No user data found for userdata60");
		// } catch (Exception e) {
		// 	GD.Print($"Failed to load data from userdata60: {e}");
		// }

		// try {
		// 	scoreSaved90 = true;

		// 	FileStream inStream = new FileStream("userdata90.set", FileMode.Open, FileAccess.Read);
		// 	BinaryReader reader = new BinaryReader(inStream);

		// 	int dataStepsCount = reader.ReadInt32();

		// 	GD.Print($"steps count: {dataStepsCount}");

		// 	savedSteps90.Clear();

		// 	for (int i = 0; i < dataStepsCount; i++) {
		// 		float dataTimeStamp = reader.ReadSingle();
		// 		int dataStepPoints = reader.ReadInt32();

		// 		// GD.Print($"loaded: {dataStepPoints} at {dataTimeStamp}s");

		// 		ActionStep saveStep = new ActionStep(dataTimeStamp, dataStepPoints);

		// 		savedSteps90.Add(saveStep);
		// 	}

		// 	reader.Dispose();

		// } catch (System.IO.FileNotFoundException) {
		// 	GD.Print("No user data found for userdata90");
		// } catch (Exception e) {
		// 	GD.Print($"Failed to load data from userdata90: {e}");
		// }

		// try {
		// 	scoreSaved120 = true;

		// 	FileStream inStream = new FileStream("userdata120.set", FileMode.Open, FileAccess.Read);
		// 	BinaryReader reader = new BinaryReader(inStream);

		// 	int dataStepsCount = reader.ReadInt32();

		// 	GD.Print($"steps count: {dataStepsCount}");

		// 	savedSteps120.Clear();

		// 	for (int i = 0; i < dataStepsCount; i++) {
		// 		float dataTimeStamp = reader.ReadSingle();
		// 		int dataStepPoints = reader.ReadInt32();

		// 		// GD.Print($"loaded: {dataStepPoints} at {dataTimeStamp}s");

		// 		ActionStep saveStep = new ActionStep(dataTimeStamp, dataStepPoints);

		// 		savedSteps120.Add(saveStep);
		// 	}

		// 	reader.Dispose();

		// } catch (System.IO.FileNotFoundException) {
		// 	GD.Print("No user data found for userdata120");
		// } catch (Exception e) {
		// 	GD.Print($"Failed to load data from userdata120: {e}");
		// }
	}




	//****
	//____ Timers, timer methods and everything related ____
	//_____________________________________________________________
	//
	// defining timer names
	public Timer roundTimer, simonSaysTimer;

	// idle buttons array and list
	public int[] idleButtons;
	public List<int> idleButtonList = new List<int>();

	// main round timer
	public void RoundTimer() {

		// reset values
		userScore = 0;
		oppScore = 0;
		roundTimer.WaitTime = roundDuration;
		g.oppScoreLabel.Text = oppScore.ToString();

		currSteps.Clear();
		idleButtonList.Clear();
		idleButtonList.AddRange(idleButtons);

		// just a separator for console
		// GD.Print("-------------------_________________");

		roundTimer.Start();
		SimonSaysTimer();
		quickSettingsButton.InputPickable = false;
		quickSettingsButton.Modulate = new Color(.6f,.6f,.6f,1f);
	}

	// standard value if no highscore is set
	public bool scoreSaved30 = false;
	public bool scoreSaved60 = false;
	public bool scoreSaved90 = false;
	public bool scoreSaved120 = false;

	public void RoundTimeout() {
		if (g.mpGame) {
			Rpc("RoundFinished", userScore);
		}
		ResetRound();

		// add final step
		ActionStep finalStep = new ActionStep(-1,0);
		currSteps.Add(finalStep);

		if (highscoreList.Count <= 20) {
			HighscoreEntry newEntry = new HighscoreEntry(userScore, g.userName, roundDuration, difficulty);
			GD.Print(g.userName);
			highscoreList.Add(newEntry);
			highscoreList.Sort((a, b) => -a.userScore.CompareTo(b.userScore));
			g.userPref.bestScore = highscoreList[0].userScore;
			g.userPref.WriteLocalUserPref();
			// LocalWriteHighscore();
		} else {
			float lowestEntry = 0;
			int lowestEntryCount = 1;
			for (int i = 1; i <= highscoreList.Count; i++) {
				if (lowestEntry <= highscoreList[i].userScore) {
					lowestEntry = highscoreList[i].userScore;
					lowestEntryCount = i;
				}
			}
			if (lowestEntry > userScore) {
				highscoreList[lowestEntryCount] = new HighscoreEntry(userScore, g.userName, roundDuration, difficulty);
				highscoreList.Sort((a, b) => -a.userScore.CompareTo(b.userScore));
				g.userPref.bestScore = highscoreList[0].userScore;
				// g.userPref.WriteLocalUserPref();
				// LocalWriteHighscore();
			}
		}

		if (g.mpGame) {
			try {
				oppScore = (Int32)highscoreList[0].userScore;
				if (userScore > (Int32)highscoreList[0].userScore) {
					g.oppNameLabel.Text = userScore + "p";
				}
			} catch {
				oppScore = 0;
				GD.Print("No scores recorded");
			}
		}

		// save steps to 'savedSteps' list if a new or no highscore is set
		if (roundDuration == 30f) {
			if (userScore > oppScore || !scoreSaved30) {
				scoreSaved30 = true;

				// // create or update user data file
				// try {
				// 	FileStream outStream = new FileStream("userdata30.set", FileMode.Create, FileAccess.Write);

				// 	LocalWrite(outStream);

				// } catch (Exception e) {
				// 	GD.Print($"Failed to save data to userdata30.set: {e}");
				// }

				savedSteps30.Clear();

				for (int i = 0; i < currSteps.Count; i++) {
					ActionStep saveStep = new ActionStep(currSteps[i].timeStamp, currSteps[i].stepPoints);

					savedSteps30.Add(saveStep);
				}
			}

		} else if (roundDuration == 60f) {
			if (userScore > oppScore || !scoreSaved60) {
				scoreSaved60 = true;

				// // create or update user data file
				// try {
				// 	FileStream outStream = new FileStream("userdata60.set", FileMode.Create, FileAccess.Write);

				// 	LocalWrite(outStream);

				// } catch (Exception e) {
				// 	GD.Print($"Failed to save data to userdata60.set: {e}");
				// }

				savedSteps60.Clear();

				for (int i = 0; i < currSteps.Count; i++) {
					ActionStep saveStep = new ActionStep(currSteps[i].timeStamp, currSteps[i].stepPoints);

					savedSteps60.Add(saveStep);
				}
			}
		} else if (roundDuration == 90f) {
			if (userScore > oppScore || !scoreSaved30) {
				scoreSaved90 = true;

				// // create or update user data file
				// try {
				// 	FileStream outStream = new FileStream("userdata90.set", FileMode.Create, FileAccess.Write);

				// 	LocalWrite(outStream);

				// } catch (Exception e) {
				// 	GD.Print($"Failed to save data to userdata90.set: {e}");
				// }

				savedSteps90.Clear();

				for (int i = 0; i < currSteps.Count; i++) {
					ActionStep saveStep = new ActionStep(currSteps[i].timeStamp, currSteps[i].stepPoints);

					savedSteps90.Add(saveStep);
				}
			}
		} else if (roundDuration == 120f) {
			if (userScore > oppScore || !scoreSaved30) {
				scoreSaved120 = true;

				// // create or update user data file
				// try {
				// 	FileStream outStream = new FileStream("userdata120.set", FileMode.Create, FileAccess.Write);

				// 	LocalWrite(outStream);

				// } catch (Exception e) {
				// 	GD.Print($"Failed to save data to userdata120.set: {e}");
				// }

				savedSteps120.Clear();

				for (int i = 0; i < currSteps.Count; i++) {
					ActionStep saveStep = new ActionStep(currSteps[i].timeStamp, currSteps[i].stepPoints);

					savedSteps120.Add(saveStep);
				}
			}
		}
	}

	// public void LocalWrite(FileStream outStream) {
	// 	BinaryWriter writer = new BinaryWriter(outStream);

	// 	writer.Write((Int32)currSteps.Count);

	// 	GD.Print($"steps saved: {currSteps.Count}");

	// 	for (int i = 0; i < currSteps.Count; i++) {
	// 		writer.Write((Single)currSteps[i].timeStamp);
	// 		writer.Write((Int32)currSteps[i].stepPoints);
	// 	}

	// 	writer.Dispose();
	// }

	// public void LocalWriteHighscore() {
	// 	try {
	// 		FileStream outStream = new FileStream("userdatahs.set", FileMode.Create, FileAccess.Write);

	// 		BinaryWriter writer = new BinaryWriter(outStream);

	// 		writer.Write((Int32)highscoreList.Count);

	// 		GD.Print($"highscores saved: {highscoreList.Count}");

	// 		for (int i = 0; i < highscoreList.Count; i++) {
	// 			writer.Write((Int32)highscoreList[i].userScore);
	// 			writer.Write((string)highscoreList[i].userName);
	// 			writer.Write((Single)highscoreList[i].roundDur);
	// 			writer.Write((string)highscoreList[i].roundDifficulty);
	// 		}

	// 		writer.Dispose();

	// 	} catch (Exception e) {
	// 		GD.Print($"Failed to save data to userdatahs.set: {e}");
	// 	}
	// }

	// resets timers and buttons
	public void ResetRound() {

		quickSettingsButton.InputPickable = true;
        quickSettingsButton.Modulate = new Color(1f,1f,1f,1f);

		// stop all local timers
		simonSaysTimer.Stop();
		roundTimer.Stop();

		// stop and reset all simon buttons
		for (int i = 1; i <= idleButtons.Length; i++) {
			simonButtonList[i-1].el.ResetState();
		}

		// reset button label and round values
		g.startRoundBtn.label.Text = "Start";
		g.stepCount = 0;
		g.stressTickCount = g.stressTick;

		g.animPack.PolygonScale(g.oppPolygons, 0f, .7f, true);
		g.animPack.PolygonScale(g.userPolygons, 0f, 1f, true);
		g.animPack.AlphaTween(g.userPolygons, 1f, .5f);
		g.animPack.AlphaTween(g.oppPolygons, 1f, .6f);
	}


	// if set to 'true', does not restart the timer when no Simon buttons are available
	public bool simonSaysWait = false;

	// timer that activates random available buttons
	public void SimonSaysTimer() {

		int[] idleButtonArray = idleButtonList.ToArray();
		int idleButtonArrayLength = idleButtonArray.Length;


		if (idleButtonArrayLength > 0) {

			int[] randomValues = GenRandom(idleButtonArray, idleButtonArrayLength);
			float randomDelay = (float)randomValues[0] / 1000;
			int randomBtnNr = randomValues[1];

			simonSaysTimer.WaitTime = randomDelay;

			simonButtonList[randomBtnNr-1].el.ButtonStateTimer();
			idleButtonList.Remove(randomBtnNr);

			simonSaysTimer.Start();



		} else {
			simonSaysWait = true;
		}

	}

	public void SimonSaysTimeout() {
		if (simonSaysTimer.IsStopped() && !simonSaysWait) {
			SimonSaysTimer();
		}
	}


	// random method
	public int[] GenRandom(int[] a, int l) {

		int[] result = {1,1};


		// random delay
		result[0] = rndm.Next(simonSaysDelaySpan[0],simonSaysDelaySpan[1]);

		// random button
		result[1] = rndm.Next(0, l);
		result[1] = a[result[1]];


		return result;
	}


	// highscore list
	public List<HighscoreEntry> highscoreList = new List<HighscoreEntry>();

	public class HighscoreEntry {
		public float userScore { get; set; }
		public string userName { get; set; }
		public float roundDur { get; set; }
		public string roundDifficulty { get; set; }

		// constructor
		public HighscoreEntry(float s, string n, float dur, int dif) {
			this.userScore = s;
			this.userName = n;
			this.roundDur = dur;
			this.roundDifficulty = dif == 0 ? "Easy" :
				dif == 1 ? "Normal" :
				dif == 2 ? "Tough" : "Hard" ;
		}
	}

	// action lists, current and saved

	public List<ActionStep> currSteps = new List<ActionStep>();

	public List<ActionStep> savedSteps30 = new List<ActionStep>();
	public List<ActionStep> savedSteps60 = new List<ActionStep>();
	public List<ActionStep> savedSteps90 = new List<ActionStep>();
	public List<ActionStep> savedSteps120 = new List<ActionStep>();


	// action step class, saving a timeStamp, timingScore and the totalScore
	public class ActionStep {
		public float timeStamp { get; set; }
		public int stepPoints { get; set; }
		public int totalScore { get; private set; }

		// constructor
		public ActionStep(float t, int p) {
			this.timeStamp = t;
			this.stepPoints = p;
			this.totalScore += p;
		}
	}




	// instance new lists
	public List<SimonButton> simonButtonList = new List<SimonButton>();


	// base class for lists
	public class GodotElement {
		public GodotElement() {
		}
	}


	// list class for simon buttons
	public class SimonButton : GodotElement {
		public SimonBtn el;
		public SimonButton(SimonBtn el) {
			this.el = el;
		}
	}



	public bool quickMenuVisible = false;
	public Tween alphaTween = new Tween();
	public void ToggleQuickSettings(Viewport viewport, InputEvent ev, int shape_idx) {
        if (ev is InputEventScreenTouch && (ev as InputEventScreenTouch).Pressed) {
			quickSettingsSprite.Animation = "pressed";
			g.InputPressed(quickSettingsButton);
			quickSettingsButton.Scale -= new Vector2(.03f,.03f);

			if (!quickMenuVisible) {
				ShowQuickMenu();
			} else if (quickMenuVisible) {
				HideQuickMenu();
			}
        }
    }

	// separate methods that can be called from other nodes
	public void ShowQuickMenu() {
		quickMenuVisible = true;
		quickSettingsButtonIcon.Animation = "cross";
		g.quickMenuContainer.Show();
		AlphaTween(g.quickMenuContainer, 1f, .5f);
	}
	public void HideQuickMenu() {
		quickMenuVisible = false;
		quickSettingsButtonIcon.Animation = "burger";
		AlphaTween(g.quickMenuContainer, 0f, .5f, true);
	}

	// public void WaitTimer() {
	// 	Timer t = new Timer();
	// 	AddChild(t);
	// 	t.OneShot = true;
	// 	t.WaitTime = .4f;
	// 	t.Start();
	// }

	public async void AlphaTween(Node2D el, float end, float delay, bool hide = false) {
		alphaTween.StopAll();
		alphaTween.InterpolateProperty(el, "modulate", el.Modulate, new Color(1f, 1f, 1f, end), delay, Tween.TransitionType.Quint, Tween.EaseType.Out);
		alphaTween.Start();

		await ToSignal(alphaTween, "tween_completed");
		if (hide && !quickMenuVisible) {
			g.quickMenuContainer.Hide();
		}
	}

	public void QuickMenuButtonRelease() {
		quickSettingsSprite.Animation = "idle";
		quickSettingsButton.Scale = new Vector2(1f,1f);
	}


//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta) {
//
//  }
}
