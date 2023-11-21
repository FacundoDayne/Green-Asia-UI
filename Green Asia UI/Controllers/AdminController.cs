using Green_Asia_UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
//using MySql.Data.MySqlClient;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.Metrics;
using Newtonsoft.Json;
using Microsoft.Build.ObjectModelRemoting;
//using MySqlX.XDevAPI;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.Runtime.Serialization;
//using Google.Protobuf.Collections;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Drawing.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;


namespace Green_Asia_UI.Controllers
{
	public class AdminController : Controller
	{
		private readonly string oldconnectionstring = "Data Source=localhost;port=3306;Initial Catalog=bom_mce_db;User Id=root;password=password123;";
		//private readonly string connectionstring = @"Server=LAPTOP-HJA4M31O\SQLEXPRESS;Database=bom_mce_db;User Id=bom_debug;Password=password123;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";
		private readonly string connectionstring = @"Server=68.71.129.120,1533;Database=bomgreen_db;User Id=bomgreen;Password=~Ni94tt39;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";
		public IActionResult Index()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			return View();
		}

		public IActionResult CreateProject()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			ProjectModel model = new ProjectModel();
			model.EngineerList = GetEngineers();
			model.BuildingList = GetBuildingTypes();
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]

		public async Task<IActionResult> CreateProject(ProjectModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			string pattern = @"^09\d{9}$";
			if (!Regex.IsMatch(model.ClientContact, pattern))
			{
				ModelState.AddModelError("ContactNumber", "This number is not valid. Use format \"09#########\".");

			}
			uint id = 0;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("INSERT INTO projects " +
					"(project_title,project_client_name,project_client_contact,project_address,project_city, " +
					"project_region,project_country,project_latitude,project_longtitude,project_date, " +
					"project_engineer_id,building_types_id,project_building_storeys,project_building_floorheight, " +
					"project_building_length,project_building_width) VALUES " +
					"(@project_title,@project_client_name,@project_client_contact,@project_address,@project_city,@project_region, " +
					"@project_country,@project_latitude,@project_longtitude,@project_date,@project_engineer_id,@building_types_id, " +
					"@project_building_storeys,@project_building_floorheight,@project_building_length,@project_building_width)" +
					"; SELECT SCOPE_IDENTITY() FROM projects;"))
				{
					command.Parameters.AddWithValue("@project_title", model.Title);
					command.Parameters.AddWithValue("@project_client_name", model.ClientName);
					command.Parameters.AddWithValue("@project_client_contact", model.ClientContact);
					command.Parameters.AddWithValue("@project_address", model.Address);
					command.Parameters.AddWithValue("@project_city", model.City);
					command.Parameters.AddWithValue("@project_region", model.Region);
					command.Parameters.AddWithValue("@project_country", model.Country);
					command.Parameters.AddWithValue("@project_latitude", model.Latitude);
					command.Parameters.AddWithValue("@project_longtitude", model.Longtitude);
					command.Parameters.AddWithValue("@project_date", model.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@project_engineer_id", model.EngineerID.ToString());
					command.Parameters.AddWithValue("@building_types_id", model.BuildingID.ToString());
					command.Parameters.AddWithValue("@project_building_storeys", model.NumberOfStoreys);
					command.Parameters.AddWithValue("@project_building_floorheight", model.FloorHeight);
					command.Parameters.AddWithValue("@project_building_length", model.BuildingLength);
					command.Parameters.AddWithValue("@project_building_width", model.BuildingWidth);
					command.Connection = conn;
					id = Convert.ToUInt32(command.ExecuteScalar());
				}
			}
			return RedirectToAction("adminProjectView", new { id = id });
		}

		public IActionResult ProjectView(int id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			ProjectViewModel model = new ProjectViewModel();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM projects a " +
					"INNER JOIN employee_info b ON a.project_engineer_id = b.employee_info_id " +
					"INNER JOIN building_types c ON a.building_types_id = c.building_types_id " +
					"WHERE a.project_id = @project_id"))
				{
					command.Parameters.AddWithValue("@project_id", id);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Title = sdr["project_title"].ToString();
							model.ClientName = sdr["project_client_name"].ToString();
							model.ClientContact = sdr["project_client_contact"].ToString();
							model.Address = sdr["project_address"].ToString();
							model.City = sdr["project_city"].ToString();
							model.Region = sdr["project_region"].ToString();
							model.Country = sdr["project_country"].ToString();
							model.Date = DateTime.Parse(sdr["project_date"].ToString());
							model.Engineer = sdr["employee_info_firstname"].ToString() + " " + sdr["employee_info_middlename"].ToString().ToUpper()[0] + ". " + sdr["employee_info_lastname"].ToString();
							model.BuildingType = sdr["description"].ToString();
							model.NumberOfStoreys = Convert.ToInt32(sdr["project_building_storeys"]);
							model.FloorHeight = Convert.ToDouble(sdr["project_building_floorheight"]);
							model.BuildingLength = Convert.ToDouble(sdr["project_building_length"]);
							model.BuildingWidth = Convert.ToDouble(sdr["project_building_width"]);
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT SCOPE_IDENTITY() FROM bom WHERE project_id = @project_id;"))
				{
					command.Parameters.AddWithValue("@project_id", id);
					command.Connection = conn;
					object result = command.ExecuteScalar();
					if (result == DBNull.Value)
					{
						model.bomCreated = false;
					}
					else
					{
						model.bomCreated = true;
					}
				}
			}
			return View(model);
		}

		public List<EngineerItem> GetEngineers()
		{
			List<EngineerItem> engineers = new List<EngineerItem>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM employee_info a INNER JOIN user_credentials b ON a.user_credentials_id = b.user_id WHERE b.user_role = 2;;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							engineers.Add(new EngineerItem()
							{
								ID = sdr["employee_info_id"].ToString(),
								Description = sdr["employee_info_firstname"].ToString() + " " + sdr["employee_info_middlename"].ToString().ToUpper()[0] + ". " + sdr["employee_info_lastname"].ToString()
							});
						}
					}
				}
			}
			return engineers;
		}

		public List<EngineerItem> GetBuildingTypes()
		{
			List<EngineerItem> engineers = new List<EngineerItem>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM building_types;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							engineers.Add(new EngineerItem()
							{
								ID = sdr["building_types_id"].ToString(),
								Description = sdr["description"].ToString()
							});
						}
					}
				}
			}
			return engineers;
		}

		public IActionResult Formulas()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			AdminFormulaDebug model = new AdminFormulaDebug();
			model.floorThickness = 0.127;
			model.wallThickness = 0.254;
			model.rebarPercentage = 0.10;
			model.rebarDiameter = 0.01;
			model.nailConstant = 20;
			model.hollowBlockLength = 0.2;
			model.hollowBlockWidth = 0.2;
			model.hollowBlockHeight = 0.2;
			model.hollowBlockVolume = model.hollowBlockHeight * model.hollowBlockLength * model.hollowBlockWidth;
			model.hollowBlockConstant = 12.5;
			model.supportBeamLength = 0.25;
			model.supportBeamWidth = 0.30;
			model.supportBeamArea = model.supportBeamLength * model.supportBeamWidth;
			model.supportBeamSpace = 2.92;
			model.supportBeamVolume = model.supportBeamArea * model.heightOfFloors;
			model.supportBeamsNeeded = model.sqmOfBuilding / model.supportBeamSpace;
			model.concreteRatioCement = 1;
			model.concreteRatioSand = 2;
			model.concreteRatioAggregate = 3;
			model.plywoodLength = 96;
			model.plywoodWidth = 48;
			model.plywoodArea = model.plywoodLength * model.plywoodWidth;
			model.plywoodSheetsPerSqm = (double)Math.Ceiling(10764 / model.plywoodArea);
			model.riserHeight = 0.178;
			model.threadDepth = 0.2794;
			model.numberOfSteps = (double)Math.Ceiling(model.heightOfFloors / model.riserHeight);
			return View();
		}














		public IActionResult adminAddProject()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			ProjectModel model = new ProjectModel();
			model.EngineerList = GetEngineers();
			model.BuildingList = GetBuildingTypes();
			model.Date = DateTime.Today;
			model.BuildingID = "1";
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]

		public ActionResult adminAddProject(ProjectModel model)
		{
			model.BuildingID = "1";
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			string pattern = @"^09\d{9}$";
			if (!Regex.IsMatch(model.ClientContact, pattern))
			{
				Debug.WriteLine("2");
				ModelState.AddModelError("ContactNumber", "This number is not valid. Use format \"09#########\".");

			}
			if (!model.EngineerID.All(Char.IsDigit))
			{
				Debug.WriteLine("3");
				ModelState.AddModelError("EngineerID", "Pick an engineer");
			}
			Debug.WriteLine("A" + Convert.ToInt32(model.EngineerID));
			if (Convert.ToInt32(model.EngineerID) < 0)
			{
				Debug.WriteLine("4");
				ModelState.AddModelError("EngineerID", "Pick an engineer");
			}
			if (!ModelState.IsValid)
			{
				Debug.WriteLine("1");
				foreach (var modelStateKey in ModelState.Keys)
				{
					var errors = ModelState[modelStateKey].Errors;
					foreach (var error in errors)
					{
						var errorMessage = error.ErrorMessage;
						// Do something with the error message
						Debug.WriteLine($"Property: {modelStateKey}, Error: {errorMessage}");
					}
				}
				return View(model);
			}
			uint id = 0;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("INSERT INTO projects " +
					"(project_title,project_client_name,project_client_contact,project_address,project_city, " +
					"project_region,project_country,project_latitude,project_longtitude,project_date, " +
					"project_engineer_id,building_types_id,project_building_storeys,project_building_floorheight, " +
					"project_building_length,project_building_width) VALUES " +
					"(@project_title,@project_client_name,@project_client_contact,@project_address,@project_city,@project_region, " +
					"@project_country,@project_latitude,@project_longtitude,@project_date,@project_engineer_id,@building_types_id, " +
					"@project_building_storeys,@project_building_floorheight,@project_building_length,@project_building_width)" +
					"; SELECT SCOPE_IDENTITY() FROM projects;"))
				{
					command.Parameters.AddWithValue("@project_title", model.Title);
					command.Parameters.AddWithValue("@project_client_name", model.ClientName);
					command.Parameters.AddWithValue("@project_client_contact", model.ClientContact);
					command.Parameters.AddWithValue("@project_address", model.Address);
					command.Parameters.AddWithValue("@project_city", model.City);
					command.Parameters.AddWithValue("@project_region", model.Region);
					command.Parameters.AddWithValue("@project_country", model.Country);
					command.Parameters.AddWithValue("@project_latitude", model.Latitude);
					command.Parameters.AddWithValue("@project_longtitude", model.Longtitude);
					command.Parameters.AddWithValue("@project_date", model.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@project_engineer_id", model.EngineerID.ToString());
					command.Parameters.AddWithValue("@building_types_id", model.BuildingID.ToString());
					command.Parameters.AddWithValue("@project_building_storeys", model.NumberOfStoreys);
					command.Parameters.AddWithValue("@project_building_floorheight", model.FloorHeight);
					command.Parameters.AddWithValue("@project_building_length", model.BuildingLength);
					command.Parameters.AddWithValue("@project_building_width", model.BuildingWidth);
					command.Connection = conn;
					id = Convert.ToUInt32(command.ExecuteScalar());
				}
			}
			return RedirectToAction("adminProjectView", new { id = id });
		}
		/*
		[HttpPost]
		[AllowAnonymous]
		
		public ActionResult adminAddProject(ProjectModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			string pattern = @"^09\d{9}$";
			if (!Regex.IsMatch(model.ClientContact, pattern))
			{
				ModelState.AddModelError("ContactNumber", "This number is not valid. Use format \"09#########\".");

			}
			uint id = 0;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("INSERT INTO projects " +
					"(project_title,project_client_name,project_client_contact,project_address,project_city, " +
					"project_region,project_country,project_latitude,project_longtitude,project_date, " +
					"project_engineer_id,building_types_id,project_building_storeys,project_building_floorheight, " +
					"project_building_length,project_building_width) VALUES " +
					"(@project_title,@project_client_name,@project_client_contact,@project_address,@project_city,@project_region, " +
					"@project_country,@project_latitude,@project_longtitude,@project_date,@project_engineer_id,@building_types_id, " +
					"@project_building_storeys,@project_building_floorheight,@project_building_length,@project_building_width)" +
					"; SELECT SCOPE_IDENTITY() FROM projects;"))
				{
					command.Parameters.AddWithValue("@project_title", model.Title);
					command.Parameters.AddWithValue("@project_client_name", model.ClientName);
					command.Parameters.AddWithValue("@project_client_contact", model.ClientContact);
					command.Parameters.AddWithValue("@project_address", model.Address);
					command.Parameters.AddWithValue("@project_city", model.City);
					command.Parameters.AddWithValue("@project_region", model.Region);
					command.Parameters.AddWithValue("@project_country", model.Country);
					command.Parameters.AddWithValue("@project_latitude", model.Latitude);
					command.Parameters.AddWithValue("@project_longtitude", model.Longtitude);
					command.Parameters.AddWithValue("@project_date", model.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@project_engineer_id", model.EngineerID.ToString());
					command.Parameters.AddWithValue("@building_types_id", model.BuildingID.ToString());
					command.Parameters.AddWithValue("@project_building_storeys", model.NumberOfStoreys);
					command.Parameters.AddWithValue("@project_building_floorheight", model.FloorHeight);
					command.Parameters.AddWithValue("@project_building_length", model.BuildingLength);
					command.Parameters.AddWithValue("@project_building_width", model.BuildingWidth);
					command.Connection = conn;
					id = Convert.ToUInt32(command.ExecuteScalar());
				}
			}
			return RedirectToAction("adminProjectView", new { id = id });
		}*/

		public IActionResult adminAddSupplier()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			return View();
		}

		public IActionResult adminContractorDash()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			List<ContractorModel> model = new List<ContractorModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				/*using (SqlCommand command = new SqlCommand(
					"SELECT " +
					"a.employee_info_id, " +
					"CONCAT(a.employee_info_firstname,' ',LEFT(a.employee_info_middlename,1),' ',a.employee_info_lastname) AS contractor_name, " +
					"COUNT(b.project_id) AS pending, " +
					"a.employee_info_city, a.employee_info_contactnum, a.employee_info_email " +
					"FROM employee_info a " +
					"LEFT JOIN projects b ON a.employee_info_id = b.project_engineer_id " +
					"LEFT JOIN bom c ON b.project_id = c.project_id " +
					"LEFT JOIN mce d ON c.bom_id = d.bom_id " +
					"INNER JOIN user_credentials e ON e.user_id = a.user_credentials_id " +
					"WHERE e.user_role = 2 " +
					"GROUP BY a.employee_info_firstname, a.employee_info_middlename, a.employee_info_lastname, " +
					"a.employee_info_city, a.employee_info_contactnum, a.employee_info_email, a.employee_info_id;"))
				{*/
				using (SqlCommand command = new SqlCommand(
					"select *," +
					"CONCAT(a.employee_info_firstname,' ',LEFT(a.employee_info_middlename,1),' ',a.employee_info_lastname) AS contractor_name " +
					" from employee_info a " +
					"INNER JOIN user_credentials e ON e.user_id = a.user_credentials_id " +
					"WHERE e.user_role = 2 ;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Add(new ContractorModel()
							{
								ID = Convert.ToInt32(sdr["employee_info_id"]),
								ContractorName = sdr["contractor_name"].ToString(),
								ContactNum = sdr["employee_info_contactnum"].ToString(),
								Email = sdr["employee_info_email"].ToString(),
								Address = sdr["employee_info_city"].ToString(),
								Pending = 0
							});

						}
					}
				}
			}
			return View(model);
		}
		
		public IActionResult adminDashboard()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			List<ClientDataModel> model = new List<ClientDataModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT TOP 5 a.project_id, a.project_date, a.project_client_name, a.project_title, " +
					"SUM(d.mce_subitem_quantity * e.supplier_material_price) AS price, f.description, " +
					"CASE " +
					"\tWHEN b.bom_id IS NULL THEN 0 " +
					"\tWHEN c.mce_id IS NULL THEN 1 " +
					"\tELSE 2 " +
					"END AS project_status " +
					", a.project_city, CONCAT(g.employee_info_firstname,' ',LEFT(g.employee_info_middlename,1),' ',g.employee_info_lastname) AS contractor_name " +
					"FROM projects a " +
					"LEFT JOIN bom b ON a.project_id = b.project_id " +
					"LEFT JOIN mce c ON b.bom_id = c.bom_id " +
					"LEFT JOIN mce_subitems d ON c.mce_id = d.mce_id " +
					"LEFT JOIN supplier_materials e ON d.supplier_material_id = e.supplier_material_id " +
					"INNER JOIN building_types f ON f.building_types_id = a.building_types_id " +
					"INNER JOIN employee_info g ON a.project_engineer_id = g.employee_info_id " +
					"GROUP BY a.project_id, a.project_date, a.project_client_name, a.project_title, f.description, a.project_city, b.bom_id, c.mce_id, " +
					"g.employee_info_firstname, g.employee_info_middlename, g.employee_info_lastname ORDER BY project_id DESC;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						double amount = 0;
						while (sdr.Read())
						{
							if (!Convert.IsDBNull(sdr["price"]))
							{
								amount = Convert.ToDouble(sdr["price"]) / 100;
							}
							model.Add(new ClientDataModel()
							{
								ID = Convert.ToInt32(sdr["project_id"]),
								ClientName = sdr["project_client_name"].ToString(),
								date = Convert.ToDateTime(sdr["project_date"].ToString()),
								Description = sdr["project_title"].ToString(),
								Amount = amount,
								Category = sdr["description"].ToString(),
								Address = sdr["project_city"].ToString(),
								ContractorName = sdr["contractor_name"].ToString(),
								Status = Convert.ToInt32(sdr["project_status"])
							});

						}
					}
				}
			}
			return View(model);
		}

		public IActionResult adminProjectDash()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			List<ClientDataModel> model = new List<ClientDataModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT a.project_id, a.project_date, a.project_client_name, a.project_title, " +
					"SUM(d.mce_subitem_quantity * e.supplier_material_price) AS price, f.description, " +
					"CASE " +
					"\tWHEN b.bom_id IS NULL THEN 0 " +
					"\tWHEN c.mce_id IS NULL THEN 1 " +
					"\tELSE 2 " +
					"END AS project_status " +
					", a.project_city, CONCAT(g.employee_info_firstname,' ',LEFT(g.employee_info_middlename,1),' ',g.employee_info_lastname) AS contractor_name " +
					"FROM projects a " +
					"LEFT JOIN bom b ON a.project_id = b.project_id " +
					"LEFT JOIN mce c ON b.bom_id = c.bom_id " +
					"LEFT JOIN mce_subitems d ON c.mce_id = d.mce_id " +
					"LEFT JOIN supplier_materials e ON d.supplier_material_id = e.supplier_material_id " +
					"INNER JOIN building_types f ON f.building_types_id = a.building_types_id " +
					"INNER JOIN employee_info g ON a.project_engineer_id = g.employee_info_id " +
					"GROUP BY a.project_id, a.project_date, a.project_client_name, a.project_title, f.description, a.project_city, b.bom_id, c.mce_id, " +
					"g.employee_info_firstname, g.employee_info_middlename, g.employee_info_lastname;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						double amount = 0;
						while (sdr.Read())
						{
							if (!Convert.IsDBNull(sdr["price"]))
							{
								amount = Convert.ToDouble(sdr["price"]) / 100;
							}
							model.Add(new ClientDataModel()
							{
								ID = Convert.ToInt32(sdr["project_id"]),
								ClientName = sdr["project_client_name"].ToString(),
								date = Convert.ToDateTime(sdr["project_date"].ToString()),
								Description = sdr["project_title"].ToString(),
								Amount = amount,
								Category = sdr["description"].ToString(),
								Address = sdr["project_city"].ToString(),
								ContractorName = sdr["contractor_name"].ToString(),
								Status = Convert.ToInt32(sdr["project_status"])
							});

						}
					}
				}
			}
			return View(model);
		}

		public IActionResult adminSupplierDash()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			List<AdminSupplierModel> model = new List<AdminSupplierModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT * FROM supplier_info;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Add(new AdminSupplierModel()
							{
								ID = Convert.ToInt32(sdr["supplier_id"]),
								Desc = sdr["supplier_desc"].ToString(),
								ContactNum = sdr["supplier_contact_number"].ToString(),
								ContactName = sdr["supplier_contact_name"].ToString(),
								Address = sdr["supplier_city"].ToString()
							});

						}
					}
				}
			}
			return View(model);
		}

		public IActionResult adminProjectView(int id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			ProjectViewModel model = new ProjectViewModel();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM projects a " +
					"INNER JOIN employee_info b ON a.project_engineer_id = b.employee_info_id " +
					"INNER JOIN building_types c ON a.building_types_id = c.building_types_id " +
					"WHERE a.project_id = @project_id"))
				{
					command.Parameters.AddWithValue("@project_id", id);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Title = sdr["project_title"].ToString();
							model.ClientName = sdr["project_client_name"].ToString();
							model.ClientContact = sdr["project_client_contact"].ToString();
							model.Address = sdr["project_address"].ToString();
							model.City = sdr["project_city"].ToString();
							model.Region = sdr["project_region"].ToString();
							model.Country = sdr["project_country"].ToString();
							model.Date = DateTime.Parse(sdr["project_date"].ToString());
							model.Engineer = sdr["employee_info_firstname"].ToString() + " " + sdr["employee_info_middlename"].ToString().ToUpper()[0] + ". " + sdr["employee_info_lastname"].ToString();
							model.BuildingType = sdr["description"].ToString();
							model.NumberOfStoreys = Convert.ToInt32(sdr["project_building_storeys"]);
							model.FloorHeight = Convert.ToDouble(sdr["project_building_floorheight"]);
							model.BuildingLength = Convert.ToDouble(sdr["project_building_length"]);
							model.BuildingWidth = Convert.ToDouble(sdr["project_building_width"]);
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT SCOPE_IDENTITY() FROM bom WHERE project_id = @project_id;"))
				{
					command.Parameters.AddWithValue("@project_id", id);
					command.Connection = conn;
					object result = command.ExecuteScalar();
					if (result == DBNull.Value)
					{
						model.bomCreated = false;
					}
					else
					{
						model.bomCreated = true;
					}
				}
			}
			return View(model);
		}



		public ActionResult adminAddContractor()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			AddEmployeeModel xmodel = new AddEmployeeModel();
			xmodel.Role = "2";

			xmodel.roles = new List<RoleList>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM user_roles;"))
				{
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							//xmodel.measurements.Add(new SelectListItem
							xmodel.roles.Add(new RoleList
							{
								id = sdr["role_id"].ToString(),
								name = sdr["role_name"].ToString()
							}
							);
						}
					}
					conn.Close();
				}
			}
			return View(xmodel);
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult adminAddContractor(AddEmployeeModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			if (!ModelState.IsValid)
			{
				model.roles = GetUserRolesFromDB();
				return View(model);
			}
			bool username_exists = false;

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM user_credentials WHERE username = @username"))
				{
					conn.Open();
					command.Connection = conn;
					command.Parameters.AddWithValue("@username", model.Username);
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						if (sdr.Read())
						{
							username_exists = true;
						}
					}
					conn.Close();
				}
			}

			if (username_exists)
			{
				model.roles = GetUserRolesFromDB();
				return View(model);
			}

			bool error = false;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlTransaction transaction = conn.BeginTransaction())
				{
					try
					{
						int info_id = 0;

						using (SqlCommand command = new SqlCommand("INSERT INTO user_credentials (username,user_password,user_role,user_status) " +
							" VALUES (@username, @password, @user_role, @user_status); SELECT SCOPE_IDENTITY() FROM user_credentials;"))
						{
							command.Connection = conn;
							command.Transaction = transaction;

							command.Parameters.AddWithValue("@username", model.Username);
							command.Parameters.AddWithValue("@password", model.Password);
							command.Parameters.AddWithValue("@user_role", Convert.ToInt32(model.Role));
							command.Parameters.AddWithValue("@user_status", 1);

							info_id = Convert.ToInt32(command.ExecuteScalar());
						}

						using (SqlCommand command = new SqlCommand("INSERT INTO employee_info (user_credentials_id, employee_info_firstname, employee_info_middlename, employee_info_lastname, employee_info_contactnum, employee_info_email, employee_info_address, employee_info_city, employee_info_status) " +
							" VALUES (@user_credentials_id, @FirstName, @MiddleName, @LastName, @Contact, @Email, @Address, @City, @Status); "))
						{
							command.Connection = conn;
							command.Transaction = transaction;

							command.Parameters.AddWithValue("@user_credentials_id", info_id);
							command.Parameters.AddWithValue("@FirstName", model.FirstName);
							command.Parameters.AddWithValue("@MiddleName", model.MiddleName);
							command.Parameters.AddWithValue("@LastName", model.LastName);
							command.Parameters.AddWithValue("@Contact", model.Contact);
							command.Parameters.AddWithValue("@Email", model.Email);
							command.Parameters.AddWithValue("@Address", model.Address);
							command.Parameters.AddWithValue("@City", model.City);
							command.Parameters.AddWithValue("@Status", 1);

							command.ExecuteNonQuery();
						}

						transaction.Commit();
					}
					catch (SqlException e)
					{
						error = true;
						Debug.WriteLine(e.Message);
						transaction.Rollback();
					}
				}
				conn.Close();
			}
			if (error)
			{
				model.roles = GetUserRolesFromDB();
				return View(model);
			}

			return RedirectToAction("adminContractorDash", "Admin");
		}


		public IActionResult adminEditContractor(int? id)
		{

			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			EmployeeInfoModel model = new EmployeeInfoModel();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM employee_info a " +
					"INNER JOIN user_credentials b " +
					"ON a.user_credentials_id = b.user_id " +
					"WHERE employee_info_id = @id;"))
				{
					command.Parameters.AddWithValue("@id", Convert.ToInt32(id));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.ID = Convert.ToInt32(id);
							model.user_id = Convert.ToInt32(sdr["user_credentials_id"]);
							model.username = sdr["username"].ToString();
							model.password = sdr["user_password"].ToString();
							model.first_name = sdr["employee_info_firstname"].ToString();
							model.middle_name = sdr["employee_info_middlename"].ToString();
							model.last_name = sdr["employee_info_lastname"].ToString();
							model.contactNum = sdr["employee_info_contactnum"].ToString();
							model.email = sdr["employee_info_email"].ToString();
							model.address = sdr["employee_info_address"].ToString();
							model.status = Convert.ToBoolean(sdr["employee_info_status"]);
						}
					}
				}
			}
			return View(model);
		}


		[HttpPost]
		[AllowAnonymous]
		public IActionResult adminEditContractor(EmployeeInfoModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			if (!ModelState.IsValid)
			{
				Debug.WriteLine("invalid");
				Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss"));

				foreach (var key in ModelState.Keys)
				{
					var errors = ModelState[key].Errors;
					foreach (var error in errors)
					{
						// Log or display the error message
						var errorMessage = error.ErrorMessage;
						Debug.WriteLine(": " + errorMessage);
						// You can also access error.Exception for exceptions if applicable
					}
				}
				return View(model);
			}

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				Debug.WriteLine("?");
				using (SqlCommand command = new SqlCommand("UPDATE employee_info SET " +
					"employee_info_firstname = @employee_info_firstname," +
					"employee_info_middlename = @employee_info_middlename," +
					"employee_info_lastname = @employee_info_lastname," +
					"employee_info_contactnum = @employee_info_contactnum," +
					"employee_info_email = @employee_info_email," +
					"employee_info_address = @employee_info_address, " +
					"employee_info_status = @employee_info_status " +
					"WHERE employee_info_id = @employee_info_id;"))
				{
					command.Connection = conn;

					command.Parameters.AddWithValue("@employee_info_id", model.ID);
					command.Parameters.AddWithValue("@employee_info_firstname", model.first_name);
					command.Parameters.AddWithValue("@employee_info_middlename", model.middle_name);
					command.Parameters.AddWithValue("@employee_info_lastname", model.last_name);
					command.Parameters.AddWithValue("@employee_info_contactnum", model.contactNum);
					command.Parameters.AddWithValue("@employee_info_email", model.email);
					command.Parameters.AddWithValue("@employee_info_address", model.address);
					command.Parameters.AddWithValue("@employee_info_status", model.status);

					command.ExecuteNonQuery();
					Debug.WriteLine("?");
				}
				Debug.WriteLine("?");
				using (SqlCommand command = new SqlCommand(
					"UPDATE user_credentials SET " +
					"user_password = @user_password, " +
					"user_status = @user_status " +
					"WHERE user_id = @user_id;"))
				{
					command.Connection = conn;

					command.Parameters.AddWithValue("@user_id", model.user_id);
					command.Parameters.AddWithValue("@user_password", model.password);
					command.Parameters.AddWithValue("@user_status", model.status);

					command.ExecuteNonQuery();
					Debug.WriteLine("?");
				}
				conn.Close();
			}
			Debug.WriteLine("?");
			return RedirectToAction("adminContractorDash", "Admin");

		}

		public List<RoleList> GetUserRolesFromDB()
		{
			List<RoleList> roles = new List<RoleList>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM user_roles WHERE role_id != 3;"))
				{
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							roles.Add(new RoleList
							{
								id = sdr["role_id"].ToString(),
								name = sdr["role_name"].ToString()
							}
							);
						}
					}
					conn.Close();
				}
			}
			return roles;
		}

		// AJAX

		public ActionResult AJAXGetContractorNumOfProjects (string selectedValue)
		{
			int numOfProjects = 0;
			try
			{
				using (SqlConnection conn = new SqlConnection(connectionstring))
				{
					conn.Open();
					using (SqlCommand command = new SqlCommand("SELECT COUNT(a.project_id) FROM projects a " +
						"LEFT JOIN bom b ON a.project_id = b.project_id " +
						"LEFT JOIN mce c ON b.bom_id = c.bom_id " +
						"WHERE a.project_engineer_id = @id " +
						"AND (c.mce_id IS NULL OR b.bom_id IS NULL);"))
					{
						command.Parameters.AddWithValue("@id", Convert.ToInt32(selectedValue));
						command.Connection = conn;
						numOfProjects = Convert.ToInt32(command.ExecuteScalar());
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
			}
			catch (InvalidCastException e)
			{
				Debug.WriteLine(e.Message);
			}
			Debug.WriteLine(selectedValue + ": " + numOfProjects);
			return Json(numOfProjects.ToString());
		}
	}
}
