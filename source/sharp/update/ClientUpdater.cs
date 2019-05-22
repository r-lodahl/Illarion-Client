using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using File = Godot.File;

public class ClientUpdater : Node
{
    public override void _Ready()
    {
        RestClient rest = new RestClient();
        RestClient.Response response = rest.GetSynchronized(
            Constants.Server.UpdateServerAddress,
            Constants.Server.MapVersionEndpoint,
            Constants.Server.UpdateServerPort,
            true);

        if (response.Error != Error.Ok || !response.IsDictionary)
        {
            GD.PrintErr($"Getting map version has failed [{response.Error}]! Please retry.");
            return;
        }

        string version = (string)((Godot.Collections.Dictionary)response.Data)["version"];

        bool outdated = true;
        File versionFile = new File();

        if (versionFile.FileExists(Constants.UserData.VersionPath))
        {
            versionFile.Open(Constants.UserData.VersionPath, (int) File.ModeFlags.Read);
            string localVersion = versionFile.GetAsText();
            versionFile.Close();
            outdated = !version.Equals(localVersion);
        }

        if (!outdated) GetTree().ChangeScene(Constants.UserData.MainScene);

        response = rest.GetSynchronized(
            Constants.Server.UpdateServerAddress,
            Constants.Server.ZippedMapEndpoint,
            Constants.Server.UpdateServerPort,
            true);

        if (response.Error != Error.Ok)
        {
            GD.PrintErr($"Getting map version has failed! [{response.Error}]");
            return;
        }

        using (Stream stream = new MemoryStream(((List<byte>)response.Data).ToArray()))
        {
            using (Unzip unzip = new Unzip(stream))
            {
                unzip.ExtractToDirectory(OS.GetUserDataDir());
            }
        }

        versionFile.Open(Constants.UserData.VersionPath, (int)File.ModeFlags.Write);
        versionFile.StoreString(version);
        versionFile.Close();

        TableConverter tableConverter = new TableConverter(Constants.UserData.TilesetPath);
        var tileDictionary = tableConverter.CreateTileMapping();
        var overlayDictionary = tableConverter.CreateOverlayMapping();

        MapConverter mapConverter = new MapConverter(tileDictionary, overlayDictionary);
        mapConverter.CreateMapChunks(String.Concat(OS.GetUserDataDir(),Constants.UserData.MapPath)); 

        GetTree().ChangeScene(Constants.UserData.MainScene);
    }
}
