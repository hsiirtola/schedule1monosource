using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.UI;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class UseBrickPress : Task
{
	public enum EStep
	{
		Pouring,
		Pressing,
		Complete
	}

	public const float PRODUCT_SCALE = 0.75f;

	protected EStep currentStep;

	protected BrickPress press;

	protected ProductItemInstance product;

	protected List<FunctionalProduct> products = new List<FunctionalProduct>();

	protected Draggable container;

	public override string TaskName { get; protected set; } = "Use brick press";

	public UseBrickPress(BrickPress _press, ProductItemInstance _product)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_press == (Object)null)
		{
			Console.LogError("Press is null!");
			return;
		}
		if (_press.GetState() != PackagingStation.EState.CanBegin)
		{
			Console.LogError("Press not ready to begin packaging!");
			return;
		}
		press = _press;
		product = _product;
		EnableMultiDragging(press.ItemContainer, 0.15f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(press.CameraPosition_Pouring.position, press.CameraPosition_Pouring.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.2f);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("packaging");
		((Component)press.Container1).gameObject.SetActive(false);
		container = press.CreateFunctionalContainer(product, 0.75f, out products);
		base.CurrentInstruction = "Pour product into mould (0/20)";
		Singleton<OnScreenMouse>.Instance.Activate();
		((MonoBehaviour)press).StartCoroutine(CheckMould());
		IEnumerator CheckMould()
		{
			while (base.TaskActive)
			{
				this.CheckMould();
				yield return (object)new WaitForSeconds(0.2f);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (currentStep == EStep.Pressing && press.Handle.CurrentPosition >= 1f)
		{
			FinishPress();
		}
	}

	public override void StopTask()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		base.StopTask();
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		if ((Object)(object)container != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)container).gameObject);
		}
		for (int i = 0; i < products.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)products[i]).gameObject);
		}
		((Component)press.Container1).gameObject.SetActive(true);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(press.CameraPosition.position, press.CameraPosition.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
		Singleton<BrickPressCanvas>.Instance.SetIsOpen(press, open: true);
		press.Handle.Locked = false;
		press.Handle.SetInteractable(e: false);
		if (currentStep == EStep.Complete)
		{
			press.CompletePress(product);
		}
		Singleton<OnScreenMouse>.Instance.Deactivate();
	}

	private void CheckMould()
	{
		if (currentStep != EStep.Pouring)
		{
			return;
		}
		List<FunctionalProduct> productInMould = press.GetProductInMould();
		base.CurrentInstruction = "Pour product into mould (" + productInMould.Count + "/20)";
		if (productInMould.Count < 20)
		{
			return;
		}
		foreach (FunctionalProduct item in productInMould)
		{
			item.ClickableEnabled = false;
		}
		DisableMultiDragging();
		BeginPress();
	}

	private void BeginPress()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		currentStep = EStep.Pressing;
		press.Handle.SetInteractable(e: true);
		container.ClickableEnabled = false;
		container.Rb.AddForce((((Component)press).transform.right + ((Component)press).transform.up) * 2f, (ForceMode)2);
		base.CurrentInstruction = "Rotate handle quickly to press product";
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.3f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(press.CameraPosition_Raising.position, press.CameraPosition_Raising.rotation, 0.3f);
	}

	private void FinishPress()
	{
		press.SlamSound.Play();
		currentStep = EStep.Complete;
		press.Handle.Locked = true;
		press.Handle.SetInteractable(e: false);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.1f);
		PlayerSingleton<PlayerCamera>.Instance.StartCameraShake(0.25f, 0.2f);
		((MonoBehaviour)press).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitForSeconds(0.8f);
			StopTask();
		}
	}
}
