using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.Law;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class DeathScreen : Singleton<DeathScreen>
{
	[Header("References")]
	public Canvas canvas;

	public RectTransform Container;

	public CanvasGroup group;

	public Button respawnButton;

	public Button loadSaveButton;

	public Animation Anim;

	public AudioSourceController Sound;

	private bool arrested;

	public bool isOpen { get; protected set; }

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)respawnButton.onClick).AddListener(new UnityAction(RespawnClicked));
		((UnityEvent)loadSaveButton.onClick).AddListener(new UnityAction(LoadSaveClicked));
		((Behaviour)canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		group.alpha = 0f;
		group.interactable = false;
	}

	private void RespawnClicked()
	{
		if (isOpen)
		{
			isOpen = false;
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			Singleton<BlackOverlay>.Instance.Open();
			yield return (object)new WaitForSeconds(0.5f);
			Close();
			Singleton<HospitalBillScreen>.Instance.Open();
			Transform val = Singleton<ScheduleOne.Map.Map>.Instance.MedicalCentre.RespawnPoint;
			if (NetworkSingleton<CurfewManager>.Instance.IsCurrentlyActive && ((Object)(object)Player.Local.LastVisitedProperty != (Object)null || ScheduleOne.Property.Property.OwnedProperties.Count > 0))
			{
				val = ((!((Object)(object)Player.Local.LastVisitedProperty != (Object)null)) ? ScheduleOne.Property.Property.OwnedProperties[0].InteriorSpawnPoint : Player.Local.LastVisitedProperty.InteriorSpawnPoint);
			}
			Player.Local.Health.SendRevive(val.position + Vector3.up * 1f, val.rotation);
			if (arrested)
			{
				Singleton<ArrestNoticeScreen>.Instance.RecordCrimes();
				Player.Local.Free_Server();
			}
			yield return (object)new WaitForSeconds(2f);
			Singleton<BlackOverlay>.Instance.Close();
		}
	}

	private void LoadSaveClicked()
	{
		Close();
		Singleton<LoadManager>.Instance.ExitToMenu(Singleton<LoadManager>.Instance.ActiveSaveInfo);
	}

	public void Open()
	{
		if (!isOpen)
		{
			isOpen = true;
			arrested = Player.Local.IsArrested;
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
			Sound.Play();
			((Component)respawnButton).gameObject.SetActive(CanRespawn());
			((Component)loadSaveButton).gameObject.SetActive(!((Component)respawnButton).gameObject.activeSelf);
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return (object)new WaitForSeconds(0.55f);
			Anim.Play();
			((Behaviour)canvas).enabled = true;
			((Component)Container).gameObject.SetActive(true);
			float lerpTime = 0.75f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				Singleton<PostProcessingManager>.Instance.SetBlur(i / lerpTime);
				yield return (object)new WaitForEndOfFrame();
			}
			Singleton<PostProcessingManager>.Instance.SetBlur(1f);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			group.interactable = true;
		}
	}

	private bool CanRespawn()
	{
		return Player.PlayerList.Count > 1;
	}

	public void Close()
	{
		isOpen = false;
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		Singleton<PostProcessingManager>.Instance.SetBlur(0f);
		((Behaviour)canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
	}
}
