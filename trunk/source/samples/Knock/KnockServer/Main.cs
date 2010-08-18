using System;
using ObviousCode.Interlace.BitTunnel;
using ObviousCode.Interlace.BitTunnelServer;
using ObviousCode.Interlace.BitTunnelLibrary;
using System.IO;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.Net;
using KnockServer;
using System.Collections.Generic;
using KnockServer.BasicCommands;
using System.Linq;
using KnockServer.ServerActivities;
using TelexplorerServer.Mounting;
using KnockServer.Mounting;

namespace TelexplorerServer
{
	class MainClass
	{
        static string _cursor = "> ";
        static string _currentPath;
        static ServerInstance _server;
        static ClientInstance _localClient;

		static bool _forceQuit = false;

        static List<ICommand> _commands;

        IPAddress _address;
        int _port;

		public static void Main (string[] args)
		{            
			AppSettings settings = new AppSettings();

            settings.ClientConnectionTimeout = 10000;

            if (!LoadSettings(settings, args))
            {
                Console.WriteLine("Bad or no parameters given");
                Console.WriteLine("Usage: Knock -a[ipaddress] -p[port]");

                return;
            }

			_localClient  = new ClientInstance(settings);
            
            _localClient.LostConnection += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs>(_localClient_LostConnection);
            _localClient.ConnectionMade += new EventHandler(_localClient_ConnectionMade);
            _localClient.ConnectionTerminated += new EventHandler(_localClient_ConnectionTerminated);			
			
			Console.Write ("Starting Server ... ");

            _server = new ServerInstance(settings);

            _server.ConnectionMade += new EventHandler(_server_ConnectionMade);
            _server.ConnectionTerminated += new EventHandler(_server_ConnectionTerminated);
            _server.LostConnection += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs>(_server_LostConnection);


            _server.Connect();

            Console.WriteLine();
			Console.WriteLine ("Server started");
            Console.WriteLine();

            LoadCommands();

            MountedFileCache.Cache.Client = _localClient;
            MountedFileCache.Cache.Server = _server;
            MountedFileCache.Cache.PromptRequested += new EventHandler(Cache_PromptRequested);
			try
			{
				Console.Write ("Preparing local client ... ");
                if (!_localClient.Connect())
                {
                    Console.WriteLine("Unable to connect ...");

                    Console.WriteLine("Stopping ...");

                    Console.WriteLine("Bye");

                    Console.ReadLine();

                    return;
                }
			}
			catch (Exception e)
			{
				Console.WriteLine ("Unable to prepare locale client: {0}", e.Message);
				
				Console.ReadLine();

                Console.WriteLine("Bye");
				return;
			}
            
			_currentPath = "/";

            Console.WriteLine();
			Console.WriteLine("Welcome. Type \"quit\" to exit");
            Console.WriteLine();

			string line = "";
			
			Console.Write(Cursor);
			
            while(true)
			{
                if (_forceQuit) break;

                line = Console.ReadLine();

                if (line == string.Empty)
                {
                    Console.Write(Cursor);
                    continue;
                }

                if (line == "quit")
                {
                    ConsoleKeyInfo response;
                    
                    do
                    {
                        Console.WriteLine();
                        Console.Write("Quit? (Y/N)");
                        response = Console.ReadKey();
                    }
                    while (response.Key != ConsoleKey.Y  && response.Key != ConsoleKey.N);

                    if (response.Key == ConsoleKey.Y)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.Write(Cursor);
                        continue;
                    }
                }

				try
				{
					ParseCommand(line);				
				}
				catch (Exception e)
				{
					Console.WriteLine ("System Error: {0}", e.Message);
				}
				
				Console.Write(Cursor);
			}
			_server.Dispose();
			_localClient.Dispose();
			
			Console.WriteLine("Bye");
		}

        private static bool LoadSettings(AppSettings settings, string[] args)
        {            
            Arguments a = new Arguments(args);

            string ip;
            string portString;

            try
            {
            	ip = a.Get("-a");
                portString = a.Get("-p");
            }
            catch (System.Exception ex)
            {
                return false;
            }

            IPAddress address;
            int port;

            if (ip == null || !IPAddress.TryParse(ip, out address))
            {
                return false;
            }

            if (portString == null || !Int32.TryParse(portString, out port))
            {
                return false;
            }

            settings.Port = port;
            settings.ServerAddress = address;

            settings.ServerIsRemote = false;

            return true;
        }
        
        static void Cache_PromptRequested(object sender, EventArgs e)
        {
            Console.Write(Cursor);
        }

