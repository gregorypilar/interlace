using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KnockServer
{
    public abstract class ConsoleCommand : ICommand
    {
        string[] _acceptedCommands;
        string _args;
        string _usedCommand;

        bool _allowsArguments;        
        int _priority;

        public ConsoleCommand(int priority, bool allowsArguments, params string[] acceptedCommands )
        {
            _acceptedCommands = acceptedCommands;
            _allowsArguments = allowsArguments;
            _priority = priority;
        }

        protected string Arguments
        {
            get
            {
                return _args;
            }
        }

        protected string UsedCommand
        {
            get
            {
                return _usedCommand;
            }
        }

        protected bool HasArguments
        {
            get
            {
                return !(string.IsNullOrEmpty(Arguments));
            }
        }

        #region ICommand Members

        public bool HandlesCommand(CommandContext context)
        {
            foreach (string command in _acceptedCommands)
            {
                if (context.Command.Trim().StartsWith(command))
                {
                    _usedCommand = command;
                    
                    if (
                        context.Command.Trim() == _usedCommand ||
                            (_allowsArguments && context.Command.ToCharArray()[_usedCommand.Length] == ' ')
                        )
                    {
                        if (_allowsArguments)
                        {
                            _args = context.Command.Trim() == command ? "" : context.Command.Trim().Substring(_usedCommand.Length + 1);                                                
                        }

                        return true;
                    }
                }
            }

            return false;
        }


        public abstract bool HandleCommand(CommandContext context);

        public int Priority
        {
            get { return _priority; }
        }

        #endregion

        protected void RequestPrompt()
        {
            if (PromptRequested != null)
            {
                PromptRequested(this, EventArgs.Empty);
            }
        }
        #region ICommand Members

        public event EventHandler PromptRequested;

        #endregion
    }
}
