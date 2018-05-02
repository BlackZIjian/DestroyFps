using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voxeland5;

public class VoxelandManager : MonoBehaviour
{

	private static VoxelandManager mInstance;

	public static VoxelandManager Instance
	{
		get
		{
			return mInstance;
		}
	}

	public Voxeland Voxeland;

	private PhotonView mPhotonView;

	public void Awake()
	{
		mInstance = this;
		mPhotonView = GetComponent<PhotonView>();
	}

	public void DigVoxel(Vector3 pos,Vector3 dir,int damage)
	{
		Ray ray = new Ray(pos,dir);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			if (hit.collider.gameObject.CompareTag("Voxeland"))
			{
				mPhotonView.RPC("DigVoxelRpc",PhotonTargets.AllBufferedViaServer,pos,dir,damage);
			}
		}
	}

	[PunRPC]
	private void DigVoxelRpc(Vector3 pos,Vector3 dir,int damage)
	{
		
		CoordDir center = Voxeland.PointOut(new Ray(pos,dir));
		StartCoroutine(WaitForVoxeland(center,damage));
	}

	private IEnumerator WaitForVoxeland(CoordDir center,int damage)
	{
		yield return new WaitUntil(() => { return Voxeland != null && Voxeland.IsReady; });
		Voxeland.brush.form = Brush.Form.volume;
		Voxeland.brush.extent = damage / 20;
		Voxeland.Alter(center, Voxeland.brush, Voxeland.EditMode.dig, Voxeland.landTypes.selected,
			Voxeland.objectsTypes.selected, Voxeland.grassTypes.selected);
	}
}