        private static void LoadCommands()
        {
            _commands = new List<ICommand>();

            _commands.Add(new PingServer());
            _commands.Add(new ServerLists());
            _commands.Add(new DirectoryMounter());
            _commands.Add(new FileMounter());
            _commands.Add(new ClearConsole());

            foreach(ICommand command in _commands)
            {
                command.PromptRequested += new EventHandler(command_PromptRequested);
            }
        }

        static void command_PromptRequested(object sender, EventArgs e)
        {
            Console.WriteLine(Cursor);
        }

        static void _server_LostConnection(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        static void _server_ConnectionTerminated(object sender, EventArgs e)
        {
            Console.WriteLine("Server Disconnected ... ");
        }

        static void _server_ConnectionMade(object sender, EventArgs e)
        {
            Console.WriteLine("Server Connected");
        }

        static void _localClient_ConnectionTerminated(object sender, EventArgs e)
        {
            Console.WriteLine("Client Connection Terminated");
            _forceQuit = true;
        }

        static void _localClient_ConnectionMade(object sender, EventArgs e)
        {
            Console.WriteLine("Client Connection Made.");
        }

        static void _localClient_LostConnection(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs e)
        {
            Console.WriteLine("Client Lost Connection: {0}", e.ThrownException.Message);
            _forceQuit = true;
        }

		static void ParseCommand (string line)
		{          
            CommandContext context = new CommandContext();

            context.Command = line ;

            context.CurrentPath = CurrentPath;
            context.LocalClient = _localClient;
            context.Server = _server;

            List<ICommand> valid = _commands.Where(c => c.HandlesCommand(context)).ToList();
            
            if (valid.Count == 1)
            {
                valid[0].HandleCommand(context);
                return;
            }
            else if (valid.Count > 1)
            {
                valid[valid.Max(c => c.Priority)].HandleCommand(context);

                return;
            }
            
			if (line.Trim().StartsWith("dir") || line.Trim().StartsWith("ls"))
			{
				ListCurrentDirectory(line.Substring(
				                 line.Trim().StartsWith("dir")?3:2 
				                                    ));
			}
			else if (line.Trim().StartsWith("cd "))
			{
				SwapPath(line.Trim().Substring(3));
			}
			else if (line.Trim()=="fsi")
			{
				ListFileSystemInfo();
			}
			else if (line.Trim()=="path" || line.Trim() =="pwd")
			{
				EchoPath();	
			}			
			else
			{
				Console.WriteLine("Bad command {0}", line);	
			}
		}

		static void EchoPath ()
		{
			Console.WriteLine ("Current Path: {0}", CurrentPath);
		}


		static void ListFileSystemInfo ()
		{
			DirectoryInfo dir = new DirectoryInfo("/");
			
			foreach (FileSystemInfo fsi in dir.GetFileSystemInfos())
			{
				Console.WriteLine (fsi.FullName);	
			}
		}

		static void SwapPath (string path)
		{
			string subPath = Path.Combine(_currentPath, path);
			
			if (path == "..")
			{
				DirectoryInfo parent = Directory.GetParent(_currentPath);

                _currentPath = parent == null ? _currentPath : parent.FullName;				
			}
			else if (Directory.Exists(subPath))
			{
				_currentPath = subPath;	
			}
			else if (Directory.Exists(path))
			{
				_currentPath = path;	
			}
			else
			{
				Console.WriteLine ("Bad path \"{0}\"", path);
				return;
			}			 
		}

		static void ListCurrentDirectory(string command)
		{
			command = command.Trim();
			if (!string.IsNullOrEmpty(command) && command != "-d" && command != "-f")
			{
				Console.WriteLine ("Bad parameter \"{0}\"", command);	
				return;
			}
			
			DirectoryInfo directory = new DirectoryInfo(CurrentPath);
			
			
			if (string.IsNullOrEmpty(command) || command == "-d")
			{
				ListDirectories (directory);
			}
			
			if (string.IsNullOrEmpty(command) || command == "-f")
			{
				ListFiles (directory);
			}
		}
		
		static void ListFiles (DirectoryInfo directory)
		{
			foreach (FileInfo file in directory.GetFiles()) 
			{
				Console.WriteLine (file.Name);	
			}
		}
		
		static void ListDirectories (DirectoryInfo directory)
		{
			foreach(DirectoryInfo subpath in directory.GetDirectories())
			{
				Console.WriteLine (subpath.Name);	
			}
		}

        static string CurrentPath
        {
            get
            {
                DirectoryInfo dir = new DirectoryInfo(_currentPath);

                return dir.FullName;
            }
        }

        static string Cursor
        {
            get
            {
                return string.Format("{0}{1}", CurrentPath, _cursor);
            }
        }
	}
}
