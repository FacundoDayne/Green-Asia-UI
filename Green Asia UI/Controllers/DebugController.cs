using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using System;
using System.IO;
using Green_Asia_UI.Models;
using iText.Layout.Borders;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Math.EC.Multiplier;
using System.Diagnostics;

namespace Green_Asia_UI.Controllers
{
	public class DebugController : Controller
	{
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly string connectionstring = @"Server=68.71.129.120,1533;Database=bomgreen_db;User Id=bomgreen;Password=~Ni94tt39;Encrypt=False;Trusted_Connection=False;MultipleActiveResultSets=true";

		public DebugController(IWebHostEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}

		public IActionResult Index()
		{
			DebugModel model = new DebugModel();

			model.suppliers = new List<SelectListItem>();
			using (SqlConnection conn = new SqlConnection(connectionstring))
			{
				conn.Open();
				using (SqlCommand command = new SqlCommand("SELECT supplier_id, supplier_desc FROM supplier_info;"))
				{
					command.Connection = conn;
					using (SqlDataReader sdr = command.ExecuteReader())
					{
						while(sdr.Read())
						{
							model.suppliers.Add(new SelectListItem()
							{
								id = sdr["supplier_id"].ToString(),
								desc = sdr["supplier_desc"].ToString()
							});
						}
					}
				}
			}

			string fileStoragePath = System.IO.Path.Combine(_hostingEnvironment.ContentRootPath, "FileStorage");
			Directory.CreateDirectory(fileStoragePath);
			string fileName = Guid.NewGuid() + System.IO.Path.GetExtension("AAA.pdf");
			string filePath = System.IO.Path.Combine(fileStoragePath, fileName);

			if (!System.IO.File.Exists(filePath))
			{
				PageSize pageSize = PageSize.A4;
				PdfDocument pdfDoc = new PdfDocument(new PdfWriter(filePath));
				pdfDoc.SetDefaultPageSize(pageSize);
				Document doc = new Document(pdfDoc);
				doc.SetMargins(36, 36, 72, 36);

				Paragraph p = new Paragraph();
				p.Add(new Text(""));
				p.SetMarginTop(30);
				doc.Add(p);

				float availableWidth = doc.GetPageEffectiveArea(pdfDoc.GetDefaultPageSize()).GetWidth();

				Table table2 = new Table(UnitValue.CreatePointArray(new float[] { availableWidth / 3, (availableWidth / 3) * 2}));
				table2.SetBorder(Border.NO_BORDER);

				Paragraph p2 = new Paragraph();
				p2.Add(new Text("Date:"));
				p2.SetFontSize(10);
				Cell cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				p2 = new Paragraph();
				p2.Add(new Text("1 August, 2023"));
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
				p2.Add(new Text("Bill of Materials"));
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
				p2.Add(new Text("Proposed construction of a one-storey house"));
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
				p2.Add(new Text("Manila"));
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
				p2.Add(new Text("Php 100,000,000.00"));
				p2.SetFontSize(10);
				p2.SetBold();
				cell = new Cell();
				cell.Add(p2);
				cell.SetBorder(Border.NO_BORDER);
				table2.AddCell(cell);

				doc.Add(table2);

				p = new Paragraph();
				p.Add(new Text(""));
				p.SetMarginTop(30);
				doc.Add(p);

				Table table = new Table(UnitValue.CreatePointArray(new float[] { availableWidth / 4, availableWidth / 4, availableWidth / 4, availableWidth / 4 }));

				Cell headerCell = new Cell(1, 4);
				headerCell.SetTextAlignment(TextAlignment.CENTER);
				Paragraph headerParagraph = new Paragraph("Materials Cost Estimate");
				headerParagraph.SetBold();
				headerCell.Add(headerParagraph);

				table.AddHeaderCell(headerCell);
				table.AddHeaderCell(createCellCentred("Material", 10));
				table.AddHeaderCell(createCellCentred("Price", 10));
				table.AddHeaderCell(createCellCentred("Quantity", 10));
				table.AddHeaderCell(createCellCentred("Total", 10));

				table.AddCell(createCell("Cement",10));
				table.AddCell(createCell("200",10));
				table.AddCell(createCell("100", 10));
				table.AddCell(createCell("20,000", 10));

				table.AddCell(createCell("Sand", 10));
				table.AddCell(createCell("800", 10));
				table.AddCell(createCell("100", 10));
				table.AddCell(createCell("80,000", 10));

				table.AddCell(createCell("Aggregate", 10));
				table.AddCell(createCell("900", 10));
				table.AddCell(createCell("100", 10));
				table.AddCell(createCell("90,000", 10));

				table.AddCell(createCell("Rebar", 10));
				table.AddCell(createCell("300", 10));
				table.AddCell(createCell("50", 10));
				table.AddCell(createCell("15,000", 10));

				doc.Add(table);

				p = new Paragraph();
				p.Add(new Text(""));
				p.SetMarginTop(30);
				doc.Add(p);

				Table table3 = new Table(UnitValue.CreatePointArray(new float[] { availableWidth / 3}));
				table3.AddCell(createCellBorderless("Prepared by:", 10));
				table3.AddCell(createCellBorderlessCentredUnderline("John W. Engineer", 10, true));
				table3.AddCell(createCellBorderlessCentred("Engineer", 10));
				doc.Add(table3);
				doc.Close();
				pdfDoc.Close();
			}
			if (System.IO.File.Exists(filePath))
			{
				model.filepath = filePath;
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

		public IActionResult PDFPrint()
		{
			DebugModel model = new DebugModel();

			string fileStoragePath = System.IO.Path.Combine(_hostingEnvironment.ContentRootPath, "FileStorage");
			Directory.CreateDirectory(fileStoragePath);
			string fileName = Guid.NewGuid() + System.IO.Path.GetExtension("AAA.pdf");
			string filePath = System.IO.Path.Combine(fileStoragePath, fileName);

			if (!System.IO.File.Exists(filePath))
			{
				PageSize pageSize = PageSize.A4;
				PdfDocument pdfDoc = new PdfDocument(new PdfWriter(filePath));
				pdfDoc.SetDefaultPageSize(pageSize);
				Document doc = new Document(pdfDoc);
				doc.SetMargins(36, 36, 72, 36);
				Paragraph p = new Paragraph();
				p.Add(new Text("PROPOSED CONSTRUCTION OF A ONE-STOREY RESIDENTIAL HOUSE")
					.SetFont(PdfFontFactory.CreateFont("Arial", PdfEncodings.IDENTITY_H))
					.SetBold()
					.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
					.SetFontSize(11)
					);
				p.Add(new Text("Total Project Cost: ")
					.SetFont(PdfFontFactory.CreateFont("Arial", PdfEncodings.IDENTITY_H))
					.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
					.SetFontSize(11)
					);
				p.Add(new Text("Php 100,000,000.00")
					.SetFont(PdfFontFactory.CreateFont("Arial", PdfEncodings.IDENTITY_H))
					.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
					.SetBold()
					.SetFontSize(11)
					);
				doc.Add(p);
				doc.Close();
				pdfDoc.Close();
			}
			if (System.IO.File.Exists(filePath))
			{
				model.filepath = filePath;
			}

			return View(model);
		}

		public IActionResult DoPrint ()
		{
			return View();
		}

		public ActionResult DownloadFile(DebugModel model)
		{
			// Specify the file path and content type
			string contentType = "application/pdf"; // Change the content type as needed
			var fs = System.IO.File.OpenRead(model.filepath);
			// Return the file for download
			return File(fs, contentType, "AAA.pdf");
		}

		public ActionResult GetMultiplier(string selectedValue, string param1)
		{
			// Perform logic to get the multiplier based on selectedValue
			// For example, you can query a database or use any other data source
			double multiplier = 0;

				using (SqlConnection conn = new SqlConnection(connectionstring))
				{
					conn.Open();
					using (SqlCommand command = new SqlCommand("SELECT supplier_material_price FROM supplier_materials " +
						"WHERE supplier_id = @supplier_id AND material_id = @material_id AND supplier_material_archived = 0 " +
						"AND supplier_material_availability = 1;"))
					{
						command.Parameters.AddWithValue("@supplier_id", Convert.ToInt32(selectedValue));
						command.Parameters.AddWithValue("@material_id", Convert.ToInt32(param1));
						command.Connection = conn;
						multiplier = Convert.ToInt32(command.ExecuteScalar());
					}
			}
			Debug.WriteLine(selectedValue + ";" + param1 + ";" + multiplier);
			return Json(multiplier);
		}
	}
}
