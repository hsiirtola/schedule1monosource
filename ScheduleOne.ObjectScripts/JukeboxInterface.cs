using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.ObjectScripts;

public class JukeboxInterface : MonoBehaviour
{
	public const float OPEN_TIME = 0.15f;

	[Header("References")]
	public Jukebox Jukebox;

	public Canvas Canvas;

	public Transform CameraPosition;

	public InteractableObject IntObj;

	public Image PausePlayImage;

	public Button ShuffleButton;

	public Button RepeatButton;

	public Button SyncButton;

	public RectTransform EntryContainer;

	public GameObject AmbientDisplayContainer;

	public TextMeshPro AmbientDisplaySongLabel;

	public TextMeshPro AmbientDisplayTimeLabel;

	[Header("Settings")]
	public Sprite PlaySprite;

	public Sprite PauseSprite;

	public Sprite SongEntryPlaySprite;

	public Sprite SongEntryPauseSprite;

	public Sprite RepeatModeSprite_None;

	public Sprite RepeatModeSprite_Track;

	public Sprite RepeatModeSprite_Queue;

	public Color DeselectedColor;

	public Color SelectedColor;

	public GameObject SongEntryPrefab;

	private List<RectTransform> songEntries = new List<RectTransform>();

	public bool IsOpen { get; private set; }

	private void Awake()
	{
		((Behaviour)Canvas).enabled = false;
		Jukebox jukebox = Jukebox;
		jukebox.onStateChanged = (Action)Delegate.Combine(jukebox.onStateChanged, new Action(RefreshUI));
		Jukebox jukebox2 = Jukebox;
		jukebox2.onStateChanged = (Action)Delegate.Combine(jukebox2.onStateChanged, new Action(RefreshSongEntries));
		Jukebox jukebox3 = Jukebox;
		jukebox3.onStateChanged = (Action)Delegate.Combine(jukebox3.onStateChanged, new Action(RefreshAmbientDisplay));
		SetupSongEntries();
		RefreshUI();
	}

	private void FixedUpdate()
	{
		UpdateAmbientDisplay();
	}

	private void UpdateAmbientDisplay()
	{
		((TMP_Text)AmbientDisplaySongLabel).text = Jukebox.currentTrack.TrackName;
		float currentTrackTime = Jukebox.CurrentTrackTime;
		float length = Jukebox.currentTrack.Clip.length;
		int num = Mathf.FloorToInt(currentTrackTime / 60f);
		int num2 = Mathf.FloorToInt(currentTrackTime % 60f);
		int num3 = Mathf.FloorToInt(length / 60f);
		int num4 = Mathf.FloorToInt(length % 60f);
		((TMP_Text)AmbientDisplayTimeLabel).text = $"{num:D2}:{num2:D2} / {num3:D2}:{num4:D2}";
	}

	private void SetupSongEntries()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		Jukebox.Track[] trackList = Jukebox.TrackList;
		foreach (Jukebox.Track track in trackList)
		{
			GameObject entry = Object.Instantiate<GameObject>(SongEntryPrefab, (Transform)(object)EntryContainer);
			((TMP_Text)((Component)entry.transform.Find("Name")).GetComponent<TextMeshProUGUI>()).text = track.TrackName;
			((TMP_Text)((Component)entry.transform.Find("Artist")).GetComponent<TextMeshProUGUI>()).text = track.ArtistName;
			entry.transform.SetAsLastSibling();
			((UnityEvent)((Component)entry.transform.Find("PlayPause")).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				SongEntryClicked(entry.GetComponent<RectTransform>());
			});
			songEntries.Add(entry.GetComponent<RectTransform>());
		}
	}

	public void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		GameInput.RegisterExitListener(Exit, 2);
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
	}

	private void OnDestroy()
	{
		GameInput.DeregisterExitListener(Exit);
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen)
		{
			action.Used = true;
			Close();
		}
	}

	public void Open()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		RefreshUI();
		RefreshSongEntries();
		IsOpen = true;
		((Behaviour)Canvas).enabled = true;
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.15f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.15f);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		RefreshAmbientDisplay();
	}

	public void Close()
	{
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		RefreshAmbientDisplay();
	}

	private void Hovered()
	{
		if (!IsOpen)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			IntObj.SetMessage("Use jukebox");
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	private void Interacted()
	{
		if (!IsOpen)
		{
			Open();
		}
	}

	public void PlayPausePressed()
	{
		Jukebox.TogglePlay();
	}

	public void BackPressed()
	{
		Jukebox.Back();
	}

	public void NextPressed()
	{
		Jukebox.Next();
	}

	public void ShufflePressed()
	{
		Jukebox.ToggleShuffle();
	}

	public void RepeatPressed()
	{
		Jukebox.ToggleRepeatMode();
	}

	public void SyncPressed()
	{
		Jukebox.ToggleSync();
	}

	public void SongEntryClicked(RectTransform entry)
	{
		int num = songEntries.IndexOf(entry);
		if (Jukebox.currentTrack == Jukebox.TrackList[num])
		{
			Jukebox.TogglePlay();
		}
		else
		{
			Jukebox.PlayTrack(num);
		}
	}

	private void RefreshSongEntries()
	{
		for (int i = 0; i < songEntries.Count; i++)
		{
			Jukebox.Track track = Jukebox.TrackList[i];
			if (Jukebox.currentTrack == track && Jukebox.IsPlaying)
			{
				((Component)((Transform)songEntries[i]).Find("PlayPause/Icon")).GetComponent<Image>().sprite = SongEntryPauseSprite;
			}
			else
			{
				((Component)((Transform)songEntries[i]).Find("PlayPause/Icon")).GetComponent<Image>().sprite = SongEntryPlaySprite;
			}
		}
	}

	private void RefreshUI()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		PausePlayImage.sprite = (Jukebox.IsPlaying ? PauseSprite : PlaySprite);
		((Selectable)ShuffleButton).targetGraphic.color = (Jukebox.Shuffle ? SelectedColor : DeselectedColor);
		((Selectable)SyncButton).targetGraphic.color = (Jukebox.Sync ? SelectedColor : DeselectedColor);
		Sprite sprite = RepeatModeSprite_None;
		switch (Jukebox.RepeatMode)
		{
		case Jukebox.ERepeatMode.None:
			sprite = RepeatModeSprite_None;
			break;
		case Jukebox.ERepeatMode.RepeatTrack:
			sprite = RepeatModeSprite_Track;
			break;
		case Jukebox.ERepeatMode.RepeatQueue:
			sprite = RepeatModeSprite_Queue;
			break;
		}
		Graphic targetGraphic = ((Selectable)RepeatButton).targetGraphic;
		((Image)((targetGraphic is Image) ? targetGraphic : null)).sprite = sprite;
		((Selectable)RepeatButton).targetGraphic.color = ((Jukebox.RepeatMode == Jukebox.ERepeatMode.None) ? DeselectedColor : SelectedColor);
	}

	private void RefreshAmbientDisplay()
	{
		AmbientDisplayContainer.gameObject.SetActive(!IsOpen && Jukebox.IsPlaying);
		if (AmbientDisplayContainer.activeSelf)
		{
			UpdateAmbientDisplay();
		}
	}
}
