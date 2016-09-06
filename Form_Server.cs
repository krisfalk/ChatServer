using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ClientSocket
{
    public class SocketClient
    {
        public Socket _Socket { get; set; }
        public string _Name { get; set; }
        public SocketClient(Socket socket)
        {
            this._Socket = socket;
        }
    } 
    public partial class FormServer : Form
    {
        private  byte[] buffer = new byte[1024];
        public List<SocketClient> clientSockets { get; set; }
        List<string> names = new List<string>();
        Dictionary<string, Socket> dictionary;
        private  Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public FormServer()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            clientSockets = new List<SocketClient>();
            dictionary = new Dictionary<string, Socket>();
        }
        private void Form_Server_Load(object sender, EventArgs e)
        {
            SetupServer();
        }
        private  void SetupServer()
        {
            label_Status.Text="Setting up server . . .";
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 10010));
            serverSocket.Listen(1);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }
        private  void AcceptCallBack(IAsyncResult result)
        {
            Socket socket = serverSocket.EndAccept(result);
            clientSockets.Add(new SocketClient(socket));
            list_Client.Items.Add(socket.RemoteEndPoint.ToString());

            statusLabel.Text = "client: " + clientSockets.Count.ToString();
            label_Status.Text = "Client connected. . .";
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }
        private  void ReceiveCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            string sendersName = "";

            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(result);
                }
                catch (Exception)
                {
                    //disconnected client
                    ClearOutSocket(socket);
                    return;
                }
                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];
                    Array.Copy(buffer, dataBuf, received);
                    string text = Encoding.ASCII.GetString(dataBuf);
                    sendersName = GetSocketsName(socket);
                    label_Status.Text = sendersName + ": " + text;
                    
                    if (text.Contains("@@"))
                    {
                        for (int i = 0; i < list_Client.Items.Count; i++)
                        {
                            if (socket.RemoteEndPoint.ToString().Equals(clientSockets[i]._Socket.RemoteEndPoint.ToString()))
                            {
                                list_Client.Items.RemoveAt(i);
                                list_Client.Items.Insert(i, text.Substring(1, text.Length - 1));
                                list_Client.SetItemChecked(i, true);
                                names.Add(text);
                                try
                                {
                                    dictionary.Add(text, socket);
                                }
                                catch
                                {

                                }
                                clientSockets[i]._Name = text;
                                BroadCastClients();
                                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                                return;
                            }
                        }
                    }
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(clientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            rich_Text.AppendText(clientSockets[i]._Name + ": " + text + "\n");
                        }
                    }
                    CheckForKeywords(text, socket);
                    SendToEveryoneChecked(text, sendersName, socket);
                }
                else
                {
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        if (clientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            clientSockets.RemoveAt(i);
                            statusLabel.Text = "client: " + clientSockets.Count.ToString();
                        }
                    }
                }
            }
            else
            {
            ClearOutSocket(socket);
            }
            //BroadCastClients();
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
        void SendToEveryoneChecked(string text, string sendersName, Socket socket)
        {
            string response = sendersName + ": " + text + "\r\n";

            foreach (string name in dictionary.Keys)
            {
                for (int i = 0; i < list_Client.Items.Count; i++)
                {
                    if (name.Contains(list_Client.Items[i].ToString()))
                    {
                        Socket newSocket = dictionary[name];
                        if (dictionary[name].Connected && list_Client.GetItemChecked(i))
                        {
                            if (dictionary[name] != socket)
                                SendData(dictionary[name], response);
                        }
                    }
                }
            }
        }
        void CheckForKeywords(string text, Socket socket)
        {
            if (text == "disconnect")
            {
                ClearOutSocket(socket);
                return;
            }
            if (text == "show dictionary")
            {
                MessageBox.Show("Dictionary:");
                for (int i = 0; i < clientSockets.Count; i++)
                {
                    MessageBox.Show(Convert.ToString(i + 1) + ". " + clientSockets[i]._Name + "\n");
                }
            }
        }
        void BroadCastClients()
        {
            for (int i = 0; i < names.Count; i++)
            {
                for (int j = 0; j < clientSockets.Count; j++)
                {
                    SendData(clientSockets[j]._Socket, ">> " + names[i].Substring(2, names[i].Length - 2) + "\r\n");
                }
            }
        }
        void ClearOutSocket(Socket socket)
        {
            Socket socketToClear = socket;
            string nameToClear = "";
            foreach(string name in dictionary.Keys)
            {
                if(dictionary[name] == socket)
                {
                    nameToClear = name;
                }
            }
            dictionary.Remove(nameToClear);
            for (int i = 0; i < names.Count; i++)
            {
                if(names[i] == nameToClear)
                {
                    names.RemoveAt(i);
                }
            }
            for (int i = 0; i < clientSockets.Count; i++)
            {
                if(clientSockets[i]._Socket == socketToClear)
                {
                    clientSockets.RemoveAt(i);
                }
            }
            for (int i = 0; i < list_Client.Items.Count; i++)
            {
                if (nameToClear.Contains(list_Client.Items[i].ToString()))
                {
                    list_Client.Items.RemoveAt(i);
                }
            }
            rich_Text.AppendText(nameToClear + " disconnected\r\n");
            SendData(socket, "*****");
        }
        string GetSocketsName(Socket socket)
        {
            foreach(string name in dictionary.Keys)
            {
                if(dictionary[name] == socket)
                {
                    return name.Substring(2, name.Length -2);
                }
            }
            return null;
        }
        void SendData(Socket socket,string dataSend)
        {

            try
            {
                byte[] data = Encoding.ASCII.GetBytes(dataSend);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
            catch
            {
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
        }
        private void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < list_Client.CheckedItems.Count; i++)
            {
                string t = list_Client.CheckedItems[i].ToString();
                for (int j = 0; j < clientSockets.Count; j++)
                {
                    if (clientSockets[j]._Socket.Connected && clientSockets[j]._Name.Equals("@" + t))
                    {
                        SendData(clientSockets[j]._Socket, "Server: " + txt_Text.Text + "\r\n");
                    }
                    else if (!clientSockets[j]._Socket.Connected)
                    {
                        clientSockets.RemoveAt(j);
                    }
                }
            }
            rich_Text.AppendText("Server: " + txt_Text.Text + "\r\n");
        }
    }
}
