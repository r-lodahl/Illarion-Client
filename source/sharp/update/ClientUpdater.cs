using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using File = Godot.File;
using Illarion.Client.Common;
using Illarion.Client.Net;

namespace Illarion.Client.Update
{
    public class ClientUpdater : Node
    {
		private RestClient rest;
		
        public override void _Ready()
        {
			rest = new RestClient();
			
			string localVersion = GetLocalVersion();
			string serverVersion = GetServerVersion();
			
			if (serverVersion.Equals(localVersion)) 
			{
				GD.Print("No Update needed!");
				//GetTree().ChangeScene(Constants.UserData.MainScene);
				return;
			}
			GD.Print("Updated is needed!");
			
			if (!DownloadMapFiles())
			{
				GD.Print("Update was not successfull.");
				return;
			}
			GD.Print("Map Download successfull.");
			
			TableConverter tableConverter = new TableConverter(Constants.UserData.TilesetPath);
            var tileDictionary = tableConverter.CreateTileMapping();
            var overlayDictionary = tableConverter.CreateOverlayMapping();

            MapConverter mapConverter = new MapConverter(tileDictionary, overlayDictionary);
            mapConverter.CreateMapChunks(String.Concat(OS.GetUserDataDir(),Constants.UserData.MapPath)); 

			UpdateVersion(serverVersion);

            GD.Print("Update finished!");
			//GetTree().ChangeScene(Constants.UserData.MainScene);
        }
		
		private string GetLocalVersion()
		{
			 File versionFile = new File();
		     if (!versionFile.FileExists(Constants.UserData.VersionPath)) return "";
			 
			 versionFile.Open(Constants.UserData.VersionPath, (int) File.ModeFlags.Read);
			 string version = versionFile.GetAsText();
			 versionFile.Close();
			 
			 return version;
		}
		
		private string GetServerVersion() 
		{
            RestClient.Response response = rest.GetSynchronized(
                Constants.Server.UpdateServerAddress,
                Constants.Server.MapVersionEndpoint,
                Constants.Server.UpdateServerPort,
                true);

            if (response.Error != Error.Ok || !response.IsDictionary)
            {
                GD.PrintErr($"Getting map version has failed [{response.Error}]!");
                return "";
            }

            return (string)((Godot.Collections.Dictionary)response.Data)["version"];
		}
		
		private bool DownloadMapFiles()
		{
			RestClient.Response response = rest.GetSynchronized(
                Constants.Server.UpdateServerAddress,
                Constants.Server.ZippedMapEndpoint,
                Constants.Server.UpdateServerPort,
                true);

            if (response.Error != Error.Ok)
            {
                GD.PrintErr($"Downloading map files has failed! [{response.Error}]");
                return false;
            }

            using (Stream stream = new MemoryStream(((List<byte>)response.Data).ToArray()))
            {
                using (Unzip unzip = new Unzip(stream))
                {
                    unzip.ExtractToDirectory(OS.GetUserDataDir());
                }
            }

            return true;
		}
		
		private void UpdateVersion(string version)
		{
			File versionFile = new File();
			versionFile.Open(Constants.UserData.VersionPath, (int)File.ModeFlags.Write);
            versionFile.StoreString(version);
            versionFile.Close();
		}
    }
}
