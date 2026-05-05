using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

using var client = server.AcceptTcpClient();
var stream = client.GetStream();

var requestBa = new byte[1024];
await stream.ReadAsync(requestBa);
var request = Encoding.ASCII.GetString(requestBa);
var requestElements = request.Split(" ");
Console.WriteLine("Request Elements");
//Console.WriteLine(string.Join(", ", requestElements));
//foreach (var element in requestElements)
//{
//    Console.WriteLine(element);
//}

var verb = requestElements[0];
var endpoint = requestElements[1];
var protocol = requestElements[2];
var host = requestElements[4];
var userAgent = requestElements[6];



//var mba = new Span<byte>();
//await stream.ReadAsync(mba);
//Encoding.ASCII.GetString(mba);

//Console.WriteLine("Split Endpoint");
//Console.WriteLine(string.Join(",", endpoint.Split("/")));

var response = endpoint switch
{
    string s when s.StartsWith("/echo") => $"{protocol} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {s.Split("/")[2].Length}\r\n\r\n{s.Split("/")[2]}",
    string s when s.StartsWith("/user-agent") => $"{protocol} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Length}\r\n\r\n",
    string s when s.StartsWith("/") && s.Length == 1 => $"{protocol} 200 OK\r\n\r\n",
    _ => "HTTP/1.1 404 Not Found\r\n\r\n"
};

var encodedResponse = Encoding.ASCII.GetBytes(response);
await stream.WriteAsync(encodedResponse, 0, encodedResponse.Length);

//string response = "GET /index.html HTTP/1.1\r\nHost: localhost:4221\r\nUser-Agent: curl/7.64.1\r\nAccept: */*\r\n\r\n";
