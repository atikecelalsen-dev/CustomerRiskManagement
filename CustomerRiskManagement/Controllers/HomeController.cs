using ClosedXML.Excel;
using CustomerRiskManagement.Helpers;
using CustomerRiskManagement.Models;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CustomerRiskManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly SqlServer _sql;

        public HomeController(SqlServer sql)
        {
            _sql = sql;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult CustomerList(string? search, int page = 1)
        {
            int pageSize = 20;
            int offset = (page - 1) * pageSize;


            string where = "";
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                where = "WHERE CODE LIKE @search OR DEFINITION_ LIKE @search";
                parameters.Add(new SqlParameter("@search", $"%{search}%"));
            }

            string query = $@"
         SELECT
             CODE,
             DEFINITION_,
             CEKTOPLAM,
             SENETTOPLAM,
             GENELTOPLAM,
             TANIMLI_RISK,
             KALAN_RISK,
             YUZDE_DURUM
         FROM dbo.VW_CARIRISK
         {where}
         ORDER BY GENELTOPLAM DESC
         OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
     ";

            parameters.Add(new SqlParameter("@offset", offset));
            parameters.Add(new SqlParameter("@pageSize", pageSize));

            var dt = _sql.GetDataTable(query, parameters.ToArray());

            var model = new List<CustomerRiskViewModel>();
            int sira = offset + 1;

            foreach (DataRow row in dt.Rows)
            {
                decimal yuzdeDurum = row.IsNull("YUZDE_DURUM") ? 0m : Convert.ToDecimal(row["YUZDE_DURUM"]);

                model.Add(new CustomerRiskViewModel
                {
                    SiraNo = sira++,
                    MusteriKodu = row["CODE"]?.ToString() ?? "",
                    MusteriUnvani = row["DEFINITION_"]?.ToString() ?? "",

                    CekToplam = row.IsNull("CEKTOPLAM") ? 0m : Convert.ToDecimal(row["CEKTOPLAM"]),
                    SenetToplam = row.IsNull("SENETTOPLAM") ? 0m : Convert.ToDecimal(row["SENETTOPLAM"]),
                    GenelToplam = row.IsNull("GENELTOPLAM") ? 0m : Convert.ToDecimal(row["GENELTOPLAM"]),
                    TanimliRisk = row.IsNull("TANIMLI_RISK") ? 0m : Convert.ToDecimal(row["TANIMLI_RISK"]),
                    KalanRisk = row.IsNull("KALAN_RISK") ? 0m : Convert.ToDecimal(row["KALAN_RISK"]),
                    YuzdeDurum = yuzdeDurum,

                    Durum = GetDurum(yuzdeDurum)
                });
            }

            ViewBag.Page = page;
            ViewBag.Search = search;

            int totalCount = GetTotalCustomerCount(search);

            ViewBag.TotalPages =
                (int)Math.Ceiling((double)totalCount / pageSize);


            return PartialView("_CustomerRiskList", model);
        }


        public IActionResult DownloadExcel(string? search)
        {
            string where = "";
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                where = "WHERE CODE LIKE @search OR DEFINITION_ LIKE @search";
                parameters.Add(new SqlParameter("@search", $"%{search}%"));
            }

            string query = $@"
                SELECT
                    CODE,
                    DEFINITION_,
                    CEKTOPLAM,
                    SENETTOPLAM,
                    GENELTOPLAM,
                    TANIMLI_RISK,
                    KALAN_RISK,
                    YUZDE_DURUM
                FROM dbo.VW_CARIRISK
                {where}
                ORDER BY GENELTOPLAM DESC;
            ";

            var dt = _sql.GetDataTable(query, parameters.ToArray());

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Müşteri Risk Raporu");

            int row = 2;
            int col = 2;

            ws.Cell(row, col).Value = "Müşteri Risk Raporu";
            ws.Range(row, col, row, col + 8).Merge();
            ws.Cell(row, col).Style.Font.Bold = true;
            ws.Cell(row, col).Style.Font.FontSize = 16;
            ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            row += 2;

            ws.Cell(row, col).Value = "Rapor Tarihi: ";
            ws.Cell(row, col + 1).Value = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            row += 2;

            string[] headers =
            {
                "Sıra",
                "Müşteri Kodu",
                "Müşteri Ünvanı",
                "Çek Toplam",
                "Senet Toplam",
                "Genel Toplam",
                "Tanımlı Risk",
                "Kalan Risk",
                "Risk %",
                "Risk Durumu"
            };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, col + i).Value = headers[i];

            var headerRange = ws.Range(row, col, row, col + headers.Length - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            int sira = 1;

            foreach (DataRow dr in dt.Rows)
            {
                ws.Cell(row, col).Value = sira++;
                ws.Cell(row, col + 1).Value = dr["CODE"]?.ToString();
                ws.Cell(row, col + 2).Value = dr["DEFINITION_"]?.ToString();

                ws.Cell(row, col + 3).Value = dr.IsNull("CEKTOPLAM") ? 0 : Convert.ToDecimal(dr["CEKTOPLAM"]);
                ws.Cell(row, col + 4).Value = dr.IsNull("SENETTOPLAM") ? 0 : Convert.ToDecimal(dr["SENETTOPLAM"]);
                ws.Cell(row, col + 5).Value = dr.IsNull("GENELTOPLAM") ? 0 : Convert.ToDecimal(dr["GENELTOPLAM"]);
                ws.Cell(row, col + 6).Value = dr.IsNull("TANIMLI_RISK") ? 0 : Convert.ToDecimal(dr["TANIMLI_RISK"]);
                ws.Cell(row, col + 7).Value = dr.IsNull("KALAN_RISK") ? 0 : Convert.ToDecimal(dr["KALAN_RISK"]);
                ws.Cell(row, col + 8).Value = dr.IsNull("YUZDE_DURUM") ? 0 : Convert.ToDecimal(dr["YUZDE_DURUM"]);

                ws.Cell(row, col + 9).Value = GetDurum(dr.IsNull("GENELTOPLAM") ? 0 : Convert.ToDecimal(dr["GENELTOPLAM"]), 
                    dr.IsNull("TANIMLI_RISK") ? 0 : Convert.ToDecimal(dr["TANIMLI_RISK"]));

                row++;
            }

            int totalRow = row;

            ws.Cell(totalRow, col + 2).Value = "TOPLAM";
            ws.Cell(totalRow, col + 3).FormulaA1 = $"SUM(E6:E{totalRow - 1})";
            ws.Cell(totalRow, col + 4).FormulaA1 = $"SUM(F6:F{totalRow - 1})";
            ws.Cell(totalRow, col + 5).FormulaA1 = $"SUM(G6:G{totalRow - 1})";
            ws.Cell(totalRow, col + 6).FormulaA1 = $"SUM(H6:H{totalRow - 1})";
            ws.Cell(totalRow, col + 7).FormulaA1 = $"SUM(I6:I{totalRow - 1})";

            var totalRange = ws.Range(totalRow, col, totalRow, col + headers.Length - 1);
            totalRange.Style.Font.Bold = true;
            totalRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            totalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            totalRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Range(5,col+9, row, col+9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var fullRange = ws.Range(6, col, totalRow, col + headers.Length - 1);
            fullRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            fullRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Columns().AdjustToContents();

            ws.Columns(col + 3, col + 7).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 8).Style.NumberFormat.Format = "0.00";

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"MusteriRiskRaporu_{DateTime.Now:yyyyMMddHHmm}.xlsx"
            );
        }


        private int GetTotalCustomerCount(string? search)
        {
            string where = "";
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                where = "WHERE CODE LIKE @search OR DEFINITION_ LIKE @search";
                parameters.Add(new SqlParameter("@search", $"%{search}%"));
            }

            string query = $@"
                SELECT COUNT(*)
                FROM dbo.VW_CARIRISK
                {where}
            ";

            return Convert.ToInt32(
                _sql.ExecuteScalar(query, parameters.ToArray())
            );
        }

        private string GetDurum(decimal risk, decimal limit)
        {
            if (risk <= 0)
                return "Risksiz";

            if (risk < limit * 0.50m)
                return "Düşük Risk";

            if (risk < limit)
                return "Yüksek Risk";

            return "Aşırı Yüksek Risk";
        }

        private decimal GetRandomLimit(int seed)
        {
            var random = new Random(seed);
            int[] limits = { 30, 40, 50, 60, 70 };
            return limits[random.Next(limits.Length)];
        }

        private decimal GetRandomRisk(int seed)
        {
            var random = new Random(seed + 500);
            return random.Next(0, 101);
        }

        private decimal GetRandomAmount(int seed, int min, int max)
        {
            var random = new Random(seed + 1000);
            return random.Next(min, max);
        }

        private string GetDurum(decimal yuzde)
        {
            if (yuzde <= 0)
                return "Risksiz";

            if (yuzde < 50)
                return "Düşük Risk";

            if (yuzde < 100)
                return "Yüksek Risk";

            return "Limit Aşımı";
        }
    }
}