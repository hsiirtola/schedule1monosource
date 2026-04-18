using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Product;

public class NewMixDiscoveryBox : MonoBehaviour
{
	private bool isOpen;

	[Header("References")]
	public Transform CameraPosition;

	public TextMeshPro PropertiesText;

	public Animation Animation;

	public InteractableObject IntObj;

	public Transform Lid;

	public MultiTypeVisualsSetter Visuals;

	private Pose closedLidPose;

	private NewMixOperation currentMix;

	public void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		closedLidPose = new Pose(Lid.localPosition, Lid.localRotation);
		CloseCase();
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
		((Component)IntObj).gameObject.SetActive(false);
		_ = NetworkSingleton<ProductManager>.Instance.IsMixComplete;
	}

	public void ShowProduct(ProductDefinition baseDefinition, List<Effect> properties)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)PropertiesText).text = string.Empty;
		foreach (Effect property in properties)
		{
			if (((TMP_Text)PropertiesText).text.Length > 0)
			{
				TextMeshPro propertiesText = PropertiesText;
				((TMP_Text)propertiesText).text = ((TMP_Text)propertiesText).text + "\n";
			}
			TextMeshPro propertiesText2 = PropertiesText;
			((TMP_Text)propertiesText2).text = ((TMP_Text)propertiesText2).text + "<color=#" + ColorUtility.ToHtmlStringRGBA(property.LabelColor) + ">" + property.Name + "</color>";
		}
		ProductDefinition productDefinition = Object.Instantiate<ProductDefinition>(baseDefinition);
		productDefinition.Properties.Clear();
		productDefinition.Initialize(properties);
		productDefinition.GenerateAppearanceSettings();
		Visuals.ApplyVisuals(productDefinition);
		((Component)this).gameObject.SetActive(true);
	}

	private void CloseCase()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		isOpen = false;
		Lid.localPosition = closedLidPose.position;
		Lid.localRotation = closedLidPose.rotation;
	}

	private void OpenCase()
	{
		isOpen = true;
		Animation.Play("New mix box open");
	}

	private void Interacted()
	{
		if (!isOpen)
		{
			OpenCase();
		}
		Registry.GetItem(currentMix.ProductID);
	}
}
