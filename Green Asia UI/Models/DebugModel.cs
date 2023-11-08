namespace Green_Asia_UI.Models
{
	public class DebugModel
	{
		public string filepath { get; set; }
		
		public List<SelectListItem> suppliers { get; set; }
	}

	public class SelectListItem
	{
		public string id { get; set; }
		public string desc { get; set; }
	}
}
