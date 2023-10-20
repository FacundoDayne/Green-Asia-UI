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
}
