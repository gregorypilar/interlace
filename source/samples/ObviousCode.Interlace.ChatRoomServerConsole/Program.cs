using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorService;
using ObviousCode.Interlace.ChatroomServer;

namespace ObviousCode.Interlace.ChatRoomServerConsole
{
    class Program
    {        
        static void Main(string[] args)
        {
            ChatroomSettings settings = new ChatroomSettings();
            
            //TODO:Add parameters to set settings

            StartChatRoomServer(settings);
        }

        private static void StartChatRoomServer(ChatroomSettings settings)
        {
            ServiceHost host = new ServiceHost();

            host.AddService(new ChatroomService(settings));
           
            host.StartServiceHost();
            host.OpenServices();

            Console.WriteLine("Type \"quit\" to shut down server");

            string input = null;

            do  
            {
                input = Console.ReadLine();
                Console.WriteLine(input);
                //TODO: Implement Console Commands
            } 
            while (input != "quit");

            host.CloseServices();
            host.StopServiceHost();
        }
    }
}
