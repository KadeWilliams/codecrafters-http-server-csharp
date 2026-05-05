using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

using var client = server.AcceptTcpClient();
var stream = client.GetStream();

var buffer = new byte[1024];
int bytesRead = await stream.ReadAsync(buffer);
//Console.WriteLine(bytesRead);
var request = Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\0');
var requestElements = request.Split("\n");
//Console.WriteLine("Request Elements");
//Console.WriteLine(string.Join(", ", requestElements));
//string output = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n";
var outDict = new Dictionary<string, string>();
foreach (var (v, i) in requestElements.Select((v, i) => (v, i)))
{
    if (i == 0)
    {
        //Console.WriteLine("1");
        outDict.Add("endpoint", v.Split(" ")[1]);
        continue;
    }

    switch (v)
    {
        case var s when s.StartsWith("User-Agent"):
            Console.WriteLine("2");
            var agent = s.Split(" ")[1];
            outDict.Add("User-Agent", agent);
            break;
    }
}
string output;

var endpoint = outDict["endpoint"];
if (endpoint.Contains("echo"))
{
    //do something
    Console.WriteLine("3");
    output = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {endpoint.Split("/")[2].Length}\r\n\r\n{endpoint.Split("/")[2]}";
}
else if (endpoint == "/")
{
    // do something
    output = "HTTP/1.1 200 OK\r\n\r\n";
}
else if (endpoint == "/user-agent")
{
    Console.WriteLine("4");
    var userAgent = outDict["User-Agent"];
    output = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Length}\r\n\r\n{userAgent}";
}
else
{
    output = "HTTP/1.1 404 Not Found\r\n\r\n";
}

//Console.WriteLine(JsonSerializer.Serialize(outDict));

var encodedResponse = Encoding.ASCII.GetBytes(output);
await stream.WriteAsync(encodedResponse, 0, encodedResponse.Length);


//foreach (var (v, i) in requestElements.Select((v, i) => (v, i)))
//{
//    Console.WriteLine(v);
//    string element = v switch
//    {
//        string s when s.StartsWith("GET") => s.Split(" ")[1], // endpoint
//        string s when s.StartsWith("/echo") => s.Split("/")[2], // {str}
//        string s when s.StartsWith("/") => s, // root path
//        string s when s.StartsWith("User-Agent") => s.Split(" ")[1], // user agent 
//        string s when s.StartsWith("Host") => s.Split(" ")[1]+"\r\n", // 
//        _ => string.Empty
//    };

//    Console.WriteLine(element);

//    if (!string.IsNullOrEmpty(element))
//    {
//        output.Add(element);
//    }


//    //Console.WriteLine($"Loop: {i}");
//    //Console.WriteLine(element);

//    //if (v.StartsWith("User-Agent"))
//    //{
//    //    string userAgent = v.Split(" ")[1];
//    //}
//}

//if (output.Count < 1)
//{
//    output.Insert(0, "HTTP/1.1 404 Not Found\r\n\r\n");
//}
//else
//{
//    output.Insert(0, "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n");
//}

//Console.WriteLine(string.Join("", output));

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
