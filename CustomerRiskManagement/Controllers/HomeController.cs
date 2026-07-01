using CustomerRiskManagement.Helpers;
using CustomerRiskManagement.Models;
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
                ORDER BY CODE
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
            ";

            parameters.Add(new SqlParameter("@offset", offset));
            parameters.Add(new SqlParameter("@pageSize", pageSize));

            var dt = _sql.GetDataTable(query, parameters.ToArray());

            var model = new List<CustomerRiskViewModel>();
            int sira = offset + 1;

            foreach (DataRow row in dt.Rows)
            {
                model.Add(new CustomerRiskViewModel
                {
                    SiraNo = sira++,
                    MusteriKodu = row["CODE"]?.ToString() ?? "",
                    MusteriUnvani = row["DEFINITION_"]?.ToString() ?? "",

                    CekToplam = Convert.ToDecimal(row["CEKTOPLAM"]),
                    SenetToplam = Convert.ToDecimal(row["SENETTOPLAM"]),
                    GenelToplam = Convert.ToDecimal(row["GENELTOPLAM"]),
                    TanimliRisk = Convert.ToDecimal(row["TANIMLI_RISK"]),
                    KalanRisk = Convert.ToDecimal(row["KALAN_RISK"]),
                    YuzdeDurum = Convert.ToDecimal(row["YUZDE_DURUM"]),

                    Durum = GetDurum(Convert.ToDecimal(row["YUZDE_DURUM"]))
                });
            }

            ViewBag.Page = page;
            ViewBag.Search = search;

            int totalCount = GetTotalCustomerCount(search);

            ViewBag.TotalPages =
                (int)Math.Ceiling((double)totalCount / pageSize);


            return PartialView("_CustomerRiskList", model);
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