using UnityEngine;

public class FlockScare : MonoBehaviour
{
	public LandingSpotController[] landingSpotControllers;

	public float scareInterval = 0.1f;

	public float distanceToScare = 2f;

	public int checkEveryNthLandingSpot = 1;

	public int InvokeAmounts = 1;

	private int lsc;

	private int ls;

	private LandingSpotController currentController;

	private void CheckProximityToLandingSpots()
	{
		IterateLandingSpots();
		if (currentController._activeLandingSpots > 0 && CheckDistanceToLandingSpot(landingSpotControllers[lsc]))
		{
			landingSpotControllers[lsc].ScareAll();
		}
		((MonoBehaviour)this).Invoke("CheckProximityToLandingSpots", scareInterval);
	}

	private void IterateLandingSpots()
	{
		ls += checkEveryNthLandingSpot;
		currentController = landingSpotControllers[lsc];
		int childCount = ((Component)currentController).transform.childCount;
		if (ls > childCount - 1)
		{
			ls -= childCount;
			if (lsc < landingSpotControllers.Length - 1)
			{
				lsc++;
			}
			else
			{
				lsc = 0;
			}
		}
	}

	private bool CheckDistanceToLandingSpot(LandingSpotController lc)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Transform child = ((Component)lc).transform.GetChild(ls);
		if ((Object)(object)((Component)child).GetComponent<LandingSpot>().landingChild != (Object)null)
		{
			Vector3 val = child.position - ((Component)this).transform.position;
			if (((Vector3)(ref val)).sqrMagnitude < distanceToScare * distanceToScare)
			{
				return true;
			}
		}
		return false;
	}

	private void Invoker()
	{
		for (int i = 0; i < InvokeAmounts; i++)
		{
			float num = scareInterval / (float)InvokeAmounts * (float)i;
			((MonoBehaviour)this).Invoke("CheckProximityToLandingSpots", scareInterval + num);
		}
	}

	private void OnEnable()
	{
		((MonoBehaviour)this).CancelInvoke("CheckProximityToLandingSpots");
		if (landingSpotControllers.Length != 0)
		{
			Invoker();
		}
	}

	private void OnDisable()
	{
		((MonoBehaviour)this).CancelInvoke("CheckProximityToLandingSpots");
	}
}
