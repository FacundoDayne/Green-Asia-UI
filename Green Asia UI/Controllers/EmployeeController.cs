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

using Microsoft.Data.SqlClient;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;


using System.Drawing;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Layout.Borders;
using Org.BouncyCastle.Math.EC.Multiplier;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Green_Asia_UI.Controllers
{
	public class EmployeeController : Controller
	{
		private readonly IWebHostEnvironment _hostingEnvironment;

		private readonly string oldconnectionstring = "Data Source=localhost;port=3306;Initial Catalog=bom_mce_db;User Id=root;password=password123;";
		//private readonly string connectionstring = @"Server=LAPTOP-HJA4M31O\SQLEXPRESS;Database=bom_mce_db;User Id=bom_debug;Password=password123;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";
		private readonly string connectionstring = @"Server=68.71.129.120,1533;Database=bomgreen_db;User Id=bomgreen;Password=~Ni94tt39;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";

		public EmployeeController(IWebHostEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}

		public IActionResult Index()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			EmployeeDashboardModel model = new EmployeeDashboardModel();
			model.projects = GetNewProjects(HttpContext.Session.GetInt32("EmployeeID"));
			return View(model);
		}

		public List<EmployeeNewProject> GetNewProjects(int? id)
		{
			List<EmployeeNewProject> projects = new List<EmployeeNewProject>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM projects a " +
					"LEFT JOIN bom b " +
					"ON a.project_id = b.project_id " +
					"WHERE b.project_id IS NULL " +
					"AND project_engineer_id = @project_engineer_id;"))
				{
					command.Parameters.AddWithValue("@project_engineer_id", (int)id);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							projects.Add(new EmployeeNewProject()
							{ 
								ID = Convert.ToInt32(sdr["project_id"]),
								Title = sdr["project_title"].ToString(),
								ClientName = sdr["project_client_name"].ToString(),
								Date = DateTime.Parse(sdr["project_date"].ToString())
							});
						}
					}
				}
			}
			return projects;
		}

		public List<TemplateListItem> GetTemplateList(int page, int pageSize)
		{
			List<TemplateListItem> model = new List<TemplateListItem>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM employee_templates WHERE employee_id = @employee_id " +
					"ORDER BY template_id " +
					"OFFSET (@pagesize * (@pagenumber - 1)) ROWS " +
					"FETCH NEXT @pagesize ROWS ONLY;"))
				{
					command.Parameters.AddWithValue("@pagesize", pageSize);
					command.Parameters.AddWithValue("@pagenumber", page);
					command.Parameters.AddWithValue("@employee_id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Add(new TemplateListItem()
							{
								ID = Convert.ToInt32(sdr["template_id"]),
								Descritpion = sdr["template_description"].ToString(),
								Long_Description = sdr["template_descritpion_long"].ToString()
							});
						}
					}
				}
			}
			return model;
		}

		public IActionResult Templates(int page = 1, int pageSize = 10)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}

			List<TemplateListItem> model = GetTemplateList(page, pageSize);
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT COUNT(template_id)  FROM employee_templates WHERE employee_id = @employee_id;"))
				{
					command.Parameters.AddWithValue("@employee_id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					double numOfRecords = Convert.ToDouble(command.ExecuteScalar());
					ViewBag.Pages = Convert.ToInt32(Math.Ceiling(Math.Ceiling(numOfRecords) / pageSize));
					ViewBag.Page = page;
					ViewBag.PageSize = pageSize;
				}
			}
			return View(model);
		}
		//AJAX
		public ActionResult getTemplatePageData(int page = 1, int pageSize = 10)
		{
			List<TemplateListItem> model = GetTemplateList(page, pageSize);
			return Json(model);

		}

		//AJAX
		public ActionResult searchTemplate(string search = "")
		{
			List<TemplateListItem> model = new List<TemplateListItem>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM employee_templates WHERE employee_id = @employee_id " +
					"AND template_description LIKE @title ORDER BY template_id;"))
				{
					command.Parameters.AddWithValue("@title", "%" + search + "%");
					command.Parameters.AddWithValue("@employee_id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Add(new TemplateListItem()
							{
								ID = Convert.ToInt32(sdr["template_id"]),
								Descritpion = sdr["template_description"].ToString(),
								Long_Description = sdr["template_descritpion_long"].ToString()
							});
						}
					}
				}
			}
			return Json(model);
		}

		public IActionResult NewTemplate()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			NewTemplateModel model = new NewTemplateModel();
			model.materials = new List<TemplateMaterial>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials"))
				{
					command.Connection = conn;

					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.materials.Add(new TemplateMaterial()
							{
								ID = Convert.ToInt32(sdr["material_id"]),
								Name = sdr["material_desc"].ToString(),
								Checked = true
							});
						}
					}
				}
			}
			return View(model);
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> NewTemplate(NewTemplateModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			model.NumberOfStoreys = 1;
			model.FloorHeight = 1;
			model.BuildingWidth = 1;
			model.BuildingLength = 1;
			if (!ModelState.IsValid)
			{

				return View(model);
			}
			int id = 0, id2= 0;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlTransaction transaction = conn.BeginTransaction())
				{
					using (SqlCommand command = new SqlCommand("INSERT INTO template_formula_constants " +
						"(floor_thickness,wall_thickness,rebar_thickness,nail_interval,hollow_block_constant,support_beam_length, " +
						"support_beam_width,support_beam_interval,concrete_ratio_cement,concrete_ratio_sand,concrete_ratio_aggregate, " +
						"plywood_length,plywood_width,riser_height,thread_depth,stairs_width,wastage,provisions) VALUES( " +
						"@floor_thickness,@wall_thickness,@rebar_thickness,@nail_interval,@hollow_block_constant,@support_beam_length, " +
						"@support_beam_width,@support_beam_interval,@concrete_ratio_cement,@concrete_ratio_sand,@concrete_ratio_aggregate, " +
						"@plywood_length,@plywood_width,@riser_height,@thread_depth,@stairs_width,@wastage,@provisions); " +
						"SELECT SCOPE_IDENTITY() FROM template_formula_constants;"))
					{
						command.Parameters.AddWithValue("@floor_thickness",				model.floorThickness		);
						command.Parameters.AddWithValue("@wall_thickness",				model.wallThickness			);
						command.Parameters.AddWithValue("@rebar_thickness",				model.rebarDiameter			);
						command.Parameters.AddWithValue("@nail_interval",				model.nailConstant			);
						command.Parameters.AddWithValue("@hollow_block_constant",		model.hollowBlockConstant	);
						command.Parameters.AddWithValue("@support_beam_length",			model.supportBeamLength		);
						command.Parameters.AddWithValue("@support_beam_width",			model.supportBeamWidth		);
						command.Parameters.AddWithValue("@support_beam_interval",		model.supportBeamSpace		);
						command.Parameters.AddWithValue("@concrete_ratio_cement",		model.concreteRatioCement	);
						command.Parameters.AddWithValue("@concrete_ratio_sand",			model.concreteRatioSand		);
						command.Parameters.AddWithValue("@concrete_ratio_aggregate",	model.concreteRatioAggregate);
						command.Parameters.AddWithValue("@plywood_length",				model.plywoodLength			);
						command.Parameters.AddWithValue("@plywood_width",				model.plywoodWidth			);
						command.Parameters.AddWithValue("@riser_height",				model.riserHeight			);
						command.Parameters.AddWithValue("@thread_depth",				model.threadDepth			);
						command.Parameters.AddWithValue("@stairs_width",				model.stairsWidth);
						command.Parameters.AddWithValue("@wastage",						1 + (model.wasteage / 100)	);
						command.Parameters.AddWithValue("@provisions",					1 + (model.provisions / 100));
						command.Connection = conn;
						command.Transaction = transaction;
						id = Convert.ToInt32(command.ExecuteScalar());
					}
					using (SqlCommand command = new SqlCommand("INSERT INTO employee_templates " +
						"(employee_id, formula_constants_id, template_description, template_descritpion_long, " +
						"template_building_length,template_building_width,template_building_floor_height, template_building_storeys) VALUES ( " +
						"@employee_id, @formula_constants_id, @template_description, @template_descritpion_long, " +
						"@template_building_length,@template_building_width,@template_building_floor_height, @template_building_storeys); " +
						"SELECT SCOPE_IDENTITY() FROM employee_templates;"))
					{
						command.Parameters.AddWithValue("@employee_id", (int)HttpContext.Session.GetInt32("EmployeeID"));
						command.Parameters.AddWithValue("@formula_constants_id", id);
						command.Parameters.AddWithValue("@template_description", model.Descritpion);
						command.Parameters.AddWithValue("@template_descritpion_long", model.Long_Description);
						command.Parameters.AddWithValue("@template_building_length", model.BuildingLength);
						command.Parameters.AddWithValue("@template_building_width", model.BuildingWidth);
						command.Parameters.AddWithValue("@template_building_floor_height", model.FloorHeight);
						command.Parameters.AddWithValue("@template_building_storeys", model.NumberOfStoreys);
						command.Connection = conn;
						command.Transaction = transaction;
						id2 = Convert.ToInt32(command.ExecuteScalar());
					}
					using (SqlCommand command = new SqlCommand("INSERT INTO template_used_materials " +
						"(template_id, material_id, is_used) VALUES ( " +
						"@template_id, @material_id, @is_used);"))
					{
						SqlParameter template_id = new SqlParameter("@template_id", SqlDbType.Int);
						SqlParameter material_id = new SqlParameter("@material_id", SqlDbType.Int);
						SqlParameter is_used = new SqlParameter("@is_used", SqlDbType.Bit);

						command.Parameters.Add(template_id);
						command.Parameters.Add(material_id);
						command.Parameters.Add(is_used);
						command.Connection = conn;
						command.Transaction = transaction;
						Debug.WriteLine($"tem: {id2}");
						Debug.WriteLine($"{model.materials.Count}");
						foreach (TemplateMaterial x in model.materials)
						{
							Debug.WriteLine($"{x.Name} || {x.Checked}");
							template_id.Value = id2;
							material_id.Value = x.ID;
							is_used.Value = true;
							command.ExecuteNonQuery();
						}
					}
					transaction.Commit();
				}
				
			}
			return RedirectToAction("Templates");
		}

		public IActionResult TemplatesEdit(int? id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			EditTemplateModel model = new EditTemplateModel();
			model.ID = (int)id;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlTransaction transaction = conn.BeginTransaction())
				{
					using (SqlCommand command = new SqlCommand("SELECT * FROM employee_templates a " +
					"INNER JOIN template_formula_constants b ON a.formula_constants_id = b.formula_constants_id " +
					"WHERE template_id = @template_id;"))
					{
						command.Parameters.AddWithValue("@template_id", (int)id);
						command.Connection = conn;
						command.Transaction = transaction;
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							while (sdr.Read())
							{
								model = new EditTemplateModel()
								{
									FormulaID = Convert.ToInt32(sdr["formula_constants_id"]),
									Descritpion = sdr["template_description"].ToString(),
									Long_Description = sdr["template_descritpion_long"].ToString(),
									BuildingLength = Convert.ToDouble(sdr["template_building_length"]),
									BuildingWidth = Convert.ToDouble(sdr["template_building_width"]),
									FloorHeight = Convert.ToDouble(sdr["template_building_floor_height"]),
									NumberOfStoreys = Convert.ToInt32(sdr["template_building_storeys"]),
									floorThickness = Convert.ToDouble(sdr["floor_thickness"]),
									wallThickness = Convert.ToDouble(sdr["wall_thickness"]),
									rebarDiameter = Convert.ToDouble(sdr["rebar_thickness"]),
									nailConstant = Convert.ToDouble(sdr["nail_interval"]),
									hollowBlockConstant = Convert.ToDouble(sdr["hollow_block_constant"]),
									supportBeamLength = Convert.ToDouble(sdr["support_beam_length"]),
									supportBeamWidth = Convert.ToDouble(sdr["support_beam_width"]),
									supportBeamSpace = Convert.ToDouble(sdr["support_beam_interval"]),
									concreteRatioCement = Convert.ToDouble(sdr["concrete_ratio_cement"]),
									concreteRatioSand = Convert.ToDouble(sdr["concrete_ratio_sand"]),
									concreteRatioAggregate = Convert.ToDouble(sdr["concrete_ratio_aggregate"]),
									plywoodLength = Convert.ToDouble(sdr["plywood_length"]),
									plywoodWidth = Convert.ToDouble(sdr["plywood_width"]),
									riserHeight = Convert.ToDouble(sdr["riser_height"]),
									threadDepth = Convert.ToDouble(sdr["thread_depth"]),
									wasteage = Math.Round((Convert.ToDouble(sdr["wastage"]) -1) * 100),
									provisions = Math.Round((Convert.ToDouble(sdr["provisions"]) -1) * 100)
								};
							}
						}
					}
					using (SqlCommand command = new SqlCommand("SELECT * FROM template_used_materials a " +
						"INNER JOIN materials b ON a.material_id = b.material_id " +
						"WHERE a.template_id = @template_id;"))
					{
						command.Parameters.AddWithValue("@template_id", (int)id);
						command.Connection = conn;
						command.Transaction = transaction;
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							model.materials = new List<TemplateMaterial>();
							while (sdr.Read())
							{
								model.materials.Add(new TemplateMaterial()
								{
									ID = Convert.ToInt32(sdr["material_id"]),
									Name = sdr["material_desc"].ToString(),
									Checked = Convert.ToBoolean(sdr["is_used"]),
									ID_Template = Convert.ToInt32(sdr["template_used_materials_id"])
								});
							}
						}
					}
					transaction.Rollback();
				}
			}

			return View(model);
		}


		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public IActionResult TemplatesEdit(EditTemplateModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}

			model.NumberOfStoreys = 1;
			model.FloorHeight = 1;
			model.BuildingWidth = 1;
			model.BuildingLength = 1;
			if (!ModelState.IsValid)
			{

				return View(model);
			}
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlTransaction transaction = conn.BeginTransaction())
				{
					using (SqlCommand command = new SqlCommand("UPDATE employee_templates SET " +
					"template_description = @template_description, template_descritpion_long = @template_descritpion_long, " +
					"template_building_length = @template_building_length, template_building_width = @template_building_width, " +
					"template_building_floor_height = @template_building_floor_height, template_building_storeys = @template_building_storeys " +
					"WHERE template_id = @template_id;" +
					"UPDATE template_formula_constants SET " +
					"floor_thickness = @floor_thickness, wall_thickness = @wall_thickness, rebar_thickness = @rebar_thickness," +
					"nail_interval = @nail_interval, hollow_block_constant = @hollow_block_constant, support_beam_length = @support_beam_length, " +
					"support_beam_width = @support_beam_width, support_beam_interval = @support_beam_interval, concrete_ratio_cement = @concrete_ratio_cement," +
					"concrete_ratio_sand = @concrete_ratio_sand, concrete_ratio_aggregate = @concrete_ratio_aggregate, plywood_length = @plywood_length, " +
					"plywood_width = @plywood_width, riser_height = @riser_height, thread_depth = @thread_depth, stairs_width = @stairs_width, wastage = @wastage, provisions = @provisions " +
					"WHERE formula_constants_id = @formula_constants_id;"))
					{
						command.Connection = conn;
						command.Transaction = transaction;

						command.Parameters.AddWithValue("@template_id", model.ID);
						command.Parameters.AddWithValue("@floor_thickness", model.floorThickness);
						command.Parameters.AddWithValue("@wall_thickness", model.wallThickness);
						command.Parameters.AddWithValue("@rebar_thickness", model.rebarDiameter);
						command.Parameters.AddWithValue("@nail_interval", model.nailConstant);
						command.Parameters.AddWithValue("@hollow_block_constant", model.hollowBlockConstant);
						command.Parameters.AddWithValue("@support_beam_length", model.supportBeamLength);
						command.Parameters.AddWithValue("@support_beam_width", model.supportBeamWidth);
						command.Parameters.AddWithValue("@support_beam_interval", model.supportBeamSpace);
						command.Parameters.AddWithValue("@concrete_ratio_cement", model.concreteRatioCement);
						command.Parameters.AddWithValue("@concrete_ratio_sand", model.concreteRatioSand);
						command.Parameters.AddWithValue("@concrete_ratio_aggregate", model.concreteRatioAggregate);
						command.Parameters.AddWithValue("@plywood_length", model.plywoodLength);
						command.Parameters.AddWithValue("@plywood_width", model.plywoodWidth);
						command.Parameters.AddWithValue("@riser_height", model.riserHeight);
						command.Parameters.AddWithValue("@thread_depth", model.threadDepth);
						command.Parameters.AddWithValue("@stairs_width", model.stairsWidth);
						command.Parameters.AddWithValue("@wastage", 1 + (model.wasteage / 100));
						command.Parameters.AddWithValue("@provisions", 1 + (model.provisions / 100));
						command.Parameters.AddWithValue("@formula_constants_id", model.FormulaID);
						command.Parameters.AddWithValue("@template_description", model.Descritpion);
						command.Parameters.AddWithValue("@template_descritpion_long", model.Long_Description);
						command.Parameters.AddWithValue("@template_building_length", model.BuildingLength);
						command.Parameters.AddWithValue("@template_building_width", model.BuildingWidth);
						command.Parameters.AddWithValue("@template_building_floor_height", model.FloorHeight);
						command.Parameters.AddWithValue("@template_building_storeys", model.NumberOfStoreys);
						command.ExecuteNonQuery();
					}
					using (SqlCommand command = new SqlCommand("UPDATE template_used_materials SET is_used = @is_used " +
						"WHERE template_used_materials_id = @template_used_materials_id;"))
					{
						command.Connection = conn;
						command.Transaction = transaction;

						SqlParameter is_used = new SqlParameter("@is_used", SqlDbType.Bit);
						SqlParameter template_used_materials_id = new SqlParameter("@template_used_materials_id", SqlDbType.Int);

						command.Parameters.Add(is_used);
						command.Parameters.Add(template_used_materials_id);

						foreach (TemplateMaterial x in model.materials)
						{
							is_used.Value = true;
							template_used_materials_id.Value = x.ID_Template;
							command.ExecuteNonQuery();
						}
					}
					transaction.Commit();
				}
			}

			return RedirectToAction("Templates");
		}


		public ActionResult BOMTemplates()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			List<Employee_BOMTemplate> model = new List<Employee_BOMTemplate>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM employee_templates WHERE employee_id = @id"))
				{
					command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Add(new Employee_BOMTemplate()
							{
								ID = Convert.ToInt32(sdr["template_id"]),
								Description = sdr["template_description"].ToString(),
								Description_Long = sdr["template_descritpion_long"].ToString(),
								Materials = new List<BOMMaterialPickItem>()
							});
						}
					}
				}
			}
			return View(model);
		}

		public ActionResult BOMGenerate(int? template_id = -1, int? id = -1)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			
			EmployeeBOMModel model = new EmployeeBOMModel();
			model.templates = new List<Employee_BOM_Template_List>();
			model.ProjectID = (int)id;
			string location = model.Latitude + "," + model.Longtitude;

			if (TempData["BOMInfo"] == null && id != -1)
			{
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
								model.BuildingType = sdr["description"].ToString();
								model.NumberOfStoreys = Convert.ToInt32(sdr["project_building_storeys"]);
								model.FloorHeight = Convert.ToDouble(sdr["project_building_floorheight"]);
								model.BuildingLength = Convert.ToDouble(sdr["project_building_length"]);
								model.BuildingWidth = Convert.ToDouble(sdr["project_building_width"]);
								model.Longtitude = sdr["project_longtitude"].ToString();
								model.Latitude = sdr["project_latitude"].ToString();
							}
						}
					}
					using (SqlCommand command = new SqlCommand("SELECT * FROM employee_templates WHERE employee_id = @employee_id;"))
					{
						command.Parameters.AddWithValue("@employee_id", HttpContext.Session.GetInt32("EmployeeID"));
						command.Connection = conn;
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							while (sdr.Read())
							{
								model.templates.Add(new Employee_BOM_Template_List()
								{
									ID = sdr["template_id"].ToString(),
									Description = sdr["template_description"].ToString()
								});
							}
						}
					}
					if (template_id == -1)
					{
						model.materialpicker = new List<BOMMaterialPickItem>();
						using (SqlCommand command = new SqlCommand("SELECT * FROM materials;"))
						{
							command.Connection = conn;
							using (SqlDataReader sdr = command.ExecuteReader())
							{
								while (sdr.Read())
								{
									model.materialpicker.Add(new BOMMaterialPickItem()
									{
										ID = sdr["material_id"].ToString(),
										Description = sdr["material_desc"].ToString(),
										IsChecked = false
									});
								}
							}
						}
					}
				}
			}

			else
			{
				if (TempData["BOMInfo"] == null)
				{
					return RedirectToAction("employeeProjectDash");
				}
				model = JsonConvert.DeserializeObject<EmployeeBOMModel>(TempData["BOMInfo"].ToString());
				model.materialpicker = new List<BOMMaterialPickItem>();
				using (SqlConnection conn = new SqlConnection(connectionstring))
				{
					conn.Open();
					using (SqlCommand command = new SqlCommand("SELECT * FROM template_used_materials a " +
								"INNER JOIN materials b ON a.material_id = b.material_id " +
								"WHERE template_id = @template_id " +
								"ORDER BY b.material_id;"))
					{
						command.Connection = conn;
						command.Parameters.AddWithValue("@template_id", (int)template_id);
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							while (sdr.Read())
							{
								model.materialpicker.Add(new BOMMaterialPickItem()
								{
									ID = sdr["material_id"].ToString(),
									Description = sdr["material_desc"].ToString(),
									IsChecked = Convert.ToBoolean(sdr["is_used"])
								});
							}
						}
					}
				}
			}
			model.TemplateID = template_id.ToString();
			Debug.WriteLine("Temp" + model.TemplateID);
			return View(model);
		}


		public ActionResult employeeMaterialsDash()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			List<EmployeeDashboardMaterials> model = new List<EmployeeDashboardMaterials>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials a " +
							"INNER JOIN measurement_units b ON a.measurement_unit_id = b.measurement_unit_id;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Add(new EmployeeDashboardMaterials()
							{
								ID = Convert.ToInt32(sdr["material_id"]),
								Description = sdr["material_desc"].ToString(),
								Description_Long = sdr["material_desc_long"].ToString(),
								UnitOfMeasurement = sdr["measurement_unit_desc"].ToString()
							});
						}
					}
				}
			}
			return View(model);
		}

		public ActionResult employeeAddMaterial()
		{
			EmployeeAddMaterial model = new EmployeeAddMaterial();
			model.categories = new List<EmployeeListItem>();
			model.measurements = new List<EmployeeListItem>();
			model.measurement_type = new List<EmployeeListItem>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM material_categories;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.categories.Add(new EmployeeListItem()
							{
								ID = Convert.ToInt32(sdr["category_id"]),
								Description = sdr["category_desc"].ToString()
							});
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT * FROM measurement_units;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.measurements.Add(new EmployeeListItem()
							{
								ID = Convert.ToInt32(sdr["measurement_unit_id"]),
								Description = sdr["measurement_unit_desc"].ToString()
							});
						}
					}
				}
			}

			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 1,
				Description = "Length"
			});
			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 2,
				Description = "Area"
			});
			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 3,
				Description = "Weight"
			});
			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 4,
				Description = "Volume"
			});
			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 5,
				Description = "Pieces"
			});

			return View(model);
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public ActionResult employeeAddMaterial(EmployeeAddMaterial model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			return View(model);

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("INSERT INTO materials(material_desc,material_desc_long,category_id,measurement_unit_id,material_measurement_type,material_measurement_value) " +
					"VALUES (@material_desc,@material_desc_long, @category_id, @measurement_unit_id, @material_measurement_type, @material_measurement_value);"))
				{
					command.Connection = conn;

					command.Parameters.AddWithValue("@material_desc", model.Description);
					command.Parameters.AddWithValue("@material_desc_long", model.Description_Long);
					command.Parameters.AddWithValue("@category_id", Convert.ToInt32(model.CategoryID));
					command.Parameters.AddWithValue("@measurement_unit_id", Convert.ToInt32(model.UnitOfMeasurement));
					command.Parameters.AddWithValue("@material_measurement_type", Convert.ToInt32(model.MeasurementType));
					command.Parameters.AddWithValue("@material_measurement_value", model.MeasurementValue);

					conn.Open();
					command.ExecuteNonQuery();
					conn.Close();
				}
			}

			return RedirectToAction("employeeMaterialsDash");
		}

		public ActionResult employeeEditMaterial(int? id)
		{
			if (id == null || id < 0)
			{
				return RedirectToAction("employeeMaterialsDash");
			}
			EmployeeAddMaterial model = new EmployeeAddMaterial();
			model.categories = new List<EmployeeListItem>();
			model.measurements = new List<EmployeeListItem>();
			model.measurement_type = new List<EmployeeListItem>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM material_categories;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.categories.Add(new EmployeeListItem()
							{
								ID = Convert.ToInt32(sdr["category_id"]),
								Description = sdr["category_desc"].ToString()
							});
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT * FROM measurement_units;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.measurements.Add(new EmployeeListItem()
							{
								ID = Convert.ToInt32(sdr["measurement_unit_id"]),
								Description = sdr["measurement_unit_desc"].ToString()
							});
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials WHERE material_id = @material_id;"))
				{
					command.Parameters.AddWithValue("@material_id", Convert.ToInt32(id));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.ID = Convert.ToInt32(id);
							model.Description = sdr["material_desc"].ToString();
							model.Description_Long = sdr["material_desc_long"].ToString();
							model.CategoryID = Convert.ToInt32(sdr["category_id"]);
							model.UnitOfMeasurement = Convert.ToInt32(sdr["measurement_unit_id"]);
							model.MeasurementType = Convert.ToInt32(sdr["material_measurement_type"]);
							model.MeasurementValue = Convert.ToDouble(sdr["material_measurement_value"]);
						}
					}
				}
			}

			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 1,
				Description = "Length"
			});
			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 2,
				Description = "Area"
			});
			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 3,
				Description = "Weight"
			});
			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 4,
				Description = "Volume"
			});
			model.measurement_type.Add(new EmployeeListItem()
			{
				ID = 5,
				Description = "Pieces"
			});

			return View(model);
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public ActionResult employeeEditMaterial(EmployeeAddMaterial model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("UPDATE materials SET " +
					"material_desc = @material_desc, " +
					"material_desc_long = @material_desc_long, " +
					"category_id = @category_id, " +
					"measurement_unit_id = @measurement_unit_id, " +
					"material_measurement_type = @material_measurement_type, " +
					"material_measurement_value = @material_measurement_value " +
					"WHERE material_id = @material_id;"))
				{
					command.Connection = conn;

					command.Parameters.AddWithValue("@material_id", model.ID);
					command.Parameters.AddWithValue("@material_desc", model.Description);
					command.Parameters.AddWithValue("@material_desc_long", model.Description_Long);
					command.Parameters.AddWithValue("@category_id", Convert.ToInt32(model.CategoryID));
					command.Parameters.AddWithValue("@measurement_unit_id", Convert.ToInt32(model.UnitOfMeasurement));
					command.Parameters.AddWithValue("@material_measurement_type", Convert.ToInt32(model.MeasurementType));
					command.Parameters.AddWithValue("@material_measurement_value", model.MeasurementValue);

					conn.Open();
					command.ExecuteNonQuery();
					conn.Close();
				}
			}

			return RedirectToAction("employeeMaterialsDash");
		}



		public ActionResult employeeInfo()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
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
					command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.ID = Convert.ToInt32(HttpContext.Session.GetInt32("EmployeeID"));
							model.user_id = Convert.ToInt32(sdr["user_credentials_id"]);
							model.username = sdr["username"].ToString();
							model.password = "";
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

		public ActionResult employeeInfoEdit()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
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
					command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.ID = Convert.ToInt32(HttpContext.Session.GetInt32("EmployeeID"));
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

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public ActionResult employeeInfoEdit(EmployeeInfoModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			string pattern = @"^09\d{9}$";
			if (model.contactNum != null)
			{
				if (!Regex.IsMatch(model.contactNum, pattern))
				{
					Debug.WriteLine("2");
					ModelState.AddModelError("contactNum", "This number is not valid. Use format \"09#########\".");

				}
			}
			string emailPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
			if (model.email != null)
			{
				if (!Regex.IsMatch(model.email, emailPattern))
				{
					ModelState.AddModelError("email", "This email is not valid. Use the format of example \"abc@domain.com\".");

				}
				if (model.email.Length > 62)
				{
					ModelState.AddModelError("email", "This email too long. Enter an email less than 63 characters long.");
				}
			}
			if (model.password != null)
			{
				if (model.password.Length > 0 && model.password.Length < 6)
				{
					ModelState.AddModelError("password", "Enter a password at least 6 characters long.");
				}
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
				if (model.password == null || model.password.Length == 0)
				{
					using (SqlCommand command = new SqlCommand(
						"UPDATE user_credentials SET " +
						"user_status = @user_status " +
						"WHERE user_id = @user_id;"))
					{
						command.Connection = conn;

						command.Parameters.AddWithValue("@user_id", model.user_id);
						command.Parameters.AddWithValue("@user_status", model.status);

						command.ExecuteNonQuery();
						Debug.WriteLine("!?" + model.user_id);
						Debug.WriteLine("!?" + model.password);
						Debug.WriteLine("!?" + model.status);
					}
				}
				else if (model.password.Length != 0 && model.password.Length > 5)
				{
					using (SqlCommand command = new SqlCommand(
						"UPDATE user_credentials SET " +
						"user_password = @user_password, " +
						"user_status = @user_status " +
						"WHERE user_id = @user_id;"))
					{
						command.Connection = conn;

						command.Parameters.AddWithValue("@user_id", model.user_id);
						command.Parameters.AddWithValue("@user_password", PasswordEncryptor.EncryptPassword(model.password));
						command.Parameters.AddWithValue("@user_status", model.status);

						command.ExecuteNonQuery();
						Debug.WriteLine("!?" + model.user_id);
						Debug.WriteLine("!?" + model.password);
						Debug.WriteLine("!?" + model.status);
					}
				}
				conn.Close();
			}
			Debug.WriteLine("?");

			return RedirectToAction("employeeInfo");

		}



		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public ActionResult BOMGenerate(EmployeeBOMModel model, string submitButton)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			if (submitButton == "Select a Template")
			{
				TempData["BOMInfo"] = JsonConvert.SerializeObject(model);
				return RedirectToAction("BOMTemplates");

				//EmployeeBOMModel model = JsonConvert.DeserializeObject<EmployeeBOMModel>(TempData["BOMGenerateData"].ToString());
			}
			string location = model.Latitude + "," + model.Longtitude;
			Debug.WriteLine("Lat: " + model.Latitude);
			Debug.WriteLine("Lon: " + model.Longtitude);
			Debug.WriteLine("Loc: " + location);

			int noOfStories = model.NumberOfStoreys;
			 double heightOfFloors = model.FloorHeight,
								lengthOfBuilding = model.BuildingLength,
								widthOfBuilding = model.BuildingWidth,
								sqmOfBuilding = heightOfFloors * lengthOfBuilding,
								π = 3.14159

				;
			double floorThickness = 0,
								wallThickness = 0,
			/*new*/             rebarPercentage = 0,
			/*new*/             rebarDiameter = 0,
								nailConstant = 0,
								hollowBlockLength = 0,
								hollowBlockWidth = 0,
								hollowBlockHeight = 0,
								hollowBlockVolume = 0,
								hollowBlockConstant = 0,
								supportBeamLength = 0,
								supportBeamWidth = 0,
								supportBeamArea = 0,
								supportBeamSpace = 0,
								supportBeamVolume = 0,
								supportBeamsNeeded = 0,
								concreteFormulaCement = 0,
								concreteFormulaSand = 0,
								concreteFormulaAggregate = 0,
								plywoodLength = 0,
								plywoodWidth = 0,
								plywoodArea = 0,
								plywoodSheetsPerSqm = 0,
								riserHeight = 0,
								threadDepth = 0,
								stairWidth = 0,
								numberOfSteps = 0,
				 /*new*/        wastage = 0,
				 /*new*/        provisions = 0;
			Debug.WriteLine(model.TemplateID);
			Debug.WriteLine(model.BuildingLength);
			Debug.WriteLine(model.BuildingWidth);
			int formulaid = 0;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM employee_templates a " +
					"INNER JOIN template_formula_constants b ON a.formula_constants_id = b.formula_constants_id " +
					"WHERE template_id = @template_id;"))
				{
					command.Parameters.AddWithValue("@template_id", Convert.ToInt32(model.TemplateID));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.FormulaID = Convert.ToInt32(sdr["formula_constants_id"]);
							Debug.WriteLine("AAAE"); 
							Debug.WriteLine(model.FormulaID); 
							formulaid = Convert.ToInt32(sdr["formula_constants_id"]);
							Debug.WriteLine(Convert.ToInt32(sdr["formula_constants_id"]));
							// measurements that are changeable
							floorThickness = Convert.ToDouble(sdr["floor_thickness"]);
							wallThickness = Convert.ToDouble(sdr["wall_thickness"]);
							/*new*/
							rebarPercentage = 0.1; //percentage of reinforcement standard 0.08 ~ 0.12
							/*new*/
							rebarDiameter = Convert.ToDouble(sdr["rebar_thickness"]);
							nailConstant = Convert.ToDouble(sdr["nail_interval"]);
							hollowBlockLength = 0.2;
							hollowBlockWidth = 0.2;
							hollowBlockHeight = 0.2;
							hollowBlockVolume = hollowBlockHeight * hollowBlockLength * hollowBlockWidth;
							hollowBlockConstant = Convert.ToDouble(sdr["hollow_block_constant"]);
							supportBeamLength = Convert.ToDouble(sdr["support_beam_length"]);
							supportBeamWidth = Convert.ToDouble(sdr["support_beam_width"]);
							supportBeamArea = supportBeamLength * supportBeamWidth;
							supportBeamSpace = Convert.ToDouble(sdr["support_beam_interval"]);
							supportBeamVolume = supportBeamArea * heightOfFloors;
							supportBeamsNeeded = sqmOfBuilding / supportBeamSpace;
							concreteFormulaCement = Convert.ToDouble(sdr["concrete_ratio_cement"]);
							concreteFormulaSand = Convert.ToDouble(sdr["concrete_ratio_sand"]);
							concreteFormulaAggregate = Convert.ToDouble(sdr["concrete_ratio_aggregate"]);
								plywoodLength = Convert.ToDouble(sdr["plywood_length"]);
							plywoodWidth = Convert.ToDouble(sdr["plywood_width"]);
							plywoodArea = plywoodLength * plywoodWidth;
							plywoodSheetsPerSqm = (double)Math.Ceiling(plywoodArea);
							riserHeight = Convert.ToDouble(sdr["riser_height"]);
								threadDepth = Convert.ToDouble(sdr["thread_depth"]);
							stairWidth = Convert.ToDouble(sdr["stairs_width"]);
								numberOfSteps = (double)Math.Ceiling(heightOfFloors / riserHeight);
							/*new*/
							wastage = Convert.ToDouble(sdr["wastage"]);
							/*new*/
							provisions = Convert.ToDouble(sdr["provisions"]);

				;
						}
					}
				}
			}

			Debug.WriteLine(floorThickness);
			Debug.WriteLine(supportBeamSpace);
			Debug.WriteLine(stairWidth);

			// prices, are changeable
			MaterialsCostComparisonItem
				Cost_rebar = GetBestPrice(5, location),
				Cost_Hollowblock = GetBestPrice(6, location),
				Cost_Cement = GetBestPrice(2, location),
				Cost_Sand = GetBestPrice(3, location),
				Cost_Aggregate = GetBestPrice(4, location),
				Cost_Plywood = GetBestPrice(1, location);
			double rebarPrice = Cost_rebar.Price, // per piece. Rebars * rebarPrice
								hollowBlockPrice = Cost_Hollowblock.Price, // per piece. NoOfHollowBlock * hollowBlockPrice
								cementPrice = Cost_Cement.Price, // per cubic meter. CementCubicMeters * cementPrice,
								sandPrice = Cost_Sand.Price, // per cubic meter
								sandReducedPrice = (1 / 3) * sandPrice,
								sandBigPrice = 2 * sandPrice,
								aggregatePrice = Cost_Aggregate.Price, // per cubic meter
								aggregateReducedPrice = (1 / 2) * aggregatePrice,
								aggregateBigPrice = 3 * aggregatePrice,
								concretePrice = cementPrice + sandPrice + aggregatePrice,
								concreteReducedPrice = cementPrice + sandReducedPrice + aggregateReducedPrice,
								concreteBigPrice = cementPrice + sandBigPrice + aggregateBigPrice,
								plywoodPrice = Cost_Plywood.Price, // per piece 1/4  
								plywoodPricePerSqm = plywoodPrice * plywoodSheetsPerSqm

				;
			int RebarCostID = Cost_rebar.SupplierMaterialID,
				HollowBlockCostID = Cost_Hollowblock.SupplierMaterialID,
				CementCostID = Cost_Cement.SupplierMaterialID,
				SandCostID = Cost_Sand.SupplierMaterialID,
				AggregateCostID = Cost_Aggregate.SupplierMaterialID,
				PlywoodCostID = Cost_Plywood.SupplierMaterialID;



			double concreteTotalRatio = concreteFormulaCement + concreteFormulaSand + concreteFormulaAggregate;
			double cementRatio = concreteFormulaCement / concreteTotalRatio;
			double sandRatio = concreteFormulaSand / concreteTotalRatio;
			double aggregateRatio = concreteFormulaAggregate / concreteTotalRatio;

			// foundation
			double foundationHeight = heightOfFloors * noOfStories + (noOfStories * floorThickness),
								foundationVolume = foundationHeight * sqmOfBuilding,
								foundationPerimeter = 2 * (lengthOfBuilding + widthOfBuilding),
								foundationWallArea = 4 * foundationPerimeter * foundationHeight,
								foundationNoOfHollowBlock = foundationWallArea * hollowBlockConstant,
			/*new*/             foundationRebar = (lengthOfBuilding * widthOfBuilding * rebarPercentage) * foundationHeight,
								foundationConcrete = foundationVolume
				;
			model.lists = new List<Employee_BOM_Materials_Lists>();
			model.lists.Add(new Employee_BOM_Materials_Lists()
			{
				Description = $"Foundation",
				ListNumber = 1,
				Items = new List<Employee_BOM_Materials_Items>()
			});
			model.lists[0].Items.Add(new Employee_BOM_Materials_Items()
			{
				Description = $"Foundation",
				Subitems = new List<Employee_BOM_Materials_Subitems>()
			});

			if (model.materialpicker[1].IsChecked == true)
			{
				int foundationConcreteCement = (int)Math.Ceiling(foundationConcrete * cementRatio);
				model.lists[0].Items[0].Subitems.Add(GetMaterial(2, foundationConcreteCement, location, 1, cementPrice, wastage, provisions, CementCostID));
			}

			if (model.materialpicker[2].IsChecked == true)
			{
				int foundationConcreteSand = (int)Math.Ceiling(foundationConcrete * sandRatio);
				model.lists[0].Items[0].Subitems.Add(GetMaterial(3, foundationConcreteSand, location, 2, sandPrice, wastage, provisions, SandCostID));
			}


			if (model.materialpicker[3].IsChecked == true)
			{
				int foundationConcreteAggregate = (int)Math.Ceiling(foundationConcrete * aggregateRatio);
				model.lists[0].Items[0].Subitems.Add(GetMaterial(4, foundationConcreteAggregate, location, 3, aggregatePrice, wastage, provisions, AggregateCostID));
			}

			if (model.materialpicker[4].IsChecked == true)
			{
				int foundationRebarAmount = (int)Math.Ceiling(foundationRebar);
				model.lists[0].Items[0].Subitems.Add(GetMaterial(5, foundationRebarAmount, location, 4, rebarPrice, wastage, provisions, RebarCostID));
			}

			if (model.materialpicker[5].IsChecked == true)
			{
				int foundationHollowBlock = (int)Math.Ceiling(foundationNoOfHollowBlock);
				model.lists[0].Items[0].Subitems.Add(GetMaterial(6, foundationHollowBlock, location, 5, hollowBlockPrice, wastage, provisions, HollowBlockCostID));
			}

			for (int i = 1; i <= model.NumberOfStoreys; i++)
			{
				model.lists.Add(new Employee_BOM_Materials_Lists()
				{
					Description = $"Storey {i}",
					ListNumber = i + 1,
					Items = new List<Employee_BOM_Materials_Items>()
				});
				model.lists[i].Items.Add(new Employee_BOM_Materials_Items()
				{
					Description = $"Storey {i} floor",
					Subitems = new List<Employee_BOM_Materials_Subitems>()
				});
				model.lists[i].Items.Add(new Employee_BOM_Materials_Items()
				{
					Description = $"Storey {i} walls",
					Subitems = new List<Employee_BOM_Materials_Subitems>()
				});
				model.lists[i].Items.Add(new Employee_BOM_Materials_Items()
				{
					Description = $"Storey {i} support beams",
					Subitems = new List<Employee_BOM_Materials_Subitems>()
				});
				if (model.NumberOfStoreys > 1)
				{
					model.lists[i].Items.Add(new Employee_BOM_Materials_Items()
					{
						Description = $"Storey {i} stairs",
						Subitems = new List<Employee_BOM_Materials_Subitems>()
					});
				}
				// stories. repeat for every storey the building has
				double storeyHeight = heightOfFloors + floorThickness,
								storeyPerimeter = 2 * (lengthOfBuilding + widthOfBuilding),
								storeyWallVolume = 4 * storeyPerimeter * storeyHeight,
								storeyFloorVolume = sqmOfBuilding * floorThickness,
								// floors
								storeyFloorPlywood = plywoodSheetsPerSqm * sqmOfBuilding,
								storeyFloorNails = storeyFloorPlywood * nailConstant,
								storeyFloorConcrete = storeyFloorVolume,
			 /*new*/            storeyFloorRebar = (lengthOfBuilding * widthOfBuilding * rebarPercentage) * floorThickness
				,


								// support beams
								storeySupportBeamsNeeded = supportBeamsNeeded,
								storeySupportBeamsConcrete = supportBeamVolume,
			 /*new*/            storeySupportBeamsRebar = (supportBeamLength * supportBeamWidth * rebarPercentage) * heightOfFloors,

								// walls
								storeyWallConcrete = storeyWallVolume,
			  /*new*/           storeyWallRebar = ((lengthOfBuilding * wallThickness * rebarPercentage) * heightOfFloors) * 4
				,

								// stairs
								stairsVolume = numberOfSteps * (riserHeight * threadDepth * stairWidth),
								stairsConcrete = stairsVolume,
			   /*new*/          stairsRebar = ((stairWidth * threadDepth * rebarPercentage) * riserHeight) * numberOfSteps


				;
				if (model.materialpicker[0].IsChecked == true)
				{
					int floorPlywood = (int)Math.Ceiling(storeyFloorPlywood);
					model.lists[i].Items[0].Subitems.Add(GetMaterial(1, floorPlywood, location, 1, plywoodPrice, wastage, provisions, PlywoodCostID));
				}

				if (model.materialpicker[1].IsChecked == true)
				{
					int floorConcreteCement = (int)Math.Ceiling(storeyFloorConcrete * cementRatio);
					model.lists[i].Items[0].Subitems.Add(GetMaterial(2, floorConcreteCement, location, 2, cementPrice, wastage, provisions, CementCostID));
				}

				if (model.materialpicker[2].IsChecked == true)
				{
					int floorConcreteSand = (int)Math.Ceiling(storeyFloorConcrete * sandRatio);
					model.lists[i].Items[0].Subitems.Add(GetMaterial(3, floorConcreteSand, location, 3, sandPrice, wastage, provisions, SandCostID));
				}

				if (model.materialpicker[3].IsChecked == true)
				{
					int floorConcreteAggregate = (int)Math.Ceiling(storeyFloorConcrete * aggregateRatio);
					model.lists[i].Items[0].Subitems.Add(GetMaterial(4, floorConcreteAggregate, location, 4, aggregatePrice, wastage, provisions, AggregateCostID));
				}

				if (model.materialpicker[4].IsChecked == true)
				{
					int floorRebarAmount = (int)Math.Ceiling(storeyFloorRebar);
					model.lists[i].Items[0].Subitems.Add(GetMaterial(5, floorRebarAmount, location, 5, rebarPrice, wastage, provisions, RebarCostID));
				}

				//wall

				if (model.materialpicker[1].IsChecked == true)
				{
					int wallConcreteCement = (int)Math.Ceiling(storeyWallConcrete * cementRatio);
					model.lists[i].Items[1].Subitems.Add(GetMaterial(2, wallConcreteCement, location, 1, cementPrice, wastage, provisions, CementCostID));
				}

				if (model.materialpicker[2].IsChecked == true)
				{
					int wallConcreteSand = (int)Math.Ceiling(storeyWallConcrete * sandRatio);
					model.lists[i].Items[1].Subitems.Add(GetMaterial(3, wallConcreteSand, location, 2, sandPrice, wastage, provisions, SandCostID));
				}

				if (model.materialpicker[3].IsChecked == true)
				{
					int wallConcreteAggregate = (int)Math.Ceiling(storeyWallConcrete * aggregateRatio);
					model.lists[i].Items[1].Subitems.Add(GetMaterial(4, wallConcreteAggregate, location, 3, aggregatePrice, wastage, provisions, AggregateCostID));
				}

				if (model.materialpicker[4].IsChecked == true)
				{
					int wallRebarAmount = (int)Math.Ceiling(storeyWallRebar);
					model.lists[i].Items[1].Subitems.Add(GetMaterial(5, wallRebarAmount, location, 4, rebarPrice, wastage, provisions, RebarCostID));
				}


				if (model.materialpicker[1].IsChecked == true)
				{
					//beam
					int beamConcreteCement = (int)Math.Ceiling(storeySupportBeamsConcrete * cementRatio);
					model.lists[i].Items[2].Subitems.Add(GetMaterial(2, beamConcreteCement, location, 1, cementPrice, wastage, provisions, CementCostID));
				}

				if (model.materialpicker[2].IsChecked == true)
				{
					int beamConcreteSand = (int)Math.Ceiling(storeySupportBeamsConcrete * sandRatio);
					model.lists[i].Items[2].Subitems.Add(GetMaterial(3, beamConcreteSand, location, 2, sandPrice, wastage, provisions, SandCostID));
				}

				if (model.materialpicker[3].IsChecked == true)
				{
					int beamConcreteAggregate = (int)Math.Ceiling(storeySupportBeamsConcrete * aggregateRatio);
					model.lists[i].Items[2].Subitems.Add(GetMaterial(4, beamConcreteAggregate, location, 3, aggregatePrice, wastage, provisions, AggregateCostID));
				}

				if (model.materialpicker[4].IsChecked == true)
				{
					int beamRebarAmount = (int)Math.Ceiling(storeySupportBeamsRebar);
					model.lists[i].Items[2].Subitems.Add(GetMaterial(5, beamRebarAmount, location, 4, rebarPrice, wastage, provisions, RebarCostID));
				}

				//stair
				if (model.NumberOfStoreys > 1)
				{

					if (model.materialpicker[1].IsChecked == true)
					{
						int stairsConcreteCement = (int)Math.Ceiling(stairsConcrete * cementRatio);
						model.lists[i].Items[3].Subitems.Add(GetMaterial(2, stairsConcreteCement, location, 1, cementPrice, wastage, provisions, CementCostID));
					}

					if (model.materialpicker[2].IsChecked == true)
					{
						int stairsConcreteSand = (int)Math.Ceiling(stairsConcrete * sandRatio);
						model.lists[i].Items[3].Subitems.Add(GetMaterial(3, stairsConcreteSand, location, 2, sandPrice, wastage, provisions, SandCostID));
					}

					if (model.materialpicker[3].IsChecked == true)
					{
						int stairsConcreteAggregate = (int)Math.Ceiling(stairsConcrete * aggregateRatio);
						model.lists[i].Items[3].Subitems.Add(GetMaterial(4, stairsConcreteAggregate, location, 3, aggregatePrice, wastage, provisions, AggregateCostID));
					}

					if (model.materialpicker[4].IsChecked == true)
					{
						int stairsRebarAmount = (int)Math.Ceiling(stairsRebar);
						model.lists[i].Items[3].Subitems.Add(GetMaterial(5, stairsRebarAmount, location, 4, rebarPrice, wastage, provisions, RebarCostID));
					}
				}
			}


			model.totalCost = 0;
			foreach (Employee_BOM_Materials_Lists x in model.lists)
			{
				foreach (Employee_BOM_Materials_Items y in x.Items)
				{
					foreach (Employee_BOM_Materials_Subitems z in y.Subitems)
					{
						model.totalCost += (z.MaterialCost * Math.Ceiling((double)z.MaterialQuantity));
					}
				}
			}
			model.FormulaID = formulaid;

			model.suppliers = new List<SupplierListItem>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM supplier_info WHERE employee_id = @employee_id;"))
				{
					command.Parameters.AddWithValue("@employee_id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.suppliers.Add(new SupplierListItem()
							{
								ID = Convert.ToInt32(sdr["supplier_id"]),
								Description = sdr["supplier_desc"].ToString(),
								ContactName = sdr["supplier_contact_name"].ToString(),
								ContactNumber = sdr["supplier_contact_number"].ToString()
							});
						}
					}
				}
			}

			//TempData["BOMGenerateData"] = JsonConvert.SerializeObject(model);
			//return RedirectToAction("BOMToAdd");
			return View("BOMAdd", model);
		}

		public ActionResult BOMAdd(EmployeeBOMModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			/*
			if (model == null)
			{
				return RedirectToAction("employeeProjectDash");
			}
			else if (model != null)
			{
				return (model);
			}
			EmployeeBOMModel model2 = JsonConvert.DeserializeObject<EmployeeBOMModel>(TempData["BOMGenerateData"].ToString());
			return View(model2);*/
			Debug.WriteLine("ooo");
			Debug.WriteLine(model.suppliers == null);
			return View(model);
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public ActionResult BOMAdd(EmployeeBOMModel model, string Continue)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			//TempData["BOMGenerateData"] = JsonConvert.SerializeObject(model);
			Debug.WriteLine("boolet");
			Debug.WriteLine(model.ProjectID);
			Debug.WriteLine(model.TemplateID);
			model.FormulaID = Convert.ToInt32(model.TemplateID);
			Debug.WriteLine(model.FormulaID);
			Debug.WriteLine("boolet");

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlTransaction trans = conn.BeginTransaction())
				{
					int bom_id = 0;
					using (SqlCommand command = new SqlCommand("INSERT INTO bom (bom_creation_date, project_id, bom_formula_id) VALUES ( " +
						"@bom_creation_date, @project_id, @bom_formula_id);" +
						" SELECT SCOPE_IDENTITY() FROM bom;"))
					{
						command.Parameters.AddWithValue("@bom_creation_date", DateTime.Today.ToString("yyyy-MM-dd"));
						command.Parameters.AddWithValue("@project_id", (int)model.ProjectID);
						command.Parameters.AddWithValue("@bom_formula_id", (int)model.FormulaID);
						command.Connection = conn;
						command.Transaction = trans;
						bom_id = Convert.ToInt32(command.ExecuteScalar());
					}

					foreach (Employee_BOM_Materials_Lists list in model.lists)
					{
						SqlCommand command2 = new SqlCommand(
							"INSERT INTO bom_lists (bom_list_desc,bom_id) VALUES " +
							"(@list_desc," +
							"@bom_id);" +
							"SELECT SCOPE_IDENTITY() FROM bom_lists;");

						command2.Parameters.AddWithValue("@list_desc", list.Description);
						command2.Parameters.AddWithValue("@bom_id", bom_id);

						command2.Connection = conn;
						command2.Transaction = trans;
						int bom_list_id = Convert.ToInt32(command2.ExecuteScalar());
						command2.Dispose();
						Debug.WriteLine("LIST");

						foreach (Employee_BOM_Materials_Items item in list.Items)
						{
							SqlCommand command3 = new SqlCommand(
							"INSERT INTO bom_items (bom_list_id,bom_list_desc) VALUES " +
							"(@bom_list_id," +
							"@bom_list_desc);" +
							"SELECT SCOPE_IDENTITY() FROM bom_items;");

							command3.Parameters.AddWithValue("@bom_list_id", bom_list_id);
							command3.Parameters.AddWithValue("@bom_list_desc", item.Description);

							command3.Connection = conn;
							command3.Transaction = trans;
							int bom_list_item_id = Convert.ToInt32(command3.ExecuteScalar());
							command3.Dispose();
							Debug.WriteLine("ITEM");

							foreach (Employee_BOM_Materials_Subitems subitem in item.Subitems)
							{
								SqlCommand command4 = new SqlCommand(
									"INSERT INTO bom_subitems (material_id,bom_subitem_quantity,bom_item_id, supplier_material_id) VALUES " +
									"(@item_id," +
									"@item_quantity," +
									"@bom_item_id, " +
									"@supplier_material_id);");

								command4.Parameters.AddWithValue("@item_id", subitem.MaterialID);
								command4.Parameters.AddWithValue("@item_quantity", subitem.MaterialQuantity);
								command4.Parameters.AddWithValue("@bom_item_id", bom_list_item_id);
								command4.Parameters.AddWithValue("@supplier_material_id", subitem.SupplierMaterialID);

								command4.Connection = conn;
								command4.Transaction = trans;
								command4.ExecuteNonQuery();
								command4.Dispose();
								Debug.WriteLine("SUBITEM");
							}
						}
					}
					trans.Commit();
					Debug.WriteLine("Saved?");
				}
			}

			return RedirectToAction("employeeProjectDash", "Employee");
		}

		public IActionResult BOMView(int? id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			int bom = 0;
			Debug.WriteLine(id);
			EmployeeBOMModel model = new EmployeeBOMModel();
			model.templates = new List<Employee_BOM_Template_List>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM bom a INNER JOIN projects b ON a.project_id = b.project_id " +
					"INNER JOIN template_formula_constants c ON a.bom_formula_id = formula_constants_id " +
					"INNER JOIN building_types d ON b.building_types_id = d.building_types_id " +
					"WHERE a.project_id = @id;"))
				{
					
					command.Parameters.AddWithValue("@id", (int)id);
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
							model.BuildingType = sdr["description"].ToString();
							model.NumberOfStoreys = Convert.ToInt32(sdr["project_building_storeys"]);
							model.FloorHeight = Convert.ToDouble(sdr["project_building_floorheight"]);
							model.BuildingLength = Convert.ToDouble(sdr["project_building_length"]);
							model.BuildingWidth = Convert.ToDouble(sdr["project_building_width"]);
							model.Longtitude = sdr["project_longtitude"].ToString();
							model.Latitude = sdr["project_latitude"].ToString();
							model.FormulaID = Convert.ToInt32(sdr["bom_formula_id"]);
							model.BOMCreationDate = DateTime.Parse(sdr["bom_creation_date"].ToString());
							model.Wastage = Convert.ToDouble(sdr["wastage"]);
							model.Provisions = Convert.ToDouble(sdr["provisions"]);
							bom = Convert.ToInt32(sdr["bom_id"]);
						}
					}
				}
				model.lists = new List<Employee_BOM_Materials_Lists>();
				using (SqlCommand command = new SqlCommand("SELECT * FROM bom_lists WHERE bom_id = @id;"))
				{
					command.Parameters.AddWithValue("@id", bom);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						int num = 1;
						while (sdr.Read())
						{
							model.lists.Add(new Employee_BOM_Materials_Lists()
							{
								Description = sdr["bom_list_desc"].ToString(),
								ListID = Convert.ToInt32(sdr["bom_list_id"]),
								ListNumber = num,
								Items = new List<Employee_BOM_Materials_Items>()
							});
							num++;
						}
					}
				}
				for (int x = 0; x < model.lists.Count; x++)
				{
					using (SqlCommand command = new SqlCommand("SELECT * FROM bom_items WHERE bom_list_id = @id;"))
					{
						command.Parameters.AddWithValue("@id", (int)model.lists[x].ListID);
						command.Connection = conn;
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							int num = 1;
							while (sdr.Read())
							{
								model.lists[x].Items.Add(new Employee_BOM_Materials_Items()
								{
									Description = sdr["bom_list_desc"].ToString(),
									ItemID = Convert.ToInt32(sdr["bom_item_id"]),
									ItemNumber = num,
									Subitems = new List<Employee_BOM_Materials_Subitems>()
								});
								num++;
							}
						}
					}
					for (int y = 0; y < model.lists[x].Items.Count; y++)
					{
						using (SqlCommand command = new SqlCommand("SELECT * FROM bom_subitems a " +
							"INNER JOIN materials b ON a.material_id = b.material_id " +
							"INNER JOIN supplier_materials c ON a.supplier_material_id = c.supplier_material_id " +
							"INNER JOIN measurement_units d ON b.measurement_unit_id = d.measurement_unit_id " +
							"INNER JOIN supplier_info e ON e.supplier_id = c.supplier_id " +
							"WHERE bom_item_id = @id;"))
						{
							command.Parameters.AddWithValue("@id", (int)model.lists[x].Items[y].ItemID);
							command.Connection = conn;
							using (SqlDataReader sdr = command.ExecuteReader())
							{
								int num = 1;
								while (sdr.Read())
								{
									model.lists[x].Items[y].Subitems.Add(new Employee_BOM_Materials_Subitems()
									{
										SubitemNumber = num,
										MaterialID = Convert.ToInt32(sdr["material_id"]),
										MaterialDesc = sdr["material_desc"].ToString(),
										MaterialUoM = sdr["measurement_unit_desc_short"].ToString(),
										MaterialQuantity = Convert.ToInt32(sdr["bom_subitem_quantity"]),
										MaterialQuantityWastage = (int)Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage),
										MaterialQuantityProvisions = (int)Math.Ceiling(Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage) * model.Provisions),
										MaterialCost = Convert.ToDouble(sdr["supplier_material_price"]) / 100,
										MaterialAmount = Math.Round((int)Math.Ceiling(Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage) * model.Provisions) * (Convert.ToDouble(sdr["supplier_material_price"]) / 100), 2),
										SupplierMaterialID = Convert.ToInt32(sdr["supplier_material_id"]),
										Supplier = sdr["supplier_desc"].ToString()
									});
									num++;
								}
							}
						}
					}
				}
			}

			model.totalCost = 0;
			foreach (Employee_BOM_Materials_Lists x in model.lists)
			{
				foreach (Employee_BOM_Materials_Items y in x.Items)
				{
					foreach (Employee_BOM_Materials_Subitems z in y.Subitems)
					{
						model.totalCost += (z.MaterialCost * Math.Ceiling((double)z.MaterialQuantity));
					}
				}
			}
			return View(model);
		}

		public IActionResult BOMViewClient(int? id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			int bom = 0;
			EmployeeBOMModel model = new EmployeeBOMModel();
			model.templates = new List<Employee_BOM_Template_List>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM bom a INNER JOIN projects b ON a.project_id = b.project_id " +
					"INNER JOIN template_formula_constants c ON a.bom_formula_id = formula_constants_id " +
					"INNER JOIN building_types d ON b.building_types_id = d.building_types_id " +
					"WHERE a.project_id = @id;"))
				{

					command.Parameters.AddWithValue("@id", (int)id);
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
							model.BuildingType = sdr["description"].ToString();
							model.NumberOfStoreys = Convert.ToInt32(sdr["project_building_storeys"]);
							model.FloorHeight = Convert.ToDouble(sdr["project_building_floorheight"]);
							model.BuildingLength = Convert.ToDouble(sdr["project_building_length"]);
							model.BuildingWidth = Convert.ToDouble(sdr["project_building_width"]);
							model.Longtitude = sdr["project_longtitude"].ToString();
							model.Latitude = sdr["project_latitude"].ToString();
							model.FormulaID = Convert.ToInt32(sdr["bom_formula_id"]);
							model.BOMCreationDate = DateTime.Parse(sdr["bom_creation_date"].ToString());
							model.Wastage = Convert.ToDouble(sdr["wastage"]);
							model.Provisions = Convert.ToDouble(sdr["provisions"]);
							bom = Convert.ToInt32(sdr["bom_id"]);
						}
					}
				}
				model.lists = new List<Employee_BOM_Materials_Lists>();
				using (SqlCommand command = new SqlCommand("SELECT * FROM bom_lists WHERE bom_id = @id;"))
				{
					command.Parameters.AddWithValue("@id", bom);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						int num = 1;
						while (sdr.Read())
						{
							model.lists.Add(new Employee_BOM_Materials_Lists()
							{
								Description = sdr["bom_list_desc"].ToString(),
								ListID = Convert.ToInt32(sdr["bom_list_id"]),
								ListNumber = num,
								Items = new List<Employee_BOM_Materials_Items>()
							});
							num++;
						}
					}
				}
				for (int x = 0; x < model.lists.Count; x++)
				{
					using (SqlCommand command = new SqlCommand("SELECT * FROM bom_items WHERE bom_list_id = @id;"))
					{
						command.Parameters.AddWithValue("@id", (int)model.lists[x].ListID);
						command.Connection = conn;
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							int num = 1;
							while (sdr.Read())
							{
								model.lists[x].Items.Add(new Employee_BOM_Materials_Items()
								{
									Description = sdr["bom_list_desc"].ToString(),
									ItemID = Convert.ToInt32(sdr["bom_item_id"]),
									ItemNumber = num,
									Subitems = new List<Employee_BOM_Materials_Subitems>()
								});
								num++;
							}
						}
					}
					for (int y = 0; y < model.lists[x].Items.Count; y++)
					{
						using (SqlCommand command = new SqlCommand("SELECT * FROM bom_subitems a " +
							"INNER JOIN materials b ON a.material_id = b.material_id " +
							"INNER JOIN supplier_materials c ON a.supplier_material_id = c.supplier_material_id " +
							"INNER JOIN measurement_units d ON b.measurement_unit_id = d.measurement_unit_id " +
							"INNER JOIN supplier_info e ON e.supplier_id = c.supplier_id " +
							"WHERE bom_item_id = @id;"))
						{
							command.Parameters.AddWithValue("@id", (int)model.lists[x].Items[y].ItemID);
							command.Connection = conn;
							using (SqlDataReader sdr = command.ExecuteReader())
							{
								int num = 1;
								while (sdr.Read())
								{
									model.lists[x].Items[y].Subitems.Add(new Employee_BOM_Materials_Subitems()
									{
										SubitemNumber = num,
										MaterialID = Convert.ToInt32(sdr["material_id"]),
										MaterialDesc = sdr["material_desc"].ToString(),
										MaterialUoM = sdr["measurement_unit_desc_short"].ToString(),
										MaterialQuantity = Convert.ToInt32(sdr["bom_subitem_quantity"]),
										MaterialQuantityWastage = (int)Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage),
										MaterialQuantityProvisions = (int)Math.Ceiling(Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage) * model.Provisions),
										MaterialCost = Convert.ToDouble(sdr["supplier_material_price"]) / 100,
										MaterialAmount = Math.Round((int)Math.Ceiling(Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage) * model.Provisions) * (Convert.ToDouble(sdr["supplier_material_price"]) / 100), 2),
										SupplierMaterialID = Convert.ToInt32(sdr["supplier_material_id"]),
										Supplier = sdr["supplier_desc"].ToString()
									});
									num++;
								}
							}
						}
					}
				}
			}

			model.totalCost = 0;
			foreach (Employee_BOM_Materials_Lists x in model.lists)
			{
				foreach (Employee_BOM_Materials_Items y in x.Items)
				{
					foreach (Employee_BOM_Materials_Subitems z in y.Subitems)
					{
						model.totalCost += (z.MaterialCost * Math.Ceiling((double)z.MaterialQuantity));
					}
				}
			}
			return View(model);
		}

		public IActionResult BOMList()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			List<Employee_BOM_List_Item> projects = new List<Employee_BOM_List_Item>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT *, " +
					"CASE \n\tWHEN c.bom_id IS NULL THEN 0" +
					"\tELSE 1 \nEND AS HasReference \nFROM bom a " +
					"INNER JOIN projects b ON a.project_id = b.project_id " +
					"LEFT JOIN mce c ON a.bom_id = c.bom_id " +
					"WHERE b.project_engineer_id = @project_engineer_id;"))
				{
					command.Parameters.AddWithValue("@project_engineer_id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							projects.Add(new Employee_BOM_List_Item()
							{
								ID = Convert.ToInt32(sdr["bom_id"]),
								Title = sdr["project_title"].ToString(),
								ClientName = sdr["project_client_name"].ToString(),
								Date = DateTime.Parse(sdr["bom_creation_date"].ToString()),
								MCEExists = Convert.ToBoolean(sdr["HasReference"])
							});
						}
					}
				}
			}
			return View(projects);
		}

		public IActionResult ToMCEAdd(int? id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			EmployeeBOMModel model = new EmployeeBOMModel();
			model.templates = new List<Employee_BOM_Template_List>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM bom a INNER JOIN projects b ON a.project_id = b.project_id " +
					"INNER JOIN template_formula_constants c ON a.bom_formula_id = formula_constants_id " +
					"INNER JOIN building_types d ON b.building_types_id = d.building_types_id " +
					"WHERE a.bom_id = @id;"))
				{

					command.Parameters.AddWithValue("@id", (int)id);
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
							model.BuildingType = sdr["description"].ToString();
							model.NumberOfStoreys = Convert.ToInt32(sdr["project_building_storeys"]);
							model.FloorHeight = Convert.ToDouble(sdr["project_building_floorheight"]);
							model.BuildingLength = Convert.ToDouble(sdr["project_building_length"]);
							model.BuildingWidth = Convert.ToDouble(sdr["project_building_width"]);
							model.Longtitude = sdr["project_longtitude"].ToString();
							model.Latitude = sdr["project_latitude"].ToString();
							model.FormulaID = Convert.ToInt32(sdr["bom_formula_id"]);
							model.BOMCreationDate = DateTime.Parse(sdr["bom_creation_date"].ToString());
							model.Wastage = Convert.ToDouble(sdr["wastage"]);
							model.Provisions = Convert.ToDouble(sdr["provisions"]);
							model.ProjectID = Convert.ToInt32(sdr["project_id"]);
						}
					}
				}
				model.lists = new List<Employee_BOM_Materials_Lists>();
				using (SqlCommand command = new SqlCommand("SELECT * FROM bom_lists WHERE bom_id = @id;"))
				{
					command.Parameters.AddWithValue("@id", (int)id);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						int num = 1;
						while (sdr.Read())
						{
							model.lists.Add(new Employee_BOM_Materials_Lists()
							{
								Description = sdr["bom_list_desc"].ToString(),
								ListID = Convert.ToInt32(sdr["bom_list_id"]),
								ListNumber = num,
								Items = new List<Employee_BOM_Materials_Items>()
							});
							num++;
						}
					}
				}
				for (int x = 0; x < model.lists.Count; x++)
				{
					using (SqlCommand command = new SqlCommand("SELECT * FROM bom_items WHERE bom_list_id = @id;"))
					{
						command.Parameters.AddWithValue("@id", (int)model.lists[x].ListID);
						command.Connection = conn;
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							int num = 1;
							while (sdr.Read())
							{
								model.lists[x].Items.Add(new Employee_BOM_Materials_Items()
								{
									Description = sdr["bom_list_desc"].ToString(),
									ItemID = Convert.ToInt32(sdr["bom_item_id"]),
									ItemNumber = num,
									Subitems = new List<Employee_BOM_Materials_Subitems>()
								});
								num++;
							}
						}
					}
					for (int y = 0; y < model.lists[x].Items.Count; y++)
					{
						using (SqlCommand command = new SqlCommand("SELECT * FROM bom_subitems a " +
							"INNER JOIN materials b ON a.material_id = b.material_id " +
							"INNER JOIN supplier_materials c ON a.supplier_material_id = c.supplier_material_id " +
							"INNER JOIN measurement_units d ON b.measurement_unit_id = d.measurement_unit_id " +
							"INNER JOIN supplier_info e ON e.supplier_id = c.supplier_id " +
							"WHERE bom_item_id = @id;"))
						{
							command.Connection = conn;
							using (SqlDataReader sdr = command.ExecuteReader())
							{
								int num = 1;
								while (sdr.Read())
								{
									model.lists[x].Items[y].Subitems.Add(new Employee_BOM_Materials_Subitems()
									{
										SubitemNumber = num,
										MaterialID = Convert.ToInt32(sdr["material_id"]),
										MaterialDesc = sdr["material_desc"].ToString(),
										MaterialUoM = sdr["measurement_unit_desc_short"].ToString(),
										MaterialQuantity = Convert.ToInt32(sdr["bom_subitem_quantity"]),
										MaterialQuantityWastage = (int)Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage),
										MaterialQuantityProvisions = (int)Math.Ceiling(Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage) * model.Provisions),
										MaterialCost = Convert.ToDouble(sdr["supplier_material_price"]) / 100,
										MaterialAmount = Math.Round((int)Math.Ceiling(Math.Ceiling(Convert.ToInt32(sdr["bom_subitem_quantity"]) * model.Wastage) * model.Provisions) * (Convert.ToDouble(sdr["supplier_material_price"]) / 100), 2),
										SupplierMaterialID = Convert.ToInt32(sdr["supplier_material_id"]),
										Supplier = sdr["supplier_desc"].ToString()
									});
									num++;
								}
							}
						}
					}
				}
			}

			model.totalCost = 0;
			foreach (Employee_BOM_Materials_Lists x in model.lists)
			{
				foreach (Employee_BOM_Materials_Items y in x.Items)
				{
					foreach (Employee_BOM_Materials_Subitems z in y.Subitems)
					{
						model.totalCost += (z.MaterialCost * Math.Ceiling((double)z.MaterialQuantity));
					}
				}
			}

			TempData["MCEData"] = JsonConvert.SerializeObject(model);
			return RedirectToAction("MCEAdd");
		}

		public IActionResult MCEAdd(Employee_MCE model)
		{

			//Employee_MCE model = JsonConvert.DeserializeObject<Employee_MCE>(TempData["MCEData"].ToString());
			return View(model);
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> MCEAdd(Employee_MCE model, string submitButton)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			Debug.WriteLine(model == null);
			Debug.WriteLine("Menshevik");
			Debug.WriteLine(model.ProjectID);
			
			if (submitButton == "Update")
			{
				for (int x = 0; x < model.materials.Count; x++)
				{
							Debug.WriteLine("Bolshevik");
							Debug.WriteLine(Math.Round(((double)model.Markup / 100.00) + 1.00, 2));
							model.materials[x].MarkedUpCost =
								Math.Ceiling(
								model.materials[x].MaterialCost *
								Math.Round((model.Markup / 100) + 1, 2));
							model.materials[x].TotalUnitRate =
								Math.Ceiling(
								model.materials[x].MarkedUpCost +
								model.materials[x].LabourCost);
							model.materials[x].MaterialAmount =
								Math.Ceiling(
								model.materials[x].TotalUnitRate *
								model.materials[x].MaterialQuantityProvisions);
				}

				model.totalIndirectCost = 0;
				foreach (Employee_MCE_Materials_Subitems x in model.materials)
				{
					model.totalIndirectCost += x.MaterialAmount;
				}
				//model.OCM = model.totalIndirectCost * 0.07;
				model.profit = model.totalIndirectCost * 0.07;
				model.subtotalCost = model.totalIndirectCost /*+ model.OCM */+ model.profit;
				model.tax = model.subtotalCost * 0.05;
				model.totalCost = model.subtotalCost + model.tax;
				return View(model);
			}
			else if (submitButton == "Submit")
			{
				using (SqlConnection conn = new SqlConnection(connectionstring))
				{
					conn.Open();
					int id = 0;
					using (SqlTransaction trans = conn.BeginTransaction())
					{
						try
						{
							using (SqlCommand command = new SqlCommand("INSERT INTO mce (bom_id ,mce_markup) VALUES" +
								"(@bom_id,@mce_markup); " +
								"SELECT SCOPE_IDENTITY() FROM mce"))
							{
								command.Parameters.AddWithValue("@bom_id", model.BOMID);
								command.Parameters.AddWithValue("@mce_markup", model.Markup);
								command.Connection = conn;
								command.Transaction = trans;
								id = Convert.ToInt32(command.ExecuteScalar());
							}
							using (SqlCommand command = new SqlCommand("INSERT INTO mce_subitems (material_id, mce_subitem_quantity, mce_id, mce_subitem_labour_cost, supplier_material_id) " +
								"VALUES (@material_id, @mce_subitem_quantity, @mce_id, " +
								"@mce_subitem_labour_cost, @supplier_material_id);"))
							{
								SqlParameter param1 = new SqlParameter("@material_id", DbType.Int32);
								SqlParameter param2 = new SqlParameter("@mce_subitem_quantity", DbType.Decimal);
								SqlParameter param3 = new SqlParameter("@mce_id", DbType.Int32);
								SqlParameter param4 = new SqlParameter("@mce_subitem_labour_cost", DbType.Int32);
								SqlParameter param5 = new SqlParameter("@supplier_material_id", DbType.Int32);

								command.Parameters.Add(param1);
								command.Parameters.Add(param2);
								command.Parameters.Add(param3);
								command.Parameters.Add(param4);
								command.Parameters.Add(param5);
								command.Connection = conn;
								command.Transaction = trans;

								foreach (Employee_MCE_Materials_Subitems x in model.materials)
								{
									param1.Value = x.MaterialID;
									param2.Value = x.MaterialQuantityProvisions;
									param3.Value = id;
									param4.Value = (int)Math.Round(x.LabourCost * 100);
									param5.Value = x.SupplierMaterialID;
									command.ExecuteNonQuery();
								}
							}
							trans.Commit();
						} catch(SqlException e)
						{
							Debug.WriteLine(e.Message);
							trans.Rollback();
						}
					}
				}
				return Redirect("employeeProjectDash");
			}
			
			return View(model);
		}

		public IActionResult MCEAdd_New(int? id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			int bom = 0;
			Employee_MCE model = new Employee_MCE();
			model.templates = new List<Employee_BOM_Template_List>();
			model.materials = new List<Employee_MCE_Materials_Subitems>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM bom a INNER JOIN projects b ON a.project_id = b.project_id " +
					"INNER JOIN template_formula_constants c ON a.bom_formula_id = formula_constants_id " +
					"INNER JOIN building_types d ON b.building_types_id = d.building_types_id " +
					"WHERE a.project_id = @id;"))
				{

					command.Parameters.AddWithValue("@id", (int)id);
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
							model.BuildingType = sdr["description"].ToString();
							model.NumberOfStoreys = Convert.ToInt32(sdr["project_building_storeys"]);
							model.FloorHeight = Convert.ToDouble(sdr["project_building_floorheight"]);
							model.BuildingLength = Convert.ToDouble(sdr["project_building_length"]);
							model.BuildingWidth = Convert.ToDouble(sdr["project_building_width"]);
							model.Longtitude = sdr["project_longtitude"].ToString();
							model.Latitude = sdr["project_latitude"].ToString();
							model.FormulaID = Convert.ToInt32(sdr["bom_formula_id"]);
							model.BOMCreationDate = DateTime.Parse(sdr["bom_creation_date"].ToString());
							model.Wastage = Convert.ToDouble(sdr["wastage"]);
							model.Provisions = Convert.ToDouble(sdr["provisions"]);
							model.ProjectID = Convert.ToInt32(sdr["project_id"]);
							model.BOMID = Convert.ToInt32(sdr["bom_id"]);
							bom = Convert.ToInt32(sdr["bom_id"]);
						}
					}
				}
				Debug.WriteLine(id);

				Debug.WriteLine(bom);
				string destination = model.Latitude + "," + model.Longtitude;
				using (SqlCommand command = new SqlCommand("SELECT e.*, g.measurement_unit_desc_short, f.Quantity FROM ( " +
					"SELECT c.material_id, SUM(d.bom_subitem_quantity) AS Quantity " +
					"FROM materials c " +
					"INNER JOIN (" +
					"SELECT a.material_id, a.bom_subitem_quantity, b.material_desc " +
					"FROM bom_subitems a " +
					"INNER JOIN materials b ON a.material_id = b.material_id " +
					"WHERE bom_item_id IN (" +
					"SELECT bom_item_id FROM bom_items WHERE bom_list_id IN ( " +
					"SELECT bom_list_id FROM bom_lists WHERE bom_id = @id " +
					")" +
					")" +
					") d ON d.material_id = c.material_id " +
					"GROUP BY c.material_id " +
					") f " +
					"INNER JOIN materials e ON f.material_id = e.material_id " +
					"INNER JOIN measurement_units g ON e.measurement_unit_id= g.measurement_unit_id;"))
				{
					command.Parameters.AddWithValue("@id", bom);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						int num = 1;
						while (sdr.Read())
						{
							MaterialsCostComparisonItem tempPrice = GetBestPrice(Convert.ToInt32(sdr["material_id"]), destination);
							model.materials.Add(new Employee_MCE_Materials_Subitems()
							{
								SubitemNumber = num,
								MaterialID = Convert.ToInt32(sdr["material_id"]),
								MaterialDesc = sdr["material_desc"].ToString(),
								MaterialUoM = sdr["measurement_unit_desc_short"].ToString(),
								MaterialQuantity = Convert.ToInt32(sdr["Quantity"]),
								MaterialQuantityWastage = (int)Math.Ceiling(Convert.ToInt32(sdr["Quantity"]) * model.Wastage),
								MaterialQuantityProvisions = (int)Math.Ceiling((Convert.ToInt32(sdr["Quantity"]) * model.Wastage) * model.Provisions),
								MaterialCost = tempPrice.Price,
								MaterialAmount = Math.Round((int)Math.Ceiling(Math.Ceiling(Convert.ToInt32(sdr["Quantity"]) * model.Wastage) * model.Provisions) * tempPrice.Price, 2),
								SupplierMaterialID = tempPrice.SupplierMaterialID,
								Supplier = tempPrice.SupplierID.ToString()
							}) ;
							num++;
						}
					}
				}

			}
			model.totalIndirectCost = 0;
			foreach (Employee_MCE_Materials_Subitems x in model.materials)
			{
				model.totalIndirectCost += (x.MaterialCost * Math.Ceiling((double)x.MaterialQuantity));
			}
			//model.OCM = model.totalIndirectCost * 0.07;
			model.profit = model.totalIndirectCost * 0.07;
			model.subtotalCost = model.totalIndirectCost + model.profit; //+ model.OCM + ;
			model.tax = model.subtotalCost * 0.05;
			model.totalCost = model.subtotalCost + model.tax;

			//TempData["MCEData"] = JsonConvert.SerializeObject(model);
			//return RedirectToAction("MCEAdd");
			return View("MCEAdd", model);
		}


		public IActionResult MCEView (int? id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			Employee_MCE model = new Employee_MCE();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				int mce = 0;
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM mce a " +
					"INNER JOIN bom b ON a.bom_id = b.bom_id " +
					"INNER JOIN projects c ON b.project_id = c.project_id " +
					"INNER JOIN building_types d ON c.building_types_id = d.building_types_id " +
					"INNER JOIN template_formula_constants e ON b.bom_formula_id = e.formula_constants_id " +
					"WHERE c.project_id = @id;"))
				{

					command.Parameters.AddWithValue("@id", (int)id);
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
							model.BuildingType = sdr["description"].ToString();
							model.NumberOfStoreys = Convert.ToInt32(sdr["project_building_storeys"]);
							model.FloorHeight = Convert.ToDouble(sdr["project_building_floorheight"]);
							model.BuildingLength = Convert.ToDouble(sdr["project_building_length"]);
							model.BuildingWidth = Convert.ToDouble(sdr["project_building_width"]);
							model.Longtitude = sdr["project_longtitude"].ToString();
							model.Latitude = sdr["project_latitude"].ToString();
							model.FormulaID = Convert.ToInt32(sdr["bom_formula_id"]);
							model.BOMCreationDate = DateTime.Parse(sdr["bom_creation_date"].ToString());
							model.Wastage = Convert.ToDouble(sdr["wastage"]);
							model.Provisions = Convert.ToDouble(sdr["provisions"]);
							model.ProjectID = Convert.ToInt32(sdr["project_id"]);
							model.BOMID = Convert.ToInt32(sdr["bom_id"]);
							model.Markup = Convert.ToDouble(sdr["mce_markup"]);
							mce = Convert.ToInt32(sdr["mce_id"]);
						}
					}
				}
				model.materials = new List<Employee_MCE_Materials_Subitems>();
				using (SqlCommand command = new SqlCommand("SELECT * FROM mce_subitems a " +
					"INNER JOIN materials b ON a.material_id = b.material_id " +
					"INNER JOIN measurement_units c ON b.measurement_unit_id = c.measurement_unit_id " +
					"INNER JOIN supplier_materials d ON a.supplier_material_id = d.supplier_material_id " +
					"WHERE a.mce_id = @id"))
				{
					command.Parameters.AddWithValue("@id", mce);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while(sdr.Read())
						{
							model.materials.Add(new Employee_MCE_Materials_Subitems()
							{
								MaterialID = Convert.ToInt32(sdr["material_id"]),
								MaterialDesc = sdr["material_desc"].ToString(),
								MaterialUoM = sdr["measurement_unit_desc_short"].ToString(),
								MaterialQuantityProvisions = Convert.ToInt32(sdr["mce_subitem_quantity"]),
								MaterialCost = Convert.ToDouble(sdr["supplier_material_price"]) / 100,
								LabourCost = Convert.ToDouble(sdr["mce_subitem_labour_cost"]) / 100
							});
						}
					}
				}
			}
			for (int x = 0; x < model.materials.Count; x++)
			{
				Debug.WriteLine(Math.Round(((double)model.Markup / 100.00) + 1.00, 2));
				model.materials[x].MarkedUpCost =
					Math.Ceiling(
					model.materials[x].MaterialCost *
					Math.Round((model.Markup / 100) + 1, 2));
				model.materials[x].TotalUnitRate =
					Math.Ceiling(
					model.materials[x].MarkedUpCost +
					model.materials[x].LabourCost);
				model.materials[x].MaterialAmount =
					Math.Ceiling(
					model.materials[x].TotalUnitRate *
					model.materials[x].MaterialQuantityProvisions);
			}

			model.totalIndirectCost = 0;
			foreach (Employee_MCE_Materials_Subitems x in model.materials)
			{
				model.totalIndirectCost += x.MaterialAmount;
			}
			//model.OCM = model.totalIndirectCost * 0.07;
			model.profit = model.totalIndirectCost * 0.07;
			model.subtotalCost = model.totalIndirectCost + /*model.OCM + */model.profit;
			model.tax = model.subtotalCost * 0.05;
			model.totalCost = model.subtotalCost + model.tax;
			return View(model);
		}


		public IActionResult SupplierList()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			List<SupplierListItem> model = new List<SupplierListItem>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM supplier_info a WHERE employee_id = @id;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("EmployeeID"));
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Add(new SupplierListItem()
							{
								ID = Convert.ToInt32(sdr["supplier_id"]),
								Description = sdr["supplier_desc"].ToString(),
								ContactName = sdr["supplier_contact_name"].ToString(),
								ContactNumber = sdr["supplier_contact_number"].ToString()
							});
						}
					}
				}
			}
			return View(model);
		}

		public IActionResult SupplierView(int? id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			SupplierInfoModel model = new SupplierInfoModel();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM supplier_info a " +
					"INNER JOIN user_credentials b ON a.user_credentials_id = b.user_id " +
					"WHERE supplier_id = @id ;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@id", (int)id);
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.ID = Convert.ToInt32(sdr["supplier_id"]);
							model.CredentialsID = Convert.ToInt32(sdr["user_credentials_id"]);
							model.EmployeeID = Convert.ToInt32(sdr["employee_id"]);
							model.EmployeeID = Convert.ToInt32(sdr["employee_id"]);
							model.Description = sdr["supplier_desc"].ToString();
							model.Address = sdr["supplier_address"].ToString();
							model.City = sdr["supplier_city"].ToString();
							model.Region = sdr["supplier_admin_district"].ToString();
							model.Country = sdr["supplier_country"].ToString();
							model.Latitude = sdr["supplier_coordinates_latitude"].ToString();
							model.Longtitude = sdr["supplier_coordinates_longtitude"].ToString();
							model.ContactName = sdr["supplier_contact_name"].ToString();
							model.ContactNumber = sdr["supplier_contact_number"].ToString();
							model.Username = sdr["username"].ToString();
							model.Password = sdr["user_password"].ToString();
							model.Status = Convert.ToBoolean(sdr["user_status"]);
						}
					};
				}
			}
			return View(model);
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public ActionResult SupplierView(SupplierInfoModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}

			TempData["SupplierData"] = JsonConvert.SerializeObject(model);
			return RedirectToAction("SupplierEdit");
		}

		public IActionResult SupplierEdit ()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			SupplierInfoModel model = JsonConvert.DeserializeObject<SupplierInfoModel>(TempData["SupplierData"].ToString());
			TempData["SupplierData"] = JsonConvert.SerializeObject(model);
			return View(model);
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public ActionResult SupplierEdit(SupplierInfoModel model, string submitButton)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			bool error = false;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlTransaction transaction = conn.BeginTransaction())
				{
					try
					{
						using (SqlCommand command = new SqlCommand("UPDATE user_credentials " +
							"SET user_password = @user_password " +
							"WHERE user_id = @user_credentials_id; " +
							"UPDATE supplier_info " +
							"SET supplier_desc = @supplier_desc, supplier_address = @supplier_address, " +
							"supplier_city = @supplier_city, supplier_admin_district = @supplier_admin_district, " +
							"supplier_country = @supplier_country, supplier_coordinates_latitude = @supplier_coordinates_latitude, " +
							"supplier_coordinates_longtitude = @supplier_coordinates_longtitude, supplier_contact_name = @supplier_contact_name, " +
							"supplier_contact_number = @supplier_contact_number " +
							"WHERE supplier_id = @supplier_id;"))
						{
							command.Connection = conn;
							command.Transaction = transaction;

							command.Parameters.AddWithValue("@user_credentials_id", model.CredentialsID);
							command.Parameters.AddWithValue("@user_password", model.Password);
							command.Parameters.AddWithValue("@supplier_id", model.ID);
							command.Parameters.AddWithValue("@supplier_desc", model.Description);
							command.Parameters.AddWithValue("@supplier_address", model.Address);
							command.Parameters.AddWithValue("@supplier_city", model.City);
							command.Parameters.AddWithValue("@supplier_admin_district", model.Region);
							command.Parameters.AddWithValue("@supplier_country", model.Country);
							command.Parameters.AddWithValue("@supplier_coordinates_longtitude", model.Longtitude);
							command.Parameters.AddWithValue("@supplier_coordinates_latitude", model.Latitude);
							command.Parameters.AddWithValue("@supplier_contact_name", model.ContactName);
							command.Parameters.AddWithValue("@supplier_contact_number", model.ContactNumber);

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
				return View(model);
			}

			return RedirectToAction("SupplierView", new { id = model.ID });
		}

		public IActionResult SupplierAdd()
		{
			return View();
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public ActionResult SupplierAdd(SupplierInfoModel model)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			if (!ModelState.IsValid)
			{
				foreach (var x in ModelState.Keys)
				{
					var modelStateEntry = ModelState[x];
					if (modelStateEntry.Errors.Any())
					{
						Debug.WriteLine(modelStateEntry.Errors.Select(e => e.ErrorMessage));
					}
				}
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
				return View(model);
			}
			int supplier = 0;
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
							"VALUES(@username, @password, @user_role, @user_status); SELECT SCOPE_IDENTITY() FROM user_credentials;"))
						{
							command.Connection = conn;
							command.Transaction = transaction;

							command.Parameters.AddWithValue("@username", model.Username);
							command.Parameters.AddWithValue("@password", model.Password);
							command.Parameters.AddWithValue("@user_role", Convert.ToInt32(3));
							command.Parameters.AddWithValue("@user_status", 1);

							info_id = Convert.ToInt32(command.ExecuteScalar());
						}

						using (SqlCommand command = new SqlCommand("INSERT INTO supplier_info (user_credentials_id,employee_id,supplier_desc,supplier_address,supplier_city,supplier_admin_district,supplier_country,supplier_coordinates_latitude,supplier_coordinates_longtitude,supplier_contact_name,supplier_contact_number) " +
							"VALUES (@user_credentials_id,@employee_id, @Desc, @Address, @City, @supplier_admin_district, @supplier_country, @Longtitude, @Latitude, @ContactName, @ContactNumber);" +
							" SELECT SCOPE_IDENTITY() FROM supplier_info;"))
						{
							command.Connection = conn;
							command.Transaction = transaction;

							command.Parameters.AddWithValue("@user_credentials_id", info_id);
							command.Parameters.AddWithValue("@employee_id", HttpContext.Session.GetInt32("EmployeeID"));
							command.Parameters.AddWithValue("@Desc", model.Description);
							command.Parameters.AddWithValue("@Address", model.Address);
							command.Parameters.AddWithValue("@City", model.City);
							command.Parameters.AddWithValue("@supplier_admin_district", model.Region);
							command.Parameters.AddWithValue("@supplier_country", model.Country);
							command.Parameters.AddWithValue("@Longtitude", model.Longtitude);
							command.Parameters.AddWithValue("@Latitude", model.Latitude);
							command.Parameters.AddWithValue("@ContactName", model.ContactName);
							command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber);

							supplier = Convert.ToInt32(command.ExecuteScalar());
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
				return View(model);
			}

			return RedirectToAction("SupplierView", new { id = supplier } );
		}



		public IActionResult employeeAddSupplier()
		{
			return View();
		}

		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public IActionResult employeeAddSupplier(AddSupplierModel model)
		{
			string pattern = @"^09\d{9}$";
			if (model.ContactNumber != null)
			{
				if (!Regex.IsMatch(model.ContactNumber, pattern))
				{
					Debug.WriteLine("2");
					ModelState.AddModelError("ContactNumber", "This number is not valid. Use format \"09#########\".");

				}
			}
			if (model.Password == null)
			{
				ModelState.AddModelError("Password", "Enter a password at least 6 characters long.");
			}
			else if (model.Password != null)
			{
				if (model.Password.Length < 6)
				{
					ModelState.AddModelError("Password", "Enter a password at least 6 characters long.");
				}
			}
			if (!ModelState.IsValid)
			{
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
							"VALUES(@username, @password, @user_role, @user_status); SELECT SCOPE_IDENTITY() FROM user_credentials;"))
						{
							command.Connection = conn;
							command.Transaction = transaction;

							command.Parameters.AddWithValue("@username", model.Username);
							command.Parameters.AddWithValue("@password", PasswordEncryptor.EncryptPassword(model.Password));
							command.Parameters.AddWithValue("@user_role", Convert.ToInt32(3));
							command.Parameters.AddWithValue("@user_status", 1);

							info_id = Convert.ToInt32(command.ExecuteScalar());
						}

						using (SqlCommand command = new SqlCommand("INSERT INTO supplier_info (user_credentials_id,employee_id,supplier_desc,supplier_address,supplier_city,supplier_admin_district,supplier_country,supplier_coordinates_latitude,supplier_coordinates_longtitude,supplier_contact_name,supplier_contact_number) " +
							"VALUES (@user_credentials_id,@employee_id, @Desc, @Address, @City, @supplier_admin_district, @supplier_country, @Longtitude, @Latitude, @ContactName, @ContactNumber);"))
						{
							command.Connection = conn;
							command.Transaction = transaction;

							command.Parameters.AddWithValue("@user_credentials_id", info_id);
							command.Parameters.AddWithValue("@employee_id", HttpContext.Session.GetInt32("EmployeeID"));
							command.Parameters.AddWithValue("@Desc", model.Description);
							command.Parameters.AddWithValue("@Address", model.Address);
							command.Parameters.AddWithValue("@City", model.City);
							command.Parameters.AddWithValue("@supplier_admin_district", model.AdminDistrict);
							command.Parameters.AddWithValue("@supplier_country", model.Country);
							command.Parameters.AddWithValue("@Longtitude", model.Longtitude);
							command.Parameters.AddWithValue("@Latitude", model.Latitude);
							command.Parameters.AddWithValue("@ContactName", model.ContactName);
							command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber);

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
				return View(model);
			}

			return RedirectToAction("employeeSupplierDash", "Employee");
		}



		public IActionResult employeeDashboard()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			NewEmployeeDashboardModel model = new NewEmployeeDashboardModel();
			model.projects = new List<EmployeeDashboardProjects>();

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT " +
					"(SELECT COUNT(a.project_id) as pending FROM projects a " +
					"LEFT JOIN bom b ON a.project_id = b.project_id " +
					"LEFT JOIN mce c ON b.bom_id = c.bom_id " +
					"WHERE a.project_engineer_id = @id " +
					"AND c.mce_id IS NOT NULL) AS processed, " +
					"(SELECT COUNT(a.project_id) as pending FROM projects a " +
					"LEFT JOIN bom b ON a.project_id = b.project_id " +
					"LEFT JOIN mce c ON b.bom_id = c.bom_id " +
					"WHERE a.project_engineer_id = @id " +
					"AND c.mce_id IS NULL) AS Pending, " +
					"(SELECT COUNT(a.project_id) as pending FROM projects a " +
					"WHERE a.project_engineer_id = @id) AS total;"))
				{
					command.Parameters.AddWithValue("@id", (int)HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.NumOfPending = Convert.ToInt32(sdr["Pending"]);
							model.NumOfProjects= Convert.ToInt32(sdr["total"]);
							model.NumOfComplete = Convert.ToInt32(sdr["processed"]);
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT a.* FROM projects a " +
					"LEFT JOIN bom b ON a.project_id = b.project_id " +
					"WHERE a.project_engineer_id = @id " +
					"AND b.bom_id IS NULL " +
					"ORDER BY a.project_id DESC;"))
				{
					command.Parameters.AddWithValue("@id", (int)HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.projects.Add(new EmployeeDashboardProjects()
							{
								ID = Convert.ToInt32(sdr["project_id"]),
								Description = sdr["project_title"].ToString()
							});
						}
					}
				}
			}

			return View(model);
		}

		public List<ClientDataModel> getProjectList(int page, int pageSize)
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
					", b.bom_id, c.mce_id " +
					"FROM projects a " +
					"LEFT JOIN bom b ON a.project_id = b.project_id " +
					"LEFT JOIN mce c ON b.bom_id = c.bom_id " +
					"LEFT JOIN mce_subitems d ON c.mce_id = d.mce_id " +
					"LEFT JOIN supplier_materials e ON d.supplier_material_id = e.supplier_material_id " +
					"INNER JOIN building_types f ON f.building_types_id = a.building_types_id " +
					"INNER JOIN employee_info g ON a.project_engineer_id = g.employee_info_id " +
					"WHERE a.project_engineer_id = @id " +
					"GROUP BY a.project_id, a.project_date, a.project_client_name, a.project_title, f.description, a.project_city, b.bom_id, c.mce_id, " +
					"g.employee_info_firstname, g.employee_info_middlename, g.employee_info_lastname  " +
					"ORDER BY a.project_id DESC OFFSET (@pagesize * (@pagenumber - 1)) ROWS FETCH NEXT @pagesize ROWS ONLY;"))
				{
					command.Parameters.AddWithValue("@pagesize", pageSize);
					command.Parameters.AddWithValue("@pagenumber", page);
					command.Parameters.AddWithValue("@id", (int)HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							double amount = 0;
							int bom = 0, mce = 0;
							if (!Convert.IsDBNull(sdr["price"]))
							{
								amount = Convert.ToDouble(sdr["price"]) / 100;
							}
							if (!Convert.IsDBNull(sdr["bom_id"]))
							{
								bom = Convert.ToInt32(sdr["bom_id"]);
							}
							if (!Convert.IsDBNull(sdr["mce_id"]))
							{
								mce = Convert.ToInt32(sdr["mce_id"]);
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
								Status = Convert.ToInt32(sdr["project_status"]),
								BOMID = bom,
								MCEID = mce
							});
						}
					}
				}
			}
			return model;
		}

		public IActionResult employeeProjectDash(int page = 1, int pageSize = 10)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			List<ClientDataModel> model = getProjectList(page, pageSize);

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT COUNT(project_id)  FROM projects WHERE project_engineer_id = @id;"))
				{
					command.Parameters.AddWithValue("@id", (int)HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					double numOfRecords = Convert.ToDouble(command.ExecuteScalar());
					ViewBag.Pages = Convert.ToInt32(Math.Ceiling(Math.Ceiling(numOfRecords) / pageSize));
					ViewBag.Page = page;
					ViewBag.PageSize = pageSize;
				}
				conn.Close();
			}

			return View(model);
		}


		//AJAX
		public ActionResult getPageData(int page = 1, int pageSize = 10)
		{
			List<ClientDataModel> model = getProjectList(page, pageSize);
			return Json(model);

		}

		//AJAX
		public ActionResult searchProject(string search = "")
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
					"WHERE a.project_engineer_id = @id AND a.project_title LIKE @title " +
					"GROUP BY a.project_id, a.project_date, a.project_client_name, a.project_title, f.description, a.project_city, b.bom_id, c.mce_id, " +
					"g.employee_info_firstname, g.employee_info_middlename, g.employee_info_lastname " +
					"ORDER BY a.project_id DESC;"))
				{
					command.Parameters.AddWithValue("@title", "%" + search + "%");
					command.Parameters.AddWithValue("@id", (int)HttpContext.Session.GetInt32("EmployeeID"));
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
			return Json(model);
		}

		public IActionResult employeeProjectView(int? id)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
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

		public IActionResult employeeSupplierDash(int page =1, int pageSize = 10)
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("SessionExpired", "Home");
			}
			List<AdminSupplierModel> model = getSupplierList(page, pageSize);
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT COUNT(supplier_id)  FROM supplier_info WHERE employee_id = @id ;"))
				{
					command.Parameters.AddWithValue("@id", (int)HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					double numOfRecords = Convert.ToDouble(command.ExecuteScalar());
					ViewBag.Pages = Convert.ToInt32(Math.Ceiling(Math.Ceiling(numOfRecords) / pageSize));
					ViewBag.Page = page;
					ViewBag.PageSize = pageSize;
				}
			}
			return View(model);
		}


		public List<AdminSupplierModel> getSupplierList(int page, int pageSize)
		{
			List<AdminSupplierModel> model = new List<AdminSupplierModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT * FROM supplier_info WHERE employee_id = @id " +
					"ORDER BY supplier_id " +
					"OFFSET (@pagesize * (@pagenumber - 1)) ROWS " +
					"FETCH NEXT @pagesize ROWS ONLY;"))
				{
					command.Parameters.AddWithValue("@pagesize", pageSize);
					command.Parameters.AddWithValue("@pagenumber", page);
					command.Parameters.AddWithValue("@id", (int)HttpContext.Session.GetInt32("EmployeeID"));
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
			return model;
		}

		//AJAX
		public ActionResult getSupplierPageData(int page = 1, int pageSize = 10)
		{
			List<AdminSupplierModel> model = getSupplierList(page, pageSize);
			return Json(model);

		}

		//AJAX
		public ActionResult searchSupplier(string search = "")
		{
			List<AdminSupplierModel> model = new List<AdminSupplierModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand(
					"SELECT * FROM supplier_info WHERE employee_id = @id AND supplier_desc LIKE @title;"))
				{
					command.Parameters.AddWithValue("@title", "%" + search + "%");
					command.Parameters.AddWithValue("@id", (int)HttpContext.Session.GetInt32("EmployeeID"));
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
			return Json(model);
		}

		public SupplierInfoModel getSupplierInfo(int? id)
		{
			SupplierInfoModel model = new SupplierInfoModel();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM supplier_info a " +
					"INNER JOIN user_credentials b " +
					"ON a.user_credentials_id = b.user_id " +
					"WHERE supplier_id = @id;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@id", id);

					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.ID = Convert.ToInt32(sdr["supplier_id"]);
							model.CredentialsID = Convert.ToInt32(sdr["user_credentials_id"]);
							model.EmployeeID = Convert.ToInt32(sdr["employee_id"]);
							model.Latitude = sdr["supplier_coordinates_latitude"].ToString();
							model.Longtitude = sdr["supplier_coordinates_longtitude"].ToString();
							model.Username = sdr["username"].ToString();
							//model.Password = sdr["user_password"].ToString();
							model.Description = sdr["supplier_desc"].ToString();
							model.ContactName = sdr["supplier_contact_name"].ToString();
							model.ContactNumber = sdr["supplier_contact_number"].ToString();
							model.Address = sdr["supplier_address"].ToString();
							model.City = sdr["supplier_city"].ToString();
							model.Region = sdr["supplier_admin_district"].ToString();
							model.Country = sdr["supplier_country"].ToString();
							model.Status = Convert.ToBoolean(sdr["user_status"]);
						}
					}
				}
			}
			Debug.WriteLine(model.ID);
			Debug.WriteLine(model.Username);
			Debug.WriteLine(model.Description);
			return model;
		}

		public IActionResult employeeSupplierView(int? id)
		{
			SupplierInfoModel model = getSupplierInfo(id);

			return View(model);
		}


		public IActionResult supplierInfoEdit(int? id)
		{
			SupplierInfoModel model = getSupplierInfo(id);

			return View(model);
		}


		[DisableRequestSizeLimit]
		[HttpPost]
		[AllowAnonymous]
		public IActionResult supplierInfoEdit(SupplierInfoModel model)
		{
			string pattern = @"^09\d{9}$";
			if (model.ContactNumber != null)
			{
				if (!Regex.IsMatch(model.ContactNumber, pattern))
				{
					Debug.WriteLine("2");
					ModelState.AddModelError("ContactNumber", "This number is not valid. Use format \"09#########\".");

				}
			}
			if (model.Password != null)
			{
				if (model.Password.Length > 0 && model.Password.Length < 6)
				{
					ModelState.AddModelError("Password", "Enter a password at least 6 characters long.");
				}
			}
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				if (model.Password == null || model.Password.Length == 0)
				{
					using (SqlCommand command = new SqlCommand("UPDATE supplier_info SET " +
					"supplier_desc = @suppleir_desc, " +
					"supplier_address = @supplier_address, " +
					"supplier_city = @supplier_city, " +
					"supplier_admin_district = @supplier_admin_district, " +
					"supplier_country = @supplier_country, " +
					"supplier_coordinates_latitude = @supplier_coordinates_latitude, " +
					"supplier_coordinates_longtitude = @supplier_coordinates_longtitude, " +
					"supplier_contact_name = @supplier_contact_name, " +
					"supplier_contact_number = @supplier_contact_number " +
					"WHERE supplier_id = @supplier_id;" +
					"UPDATE user_credentials SET " +
					"user_status = @user_status " +
					"WHERE user_id = @user_id;"))
					{
						command.Connection = conn;
						command.Parameters.AddWithValue("@supplier_id", model.ID);
						command.Parameters.AddWithValue("@user_id", model.CredentialsID);
						command.Parameters.AddWithValue("@suppleir_desc", model.Description);
						command.Parameters.AddWithValue("@supplier_contact_name", model.ContactName);
						command.Parameters.AddWithValue("@supplier_contact_number", model.ContactNumber);
						command.Parameters.AddWithValue("@supplier_address", model.Address);
						command.Parameters.AddWithValue("@supplier_city", model.City);
						command.Parameters.AddWithValue("@supplier_admin_district", model.Region);
						command.Parameters.AddWithValue("@supplier_country", model.Country);
						command.Parameters.AddWithValue("@supplier_coordinates_latitude", model.Latitude);
						command.Parameters.AddWithValue("@supplier_coordinates_longtitude", model.Longtitude);
						command.Parameters.AddWithValue("@user_status", model.Status);
						command.ExecuteNonQuery();
						Debug.WriteLine("N");
					}
				}
				else if (model.Password.Length != 0 && model.Password.Length > 5)
				{
					using (SqlCommand command = new SqlCommand("UPDATE supplier_info SET " +
					"supplier_desc = @suppleir_desc, " +
					"supplier_address = @supplier_address, " +
					"supplier_city = @supplier_city, " +
					"supplier_admin_district = @supplier_admin_district, " +
					"supplier_country = @supplier_country, " +
					"supplier_coordinates_latitude = @supplier_coordinates_latitude, " +
					"supplier_coordinates_longtitude = @supplier_coordinates_longtitude, " +
					"supplier_contact_name = @supplier_contact_name, " +
					"supplier_contact_number = @supplier_contact_number " +
					"WHERE supplier_id = @supplier_id;" +
					"UPDATE user_credentials SET " +
					"user_password = @user_password, " +
					"user_status = @user_status " +
					"WHERE user_id = @user_id;"))
					{
						command.Connection = conn;
						command.Parameters.AddWithValue("@supplier_id", model.ID);
						command.Parameters.AddWithValue("@user_id", model.CredentialsID);
						command.Parameters.AddWithValue("@suppleir_desc", model.Description);
						command.Parameters.AddWithValue("@user_password", PasswordEncryptor.EncryptPassword(model.Password));
						command.Parameters.AddWithValue("@supplier_contact_name", model.ContactName);
						command.Parameters.AddWithValue("@supplier_contact_number", model.ContactNumber);
						command.Parameters.AddWithValue("@supplier_address", model.Address);
						command.Parameters.AddWithValue("@supplier_city", model.City);
						command.Parameters.AddWithValue("@supplier_admin_district", model.Region);
						command.Parameters.AddWithValue("@supplier_country", model.Country);
						command.Parameters.AddWithValue("@supplier_coordinates_latitude", model.Latitude);
						command.Parameters.AddWithValue("@supplier_coordinates_longtitude", model.Longtitude);
						command.Parameters.AddWithValue("@user_status", model.Status);
						command.ExecuteNonQuery();
						Debug.WriteLine("P");
					}
				}
			}
			return RedirectToAction("employeeSupplierView", new { id = model.ID });
		}







		public Employee_BOM_Materials_Subitems GetMaterial(int material_id, double Quantity, string destination, int subitem_num, double cost, double wastage, double provisions, int SupplierMaterialID)
		{
			Employee_BOM_Materials_Subitems item = new Employee_BOM_Materials_Subitems();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT a.material_id, a.material_desc, b.measurement_unit_desc_short, " +
					"(SELECT supplier_id FROM supplier_materials a WHERE a.supplier_material_id = @supplier_material_id) as supplier_id " +
					"FROM materials a " +
					" INNER JOIN measurement_units b " +
					" ON a.measurement_unit_id = b.measurement_unit_id " +
					" WHERE material_id = @material_id;"))
				{
					command.Parameters.AddWithValue("@material_id", material_id);
					command.Parameters.AddWithValue("@supplier_material_id", SupplierMaterialID);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							item.SubitemNumber = subitem_num;
							item.MaterialID = Convert.ToInt32(sdr["material_id"]);
							item.MaterialDesc = sdr["material_desc"].ToString();
							item.MaterialUoM = sdr["measurement_unit_desc_short"].ToString();
							item.MaterialQuantity = (int)Math.Ceiling(Quantity);
							item.MaterialQuantityWastage = (int)Math.Ceiling(Math.Ceiling(Quantity) * wastage);
							item.MaterialQuantityProvisions = (int)Math.Ceiling(Math.Ceiling(Math.Ceiling(Quantity) * wastage)*provisions);
							item.MaterialCost = cost;
							item.MaterialAmount = Math.Ceiling(Math.Ceiling(Math.Ceiling(Quantity) * wastage) * provisions) * cost;
							item.SupplierMaterialID = SupplierMaterialID;
							item.SupplierID = sdr["supplier_id"].ToString();
							item.Supplier = "0";
						}
					}
				}
			}
			return item;
		}


		private MaterialsCostComparisonItem GetBestPrice(int MaterialID, string destination)
		{
			string _apikey = "ApFkiZUGSuNuTphyHstPFnkvL0IGwOKelabezyQVt4RwYTD-yE5n5dMgmeHugQgN";

			List<MaterialsCostComparisonItem> MaterialsCosts = new List<MaterialsCostComparisonItem>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT c.material_id AS MaterialID, c.material_desc_long AS Material, a.supplier_material_id AS SupplierMaterialID, " +
					"a.supplier_material_price AS Price, d.supplier_id AS SupplierID, d.supplier_desc AS Supplier, d.employee_id AS Employee, " +
					"CONCAT(d.supplier_coordinates_latitude, ',', d.supplier_coordinates_longtitude) AS Coordinates " +
					"FROM supplier_materials a LEFT JOIN (" +
					"SELECT MIN(b.supplier_material_price) AS min_value FROM supplier_materials b WHERE b.material_id = @id AND b.supplier_material_availability = 1  " +
					"AND b.supplier_material_archived = 0) min_table " +
					"ON a.supplier_material_price = min_table.min_value " +
					"INNER JOIN materials c ON a.material_id = c.material_id " +
					"INNER JOIN supplier_info d ON a.supplier_id = d.supplier_id " +
					"WHERE a.material_id = @id AND d.employee_id = @employee_id  " +
					"AND a.supplier_material_availability = 1 AND a.supplier_material_archived = 0 ;"))
				{
					command.Parameters.AddWithValue("@id", MaterialID);
					command.Parameters.AddWithValue("@employee_id", (int)HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							MaterialsCosts.Add(new MaterialsCostComparisonItem()
							{
								MaterialID = Convert.ToInt32(sdr["MaterialID"]),
								Description_Long = sdr["Material"].ToString(),
								Price = Convert.ToDouble(sdr["Price"]) / 100,
								SupplierID = Convert.ToInt32(sdr["SupplierID"]),
								SupplierDesc = sdr["Supplier"].ToString(),
								SupplierCoords = sdr["Coordinates"].ToString(),
								SupplierMaterialID = Convert.ToInt32(sdr["SupplierMaterialID"])
							});

						}
					}
				}
				conn.Close();
			}
			Debug.WriteLine("AAA");

			Debug.WriteLine(destination);
			Debug.WriteLine(MaterialsCosts[0].SupplierCoords);


			if (MaterialsCosts.Count > 1)
			{
				Debug.WriteLine("BBB");
				List<string> coords = new List<string>();
				foreach (MaterialsCostComparisonItem x in MaterialsCosts)
				{
					coords.Add(x.SupplierCoords);
				}
				Debug.WriteLine(coords.Count);
				foreach (string s in coords)
				{
					Debug.WriteLine(s);
				}
				Debug.WriteLine("d: " + destination);
				BingMapsService bing = new BingMapsService(_apikey);
				List<double> distances = bing.GetDistancesAsync(coords, destination).Result;


				int lowestIndex = 0;
				double lowestValue = distances[0];

				for (int i = 0; i < MaterialsCosts.Count; i++)
				{
					MaterialsCosts[i].Distance = distances[0];
					if (distances[0] < lowestValue)
					{
						lowestIndex = i;
						lowestValue = distances[0];
					}
				}

				return MaterialsCosts[lowestIndex];
			}
			else
			{
				Debug.WriteLine("CCC");
				return MaterialsCosts[0];
			}
		}


		public string getContractorName()
		{
			string name = "";
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM employee_info b WHERE employee_info_id = @employee_info_id;;"))
				{
					command.Parameters.AddWithValue("@employee_info_id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							name = sdr["employee_info_firstname"].ToString() + " " + sdr["employee_info_middlename"].ToString().ToUpper()[0] + ". " + sdr["employee_info_lastname"].ToString();
						}
					}
				}
			}
			return name;
		}


		public ActionResult DownloadMCE(Employee_MCE model)
		{
			/*
			string fileStoragePath = System.IO.Path.Combine(_hostingEnvironment.ContentRootPath, "FileStorage");
			Directory.CreateDirectory(fileStoragePath);
			string fileName = Guid.NewGuid() + System.IO.Path.GetExtension("AAA.pdf");
			string filePath = System.IO.Path.Combine(fileStoragePath, fileName);
			*/
			MemoryStream memoryStream = new MemoryStream();

			PdfWriter pdfWriter = new PdfWriter(memoryStream);
				
			PageSize pageSize = PageSize.A4;
			PdfDocument pdfDoc = new PdfDocument(pdfWriter);
			pdfDoc.SetDefaultPageSize(pageSize);
			Document doc = new Document(pdfDoc);
			

				doc.SetMargins(36, 36, 72, 36);

				Paragraph p = new Paragraph();
				p.Add(new Text(""));
				p.SetMarginTop(30);
				doc.Add(p);

				float availableWidth = doc.GetPageEffectiveArea(pdfDoc.GetDefaultPageSize()).GetWidth();

				Table table2 = new Table(UnitValue.CreatePointArray(new float[] { availableWidth / 3, (availableWidth / 3) * 2 }));
				table2.SetBorder(Border.NO_BORDER);

				Paragraph p2 = new Paragraph();
				p2.Add(new Text("Date:"));
				p2.SetFontSize(10);
				Cell cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text(model.Date.ToString("dd MMMM, yyyy")));
				p2.SetFontSize(10);
				p2.SetBold();
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Subject:"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Materials Cost Estimate"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Project Title:"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text(model.Title));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Client Name:"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text(model.ClientName));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Location:"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text(model.Address + ", " + model.City + ", " + model.Region + ", " + model.Country));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Mode of Implementation:"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("By Contract"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Amount:"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Php " + Math.Round(model.totalCost, 2).ToString()));
				p2.SetFontSize(10);
				p2.SetBold();
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("Building Area:"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text((model.BuildingWidth * model.BuildingLength).ToString("N") + " Square Metres"));
				p2.SetFontSize(10);
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				doc.Add(table2);

				p = new Paragraph();
				p.Add(new Text(""));
				p.SetMarginTop(30);
				doc.Add(p);

				Table table = new Table(UnitValue.CreatePointArray(new float[] {
				availableWidth / 8, availableWidth / 8, availableWidth / 8, availableWidth / 8,
				availableWidth / 8, availableWidth / 8, availableWidth / 8, availableWidth / 8 }));

				Cell headerCell = new Cell(1, 8);
				headerCell.SetTextAlignment(TextAlignment.CENTER);
				Paragraph headerParagraph = new Paragraph("Materials Cost Estimate");
				headerParagraph.SetBold();
				headerCell.Add(headerParagraph);

				table.AddHeaderCell(headerCell);
				table.AddHeaderCell(createCellCentred("Material Name", 10, true));
				table.AddHeaderCell(createCellCentred("Quantity", 10, true));
				table.AddHeaderCell(createCellCentred("UoM", 10, true));
				table.AddHeaderCell(createCellCentred("Cost per Unit", 10, true));
				table.AddHeaderCell(createCellCentred("Marked Up Cost", 10, true));
				table.AddHeaderCell(createCellCentred("Labour Cost", 10, true));
				table.AddHeaderCell(createCellCentred("Total Unit Rate", 10, true));
				table.AddHeaderCell(createCellCentred("Total Amount", 10, true));

				foreach (Employee_MCE_Materials_Subitems x in model.materials)
				{
					table.AddCell(createCell(x.MaterialDesc, 10));
					table.AddCell(createCell(x.MaterialQuantityProvisions.ToString("N"), 10));
					table.AddCell(createCell(x.MaterialUoM, 10));
					table.AddCell(createCell("Php " + x.MaterialCost.ToString("N"), 10));
					table.AddCell(createCell("Php " + x.MarkedUpCost.ToString("N"), 10));
					table.AddCell(createCell("Php " + x.LabourCost.ToString("N"), 10));
					table.AddCell(createCell("Php " + x.TotalUnitRate.ToString("N"), 10));
					table.AddCell(createCell("Php " + x.MaterialAmount.ToString("N"), 10));
				}
				doc.Add(table);

				Table table4 = new Table(UnitValue.CreatePointArray(new float[] {
				(availableWidth / 8) * 6, (availableWidth / 8) * 2 }));

				table4.AddCell(createCellRight("Total Cost of Items:", 10));
				table4.AddCell(createCell("Php " + model.totalIndirectCost.ToString("N"), 10, true));
				table4.AddCell(createCellRight("Profit:", 10));
				table4.AddCell(createCell("Php " + model.profit.ToString("N"), 10));
				table4.AddCell(createCellRight("Sub-Total:", 10));
				table4.AddCell(createCell("Php " + model.subtotalCost.ToString("N"), 10, true));
				table4.AddCell(createCellRight("Vat(5%)", 10));
				table4.AddCell(createCell("Php " + model.tax.ToString("N"), 10));
				table4.AddCell(createCellRight("Total Estimated Cost", 10));
				table4.AddCell(createCell("Php " + model.totalCost.ToString("N"), 10, true));

				doc.Add(table4);


				p = new Paragraph();
				p.Add(new Text(""));
				p.SetMarginTop(30);
				doc.Add(p);

				Table table3 = new Table(UnitValue.CreatePointArray(new float[] { availableWidth / 3 }));
				table3.AddCell(createCellBorderless("Prepared by:", 10));
				table3.AddCell(createCellBorderlessCentredUnderline(getContractorName(), 10, true));
				table3.AddCell(createCellBorderlessCentred("Engineer", 10));
				doc.Add(table3);

			//Debug.WriteLine(memoryStream.Length);

			//Debug.WriteLine(pdfWriter.Length);
			//pdfWriter.CopyTo(memoryStream);

			//Debug.WriteLine(memoryStream.Length);
			// Document elements insertion code
			doc.Flush();
			doc.Close();
			byte[] baos = memoryStream.ToArray();
			//memoryStream.Position = 0;
					return File(baos, "application/pdf", "MCE" + model.Date.ToString("dd_MMMM_yyyy") + ".pdf");
				
				if (true)
				{
					/* Specify the file path and content type
					string contentType = "application/pdf"; // Change the content type as needed
					var fs = System.IO.File.OpenRead(filePath);
					 Return the file for download*/
				}
			return View(model);
		}


		private Cell createCell(string text, float size)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			Cell cell = new Cell();
			cell.Add(p2);
			return cell;
		}

		private Cell createCell(string text, float size, bool isBold)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			p2.SetBold();
			Cell cell = new Cell();
			cell.Add(p2);
			return cell;
		}

		private Cell createCellCentred(string text, float size)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			p2.SetTextAlignment(TextAlignment.CENTER);
			Cell cell = new Cell();
			cell.Add(p2);
			return cell;
		}

		private Cell createCellRight(string text, float size)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			p2.SetTextAlignment(TextAlignment.RIGHT);
			Cell cell = new Cell();
			cell.Add(p2);
			return cell;
		}

		private Cell createCellCentred(string text, float size, bool isBold)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			p2.SetBold();
			p2.SetTextAlignment(TextAlignment.CENTER);
			Cell cell = new Cell();
			cell.Add(p2);
			return cell;
		}

		private Cell createCellBorderless(string text, float size)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			Cell cell = new Cell();
			cell.Add(p2);
			cell.SetBorder(Border.NO_BORDER);
			return cell;
		}

		private Cell createCellBorderless(string text, float size, bool isBold)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			p2.SetBold();
			Cell cell = new Cell();
			cell.Add(p2);
			cell.SetBorder(Border.NO_BORDER);
			return cell;
		}

		private Cell createCellBorderlessCentred(string text, float size)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			p2.SetTextAlignment(TextAlignment.CENTER);
			Cell cell = new Cell();
			cell.Add(p2);
			cell.SetBorder(Border.NO_BORDER);
			return cell;
		}

		private Cell createCellBorderlessCentred(string text, float size, bool isBold)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			p2.SetBold();
			p2.SetTextAlignment(TextAlignment.CENTER);
			Cell cell = new Cell();
			cell.Add(p2);
			cell.SetBorder(Border.NO_BORDER);
			return cell;
		}

		private Cell createCellBorderlessCentredUnderline(string text, float size, bool isBold)
		{
			Paragraph p2 = new Paragraph();
			p2.Add(new Text(text));
			p2.SetFontSize(size);
			p2.SetBold();
			p2.SetUnderline();
			p2.SetTextAlignment(TextAlignment.CENTER);
			Cell cell = new Cell();
			cell.Add(p2);
			cell.SetBorder(Border.NO_BORDER);
			return cell;
		}


		public ActionResult CheckUsernameExisting(string input)
		{
			// Perform logic to get the multiplier based on selectedValue
			// For example, you can query a database or use any other data source
			StringBuilder username = new StringBuilder();

			int user_id = 0;

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT IDENT_CURRENT('user_credentials') AS NextAvailableID;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							user_id = Convert.ToInt32(sdr["NextAvailableID"]) + 1;
						}
					}
				}
			}
			for (int x = 0; x < 6 - user_id.ToString().Length; x++)
			{
				username.Append("0");
			}
			username.Append(user_id);

			return Json(username.ToString());
		}
	}


}

