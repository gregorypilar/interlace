using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ObviousCode.Interlace.ClientApplication.Properties;
using ObviousCode.Interlace.Chatroom.Library;
using Interlace.PropertyLists;

namespace ObviousCode.Interlace.ClientApplication
{
    public partial class MainForm : Form
    {       
        private delegate void MessageReceivedDelegate(PropertyDictionary message);       

        Dictionary<string, MessageReceivedDelegate> _messageHandlers;
        BindingList<string> _chats;

        public MainForm()
        {
            InitializeComponent();            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Client.Adapter.PropertyChanged += new PropertyChangedEventHandler(Adapter_PropertyChanged);
            Client.Adapter.MessageReceived += new EventHandler<MessageReceivedEventArgs>(Adapter_MessageReceived);
            Client.Adapter.ServerConnectionFailed += new EventHandler(Adapter_ServerConnectionFailed);

            LoadMessageHandlers();

            _changeConnectionStatus.Select();

            _chats = new BindingList<string>();
            _chat.DataSource = _chats;            
        }
        
        private void LoadMessageHandlers()//Split out to plugin model
        {
            _messageHandlers = new Dictionary<string, MessageReceivedDelegate>();

            _messageHandlers[ChatroomKeys.LoginRequest] = LogInMessageReceived;
            _messageHandlers[ChatroomKeys.Broadcast] = BroadcastMessageReceived;
            _messageHandlers[ChatroomKeys.Message] = ServerMessageReceived;
            _messageHandlers[ChatroomKeys.ChatterListUpdate] = UpdateUserList;
        }

        private void UpdateOnLogStatusChange()
        {
            _changeConnectionStatus.Text = Client.Adapter.LoggedIn ? "Log Out" : "Login";
            _status.Text = Client.Adapter.LoggedIn ? "Connected to Chat Server" : "Not Connected";
            _changeConnectionStatus.Image = Client.Adapter.LoggedIn ? Resources.StopHS : Resources.PlayHS;
            _messageInput.Enabled = Client.Adapter.LoggedIn;
            _chat.Enabled = Client.Adapter.LoggedIn;
        }

        private void ServerMessageReceived(PropertyDictionary message)
        {            
            if (!message.HasStringFor(ChatroomKeys.Message)) return;//Ignore potential hacked message
            
            string chat = message.StringFor(ChatroomKeys.Message);

            AddToChat(chat);
        }        

        private void BroadcastMessageReceived(PropertyDictionary message)
        {
            if (!message.HasStringFor(ChatroomKeys.SenderName)) return;//Ignore potential hacked message
            if (!message.HasStringFor(ChatroomKeys.Message)) return;//Ignore potential hacked message

            string name = message.StringFor(ChatroomKeys.SenderName);
            string chat = message.StringFor(ChatroomKeys.Message);
            
            AddToChat(string.Format("{0}: {1}", name, chat));
        }

        private void UpdateUserList(PropertyDictionary message)
        {            
            if (!message.HasArrayFor(ChatroomKeys.Message)) return;//Ignore potential hacked message

            PropertyArray chatters = message.ArrayFor(ChatroomKeys.Message);

            _chatterList.Nodes.Clear();

            foreach (string chatter in chatters)
            {
                _chatterList.Nodes.Add(chatter);
            }
        }

        private void AddToChat(string chat)
        {
            _chats.Add(chat);
        }

        private void LogInMessageReceived(PropertyDictionary message)
        {
            string dialogText = "Log in response received, however contained no message";

            if (message.HasStringFor(ChatroomKeys.Message))
            {
                switch (message.StringFor(ChatroomKeys.Message))
                {
                    case ChatroomKeys.LoginSuccess:
                       
                        _messageInput.Focus();

                        return;//No message required. Start chatting.
                        
                    case ChatroomKeys.LoginFail_UserNameInUse:

                        dialogText = "Requested username already being used";
                        break;
                    case ChatroomKeys.LoginFail_TooManyClients:

                        dialogText = "Sorry, chat room is full. Please try again later.";
                        break;

                    default:
                        break;
                }
            }

            MessageBox.Show(dialogText);
        }        

        void Adapter_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<MessageReceivedEventArgs>(Adapter_MessageReceived), new object[] { sender, e });                
                return;
            }

            if (!e.Message.HasStringFor(ChatroomKeys.MessageType))
            {
                MessageBox.Show("Message Received. Unable to determine type");
                return;
            }

            string key = e.Message.StringFor(ChatroomKeys.MessageType);

            if (!_messageHandlers.ContainsKey(key))
            {
                MessageBox.Show(string.Format("Message Received of type {0}. No handler available.", key));
                return;
            }
           
            _messageHandlers[key](e.Message);
        }

        void Adapter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new PropertyChangedEventHandler(Adapter_PropertyChanged), sender, e);
                return;
            }

            if (e.PropertyName == "LoggedIn")
            {
                UpdateOnLogStatusChange();
            }
        }

        void Adapter_ServerConnectionFailed(object sender, EventArgs e)
        {
            MessageBox.Show("Connection to server lost");
        }

        private void _changeConnectionStatus_Click(object sender, EventArgs e)
        {
            if (Client.Adapter.LoggedIn)
            {
                if (MessageBox.Show(this, "Logging out of Interlace Chat?", "Log Out?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    Logout();
                }
            }
            else
            {
                Login();
            }
        }

        private void Logout()
        {
            if (Client.Adapter.HostStarted)
            {                
                Client.Adapter.Logout();
            }
            _chatterList.Nodes.Clear();
        }

        private void Login()
        {
            if (!Client.Adapter.HostStarted)
            {
                Client.Adapter.StartHost();
            }

            UsernameRequestDialog dialog = new UsernameRequestDialog();

            dialog.StartPosition = FormStartPosition.CenterParent;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                string status = Client.Adapter.RequestLogin(dialog.RequestedUsername);

                if (status == ChatroomKeys.LoginFail_ServerNotConnected)
                {
                    MessageBox.Show("Not connected to Server");
                }
            }
        }

        private void _mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logout();
        }

        private void _messageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Client.Adapter.SendMessage(_messageInput.Text);

                _messageInput.Text = string.Empty;
                
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                _messageInput.Text = string.Empty;
                
                return;
            }
        }

    }
}
