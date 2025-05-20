
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using Steamworks;

Console.WriteLine("Starting uploader...");

var appID = new AppId_t(2638050);
var changeNote = ""; // TODO

var configPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "config.json");
var contentPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "dist");
var previewPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "assets", "modPreview.png");

// read config
var jsonOptions = new JsonSerializerOptions { 
    IncludeFields = true,
    WriteIndented = true,
};
Config config;
using (var stream = File.OpenRead(configPath))
    config = JsonSerializer.Deserialize<Config>(stream, jsonOptions);

var m_bInitialized = SteamAPI.Init();
if (!m_bInitialized) {
    Console.WriteLine("[Steamworks.NET] SteamAPI_Init() failed. Is Steam running and you're logged in?");
    return;
}

// start running callbacks in a separate thread
using var wtoken = new CancellationTokenSource();
MySteamAPI.StartRunningCallbacks(wtoken);

// create new item or use existing published file ID
PublishedFileId_t publishedFileID;
if (config.publishedFileID.HasValue) {
    publishedFileID = new PublishedFileId_t(config.publishedFileID.Value);
} else {
    var (res, failure) = await MySteamAPI.CreateItemAsync(appID);
    if (failure || res.m_eResult != EResult.k_EResultOK) {
        Console.WriteLine($"Failed to create item: {res.m_eResult}");
        return;
    }
    publishedFileID = res.m_nPublishedFileId;

    // write published file ID to config
    config.publishedFileID = publishedFileID.m_PublishedFileId;
    File.WriteAllText(configPath, JsonSerializer.Serialize(config, jsonOptions));
}

// update item
var handle = MySteamAPI.StartItemUpdate(appID, publishedFileID);
SteamUGC.SetItemTitle(handle, config.title);
SteamUGC.SetItemDescription(handle, config.description);
SteamUGC.SetItemUpdateLanguage(handle, "english");
SteamUGC.SetItemVisibility(handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic); // TODO
// SteamUGC.SetItemTags(handle, new[] { "tag1", "tag2" });
SteamUGC.SetItemPreview(handle, previewPath);
SteamUGC.SetItemContent(handle, contentPath);

// submit item
var (resUpdate, failureUpdate) = await MySteamAPI.SubmitItemUpdateAsync(handle, changeNote);
if (failureUpdate || resUpdate.m_eResult != EResult.k_EResultOK) {
    Console.WriteLine($"Failed to update item: {resUpdate.m_eResult}");
    return;
} else {
    Console.WriteLine($"Item updated successfully!");
}

wtoken.Cancel();
SteamAPI.Shutdown();
Console.WriteLine("Exiting");


public class Config
{
    public string name;
    public string title;
    public string description;
    public string version;
    public ulong? publishedFileID;
}