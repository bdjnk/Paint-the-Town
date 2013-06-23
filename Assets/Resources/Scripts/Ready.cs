using UnityEngine;
using System.Collections;

public class Ready : MonoBehaviour
{
	public GameObject playerPrefab;
	private GameObject player;
	
	void Start() // at this point we are ready to go
	{
		player = Network.Instantiate(playerPrefab, new Vector3(-10, 2, -10), Quaternion.identity, 4) as GameObject;
	}
}