using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

public class BombPlantLocation : MonoBehaviour
{
	public const float COUNTDOWN_TIME = 45f;

	public const float BEEP_INTERVAL_MAX = 1f;

	public const float BEEP_INTERVAL_MIN = 0.125f;

	[Header("References")]
	public InteractableObject IntObj;

	public GameObject BombModel;

	public UnityEvent onPlantBomb;

	public UnityEvent onBeep;

	public UnityEvent onDetonate;

	public bool BombPlanted { get; private set; }

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
		BombModel.gameObject.SetActive(false);
	}

	private void Hovered()
	{
		if (CanPlantBomb())
		{
			IntObj.SetMessage("Plant bomb");
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	private void Interacted()
	{
		if (CanPlantBomb())
		{
			PlantBomb();
		}
	}

	public void PlantBomb()
	{
		if (!BombPlanted)
		{
			if (onPlantBomb != null)
			{
				onPlantBomb.Invoke();
			}
			BombPlanted = true;
			PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem("bomb");
			BombModel.SetActive(true);
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Detonate());
		}
		IEnumerator Detonate()
		{
			float t = 0f;
			float timeSinceLastBeep = 1000f;
			float beepTime = 1f;
			while (t < 45f)
			{
				t += Time.deltaTime;
				timeSinceLastBeep += Time.deltaTime;
				if (timeSinceLastBeep >= beepTime)
				{
					timeSinceLastBeep = 0f;
					UnityEvent obj = onBeep;
					if (obj != null)
					{
						obj.Invoke();
					}
					beepTime = Mathf.Lerp(1f, 0.125f, t / 45f);
				}
				yield return (object)new WaitForEndOfFrame();
			}
			BombModel.gameObject.SetActive(false);
			if (onDetonate != null)
			{
				onDetonate.Invoke();
			}
		}
	}

	private bool CanPlantBomb()
	{
		if (BombPlanted)
		{
			return false;
		}
		if (PlayerSingleton<PlayerInventory>.Instance.GetAmountOfItem("bomb") == 0)
		{
			return false;
		}
		return true;
	}
}
