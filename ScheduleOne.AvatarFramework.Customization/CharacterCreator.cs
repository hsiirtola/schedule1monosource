using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Networking;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.CharacterCreator;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Customization;

public class CharacterCreator : Singleton<CharacterCreator>
{
	public enum ECategory
	{
		Body,
		Hair,
		Face,
		Eyes,
		Eyebrows,
		Clothing,
		Accessories
	}

	public List<BaseCharacterCreatorField> Fields = new List<BaseCharacterCreatorField>();

	[Header("References")]
	public Transform Container;

	public Transform CameraPosition;

	public Transform RigContainer;

	public Avatar Rig;

	public Canvas Canvas;

	public Animation CanvasAnimation;

	[Header("Settings")]
	public bool DemoCreator;

	public BasicAvatarSettings DefaultSettings;

	public List<BasicAvatarSettings> Presets;

	public UnityEvent<BasicAvatarSettings> onComplete;

	public UnityEvent<BasicAvatarSettings, List<ClothingInstance>> onCompleteWithClothing;

	private Dictionary<string, ClothingDefinition> lastSelectedClothingDefinitions = new Dictionary<string, ClothingDefinition>();

	private float rigTargetY;

	public bool IsOpen { get; protected set; }

	public BasicAvatarSettings ActiveSettings { get; protected set; }

	protected override void Awake()
	{
		if (DemoCreator)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		base.Awake();
		Fields = ((Component)Canvas).GetComponentsInChildren<BaseCharacterCreatorField>(true).ToList();
	}

	protected override void Start()
	{
		base.Start();
		((Component)Container).gameObject.SetActive(false);
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		RigContainer.localEulerAngles = Vector3.Lerp(RigContainer.localEulerAngles, new Vector3(0f, rigTargetY, 0f), Time.deltaTime * 5f);
	}

	public void Open(BasicAvatarSettings initialSettings, bool showUI = true)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		if (showUI)
		{
			ShowUI();
		}
		if (!DemoCreator)
		{
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0f);
		}
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
		((Component)Container).gameObject.SetActive(true);
		if (InstanceFinder.IsServer && !Singleton<Lobby>.Instance.IsInLobby)
		{
			NetworkSingleton<TimeManager>.Instance.SetTimeSpeedMultiplier(0f);
		}
		if ((Object)(object)initialSettings != (Object)null)
		{
			ActiveSettings = Object.Instantiate<BasicAvatarSettings>(initialSettings);
		}
		else
		{
			ActiveSettings = ScriptableObject.CreateInstance<BasicAvatarSettings>();
		}
		Rig.LoadAvatarSettings(ActiveSettings.GetAvatarSettings());
		for (int i = 0; i < Fields.Count; i++)
		{
			Fields[i].ApplyValue();
			Fields[i].WriteValue(applyValue: false);
		}
	}

	public void ShowUI()
	{
		((Behaviour)Canvas).enabled = true;
		CanvasAnimation.Play("Character creator fade in");
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
	}

	public void Close()
	{
		IsOpen = false;
		((MonoBehaviour)this).StartCoroutine(Close());
		IEnumerator Close()
		{
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			rigTargetY = 0f;
			((Behaviour)Canvas).enabled = false;
			if (InstanceFinder.IsServer)
			{
				NetworkSingleton<TimeManager>.Instance.SetTimeSpeedMultiplier(1f);
			}
			yield break;
		}
	}

	public void DisableStuff()
	{
		((Component)Container).gameObject.SetActive(false);
	}

	public void Done()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			List<ClothingInstance> list = new List<ClothingInstance>();
			if (!string.IsNullOrEmpty(ActiveSettings.Shoes))
			{
				EClothingColor clothingColor = ClothingColorExtensions.GetClothingColor(ActiveSettings.ShoesColor);
				list.Add(new ClothingInstance(lastSelectedClothingDefinitions["Shoes"], 1, clothingColor));
			}
			if (!string.IsNullOrEmpty(ActiveSettings.Top))
			{
				EClothingColor clothingColor2 = ClothingColorExtensions.GetClothingColor(ActiveSettings.TopColor);
				list.Add(new ClothingInstance(lastSelectedClothingDefinitions["Top"], 1, clothingColor2));
			}
			if (!string.IsNullOrEmpty(ActiveSettings.Bottom))
			{
				EClothingColor clothingColor3 = ClothingColorExtensions.GetClothingColor(ActiveSettings.BottomColor);
				list.Add(new ClothingInstance(lastSelectedClothingDefinitions["Bottom"], 1, clothingColor3));
			}
			if (onComplete != null)
			{
				onComplete.Invoke(ActiveSettings);
			}
			if (onCompleteWithClothing != null)
			{
				onCompleteWithClothing.Invoke(ActiveSettings, list);
			}
			Close();
		}
	}

	public void SliderChanged(float newVal)
	{
		rigTargetY = newVal * 359f;
	}

	public T SetValue<T>(string fieldName, T value, ClothingDefinition definition)
	{
		if (!lastSelectedClothingDefinitions.ContainsKey(fieldName))
		{
			lastSelectedClothingDefinitions.Add(fieldName, definition);
		}
		else
		{
			lastSelectedClothingDefinitions[fieldName] = definition;
		}
		if (fieldName == "Preset")
		{
			SelectPreset(value as string);
			return default(T);
		}
		ActiveSettings.SetValue(fieldName, value);
		return value;
	}

	public void SelectPreset(string presetName)
	{
		BasicAvatarSettings basicAvatarSettings = Presets.Find((BasicAvatarSettings p) => ((Object)p).name == presetName);
		if ((Object)(object)basicAvatarSettings == (Object)null)
		{
			Debug.LogError((object)("Preset not found: " + presetName));
			return;
		}
		ActiveSettings = Object.Instantiate<BasicAvatarSettings>(basicAvatarSettings);
		Rig.LoadAvatarSettings(ActiveSettings.GetAvatarSettings());
		for (int num = 0; num < Fields.Count; num++)
		{
			Fields[num].ApplyValue();
		}
	}

	public void RefreshCategory(ECategory category)
	{
		AvatarSettings avatarSettings = ActiveSettings.GetAvatarSettings();
		switch (category)
		{
		case ECategory.Body:
			Rig.ApplyBodySettings(avatarSettings);
			Rig.ApplyEyeLidColorSettings(avatarSettings);
			Rig.ApplyBodyLayerSettings(avatarSettings);
			break;
		case ECategory.Hair:
			Rig.ApplyHairSettings(avatarSettings);
			Rig.ApplyHairColorSettings(avatarSettings);
			Rig.ApplyFaceLayerSettings(avatarSettings);
			break;
		case ECategory.Face:
			Rig.ApplyFaceLayerSettings(avatarSettings);
			break;
		case ECategory.Eyes:
			Rig.ApplyEyeBallSettings(avatarSettings);
			Rig.ApplyEyeLidColorSettings(avatarSettings);
			Rig.ApplyEyeLidSettings(avatarSettings);
			break;
		case ECategory.Eyebrows:
			Rig.ApplyEyebrowSettings(avatarSettings);
			break;
		case ECategory.Clothing:
			Rig.ApplyBodyLayerSettings(avatarSettings);
			break;
		case ECategory.Accessories:
			Rig.ApplyAccessorySettings(avatarSettings);
			break;
		}
	}
}
