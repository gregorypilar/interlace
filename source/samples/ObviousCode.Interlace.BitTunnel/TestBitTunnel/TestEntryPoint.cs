using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Core;
using MbUnit.Framework;
using ObviousCode.Interlace.BitTunnelUtilities;

namespace TestBitTunnel
{
    public class TestEntryPoint
    {
        public static void Main(string[] args)
        {
            AutoRunner runner = new AutoRunner();

            runner.Load();
            runner.Run();
            
            runner.ReportToHtml();            
        }        
    }
}
