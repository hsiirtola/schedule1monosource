using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.Clothing;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Cutscenes;

public class IntroManager : Singleton<IntroManager>
{
	public const float SKIP_TIME = 0.5f;

	public int CurrentStep;

	[Header("Settings")]
	public int TimeOfDayOverride = 2000;

	[Header("References")]
	public GameObject Container;

	public Transform PlayerInitialPosition;

	public Transform PlayerInitialPosition_AfterRVExplosion;

	public Transform CameraContainer;

	public Animation Anim;

	public GameObject SkipContainer;

	public Image SkipDial;

	public GameObject[] DisableDuringIntro;

	public RV rv;

	public UnityEvent onIntroDone;

	public UnityEvent onIntroDoneAsServer;

	public string MusicName;

	private float currentSkipTime;

	private bool depressed = true;

	public bool IsPlaying { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		Container.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (!Anim.isPlaying)
		{
			return;
		}
		if ((GameInput.GetButton(GameInput.ButtonCode.Jump) || GameInput.GetButton(GameInput.ButtonCode.Submit) || GameInput.GetButton(GameInput.ButtonCode.PrimaryClick)) && depressed)
		{
			currentSkipTime += Time.deltaTime;
			if (currentSkipTime >= 0.5f)
			{
				currentSkipTime = 0f;
				if (IsPlaying)
				{
					Debug.Log((object)"Skipping!");
					int num = CurrentStep + 1;
					float time = Anim.clip.events[num].time;
					Anim[((Object)Anim.clip).name].time = time;
					CurrentStep = num;
					depressed = false;
				}
			}
			SkipDial.fillAmount = currentSkipTime / 0.5f;
			SkipContainer.SetActive(true);
		}
		else
		{
			currentSkipTime = 0f;
			SkipContainer.SetActive(false);
			if (!GameInput.GetButton(GameInput.ButtonCode.Jump) && !GameInput.GetButton(GameInput.ButtonCode.Submit) && !GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
			{
				depressed = true;
			}
		}
	}

	[Button]
	public void Play()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		IsPlaying = true;
		Console.Log("Starting Intro...");
		Container.SetActive(true);
		((Component)rv.ModelContainer).gameObject.SetActive(false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraContainer.position, CameraContainer.rotation, 0f);
		((Component)PlayerSingleton<PlayerCamera>.Instance.CameraContainer).transform.SetParent(CameraContainer);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		GameObject[] disableDuringIntro = DisableDuringIntro;
		for (int i = 0; i < disableDuringIntro.Length; i++)
		{
			disableDuringIntro[i].gameObject.SetActive(false);
		}
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => Singleton<LoadManager>.Instance.IsGameLoaded));
			Anim.Play();
			PlayMusic();
			yield return (object)new WaitForSeconds(0.1f);
			yield return (object)new WaitUntil((Func<bool>)(() => !Anim.isPlaying));
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false, returnToOriginalRotation: false);
			Singleton<BlackOverlay>.Instance.Open();
			yield return (object)new WaitForSeconds(2f);
			Singleton<CharacterCreator>.Instance.Open(Singleton<CharacterCreator>.Instance.DefaultSettings);
			Singleton<CharacterCreator>.Instance.onCompleteWithClothing.AddListener((UnityAction<BasicAvatarSettings, List<ClothingInstance>>)CharacterCreationDone);
			yield return (object)new WaitForSeconds(0.05f);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			Container.gameObject.SetActive(false);
			((Component)rv.ModelContainer).gameObject.SetActive(true);
			PlayerSingleton<PlayerMovement>.Instance.Teleport(NetworkSingleton<GameManager>.Instance.SpawnPoint.position);
			((Component)this).transform.forward = NetworkSingleton<GameManager>.Instance.SpawnPoint.forward;
			GameObject[] disableDuringIntro2 = DisableDuringIntro;
			for (int num = 0; num < disableDuringIntro2.Length; num++)
			{
				disableDuringIntro2[num].gameObject.SetActive(true);
			}
			yield return (object)new WaitForSeconds(1f);
			Singleton<BlackOverlay>.Instance.Close(1f);
		}
	}

	private void PlayMusic()
	{
		if (Singleton<MusicManager>.Instance.TryGetTrack(MusicName, out var track))
		{
			track.Enable();
		}
	}

	public void CharacterCreationDone(BasicAvatarSettings avatar, List<ClothingInstance> clothes)
	{
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			Singleton<BlackOverlay>.Instance.Open();
			yield return (object)new WaitForSeconds(0.5f);
			if (!rv.IsDestroyed)
			{
				((Component)Player.Local).transform.position = PlayerInitialPosition.position;
				((Component)Player.Local).transform.rotation = PlayerInitialPosition.rotation;
			}
			else
			{
				((Component)Player.Local).transform.position = PlayerInitialPosition_AfterRVExplosion.position;
				((Component)Player.Local).transform.rotation = PlayerInitialPosition_AfterRVExplosion.rotation;
			}
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
			Singleton<CharacterCreator>.Instance.DisableStuff();
			yield return (object)new WaitForSeconds(0.5f);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = true;
			Singleton<BlackOverlay>.Instance.Close(1f);
			foreach (ClothingInstance item in clothes)
			{
				Player.Local.Clothing.InsertClothing(item);
			}
			if (onIntroDone != null)
			{
				onIntroDone.Invoke();
			}
			if (InstanceFinder.IsServer)
			{
				if (onIntroDoneAsServer != null)
				{
					onIntroDoneAsServer.Invoke();
				}
				Singleton<SaveManager>.Instance.Save();
			}
			else
			{
				Player.Local.RequestSavePlayer();
			}
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void PassedStep(int stepIndex)
	{
		CurrentStep = stepIndex;
	}
}
