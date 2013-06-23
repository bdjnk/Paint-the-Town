using UnityEngine;
using System.Collections;

public class PG_Shot : MonoBehaviour
{
	public PG_Gun gun;
	
	public float persist = 3f;
	private float timeAtStart;
	private GameObject stunSound;
	
	private void Start()
	{
		if (gun == null)
		{
			Destroy(gameObject);
		}
		timeAtStart = (float)Network.time;
		
		stunSound = Resources.Load("Prefabs/Stun") as GameObject;
	}
	
	private void Update()
	{
		// persist for network functionality && this shot belongs to me (on the network)
		if (Network.time  > timeAtStart + persist && gun != null && gun.networkView.isMine)
		{
			Network.Destroy(gameObject); // destroy for the server and all clients
		}
	}
	
	private void OnTriggerEnter(Collider other)
	{
		PG_Cube cubeScript =  other.GetComponent<PG_Cube>();
		
		if (cubeScript != null) // shot hit a cube
		{
			if (Network.isServer) // <== AUTHORITATIVE SERVER
			{
				cubeScript.Struck(this); // only server based cube strikes are real
			}
		}
		else if (other.tag == "Red" || other.tag == "Blue") // shot hit a player
		{
			if (gun.tag != "Bot") // shot was fired by a player
			{
				if (gun.aggressive)
				{
					other.GetComponentInChildren<PG_Gun>().freezeTimeout = (float)Network.time + 2f;
				}
				else // peaceful
				{
					gun.freezeTimeout = (float)Network.time + 2;
					Instantiate(stunSound, this.transform.position, Quaternion.identity); //play sound
				}
			}
			else if (gun.tag == "Bot")
			{
				if (gun.GetComponent<PG_Bot>().myColor != other.GetComponent<PlayerManager>().myColor)
				{
					other.GetComponentInChildren<PG_Gun>().freezeTimeout = (float)Network.time + 1;
				}
			}
		}
		else if (other.tag == "Bot") // shot hit a bot
		{
			if (gun.tag != "Bot") // shot was fired by a player
			{
				other.GetComponent<PG_Gun>().freezeTimeout = (float)Network.time + 2;
			}
		}
		Destroy(gameObject);
	}
}
