using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ConsoleApp1
{
    internal static class Program
    {
        private static readonly List<Location> Locations = new List<Location>
        {
            new Location("melbourne","-37.814", "144.96332"), 
            new Location("auckland", "-36.86667", "174.76667"),
            new Location("sydney","-33.86785", "151.20732")
        };
        
        private static void Main()
        { 
            var server = new HttpListener();
            server.Prefixes.Add("http://localhost:8080/");
            server.Start();
 
            while (true)
            {
                var context = server.GetContext(); // Gets the request   
                context.Response.ContentType = "application/json";
                var buffer = Buffer(context);

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        private static byte[] Buffer(HttpListenerContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "GET" when !IsCorrectPath(context):
                    context.Response.StatusCode = 400;
                    break;
                case "GET" when Locations.Any(l => l.Name == context.Request.QueryString["address"]):
                    context.Response.StatusCode = 200;
                    break;
                case "GET":
                    context.Response.StatusCode = 404;
                    break;
                case "POST" when !IsCorrectPath(context):
                    context.Response.StatusCode = 400;
                    break;
//                case "POST":
//                {
////                    var loc = new Tuple<string, string>(
////                        context.Request.QueryString["latitude"], 
////                        context.Request.QueryString["longitude"]);
////
////                    Locations.Add(context.Request.QueryString["address"], loc); 
////                    
////                    return System.Text.Encoding.UTF8.GetBytes("location added to locations");
//                }
                    

                default:
                    context.Response.StatusCode = 405;
                    break;
            }

            return GetBuffer(context);
        } 

        private static byte[] GetBuffer(HttpListenerContext context)
        {
            var response = "";
            var statusCode = context.Response.StatusCode; 
            var loc = context.Request.QueryString["address"];
            
            switch (statusCode)
            {
                case 400:
                    response = $"Error {statusCode}: Incorrect Path";
                    break;
                case 404:
                    response = $"Error {statusCode}: Could Not Find Resource";
                    break;
                case 405:
                    response = $"Error {statusCode}: Method Not Allowed";
                    break;
                case 200:
                    var location = Locations.First(l => l.Name == loc);
                    response = $"{{ \"location\": {location.Name}, \"latitude\" : {location.Latitude}, \"longitude\" : {location.Longitude} }}";
                    break;
            } 
            
            return System.Text.Encoding.UTF8.GetBytes(response);
        }

        private static bool IsCorrectPath(HttpListenerContext context)
        {
            return context.Request.RawUrl.StartsWith("/geocode/json");
        }
    }

    internal class Location
    {
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public Location(string name, string latitude, string longitude)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}