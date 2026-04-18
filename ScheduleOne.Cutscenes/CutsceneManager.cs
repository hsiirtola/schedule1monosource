using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Cutscenes;

public class CutsceneManager : Singleton<CutsceneManager>
{
	public List<Cutscene> Cutscenes;

	[Header("Run cutscene by name")]
	[SerializeField]
	private string cutsceneName = "Wake up morning";

	private Cutscene playingCutscene;

	[Button]
	private void RunCutscene()
	{
		Play(cutsceneName);
	}

	public void Play(string name)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		Cutscene cutscene = Cutscenes.Find((Cutscene c) => c.Name.ToLower() == name.ToLower());
		if ((Object)(object)cutscene != (Object)null)
		{
			cutscene.Play();
			playingCutscene = cutscene;
			playingCutscene.onEnd.AddListener(new UnityAction(Ended));
		}
	}

	private void Ended()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		playingCutscene.onEnd.RemoveListener(new UnityAction(Ended));
		playingCutscene = null;
	}
}
