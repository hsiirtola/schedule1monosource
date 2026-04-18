using System;

namespace ScheduleOne.Polling;

[Serializable]
public class PollAnswer
{
	public int pollId;

	public int answer;

	public string ticket;

	public PollAnswer(int _pollId, int _answer, string _ticket)
	{
		pollId = _pollId;
		answer = _answer;
		ticket = _ticket;
	}
}
