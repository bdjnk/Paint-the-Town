using UnityEngine;
using System.Collections;

public class SpotLight : MonoBehaviour
{
	private float rate = 0.07f;
	private GameObject cube1;
	private GameObject cube2;	
	private GameObject mainCam;	
	private Light mLight;
	
	void Start()
	{
		cube1 = GameObject.Find("PositiveCube");
		cube2 = GameObject.Find("PaintCube");	
		mainCam = GameObject.Find("Main Camera");	
		
		trackAlongLine = cube1.transform.position;
		transform.forward= mainCam.transform.position - transform.position;
		mLight = GetComponentInChildren<Light>();
	}

	Vector3 trackAlongLine;
	Vector3 line;
	
	bool reverse = true;
	bool initialRoll = false;
	bool finalRoll = false;
	bool nodd = true;
	
	float incrementLook = 0.02f;
	float trackTotal = 0.0f;
	int reverseCount = 0;
	bool shoot = false;
	float reverseTime = 0.0f;
	
	void Update()
	{
		if (nodd)
		{
			trackTotal += incrementLook;
			if (trackTotal < 0.3f)
			{
				transform.forward += new Vector3(0f, incrementLook, 0f);
			}
			else if (trackTotal < 0.9f)
			{
				transform.forward += new Vector3(0f, -incrementLook, 0f);
			}
			else if (trackTotal < 1.2f)
			{
				transform.forward += new Vector3(0f, incrementLook, 0f);
			}
			else
			{
				nodd = false;
				initialRoll = true;
				transform.forward= transform.position - mainCam.transform.position;
				trackAlongLine = mainCam.transform.position;
			}
		}
		
		if (initialRoll)
		{
			trackAlongLine += (cube1.transform.position - mainCam.transform.position).normalized*rate*2;
			if ((trackAlongLine - cube1.transform.position).magnitude < 1.0f)
			{
				initialRoll = false;
				finalRoll = true;
				trackTotal = 0.0f;
			} 
			transform.forward = trackAlongLine - transform.position;
		}
		
		if (finalRoll)
		{
			if (!reverse)
			{
				trackAlongLine += (cube2.transform.position - cube1.transform.position).normalized*rate;
			}
			else
			{
				trackAlongLine += (cube1.transform.position - cube2.transform.position).normalized*rate;
			}
			if ((trackAlongLine - cube1.transform.position).magnitude < 1.0f || (trackAlongLine - cube2.transform.position).magnitude < 1.0f)
			{
				if (Time.realtimeSinceStartup - reverseTime > 1.0f)
				{
					reverse = !reverse;
					reverseTime = Time.realtimeSinceStartup;
					reverseCount++;
				}
				if (reverseCount==4)
				{
					finalRoll = false;
					shoot = true;
				}
			}
			trackTotal += incrementLook;
			if (trackTotal < 0.3f)
			{
				trackAlongLine += new Vector3(0f, incrementLook, 0f);
			}
			else if(trackTotal < 0.9f)
			{
				trackAlongLine += new Vector3(0f, -incrementLook, 0f);
			}
			else if(trackTotal < 1.2f)
			{
				trackAlongLine += new Vector3(0f, incrementLook, 0f);
			}
			else
			{
				trackTotal = 0.0f;
			}
			transform.forward = trackAlongLine - transform.position;
		}
		
		if (shoot)
		{
			Vector3 pos = transform.position + transform.forward * transform.localScale.z * 1f;
			GameObject shot = GameObject.Find ("RedShot");
			shot.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			shot.rigidbody.velocity = shot.transform.TransformDirection(new Vector3(0, 0, 15f));
			shoot = false;
		}
		
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Application.LoadLevel("Menu");
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			Application.LoadLevel("Menu");
		}
		
	}
	
	public void StartDimLight()
	{
		StartCoroutine("DimLight");
	}
	
	public IEnumerator DimLight()
	{
		for (float i = mLight.intensity; i > 0.11f; i -= 0.1f){
			mLight.intensity =i;
			yield return new WaitForSeconds(0.2f);
		}
	}
	
	public void StartBrightenLight()
	{
		StartCoroutine("BrightenLight");
	}
	
	public IEnumerator BrightenLight()
	{
		for (float i = 0.10f; i < 1.20f; i += 0.1f)
		{
			mLight.intensity = i;
			yield return new WaitForSeconds(0.1f);
		}
		mLight.intensity = 1.20f;
	}
	
	public void Reset()
	{
		finalRoll = true;
		reverse = true;
		reverseCount = 0;
		StartBrightenLight();
		transform.forward= mainCam.transform.position - transform.position;
	}
}
