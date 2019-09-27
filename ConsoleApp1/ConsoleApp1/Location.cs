namespace ConsoleApp1
{
    public class Location
    {
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; } 

        public Location(string name, string latitude, string longitude)
        {
            Address = name;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}