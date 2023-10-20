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

using Microsoft.Data.SqlClient;


namespace Green_Asia_UI.Controllers
{
	public class AdminController : Controller
	{
		private readonly string oldconnectionstring = "Data Source=localhost;port=3306;Initial Catalog=bom_mce_db;User Id=root;password=password123;";
		private readonly string connectionstring = @"Server=LAPTOP-HJA4M31O\SQLEXPRESS;Database=bom_mce_db;User Id=bom_debug;Password=password123;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";
		//private readonly string connectionstring = @"Server=68.71.129.120,1533;Database=bomgreen_db;User Id=bomgreen;Password=~Ni94tt39;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult CreateProject()
		{
			ProjectModel model = new ProjectModel();
			model.EngineerList = GetEngineers();
			model.BuildingList = GetBuildingTypes();
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateProject(ProjectModel model)
		{
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
			return RedirectToAction("ProjectView", new { id = id });
		}

		public IActionResult ProjectView(int id)
		{
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













		public IActionResult adminAddContractor()
		{
			return View();
		}

		public IActionResult adminAddProject()
		{
			ProjectModel model = new ProjectModel();
			model.EngineerList = GetEngineers();
			model.BuildingList = GetBuildingTypes();
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult adminAddProject(ProjectModel model)
		{
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

		public IActionResult adminAddSupplier()
		{
			return View();
		}

		public IActionResult adminContractorDash()
		{
			List<ContractorModel> model = new List<ContractorModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
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
					"WHERE d.mce_id IS NULL AND e.user_role = 2 " +
					"GROUP BY a.employee_info_firstname, a.employee_info_middlename, a.employee_info_lastname, " +
					"a.employee_info_city, a.employee_info_contactnum, a.employee_info_email, a.employee_info_id;"))
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
								Pending = Convert.ToInt32(sdr["pending"])
							});

						}
					}
				}
			}
			return View(model);
		}

		public IActionResult adminDashboard()
		{
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

		public IActionResult adminProjectDash()
		{
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
	}
}
