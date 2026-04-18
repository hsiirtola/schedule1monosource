using System;

namespace UnityEngine.PostProcessing;

[Serializable]
public sealed class ColorGradingCurve
{
	public AnimationCurve curve;

	[SerializeField]
	private bool m_Loop;

	[SerializeField]
	private float m_ZeroValue;

	[SerializeField]
	private float m_Range;

	private AnimationCurve m_InternalLoopingCurve;

	public ColorGradingCurve(AnimationCurve curve, float zeroValue, bool loop, Vector2 bounds)
	{
		this.curve = curve;
		m_ZeroValue = zeroValue;
		m_Loop = loop;
		m_Range = ((Vector2)(ref bounds)).magnitude;
	}

	public void Cache()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if (!m_Loop)
		{
			return;
		}
		int length = curve.length;
		if (length >= 2)
		{
			if (m_InternalLoopingCurve == null)
			{
				m_InternalLoopingCurve = new AnimationCurve();
			}
			Keyframe val = curve[length - 1];
			((Keyframe)(ref val)).time = ((Keyframe)(ref val)).time - m_Range;
			Keyframe val2 = curve[0];
			((Keyframe)(ref val2)).time = ((Keyframe)(ref val2)).time + m_Range;
			m_InternalLoopingCurve.keys = curve.keys;
			m_InternalLoopingCurve.AddKey(val);
			m_InternalLoopingCurve.AddKey(val2);
		}
	}

	public float Evaluate(float t)
	{
		if (curve.length == 0)
		{
			return m_ZeroValue;
		}
		if (!m_Loop || curve.length == 1)
		{
			return curve.Evaluate(t);
		}
		return m_InternalLoopingCurve.Evaluate(t);
	}
}
