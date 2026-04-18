using System.Collections;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCustomization;

public class CharacterCustomizationUI : MonoBehaviour
{
	[Header("Settings")]
	public string Title = "Customize";

	public CharacterCustomizationCategory[] Categories;

	public bool LoadAvatarSettingsNaked;

	[Header("References")]
	public Canvas Canvas;

	public RectTransform MainContainer;

	public RectTransform MenuContainer;

	public TextMeshProUGUI TitleText;

	public RectTransform ButtonContainer;

	public Button ExitButton;

	public Slider RigRotationSlider;

	public RectTransform PreviewIndicator;

	public CharacterCustomizationShop CharacterCustomizationShop;

	[Header("Prefab")]
	public Button CategoryButtonPrefab;

	private float rigTargetY;

	private Coroutine openCloseRoutine;

	protected BasicAvatarSettings currentSettings;

	public bool IsOpen { get; private set; }

	public CharacterCustomizationCategory ActiveCategory { get; private set; }

	private void OnValidate()
	{
		Categories = ((Component)this).GetComponentsInChildren<CharacterCustomizationCategory>(true);
		((TMP_Text)TitleText).text = Title;
	}

	private void Awake()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		GameInput.RegisterExitListener(Exit, 1);
		((UnityEvent<float>)(object)RigRotationSlider.onValueChanged).AddListener((UnityAction<float>)delegate(float value)
		{
			rigTargetY = value * 359f;
		});
		Categories = ((Component)this).GetComponentsInChildren<CharacterCustomizationCategory>(true);
		((TMP_Text)TitleText).text = Title;
		((UnityEvent)ExitButton.onClick).AddListener(new UnityAction(Close));
		for (int num = 0; num < Categories.Length; num++)
		{
			Button obj = Object.Instantiate<Button>(CategoryButtonPrefab, (Transform)(object)ButtonContainer);
			((TMP_Text)((Component)obj).GetComponentInChildren<TextMeshProUGUI>()).text = Categories[num].CategoryName;
			CharacterCustomizationCategory category = Categories[num];
			((UnityEvent)obj.onClick).AddListener((UnityAction)delegate
			{
				SetActiveCategory(category);
			});
		}
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)MainContainer).gameObject.SetActive(false);
		((Component)CharacterCustomizationShop.AvatarRig).gameObject.SetActive(false);
		SetActiveCategory(null);
	}

	protected virtual void Update()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			CharacterCustomizationShop.RigContainer.localEulerAngles = Vector3.Lerp(CharacterCustomizationShop.RigContainer.localEulerAngles, new Vector3(0f, rigTargetY, 0f), Time.deltaTime * 5f);
		}
	}

	public void SetActiveCategory(CharacterCustomizationCategory category)
	{
		ActiveCategory = category;
		for (int i = 0; i < Categories.Length; i++)
		{
			((Component)Categories[i]).gameObject.SetActive((Object)(object)Categories[i] == (Object)(object)category);
			if ((Object)(object)Categories[i] == (Object)(object)category)
			{
				Categories[i].Open();
			}
		}
		((Component)MenuContainer).gameObject.SetActive((Object)(object)category == (Object)null);
	}

	public virtual bool IsOptionCurrentlyApplied(CharacterCustomizationOption option)
	{
		return false;
	}

	public virtual void OptionSelected(CharacterCustomizationOption option)
	{
		((Component)PreviewIndicator).gameObject.SetActive(!option.purchased);
	}

	public virtual void OptionDeselected(CharacterCustomizationOption option)
	{
		Console.Log("Deselected option: " + option.Label);
	}

	public virtual void OptionPurchased(CharacterCustomizationOption option)
	{
		((Component)PreviewIndicator).gameObject.SetActive(false);
	}

	public virtual void Open()
	{
		if (openCloseRoutine == null)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			currentSettings = Object.Instantiate<BasicAvatarSettings>(Player.Local.CurrentAvatarSettings);
			openCloseRoutine = ((MonoBehaviour)this).StartCoroutine(Close());
		}
		IEnumerator Close()
		{
			Singleton<BlackOverlay>.Instance.Open();
			yield return (object)new WaitForSeconds(0.6f);
			IsOpen = true;
			((Behaviour)Canvas).enabled = true;
			((Component)MainContainer).gameObject.SetActive(true);
			((Component)CharacterCustomizationShop.AvatarRig).gameObject.SetActive(true);
			if (LoadAvatarSettingsNaked)
			{
				CharacterCustomizationShop.AvatarRig.LoadNakedSettings(Player.Local.Avatar.CurrentSettings);
			}
			else
			{
				CharacterCustomizationShop.AvatarRig.LoadAvatarSettings(Player.Local.Avatar.CurrentSettings);
			}
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CharacterCustomizationShop.CameraPosition.position, CharacterCustomizationShop.CameraPosition.rotation, 0f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0f);
			SetActiveCategory(null);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			Singleton<BlackOverlay>.Instance.Close();
			openCloseRoutine = null;
		}
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			if ((Object)(object)ActiveCategory != (Object)null)
			{
				ActiveCategory.Back();
			}
			else
			{
				Close();
			}
		}
	}

	protected virtual void Close()
	{
		if (openCloseRoutine == null)
		{
			SetActiveCategory(null);
			IsOpen = false;
			((Behaviour)Canvas).enabled = false;
			((Component)MainContainer).gameObject.SetActive(false);
			Player.Local.SendAppearance(currentSettings);
			openCloseRoutine = ((MonoBehaviour)this).StartCoroutine(Close());
		}
		IEnumerator Close()
		{
			Singleton<BlackOverlay>.Instance.Open();
			yield return (object)new WaitForSeconds(0.6f);
			((Component)CharacterCustomizationShop.AvatarRig).gameObject.SetActive(false);
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f);
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
				PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			Singleton<BlackOverlay>.Instance.Close();
			openCloseRoutine = null;
		}
	}
}
