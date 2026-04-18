using System;
using System.Collections;
using System.Collections.Generic;
using SFB;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CanvasSampleOpenFileTextMultiple : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	public Text output;

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(OnClick));
	}

	private void OnClick()
	{
		string[] array = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", multiselect: true);
		if (array.Length != 0)
		{
			List<string> list = new List<string>(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(new Uri(array[i]).AbsoluteUri);
			}
			((MonoBehaviour)this).StartCoroutine(OutputRoutine(list.ToArray()));
		}
	}

	private IEnumerator OutputRoutine(string[] urlArr)
	{
		string outputText = "";
		for (int i = 0; i < urlArr.Length; i++)
		{
			WWW loader = new WWW(urlArr[i]);
			yield return loader;
			outputText += loader.text;
		}
		output.text = outputText;
	}
}
