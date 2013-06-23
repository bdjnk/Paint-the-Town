using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
	// Menu GUI Variables
	public GUISkin skin; // general style for all elements
	private GUIStyle style; // style for small corrections as needed
	
    private Vector2 scrollPosition = Vector2.zero;
	
	public int serverHeight = 25;
	private float edging = 0.01f;
	private float edge;
	
	private int innerWidth;
	private int scrollBarWidth = 17;
	
	// Server Info Variables
	private int[] citySize = {3, 3, 3};
    private int[] minBuildingSize = {1, 1, 1};
    private int[] maxBuildingSize = {3, 3, 3};
	private int maxPlayers = 10;
	private bool bots = true;
	private bool upgrades = true;
	private bool aggressive = true;
	private bool floor = true;
	private bool timed = false;
	private float timer = 2;
	private float spacing = 1.5f;
	
	// Server / Host Variables
	private string gameName = "123Paint the Town123";
	private HostData[] hostData = null;
	
	private string serverName;
	private string playerName;
	
	private int levelType; //0 is space,1 is sky (earth)
	
	// State Variables
	private int firstRun = 0;
	
	private State state;
	private enum State
    {
		list,
		create,
		play
    };
	
	private void Awake()
	{
		firstRun = PlayerPrefs.GetInt("firstRun");
		if (firstRun == 0)
		{
			PlayerPrefs.SetInt("firstRun", 1);
		}
		else
		{
			// do stuff maybe...
		}
		playerName = PlayerPrefs.GetString("playerName", "Player Name");
		serverName = PlayerPrefs.GetString("serverName", "Server Name");
		
		maxPlayers = PlayerPrefs.GetInt("maxPlayers", 10);
		bots = System.Convert.ToBoolean(PlayerPrefs.GetInt("hasBots", 1));
		upgrades = System.Convert.ToBoolean(PlayerPrefs.GetInt("hasUpgrades", 1));
		aggressive = System.Convert.ToBoolean(PlayerPrefs.GetInt("aggressive", 1));
		floor = System.Convert.ToBoolean(PlayerPrefs.GetInt("hasFloor", 1));
		timed = System.Convert.ToBoolean(PlayerPrefs.GetInt("isTimed", 0));
		timer = PlayerPrefs.GetFloat("timer", 2);
		spacing = PlayerPrefs.GetFloat("buildingSpacing", 1.0f);
		citySize[0] = PlayerPrefs.GetInt("citySizeX", 4);
		citySize[1] = PlayerPrefs.GetInt("citySizeY", 2);
		citySize[2] = PlayerPrefs.GetInt("citySizeZ", 3);
		minBuildingSize[0] = PlayerPrefs.GetInt("minBuildingSizeX", 1);
		minBuildingSize[1] = PlayerPrefs.GetInt("minBuildingSizeY", 1);
		minBuildingSize[2] = PlayerPrefs.GetInt("minBuildingSizeZ", 1);
		maxBuildingSize[0] = PlayerPrefs.GetInt("maxBuildingSizeX", 3);
		maxBuildingSize[1] = PlayerPrefs.GetInt("maxBuildingSizeY", 2);
		maxBuildingSize[2] = PlayerPrefs.GetInt("maxBuildingSizeZ", 3);
		
		state = State.list;
		
		style = new GUIStyle();
		style.normal.textColor = Color.white;
	}
	
	private void Start()
	{
		MasterServer.RequestHostList(gameName);
	}
	
	private void OnGUI()
	{
		GUI.skin = skin;
		edge = edging * Screen.width;
		
		switch(state)
		{
		case State.list:
			ListServers();
			break;
		case State.create:
			CustomizeServer();
			break;
		case State.play:
			break;
		}
	}
	
	public int sliderHeight = 22;
	
	float FloatSlider(Rect rect, float val, float min, float max, string label, float step, bool dynamic)
	{
		Vector2 size = skin.GetStyle("Label").CalcSize(new GUIContent(max.ToString("#0.0")));
		style.alignment = TextAnchor.MiddleRight;
		GUI.Label(new Rect(rect.x, rect.y, size.x, rect.height), val.ToString("#0.0"), style);
		
		float width = dynamic ? (max-min+step)*10/step : rect.width;
		GUI.Label(new Rect(rect.x+(width)+(size.x+5)+5, rect.y, rect.width+90-(size.x+5)-5, rect.height), label);
		
		float f = GUI.HorizontalSlider(new Rect(rect.x+(size.x+5), rect.y+(rect.height-12)/2, width, 12), val, min, max);
		val = Mathf.Round(f / step) * step;
		return val;
	}
	
	int IntSlider(Rect rect, int val, int min, int max, string label, int step, bool dynamic)
	{
		Vector2 size = skin.GetStyle("Label").CalcSize(new GUIContent(max.ToString()));
		style.alignment = TextAnchor.MiddleRight;
		GUI.Label(new Rect(rect.x, rect.y, size.x, rect.height), val.ToString(), style);
		
		float width = dynamic ? (max-min+step)*10/step : rect.width;
		GUI.Label(new Rect(rect.x+(width)+(size.x+5)+5, rect.y, rect.width-90-(size.x+5)-5, rect.height), label);
		
		float f = GUI.HorizontalSlider(new Rect(rect.x+(size.x+5), rect.y+(rect.height-12)/2, width, 12), val, min, max);
		val = Mathf.RoundToInt(f / (float)step) * step;
		return val;
	}
	
	private void CustomizeServer() // CREATE A NEW SERVER
	{
		GUI.BeginGroup(new Rect(Screen.width/2-200, edge, 400, sliderHeight*15+edge*3), skin.box);
		
		GUI.Label(new Rect(edge, sliderHeight*0+edge, 100, sliderHeight), "Server Name: ");
		serverName = GUI.TextField(new Rect(edge+100, sliderHeight*0+edge, 300-edge*2, sliderHeight), serverName);
		
		timed = GUI.Toggle(new Rect(edge, sliderHeight*2+edge, 100, sliderHeight), timed, " Timed Game");
		timer = FloatSlider(new Rect(edge+100, sliderHeight*2+edge, 200, sliderHeight), timer, 0.2f, 4, "Minutes", 0.1f, false);
		//timer = IntSlider(new Rect(edge+100, sliderHeight*2+edge, 200, sliderHeight), timer, 10, 480, "Seconds", 10, false);
		
		GUI.Label(new Rect(edge, sliderHeight*4+edge, 200, sliderHeight), "City Dimensions");
		citySize[0] = IntSlider(new Rect(edge, sliderHeight*5+edge, 200, sliderHeight), citySize[0], 1, 9, "Width", 1, true);
		citySize[1] = IntSlider(new Rect(edge, sliderHeight*6+edge, 200, sliderHeight), citySize[1], 1, 9, "Height", 1, true);
		citySize[2] = IntSlider(new Rect(edge, sliderHeight*7+edge, 200, sliderHeight), citySize[2], 1, 9, "Depth", 1, true);
		
		maxPlayers = IntSlider(new Rect(edge+200, sliderHeight*4+edge, 200, sliderHeight), maxPlayers, 2, 14, "Max Players", 2, true);
		
		floor = GUI.Toggle(new Rect(edge+300, sliderHeight*5+edge, 100, sliderHeight), floor, " Floor");
		upgrades = GUI.Toggle(new Rect(edge+200, sliderHeight*5+edge, 100, sliderHeight), upgrades, " Upgrades");
		bots = GUI.Toggle(new Rect(edge+300, sliderHeight*6+edge, 100, sliderHeight), bots, " Bots");
		aggressive = GUI.Toggle(new Rect(edge+200, sliderHeight*6+edge, 100, sliderHeight), aggressive, " Aggressive");
		
		spacing = FloatSlider(new Rect(edge+200, sliderHeight*7+edge, 85, sliderHeight), spacing, 0, 9, "Spacing", 0.2f, false);
		
		
		GUI.Label(new Rect(edge, sliderHeight*9+edge, 300, sliderHeight), "Minimum Building Dimensions");
		minBuildingSize[0] = IntSlider(new Rect(edge, sliderHeight*10+edge, 200, sliderHeight), minBuildingSize[0], 1, 9, "Width", 1, true);
		if (minBuildingSize[0] > maxBuildingSize[0]) { maxBuildingSize[0] = minBuildingSize[0]; }
		minBuildingSize[1] = IntSlider(new Rect(edge, sliderHeight*11+edge, 200, sliderHeight), minBuildingSize[1], 1, 9, "Height", 1, true);
		if (minBuildingSize[1] > maxBuildingSize[1]) { maxBuildingSize[1] = minBuildingSize[1]; }
		minBuildingSize[2] = IntSlider(new Rect(edge, sliderHeight*12+edge, 200, sliderHeight), minBuildingSize[2], 1, 9, "Depth", 1, true);
		if (minBuildingSize[2] > maxBuildingSize[2]) { maxBuildingSize[2] = minBuildingSize[2]; }
		
		GUI.Label(new Rect(edge+200, sliderHeight*9+edge, 200, sliderHeight), "Maximum Building Dimensions");
		maxBuildingSize[0] = IntSlider(new Rect(edge+200, sliderHeight*10+edge, 200, sliderHeight), maxBuildingSize[0], 1, 9, "Width", 1, true);
		if (maxBuildingSize[0] < minBuildingSize[0]) { minBuildingSize[0] = maxBuildingSize[0]; }
		maxBuildingSize[1] = IntSlider(new Rect(edge+200, sliderHeight*11+edge, 200, sliderHeight), maxBuildingSize[1], 1, 9, "Height", 1, true);
		if (maxBuildingSize[1] < minBuildingSize[1]) { minBuildingSize[1] = maxBuildingSize[1]; }
		maxBuildingSize[2] = IntSlider(new Rect(edge+200, sliderHeight*12+edge, 200, sliderHeight), maxBuildingSize[2], 1, 9, "Depth", 1, true);
		if (maxBuildingSize[2] < minBuildingSize[2]) { minBuildingSize[2] = maxBuildingSize[2]; }
		
		if (GUI.Button(new Rect(50, sliderHeight*14+edge, 100, sliderHeight), "Earth"))
		{
			levelType = 1;
			SavePreferences();
			state = State.play;
			CreateServer();
		}
		if (GUI.Button(new Rect(150, sliderHeight*14+edge, 100, sliderHeight), "Space"))
		{
			levelType = 0;
			SavePreferences();
			PlayerPrefs.SetInt("hasFloor", 0);
			state = State.play;
			CreateServer();
		}
		if (GUI.Button(new Rect(250, sliderHeight*14+edge, 100, sliderHeight), "Cancel"))
		{
			PlayerPrefs.SetString("serverName", serverName);
			state = State.list;
		}
		
		GUI.EndGroup();
	}
	
	private void ListServers() // LIST OF SERVERS, THIS IS THE INITIAL MENU PAGE
	{
		GUI.Box(new Rect(-9, -9, Screen.width+9, Screen.height+9), "");
		
		innerWidth = Screen.width-scrollBarWidth;
		
		if (GUI.Button(new Rect(10, 10, 65, 25), "Refresh")) // refresh our list of servers
		{
			MasterServer.RequestHostList(gameName);
		}
		GUI.Label(new Rect(90, 10, 200, 25), "the server list to join a game, or");
		if (GUI.Button(new Rect(290, 10, 80, 25), "create one."))
		{
			state = State.create;
		}
		
		GUI.Label(new Rect((Screen.width-edge)-200, 10, 40, 25), "Name:");
		playerName = GUI.TextField(new Rect((Screen.width-edge)-150, 10, 150, 25), playerName, 16);
		PlayerPrefs.SetString("playerName", playerName); // may be inefficient...
		
		GUI.BeginGroup(new Rect(edge, 50, innerWidth, 25));
			
		GUI.Label(new Rect(edge, 0, innerWidth-300, 25), "Name");
		GUI.Label(new Rect(innerWidth-300, 0, 100, 25), "Size");
		GUI.Label(new Rect(innerWidth-200, 0, 100, 25), "Players");
		GUI.Label(new Rect(innerWidth-100, 0, 100, 25), "Ping");
		
		GUI.EndGroup();
		
		hostData = MasterServer.PollHostList();
		if (hostData == null || hostData.Length == 0) // the server list is empty (or null)
		{
			GUI.Label(new Rect(edge, 80, 250, serverHeight), "No servers available, try refreshing.");
		}
		else
		{
			// this scrollbox's outer height is limited by the screen height in increments of serverHeight
			scrollPosition = GUI.BeginScrollView(
				new Rect(edge, 80, (Screen.width-edge*2),
					Mathf.Min(serverHeight*hostData.Length, Mathf.Round((Screen.height-60)/serverHeight)*serverHeight)),
				scrollPosition, new Rect(0, 0, innerWidth*0.98f, serverHeight*hostData.Length), false, true);
			
			for (int i = 0; i < hostData.Length; i++) // for each server we know about
			{
				GUI.BeginGroup(new Rect(0, serverHeight*i, innerWidth*0.98f, serverHeight), new GUIContent());
				
				if (GUI.Button(new Rect(0, 0, innerWidth-edge*2, serverHeight), ""))
				{
					state = State.play;
					Network.Connect(hostData[i]);
				}
				
				GUI.Label(new Rect(edge, 0, innerWidth-300, serverHeight), hostData[i].gameName);
				
				GUI.Label(new Rect(innerWidth-300, 0, 100, serverHeight), "Size");
				GUI.Label(new Rect(innerWidth-200, 0, 100, serverHeight), hostData[i].connectedPlayers.ToString());
				GUI.Label(new Rect(innerWidth-100, 0, 100, serverHeight), "Ping");
				
				GUI.EndGroup();
			}
			
	        GUI.EndScrollView();
		}
	}
	
	public void Update()
	{
		if (state != State.play)
		{
			return; // only proceed in a play state
		}
		
		if (Input.GetKeyUp(KeyCode.X))
		{
			Network.Disconnect();
			return;
		}
	}
	
	// ========================================= Server Handling (maybe move to seperate script)
	
	private void CreateServer()
	{
		Network.InitializeServer(maxPlayers, Random.Range(40770, 50770), !Network.HavePublicAddress());
		MasterServer.RegisterHost(gameName, serverName);
	}
	
	// Called on the server whenever a Network.InitializeServer was invoked and has completed.
	private void OnServerInitialized()
	{
		GetComponent<PG_Map>().CreateMap();
	}
	
	// Called on the client when you have successfully connected to a server.
	private void OnConnectedToServer()
	{
	}
	
	// Called on the server whenever a new player has successfully connected.
	private void OnPlayerConnected(NetworkPlayer netPlayer)
	{
	}
	
	// Called on the server whenever a player is disconnected from the server.
	private void OnPlayerDisconnected(NetworkPlayer netPlayer)
	{
		ClearServer(netPlayer);
		
		Network.DestroyPlayerObjects(netPlayer);
		GetComponent<GameData>().networkView.RPC("RemovePlayer", RPCMode.AllBuffered, netPlayer);
	}
	public void ClearServer(NetworkPlayer netPlayer)
	{
		//TODO these numbers should really be constants in a seprate static class
		Network.RemoveRPCs(netPlayer, 4); // player
		Network.RemoveRPCs(netPlayer, 3); // shots
		
		GameData gameData = GetComponent<GameData>();
		
		foreach (GameObject player in gameData.players)
		{
			if (player.networkView.owner == netPlayer)
			{
				if (player.GetComponent<PlayerManager>().myColor == gameData.red)
				{
					gameData.networkView.RPC("LeaveRed", RPCMode.AllBuffered);
				}
				else // color == blue
				{
					gameData.networkView.RPC("LeaveBlue", RPCMode.AllBuffered);
				}
				Network.RemoveRPCs(player.networkView.viewID);
			}
		}
	}
	
	// Called on client during disconnection from server, but also on the server when the connection has disconnected.
	private void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		Screen.lockCursor = false;
		Screen.showCursor = true;
		
		ClearClient();
		
		MasterServer.RequestHostList(gameName);
		state = State.list;
	}
	[RPC] public void ClearClient()
	{
		GetComponent<GameData>().ClearData(true);
		GetComponent<UpgradeManager>().enabled = false;
			
		foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			if (go.tag != "Master")
			{
				Destroy(go);
			}
		}
	}
	
	// Called on clients or servers when reporting events from the MasterServer.
	private void OnMasterServerEvent(MasterServerEvent mse)
	{
		Debug.Log(mse);
	}
	
	private void SavePreferences()
	{
		PlayerPrefs.SetString("serverName", serverName);
			
		PlayerPrefs.SetInt("maxPlayers", maxPlayers);
		
		PlayerPrefs.SetInt("hasBots", bots ? 1 : 0);
		PlayerPrefs.SetInt("hasUpgrades", upgrades ? 1 : 0);
		PlayerPrefs.SetInt("aggressive", aggressive ? 1 : 0);
		PlayerPrefs.SetInt("hasFloor", floor ? 1 : 0);
		
		PlayerPrefs.SetInt("isTimed", timed ? 1 : 0);
		PlayerPrefs.SetFloat("timer", timer);
		
		PlayerPrefs.SetFloat("buildingSpacing", spacing);
		
		PlayerPrefs.SetInt("citySizeX", citySize[0]);
		PlayerPrefs.SetInt("citySizeY", citySize[1]);
		PlayerPrefs.SetInt("citySizeZ", citySize[2]);
		
		PlayerPrefs.SetInt("minBuildingSizeX", minBuildingSize[0]);
		PlayerPrefs.SetInt("minBuildingSizeY", minBuildingSize[1]);
		PlayerPrefs.SetInt("minBuildingSizeZ", minBuildingSize[2]);
		
		PlayerPrefs.SetInt("maxBuildingSizeX", maxBuildingSize[0]);
		PlayerPrefs.SetInt("maxBuildingSizeY", maxBuildingSize[1]);
		PlayerPrefs.SetInt("maxBuildingSizeZ", maxBuildingSize[2]);
		
		PlayerPrefs.SetInt("serverType", levelType); //intLevel converts to levelType
	}
}
