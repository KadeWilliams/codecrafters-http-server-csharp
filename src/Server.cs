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

        Console.WriteLine($"All args: {JsonSerializer.Serialize(args)}");
        var root = "";
        string contents = "";
        if (args.Length > 0)
        {
            if (args[0] == "--directory")
            {
                root = args[1];
            }
        }
        var stream = client.GetStream();
        var buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer);

        var request = Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\0');
        var requestElements = request.Split("\n");
        Console.WriteLine($"Request Elements: {JsonSerializer.Serialize(requestElements)}");

        var outDict = new Dictionary<string, string>();
        foreach (var (v, i) in requestElements.Select((v, i) => (v, i)))
        {
            if (i == 0)
            {
                var fullRequest = v.Split(" ");
                outDict.Add("verb", fullRequest[0]);
                outDict.Add("endpoint", fullRequest[1]);
                outDict.Add("contents", requestElements[requestElements.Length - 1]);
                continue;
            }

            switch (v)
            {
                case var s when s.StartsWith("User-Agent"):
                    var agent = s.Split(" ")[1];
                    outDict.Add("User-Agent", agent);
                    break;
                case var s when s.StartsWith("Accept-Encoding"):
                    var encoding = s.Split(" ")[1];
                    outDict.Add("Accept-Encoding", encoding);
                    break;
            }
        }
        string output = "";

        var outputList = new List<string>();
        var endpoint = outDict["endpoint"];
        //outputList.Add(endpoint);

        if (outDict["verb"] == "POST")
        {
            outputList.Insert(0, "HTTP/1.1 201 Created\r\n");
        }
        else if (outDict["verb"] == "GET")
        {
            outputList.Insert(0, "HTTP/1.1 200 OK\r\n");
        }

        if (outDict.ContainsKey("Accept-Encoding") && outDict["Accept-Encoding"].Trim() == "gzip")
        {
            outputList.Add($"Content-Encoding: {outDict["Accept-Encoding"].Trim()}\r\n");
        }

        if (endpoint.Contains("echo"))
        {
            outputList.Add($"Content-Type: text/plain\r\nContent-Length: {endpoint.Split("/")[2].Length}\r\n\r\n{endpoint.Split("/")[2]}");
        }
        else if (endpoint == "/user-agent")
        {
            var userAgent = outDict["User-Agent"];
            outputList.Add($"Content-Type: text/plain\r\nContent-Length: {userAgent.Trim().Length}\r\n\r\n{userAgent}");
        }
        else if (endpoint.Contains("/files"))
        {
            var fileName = endpoint.Split("/")[2];
            var fullPath = root + fileName;
            if (!File.Exists(fullPath) && !(outDict["verb"] == "POST"))
            {
                outputList.Clear();
                outputList.Add("HTTP/1.1 404 Not Found\r\n");
            }
            else
            {
                if (outDict["verb"] == "POST")
                {
                    contents = outDict["contents"];
                    File.WriteAllText(fullPath, contents);
                }
                else
                {
                    var fileContents = File.ReadAllText(fullPath);
                    outputList.Add($"Content-Type: application/octet-stream\r\nContent-Length: {fileContents.Length}\r\n\r\n{fileContents}");
                }
            }
        }
        else if (endpoint != "/")
        {
            outputList.Clear();
            outputList.Add("HTTP/1.1 404 Not Found\r\n");
        }

        //var endpoint = outDict["endpoint"];
        //if (endpoint.Contains("echo"))
        //{
        //    output = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {endpoint.Split("/")[2].Length}\r\n\r\n{endpoint.Split("/")[2]}";
        //}
        //else if (endpoint == "/")
        //{
        //    output = "HTTP/1.1 200 OK\r\n\r\n";
        //}
        //else if (endpoint == "/user-agent")
        //{
        //    var userAgent = outDict["User-Agent"];
        //    output = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Trim().Length}\r\n\r\n{userAgent}";
        //}
        //else if (endpoint.Contains("/files"))
        //{
        //    var fileName = endpoint.Split("/")[2];
        //    //Console.WriteLine(fileName);
        //    //Console.WriteLine(root + "/" + fileName);
        //    var fullPath = root + fileName;
        //    if (!File.Exists(fullPath) && !(outDict["verb"] == "POST"))
        //    {
        //        output = "HTTP/1.1 404 Not Found\r\n\r\n";
        //    }
        //    else
        //    {
        //        if (outDict["verb"] == "POST")
        //        {
        //            contents = outDict["contents"];
        //            File.WriteAllText(fullPath, contents);
        //            output = $"HTTP/1.1 201 Created\r\n\r\n";
        //        }
        //        else
        //        {
        //            var fileContents = File.ReadAllText(fullPath);
        //            output = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContents.Length}\r\n\r\n{fileContents}";
        //        }
        //    }
        //}
        //else
        //{
        //    output = "HTTP/1.1 404 Not Found\r\n\r\n";
        //}

        Console.WriteLine($"{string.Join("", outputList)}");
        output = string.Join("", outputList);
        var encodedResponse = Encoding.ASCII.GetBytes(output);
        Console.WriteLine($"{encodedResponse[0]} {encodedResponse[1]}");
        await stream.WriteAsync(encodedResponse, 0, encodedResponse.Length);
        client.Close();
    });
}


