using UnityEngine;
using System.Collections;

public class PaintCubeScript : MonoBehaviour
{
	SpotLight mLight; 
	
	void Start()
	{
			mLight = GameObject.Find("Camera").GetComponent<SpotLight>();
	}

	void OnTriggerEnter(Collider other)
	{
		StartCoroutine("SetNextTexture", false);
		other.transform.rigidbody.velocity= new Vector3(0f, 0f, 0f);
		other.transform.localPosition = new Vector3(0f, 0f, 0f);
		other.transform.localScale = new Vector3(0f, 0f, 0f);

	}

	IEnumerator SetNextTexture(bool reduce)
	{
		if (!reduce)
		{
			for (int i = 1; i <= 7; i++)
			{
				renderer.material.SetTexture("_DecalTex", Resources.Load("Textures/PaintTheTown"+i) as Texture);
				yield return new WaitForSeconds(0.25f);
			}
		} 
		StartStory();
	}

	public void StartCreatedBy()
	{
		renderer.material.SetTexture("_DecalTex", Resources.Load("Textures/CreatedBy") as Texture);
		renderer.material.SetTextureScale("_DecalTex", new Vector2(1.0f, 0.2f));
		renderer.material.SetTextureOffset("_DecalTex", new Vector2(0f, -0.05f));
		StartCoroutine("SetTextureOffset");
	}

	IEnumerator SetTextureOffset()
	{
		for (int i = 0; i < 103; i++)
		{
			renderer.material.SetTextureOffset("_DecalTex", new Vector2(0f, 0.01f*(i-20)));
			yield return new WaitForSeconds(0.15f);
		}
		yield return new WaitForSeconds(4.0f);
		StartResetPaint();
	}

	public void StartStory()
	{
		renderer.material.SetTexture("_DecalTex", Resources.Load("Textures/Story4") as Texture);
		renderer.material.SetTextureScale("_DecalTex", new Vector2(1.0f, 0.2f));
		renderer.material.SetTextureOffset("_DecalTex",new Vector2(0f, -0.05f));
		StartCoroutine("SetTextureOffset2");
	}

	public IEnumerator SetTextureOffset2()
	{
		for (int i = 0; i < 88; i++)
		{
			renderer.material.SetTextureOffset("_DecalTex", new Vector2(0f, 0.01f*(i-5)));
			yield return new WaitForSeconds(0.2f);
		}
		StartCreatedBy();
	}

	public void StartResetPaint()
	{
		StartCoroutine("LightReset");
	}

	public IEnumerator LightReset()
	{
		mLight.StartDimLight();
		yield return new WaitForSeconds(4.0f);
		mLight.Reset();
		yield return new WaitForSeconds(4.0f);
		ResetPaint();
	}

	public void ResetPaint()
	{
		renderer.material.SetTexture("_DecalTex", Resources.Load("Textures/PaintTheTown1") as Texture);
		renderer.material.SetTextureScale("_DecalTex", new Vector2(1.0f, 1.0f));
		renderer.material.SetTextureOffset("_DecalTex", new Vector2(0f, 0f));
	}
}
