using UnityEngine;
using System.Collections;

public class PG_Map : MonoBehaviour
{
	public GameObject botPrefab;
	public GameObject donePrefab;
	
	public bool floor = true;
	public float spacing = 1.5f;
	
    private int[] minBuildingSize;
    private int[] maxBuildingSize;
	
	public GameObject cubePrefab;
	public GameObject buildingPrefab;
	public GameObject lightingPrefab;
	public GameObject groundPrefab;
	
	public int cubeCount = 0;
	
	public void CreateMap()
	{
		cubeCount = 0;
		
		minBuildingSize = new int [] {
			PlayerPrefs.GetInt("minBuildingSizeX"),
			PlayerPrefs.GetInt("minBuildingSizeY"),
			PlayerPrefs.GetInt("minBuildingSizeZ")
		};
		maxBuildingSize = new int [] {
			PlayerPrefs.GetInt("maxBuildingSizeX"),
			PlayerPrefs.GetInt("maxBuildingSizeY"),
			PlayerPrefs.GetInt("maxBuildingSizeZ")
		};
		
		Vector3 offset = Vector3.zero;
		Vector3 temp = Vector3.zero; //TODO give a meaningful name
		
		Vector3 farCorner = Vector3.zero;
		
		int width = PlayerPrefs.GetInt("citySizeX");
		int height = PlayerPrefs.GetInt("citySizeY");
		int depth = PlayerPrefs.GetInt("citySizeZ");
		
		spacing = PlayerPrefs.GetFloat("buildingSpacing", 1.5f);
		floor = System.Convert.ToBoolean(PlayerPrefs.GetInt("hasFloor", 1));
		
		for (int w = 0; w < width; w++)
		{
			for (int h = 0; h < height; h++)
			{
				for (int d = 0; d < depth; d++)
				{
					temp.Set(offset.x * w, offset.y * h, offset.z * d);
					offset = Vector3.Max(offset, MakeBuilding(temp));
				}
			}
		}
		for (int w = 0; w < width ; w++) { farCorner.x += offset.x; }
		for (int h = 0; h < height; h++) { farCorner.y += offset.y; }
		for (int d = 0; d < depth ; d++) { farCorner.z += offset.z; }
		
		farCorner -= new Vector3(spacing+1.5f, spacing+1.5f, spacing+1.5f);
		Vector3 mapCenter = new Vector3(farCorner.x/2, farCorner.y/2, farCorner.z/2);
		
		GetComponent<GameData>().networkView.RPC("SetMapExtents", RPCMode.AllBuffered, mapCenter, farCorner);
		
		//----------------------------------------------- bots
		if (System.Convert.ToBoolean(PlayerPrefs.GetInt("hasBots", 1)))
		{
			GameObject bot;
			Vector3 position;
			Quaternion facing;
			
			int farOut = 20;
			int bottom = floor ? 0 : -farOut;
			float middle = floor ? farCorner.y : mapCenter.y;
			
			Vector3 farthest = farCorner + new Vector3(farOut, farOut, farOut);
			
			// ------------------------------------------ top bots
			position = new Vector3(-farOut, farthest.y, -farOut);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(-farOut, farthest.y, farthest.z);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(farthest.x, farthest.y, -farOut);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(farthest.x, farthest.y, farthest.z);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			// ------------------------------------------ bottom bots
			position = new Vector3(-farOut, bottom, -farOut);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(-farOut, bottom, farthest.z);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(farthest.x, bottom, -farOut);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(farthest.x, bottom, farthest.z);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			farOut = 40;
			farthest = farCorner + new Vector3(farOut, farOut, farOut);
			
			// ------------------------------------------ middle bots
			position = new Vector3(-farOut, middle, -farOut);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(-farOut, middle, farthest.z);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(farthest.x, middle, -farOut);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			position = new Vector3(farthest.x, middle, farthest.z);
			facing = Quaternion.LookRotation(mapCenter - position);
			bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
			// ------------------------------------------ center bots
			if (farCorner.x > 20)
			{
				farOut = 20;
				farthest = farCorner + new Vector3(farOut, farOut, farOut);
				
				position = new Vector3(farCorner.x/2, farthest.y, -farOut);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
				
				position = new Vector3(farCorner.x/2, farthest.y, farthest.z);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
				
				position = new Vector3(farCorner.x/2, bottom, -farOut);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
					
				position = new Vector3(farCorner.x/2, bottom, farthest.z);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
				
				farOut = 40;
				farthest = farCorner + new Vector3(farOut, farOut, farOut);
				
				position = new Vector3(farCorner.x/2, middle, -farOut);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
				
				position = new Vector3(farCorner.x/2, middle, farthest.z);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			}
			if (farCorner.z > 20)
			{
				farOut = 20;
				farthest = farCorner + new Vector3(farOut, farOut, farOut);
				
				position = new Vector3(-farOut, farthest.y, farCorner.z/2);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
				
				position = new Vector3(farthest.x, farthest.y, farCorner.z/2);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
				position = new Vector3(-farOut, bottom, farCorner.z/2);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
				position = new Vector3(farthest.x, bottom, farCorner.z/2);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
				
				farOut = 40;
				farthest = farCorner + new Vector3(farOut, farOut, farOut);
			
				position = new Vector3(-farOut, middle, farCorner.z/2);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			
				position = new Vector3(farthest.x, middle, farCorner.z/2);
				facing = Quaternion.LookRotation(mapCenter - position);
				bot = Network.Instantiate(botPrefab, position, facing, 2) as GameObject;
			}
		}
		//----------------------------------------------- bots
		
		if (floor)
		{
			GameObject ground = Network.Instantiate(groundPrefab, new Vector3((maxBuildingSize[0]*1.5f*width-1.5f)/2, -0.5f, (maxBuildingSize[2]*1.5f*depth-1.5f)/2), Quaternion.identity, 0) as GameObject;
			ground.isStatic = true;
		}
		GetComponent<GameData>().networkView.RPC("SetCubeCount", RPCMode.AllBuffered, cubeCount);
		
		float timer = System.Convert.ToBoolean(PlayerPrefs.GetInt("isTimed", 1)) ? PlayerPrefs.GetFloat("timer", 2) : 0;
		GetComponent<GameData>().networkView.RPC("SetGameLength", RPCMode.AllBuffered, timer*60);
		
		GameObject done = Network.Instantiate(donePrefab, Vector3.zero, Quaternion.identity, 1) as GameObject;
		done.isStatic = true;
	}
	
