using System;
using System.Collections;
using FishNet;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class DemoIntro : Singleton<DemoIntro>
{
	public const float SKIP_TIME = 0.5f;

	public Animation Anim;

	public Transform PlayerInitialPosition;

	public GameObject SkipContainer;

	public Image SkipDial;

	public int SkipEvents = 3;

	public UnityEvent onStart;

	public UnityEvent onStartAsServer;

	public UnityEvent onCutsceneDone;

	public UnityEvent onIntroDone;

	public UnityEvent onIntroDoneAsServer;

	private int CurrentStep;

	public string MusicName;

	private float currentSkipTime;

	private bool depressed = true;

	private bool waitingForCutsceneEnd;

	public bool IsPlaying { get; protected set; }

	private void Update()
	{
		if (waitingForCutsceneEnd && !Anim.isPlaying)
		{
			CutsceneDone();
		}
		if (!Anim.isPlaying)
		{
			return;
		}
		if ((GameInput.GetButton(GameInput.ButtonCode.Jump) || GameInput.GetButton(GameInput.ButtonCode.Submit) || GameInput.GetButton(GameInput.ButtonCode.PrimaryClick)) && depressed && CurrentStep < SkipEvents - 1)
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
		IsPlaying = true;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
		Anim.Play();
		((MonoBehaviour)this).Invoke("PlayMusic", 1f);
		if (onStart != null)
		{
			onStart.Invoke();
		}
		waitingForCutsceneEnd = true;
		if (InstanceFinder.IsServer && onStartAsServer != null)
		{
			onStartAsServer.Invoke();
		}
	}

	private void PlayMusic()
	{
		if (Singleton<MusicManager>.Instance.TryGetTrack(MusicName, out var _))
		{
			throw new NotImplementedException();
		}
	}

	public void ShowAvatar()
	{
		Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.Open(Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.DefaultSettings, showUI: false);
	}

	public void CutsceneDone()
	{
		waitingForCutsceneEnd = false;
		Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.ShowUI();
		Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.onComplete.AddListener((UnityAction<BasicAvatarSettings>)CharacterCreationDone);
		if (onCutsceneDone != null)
		{
			onCutsceneDone.Invoke();
		}
		IsPlaying = false;
	}

	public void PassedStep(int stepIndex)
	{
		CurrentStep = stepIndex;
	}

	public void CharacterCreationDone(BasicAvatarSettings avatar)
	{
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			Singleton<BlackOverlay>.Instance.Open();
			yield return (object)new WaitForSeconds(0.5f);
			((Component)Player.Local).transform.position = PlayerInitialPosition.position;
			((Component)Player.Local).transform.rotation = PlayerInitialPosition.rotation;
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
			Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.DisableStuff();
			yield return (object)new WaitForSeconds(0.5f);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = true;
			Singleton<BlackOverlay>.Instance.Close(1f);
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
}
