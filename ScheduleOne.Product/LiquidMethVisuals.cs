using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.Product;

public class LiquidMethVisuals : MonoBehaviour
{
	public MeshRenderer StaticLiquidMesh;

	public LiquidContainer LiquidContainer;

	public ParticleSystem PourParticles;

	public void Setup(LiquidMethDefinition def)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)def == (Object)null))
		{
			if ((Object)(object)StaticLiquidMesh != (Object)null)
			{
				((Renderer)StaticLiquidMesh).material.color = def.StaticLiquidColor;
			}
			if ((Object)(object)LiquidContainer != (Object)null)
			{
				LiquidContainer.SetLiquidColor(def.LiquidVolumeColor);
			}
			if ((Object)(object)PourParticles != (Object)null)
			{
				MainModule main = PourParticles.main;
				((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(def.PourParticlesColor);
			}
		}
	}
}
