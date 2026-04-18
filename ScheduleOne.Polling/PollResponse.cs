using System;
using System.Linq;

namespace ScheduleOne.Polling;

[Serializable]
public class PollResponse
{
	public PollData[] polls;

	public int active;

	public int confirmed;

	public PollData GetActive()
	{
		return polls.FirstOrDefault((PollData x) => x.pollId == active);
	}

	public PollData GetConfirmed()
	{
		return polls.FirstOrDefault((PollData x) => x.pollId == confirmed);
	}
}
