using FishNet.Serializing;
using ScheduleOne.Product;

namespace ScheduleOne.ItemFramework;

public static class ItemSerializers
{
	public const string NullItem = "";

	public static void WriteItemInstance(this Writer writer, ItemInstance value)
	{
		if (value == null)
		{
			writer.WriteString("");
		}
		else
		{
			value.Write(writer);
		}
	}

	public static ItemInstance ReadItemInstance(this Reader reader)
	{
		return ItemInstance.CreateInstanceAndRead(reader);
	}

	public static void WriteProductItemInstance(this Writer writer, ProductItemInstance value)
	{
		writer.WriteItemInstance(value);
	}

	public static ProductItemInstance ReadProductItemInstance(this Reader reader)
	{
		return reader.ReadItemInstance() as ProductItemInstance;
	}
}
