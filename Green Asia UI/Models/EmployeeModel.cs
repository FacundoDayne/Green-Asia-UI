﻿using System.ComponentModel.DataAnnotations;

namespace Green_Asia_UI.Models
{
	public class EmployeeNewProject
	{
		public int ID { get; set; }
		public string Title { get; set; }
		public string ClientName { get; set; }
		public DateTime Date { get; set; }
	}
	public class EmployeeDashboardModel
	{
		public List<EmployeeNewProject> projects { get; set; }
	}

	public class TemplateMaterial
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public bool Checked { get; set; }
		public int ID_Template { get; set; }
	}

	public class NewTemplateModel
	{
		[Display(Name = "Name")]
		public string Descritpion { get; set; }
		[Display(Name = "Description")]
		public string Long_Description { get; set; }

		public List<TemplateMaterial> materials { get; set; }

		[Display(Name = "Number of Storeys")]
		[Range(1, 1000000000)]
		public int NumberOfStoreys { get; set; }

		[Display(Name = "Height of floors (m)")]
		[Range(1, 1000000000)]
		public double FloorHeight { get; set; }

		[Display(Name = "Building Length (m)")]
		[Range(1, 1000000000)]
		public double BuildingLength { get; set; }

		[Display(Name = "Building Width (m)")]
		[Range(1, 1000000000)]
		public double BuildingWidth { get; set; }



		[Display(Name = "Floor Thickness (m)")]
		public double floorThickness { get; set; }

		[Display(Name = "Wall Thickness (m)")]
		public double wallThickness { get; set; }

		[Display(Name = "Rebar diameter (m)")]
		public double rebarDiameter { get; set; }

		[Display(Name = "Nail constant (m)")]
		public double nailConstant { get; set; }

		[Display(Name = "Hollow block constant (m)")]
		public double hollowBlockConstant { get; set; }

		[Display(Name = "Support beam length (m)")]
		public double supportBeamLength { get; set; }

		[Display(Name = "Support beam width (m)")]
		public double supportBeamWidth { get; set; }

		[Display(Name = "Support beam interval (m)")]
		public double supportBeamSpace { get; set; }

		[Display(Name = "Concrete Cement Ratio (m)")]
		public double concreteRatioCement { get; set; }

		[Display(Name = "Concrete Sand Ratio (m)")]
		public double concreteRatioSand { get; set; }

		[Display(Name = "Concrete Aggregate Ratio (m)")]
		public double concreteRatioAggregate { get; set; }

		[Display(Name = "Plywood Length (inch)")]
		public double plywoodLength { get; set; }

		[Display(Name = "Plywood Width (inch)")]
		public double plywoodWidth { get; set; }

		[Display(Name = "Stairs riser height (m)")]
		public double riserHeight { get; set; }

		[Display(Name = "Stairs thread depth (m)")]
		public double threadDepth { get; set; }

		[Display(Name = "Stairs width (m)")]
		public double stairsWidth { get; set; }

		[Display(Name = "Wastage (%)")]
		public double wasteage { get; set; }

		[Display(Name = "Provisions (%)")]
		public double provisions { get; set; }
	}
	public class TemplateListItem
	{
		public int ID { get; set; }
		public string Descritpion { get; set; }
		public string Long_Description { get; set; }
	}
	public class EditTemplateModel
	{
		public int ID { get; set; }
		public int FormulaID { get; set; }
		[Display(Name = "Name")]
		public string Descritpion { get; set; }
		[Display(Name = "Description")]
		public string Long_Description { get; set; }

		public List<TemplateMaterial> materials { get; set; }


		[Display(Name = "Number of Storeys")]
		[Range(1, 1000000000)]
		public int NumberOfStoreys { get; set; }

		[Display(Name = "Height of floors (m)")]
		[Range(1, 1000000000)]
		public double FloorHeight { get; set; }

		[Display(Name = "Building Length (m)")]
		[Range(1, 1000000000)]
		public double BuildingLength { get; set; }

		[Display(Name = "Building Width (m)")]
		[Range(1, 1000000000)]
		public double BuildingWidth { get; set; }



		[Display(Name = "Floor Thickness (m)")]
		public double floorThickness { get; set; }

		[Display(Name = "Wall Thickness (m)")]
		public double wallThickness { get; set; }

		[Display(Name = "Rebar diameter (m)")]
		public double rebarDiameter { get; set; }

		[Display(Name = "Nail constant (m)")]
		public double nailConstant { get; set; }

		[Display(Name = "Hollow block constant (m)")]
		public double hollowBlockConstant { get; set; }

		[Display(Name = "Support beam length (m)")]
		public double supportBeamLength { get; set; }

		[Display(Name = "Support beam width (m)")]
		public double supportBeamWidth { get; set; }

		[Display(Name = "Support beam interval (m)")]
		public double supportBeamSpace { get; set; }

		[Display(Name = "Concrete Cement Ratio (m)")]
		public double concreteRatioCement { get; set; }

		[Display(Name = "Concrete Sand Ratio (m)")]
		public double concreteRatioSand { get; set; }

		[Display(Name = "Concrete Aggregate Ratio (m)")]
		public double concreteRatioAggregate { get; set; }

		[Display(Name = "Plywood Length (inch)")]
		public double plywoodLength { get; set; }

		[Display(Name = "Plywood Width (inch)")]
		public double plywoodWidth { get; set; }

		[Display(Name = "Stairs riser height (m)")]
		public double riserHeight { get; set; }

		[Display(Name = "Stairs thread depth (m)")]
		public double threadDepth { get; set; }

		[Display(Name = "Stairs width (m)")]
		public double stairsWidth { get; set; }

		[Display(Name = "Wastage (%)")]
		public double wasteage { get; set; }

		[Display(Name = "Provisions (%)")]
		public double provisions { get; set; }
	}



	public class Employee_BOMTemplate
	{
		public int ID { get; set; }
		public string Description { get; set; }
		public string Description_Long { get; set; }
		public IList<BOMMaterialPickItem> Materials { get; set; }
	}

	public class Employee_BOM_Template_List
	{
		public string ID { get; set; }
		public string Description { get; set; }
	}

	public class BOMMaterialPickItem
	{
		public string ID { get; set; }
		public string Description { get; set; }
		public bool IsChecked { get; set; }
	}

	public class Employee_BOM_Materials_Subitems
	{
		public int SubitemNumber { get; set; }
		public int MaterialID { get; set; }
		public string MaterialDesc { get; set; }
		public string MaterialUoM { get; set; }
		public int MaterialQuantity { get; set; }
		public int MaterialQuantityWastage { get; set; }
		public int MaterialQuantityProvisions { get; set; }
		public double MaterialCost { get; set; }
		public double MarkedUpCost { get; set; }
		public double MaterialAmount { get; set; }
		public double LabourCost { get; set; }
		public double TotalUnitRate { get; set; }
		public double SupplierMaterialID { get; set; }
		public string Supplier { get; set; }
		public string SupplierID { get; set; }
	}

	public class Employee_BOM_Materials_Items
	{
		public IList<Employee_BOM_Materials_Subitems> Subitems { get; set; }
		public int ItemID { get; set; }
		public int ItemNumber { get; set; }
		public string Description { get; set; }
	}

	public class Employee_BOM_Materials_Lists
	{
		public IList<Employee_BOM_Materials_Items> Items { get; set; }
		public char ListLetter { get; set; }
		public int ListNumber { get; set; }
		public string Description { get; set; }
		public int ListID { get; set; }
	}

	public class EmployeeBOMModel
	{
		public List<Employee_BOM_Materials_Lists> lists { get; set; }
		public List<Employee_BOM_Template_List> templates { get; set; }
		public List<BOMMaterialPickItem> materialpicker { get; set; }
		public List<SupplierListItem> suppliers { get; set; }

		public double totalCost { get; set; }

		public int FormulaID { get; set; }
		public int ProjectID { get; set; }

		public double Wastage { get; set; }
		public double Provisions { get; set; }


		[Display(Name = "Markup (%)")]
		public double Markup { get; set; }


		[Display(Name = "BOM Creation Date")]
		public DateTime BOMCreationDate { get; set; }

		[Display(Name = "Template")]
		public string TemplateID { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project Title")]
		public string Title { get; set; }

		[StringLength(100, MinimumLength = 0)]
		[Display(Name = "Client Name")]
		public string ClientName { get; set; }

		[StringLength(100, MinimumLength = 0)]
		[Display(Name = "Client Contact")]
		public string ClientContact { get; set; }

		[Display(Name = "Project Date")]
		public DateTime Date { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project Address")]
		public string Address { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project City")]
		public string City { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project Region")]
		public string Region { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project Country")]
		public string Country { get; set; }

		[Display(Name = "Longtitude")]
		public string Longtitude { get; set; }

		[Display(Name = "Latitude")]
		public string Latitude { get; set; }

		[Display(Name = "Building Type")]
		public string BuildingType { get; set; }

		[Display(Name = "Number of Storeys")]
		[Range(1, 1000000000)]
		public int NumberOfStoreys { get; set; }

		[Display(Name = "Height of floors")]
		[Range(1, 1000000000)]
		public double FloorHeight { get; set; }

		[Display(Name = "Building Length")]
		[Range(1, 1000000000)]
		public double BuildingLength { get; set; }

		[Display(Name = "Building Width")]
		[Range(1, 1000000000)]
		public double BuildingWidth { get; set; }
	}

	public class Employee_BOM_List_Item
	{
		public int ID { get; set; }
		public string Title { get; set; }
		public string ClientName { get; set; }
		public DateTime Date { get; set; }
		public bool MCEExists { get; set; }
	}


	public class Employee_MCE_Materials_Subitems
	{
		public int SubitemNumber { get; set; }
		public int MaterialID { get; set; }
		public string MaterialDesc { get; set; }
		public string MaterialUoM { get; set; }
		public int MaterialQuantity { get; set; }
		public int MaterialQuantityWastage { get; set; }
		public int MaterialQuantityProvisions { get; set; }
		public double MaterialCost { get; set; }
		public double MarkedUpCost { get; set; }
		public double MaterialAmount { get; set; }
		public double LabourCost { get; set; }
		public double TotalUnitRate { get; set; }
		public double SupplierMaterialID { get; set; }
		public string Supplier { get; set; }
		public int SupplierID { get; set; }
	}

	public class Employee_MCE
	{
		public List<Employee_MCE_Materials_Subitems> materials { get; set; }
		public List<Employee_BOM_Template_List> templates { get; set; }

		public double totalIndirectCost { get; set; }
		public double OCM { get; set; }
		public double profit { get; set; }
		public double subtotalCost { get; set; }
		public double tax { get; set; }
		public double totalCost { get; set; }

		public int FormulaID { get; set; }
		public int ProjectID { get; set; }
		public int BOMID { get; set; }

		public double Wastage { get; set; }
		public double Provisions { get; set; }


		[Display(Name = "Markup (%)")]
		public double Markup { get; set; }


		[Display(Name = "BOM Creation Date")]
		public DateTime BOMCreationDate { get; set; }

		[Display(Name = "Template")]
		public string TemplateID { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project Title")]
		public string Title { get; set; }

		[StringLength(100, MinimumLength = 0)]
		[Display(Name = "Client Name")]
		public string ClientName { get; set; }

		[StringLength(100, MinimumLength = 0)]
		[Display(Name = "Client Contact")]
		public string ClientContact { get; set; }

		[Display(Name = "Project Date")]
		public DateTime Date { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project Address")]
		public string Address { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project City")]
		public string City { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project Region")]
		public string Region { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Display(Name = "Project Country")]
		public string Country { get; set; }

		[Display(Name = "Longtitude")]
		public string Longtitude { get; set; }

		[Display(Name = "Latitude")]
		public string Latitude { get; set; }

		[Display(Name = "Building Type")]
		public string BuildingType { get; set; }

		[Display(Name = "Number of Storeys")]
		[Range(1, 1000000000)]
		public int NumberOfStoreys { get; set; }

		[Display(Name = "Height of floors")]
		[Range(1, 1000000000)]
		public double FloorHeight { get; set; }

		[Display(Name = "Building Length")]
		[Range(1, 1000000000)]
		public double BuildingLength { get; set; }

		[Display(Name = "Building Width")]
		[Range(1, 1000000000)]
		public double BuildingWidth { get; set; }
	}


	public class JSONSupplierListItem
	{
		public int ID { get; set; }
		public double Multiplier { get; set; }
	}


	public class SupplierListItem
	{
		public int ID { get; set; }
		public string Description { get; set; }
		public string ContactName { get; set; }
		public string ContactNumber { get; set; }
	}

	public class SupplierInfoModel
	{
		public int ID { get; set; }
		public int CredentialsID { get; set; }
		public int EmployeeID { get; set; }
		public string Description { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public string Country { get; set; }
		public string Longtitude { get; set; }
		public string Latitude { get; set; }
		public string ContactName { get; set; }
		public string ContactNumber { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool Status { get; set; }
	}
}
