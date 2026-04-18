using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ScheduleOne.Product;

public class ProductConsumeAnimation : MonoBehaviour
{
	[FormerlySerializedAs("ConsumeAnimationBool")]
	[SerializeField]
	private string _thirdPersonAnimationBool = string.Empty;

	[FormerlySerializedAs("ConsumeAnimationTrigger")]
	[SerializeField]
	private string _thirdPersonAnimationTrigger = string.Empty;

	[SerializeField]
	private AvatarEquippable _thirdPersonEquippable;

	[Header("References")]
	public AudioSourceController ConsumeSound;

	[Header("Events")]
	public UnityEvent onPrepareStart;

	public UnityEvent onPrepareCancel;

	public UnityEvent onConsume;

	[field: SerializeField]
	public string ConsumeDescription { get; private set; } = "Smoke";

	[field: SerializeField]
	public float PrepareDuration { get; private set; } = 1.5f;

	[field: SerializeField]
	public float EffectsApplyDelay { get; private set; } = 2f;

	public void StartPrepare()
	{
		if (onPrepareStart != null)
		{
			onPrepareStart.Invoke();
		}
	}

	public void CancelPrepare()
	{
		if (onPrepareCancel != null)
		{
			onPrepareCancel.Invoke();
		}
	}

	public void StartConsume()
	{
		if (((Component)this).transform.IsChildOf(((Component)Player.Local).transform))
		{
			if (!string.IsNullOrEmpty(_thirdPersonAnimationTrigger))
			{
				Player.Local.SendAnimationTrigger(_thirdPersonAnimationTrigger);
			}
			else if (!string.IsNullOrEmpty(_thirdPersonAnimationBool))
			{
				Player.Local.SendAnimationBool(_thirdPersonAnimationBool, val: true);
			}
			if ((Object)(object)_thirdPersonEquippable != (Object)null)
			{
				Player.Local.SendEquippable_Networked(_thirdPersonEquippable.AssetPath);
			}
		}
		if ((Object)(object)ConsumeSound != (Object)null)
		{
			ConsumeSound.DuplicateAndPlayOneShot(((Component)PlayerSingleton<PlayerInventory>.Instance.equipContainer).transform);
		}
		if (onConsume != null)
		{
			onConsume.Invoke();
		}
	}

	public void StopConsume()
	{
		if (!string.IsNullOrEmpty(_thirdPersonAnimationBool))
		{
			Player.Local.SendAnimationBool(_thirdPersonAnimationBool, val: false);
		}
		if ((Object)(object)_thirdPersonEquippable != (Object)null)
		{
			Player.Local.SendEquippable_Networked(string.Empty);
		}
	}
}