	private Vector3 MakeBuilding(Vector3 offset)
	{
		int width = Random.Range(minBuildingSize[0], maxBuildingSize[0]+1);
		int height = Random.Range(minBuildingSize[1], maxBuildingSize[1]+1);
		int depth = Random.Range(minBuildingSize[2], maxBuildingSize[2]+1);
		
		GameObject building = Network.Instantiate(buildingPrefab, Vector3.zero,Quaternion.identity, 0) as GameObject;
		
		Vector3 center = Vector3.zero;
		int buildingCubeCount = 0;
		
		for (int w = 0; w < width; w++)
		{
			for (int h = 0; h < height; h++)
			{
				for (int d = 0; d < depth; d++)
				{
					GameObject cube = Network.Instantiate(cubePrefab, new Vector3(1.5f*w, 1.5f*h, 1.5f*d), Quaternion.identity, 0) as GameObject;
					networkView.RPC("SetParent", RPCMode.AllBuffered, cube.networkView.viewID, building.networkView.viewID);
					cube.isStatic = true;
					
			    	buildingCubeCount++;
					center += cube.transform.position;
				}
			}
		}
		building.transform.position += offset;
		building.isStatic = true;
		
		building.GetComponent<PG_Building>().networkView.RPC("SetCubeCount", RPCMode.AllBuffered, buildingCubeCount);
		cubeCount += buildingCubeCount;
		
		int count = width * height * depth;
		center /= count;
		
		GameObject light = Network.Instantiate(lightingPrefab, Vector3.zero, Quaternion.identity, 0) as GameObject;
		networkView.RPC("SetParent", RPCMode.AllBuffered, light.networkView.viewID, building.networkView.viewID);
		networkView.RPC("AddLight", RPCMode.AllBuffered, light.networkView.viewID, center, count);
		light.isStatic = true;
		
		return (new Vector3(width*1.5f+spacing, height*1.5f+spacing, depth*1.5f+spacing));// * spacing);
	}
	
	[RPC] private void AddLight(NetworkViewID id, Vector3 center, int count)
	{
		GameObject light = NetworkView.Find(id).gameObject;
		light.AddComponent(typeof(Light));
		light.transform.localPosition = center;
		light.light.intensity = count / 20f;
	}
	
	//TODO this ought to be moved to a static class
	[RPC] private void SetParent(NetworkViewID childID, NetworkViewID parentID)
	{
		NetworkView.Find(childID).transform.parent = NetworkView.Find(parentID).transform;
	}
}
