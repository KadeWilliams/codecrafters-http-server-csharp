using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

using var client = server.AcceptTcpClient();
var stream = client.GetStream();

var buffer = new byte[1024];
int bytesRead = await stream.ReadAsync(buffer);
Console.WriteLine(bytesRead);
var request = Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\0');
var requestElements = request.Split("\n");
Console.WriteLine("Request Elements");
//Console.WriteLine(string.Join(", ", requestElements));
string output = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n";
foreach (var (v, i) in requestElements.Select((v, i) => (v, i)))
{
    string element = v switch
    {
        string s when s.StartsWith("/echo") => s.Split("/")[2],
        string s when s.StartsWith("/") => s,
        string s when s.StartsWith("User-Agent") => s.Split(" ")[1],
        string s when s.StartsWith("Host") => s.Split(" ")[1],
        _ => v
    };

    Console.WriteLine($"Loop: {i}");
    Console.WriteLine(element);

    //if (v.StartsWith("User-Agent"))
    //{
    //    string userAgent = v.Split(" ")[1];
    //}
}

//var verb = requestElements[0];
//var endpoint = requestElements[1];
//var protocol = requestElements[2];
//var host = requestElements[3];
//var userAgent = requestElements[4].Trim();
//Console.WriteLine(userAgent);
//Console.WriteLine(userAgent.Length);




//var mba = new Span<byte>();
//await stream.ReadAsync(mba);
//Encoding.ASCII.GetString(mba);

//Console.WriteLine("Split Endpoint");
//Console.WriteLine(string.Join(",", endpoint.Split("/")));

//var response = endpoint switch
//{
//string s when s.StartsWith("/echo") => $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {s.Split("/")[2].Length}\r\n\r\n{s.Split("/")[2]}",
//string s when s.StartsWith("/user-agent") => $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Length}\r\n\r\n{userAgent}",
//string s when s.StartsWith("/") && s.Length == 1 => $"HTTP/1.1 200 OK\r\n\r\n",
//_ => "HTTP/1.1 404 Not Found\r\n\r\n"
//};

//var encodedResponse = Encoding.ASCII.GetBytes(response);
//await stream.WriteAsync(encodedResponse, 0, encodedResponse.Length);

//string response = "GET /index.html HTTP/1.1\r\nHost: localhost:4221\r\nUser-Agent: curl/7.64.1\r\nAccept: */*\r\n\r\n";
