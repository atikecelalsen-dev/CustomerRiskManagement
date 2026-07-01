//namespace CustomerRiskManagement.Models
//{
//    public class CustomerRiskViewModel
//    {

//        public int Id { get; set; }
//        public int SiraNo { get; set; }

//        public string MusteriKodu { get; set; } = "";

//        public string MusteriUnvani { get; set; } = "";

//        public decimal Borc { get; set; }

//        public decimal Alacak { get; set; }

//        public decimal BelirlenenRiskLimiti { get; set; }

//        public decimal MevcutRiskOrani { get; set; }

//        public string Durum { get; set; } = "";
//    }
//}
namespace CustomerRiskManagement.Models
{
    public class CustomerRiskViewModel
    {
        public int SiraNo { get; set; }
        public string MusteriKodu { get; set; } = "";
        public string MusteriUnvani { get; set; } = "";

        public decimal CekToplam { get; set; }
        public decimal SenetToplam { get; set; }
        public decimal GenelToplam { get; set; }

        public decimal TanimliRisk { get; set; }
        public decimal KalanRisk { get; set; }
        public decimal YuzdeDurum { get; set; }

        public string Durum { get; set; } = "";
    }
}