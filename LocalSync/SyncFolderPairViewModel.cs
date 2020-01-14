using System.Diagnostics;
using System.IO.Abstractions;
using System.Windows.Input;
using ReactiveUI;

namespace LocalSync
{
    public class SyncFolderPairViewModel : ReactiveObject
    {
        public string Name => $"`{Left.FullName}` vs `{Right.FullName}`";

        public IDirectoryInfo Left {get; set; }
        public IDirectoryInfo Right {get; set; }

        public ICommand Compare => ReactiveCommand.Create(() => 
        { 
            Process.Start(@"C:\Program Files\Beyond Compare 4\BCompare.exe", $"\"{Left.FullName}\" \"{Right.FullName}\""); 
        });
    }
}