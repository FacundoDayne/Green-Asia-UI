using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Bcpg;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Green_Asia_UI.Models
{
	public class EmployeeDashboardProjects
	{
		public int ID { get; set; }
		public string Description { get; set; }
	}
	public class NewEmployeeDashboardModel
	{
		public List<EmployeeDashboardProjects> projects { get; set; }
		public int NumOfProjects { get; set; }
		public int NumOfPending { get; set; }
		public int NumOfComplete { get; set; }
	}
	public class EmployeeDashboardMaterials
	{
		public int ID { get; set; }
		public string Description { get; set; }
		public string Description_Long { get; set; }
		public string UnitOfMeasurement { get; set; }
	}

	public class EmployeeListItem
	{
		public int ID { get; set; }
		public string Description { get; set; }
	}

	public class EmployeeAddMaterial
	{
		public IList<EmployeeListItem>? measurements { get; set; }
		public IList<EmployeeListItem>? categories { get; set; }
		public IList<EmployeeListItem>? measurement_type { get; set; }

		public int ID { get; set; }

		[StringLength(100, MinimumLength = 1)]
		[Required]
		[Display(Name = "Description")]
		public string Description { get; set; }

		[StringLength(255, MinimumLength = 1)]
		[Required]
		[Display(Name = "Description")]
		public string Description_Long { get; set; }

		[Required]
		[Display(Name = "Category")]
		public int CategoryID { get; set; }

		[Required]
		[Display(Name = "Unit of Measurement")]
		public int UnitOfMeasurement { get; set; }

		[Required]
		[Display(Name = "Measurement Type")]
		public int MeasurementType { get; set; }

		[Required]
		[Display(Name = "Measurement Value")]
		public double MeasurementValue { get; set; }
	}

	public class EmployeeInfoModel
	{
		public int ID { get; set; }
		public int user_id { get; set; }
		public string username { get; set; }
		
		[BindNever]
		[Display(Name = "Password")]
		public string? password { get; set; }


		[Required]
		[StringLength(50, MinimumLength = 1)]
		[Display(Name = "First Name")]
		public string first_name { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 1)]
		[Display(Name = "Middle Name")]
		public string middle_name { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 1)]
		[Display(Name = "Last Name")]
		public string last_name { get; set; }
		

		[Required]
		[StringLength(13, MinimumLength = 11)]
		[Display(Name = "Mobile Number")]
		public string contactNum { get; set; }

		[Required]
		[StringLength(62, MinimumLength = 1)]
		[Display(Name = "Email")]
		public string email { get; set; }


		[Required]
		[StringLength(95, MinimumLength = 1)]
		[Display(Name = "Address")]
		public string address { get; set; }

		[Display(Name = "Account Status")]
		public bool status { get; set; }
	}
}
