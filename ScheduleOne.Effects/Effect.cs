using ScheduleOne.Employees;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

public abstract class Effect : ScriptableObject
{
	public string Name = string.Empty;

	public string Description = string.Empty;

	public string ID = string.Empty;

	[Range(1f, 5f)]
	public int Tier = 1;

	[Range(0f, 1f)]
	public float Addictiveness = 0.1f;

	public Color ProductColor = Color.white;

	public Color LabelColor = Color.white;

	public bool ImplementedPriorMixingRework;

	[Header("Value")]
	[Range(-100f, 100f)]
	public int ValueChange;

	[Range(0f, 2f)]
	public float ValueMultiplier = 1f;

	[Range(-1f, 1f)]
	public float AddBaseValueMultiple;

	public Vector2 MixDirection = Vector2.zero;

	public float MixMagnitude = 1f;

	public virtual void ApplyToNPC(NPC npc)
	{
		if (npc is Employee)
		{
			ApplyToEmployee(npc as Employee);
		}
	}

	public virtual void ClearFromNPC(NPC npc)
	{
		if (npc is Employee)
		{
			ClearFromEmployee(npc as Employee);
		}
	}

	public abstract void ApplyToPlayer(Player player);

	public abstract void ClearFromPlayer(Player player);

	protected virtual void ApplyToEmployee(Employee employee)
	{
	}

	protected virtual void ClearFromEmployee(Employee employee)
	{
	}

	public void OnValidate()
	{
		if (Name == string.Empty)
		{
			Name = ((Object)this).name;
		}
		if (ID == string.Empty)
		{
			ID = ((Object)this).name.ToLower();
		}
	}
}
