using System.Threading;
using System.Threading.Tasks;
using Steamworks;

public static class MySteamAPI 
{
    public static void StartRunningCallbacks(CancellationTokenSource wtoken) {
        var _ = Task.Run(async () =>
        {
            do
            {
                SteamAPI.RunCallbacks();
                await Task.Delay(100, wtoken.Token);

            } while (!wtoken.Token.IsCancellationRequested);
        }, wtoken.Token);
    }

    public static async Task<(CreateItemResult_t result, bool failure)> CreateItemAsync(AppId_t appID) {

        var isDone = false;
        var res = new CreateItemResult_t();
        bool f = true;
        var cr = CallResult<CreateItemResult_t>.Create(
            (result, bIOFailure) =>
            {
                res = result;
                f = bIOFailure;
                isDone = true;
            });
        var call = SteamUGC.CreateItem(appID, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
        cr.Set(call);

        do {
            await Task.Delay(100);
        } while (!isDone);

        return (res, f);
    }
    
    public static UGCUpdateHandle_t StartItemUpdate(AppId_t appID, PublishedFileId_t publishedFileID) {
        var handle = SteamUGC.StartItemUpdate(appID, publishedFileID);
        return handle;
    }

    public static async Task<(SubmitItemUpdateResult_t result, bool failure)> SubmitItemUpdateAsync(UGCUpdateHandle_t handle, string changeNote) {

        var isDone = false;
        var res = new SubmitItemUpdateResult_t();
        bool f = true;
        var cr = CallResult<SubmitItemUpdateResult_t>.Create(
            (result, bIOFailure) =>
            {
                res = result;
                f = bIOFailure;
                isDone = true;
            });
        var call = SteamUGC.SubmitItemUpdate(handle, changeNote);
        cr.Set(call);

        do {
            await Task.Delay(100);
        } while (!isDone);

        return (res, f);
    }

}