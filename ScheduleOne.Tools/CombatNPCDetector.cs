using System.Collections;
using ScheduleOne.NPCs;
using ScheduleOne.Police;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

[RequireComponent(typeof(Rigidbody))]
public class CombatNPCDetector : MonoBehaviour
{
	public bool DetectOnlyInCombat;

	public UnityEvent onDetected;

	public float ContactTimeForDetection = 0.5f;

	private NPC npcInContact;

	private float contactTime;

	private Coroutine detectionRoutine;

	private void Awake()
	{
		Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).gameObject.AddComponent<Rigidbody>();
		}
		val.isKinematic = true;
	}

	private IEnumerator UpdateWhileDetected()
	{
		while (true)
		{
			contactTime += Time.fixedDeltaTime;
			if (contactTime >= ContactTimeForDetection)
			{
				contactTime = 0f;
				if (onDetected != null)
				{
					onDetected.Invoke();
				}
			}
			yield return (object)new WaitForFixedUpdate();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		NPC componentInParent = ((Component)other).GetComponentInParent<NPC>();
		if ((Object)(object)componentInParent != (Object)null && (!DetectOnlyInCombat || componentInParent.Behaviour.CombatBehaviour.Active))
		{
			npcInContact = componentInParent;
			if (detectionRoutine == null)
			{
				detectionRoutine = ((MonoBehaviour)this).StartCoroutine(UpdateWhileDetected());
				contactTime = 0f;
			}
			return;
		}
		PoliceOfficer policeOfficer = componentInParent as PoliceOfficer;
		if ((Object)(object)policeOfficer != (Object)null && (!DetectOnlyInCombat || policeOfficer.PursuitBehaviour.Active))
		{
			npcInContact = componentInParent;
			if (detectionRoutine == null)
			{
				detectionRoutine = ((MonoBehaviour)this).StartCoroutine(UpdateWhileDetected());
				contactTime = 0f;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		NPC componentInParent = ((Component)other).GetComponentInParent<NPC>();
		if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent == (Object)(object)npcInContact)
		{
			npcInContact = null;
			if (detectionRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(detectionRoutine);
				detectionRoutine = null;
			}
		}
	}
}
