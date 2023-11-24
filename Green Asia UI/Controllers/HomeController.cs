﻿using Green_Asia_UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.Metrics;
using Newtonsoft.Json;
using Microsoft.Build.ObjectModelRemoting;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.Runtime.Serialization;
//using Google.Protobuf.Collections;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Drawing.Text;
using Microsoft.AspNetCore.RateLimiting;

using System.Security.Cryptography;
//using Sql.Data.SqlClient;
//using SqlX.XDevAPI;

using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Green_Asia_UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly string oldconnectionstring = "Data Source=localhost;port=3306;Initial Catalog=bom_mce_db;User Id=root;password=password123;";
		//private readonly string connectionstring = @"Server=LAPTOP-HJA4M31O\SQLEXPRESS;Database=bom_mce_db;User Id=bom_debug;Password=password123;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";
		private readonly string connectionstring = @"Server=68.71.129.120,1533;Database=bomgreen_db;User Id=bomgreen;Password=~Ni94tt39;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";

		public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult HomePage()
        {
            return View();
		}

		public IActionResult SessionExpired()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult HomePage(LoginViewModel model)
		{
			Debug.WriteLine("a");
			
			if (!ModelState.IsValid)
			{
				Debug.WriteLine("a");
				return View(model);
			}
			
			int role = 0;
			int id = 0;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM user_credentials WHERE username = @username AND user_status = 1;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@username", model.Username);
					command.Parameters.AddWithValue("@password", model.Password);
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						Debug.WriteLine("a");
						if (!sdr.Read())
						{
							ModelState.AddModelError("ValidationSummary", "Username or password is invalid");
							return View(model);
						}
						else
						{
							if (PasswordEncryptor.VerifyPassword(model.Password, sdr["user_password"].ToString()))
							{
								role = Convert.ToInt32(sdr["user_role"]);
								id = Convert.ToInt32(sdr["user_id"]);
								Debug.WriteLine("id");
								HttpContext.Session.SetInt32("UserID", Convert.ToInt32(sdr["user_id"])); //.Session["UserID"] = Convert.ToInt32(sdr["user_id"]);
							}
							else
							{
								ModelState.AddModelError("Password", "Password is invalid");
								return View(model);
							}
						}
					}
				}

				if (role == 1) //admin
				{
					using (SqlCommand command = new SqlCommand("SELECT employee_info_id FROM employee_info WHERE user_credentials_id = @user_credentials_id;"))
					{
						command.Connection = conn;
						command.Parameters.AddWithValue("@user_credentials_id", id);
						HttpContext.Session.SetInt32("EmployeeID", Convert.ToInt32(command.ExecuteScalar()));
					}
					HttpContext.Session.SetInt32("UserRole", 1);
					Debug.WriteLine("admin");
					return RedirectToAction("adminDashboard", "Admin");
				}
				else if (role == 2) //employee
				{
					using (SqlCommand command = new SqlCommand("SELECT employee_info_id FROM employee_info WHERE user_credentials_id = @user_credentials_id;"))
					{
						command.Connection = conn;
						command.Parameters.AddWithValue("@user_credentials_id", id);
						HttpContext.Session.SetInt32("EmployeeID", Convert.ToInt32(command.ExecuteScalar()));
					}
					Debug.WriteLine("emp");
					HttpContext.Session.SetInt32("UserRole", 2);
					return RedirectToAction("employeeDashboard", "Employee");
				}
				else if (role == 3) //supplier
				{
					using (SqlCommand command = new SqlCommand("SELECT supplier_id FROM supplier_info WHERE user_credentials_id = @user_credentials_id;"))
					{
						command.Connection = conn;
						command.Parameters.AddWithValue("@user_credentials_id", id);
						HttpContext.Session.SetInt32("SupplierID", Convert.ToInt32(command.ExecuteScalar()));
					}
					HttpContext.Session.SetInt32("UserRole", 3);
					Debug.WriteLine("sup");
					return RedirectToAction("supplierDashboard", "Supplier");
				}
				conn.Close();
			}
			return View(model);
		}

		public IActionResult Account()
        {
            return View();
        }

        public IActionResult BillOfMaterials()
        {
			TempData["BOMModel"] = null;
			BOMProjectsList model = new BOMProjectsList();
			model.projects = new List<BOMProjectsListItem>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM bom WHERE bom_project_engineer_id = @id;"))
				{
					command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("EmployeeID"));
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.projects.Add(new BOMProjectsListItem()
							{
								id = Convert.ToInt32(sdr["bom_id"]),
								title = sdr["bom_project_title"].ToString(),
								date = DateTime.Parse(sdr["bom_project_date"].ToString())
							});
						}
					}
				}
			}
			return View(model);  
        }

        public IActionResult GenerateBOM()
        {
			GenerateBOMModel model = new GenerateBOMModel();
			
			

			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> GenerateBOM(GenerateBOMModel model)
		{

			List<CategoryList> Categories = GetCategoriesFromDB();
			List<MaterialsListModel> Materials = GetMaterialsFromDB();
			List<MeasurementList> Measurements = GetMeasurementsFromDB();

			if (!ModelState.IsValid)
			{

				return View(model);
			}

			double BuildingArea = model.BuildingLength * model.BuildingWidth;
			double floorThickness = 0.127,
					wallThickness = 0.254,
					rebarConstant = 0.15,
					nailConstant = 20f,
					hollowBlockConstant = 12.5,
					supportBeamLength = 0.25,
					supportBeamWidth = 0.30,
					supportBeamArea = supportBeamLength * supportBeamWidth,
					supportBeamSpace = 2.92f,
					supportBeamVolume = supportBeamArea * model.FloorHeight,
					supportBeamsNeeded = BuildingArea / supportBeamSpace,
					concreteFormulaCement = 1,
					concreteFormulaSand = 2,
					concreteFormulaAggregate = 3,
					plywoodLength = 96,
					plywoodWidth = 48,
					plywoodArea = plywoodLength * plywoodWidth,
					plywoodSheetsPerSqm = (float)Math.Ceiling(10764 / plywoodArea),
					riserHeight = 0.178,
					threadDepth = 0.2794,
					numberOfSteps = (float)Math.Ceiling(model.FloorHeight / riserHeight);

			Debug.WriteLine(supportBeamArea);
			Debug.WriteLine(supportBeamVolume);
			Debug.WriteLine(supportBeamsNeeded);

			double foundationHeight, foundationVolume, foundationPerimeter, foundationWallArea, foundationNoOfHollowBlock, foundationRebar, foundationConcrete;
			double storeyHeight, storeyPerimeter, storeyWallVolume, storeyFloorVolume, storeyFloorPlywood=0, storeyFloorNails=0, storeyFloorConcrete = 0, storeyFloorRebar = 0;
			double storeySupportBeamsNeeded, storeySupportBeamsConcrete, storeySupportBeamsRebar;
			double storeyWallConcrete, storeyWallRebar, stairsVolume, stairsConcrete, stairsRebar;

			double totalConcrete, totalBlocks, totalRebar, totalPlywood, totalNails;

			foundationHeight = model.FloorHeight * model.NumberOfStoreys + (model.NumberOfStoreys * floorThickness);
			foundationVolume = foundationHeight * BuildingArea;
			foundationPerimeter = 2 * (model.BuildingWidth + model.BuildingLength);
			foundationWallArea = 4 * foundationPerimeter * foundationHeight;
			foundationNoOfHollowBlock = foundationWallArea * hollowBlockConstant;
			foundationRebar = rebarConstant * foundationVolume;
			foundationConcrete = foundationVolume;

			storeyHeight = (model.FloorHeight + floorThickness) * model.NumberOfStoreys;
			storeyPerimeter = (2 * (model.BuildingWidth + model.BuildingLength)) * model.NumberOfStoreys;
			storeyWallVolume = (storeyPerimeter * storeyHeight) * model.NumberOfStoreys;
			storeyFloorVolume = (BuildingArea * floorThickness) * model.NumberOfStoreys;

			storeyFloorConcrete = storeyFloorVolume;
            storeyFloorRebar = rebarConstant * storeyFloorVolume;
			
			storeyFloorPlywood = plywoodSheetsPerSqm * BuildingArea;
            storeyFloorNails = storeyFloorPlywood * nailConstant;

			storeySupportBeamsNeeded = supportBeamsNeeded;
            storeySupportBeamsConcrete = supportBeamVolume * supportBeamsNeeded;
            storeySupportBeamsRebar = rebarConstant * supportBeamVolume * supportBeamsNeeded;


			storeyWallConcrete = storeyWallVolume;
			storeyWallRebar = rebarConstant * storeyWallVolume;

			stairsVolume = numberOfSteps * riserHeight * threadDepth * 1.25;
			stairsConcrete = stairsVolume;
			stairsRebar = rebarConstant * stairsVolume;

			totalConcrete = stairsConcrete + storeyWallConcrete + storeySupportBeamsConcrete + storeyFloorConcrete + foundationConcrete;
			totalBlocks = foundationNoOfHollowBlock;
			totalRebar = stairsRebar + storeyWallRebar + storeySupportBeamsRebar + storeyFloorRebar + foundationRebar;
			totalPlywood = storeyFloorPlywood;
			totalNails = storeyFloorNails;

			double costConcrete = totalConcrete * ((230 * 0.16667) + (800 * 0.33333) + (950 * 0.5));
			double costBlocks = totalBlocks * 13;
			double costRebar = totalRebar * 13;
			double costPlywood = totalPlywood * 490;

			string debug = $"Total Concrete: {totalConcrete} | {costConcrete}\n" +
							$"Total Rebar: {totalRebar} | {costRebar}\n" +
							$"Total Blocks: {totalBlocks} | {costBlocks}\n" +
							$"Total Plywood: {totalPlywood} | {costPlywood}\n" +
							$"Total Nails: {totalNails}\n";

			Debug.WriteLine(debug);

			BillOfMaterialsModel bommodel = new BillOfMaterialsModel();

			bommodel.Title = model.Title;
			bommodel.Address = model.Address;
			bommodel.City = model.City;
			bommodel.Region = model.Region;
			bommodel.Country = model.Country;
			bommodel.Longtitude = model.Longtitude;
			bommodel.Latitude = model.Latitude;
			bommodel.ProjectDate = DateTime.Today;
			bommodel.ProjectRef = "";
			bommodel.Engineer_ID = Convert.ToInt32(HttpContext.Session.GetInt32("UserID"));
			bommodel.ID = 0;
			bommodel.storeys = model.NumberOfStoreys;
			bommodel.floorHeight = model.FloorHeight;
			bommodel.length = model.BuildingLength;
			bommodel.width = model.BuildingWidth;
			
			bommodel.materials = Materials;
			bommodel.categories = Categories;
			bommodel.measurements = Measurements;

			bommodel.lists = new List<BOMList>();
			bommodel.lists.Add(new BOMList()
			{
				Desc = "Foundation",
				items = new List<BOMItems>()
			});
			bommodel.lists[0].items.Add(GetConcreteMaterials(foundationConcrete, foundationRebar, (bommodel.Latitude + "," + bommodel.Longtitude)));
			bommodel.lists[0].items.Add(GetBrickMaterials(foundationNoOfHollowBlock, (bommodel.Latitude + "," + bommodel.Longtitude)));
			////////
			bommodel.lists.Add(new BOMList()
			{
				Desc = "Storeys",
				items = new List<BOMItems>()
			});
			bommodel.lists[1].items.Add(GetConcreteMaterials(storeyFloorConcrete, storeyFloorRebar, (bommodel.Latitude + "," + bommodel.Longtitude)));
			
			///////
			bommodel.lists.Add(new BOMList()
			{
				Desc = "Support Beams",
				items = new List<BOMItems>()
			});
			bommodel.lists[2].items.Add(GetConcreteMaterials(storeySupportBeamsConcrete, storeySupportBeamsRebar, (bommodel.Latitude + "," + bommodel.Longtitude)));
			
			//////
			bommodel.lists.Add(new BOMList()
			{
				Desc = "Storey Walls",
				items = new List<BOMItems>()
			});
			bommodel.lists[3].items.Add(GetConcreteMaterials(storeyWallConcrete, storeyWallRebar, (bommodel.Latitude + "," + bommodel.Longtitude)));
			//////
			bommodel.lists.Add(new BOMList()
			{
				Desc = "Stairs",
				items = new List<BOMItems>()
			});
			bommodel.lists[4].items.Add(GetConcreteMaterials(stairsConcrete, stairsRebar, (bommodel.Latitude + "," + bommodel.Longtitude)));

			TempData["BOMModel"] = JsonConvert.SerializeObject(bommodel);

			Debug.WriteLine(bommodel.lists.Count);
			Debug.WriteLine(bommodel.lists[0].items.Count);
			Debug.WriteLine(bommodel.lists[0].items[0].subitems.Count);

			return RedirectToAction("BOMView");
		}

		public IActionResult MaterialCostEstimate(MaterialCostEstimateModel? bommodel)
        {
			MaterialCostEstimateModel model = null;
			if (TempData["MVCModel"] == null)
			{
				Debug.WriteLine("nuts");
				model = new MaterialCostEstimateModel();
				model.lists = new List<MCEList>();
			}
			else
			{
				model = JsonConvert.DeserializeObject<MaterialCostEstimateModel>(TempData["MVCModel"].ToString());
			}
			TempData["MVCModel"] = JsonConvert.SerializeObject(model);
			return View(model);
        }

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> MaterialCostEstimate(MaterialCostEstimateModel model, string submitButton) //
		{
			switch(submitButton)
			{
				case "a":
					return RedirectToAction("AccountPartner");
					break;
				case "b":
					return RedirectToAction("AccountEmployee");
					break;
			}
			return View();
		}

		public IActionResult MCEAddList()
		{
			return View();
		}
		
		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> MCEAddList(MCEAddListModel listmodel)
		{
			MaterialCostEstimateModel model = JsonConvert.DeserializeObject<MaterialCostEstimateModel>(TempData["MVCModel"].ToString());
			model.lists.Add(new MCEList()
			{
				Desc = listmodel.Description,
				items = new List<MCEItem>()
			});
			TempData["MVCModel"] = JsonConvert.SerializeObject(model);
			return RedirectToAction("MaterialCostEstimate");
		}

		public IActionResult MCEAddItem(int? id)
		{
			MCEAddItemModel model = new MCEAddItemModel();
			model.listId = id;
			return View(model);
		}
		
		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> MCEAddItem(MCEAddItemModel itemmodel, int? id)
		{
			MaterialCostEstimateModel model = JsonConvert.DeserializeObject<MaterialCostEstimateModel>(TempData["MVCModel"].ToString());
			model.lists[Convert.ToInt32(id)].items.Add(new MCEItem()
			{
				item_desc = itemmodel.Description,
				subitems = new List<MCESubitem>()
			});
			TempData["MVCModel"] = JsonConvert.SerializeObject(model);
			return RedirectToAction("MaterialCostEstimate");
		}

		public IActionResult MCEAddSubitem(string? id)
		{
			string[] id_split = id.Split('s');
			return View();
		}
		/*
		[HttpPost]
		[AllowAnonymous]
		public Task<ActionResult> MCEAddSubitem()
		{
			return View();
		}*/

		public IActionResult AddProduct()
		{
			AddMaterialModel model = new AddMaterialModel();
			model.CategoryList = GetMaterialCategories();
			model.MeasurementTypeList = GetMeasurementTypes();
			model.MeasurementUnitList = GetMeasurementUnits();

			return View(model);
		}
		[HttpPost]
		[AllowAnonymous]
		public ActionResult AddProduct(AddMaterialModel model)
		{
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

				model.CategoryList = GetMaterialCategories();
				model.MeasurementTypeList = GetMeasurementTypes();
				model.MeasurementUnitList = GetMeasurementUnits();
				return View(model);
			}


			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("INSERT INTO materials(material_desc,material_desc_long,category_id,measurement_unit_id,material_measurement_type,material_measurement_value) " +
					"VALUES (@material_desc,@material_desc_long, @category_id, @measurement_unit_id, @material_measurement_type, @material_measurement_value);"))
				{
					command.Connection = conn;

					command.Parameters.AddWithValue("@material_desc", model.Description);
					command.Parameters.AddWithValue("@material_desc_long", model.Description_Long);
					command.Parameters.AddWithValue("@category_id", Convert.ToInt32(model.CategoryID));
					command.Parameters.AddWithValue("@measurement_unit_id", Convert.ToInt32(model.MeasurementID));
					command.Parameters.AddWithValue("@material_measurement_type", Convert.ToInt32(model.MeasurementType));
					command.Parameters.AddWithValue("@material_measurement_value", model.MeasurementValue);

					conn.Open();
					command.ExecuteNonQuery();
					conn.Close();
				}
			}

			return RedirectToAction("Account");
		}

		public IActionResult EditProduct(int? id)
		{
			EditMaterialModel model = new EditMaterialModel();
			model.CategoryList = GetMaterialCategories();
			model.MeasurementTypeList = GetMeasurementTypes();
			model.MeasurementUnitList = GetMeasurementUnits();

			if (id != null)
			{
				using (SqlConnection connection = new SqlConnection(connectionstring))
				{
					using (SqlCommand command = new SqlCommand("SELECT * FROM materials WHERE material_id = @id;"))
					{
						connection.Open();
						command.Connection = connection;
						command.Parameters.AddWithValue("@id", Convert.ToInt32(id));
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							while (sdr.Read())
							{
								model.ID = (int)id;
								model.Description = sdr["material_desc"].ToString();
								model.Description_Long = sdr["material_desc_long"].ToString();
								model.CategoryID = Convert.ToInt32(sdr["category_id"]);
								model.MeasurementType = Convert.ToInt32(sdr["material_measurement_type"]);
								model.MeasurementID = Convert.ToInt32(sdr["measurement_unit_id"]);
								model.MeasurementValue = Convert.ToDouble(sdr["material_measurement_value"]);
							}
						}
					}
				}
			}
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult EditProduct(MaterialsEditModel model)
		{
			if(!ModelState.IsValid)
			{
				model.measurements = new List<MeasurementList>();
				model.categories = new List<CategoryList>();
				model.manufacturers = new List<ManufacturerList>();
				using (SqlConnection conn = new SqlConnection(connectionstring))
				{
					using (SqlCommand command = new SqlCommand("SELECT * FROM measurement_units;"))
					{
						command.Connection = conn;
						conn.Open();
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							Debug.WriteLine("dom");
							while (sdr.Read())
							{
								Debug.WriteLine("som");
								//xmodel.measurements.Add(new SelectListItem
								model.measurements.Add(new MeasurementList
								{
									Id = sdr["measurement_unit_id"].ToString(),
									description = sdr["unit_desc"].ToString()
								}
								);
							}
						}
						conn.Close();
					}
					using (SqlCommand command = new SqlCommand("SELECT * FROM material_categories;"))
					{
						command.Connection = conn;
						conn.Open();
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							Debug.WriteLine("dom");
							while (sdr.Read())
							{
								Debug.WriteLine("som");
								//xmodel.categories.Add(new SelectListItem
								model.categories.Add(new CategoryList
								{
									Id = sdr["category_id"].ToString(),
									description = sdr["category_desc"].ToString()
								}
								);
							}
						}
						conn.Close();
					}
					using (SqlCommand command = new SqlCommand("SELECT * FROM material_manufacturers;"))
					{
						command.Connection = conn;
						conn.Open();
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							Debug.WriteLine("dom");
							while (sdr.Read())
							{
								Debug.WriteLine("som");
								//xmodel.manufacturers.Add(new SelectListItem
								model.manufacturers.Add(new ManufacturerList
								{
									Id = sdr["manufacturer_id"].ToString(),
									description = sdr["manufacturer_desc"].ToString()
								}
								);
							}
						}
						conn.Close();
					}
				}
				return View(model);
			}

			decimal length = 0, width = 0, height = 0, weight = 0, volume = 0;

			if (model.Length != null)
				length = model.Length.Value;
			if (model.Width != null)
				width = model.Width.Value;
			if (model.Height != null)
				height = model.Height.Value;
			if (model.Weight != null)
				weight = model.Weight.Value;
			if (model.Volume != null)
				volume = model.Volume.Value;

			using (SqlConnection connection = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("UPDATE materials SET " +
					"material_desc = @material_desc, " +
					"material_desc_long = @material_desc_long, " +
					"unit_id = @unit_id, " +
					"category_id = @category_id, " +
					"manufacturer_id = @manufacturer_id, " +
					"price = @price, " +
					"length = @length, " +
					"width = @width, " +
					"height = @height, " +
					"weight = @weight, " +
					"volume = @volume " +
					"WHERE material_id = @id;"))
				{
					connection.Open();
					command.Connection = connection;
					command.Parameters.AddWithValue("@id", Convert.ToInt32(model.ID));
					command.Parameters.AddWithValue("@material_desc", model.Description);
					command.Parameters.AddWithValue("@material_desc_long", model.LongDescription);
					command.Parameters.AddWithValue("@unit_id", Convert.ToInt32(model.MeasurementUnit));
					command.Parameters.AddWithValue("@category_id", Convert.ToInt32(model.Category));
					command.Parameters.AddWithValue("@manufacturer_id", Convert.ToInt32(model.Manufacturer));
					command.Parameters.AddWithValue("@price", Convert.ToInt32(Math.Floor(model.Price * 100)));

					if (length != 0)
						command.Parameters.AddWithValue("@length", length);
					else
						command.Parameters.AddWithValue("@length", DBNull.Value);

					if (width != 0)
						command.Parameters.AddWithValue("@width", width);
					else
						command.Parameters.AddWithValue("@width", DBNull.Value);

					if (height != 0)
						command.Parameters.AddWithValue("@height", height);
					else
						command.Parameters.AddWithValue("@height", DBNull.Value);

					if (weight != 0)
						command.Parameters.AddWithValue("@weight", weight);
					else
						command.Parameters.AddWithValue("@weight", DBNull.Value);

					if (volume != 0)
						command.Parameters.AddWithValue("@volume", volume);
					else
						command.Parameters.AddWithValue("@volume", DBNull.Value);

					command.ExecuteNonQuery();
				}
			}

			return RedirectToAction("Account");
		}

		/*
		public IActionResult EditProduct(int id)
		{
			MaterialsEditModel model = new MaterialsEditModel();
			using (SqlConnection connection = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials WHERE material_id = @id;"))
				{
					connection.Open();
					command.Connection = connection;
					command.Parameters.AddWithValue("@id", Convert.ToInt32(id));
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.ID = id.ToString();
							model.Description = sdr["material_desc"].ToString();
							model.LongDescription = sdr["material_desc_long"].ToString();
							model.MeasurementUnit = sdr["unit_id"].ToString();
							model.Category = sdr["category_id"].ToString();
							model.Manufacturer = sdr["manufacturer_id"].ToString();
							model.Price = Convert.ToDecimal(sdr["price"]);
							model.Length = Convert.ToDecimal(sdr["length"]);
							model.Width = Convert.ToDecimal(sdr["width"]);
							model.Height = Convert.ToDecimal(sdr["height"]);
							model.Weight = Convert.ToDecimal(sdr["weight"]);
							model.Volume = Convert.ToDecimal(sdr["volume"]);
						}
					}
				}
			}
			return View(model);
		}
		*/
		public IActionResult MaterialsList()
		{
			MaterialsEditViewModel model = new MaterialsEditViewModel();
			model.items = new List<MaterialsEditViewItem>();
			using (SqlConnection connection = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials;"))
				{
					connection.Open();
					command.Connection = connection;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.items.Add(new MaterialsEditViewItem
							{
								ID = Convert.ToInt32(sdr["material_id"]),
								Description = sdr["material_desc"].ToString()
							});
						}
					}
				}
			}
			return View(model);
		}

		public IActionResult AddEmployee()
		{
			if (HttpContext.Session.GetInt32("EmployeeID") == null)
			{
				return RedirectToAction("HomePage", "Home");
			}
			AddEmployeeModel xmodel = new AddEmployeeModel();

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
		public ActionResult AddEmployee(AddEmployeeModel model)
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


		public IActionResult EditEmployee()
        {
            return View();
        }

		public IActionResult AddPartner()
        {
            return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult AddPartner(AddSupplierModel model)
		{
			if (!ModelState.IsValid)
			{
				foreach(var x in ModelState.Keys)
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

		public IActionResult EditPartner()
        {
            return View();
        }

        public IActionResult LogIn()
        {
            return View();
        }

        public IActionResult AccountPartner()
		{
			Debug.WriteLine("wead");
			return View();
        }

        public IActionResult AccountEmployee()
        {
            return View();
        }

		public IActionResult BOMView(int? id)
		{
			BillOfMaterialsModel model = null;

			if (TempData["BOMModel"] == null && id == null)
			{
				Debug.WriteLine("nuts");
				model = new BillOfMaterialsModel();
				model.lists = new List<BOMList>();
				model.materials = GetMaterialsFromDB();
				model.categories = GetCategoriesFromDB();
				model.measurements = GetMeasurementsFromDB();
			}
			else if (TempData["BOMModel"] != null && id == null)
			{
				Debug.WriteLine("bunger");
				model = JsonConvert.DeserializeObject<BillOfMaterialsModel>(TempData["BOMModel"].ToString());
				model.materials = GetMaterialsFromDB();
				model.categories = GetCategoriesFromDB();
				model.measurements = GetMeasurementsFromDB();
			}
			
			else if (TempData["BOMModel"] == null && id != null)
			{
				model = new BillOfMaterialsModel();
				model.lists = new List<BOMList>();
				model.materials = GetMaterialsFromDB();
				model.categories = GetCategoriesFromDB();
				model.measurements = GetMeasurementsFromDB();

				int buildingmaterialid = 0;

				using (SqlConnection conn = new SqlConnection(connectionstring))
				{
					conn.Open();

					SqlCommand command1 = new SqlCommand("SELECT * FROM bom WHERE bom_id = @bom_id");
					command1.Parameters.AddWithValue("@bom_id", id);
					command1.Connection = conn;
					using (SqlDataReader sdr = command1.ExecuteReader())
					{
						while(sdr.Read())
						{
							model.Title = sdr["project_title"].ToString();
							model.Address = sdr["project_location"].ToString();
							model.ProjectRef = sdr["project_ref"].ToString();
							model.ProjectDate = DateTime.Parse(sdr["project_date"].ToString());
							model.Engineer_ID = Convert.ToInt32(sdr["project_engineer_id"]);
							buildingmaterialid = Convert.ToInt32(sdr["building_material_id"]);
							model.storeys = Convert.ToInt32(sdr["building_storeys"]);
							model.floorHeight = Convert.ToDouble(sdr["building_floorheight"]);
							model.length = Convert.ToDouble(sdr["building_length"]);
							model.width = Convert.ToDouble(sdr["building_width"]);
						}
					}
					command1.Dispose();

					SqlCommand command2 = new SqlCommand("SELECT * FROM bom_lists WHERE bom_id = @bom_id;");
					command2.Parameters.AddWithValue("@bom_id", id);
					command2.Connection = conn;
					using (SqlDataReader sdr = command2.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.lists.Add(new BOMList()
							{
								id = Convert.ToInt32(sdr["bom_list_id"]),
								Desc = sdr["list_desc"].ToString(),
								items = new List<BOMItems>()
							});
						}
					}
					command2.Dispose();

					int listindex = 0;
					foreach(BOMList list in model.lists)
					{
						SqlCommand command3 = new SqlCommand("SELECT * FROM bom_items WHERE bom_list_id = @bom_list_id");

						command3.Parameters.AddWithValue("@bom_list_id", list.id);
						command3.Connection = conn;
						using (SqlDataReader sdr = command3.ExecuteReader())
						{
							while (sdr.Read())
							{
								model.lists[listindex].items.Add(new BOMItems()
								{
									id = Convert.ToInt32(sdr["bom_item_id"]),
									item_id = Convert.ToInt32(sdr["item_id"]),
									subitems = new List<BOMSubitems>()
								});
							}
						}
						command3.Dispose();


						int itemindex = 0;
						foreach (BOMItems item in list.items)
						{
							SqlCommand command4 = new SqlCommand("SELECT * FROM bom_subitems WHERE bom_item_id = @bom_item_id");
							command4.Parameters.AddWithValue("@bom_item_id", item.id);
							command4.Connection = conn;
							using (SqlDataReader sdr = command4.ExecuteReader())
							{
								while (sdr.Read())
								{
									model.lists[listindex].items[itemindex].subitems.Add(new BOMSubitems()
									{
										id = Convert.ToInt32(sdr["bom_item_id"]),
										item_id = Convert.ToInt32(sdr["item_id"]),
										Quantity = sdr["item_quantity"].ToString()
									});
								}
							}
							command4.Dispose();
							itemindex++;
						}
						listindex++;
					}
					
					conn.Close();
				}
			}
			TempData["BOMModel"] = JsonConvert.SerializeObject(model);

			Debug.WriteLine(model.lists.Count);
			Debug.WriteLine(model.lists[0].items.Count);
			Debug.WriteLine(model.lists[0].items[0].subitems.Count);

			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> BOMView(BillOfMaterialsModel xmodel)
		{
			BillOfMaterialsModel model = JsonConvert.DeserializeObject<BillOfMaterialsModel>(TempData["BOMModel"].ToString());
			model.Title = xmodel.Title;
			model.Address = xmodel.Address;
			model.ProjectDate = xmodel.ProjectDate;
			model.ProjectRef = xmodel.ProjectRef;
			TempData["BOMModel"] = JsonConvert.SerializeObject(model);
			if (!ModelState.IsValid)
			{
				model.materials = GetMaterialsFromDB();
				model.categories = GetCategoriesFromDB();
				model.measurements = GetMeasurementsFromDB();
				return View(model);
			}

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlTransaction trans = conn.BeginTransaction())
				{
					try
					{
						SqlCommand command = new SqlCommand(
							"INSERT INTO bom " +
							"(bom_project_title,bom_project_address,bom_project_city,bom_project_admin_district, " +
							"bom_project_country,bom_project_latitude,bom_project_longtitude,bom_project_date,bom_project_ref, " +
							"bom_project_engineer_id,bom_building_storeys,bom_building_floorheight,bom_building_length," +
							"bom_building_width) VALUES " +
							"(@project_title, " +
							"@project_location, " +
							"@project_city, " +
							"@project_region, " +
							"@project_country, " +
							"@project_latitude, " +
							"@project_longttude, " +
							"@project_date, " +
							"@project_ref, " +
							"@project_engineer_id," +
							"@building_storeys," +
							"@building_floorheight," +
							"@building_length," +
							"@building_width);" +
							"SELECT SCOPE_IDENTITY() FROM bom;");
						Debug.WriteLine(model.Title);
						Debug.WriteLine(model.Address);
						Debug.WriteLine(model.ProjectDate.ToString("dd-MM-yyyy"));
						Debug.WriteLine(model.ProjectRef);
						command.Parameters.AddWithValue("@project_title", model.Title);
						command.Parameters.AddWithValue("@project_location", model.Address);
						command.Parameters.AddWithValue("@project_city", model.City);
						command.Parameters.AddWithValue("@project_region", model.Region);
						command.Parameters.AddWithValue("@project_country", model.Country);
						command.Parameters.AddWithValue("@project_latitude", model.Latitude);
						command.Parameters.AddWithValue("@project_longttude", model.Longtitude);
						command.Parameters.AddWithValue("@project_date", model.ProjectDate);
						command.Parameters.AddWithValue("@project_ref", model.ProjectRef);
						command.Parameters.AddWithValue("@project_engineer_id", Convert.ToInt32(HttpContext.Session.GetInt32("UserID")));
						command.Parameters.AddWithValue("@building_storeys", model.storeys);
						command.Parameters.AddWithValue("@building_floorheight", model.floorHeight);
						command.Parameters.AddWithValue("@building_length", model.length);
						command.Parameters.AddWithValue("@building_width", model.width);

						command.Connection = conn;
						command.Transaction = trans;
						int bom_id = Convert.ToInt32(command.ExecuteScalar());
						command.Dispose();
						Debug.WriteLine("BOM");

						foreach (BOMList list in model.lists)
						{
							SqlCommand command2 = new SqlCommand(
								"INSERT INTO bom_lists (bom_list_desc,bom_id) VALUES " +
								"(@list_desc," +
								"@bom_id);" +
								"SELECT SCOPE_IDENTITY() FROM bom_lists;");

							command2.Parameters.AddWithValue("@list_desc", list.Desc);
							command2.Parameters.AddWithValue("@bom_id", bom_id);

							command2.Connection = conn; 
							command2.Transaction = trans;
							int bom_list_id = Convert.ToInt32(command2.ExecuteScalar());
							command2.Dispose();
							Debug.WriteLine("LIST");

							foreach (BOMItems item in list.items)
							{
								SqlCommand command3 = new SqlCommand(
								"INSERT INTO bom_items (bom_list_id,category_id) VALUES " +
								"(@bom_list_id," +
								"@item_id);" +
								"SELECT SCOPE_IDENTITY() FROM bom_items;");

								command3.Parameters.AddWithValue("@bom_list_id", bom_list_id);
								command3.Parameters.AddWithValue("@item_id", item.item_id);

								command3.Connection = conn;
								command3.Transaction = trans;
								int bom_list_item_id = Convert.ToInt32(command3.ExecuteScalar());
								command3.Dispose();
								Debug.WriteLine("ITEM");

								foreach (BOMSubitems subitem in item.subitems)
								{
									SqlCommand command4 = new SqlCommand(
										"INSERT INTO bom_subitems (material_id,bom_subitem_quantity,bom_item_id) VALUES " +
										"(@item_id," +
										"@item_quantity," +
										"@bom_item_id);");

									command4.Parameters.AddWithValue("@item_id", subitem.item_id);
									command4.Parameters.AddWithValue("@item_quantity", subitem.Quantity);
									command4.Parameters.AddWithValue("@bom_item_id", bom_list_item_id);

									command4.Connection = conn;
									command4.Transaction = trans;
									command4.ExecuteNonQuery();
									command4.Dispose();
									Debug.WriteLine("SUBITEM");
								}
							}
						}
						trans.Commit();
					}
					catch (SqlException e)
					{
						Debug.WriteLine("DB Write Error: " + e.Message);
						trans.Rollback();
					}
				}
			}
			return RedirectToAction("Account");
		}

		public IActionResult BOMAddList()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> BOMAddList(BOMAddListModel listmodel)
		{
			BillOfMaterialsModel model = JsonConvert.DeserializeObject<BillOfMaterialsModel>(TempData["BOMModel"].ToString());
			model.lists.Add(new BOMList()
			{
				Desc = listmodel.Description,
				items = new List<BOMItems>()
			});
			TempData["BOMModel"] = JsonConvert.SerializeObject(model);
			return RedirectToAction("BOMView");
		}

		public IActionResult BOMAddItem(int? id)
		{
			BOMAddItemModel model = new BOMAddItemModel();
			model.listId = id;
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> BOMAddItem(BOMAddItemModel itemmodel, int? id)
		{
			BillOfMaterialsModel model = JsonConvert.DeserializeObject<BillOfMaterialsModel>(TempData["BOMModel"].ToString());
			/*model.lists[Convert.ToInt32(id)].items.Add(new BOMItems()
			{
				Desc = itemmodel.Description,
				subitems = new List<BOMSubitems>()
			});*/
			TempData["BOMModel"] = JsonConvert.SerializeObject(model);
			return RedirectToAction("BOMView");
		}

		public IActionResult BOMAddSubitem(string? id)
		{
			BOMAddSubitemModel model = new BOMAddSubitemModel();
			model.id = id;
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> BOMAddSubitem(BOMAddSubitemModel itemmodel, string? id)
		{
			int[] ids = { Convert.ToInt32(id.Split('s')[0]), Convert.ToInt32(id.Split('s')[1]) };
			BillOfMaterialsModel model = JsonConvert.DeserializeObject<BillOfMaterialsModel>(TempData["BOMModel"].ToString());
			/*model.lists[ids[0]].items[ids[1]].subitems.Add(new BOMSubitems()
			{
				Desc = itemmodel.Description,
				UoM = itemmodel.UoM,
				Quantity = itemmodel.Quantity.ToString()
			});*/
			TempData["BOMModel"] = JsonConvert.SerializeObject(model);
			return RedirectToAction("BOMView");
		}

		public IActionResult BOMEdit(int? id)
		{
			BillOfMaterialsModel model = null;

			if (TempData["BOMModel"] == null && id == null)
			{
				Debug.WriteLine("nuts");
				model = new BillOfMaterialsModel();
				model.lists = new List<BOMList>();
				model.materials = GetMaterialsFromDB();
				model.categories = GetCategoriesFromDB();
				model.measurements = GetMeasurementsFromDB();
			}
			else if (TempData["BOMModel"] != null && id == null)
			{
				Debug.WriteLine("bunger");
				model = JsonConvert.DeserializeObject<BillOfMaterialsModel>(TempData["BOMModel"].ToString());
				model.materials = GetMaterialsFromDB();
				model.categories = GetCategoriesFromDB();
				model.measurements = GetMeasurementsFromDB();
			}

			else if (TempData["BOMModel"] == null && id != null)
			{
				model = new BillOfMaterialsModel();
				model.lists = new List<BOMList>();
				model.materials = GetMaterialsFromDB();
				model.categories = GetCategoriesFromDB();
				model.measurements = GetMeasurementsFromDB();

				int buildingmaterialid = 0;

				using (SqlConnection conn = new SqlConnection(connectionstring))
				{
					conn.Open();

					SqlCommand command1 = new SqlCommand("SELECT * FROM bom WHERE bom_id = @bom_id");
					command1.Parameters.AddWithValue("@bom_id", id);
					command1.Connection = conn;
					using (SqlDataReader sdr = command1.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.Title = sdr["project_title"].ToString();
							model.Address = sdr["project_location"].ToString();
							model.ProjectRef = sdr["project_ref"].ToString();
							model.ProjectDate = DateTime.Parse(sdr["project_date"].ToString());
							model.Engineer_ID = Convert.ToInt32(sdr["project_engineer_id"]);
							buildingmaterialid = Convert.ToInt32(sdr["building_material_id"]);
							model.storeys = Convert.ToInt32(sdr["building_storeys"]);
							model.floorHeight = Convert.ToDouble(sdr["building_floorheight"]);
							model.length = Convert.ToDouble(sdr["building_length"]);
							model.width = Convert.ToDouble(sdr["building_width"]);
						}
					}
					command1.Dispose();

					SqlCommand command2 = new SqlCommand("SELECT * FROM bom_lists WHERE bom_id = @bom_id;");
					command2.Parameters.AddWithValue("@bom_id", id);
					command2.Connection = conn;
					using (SqlDataReader sdr = command2.ExecuteReader())
					{
						while (sdr.Read())
						{
							model.lists.Add(new BOMList()
							{
								id = Convert.ToInt32(sdr["bom_list_id"]),
								Desc = sdr["list_desc"].ToString(),
								items = new List<BOMItems>()
							});
						}
					}
					command2.Dispose();

					int listindex = 0;
					foreach (BOMList list in model.lists)
					{
						SqlCommand command3 = new SqlCommand("SELECT * FROM bom_items WHERE bom_list_id = @bom_list_id");

						command3.Parameters.AddWithValue("@bom_list_id", list.id);
						command3.Connection = conn;
						using (SqlDataReader sdr = command3.ExecuteReader())
						{
							while (sdr.Read())
							{
								model.lists[listindex].items.Add(new BOMItems()
								{
									id = Convert.ToInt32(sdr["bom_item_id"]),
									item_id = Convert.ToInt32(sdr["item_id"]),
									subitems = new List<BOMSubitems>()
								});
							}
						}
						command3.Dispose();


						int itemindex = 0;
						foreach (BOMItems item in list.items)
						{
							SqlCommand command4 = new SqlCommand("SELECT * FROM bom_subitems WHERE bom_item_id = @bom_item_id");
							command4.Parameters.AddWithValue("@bom_item_id", item.id);
							command4.Connection = conn;
							using (SqlDataReader sdr = command4.ExecuteReader())
							{
								while (sdr.Read())
								{
									model.lists[listindex].items[itemindex].subitems.Add(new BOMSubitems()
									{
										id = Convert.ToInt32(sdr["bom_item_id"]),
										item_id = Convert.ToInt32(sdr["item_id"]),
										Quantity = sdr["item_quantity"].ToString()
									});
								}
							}
							command4.Dispose();
							itemindex++;
						}
						listindex++;
					}

					conn.Close();
				}
			}
			TempData["BOMModel"] = JsonConvert.SerializeObject(model);

			Debug.WriteLine(model.lists.Count);
			Debug.WriteLine(model.lists[0].items.Count);
			Debug.WriteLine(model.lists[0].items[0].subitems.Count);

			return View(model);
		}


		public IActionResult NewMCE(int? id)
		{
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();

				

				conn.Close();
			}
			return View();
		}

		public IActionResult SupplierMaterialsView()
		{
			List<SupplierMaterialViewItems> model = new List<SupplierMaterialViewItems>();
			model = GetSupplierMaterials();
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> SupplierMaterialsView(List<SupplierMaterialViewItems> model)
		{
			if (!ModelState.IsValid)
			{
				//return View(model);
			}
			Debug.WriteLine("eh");

			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				foreach (SupplierMaterialViewItems x in model)
				{
					if ((x.PreviousPrice != x.Price) || (x.PreviousIsAvailable != x.IsAvailable)) {
						Debug.WriteLine("lul");
						using (SqlCommand command = new SqlCommand("UPDATE supplier_materials SET " +
							"supplier_material_archived = @supplier_material_archived " +
							"WHERE supplier_material_id = @supplier_material_id; " +
							"INSERT INTO supplier_materials (supplier_id, material_id, supplier_material_price, supplier_material_availability, supplier_material_archived) " +
							"VALUES (@supplier_id, @material_id, @supplier_material_price, @supplier_material_availability, @supplier_material_archived2);"))
						{
							command.Connection = conn;
							command.Parameters.AddWithValue("@supplier_material_id", x.ID);
							command.Parameters.AddWithValue("@supplier_id", HttpContext.Session.GetInt32("SupplierID"));
							command.Parameters.AddWithValue("@material_id", x.MaterialID);
							command.Parameters.AddWithValue("@supplier_material_price", x.Price * 100);
							command.Parameters.AddWithValue("@supplier_material_availability", x.IsAvailable);
							command.Parameters.AddWithValue("@supplier_material_archived", true);
							command.Parameters.AddWithValue("@supplier_material_archived2", false);
							command.ExecuteNonQuery();
						}
					}
				}
			}
			//model = GetSupplierMaterials();
			return View(model);
		}

		public IActionResult MaterialsCostList()
		{
			List<MaterialsCostListModel> model = GetMaterialsCost();
			return View(model);
		}

		public List<MaterialsCostListModel> GetMaterialsCost()
		{
			List<MaterialsCostListModel> materials = new List<MaterialsCostListModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT c.material_desc_long, b.supplier_desc, a.supplier_material_price FROM supplier_materials a " +
					"INNER JOIN supplier_info b ON a.supplier_id = b.supplier_id " +
					"INNER JOIN materials c ON a.material_id = c.material_id " +
					"WHERE supplier_material_availability = 1 AND supplier_material_archived = 0 " +
					"ORDER BY c.material_id;"))
				{
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							materials.Add(new MaterialsCostListModel()
							{
								Description_Long = sdr["material_desc_long"].ToString(),
								Supplier_Desc = sdr["supplier_desc"].ToString(),
								Price = Convert.ToDouble(sdr["supplier_material_price"]) / 100
							});
						}
					}
					conn.Close();
				}
			}
			return materials;
		}

		public List<MaterialsListModel> GetMaterialsFromDB()
		{
			List<MaterialsListModel> materials = new List<MaterialsListModel>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials a " +
					"INNER JOIN measurement_units b ON a.measurement_unit_id = b.measurement_unit_id " +
					"INNER JOIN material_categories c ON a.category_id = c.category_id;"))
				{
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							materials.Add(new MaterialsListModel()
							{
								ID = Convert.ToInt32(sdr["material_id"]),
								Description = sdr["material_desc"].ToString(),
								Description_Long = sdr["material_desc_long"].ToString(),
								UoM_ID = Convert.ToInt32(sdr["measurement_unit_id"]),
								UoM_Desc = sdr["measurement_unit_desc"].ToString(),
								Category_ID = Convert.ToInt32(sdr["category_id"]),
								Category_Desc = sdr["category_desc"].ToString(),
								MeasurementType = Convert.ToInt32(sdr["material_measurement_type"]),
								MeasurementValue = Convert.ToDouble(sdr["material_measurement_value"])
							});
						}
					}
					conn.Close();
				}
			}
			return materials;
		}

		public List<SupplierMaterialViewItems> GetSupplierMaterials()
		{
			List<SupplierMaterialViewItems> materials = new List<SupplierMaterialViewItems>();
			bool isCreated = true;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM supplier_materials WHERE supplier_id = @supplier_id AND supplier_material_archived = 0;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@supplier_id", HttpContext.Session.GetInt32("SupplierID"));
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						if (!sdr.Read())
						{
							isCreated = false;
						}
					}
				}
				if (!isCreated)
				{
					List<int> material_ids = new List<int>();
					using (SqlCommand command = new SqlCommand("SELECT * FROM materials;"))
					{
						command.Connection = conn;
						using (SqlDataReader sdr = command.ExecuteReader())
						{
							while (sdr.Read())
							{
								material_ids.Add(Convert.ToInt32(sdr["material_id"]));
							}
						}
					}

					foreach (int x_id in material_ids)
					{
						using (SqlCommand command = new SqlCommand("INSERT INTO supplier_materials (supplier_id, material_id, supplier_material_price, supplier_material_availability, supplier_material_archived) " +
						"VALUES (@supplier_id, @material_id, @supplier_material_price, @supplier_material_availability, @supplier_material_archived);"))
						{
							command.Connection = conn;
							command.Parameters.AddWithValue("@supplier_id", HttpContext.Session.GetInt32("SupplierID"));
							command.Parameters.AddWithValue("@material_id", x_id);
							command.Parameters.AddWithValue("@supplier_material_price", 0);
							command.Parameters.AddWithValue("@supplier_material_availability", false);
							command.Parameters.AddWithValue("@supplier_material_archived", false);
							command.ExecuteNonQuery();
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT * FROM supplier_materials a INNER JOIN materials b ON a.material_id = b.material_id INNER JOIN measurement_units c ON b.measurement_unit_id = c.measurement_unit_id WHERE supplier_id = @supplier_id AND supplier_material_archived = 0 ORDER BY b.material_id ASC;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@supplier_id", HttpContext.Session.GetInt32("SupplierID"));
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							materials.Add(new SupplierMaterialViewItems()
							{
								IsAvailable = Convert.ToBoolean(sdr["supplier_material_availability"]),
								PreviousIsAvailable = Convert.ToBoolean(sdr["supplier_material_availability"]),
								ID = Convert.ToInt32(sdr["supplier_material_id"]),
								MaterialID = Convert.ToInt32(sdr["material_id"]),
								Description_Long = sdr["material_desc_long"].ToString(),
								MeasurementString = sdr["measurement_unit_desc_short"].ToString(),
								MeasurementValue = sdr["material_measurement_value"].ToString(),
								Price = Convert.ToDouble(sdr["supplier_material_price"]) / 100,
								PreviousPrice = Convert.ToDouble(sdr["supplier_material_price"]) / 100
							});
						}
					}
				}
				conn.Close();
			}
			return materials;
		}

		public List<CategoryList> GetCategoriesFromDB()
		{
			List<CategoryList> materials = new List<CategoryList>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM material_categories;"))
				{
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							materials.Add(new CategoryList()
							{
								Id = sdr["category_id"].ToString(),
								description = sdr["category_desc"].ToString()
							});
						}
					}
					conn.Close();
				}
			}
			return materials;
		}

		public List<MeasurementList> GetMeasurementsFromDB()
		{
			List<MeasurementList> materials = new List<MeasurementList>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM measurement_units;"))
				{
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							materials.Add(new MeasurementList()
							{
								Id = sdr["measurement_unit_id"].ToString(),
								description = sdr["measurement_unit_desc"].ToString()
							});
						}
					}
					conn.Close();
				}
			}
			return materials;
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

		public List<MaterialCategories> GetMaterialCategories()
		{
			List<MaterialCategories> Categories = new List<MaterialCategories>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM material_categories;"))
				{
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							Categories.Add(new MaterialCategories
							{
								ID = sdr["category_id"].ToString(),
								Description = sdr["category_desc"].ToString()
							}
							);
						}
					}
					conn.Close();
				}
			}
			return Categories;
		}

		public List<MeasurementUnits> GetMeasurementUnits()
		{
			List<MeasurementUnits> Units = new List<MeasurementUnits>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				using (SqlCommand command = new SqlCommand("SELECT * FROM measurement_units;"))
				{
					command.Connection = conn;
					conn.Open();
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							Units.Add(new MeasurementUnits
							{
								ID = sdr["measurement_unit_id"].ToString(),
								Description = sdr["measurement_unit_desc"].ToString(),
								Description_Plural = sdr["measurement_unit_desc_plural"].ToString(),
								Description_Abrev = sdr["measurement_unit_desc_short"].ToString(),
								Type = Convert.ToInt32(sdr["measurement_unit_type"]),
							}
							);
						}
					}
					conn.Close();
				}
			}
			return Units;
		}

		public List<MeasurementTypes> GetMeasurementTypes()
		{
			List<MeasurementTypes> Types = new List<MeasurementTypes>();
			Types.Add(new MeasurementTypes()
			{
				ID = "1",
				Description = "Length"
			});
			Types.Add(new MeasurementTypes()
			{
				ID = "2",
				Description = "Area"
			});
			Types.Add(new MeasurementTypes()
			{
				ID = "3",
				Description = "Weight"
			});
			Types.Add(new MeasurementTypes()
			{
				ID = "4",
				Description = "Volume"
			});
			Types.Add(new MeasurementTypes()
			{
				ID = "5",
				Description = "Pieces"
			});
			return Types;
		}

		public BOMItems GetConcreteMaterials(double ConcreteQuantity, double RebarQuantity, string destination)
		{
			BOMItems item = new BOMItems();
			item.subitems = new List<BOMSubitems>();
			double[] ratio = { 0, 0, 0 }, ratio_final = { 0, 0, 0 };
			double ratio_total = 0;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM material_categories WHERE category_id = 2;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							item.item_id = Convert.ToInt32(sdr["category_id"]);
							item.item_desc = sdr["category_desc"].ToString();
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials WHERE material_id IN (2,3,4,5);"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							item.subitems.Add(new BOMSubitems()
							{
								item_id = Convert.ToInt32(sdr["material_id"]),
								subitem_desc = sdr["material_desc"].ToString()
							});
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT b.* FROM employee_info a " +
					"JOIN employee_formula_constants b " +
					"ON a.formula_constants_id = b.formula_constants_id " +
					"WHERE a.employee_info_id = @id;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("EmployeeID"));
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							ratio[0] = Convert.ToInt32(sdr["concrete_ratio_cement"]);
							ratio[1] = Convert.ToInt32(sdr["concrete_ratio_sand"]);
							ratio[2] = Convert.ToInt32(sdr["concrete_ratio_aggregate"]);
						}
					}
				}
			}
			Debug.WriteLine("EEE: " + item.subitems.Count());
			ratio_total = ratio[0] + ratio[1] + ratio[2];
			for (int i = 0; i < ratio.Count(); i++)
			{
				ratio_final[i] = ratio[i] / ratio_total;
			}


			for (int i = 0; i < item.subitems.Count; i++)
			{
				MaterialsCostComparisonItem price = GetBestPrice(item.subitems[i].item_id, destination);
				{
					item.subitems[i].subitem_cost = price.Price.ToString();
					item.subitems[i].Supplier = price.SupplierDesc;
				}
			}

			for (int i = 0; i < 3; i++)
			{
				item.subitems[i].Quantity = Math.Ceiling(ConcreteQuantity * ratio_final[i]).ToString();
				item.subitems[i].Amount = Math.Round(Convert.ToDouble(item.subitems[i].Quantity) * Convert.ToDouble(item.subitems[i].subitem_cost),2).ToString();
			}
			item.subitems[3].Quantity = Math.Ceiling(RebarQuantity).ToString();
			item.subitems[3].Amount = Math.Round(Convert.ToDouble(RebarQuantity) * Convert.ToDouble(item.subitems[3].subitem_cost), 2).ToString();
			return item;
		}

		public BOMItems GetBrickMaterials(double Quantity, string destination)
		{
			BOMItems item = new BOMItems();
			item.subitems = new List<BOMSubitems>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM material_categories WHERE category_id = 3;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							item.item_id = Convert.ToInt32(sdr["category_id"]);
							item.item_desc = sdr["category_desc"].ToString();
						}
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials WHERE material_id IN (1);"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							item.subitems.Add(new BOMSubitems()
							{
								item_id = Convert.ToInt32(sdr["material_id"]),
								subitem_desc = sdr["material_desc"].ToString()
							});
						}
					}
				}
			}


			for (int i = 0; i < item.subitems.Count; i++)
			{
				MaterialsCostComparisonItem price = GetBestPrice(item.subitems[i].item_id, destination);
				{
					item.subitems[i].subitem_cost = price.Price.ToString();
					item.subitems[i].Supplier = price.SupplierDesc;
					item.subitems[i].Quantity = Math.Ceiling(Quantity).ToString();
					item.subitems[i].Amount = Math.Round(Quantity * Convert.ToDouble(item.subitems[i].subitem_cost), 2).ToString();
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
				using (SqlCommand command = new SqlCommand("SELECT c.material_id AS MaterialID, c.material_desc_long AS Material, " +
					"a.supplier_material_price AS Price, d.supplier_id AS SupplierID, d.supplier_desc AS Supplier, " +
					"CONCAT(d.supplier_coordinates_latitude, ',', d.supplier_coordinates_longtitude) AS Coordinates " +
					"FROM supplier_materials a JOIN (" +
					"SELECT MIN(b.supplier_material_price) AS min_value FROM supplier_materials b WHERE b.material_id = @id AND b.supplier_material_availability = 1) min_table " +
					"ON a.supplier_material_price = min_table.min_value " +
					"INNER JOIN materials c ON a.material_id = c.material_id " +
					"INNER JOIN supplier_info d ON a.supplier_id = d.supplier_id " +
					"WHERE a.material_id = @id " +
					"AND a.supplier_material_availability = 1;"))
				{
					command.Parameters.AddWithValue("@id", MaterialID);
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while(sdr.Read())
						{
							MaterialsCosts.Add(new MaterialsCostComparisonItem()
							{
								MaterialID = Convert.ToInt32(sdr["MaterialID"]),
								Description_Long = sdr["Material"].ToString(),
								Price = Convert.ToDouble(sdr["Price"]) / 100,
								SupplierID = Convert.ToInt32(sdr["SupplierID"]),
								SupplierDesc = sdr["Supplier"].ToString(),
								SupplierCoords = sdr["Coordinates"].ToString()
							});

						}
					}
				}
				conn.Close();
			}
			Debug.WriteLine("AAA");
			if (MaterialsCosts.Count > 1)
			{
				Debug.WriteLine("BBB");
				List<string> coords = new List<string>();
				foreach (MaterialsCostComparisonItem x in MaterialsCosts)
				{
					coords.Add(x.SupplierCoords);
				}
				BingMapsService bing = new BingMapsService(_apikey);
				List<double> distances = bing.GetDistancesAsync(coords,destination).Result;


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

			[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

		
    }

	public class BingMapsService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;

		public BingMapsService(string apiKey)
		{
			_httpClient = new HttpClient();
			_apiKey = apiKey;
		}

		public async Task<List<double>> GetDistancesAsync(List<string> origins, string destination)
		{
			// Build the API request URL with the list of origins and one destination.
			string originParams = string.Join(";", origins);
			string apiUrl = $"https://dev.virtualearth.net/REST/v1/Routes/DistanceMatrix?origins={originParams}&destinations={destination}&travelMode=driving&key={_apiKey}";

			// Send the API request.
			HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

			if (response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();

				// Parse the JSON response to extract the distances.
				var distances = new List<double>();
				var jsonData = JObject.Parse(responseBody);

				foreach (var resource in jsonData["resourceSets"][0]["resources"])
				{
					double distance = (double)resource["results"][0]["travelDistance"];
					distances.Add(distance);
				}

				return distances;
			}
			else
			{
				throw new Exception($"Error: {response.StatusCode}");
			}
		}
	}
}
public class PasswordEncryptor
{
	public static string EncryptPassword(string password)
	{
		// Generate a random salt
		byte[] salt;
		new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

		// Derive a 256-bit subkey (use more iterations for increased security)
		var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
		byte[] hash = pbkdf2.GetBytes(32); // 256 bits

		// Combine salt and hash
		byte[] hashBytes = new byte[48]; // 16 (salt) + 32 (hash)
		Array.Copy(salt, 0, hashBytes, 0, 16);
		Array.Copy(hash, 0, hashBytes, 16, 32);

		// Convert to Base64 for storage
		string encryptedPassword = Convert.ToBase64String(hashBytes);

		// Truncate to 64 characters if necessary
		if (encryptedPassword.Length > 64)
		{
			encryptedPassword = encryptedPassword.Substring(0, 64);
		}

		return encryptedPassword;
	}

	public static bool VerifyPassword(string enteredPassword, string storedEncryptedPassword)
	{
		// Convert the stored encrypted password from Base64
		byte[] hashBytes = Convert.FromBase64String(storedEncryptedPassword);

		// Extract the salt
		byte[] salt = new byte[16];
		Array.Copy(hashBytes, 0, salt, 0, 16);

		// Derive the key using the same salt and iteration count
		var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000, HashAlgorithmName.SHA256);
		byte[] enteredHash = pbkdf2.GetBytes(32);

		// Compare the derived key with the stored hash
		for (int i = 0; i < 32; i++)
		{
			if (enteredHash[i] != hashBytes[i + 16])
			{
				return false; // Passwords don't match
			}
		}

		return true; // Passwords match
	}
}