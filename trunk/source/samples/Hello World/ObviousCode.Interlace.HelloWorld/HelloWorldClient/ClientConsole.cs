using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorService;
using HelloWorldClient.Services;

namespace HelloWorldClient
{
    public class ClientConsole
    {
        ServiceHost _host;
        MessageClientService _service;

        public ClientConsole()
        {
            _host = new ServiceHost();
            _service = new MessageClientService();
        }

        public void StartConsole()
        {
            _host.AddService(_service);

            _host.StartServiceHost();            

            StartAcceptingInput();
        }

        private void StartAcceptingInput()
        {
            Console.WriteLine("Press Enter to Connect to Server");
            Console.ReadLine();

            _host.OpenServices();

            Console.WriteLine("Type message and press enter to communicate with Server");
            Console.WriteLine("(\"quit\" to exit)");

            string line = Console.ReadLine();

            while (line != "quit")
            {                
                _service.SendMessage(line);
                line = Console.ReadLine();
            }

            Console.WriteLine("Closing client ...");

            _host.CloseServices();
            _host.StopServiceHost();
        }
    }
}
