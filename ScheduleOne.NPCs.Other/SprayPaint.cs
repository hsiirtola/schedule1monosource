using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.AvatarFramework.Equipping;
using UnityEngine;

namespace ScheduleOne.NPCs.Other;

public class SprayPaint : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private NPC _npc;

	[SerializeField]
	private AvatarEquippable _sprayPaintPrefab;

	[SerializeField]
	private AvatarAnimation _anim;

	[SerializeField]
	private AudioSourceController _spraySound;

	private AvatarEquippable _sprayPaint;

	private ParticleSystem _sprayEffect;

	private void Awake()
	{
		if ((Object)(object)_npc == (Object)null)
		{
			_npc = ((Component)this).GetComponentInParent<NPC>();
		}
		if ((Object)(object)_anim == (Object)null)
		{
			_anim = ((Component)_npc).GetComponentInChildren<AvatarAnimation>();
		}
	}

	public void Begin()
	{
		_sprayPaint = _npc.SetEquippable_Return(_sprayPaintPrefab.AssetPath);
		_sprayEffect = ((Component)_sprayPaint).GetComponentInChildren<ParticleSystem>(true);
		_spraySound = ((Component)_sprayPaint).GetComponentInChildren<AudioSourceController>(true);
		_anim.SetBool("UseSprayCan", value: true);
	}

	public void End()
	{
		_anim.SetBool("UseSprayCan", value: false);
		if ((Object)(object)_sprayPaint != (Object)null)
		{
			_npc.SetEquippable_Return(string.Empty);
			_sprayPaint = null;
		}
	}

	public void SetEffect(bool value, Color colour = default(Color))
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		((Component)_spraySound).gameObject.SetActive(value);
		((Component)_sprayEffect).gameObject.SetActive(value);
		if (value)
		{
			MainModule main = _sprayEffect.main;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(colour);
		}
	}
}
