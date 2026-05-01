using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

using var client = server.AcceptTcpClient();
var stream = client.GetStream();

var requestBa = new byte[256];
await stream.ReadAsync(requestBa);
var request = Encoding.ASCII.GetString(requestBa);
var requestElements = request.Split(" ");
Console.WriteLine(requestElements[1]);

//var mba = new Span<byte>();
//await stream.ReadAsync(mba);
//Encoding.ASCII.GetString(mba);


//string response = "GET /index.html HTTP/1.1\r\nHost: localhost:4221\r\nUser-Agent: curl/7.64.1\r\nAccept: */*\r\n\r\n";
//var encodedResponse = Encoding.ASCII.GetBytes(response);

//await stream.WriteAsync(encodedResponse, 0, encodedResponse.Length);
