using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CanvasSampleSaveFileImage : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	public Text output;

	private byte[] _textureBytes;

	private void Awake()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		int num = 100;
		int num2 = 100;
		Texture2D val = new Texture2D(num, num2, (TextureFormat)3, false);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				val.SetPixel(i, j, Color.red);
			}
		}
		val.Apply();
		_textureBytes = ImageConversion.EncodeToPNG(val);
		Object.Destroy((Object)(object)val);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(OnClick));
	}

	public void OnClick()
	{
		string text = StandaloneFileBrowser.SaveFilePanel("Title", "", "sample", "png");
		if (!string.IsNullOrEmpty(text))
		{
			File.WriteAllBytes(text, _textureBytes);
		}
	}
}
