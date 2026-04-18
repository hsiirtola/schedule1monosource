namespace Funly.SkyStudio;

public struct ProfileBlendingState(SkyProfile blendedProfile, SkyProfile fromProfile, SkyProfile toProfile, float progress, float outProgress, float inProgress, float timeOfDay)
{
	public SkyProfile blendedProfile = blendedProfile;

	public SkyProfile fromProfile = fromProfile;

	public SkyProfile toProfile = toProfile;

	public float progress = progress;

	public float outProgress = outProgress;

	public float inProgress = inProgress;

	public float timeOfDay = timeOfDay;
}
