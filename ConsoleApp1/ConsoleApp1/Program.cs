using System;
using System.Collections.Generic;
using System.Net;

namespace ConsoleApp1
{
    internal static class Program
    {
        private static void Main()
        {
            var locations = new Dictionary<string, Tuple<string, string>>
            {
                {"melbourne", new Tuple<string, string>("-37.814", "144.96332")},
                {"auckland", new Tuple<string, string>("-36.86667", "174.76667")},
                {"sydney", new Tuple<string, string>("-33.86785", "151.20732")}
            };
            
            var server = new HttpListener();
            server.Prefixes.Add("http://localhost:8080/");
            server.Start();
 
            while (true)
            {
                var context = server.GetContext(); // Gets the request 
                var queryString = context.Request.QueryString;
                var city = queryString["address"];

                var response = context.Response; // Create the Response
                byte[] buffer;


                if (context.Request.HttpMethod == "GET")
                {
                    if (!IsCorrectPath(context))
                    {
                        response.StatusCode = 400;
                        buffer = GetErrorBuffer(response.StatusCode);
                    }
                    else
                    {
                        if (locations.ContainsKey(city))
                        { 
                            response.StatusCode = 200;
                            response.ContentType = "application/json"; 
                            buffer = GetSuccessBuffer(city, locations);
                        }
                        else
                        {
                            response.StatusCode = 404;
                            buffer = GetErrorBuffer(response.StatusCode);
                        }
                    }
                }
                else
                {
                    response.StatusCode = 405;
                    buffer = GetErrorBuffer(response.StatusCode);
                }

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        private static byte[] GetSuccessBuffer(string city, Dictionary<string, Tuple<string, string>> locations)
        {
            var (latitude, longitude) = locations[city];

            var response = $"{{ \"location\": {city}, \"latitude\" : {latitude}, \"longitude\" : {longitude} }}";
            
            return System.Text.Encoding.UTF8.GetBytes(response);
        }

        private static byte[] GetErrorBuffer(int statusCode)
        {
            var response = "";

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
            }

            return System.Text.Encoding.UTF8.GetBytes(response);
        }

        private static bool IsCorrectPath(HttpListenerContext context)
        {
            var path = context.Request.RawUrl.Split("/");
            if (path[1] != "geocode") return false;
            var secondHalf = path[2].Split("?");
            return secondHalf[0] == "json";
        }
    }
}