using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class CashCounter : MonoBehaviour
{
	public const float NoteLerpTime = 0.18f;

	public bool IsOn;

	[Header("References")]
	public GameObject UpperNotes;

	public GameObject LowerNotes;

	public Transform NoteStartPoint;

	public Transform NoteEndPoint;

	public List<Transform> MovingNotes = new List<Transform>();

	public AudioSourceController Audio;

	private bool lerping;

	public virtual void LateUpdate()
	{
		UpperNotes.gameObject.SetActive(IsOn);
		LowerNotes.gameObject.SetActive(IsOn);
		if (IsOn)
		{
			if (!lerping)
			{
				lerping = true;
				for (int i = 0; i < MovingNotes.Count; i++)
				{
					if (Singleton<CoroutineService>.InstanceExists)
					{
						((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(LerpNote(MovingNotes[i]));
					}
				}
			}
			if (!Audio.IsPlaying)
			{
				Audio.Play();
			}
		}
		else
		{
			lerping = false;
			if (Audio.IsPlaying)
			{
				Audio.Stop();
			}
		}
	}

	private IEnumerator LerpNote(Transform note)
	{
		yield return (object)new WaitForSeconds((float)MovingNotes.IndexOf(note) / (float)(MovingNotes.Count + 1) * 0.18f);
		((Component)note).gameObject.SetActive(true);
		while (IsOn)
		{
			note.position = NoteStartPoint.position;
			note.rotation = NoteStartPoint.rotation;
			for (float i = 0f; i < 0.18f; i += Time.deltaTime)
			{
				note.position = Vector3.Lerp(NoteStartPoint.position, NoteEndPoint.position, i / 0.18f);
				note.rotation = Quaternion.Lerp(NoteStartPoint.rotation, NoteEndPoint.rotation, i / 0.18f);
				yield return (object)new WaitForEndOfFrame();
			}
		}
		((Component)note).gameObject.SetActive(false);
	}
}
