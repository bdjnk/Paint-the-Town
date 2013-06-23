using UnityEngine;
using System.Collections;

public class PG_Gun : MonoBehaviour
{
	public GameObject shotPrefab;
	public float speed = 15f; // speed of shot
	public float rate = 0.2f; // rate of fire, portion of a second before firing again
	public float power = 3f;
	private float delay = 0;
	
	public bool aggressive = false;
	
	public Texture2D crosshairImage;
	public Texture bs, eb, fs, qm, rf;
	private float bsd, ebd, fsd, qmd, rfd;
	
	private void OnGUI() // replace with GUITextures (much faster)
	{
		if (tag != "Bot") // human player
		{
			float xMin = (Screen.width / 2) - (crosshairImage.width / 2);
			float yMin = (Screen.height / 2) - (crosshairImage.height / 2);
			GUI.DrawTexture(new Rect(xMin, yMin, crosshairImage.width, crosshairImage.height), crosshairImage);
			
			if (bs != null)
				GUI.Box(new Rect(10, Screen.height-50, 40, 40), bs);
			if (rf != null)
				GUI.Box(new Rect(60, Screen.height-50, 40, 40), rf);
			if (fs != null)
				GUI.Box(new Rect(110, Screen.height-50, 40, 40), fs);
			if (qm != null)
				GUI.Box(new Rect(160, Screen.height-50, 40, 40), qm);
			if (eb != null)
				GUI.Box(new Rect(210, Screen.height-50, 40, 40), eb);
		}
	}
	
	private void Start()
	{
		if (Network.isServer)
		{
			networkView.RPC("SetAggressive", RPCMode.AllBuffered, PlayerPrefs.GetInt("aggressive", 1));
		}
		Screen.showCursor = false;
		bs = eb = fs = qm = rf = null;
	}
	
	[RPC] private void SetAggressive(int state) { aggressive = System.Convert.ToBoolean(state); }
	
	public float freezeTimeout = 0;
	
	private void Update()
	{
		Downgrade();
		
		if (tag != "Bot") // human player
		{
			if (Input.GetButton("Fire1"))
			{
				transform.parent.GetComponent<PlayerManager>().MouseEnable(true);
				Screen.lockCursor = true;
				Shoot();
			}
		}
	}
	
	public void Shoot()
	{
		if (Network.time > delay && Network.time > freezeTimeout && networkView.isMine)
		{
			delay = (float)Network.time + rate;
			Vector3 pos = transform.position + transform.forward * transform.localScale.z * 1f;
			GameObject shot = Network.Instantiate(shotPrefab, pos, transform.rotation, 3) as GameObject;
			networkView.RPC("InitializeShot", RPCMode.All, shot.networkView.viewID, networkView.viewID);
		}
	}
	
	[RPC] private void InitializeShot(NetworkViewID shotID, NetworkViewID gunID)
	{
		GetComponent<AudioSource>().Play();
		
		NetworkView shotNetView = NetworkView.Find(shotID);
		NetworkView gunNetView = NetworkView.Find(gunID);
		
		if (shotNetView != null && gunNetView != null) // shouldn't be null, but sometimes it is...
		{
			GameObject shot = shotNetView.gameObject;
			GameObject gun = gunNetView.gameObject;
			
			if (shot != null && gunNetView != null)
			{
				shot.rigidbody.velocity = transform.TransformDirection(new Vector3(0, 0, speed));
				shot.GetComponent<PG_Shot>().gun = gun.GetComponent<PG_Gun>();
			}
		}
	}
	
	private void Downgrade()
	{
		if (bs != null && bsd - Time.time < 0)
		{
			bs = null;
			power -= 2;
		}
		if (rf != null && rfd - Time.time < 0)
		{
			rf = null;
			rate *= 2;
		}
		if (fs != null && fsd - Time.time < 0)
		{
			fs = null;
			speed /= 2;
		}
		if (qm != null && qmd - Time.time < 0)
		{
			qm = null;
						
			CharacterMotor cm = transform.parent.GetComponent<CharacterMotor>();
			cm.jumping.baseHeight = 1;
			cm.movement.maxForwardSpeed /= 2;
			cm.movement.maxSidewaysSpeed /= 2;
			cm.movement.maxBackwardsSpeed /= 2;
			cm.movement.maxGroundAcceleration /= 3;
		}
		if (eb != null && ebd - Time.time < 0)
		{
			eb = null;
		}
	}
	
	[RPC] public void Upgrade(string upgrade) // all upgrade numbers should be variables
	{
		if (upgrade == "BlastShots")
		{
			if (bs == null)
			{
				bs = Resources.Load("Textures/"+upgrade) as Texture;
				power += 2;
			}
			bsd = Time.time + 9.0f;
		}
		else if (upgrade == "RapidFire")
		{
			if (rf == null)
			{
				rf = Resources.Load("Textures/"+upgrade) as Texture;
				rate /= 2;
			}
			rfd = Time.time + 9.0f;
		}
		else if (upgrade == "FastShots")
		{
			if (fs == null)
			{
				fs = Resources.Load("Textures/"+upgrade) as Texture;
				speed *= 2;
			}
			fsd = Time.time + 9.0f;
		}
		else if (upgrade == "MoveQuick")
		{
			if (qm == null)
			{
				qm = Resources.Load("Textures/"+upgrade) as Texture;
				
				CharacterMotor cm = transform.parent.GetComponent<CharacterMotor>();
				cm.jumping.baseHeight = 4;
				cm.movement.maxForwardSpeed *= 2;
				cm.movement.maxSidewaysSpeed *= 2;
				cm.movement.maxBackwardsSpeed *= 2;
				cm.movement.maxGroundAcceleration *= 3;
			}
				qmd = Time.time + 9.0f;
		}
		else if (upgrade == "EvadeBots")
		{
			if (eb == null) {
				eb = Resources.Load("Textures/"+upgrade) as Texture;
				// do nothing else, for now
			}
			ebd = Time.time + 9.0f;
		}
	}
}
