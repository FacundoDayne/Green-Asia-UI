using Green_Asia_UI.Models;
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

//using Sql.Data.SqlClient;
//using SqlX.XDevAPI;

using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Green_Asia_UI.Controllers
{
	public class SupplierController : Controller
	{
		private readonly string connectionstring = @"Server=68.71.129.120,1533;Database=bomgreen_db;User Id=bomgreen;Password=~Ni94tt39;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";

		public IActionResult supplierDashboard()
		{
			return View();
		}

		private List<SupplierMaterialsItem> GetMaterials()
		{
			List<SupplierMaterialsItem> materials = new List<SupplierMaterialsItem>();
			List<int> missing_materials = new List<int>();
			bool isCreated = true;
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM materials a LEFT JOIN " +
					"(SELECT * FROM supplier_materials WHERE supplier_id = @supplier_id AND supplier_material_archived = 0) " +
					"b ON a.material_id = b.material_id;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@supplier_id", HttpContext.Session.GetInt32("SupplierID"));
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							if (sdr.IsDBNull("supplier_material_id"))
							{
								missing_materials.Add(Convert.ToInt32(sdr["material_id"]));
							}
						}
					}
				}
				using (SqlCommand command = new SqlCommand("INSERT INTO supplier_materials " +
					"(supplier_id, material_id, supplier_material_price, supplier_material_availability, supplier_material_archived) " +
					"VALUES (@supplier_id, @material_id, @supplier_material_price, @supplier_material_availability, @supplier_material_archived);"))
				{
					SqlParameter param1 = new SqlParameter("@supplier_id", SqlDbType.Int);
					SqlParameter param2 = new SqlParameter("@material_id", SqlDbType.Int);
					SqlParameter param3 = new SqlParameter("@supplier_material_price", SqlDbType.Float);
					SqlParameter param4 = new SqlParameter("@supplier_material_availability", SqlDbType.Bit);
					SqlParameter param5 = new SqlParameter("@supplier_material_archived", SqlDbType.Bit);

					command.Parameters.Add(param1);
					command.Parameters.Add(param2);
					command.Parameters.Add(param3);
					command.Parameters.Add(param4);
					command.Parameters.Add(param5);

					command.Connection = conn;

					foreach (int x in missing_materials)
					{
						param1.Value = HttpContext.Session.GetInt32("SupplierID");
						param2.Value = x;
						param3.Value = 0;
						param4.Value = false;
						param5.Value = false;
						command.ExecuteNonQuery();
					}
				}
				using (SqlCommand command = new SqlCommand("SELECT * FROM supplier_materials a " +
					"INNER JOIN materials b ON a.material_id = b.material_id " +
					"INNER JOIN measurement_units c ON b.measurement_unit_id = c.measurement_unit_id " +
					"WHERE supplier_id = @supplier_id AND supplier_material_archived = 0 ORDER BY b.material_id ASC;"))
				{
					command.Connection = conn;
					command.Parameters.AddWithValue("@supplier_id", HttpContext.Session.GetInt32("SupplierID"));
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while (sdr.Read())
						{
							materials.Add(new SupplierMaterialsItem()
							{
								ID = Convert.ToInt32(sdr["supplier_material_id"]),
								Material_ID = Convert.ToInt32(sdr["material_id"]),
								Description = sdr["material_desc_long"].ToString(),
								Description_Long = sdr["material_desc_long"].ToString(),
								UoM = sdr["measurement_unit_desc_short"].ToString(),
								Quantity = sdr["material_measurement_value"].ToString(),
								Price = Convert.ToDouble(sdr["supplier_material_price"]) / 100,
								PreviousPrice = Convert.ToDouble(sdr["supplier_material_price"]) / 100,
								Availability = Convert.ToBoolean(sdr["supplier_material_availability"]),
								PreviousAvailability = Convert.ToBoolean(sdr["supplier_material_availability"])
							});
						}
					}
				}
				conn.Close();
			}
			return materials;
		}

		public IActionResult supplierMaterialsDash()
		{
			List<SupplierMaterialsItem> materials = GetMaterials();
			
			return View(materials);
		}

		public IActionResult supplierMaterialsEdit()
		{
			List<SupplierMaterialsItem> materials = GetMaterials();

			return View(materials);
		}

		[HttpPost]
		[AllowAnonymous]
		public IActionResult supplierMaterialsEdit(List<SupplierMaterialsItem> model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				foreach (SupplierMaterialsItem x in model)
				{
					if ((x.PreviousPrice != x.Price) || (x.Availability != x.PreviousAvailability))
					{
						using (SqlCommand command = new SqlCommand("UPDATE supplier_materials SET " +
							"supplier_material_archived = @supplier_material_archived " +
							"WHERE supplier_material_id = @supplier_material_id; " +
							"INSERT INTO supplier_materials (supplier_id, material_id, supplier_material_price, supplier_material_availability, supplier_material_archived) " +
							"VALUES (@supplier_id, @material_id, @supplier_material_price, @supplier_material_availability, @supplier_material_archived2);"))
						{
							command.Connection = conn;
							command.Parameters.AddWithValue("@supplier_material_id", x.ID);
							command.Parameters.AddWithValue("@supplier_id", HttpContext.Session.GetInt32("SupplierID"));
							command.Parameters.AddWithValue("@material_id", x.Material_ID);
							command.Parameters.AddWithValue("@supplier_material_price", x.Price * 100);
							command.Parameters.AddWithValue("@supplier_material_availability", x.Availability);
							command.Parameters.AddWithValue("@supplier_material_archived", true);
							command.Parameters.AddWithValue("@supplier_material_archived2", false);
							command.ExecuteNonQuery();
						}
					}
				}
			}
			return RedirectToAction("supplierMaterialsDash");
		}
	}
}
