using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Green_Asia_UI.Models
{
	public class ClientDataModel
	{
		public int ID { get; set; }
		public int Status { get; set; }
		public DateTime date { get; set; }
		public string ClientName { get; set; }
		public string Description { get; set; }
		public double Amount { get; set; }
		public string Category { get; set; }
		public string Address { get; set; }
		public string ContractorName { get; set; }
	}

	public class ContractorModel
	{
		public int ID { get; set; }
		public string ContractorName { get; set; }
		public string ContactNum { get; set; }
		public string Email { get; set; }
		public string Address { get; set; }
		public int Pending { get; set; }
	}

	public class AdminSupplierModel
	{
		public int ID { get; set; }
		public string Desc { get; set; }
		public string ContactName { get; set; }
		public string ContactNum { get; set; }
		public string Address { get; set; }
	}

	public class AdminDashboardModel
	{

	}
}
