using UnityEngine;
using System.Collections;

public class PG_Bot : MonoBehaviour
{
	public float fov = 90.0f;
	private RaycastHit hit;
	private int speed = 3;
	private int maxDistance = 45;
	
	public PG_Gun gun;
	public Color myColor;
	
	public float farOut;
	
	private GameData gameData;
	
	private void Start()
	{
		gameData = GameObject.FindGameObjectWithTag("Master").GetComponent<GameData>();
		SetColor("Red");
		
		if (transform.position.x > gameData.extent.x)
		{
			farOut = transform.position.x - gameData.extent.x;
		}
		else if (transform.position.x < 0)
		{
			farOut = -transform.position.x;
		}
		else // within the extent of the map
		{
			if (transform.position.z > gameData.extent.z)
			{
				farOut = transform.position.z - gameData.extent.z;
			}
			else if (transform.position.z < 0)
			{
				farOut = -transform.position.z;
			}
		}
	}
	
	public void SetColor(string color)
	{
		gun.shotPrefab = Resources.Load("Prefabs/"+color+"Shot") as GameObject;
		
		ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
		myColor = color == "Blue" ? gameData.blue : gameData.red;
		ps.startColor = ps.light.color = color == "Blue" ? Color.blue : Color.red;
	}
	
	private void Update()
	{
		if (gameData.blueScore > gameData.redScore)
		{
			if (myColor != gameData.red)
			{
				SetColor("Red");
			}
		}
		else if (gameData.redScore > gameData.blueScore)
		{
			if (myColor != gameData.blue)
			{
				SetColor("Blue");
			}
		}
		
		Transform target = null;
		float distance = 999;
		
		foreach (GameObject player in gameData.players)
		{
			if (player != null && player.GetComponent<PlayerManager>().myColor != myColor)
			{
				float closest = Vector3.Distance(player.transform.position, transform.position);
				
				if (closest < distance)
				{
					distance = closest;
					target = player.transform;
				}
			}
		}
		
		float angle;
		
		if (target != null && target.GetComponentInChildren<PG_Gun>().eb == null)
		{
			angle = Vector3.Angle(target.position - transform.position, transform.forward);
			
			if (angle > -2 && angle < 2 && gameData.state == GameData.State.inGame)
			{
				gun.Shoot();
			}
			if (LineOfSight(target) || distance < maxDistance/3)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position), speed);
			}
			else
			{
				LazyMode();
			}
		}
		else
		{
			LazyMode();
		}
		
		if (transform.position.x <= -farOut+0.1f && transform.position.z <= gameData.extent.z+farOut+0.1f) //(0,f,0->f)
		{
			transform.Translate(new Vector3(0, 0, 1) * Time.deltaTime * speed, Space.World);
		}
		if (transform.position.x <= gameData.extent.x+farOut+0.1f && transform.position.z >= gameData.extent.z+farOut-0.1f) //(0->f,f,f)
		{
			transform.Translate(new Vector3(1, 0, 0) * Time.deltaTime * speed, Space.World);
		}
		if (transform.position.x >= gameData.extent.x+farOut-0.1f && transform.position.z >= -farOut-0.1f) //(f,f,f->0)
		{
			transform.Translate(new Vector3(0, 0, -1) * Time.deltaTime * speed, Space.World);
		}
		if (transform.position.x >= -farOut-0.1f && transform.position.z <= -farOut+0.1f) //(f->0,f,0)
		{
			transform.Translate(new Vector3(-1, 0, 0) * Time.deltaTime * speed, Space.World);
		}
	}
	
	private bool LineOfSight(Transform target)
	{
		if (Vector3.Angle(target.position - transform.position, transform.forward) <= fov
			&& Physics.Raycast(new Ray(transform.position, (target.position - transform.position).normalized), out hit, maxDistance)
			&& hit.transform == target)
		{
			return true;
		}
		return false;
	}
	
	private void LazyMode()
	{
		transform.Rotate(Random.value*speed, Random.value*speed, Random.value*speed); // rotate randomly rather that looking to the center of the map
		//transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(gameData.mapCenter - transform.position), speed);
		
		if (gameData.state == GameData.State.inGame
			&& Physics.Raycast(new Ray(transform.position, transform.forward), out hit, maxDistance)
			&& hit.transform.tag == "Cube")
		{
			gun.Shoot();
		}
	}
}
