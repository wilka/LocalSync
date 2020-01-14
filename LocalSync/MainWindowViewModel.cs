using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using ReactiveUI;

namespace LocalSync
{
    public class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel(IDirectoryInfoFactory directoryInfoFactory, IFileInfoFactory fileInfoFactory)
        {
            SyncFolders = new ObservableCollection<SyncFolderPairViewModel>();

            var foldersFile = fileInfoFactory.FromFileName(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "LocalSyncFolders.txt"));

            using var reader = foldersFile.OpenText();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split('|');
                if (parts.Length != 2)
                {
                    throw new Exception("Each line in the settings file must be a pair of folders separated by `|`");
                }

                SyncFolders.Add(new SyncFolderPairViewModel
                {
                    Left = directoryInfoFactory.FromDirectoryName(parts[0].Trim()),
                    Right = directoryInfoFactory.FromDirectoryName(parts[1].Trim()),
                });
            }
        }

        public ObservableCollection<SyncFolderPairViewModel> SyncFolders { get; }
    }
}