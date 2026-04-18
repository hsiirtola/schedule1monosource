using UnityEngine;

namespace ScheduleOne.Graffiti;

public static class ESprayColorExtensions
{
	public static Color GetColor(this ESprayColor color)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(color switch
		{
			ESprayColor.Black => Color.black, 
			ESprayColor.White => Color.white, 
			ESprayColor.Red => Color.red, 
			ESprayColor.Green => Color.green, 
			ESprayColor.Blue => Color32.op_Implicit(new Color32((byte)30, (byte)100, byte.MaxValue, byte.MaxValue)), 
			ESprayColor.Yellow => Color.yellow, 
			ESprayColor.Pink => Color32.op_Implicit(new Color32(byte.MaxValue, (byte)50, (byte)220, byte.MaxValue)), 
			ESprayColor.Brown => Color32.op_Implicit(new Color32((byte)139, (byte)69, (byte)19, byte.MaxValue)), 
			_ => Color.clear, 
		});
	}
}
