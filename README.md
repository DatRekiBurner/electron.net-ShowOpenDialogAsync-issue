# Electron.NET bug
The whole point to this repo is to highlight a issue I found when using [Electron.NET](https://github.com/ElectronNET/Electron.NET).
There seems to be an issue when using
```cs
Electron.IpcMain.On("select-directory", async (args) =>
{
    BrowserWindow mainWindow = Electron.WindowManager.BrowserWindows.First();
    OpenDialogOptions options = new()
    {
        Properties = new OpenDialogProperty[]
        {
            OpenDialogProperty.openDirectory
        }
    };

    string[] paths = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
    Electron.IpcMain.Send(mainWindow, "select-directory-reply", paths);
});
```
Where every time you revisit a page that uses that Electron function it spawns one additional `open dialog`.
So if you vist one page that uses it three times in a row it will open three `open dialog`s.
