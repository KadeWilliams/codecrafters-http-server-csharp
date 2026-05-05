using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true)
{
    var client = server.AcceptTcpClient();
    Task.Run(async () =>
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer);

        var request = Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\0');
        var requestElements = request.Split("\n");

        var outDict = new Dictionary<string, string>();
        foreach (var (v, i) in requestElements.Select((v, i) => (v, i)))
        {
            if (i == 0)
            {
                outDict.Add("endpoint", v.Split(" ")[1]);
                continue;
            }

            switch (v)
            {
                case var s when s.StartsWith("User-Agent"):
                    var agent = s.Split(" ")[1];
                    outDict.Add("User-Agent", agent);
                    break;
            }
        }
        string output;

        var endpoint = outDict["endpoint"];
        if (endpoint.Contains("echo"))
        {
            output = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {endpoint.Split("/")[2].Length}\r\n\r\n{endpoint.Split("/")[2]}";
        }
        else if (endpoint == "/")
        {
            output = "HTTP/1.1 200 OK\r\n\r\n";
        }
        else if (endpoint == "/user-agent")
        {
            var userAgent = outDict["User-Agent"];
            output = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Trim().Length}\r\n\r\n{userAgent}";
        }
        else
        {
            output = "HTTP/1.1 404 Not Found\r\n\r\n";
        }

        //Console.WriteLine(JsonSerializer.Serialize(outDict));

        var encodedResponse = Encoding.ASCII.GetBytes(output);
        await stream.WriteAsync(encodedResponse, 0, encodedResponse.Length);
        client.Close();
    });
}


