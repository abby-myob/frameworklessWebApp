using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConsoleApp1
{
    public class Server
    {
        private const string Path = "/geocode/json";

        private readonly List<Location> _locations = new List<Location>
        {
            new Location("melbourne", "-37.814", "144.96332"),
            new Location("auckland", "-36.86667", "174.76667"),
            new Location("sydney", "-33.86785", "151.20732")
        };

        private bool IsCorrectPath(HttpListenerContext context)
        {
            return context.Request.RawUrl.StartsWith(Path);
        }

        public void StartServer()
        {
            var server = new HttpListener();
            server.Prefixes.Add("http://localhost:8080/");
            server.Start();

            while (true)
            {
                var context = server.GetContext(); // Gets the request 
                Console.WriteLine("request...");
                context.Response.ContentType = "application/json";
                context.Response.AppendHeader("Access-Control-Allow-Origin",
                    "http://localhost:63343"); // This could be the IP problem in the cohort problem bruv. 

                byte[] buffer;

                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        buffer = GetMethod(context);
                        break;
                    case "POST":
                        buffer = PostMethod(context);
                        break;
                    case "DELETE":
                        buffer = DeleteMethod(context);
                        break;
                    default:
                        context.Response.StatusCode = 405;
                        buffer = GetErrorBuffer(context);
                        break;
                }


                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        private byte[] GetMethod(HttpListenerContext context)
        {
            if (!IsCorrectPath(context))
            {
                context.Response.StatusCode = 400;
                return GetErrorBuffer(context);
            }

            if (_locations.Any(l => l.Address == context.Request.QueryString["Address"])) // if location is in address
            {
                context.Response.StatusCode = 200;

                var location = _locations.First(l => l.Address == context.Request.QueryString["Address"]);

                return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(location));
            }

            if (IsCorrectPath(context) || context.Request.QueryString.Count == 0) // return all addresses 
            {
                context.Response.StatusCode = 200;

                return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_locations));
            }

            context.Response.StatusCode = 404;
            return GetErrorBuffer(context);
        }

        private byte[] PostMethod(HttpListenerContext context)
        {
            if (!IsCorrectPath(context))
            {
                context.Response.StatusCode = 400;
                return GetErrorBuffer(context);
            }

            System.IO.Stream body = context.Request.InputStream;
            System.Text.Encoding encoding = context.Request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
            var s = reader.ReadToEnd();
            body.Close();
            reader.Close();

            var newLocation = JsonConvert.DeserializeObject<Location>(s);
            _locations.Add(newLocation);
            context.Response.StatusCode = 200;

            return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newLocation));
        }

        private byte[] DeleteMethod(HttpListenerContext context)
        {
//             TODO What's the best way to delete something, do you use query params or just json input??? 
            if (!IsCorrectPath(context))
            {
                context.Response.StatusCode = 400;
                return GetErrorBuffer(context);
            }

            // This could be done in a method as both post and delete use it now. For fetching the json details. 
            System.IO.Stream body = context.Request.InputStream;
            System.Text.Encoding encoding = context.Request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
            var s = reader.ReadToEnd();
            body.Close();
            reader.Close();

            var newLocation = JsonConvert.DeserializeObject<Location>(s);
            var locationToRemove = _locations.First(l => l.Address == newLocation.Address);
            _locations.Remove(locationToRemove);
            context.Response.StatusCode = 200;

            return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newLocation));
        }

        private byte[] GetErrorBuffer(HttpListenerContext context)
        {
            var response = "";
            var statusCode = context.Response.StatusCode;

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
    }
}