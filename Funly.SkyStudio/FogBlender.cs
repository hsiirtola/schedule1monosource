namespace Funly.SkyStudio;

public class FogBlender : FeatureBlender
{
	protected override string featureKey => "FogFeature";

	protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
	{
		helper.BlendNumber("FogEndDistanceKey");
		helper.BlendNumber("FogLengthKey");
		helper.BlendColor("FogColorKey");
	}

	protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
	{
		helper.BlendNumberIn("FogEndDistanceKey");
	}

	protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
	{
		helper.BlendNumberOut("FogEndDistanceKey");
	}
}
