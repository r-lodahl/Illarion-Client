using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using File = Godot.File;
using Directory = System.IO.Directory;
using Illarion.Client.Common;
using Illarion.Client.Net;

namespace Illarion.Client.Update
{
    public class ClientUpdater : Node
    {
		private RestClient rest;
		
        public override void _Ready()
        {
			GD.PrintErr("---------------------------------------");
			
			rest = new RestClient();
			
			string localVersion = GetLocalVersion();
			string serverVersion = GetServerVersion();
			
			if (serverVersion.Equals(localVersion)) 
			{
				GD.Print("No Update needed!");
				GetTree().ChangeScene(Constants.UserData.MainScene);
				return;
			}
			GD.Print("Updated is needed!");
			
			ClearMapFolder();
			
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
            mapConverter.CreateMapChunks(String.Concat(OS.GetUserDataDir(), Constants.UserData.ServerMapPath)); 

			UpdateVersion(serverVersion);

			RemoveDownloadsFolder();

            GD.Print("Update finished!");
			GetTree().ChangeScene(Constants.UserData.MainScene);
        }
		
		private void RemoveDownloadsFolder()
		{
			Directory.Delete(String.Concat(OS.GetUserDataDir(), Constants.UserData.ServerMapPath), true);
		}
		
		private void ClearMapFolder()
		{
			string mapDataPath = String.Concat(OS.GetUserDataDir(), Constants.UserData.MapPath);
			
			if (Directory.Exists(mapDataPath)) Directory.Delete(mapDataPath, true);
			
			Directory.CreateDirectory(mapDataPath);
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

            if (!response.IsSuccessful || !response.IsDictionary)
            {
                GD.PrintErr($"Getting map version has failed [{response.Status}]!");
                return "";
            }

            return response.Dictionary["version"];
		}
		
		private bool DownloadMapFiles()
		{
			RestClient.Response response = rest.GetSynchronized(
                Constants.Server.UpdateServerAddress,
                Constants.Server.ZippedMapEndpoint,
                Constants.Server.UpdateServerPort,
                true);

            if (!response.IsSuccessful || !response.IsByteArray)
            {
                GD.PrintErr($"Downloading map files has failed! [{response.Status}]");
                return false;
            }

            using (Stream stream = new MemoryStream((response.ByteArray)))
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
