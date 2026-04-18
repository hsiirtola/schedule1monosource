using FishNet.Object;
using ScheduleOne.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Skating;

public class SkateboardAudio : MonoBehaviour
{
	public Skateboard Board;

	[Header("References")]
	public AudioSourceController JumpAudio;

	public AudioSourceController LandAudio;

	public AudioSourceController RollingAudio;

	public AudioSourceController DirtRollingAudio;

	public AudioSourceController WindAudio;

	private void Awake()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		if ((Object)(object)Board == (Object)null)
		{
			Board = ((Component)this).GetComponentInParent<Skateboard>();
		}
		Board.OnJump.AddListener((UnityAction<float>)PlayJump);
		Board.OnLand.AddListener(new UnityAction(PlayLand));
	}

	private void Start()
	{
		if (Board.IsGrounded())
		{
			PlayLand();
		}
		RollingAudio.VolumeMultiplier = 0f;
		RollingAudio.Play();
		DirtRollingAudio.VolumeMultiplier = 0f;
		DirtRollingAudio.Play();
		WindAudio.VolumeMultiplier = 0f;
		WindAudio.Play();
	}

	private void Update()
	{
		float num = Mathf.Clamp(Mathf.Abs(Board.CurrentSpeed_Kmh) / Board.CurentSettings.TopSpeed_Kmh, 0f, 1.5f);
		float volumeMultiplier = num;
		if (Board.AirTime > 0.2f)
		{
			volumeMultiplier = 0f;
		}
		DirtRollingAudio.VolumeMultiplier = 0f;
		RollingAudio.VolumeMultiplier = 0f;
		AudioSourceController obj = (Board.IsOnTerrain() ? DirtRollingAudio : RollingAudio);
		obj.VolumeMultiplier = volumeMultiplier;
		obj.PitchMultiplier = Mathf.Lerp(0.75f, 1f, num);
		if (((NetworkBehaviour)Board).IsOwner)
		{
			WindAudio.VolumeMultiplier = num;
			WindAudio.PitchMultiplier = Mathf.Lerp(1.2f, 1.5f, num);
		}
		else
		{
			WindAudio.VolumeMultiplier = 0f;
		}
	}

	public void PlayJump(float force)
	{
		JumpAudio.VolumeMultiplier = Mathf.Lerp(0.5f, 1f, force);
		JumpAudio.Play();
	}

	public void PlayLand()
	{
		LandAudio.Play();
	}
}
