using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HelloWorldClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientConsole console = new ClientConsole();
            console.StartConsole();
        }
    }
}
