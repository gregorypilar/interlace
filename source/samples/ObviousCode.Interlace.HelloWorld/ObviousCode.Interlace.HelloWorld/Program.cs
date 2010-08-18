using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorService;
using ObviousCode.InterlaceApps.HelloWorld.Services;

namespace ObviousCode.InterlaceApps.HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            StartService();
        }

        private static void StartService()
        {
            ServiceHost host = new ServiceHost();

            host.AddService(new HelloWorldService());

            host.StartServiceHost();
            host.OpenServices();

            Console.WriteLine("Press enter to stop server");
            Console.ReadLine();

            host.CloseServices();
            host.StopServiceHost();
        }
    }
}
