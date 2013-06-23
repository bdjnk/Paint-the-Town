using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameData : MonoBehaviour
{
	public GameObject player; // this client's "First Person Controller(Clone)"
	
	private bool ready = false;
	
	public Color red;
	public Color blue;
	public Color gray;
	
	public int redCount;
	public int blueCount;
	
	public int redScore = 0;
	public int blueScore = 0;
	
	public int redOwned = 0;
	public int blueOwned = 0;
	[RPC] public void AddRedOwned(int owned) { redOwned += owned; }
	[RPC] public void AddBlueOwned(int owned) { blueOwned += owned; }
	
	public int redPercent = 0;
	public int bluePercent = 0;
	
	public int percentToWin = 75;
	
	public int totalCubes = 0;
	[RPC] public void SetCubeCount(int cubeCount) { totalCubes = cubeCount; }
	
	public Vector3 mapCenter;
	public Vector3 extent;
	[RPC] public void SetMapExtents(Vector3 center, Vector3 far)
	{
		mapCenter = center;
		extent = far;
	}
	
	public List<GameObject> players;
	public List<NetworkPlayer> netPlayers;
	
	public GameObject[] bots;
	
	private GameObject winSound;
	private GameObject ground;
	
	public State state;
	public enum State
    {
		betweenGames,
		inGame
    };
	
	public LevelType levelType;
	public enum LevelType
	{
		space,
		sky
	};
	
	public float gameLength = 0;
	[RPC] public void SetGameLength(float length) { gameLength = length; }
	
	public float gameEndTime = 0;
	[RPC] private void SetEndTime(float endTime, int newState, int levelState)
	{
		gameEndTime = endTime;

		state = (State)newState;
		levelType = (LevelType)levelState;
		
		Instantiate(winSound, this.transform.position, Quaternion.identity);
		
		if (state == State.inGame) // starting a new round
		{
			ClearData(false);
			
			if (levelType == LevelType.space)
			{
				LoadSpace();
			} 
			if (levelType == LevelType.sky)
			{
				LoadSky();
			}
		}
	}
	
	private void LoadSpace()
	{
		Material mSky = Resources.Load("Skyboxes/DeepSpaceBlueWithPlanet/DSBWP") as Material;
		RenderSettings.skybox = mSky;
		ground = GameObject.Find("Ground(Clone)");
		
		if (ground != null) // disable it
		{
			ground.SetActive(false);
		}
		//player.GetComponentInChildren<CharacterMotor>().movement.maxFallSpeed = 5f;
	}
	
	private void LoadSky()
	{
		Material mSky = Resources.Load("Skyboxes/Sunny2/Sunny2 Skybox") as Material;
		RenderSettings.skybox = mSky;
		
		if (ground != null)
		{
			ground.SetActive(true);
		}
		if (player != null && player.transform.position.y < -0.5f) // keep player above ground
		{
			player.transform.position= new Vector3(player.transform.position.x, 3f, player.transform.position.z);
		}
		//player.GetComponentInChildren<CharacterMotor>().movement.maxFallSpeed = 20f;
	}
	
	bool countChanged = false;
	
	private void Update()
	{
		if (!ready) { return; }
		
		redPercent = (int)(100.0f * redScore/totalCubes);
		bluePercent = (int)(100.0f * blueScore/totalCubes);
		
		// -- code for rebalancing as necessary ---------------------------------------------------
		if (networkView.isMine && countChanged)
		{
			countChanged = false;
			
			PlayerManager best;
			PlayerManager worst;
			best = worst = player.GetComponent<PlayerManager>();
			
			if (blueCount > redCount+1) // if we have 2 more blue players than red players
			{
				players.RemoveAll(item => item == null);
				foreach (GameObject otherPlayer in players)
				{
					PlayerManager temp = otherPlayer.GetComponent<PlayerManager>();
					
					if (temp.myColor == blue)
					{
						if (worst.myColor == red || best.myColor == red)
						{
							best = worst = temp;
							continue;
						}
						if (temp.myScore < worst.myScore)
						{
							worst = temp;
						}
						else if (temp.myScore > best.myScore)
						{
							best = temp;
						}
					}
				}
				
				if (redScore >= blueScore)
				{
					best.JoinTeam(true); // true means we're switching colors
				}
				else
				{
					worst.JoinTeam(true);
				}
			}
			else if (redCount > blueCount+1)
			{
				players.RemoveAll(item => item == null);
				foreach (GameObject otherPlayer in players)
				{
					PlayerManager temp = otherPlayer.GetComponent<PlayerManager>();
					
					if (temp.myColor == red)
					{
						if (worst.myColor == blue || best.myColor == blue)
						{
							best = worst = temp;
							continue;
						}
						if (temp.myScore < worst.myScore)
						{
							worst = temp;
						}
						else if (temp.myScore > best.myScore)
						{
							best = temp;
						}
					}
				}
				
				if (blueScore > redScore)
				{
					best.JoinTeam(true); // true means we're switching colors
				}
				else
				{
					worst.JoinTeam(true);
				}
			}
		}
		
		// -- code for ending and restarting rounds -----------------------------------------------
		if (Network.isServer)
		{
			if (state == State.inGame)
			{
				if ((redPercent >= percentToWin || bluePercent >= percentToWin)
					|| (gameLength != 0 && gameEndTime > 0 && Network.time > gameEndTime)
					|| (redOwned > totalCubes/2 || blueOwned > totalCubes/2))
				{
					state = State.betweenGames;
					networkView.RPC("SetEndTime", RPCMode.AllBuffered, (float)Network.time + 8, (int)state, (int)levelType);
				}
			}
			else if (state == State.betweenGames && Network.time > gameEndTime)
			{
				Network.RemoveRPCs(networkView.owner);
				foreach(NetworkPlayer netPlayer in netPlayers)
				{
					GetComponent<MenuManager>().ClearServer(netPlayer);
					GetComponent<MenuManager>().networkView.RPC("ClearClient", RPCMode.All);
				}
				
				GetComponent<PG_Map>().CreateMap();
				networkView.RPC("SetEndTime", RPCMode.AllBuffered, (float)Network.time + gameLength, (int)state, (int)levelType);
			}
		}
	}
	
	private void OnEnable()
	{
		if (Network.isServer)
		{
			levelType = (LevelType) PlayerPrefs.GetInt("serverType", (int) LevelType.sky);
			Debug.Log("level: " + levelType.ToString() + "  " + (int) levelType);
			int intLevelType = (int) levelType;
			networkView.RPC("SetEndTime", RPCMode.AllBuffered, (float)Network.time + gameLength, (int)State.inGame, intLevelType);
		}
		bots = GameObject.FindGameObjectsWithTag("Bot");
		
		state = State.inGame;
	}
	
	private void Awake()
	{
		red = new Color(1, 0.4f, 0.4f);
		blue = new Color(0.4f, 0.6f, 1);
		gray = new Color(0.8f, 0.8f, 0.8f);
		
		ClearData(true);
		
		winSound = Resources.Load("Prefabs/Winner") as GameObject;
		
	}
	
	public Vector3 GetTeam(GameObject newPlayer, bool switching)
	{
		player = newPlayer;
		networkView.RPC("AddPlayer", RPCMode.AllBuffered, player.networkView.viewID);
		
		ready = true;
		
		if (redCount < blueCount)
		{
			if (switching)
			{
				networkView.RPC("LeaveBlue", RPCMode.AllBuffered);
			}
			networkView.RPC("JoinRed", RPCMode.AllBuffered);
			return new Vector3(red.r, red.g, red.b);
		}
		else // blueCount <= redCount
		{
			if (switching)
			{
				networkView.RPC("LeaveRed", RPCMode.AllBuffered);
			}
			networkView.RPC("JoinBlue", RPCMode.AllBuffered);
			return new Vector3(blue.r, blue.g, blue.b);
		}
	}
	
	[RPC] private void AddPlayer(NetworkViewID id)
	{
		GameObject player = NetworkView.Find(id).gameObject;
		players.Add(player);
		netPlayers.Add(player.networkView.owner);
	}
	
	[RPC] private void JoinRed() { redCount++; countChanged = true; }
	[RPC] private void JoinBlue() { blueCount++; countChanged = true; }
	
	[RPC] public void LeaveRed() { redCount--; countChanged = true; }
	[RPC] public void LeaveBlue() { blueCount--; countChanged = true; }
	
	[RPC] public void RemovePlayer(NetworkPlayer netPlayer)
	{
		players.RemoveAll(item => item == null);
		netPlayers.Remove(netPlayer);
	}
	
	[RPC] public void ClearData(bool all)
	{
		redScore = 0;
		blueScore = 0;
		redOwned = 0;
		blueOwned = 0;
	  	redPercent = 0;
	  	bluePercent = 0;
		
		if (all)
		{
			redCount = 0;
			blueCount = 0;
	  		totalCubes = 0;
			ready = false;
			gameLength = 0;
			gameEndTime = 0;
			mapCenter = Vector3.zero;
			players = new List<GameObject>(20);
			netPlayers = new List<NetworkPlayer>(20);
			enabled = false;
		}
	}
}