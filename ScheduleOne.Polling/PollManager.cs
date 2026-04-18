using System;
using System.Collections;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace ScheduleOne.Polling;

public class PollManager : MonoBehaviour
{
	public enum EPollSubmissionResult
	{
		InProgress,
		Success,
		Failed
	}

	public const string SERVER_URL = "https://us-central1-s1-polling-987345.cloudfunctions.net/poll";

	private CallResult<EncryptedAppTicketResponse_t> appTicketCallbackResponse;

	private TaskCompletionSource<string> tokenCompletion;

	private PollResponse receivedPollResponse;

	private int sentResponse = -1;

	private string appTicket = string.Empty;

	public Action<PollData> onActivePollReceived;

	public Action<PollData> onConfirmedPollReceived;

	private bool appTicketRequested;

	public PollData ActivePoll { get; private set; }

	public PollData ConfirmedPoll { get; private set; }

	public EPollSubmissionResult SubmissionResult { get; private set; }

	public string SubmisssionFailedMesssage { get; private set; } = string.Empty;

	private void Start()
	{
		if (SteamManager.Initialized)
		{
			((MonoBehaviour)this).StartCoroutine(RequestPoll("https://us-central1-s1-polling-987345.cloudfunctions.net/poll", ResponseCallback));
		}
	}

	private void Update()
	{
	}

	public void GenerateAppTicket()
	{
		if (!appTicketRequested)
		{
			if (!SteamManager.Initialized)
			{
				Console.LogError("Steam not initialized, cannot generate app ticket.");
				return;
			}
			appTicketRequested = true;
			InitAppTicket();
		}
	}

	public void SelectPollResponse(int responseIndex)
	{
		if (string.IsNullOrEmpty(appTicket))
		{
			SubmisssionFailedMesssage = "Failed to generate session ticket.";
			SubmissionResult = EPollSubmissionResult.Failed;
			return;
		}
		Console.Log("Sending poll response: " + responseIndex);
		sentResponse = responseIndex;
		SubmissionResult = EPollSubmissionResult.InProgress;
		PollAnswer answer = new PollAnswer(receivedPollResponse.active, responseIndex, appTicket);
		((MonoBehaviour)this).StartCoroutine(SubmitAnswerToServer(answer));
	}

	private async Task InitAppTicket()
	{
		appTicketCallbackResponse = CallResult<EncryptedAppTicketResponse_t>.Create((APIDispatchDelegate<EncryptedAppTicketResponse_t>)OnEncryptedAppTicketResponse);
		appTicket = CleanTicket(await GetAppTicket());
		Console.Log("App ticket: " + appTicket);
	}

	private IEnumerator SubmitAnswerToServer(PollAnswer answer)
	{
		string text = JsonUtility.ToJson((object)answer);
		Console.Log("Submitting poll response: " + text);
		UnityWebRequest req = UnityWebRequest.Post("https://us-central1-s1-polling-987345.cloudfunctions.net/poll", text, "application/json");
		try
		{
			yield return req.SendWebRequest();
			Console.Log("Result: " + ((object)req.result/*cast due to .constrained prefix*/).ToString());
			Console.Log("Response data: " + req.downloadHandler.text);
			if ((int)req.result != 1)
			{
				Console.LogError("Failed to send poll response!");
				SubmisssionFailedMesssage = req.downloadHandler.text;
				SubmissionResult = EPollSubmissionResult.Failed;
				yield break;
			}
			Console.Log("Successfully submitted poll response!");
			SubmissionResult = EPollSubmissionResult.Success;
			RecordSubmission(answer.pollId, answer.answer);
		}
		finally
		{
			((IDisposable)req)?.Dispose();
		}
	}

	private IEnumerator RequestPoll(string url, Action<string> callback = null)
	{
		UnityWebRequest request = UnityWebRequest.Get(url);
		yield return request.SendWebRequest();
		string text = request.downloadHandler.text;
		callback?.Invoke(text);
	}

	private void ResponseCallback(string data)
	{
		Console.Log("Received poll response: " + data);
		PollResponseWrapper pollResponseWrapper = null;
		try
		{
			pollResponseWrapper = JsonUtility.FromJson<PollResponseWrapper>(data);
		}
		catch (Exception ex)
		{
			Console.LogError("Failed to parse poll response: " + ex.Message);
		}
		if (pollResponseWrapper == null)
		{
			Console.LogError("Failed to parse poll response wrapper: " + data);
			return;
		}
		if (!pollResponseWrapper.success)
		{
			Console.LogError("Failed to get poll response: " + data);
			return;
		}
		receivedPollResponse = pollResponseWrapper.data;
		Console.Log("Received " + receivedPollResponse.polls.Length + " polls.");
		ActivePoll = receivedPollResponse.GetActive();
		ConfirmedPoll = receivedPollResponse.GetConfirmed();
		if (ActivePoll != null)
		{
			Console.Log("Active poll: " + receivedPollResponse.GetActive()?.question);
			if (onActivePollReceived != null)
			{
				onActivePollReceived(ActivePoll);
			}
		}
		else if (ConfirmedPoll != null)
		{
			Console.Log("Confirmed poll: " + receivedPollResponse.GetConfirmed()?.question);
			if (onConfirmedPollReceived != null)
			{
				onConfirmedPollReceived(ConfirmedPoll);
			}
		}
		if (TryGetExistingPollResponse(receivedPollResponse.active, out var response))
		{
			Console.Log("Found existing poll response: " + response);
			sentResponse = response;
		}
	}

	private void OnEncryptedAppTicketResponse(EncryptedAppTicketResponse_t response, bool ioFailure)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)response.m_eResult != 1 || ioFailure)
		{
			Console.LogError("Failed to get valid ticket response");
			return;
		}
		Console.Log("Received ticket response");
		byte[] array = new byte[1024];
		uint newSize = default(uint);
		if (!SteamUser.GetEncryptedAppTicket(array, 1024, ref newSize))
		{
			Console.LogError("GetEncryptedAppTicket fail");
			return;
		}
		Array.Resize(ref array, (int)newSize);
		string result = BitConverter.ToString(array);
		tokenCompletion.SetResult(result);
	}

	private Task<string> GetAppTicket()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamManager.Initialized)
		{
			Console.LogError("Steam not init");
			return null;
		}
		tokenCompletion = new TaskCompletionSource<string>();
		SteamAPICall_t val = SteamUser.RequestEncryptedAppTicket((byte[])null, 0);
		appTicketCallbackResponse.Set(val, (APIDispatchDelegate<EncryptedAppTicketResponse_t>)null);
		return tokenCompletion.Task;
	}

	private static string CleanTicket(string ticket)
	{
		return ticket.Replace("-", "");
	}

	public static bool TryGetExistingPollResponse(int pollId, out int response)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		string text = ((object)SteamUser.GetSteamID()/*cast due to .constrained prefix*/).ToString();
		response = PlayerPrefs.GetInt("poll_response_" + text + pollId, -1);
		if (response == -1)
		{
			response = PlayerPrefs.GetInt("poll_response_" + pollId, -1);
		}
		return response != -1;
	}

	private static void RecordSubmission(int pollId, int response)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		string text = ((object)SteamUser.GetSteamID()/*cast due to .constrained prefix*/).ToString();
		PlayerPrefs.SetInt("poll_response_" + text + pollId, response);
	}
}
