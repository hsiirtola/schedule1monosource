using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	public Camera lookAtCamera;

	public bool lookOnlyOnAwake;

	public void Start()
	{
		if ((Object)(object)lookAtCamera == (Object)null)
		{
			lookAtCamera = Camera.main;
		}
		if (lookOnlyOnAwake)
		{
			LookCam();
		}
	}

	public void Update()
	{
		if (!lookOnlyOnAwake)
		{
			LookCam();
		}
	}

	public void LookCam()
	{
		((Component)this).transform.LookAt(((Component)lookAtCamera).transform);
	}
}
