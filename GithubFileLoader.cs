using Godot;
using System;
using System.IO;
using System.Net;

public class GithubFileLoader : Node
{
	private const string url = "https://api.github.com/repos/restsharp/restsharp/releases";
	private const string urlParameters = "";

	public static void MakeRestRequest()
	{		
		var request = (HttpWebRequest)WebRequest.Create(url);
		
		request.Method = "GET";
		request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
		request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
		
		var content = string.Empty;
		
		using (var response = (HttpWebResponse)request.GetResponse())
		{
			using (var stream = response.GetResponseStream())
			{
				using (var reader = new StreamReader(stream)) 
				{
					content = reader.ReadToEnd();
				}
			}
		}
		Console.WriteLine("muh");
		Console.WriteLine(content);
	}
	
	public static void GetCurrentGitMapHash()
	{
		
	}
}
