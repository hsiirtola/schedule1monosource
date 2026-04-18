using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.StationFramework;

public class IngredientModule : ItemModule
{
	public IngredientPiece[] Pieces;

	public override void ActivateModule(StationItem item)
	{
		base.ActivateModule(item);
		for (int i = 0; i < Pieces.Length; i++)
		{
			((Component)Pieces[i]).GetComponent<DraggableConstraint>().SetContainer(((Component)item).transform.parent);
		}
	}
}
