using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI;

public class BlackOverlay : Singleton<BlackOverlay>
{
	[Header("References")]
	public Canvas canvas;

	public CanvasGroup group;

	private Coroutine fadeRoutine;

	public bool isShown { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		isShown = false;
		((Behaviour)canvas).enabled = false;
		group.alpha = 0f;
	}

	public void Open(float fadeTime = 0.5f)
	{
		isShown = true;
		((Behaviour)canvas).enabled = true;
		if (fadeRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(fadeRoutine);
		}
		fadeRoutine = ((MonoBehaviour)this).StartCoroutine(Fade(1f, fadeTime));
	}

	public void Close(float fadeTime = 0.5f)
	{
		isShown = false;
		if (fadeRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(fadeRoutine);
		}
		fadeRoutine = ((MonoBehaviour)this).StartCoroutine(Fade(0f, fadeTime));
	}

	private IEnumerator Fade(float endOpacity, float fadeTime)
	{
		float start = group.alpha;
		for (float i = 0f; i < fadeTime; i += Time.deltaTime)
		{
			group.alpha = Mathf.Lerp(start, endOpacity, i / fadeTime);
			yield return (object)new WaitForEndOfFrame();
		}
		group.alpha = endOpacity;
		if (endOpacity == 0f)
		{
			((Behaviour)canvas).enabled = false;
		}
		fadeRoutine = null;
	}
}
