using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CJR.Common.IO
{
    public interface IFileUtilities
    {
        IList<FileEntity> GetListFromFolder(string originPath, string s, DateTime addYears);
        void MoveFileWithOverwrite(string fullPath, string concatenateFilePaths);
        string EnsurePath(string concatenateFilePaths);
        void CopyAllFiles(string sourceFolder, string destinationFolder);
    }
    public class FileUtilities : IFileUtilities
    {
        public IList<FileEntity> GetListFromFolder(string originPath, string s, DateTime addYears)
        {
            throw new NotImplementedException();
        }

        public void MoveFileWithOverwrite(string fullPath, string concatenateFilePaths)
        {
            throw new NotImplementedException();
        }

        public string EnsurePath(string concatenateFilePaths)
        {
            throw new NotImplementedException();
        }

        public void CopyAllFiles(string sourceFolder, string destinationFolder)
        {
            throw new NotImplementedException();
        }
    }
}
