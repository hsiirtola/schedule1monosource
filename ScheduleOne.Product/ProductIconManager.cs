using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Product.Packaging;
using UnityEngine;

namespace ScheduleOne.Product;

public class ProductIconManager : Singleton<ProductIconManager>
{
	[Serializable]
	public class ProductIcon
	{
		[HideInInspector]
		public string name;

		public string ProductID;

		public string PackagingID;

		public Sprite Icon;
	}

	public const string ProductIconPath = "Textures/ProductIcons";

	[SerializeField]
	private List<ProductIcon> icons = new List<ProductIcon>();

	[Header("Product and packaging")]
	public IconGenerator IconGenerator;

	public ProductDefinition[] Products;

	public PackagingDefinition[] Packaging;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < Products.Length; i++)
		{
			for (int j = 0; j < Packaging.Length; j++)
			{
				GetIcon(((BaseItemDefinition)Products[i]).ID, ((BaseItemDefinition)Packaging[j]).ID);
			}
			GetIcon(((BaseItemDefinition)Products[i]).ID, "none");
		}
	}

	public Sprite GetIcon(string productID, string packagingID, bool ignoreError = false)
	{
		ProductIcon productIcon = icons.Find((ProductIcon x) => x.ProductID == productID && x.PackagingID == packagingID);
		if (productIcon == null)
		{
			if (!ignoreError)
			{
				Console.LogError("Failed to find icon for packaging (" + packagingID + ") containing product (" + productID + ")");
			}
			return null;
		}
		return productIcon.Icon;
	}

	public Sprite GenerateIcons(string productID)
	{
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Registry.GetItem(productID) == (Object)null)
		{
			Console.LogError("Failed to find product with ID: " + productID);
			return null;
		}
		if (icons.Any((ProductIcon x) => x.ProductID == productID) && (Object)(object)Registry.GetItem(productID) != (Object)null)
		{
			return ((BaseItemDefinition)Registry.GetItem(productID)).Icon;
		}
		for (int num = 0; num < Packaging.Length; num++)
		{
			Texture2D val = GenerateProductTexture(productID, ((BaseItemDefinition)Packaging[num]).ID);
			if ((Object)(object)val == (Object)null)
			{
				Console.LogError("Failed to generate icon for packaging (" + ((BaseItemDefinition)Packaging[num]).ID + ") containing product (" + productID + ")");
			}
			else
			{
				ProductIcon productIcon = new ProductIcon();
				productIcon.name = productID;
				productIcon.ProductID = productID;
				productIcon.PackagingID = ((BaseItemDefinition)Packaging[num]).ID;
				val.Apply();
				productIcon.Icon = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f));
				icons.Add(productIcon);
			}
		}
		Texture2D val2 = GenerateProductTexture(productID, "none");
		val2.Apply();
		return Sprite.Create(val2, new Rect(0f, 0f, (float)((Texture)val2).width, (float)((Texture)val2).height), new Vector2(0.5f, 0.5f));
	}

	private Texture2D GenerateProductTexture(string productID, string packagingID)
	{
		return IconGenerator.GeneratePackagingIcon(packagingID, productID);
	}
}
