using Godot;
using System;

public class UnzipHelper : Node
{
	public static void UnzipFileToFolder(string file, string outputPath)
	{
		using(var unzip = new Unzip(file))
		{
			unzip.ExtractToDirectory(outputPath);
		}
	}
}
