using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.Erlang;
using Interlace.PropertyLists;
using Interlace.ReactorCore;

namespace ObviousCode.InterlaceApps.HelloWorld.Services.Protocol
{
    public class HelloWorldProtocol : TermProtocol
    {
        protected override void ConnectionMade()
        {
            Console.WriteLine("Connection Made, preparing to greet world ...");            
        }

        protected override void TermReceived(object term)
        {
            try
            {
                string message = (term as Atom).Value;
                
                PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

                dictionary.SetValueFor("Message", "Hello World!");

                SendTerm(
                    Atom.From(dictionary.PersistToString())
                    );
            }
            catch (System.Exception ex)
            {
            	
            }
            
        }

        protected override void ConnectionLost(CloseReason reason)
        {
            Console.WriteLine("Connection Lost: {0}", reason);

            base.ConnectionLost(reason);
        }
    }
}
