using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KnockServer.BasicCommands
{
    public class ClearConsole : ConsoleCommand 
    {
        public ClearConsole() : base(10, false, "cls", "clear-console")
        {

        }

        #region ICommand Members

        
        public override bool HandleCommand(CommandContext context)
        {
            Console.Clear();

            return true;
        }

        #endregion
    }
}
