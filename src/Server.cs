using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
server.AcceptSocket(); // wait for client

using var client = server.AcceptTcpClient();

var stream = client.GetStream();

string response = "HTTP/1.1 200 OK\r\n\r\n";
var encodedResponse = Encoding.ASCII.GetBytes(response);

stream.Write(encodedResponse);
stream.Flush();
