using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
	private GameData gameData;
	public bool isServer;
	
	public int myScore = 0;
	private int myPercent = 0;
	public Color myColor;
	
	public PG_Gun gun;
	
	private void Awake()
	{
		gameData = GameObject.FindGameObjectWithTag("Master").GetComponent<GameData>();
		gun = GetComponentInChildren<PG_Gun>();
	}
	
	public bool ready = false;
	[RPC] public void Ready() { ready = true; }
	
	private void OnNetworkInstantiate (NetworkMessageInfo info)
	{
		networkView.RPC("Ready", RPCMode.All); // clones, we are ready!
	}
	
	public void Enable(bool state)
	{
		enabled = state;
		
		GetComponent<CharacterMotor>().enabled = state;
		GetComponent<FPSInputController>().enabled = state;
		
		GetComponentInChildren<Camera>().enabled = state;
		gun.enabled = state;
		
		GetComponentInChildren<AudioListener>().enabled = state;
		
		MouseEnable(state);
		
		gameData.GetComponent<UpgradeManager>().enabled = true;
		
		if (Network.isServer) // only gets called once...
		{
			networkView.RPC("SetAsServer", RPCMode.AllBuffered);
		}
	}
	[RPC] private void SetAsServer() { isServer = true; }
	
	public void MouseEnable(bool state)
	{
		Screen.lockCursor = state;
		
		foreach (MouseLook mouseLook in GetComponentsInChildren<MouseLook>())
		{
			mouseLook.enabled = state;
		}
	}
	
	public void JoinTeam(bool switching) // called once from PlayerSetup.Update()
	{		
		Vector3 color = gameData.GetTeam(gameObject, switching);
		networkView.RPC("SetColor", RPCMode.AllBuffered, color, networkView.viewID);
	}
	
	[RPC] private void SetColor(Vector3 color, NetworkViewID playerID)
	{
		NetworkView playerNetView = NetworkView.Find(playerID);
		
		if (playerNetView != null)
		{
			GameObject player = playerNetView.gameObject;
			
			if (player != null)
			{
				myColor = new Color(color.x, color.y, color.z);
				player.GetComponentInChildren<MeshRenderer>().material.color = myColor;
				
				string colorName = Mathf.Approximately(color.x, 1) ? "Red" : "Blue"; // if red ~= 255
				
				player.GetComponentInChildren<PG_Gun>().shotPrefab = Resources.Load("Prefabs/"+colorName+"Shot") as GameObject;
				player.tag = colorName;
				
				if (gameData.levelType == GameData.LevelType.space)
				{
					player.GetComponentInChildren<CharacterMotor>().movement.maxFallSpeed = 1f;
				}
				else
				{
					player.GetComponentInChildren<CharacterMotor>().movement.maxFallSpeed = 20f;
				}
			}
		}
	}
	
	private void Update()
	{
		myPercent = (int)(100.0f * myScore/gameData.totalCubes);
		
		if (Input.GetKeyUp(KeyCode.Q))
		{
			showHUD = !showHUD;
		}
		
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			MouseEnable(false);
		}
		
		if (gameData.state == GameData.State.inGame)
		{
			gun.enabled = true;
		}
		else //if (gameData.state == GameData.State.betweenGames)
		{
			gun.enabled = false;
		}
	}
	
	private bool showHUD = true;
	
	private void OnGUI()
	{
		float edge = Screen.width*0.02f;
		float buttonW = 100f;
		float buttonH = 70f;
		
		if (gameData.state == GameData.State.inGame && gameData.gameLength > 0)
		{
			GUI.Box(new Rect(Screen.width-buttonW-edge, Screen.height-buttonH*0.6f-edge, buttonW, buttonH*0.6f), "Countdown:\n"+Mathf.CeilToInt(gameData.gameEndTime-(float)Network.time));
		}
		
		if (gun.freezeTimeout > Network.time)
		{
			GUI.Box(new Rect(edge, 0.6f+edge, buttonW, buttonH*0.6f), "Gun Disabled:\n"+(gun.freezeTimeout-(float)Network.time+0.05f).ToString("#0.0"));
		}
		
		if (!showHUD) { return; } // else show HUD
		
		if (gameData.state == GameData.State.inGame)
		{
			if (tag == "Red") // display the lists
			{
				GUI.Box(new Rect(Screen.width-buttonW-edge, edge, buttonW, buttonH), "Red Team:\n"+gameData.redCount+" players\n"+gameData.redScore+" cubes\n"+gameData.redPercent+"%");
				GUI.Box(new Rect(Screen.width-buttonW-edge, edge*2+buttonH, buttonW, buttonH), "Blue Team:\n"+gameData.blueCount+" players\n"+gameData.blueScore+" cubes\n"+gameData.bluePercent+"%");
			}
			else if (tag == "Blue")
			{
				GUI.Box(new Rect(Screen.width-buttonW-edge, edge, buttonW, buttonH), "Blue Team:\n"+gameData.blueCount+" players\n"+gameData.blueScore+" cubes\n"+gameData.bluePercent+"%");
				GUI.Box(new Rect(Screen.width-buttonW-edge, edge*2+buttonH, buttonW, buttonH), "Red Team:\n"+gameData.redCount+" players\n"+gameData.redScore+" cubes\n"+gameData.redPercent+"%");
			}
			GUI.Box(new Rect(Screen.width-buttonW-edge, edge*3+buttonH*2, buttonW, buttonH*0.8f), "Personal:\n"+myScore+" cubes\n"+myPercent+"%");
		}
		
		if (gameData.state == GameData.State.betweenGames)
		{
			if (gameData.redScore > gameData.blueScore) // display the appropriate list
			{
				GUI.Box(new Rect(-9, -9, Screen.width+9, Screen.height+9), "\nRed Team Wins!\n"+gameData.redCount+" players\n"+gameData.redScore+" cubes\n"+gameData.redPercent+"%"
					+"\n\n\nBlue Team \n"+gameData.blueCount+" players\n"+gameData.blueScore+" cubes\n"+gameData.bluePercent+"%"+"\n\n\nRestart in: \n"+Mathf.CeilToInt(gameData.gameEndTime-(float)Network.time));
			}
			else 
			{
				GUI.Box(new Rect(-9, -9, Screen.width+9, Screen.height+9), "\nBlue Team Wins!\n"+gameData.blueCount+" players\n"+gameData.blueScore+" cubes\n"+gameData.bluePercent+"%"
					+"\n\n\nRed Team \n"+gameData.redCount+" players\n"+gameData.redScore+" cubes\n"+gameData.redPercent+"%"+"\n\n\nRestart in: \n"+Mathf.CeilToInt(gameData.gameEndTime-(float)Network.time));
			}
		}
		else if (gameData.redPercent > gameData.percentToWin-5 || gameData.bluePercent > gameData.percentToWin-5)
		{
			if (gameData.blueScore > gameData.redScore)
			{
				GUI.Box(new Rect(Screen.width-buttonW-edge, edge*4+buttonH*2+buttonH*0.8f, buttonW, buttonH*0.6f), "Blue Team\n"+(gameData.percentToWin - gameData.bluePercent)+"% to Win");
			}
			else
			{
				GUI.Box(new Rect(Screen.width-buttonW-edge, edge*4+buttonH*2+buttonH*0.8f, buttonW, buttonH*0.6f), "Red Team\n"+(gameData.percentToWin - gameData.redPercent)+"% to Win");
			}
		}
	}
}