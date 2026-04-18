using UnityEngine;

namespace ScheduleOne.AvatarFramework;

public class Eyebrow : MonoBehaviour
{
	public enum ESide
	{
		Right,
		Left
	}

	private const float eyebrowHeightMultiplier = 0.01f;

	[SerializeField]
	private Vector3 EyebrowDefaultScale;

	[SerializeField]
	private Vector3 EyebrowDefaultLocalPos;

	[SerializeField]
	protected ESide Side;

	[SerializeField]
	protected Transform Model;

	[SerializeField]
	protected MeshRenderer Rend;

	[Header("Eyebrow Data - Readonly")]
	[SerializeField]
	private Color col;

	[SerializeField]
	private float scale = 1f;

	[SerializeField]
	private float thickness = 1f;

	[SerializeField]
	private float restingAngle;

	public void SetScale(float _scale)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		scale = _scale;
		Model.localScale = new Vector3(EyebrowDefaultScale.x, EyebrowDefaultScale.y, EyebrowDefaultScale.z * thickness) * scale;
	}

	public void SetThickness(float thickness)
	{
		this.thickness = thickness;
		SetScale(scale);
	}

	public void SetRestingAngle(float _angle)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		restingAngle = _angle;
		((Component)this).transform.localRotation = Quaternion.Euler(((Component)this).transform.localEulerAngles.x, ((Component)this).transform.localEulerAngles.y, restingAngle * ((Side == ESide.Left) ? (-1f) : 1f));
	}

	public void SetRestingHeight(float normalizedHeight)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		normalizedHeight = Mathf.Clamp(normalizedHeight, -1.1f, 1.5f);
		((Component)Model).transform.localPosition = new Vector3(EyebrowDefaultLocalPos.x, EyebrowDefaultLocalPos.y + normalizedHeight * 0.01f, EyebrowDefaultLocalPos.z);
	}

	public void SetColor(Color _col)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		col = _col;
		((Renderer)Rend).material.color = col;
	}
}
