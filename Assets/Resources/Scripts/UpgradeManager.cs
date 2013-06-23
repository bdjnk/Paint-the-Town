using UnityEngine;
using System.Collections;

public class UpgradeManager : MonoBehaviour
{
	public Texture[] upgrades;
	public GameObject[] cubes;
	
	private bool hasUpgrades;
	
	public float interval = 2;
	private float delay = 0;

	private void Start()
	{
		if (Network.isServer)
		{
			networkView.RPC("SetEnabled", RPCMode.AllBuffered, PlayerPrefs.GetInt("hasUpgrades", 1));
		}
	}
	
	[RPC] private void SetEnabled(int state) { hasUpgrades = System.Convert.ToBoolean(state); }
	
	private void OnEnable()
	{
		delay = (float)Network.time + interval;
		cubes = GameObject.FindGameObjectsWithTag("Cube");
	}
	
	private void Update()
	{
		if (hasUpgrades && Network.time > delay)
		{
			Texture upgrade = upgrades[Random.Range(0, upgrades.Length)]; // random upgrade texture
			GameObject cube = cubes[Random.Range(0, cubes.Length)]; // grab a random cube from the map
			
			cube.GetComponent<PG_Cube>().networkView.RPC("SetDecal", RPCMode.AllBuffered, upgrade.name);
			delay = (float)Network.time + interval;
		}
	}
}
