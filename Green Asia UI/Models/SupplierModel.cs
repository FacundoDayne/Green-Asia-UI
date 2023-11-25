using System.ComponentModel.DataAnnotations;

namespace Green_Asia_UI.Models
{
	public class SupplierMaterialsItem
	{
		public int ID { get; set; }
		public int Material_ID { get; set; }
		public string Description { get; set; }
		public string Description_Long { get; set; }
		public string UoM { get; set; }
		public string Quantity { get; set; }

		[Range(typeof(double), "0", "100000000", ErrorMessage = "Value must be 0 or above")]
		public double Price { get; set; }

		[Range(typeof(double), "0", "100000000", ErrorMessage = "Value must be 0 or above")]
		public double PreviousPrice { get; set; }
		public bool Availability { get; set; }
		public bool PreviousAvailability { get; set; }
	}

	public class SupplierInfo
	{


	}
}
