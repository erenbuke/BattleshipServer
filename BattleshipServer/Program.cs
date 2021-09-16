using BattleshipServer.Classes;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipServer
{
    class Program
    {
        public static string getMessage(Socket user)
        {
            byte[] b = new byte[500];
            int k = user.Receive(b);
            string message = string.Empty;

            for(int i = 0; i < k; i++)
            {
                message += Convert.ToChar(b[i]);
            }

            Console.WriteLine("'" + message + "' received from " + user.RemoteEndPoint);

            return message;
        }

        public static void sendMessage(Socket user, string message)
        {
            ASCIIEncoding asen = new ASCIIEncoding();
            user.Send(asen.GetBytes(message));
            Console.WriteLine("'" + message + "' send to " + user.RemoteEndPoint);
        }

        static void Main(string[] args)
        {
            IPAddress ip = null;
            int gameend, turnend;
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var tempip in host.AddressList)
            {
                if(tempip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = tempip;
                }
            }

            TcpListener mylist = new TcpListener(ip,5000);
            mylist.Start();

            Console.WriteLine(ip.ToString());
            Console.WriteLine("The server is running at port 5000");
            Console.WriteLine("The local End point is:" + mylist.LocalEndpoint);
            Console.WriteLine("Waiting for connection...\n");

            Socket user1 = mylist.AcceptSocket();
            User player1 = new User();
            player1.username = getMessage(user1);

            Console.WriteLine(user1.RemoteEndPoint + " connected. Username: " + player1.username);
            sendMessage(user1, "Waiting for player2");

            Socket user2 = mylist.AcceptSocket();
            User player2 = new User();
            player2.username = getMessage(user2);

            Console.WriteLine(user2.RemoteEndPoint + " connected. Username: " + player2.username + "\n");

            sendMessage(user1, "ok");
            sendMessage(user2, "ok");

            sendMessage(user1, player2.username);
            sendMessage(user1, player1.username);

            sendMessage(user2, player1.username);
            sendMessage(user2, player2.username);

            gameend = 0;
            turnend = 0;
            string msg1, msg2;

            player1.shipcount = 5;
            player2.shipcount = 5;

            Task chat1;
            Task chat2;
            Task chat3;

            while (gameend == 0)
            {
                sendMessage(user1, player1.username + " your turn-t");

                while (turnend == 0)
                {
                    chat2 = Task.Factory.StartNew(() =>
                    {
                        msg1 = getMessage(user1);
                        msg1 = player1.username + ": " + msg1;

                        if (msg1.EndsWith("-t"))
                        {
                            sendMessage(user2, msg1);
                            sendMessage(user1, msg1);
                        }
                        else
                        {
                            //
                            // Hit Control
                            //
                        }
                    });

                    chat1 = Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            msg2 = getMessage(user2);
                            msg2 = player2.username + ": " + msg2;
                            sendMessage(user1, msg2);
                            sendMessage(user2, msg2);

                            if (chat2.IsCompleted)
                            {
                                break;
                            }
                        }
                    });
                    chat2.Wait();
                }

                sendMessage(user2, player2.username + " your turn.");

                while (turnend == 1)
                {
                    chat3 = Task.Factory.StartNew(() =>
                    {
                        msg2 = getMessage(user2);
                        msg2 = player2.username + ": " + msg2;

                        if (msg2.EndsWith("-t"))
                        {
                            sendMessage(user2, msg2.Substring(0, (msg2.Length - 2)));
                            sendMessage(user1, msg2.Substring(0, (msg2.Length - 2)));
                        }
                        else
                        {
                            //
                            // Hit Control
                            //
                        }
                    });

                    chat2 = Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            msg1 = getMessage(user1);
                            msg1 = player1.username + ": " + msg1;

                            sendMessage(user2, msg1.Substring(0, (msg1.Length - 2)));
                            sendMessage(user1, msg1.Substring(0, (msg1.Length - 2)));

                            if (chat3.IsCompleted)
                            {
                                break;
                            }
                        }
                    });
                }
            }
        }
    }
}
