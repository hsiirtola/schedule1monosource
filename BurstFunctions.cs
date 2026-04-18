using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public static class BurstFunctions
{
	public delegate void Average_00000087_0024PostfixBurstDelegate(ref NativeArray<float> arr, out float result);

	internal static class Average_00000087_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(Average_00000087_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static Average_00000087_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref NativeArray<float> arr, out float result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<float>, ref float, void>)functionPointer)(ref arr, ref result);
					return;
				}
			}
			Average_0024BurstManaged(ref arr, out result);
		}
	}

	public delegate void Average_00000088_0024PostfixBurstDelegate(ref NativeArray<Vector3> arr, out Vector3 result);

	internal static class Average_00000088_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(Average_00000088_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static Average_00000088_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref NativeArray<Vector3> arr, out Vector3 result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref Vector3, void>)functionPointer)(ref arr, ref result);
					return;
				}
			}
			Average_0024BurstManaged(ref arr, out result);
		}
	}

	[BurstCompile]
	public static void Average(ref NativeArray<float> arr, out float result)
	{
		Average_00000087_0024BurstDirectCall.Invoke(ref arr, out result);
	}

	[BurstCompile]
	public static void Average(ref NativeArray<Vector3> arr, out Vector3 result)
	{
		Average_00000088_0024BurstDirectCall.Invoke(ref arr, out result);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	public static void Average_0024BurstManaged(ref NativeArray<float> arr, out float result)
	{
		float num = 0f;
		for (int i = 0; i < arr.Length; i++)
		{
			num += arr[i];
		}
		result = num / (float)arr.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	public static void Average_0024BurstManaged(ref NativeArray<Vector3> arr, out Vector3 result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int i = 0; i < arr.Length; i++)
		{
			val += arr[i];
		}
		result = val / (float)arr.Length;
	}
}
