using System.Collections;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Product;

public class Product_Equippable : Equippable_Viewmodel
{
	[Header("References")]
	public ProductVisualsSetter Visuals;

	public Transform ModelContainer;

	private ProductConsumeAnimation consumeAnimation;

	private bool isConsumable;

	private float consumeTime;

	private bool consumingInProgress;

	private Vector3 defaultModelPosition = Vector3.zero;

	private Coroutine consumeRoutine;

	private bool mouseUp;

	public string ConsumeDescription => consumeAnimation.ConsumeDescription;

	public float PrepareDuration => consumeAnimation.PrepareDuration;

	public float EffectsApplyDelay => consumeAnimation.EffectsApplyDelay;

	public override void Equip(ItemInstance item)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		base.Equip(item);
		ProductItemInstance productItemInstance = item as ProductItemInstance;
		consumeAnimation = Object.Instantiate<ProductConsumeAnimation>((productItemInstance.Definition as ProductDefinition).ConsumeAnimation, ((Component)this).transform.parent);
		((Component)consumeAnimation).transform.localRotation = Quaternion.identity;
		((Component)consumeAnimation).transform.localPosition = Vector3.zero;
		((Component)consumeAnimation).transform.SetParent(((Component)this).transform);
		isConsumable = productItemInstance.Amount == 1 && (Object)(object)consumeAnimation != (Object)null;
		if (isConsumable)
		{
			Singleton<InputPromptsCanvas>.Instance.LoadModule("consumable");
			((TMP_Text)((Component)((Component)Singleton<InputPromptsCanvas>.Instance.currentModule).gameObject.GetComponentsInChildren<Transform>().FirstOrDefault((Transform c) => ((Object)((Component)c).gameObject).name == "Label")).GetComponent<TextMeshProUGUI>()).text = "(Hold) " + ConsumeDescription;
		}
		ApplyProductVisuals(productItemInstance);
		if ((Object)(object)ModelContainer == (Object)null)
		{
			Console.LogWarning("Model container not set for equippable product: " + ((BaseItemInstance)item).Name);
			ModelContainer = ((Component)this).transform.GetChild(0);
		}
		defaultModelPosition = ModelContainer.localPosition;
	}

	protected virtual void ApplyProductVisuals(ProductItemInstance product)
	{
		Visuals.ApplyVisuals(product);
	}

	public override void Unequip()
	{
		if (isConsumable)
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		}
		if (consumingInProgress)
		{
			consumeAnimation.StopConsume();
			((MonoBehaviour)this).StopCoroutine(consumeRoutine);
		}
		base.Unequip();
	}

	protected override void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		Vector3 val = defaultModelPosition;
		if (!GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
		{
			mouseUp = true;
		}
		if (isConsumable && !consumingInProgress && mouseUp && GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && !Singleton<PauseMenu>.Instance.IsPaused)
		{
			if (consumeTime == 0f)
			{
				StartPrepare();
			}
			consumeTime += Time.deltaTime;
			Singleton<HUD>.Instance.ShowRadialIndicator(consumeTime / PrepareDuration);
			if (consumeTime >= PrepareDuration)
			{
				Consume();
			}
		}
		else
		{
			if (consumeTime > 0f && !consumingInProgress)
			{
				CancelPrepare();
			}
			consumeTime = 0f;
		}
		if (consumeTime > 0f || consumingInProgress)
		{
			val = defaultModelPosition - ((Component)ModelContainer).transform.parent.InverseTransformDirection(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.up) * 0.25f;
		}
		((Component)ModelContainer).transform.localPosition = Vector3.Lerp(((Component)ModelContainer).transform.localPosition, val, Time.deltaTime * 6f);
	}

	protected virtual void StartPrepare()
	{
		if ((Object)(object)consumeAnimation != (Object)null)
		{
			consumeAnimation.StartPrepare();
		}
	}

	protected virtual void CancelPrepare()
	{
		if ((Object)(object)consumeAnimation != (Object)null)
		{
			consumeAnimation.CancelPrepare();
		}
	}

	protected virtual void Consume()
	{
		consumingInProgress = true;
		if ((Object)(object)consumeAnimation != (Object)null)
		{
			consumeAnimation.StartConsume();
		}
		consumeRoutine = ((MonoBehaviour)this).StartCoroutine(ConsumeRoutine());
		IEnumerator ConsumeRoutine()
		{
			yield return (object)new WaitForSeconds(EffectsApplyDelay);
			consumingInProgress = false;
			ApplyEffects();
			((BaseItemInstance)itemInstance).ChangeQuantity(-1);
			PlayerSingleton<PlayerInventory>.Instance.Reequip();
			if ((Object)(object)consumeAnimation != (Object)null)
			{
				consumeAnimation.StopConsume();
			}
		}
	}

	protected virtual void ApplyEffects()
	{
		Player.Local.ConsumeProduct(itemInstance as ProductItemInstance);
	}
}
