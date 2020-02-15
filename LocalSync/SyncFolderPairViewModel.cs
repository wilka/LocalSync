using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using System.Reactive.Linq;

namespace LocalSync
{
    public class SyncFolderPairViewModel : ReactiveObject
    {
        public string Name => $"`{Left.FullName}` vs `{Right.FullName}`";

        public IDirectoryInfo Left {get; }
        public IDirectoryInfo Right {get; }

        public SyncFolderPairViewModel(IDirectoryInfo left, IDirectoryInfo right)
        {
            Left = left;
            Right = right;

            DoFoldersMatchCommand = ReactiveCommand.CreateFromTask<bool?>(CompareFolders);
            doFoldersMatch = DoFoldersMatchCommand.ToProperty(this, x => x.DoFoldersMatch, default(bool?));

            DoFoldersMatchCommand.Execute().Subscribe();
        }

        private async Task<bool?> CompareFolders()
        {
            var leftFiles = GetFiles(Left);
            var rightFiles = GetFiles(Right);

            return await CompareFiles(leftFiles, rightFiles);
        }

        private async Task<bool> CompareFiles(FileWithRelativePath[] leftFiles, FileWithRelativePath[] rightFiles)
        {
            if(leftFiles.Length != rightFiles.Length)
            {
                return false;
            }

            var streamCompare = new NeoSmart.StreamCompare.StreamCompare();

            for (int i = 0; i < leftFiles.Length; i++)
            {
                var left = leftFiles[i];
                var right = rightFiles[i];

                if(left.RelativePath != right.RelativePath)
                {
                    return false;
                }

                if(left.FileInfo.Length != right.FileInfo.Length)
                {
                    return false;
                }

                await using var leftStream = left.FileInfo.OpenRead();
                await using var rightStream = right.FileInfo.OpenRead();

                if (!await streamCompare.AreEqualAsync(leftStream, rightStream))
                {
                    return false;
                }
            }

            return true;
        }

        private static FileWithRelativePath[] GetFiles(IDirectoryInfo directory)
        {
            return directory.GetFiles("*.*", SearchOption.AllDirectories)
                .Select(x => new FileWithRelativePath(x, directory))
                .OrderBy(x => x.RelativePath)
                .ToArray();
        }

        private class FileWithRelativePath
        {
            public IFileInfo FileInfo { get; }
            public string RelativePath { get; }

            public FileWithRelativePath(IFileInfo fileInfo, IDirectoryInfo directory)
            {
                FileInfo = fileInfo;
                RelativePath = Path.GetRelativePath(directory.FullName, fileInfo.FullName).ToLowerInvariant();
            }
        }

        
        readonly ObservableAsPropertyHelper<bool?> doFoldersMatch;
        public bool? DoFoldersMatch => doFoldersMatch.Value;

        public ReactiveCommand<Unit, bool?> DoFoldersMatchCommand { get; }

        public ICommand Compare => ReactiveCommand.Create(() => 
        { 
            Process.Start(@"C:\Program Files\Beyond Compare 4\BCompare.exe", $"\"{Left.FullName}\" \"{Right.FullName}\""); 
        });
    }
}