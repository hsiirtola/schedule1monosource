using System;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class CircularTextMeshPro : MonoBehaviour
{
	[SerializeField]
	private TMP_Text text;

	public AnimationCurve vertexCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
	{
		new Keyframe(0f, 0f, 0f, 1f, 0f, 0f),
		new Keyframe(0.5f, 1f),
		new Keyframe(1f, 0f, 1f, 0f, 0f, 0f)
	});

	public float yCurveScaling = 50f;

	private bool isForceUpdatingMesh;

	private void Reset()
	{
		text = ((Component)this).gameObject.GetComponent<TMP_Text>();
	}

	private void Awake()
	{
		if (!Object.op_Implicit((Object)(object)text))
		{
			text = ((Component)this).gameObject.GetComponent<TMP_Text>();
		}
	}

	private void OnEnable()
	{
		WarpText();
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add((Action<Object>)ReactToTextChanged);
	}

	private void OnDisable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove((Action<Object>)ReactToTextChanged);
		text.ForceMeshUpdate(false, false);
	}

	private void OnValidate()
	{
		WarpText();
	}

	private void ReactToTextChanged(Object obj)
	{
		TMP_Text val = (TMP_Text)(object)((obj is TMP_Text) ? obj : null);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)text) && (Object)(object)val == (Object)(object)text && !isForceUpdatingMesh)
		{
			WarpText();
		}
	}

	private void WarpText()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)text))
		{
			return;
		}
		isForceUpdatingMesh = true;
		vertexCurve.preWrapMode = (WrapMode)1;
		vertexCurve.postWrapMode = (WrapMode)1;
		text.havePropertiesChanged = true;
		text.ForceMeshUpdate(false, false);
		TMP_TextInfo textInfo = text.textInfo;
		if (textInfo == null)
		{
			return;
		}
		int characterCount = textInfo.characterCount;
		if (characterCount == 0)
		{
			return;
		}
		Bounds bounds = text.bounds;
		float x = ((Bounds)(ref bounds)).min.x;
		bounds = text.bounds;
		float x2 = ((Bounds)(ref bounds)).max.x;
		for (int i = 0; i < characterCount; i++)
		{
			if (textInfo.characterInfo[i].isVisible)
			{
				int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				int materialReferenceIndex = textInfo.characterInfo[i].materialReferenceIndex;
				Vector3[] vertices = textInfo.meshInfo[materialReferenceIndex].vertices;
				Vector3 val = Vector2.op_Implicit(new Vector2((vertices[vertexIndex].x + vertices[vertexIndex + 2].x) / 2f, textInfo.characterInfo[i].baseLine));
				ref Vector3 reference = ref vertices[vertexIndex];
				reference += -val;
				ref Vector3 reference2 = ref vertices[vertexIndex + 1];
				reference2 += -val;
				ref Vector3 reference3 = ref vertices[vertexIndex + 2];
				reference3 += -val;
				ref Vector3 reference4 = ref vertices[vertexIndex + 3];
				reference4 += -val;
				float num = (val.x - x) / (x2 - x);
				float num2 = num + 0.0001f;
				float num3 = vertexCurve.Evaluate(num) * yCurveScaling;
				float num4 = vertexCurve.Evaluate(num2) * yCurveScaling;
				Vector3 val2 = new Vector3(1f, 0f, 0f);
				Vector3 val3 = new Vector3(num2 * (x2 - x) + x, num4) - new Vector3(val.x, num3);
				float num5 = Mathf.Acos(Vector3.Dot(val2, ((Vector3)(ref val3)).normalized)) * 57.29578f;
				float num6 = ((Vector3.Cross(val2, val3).z > 0f) ? num5 : (360f - num5));
				Matrix4x4 val4 = Matrix4x4.TRS(new Vector3(0f, num3, 0f), Quaternion.Euler(0f, 0f, num6), Vector3.one);
				vertices[vertexIndex] = ((Matrix4x4)(ref val4)).MultiplyPoint3x4(vertices[vertexIndex]);
				vertices[vertexIndex + 1] = ((Matrix4x4)(ref val4)).MultiplyPoint3x4(vertices[vertexIndex + 1]);
				vertices[vertexIndex + 2] = ((Matrix4x4)(ref val4)).MultiplyPoint3x4(vertices[vertexIndex + 2]);
				vertices[vertexIndex + 3] = ((Matrix4x4)(ref val4)).MultiplyPoint3x4(vertices[vertexIndex + 3]);
				ref Vector3 reference5 = ref vertices[vertexIndex];
				reference5 += val;
				ref Vector3 reference6 = ref vertices[vertexIndex + 1];
				reference6 += val;
				ref Vector3 reference7 = ref vertices[vertexIndex + 2];
				reference7 += val;
				ref Vector3 reference8 = ref vertices[vertexIndex + 3];
				reference8 += val;
				text.UpdateVertexData();
			}
		}
		isForceUpdatingMesh = false;
	}
}
