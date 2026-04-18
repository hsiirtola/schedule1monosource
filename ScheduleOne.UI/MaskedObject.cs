using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScheduleOne.UI;

public class MaskedObject : UIBehaviour
{
	[SerializeField]
	private CanvasRenderer canvasRendererToClip;

	public bool includeChildren;

	[SerializeField]
	private Canvas rootCanvas;

	[SerializeField]
	private RectTransform maskRectTransform;

	private bool initialized;

	private List<CanvasRenderer> canvasRenderersToClip = new List<CanvasRenderer>();

	protected override void OnRectTransformDimensionsChange()
	{
		((UIBehaviour)this).OnRectTransformDimensionsChange();
		if (initialized)
		{
			SetTargetClippingRect();
		}
	}

	protected override void Awake()
	{
		((UIBehaviour)this).Awake();
		Initialize(rootCanvas, maskRectTransform);
	}

	protected override void Start()
	{
		canvasRenderersToClip.Add(canvasRendererToClip);
		if (includeChildren)
		{
			CanvasRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<CanvasRenderer>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if ((Object)(object)componentsInChildren[i] != (Object)(object)canvasRendererToClip)
				{
					canvasRenderersToClip.Add(componentsInChildren[i]);
				}
			}
		}
		SetTargetClippingRect();
	}

	public void Initialize(Canvas rootCanvas, RectTransform maskRectTransform)
	{
		this.rootCanvas = rootCanvas;
		this.maskRectTransform = maskRectTransform;
		SetTargetClippingRect();
		initialized = true;
	}

	private void SetTargetClippingRect()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = maskRectTransform.rect;
		((Rect)(ref rect)).center = ((Rect)(ref rect)).center + Vector2.op_Implicit(((Component)rootCanvas).transform.InverseTransformPoint(((Transform)maskRectTransform).position));
		foreach (CanvasRenderer item in canvasRenderersToClip)
		{
			item.EnableRectClipping(rect);
		}
	}
}
