using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Seizure", menuName = "Properties/Seizure Property")]
public class Seizure : Effect
{
	public const float CAMERA_JITTER_INTENSITY = 1f;

	public const float DURATION_NPC = 60f;

	public const float DURATION_PLAYER = 30f;

	public override void ApplyToNPC(NPC npc)
	{
		npc.PlayVO(EVOLineType.Hurt);
		npc.Behaviour.RagdollBehaviour.Seizure = true;
		npc.Movement.ActivateRagdoll_Server();
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitForSeconds(60f);
			npc.Behaviour.RagdollBehaviour.Seizure = false;
		}
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Seizure = true;
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitForSeconds(30f);
			player.Seizure = false;
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Behaviour.RagdollBehaviour.Seizure = false;
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Seizure = false;
	}
}
