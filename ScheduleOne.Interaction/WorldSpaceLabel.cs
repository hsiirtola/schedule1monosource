using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Interaction;

public class WorldSpaceLabel
{
	public string text = string.Empty;

	public Color32 color = Color32.op_Implicit(Color.white);

	public Vector3 position = Vector3.zero;

	public float scale = 1f;

	public RectTransform rect;

	public Text textComp;

	public bool active = true;

	public WorldSpaceLabel(string _text, Vector3 _position)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		text = _text;
		position = _position;
		rect = Object.Instantiate<GameObject>(Singleton<InteractionCanvas>.Instance.WSLabelPrefab, (Transform)(object)Singleton<InteractionCanvas>.Instance.WSLabelContainer).GetComponent<RectTransform>();
		textComp = ((Component)rect).GetComponent<Text>();
		Singleton<InteractionCanvas>.Instance.ActiveWSlabels.Add(this);
		RefreshDisplay();
	}

	public void RefreshDisplay()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)PlayerSingleton<PlayerCamera>.Instance).transform.InverseTransformPoint(position).z < -3f || !active)
		{
			((Component)rect).gameObject.SetActive(false);
			return;
		}
		textComp.text = text;
		((Graphic)textComp).color = Color32.op_Implicit(color);
		((Transform)rect).position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(position);
		float num = Mathf.Clamp(1f / Vector3.Distance(position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position), 0f, 1f) * 0.75f * scale;
		((Transform)rect).localScale = new Vector3(num, num, 1f);
		((Component)rect).gameObject.SetActive(true);
	}

	public void Destroy()
	{
		Singleton<InteractionCanvas>.Instance.ActiveWSlabels.Remove(this);
		((Component)rect).gameObject.SetActive(false);
		Object.Destroy((Object)(object)((Component)rect).gameObject);
	}
}
