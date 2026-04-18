using UnityEngine;

namespace ScheduleOne.Property;

public class PropertyContentsContainer : MonoBehaviour
{
	public Property Property { get; private set; }

	public void SetProperty(Property property)
	{
		if ((Object)(object)property == (Object)null)
		{
			Debug.LogError((object)"PropertyContentsContainer: Attempted to set a null property.");
		}
		else
		{
			Property = property;
		}
	}
}
