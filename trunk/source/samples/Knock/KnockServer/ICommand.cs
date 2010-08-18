using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KnockServer
{
    public interface ICommand
    {
        event EventHandler PromptRequested;
        bool HandlesCommand(CommandContext context);
        bool HandleCommand(CommandContext context);

        int Priority { get; }
    }
}
