namespace Mobile_Web_Api.Model
{
    public class Helmet
    {
        public int? Id { get; set; }
        public string? Helmet_ID { get; set; }
        public DateTime? Disconnect_Time { get; set; }
        public DateTime? Created_On { get; set; }
        public DateTime API_DateTime { get; set; }
        public string? Created_By { get; set; }
        public DateTime? Updated_On { get; set; }
        public string? Updated_By { get; set; }
        public decimal? Latitude { get; set; }
        public string? DATE { get; set; }

        public decimal? Longitude { get; set; }
        public string? Disconnect_Reason_Code { get; set; }
        public string? Vehicle_Type { get; set; }
        public double? SpeedStatus { get; set; }
        public int? IS_Wear_Helmet { get; set; }
        public int? Is_Wrong_Way { get; set; }
        public string? User_Id { get; set; }

        public double? Speed { get; set; }
        public string? STATUS_DESC { get; set; }
        public string? REASON { get; set; }
        public int? COUNT { get; set; }
        public bool? WrongWay { get; set; }


    }
}
