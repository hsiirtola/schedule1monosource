using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DebugPanelAnimation : MonoBehaviour
{
	public enum AnimationType
	{
		Alpha,
		Scale
	}

	[Header("Target Image")]
	public Image targetImage;

	[Header("Animation Settings")]
	public AnimationType animationType;

	public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float duration = 1f;

	private float timer;

	private bool isPlaying;

	private Color originalColor;

	private Vector3 originalScale;

	private void Awake()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetImage == (Object)null)
		{
			targetImage = ((Component)this).GetComponent<Image>();
		}
		originalColor = ((Graphic)targetImage).color;
		originalScale = ((Transform)((Graphic)targetImage).rectTransform).localScale;
	}

	private void Update()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (isPlaying)
		{
			timer += Time.deltaTime;
			float num = Mathf.Clamp01(timer / duration);
			float num2 = Mathf.Clamp01(alphaCurve.Evaluate(num));
			if (animationType == AnimationType.Alpha)
			{
				Color color = originalColor;
				color.a = num2;
				((Graphic)targetImage).color = color;
			}
			else if (animationType == AnimationType.Scale)
			{
				((Transform)((Graphic)targetImage).rectTransform).localScale = Vector3.Lerp(Vector3.zero, originalScale, num2);
			}
			if (num >= 1f)
			{
				isPlaying = false;
			}
		}
	}

	public void Play()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		timer = 0f;
		isPlaying = true;
		if (animationType == AnimationType.Alpha)
		{
			((Graphic)targetImage).color = originalColor;
		}
		else if (animationType == AnimationType.Scale)
		{
			((Transform)((Graphic)targetImage).rectTransform).localScale = Vector3.zero;
		}
	}
}
