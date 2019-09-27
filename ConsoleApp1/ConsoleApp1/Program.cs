using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    internal static class Program
    { 
        private static void Main()
        {
            var server = new Server(); 

            server.StartServer();  
        }  
    } 
}