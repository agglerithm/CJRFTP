namespace CJR.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CJR.Common.IO;

    public static class FileUtilitiesExtensions
    {
        public static void MoveAll(this IFileUtilities fu, string source_folder, string destination_folder)
        {
            fu.CopyAllFiles(source_folder, destination_folder);
            fu.DeleteAll(source_folder);
        }

        public static void DeleteAll(this IFileUtilities fu, string folder)
        {
            var lst = fu.GetListFromFolder(folder);
            foreach (FileEntity file in lst)
                File.Delete(file.FullPath);
        }

        public static FileStream GetStream(this IFileUtilities fu, string filePath)
        {
            return File.OpenRead(filePath);
        }

        public static IList<FileEntity> GetListFromFolder(this IFileUtilities fu, string folder)
        {
            return fu.GetListFromFolder(folder.ConcatenateFilePaths(""), "*.*", DateTime.Today.AddYears(-30));
        }

        public static void MoveWithDatePatternSuffix(this IFileUtilities fu, string source, string destination, 
                                              string fname, string pattern, bool overwrite)
        {
            var full_source_path = source.ConcatenateFilePaths(fname);
            var full_dest_path = destination.ConcatenateFilePaths(fname);
            if(fname.Length > 20)
            {
                fu.MoveWithOverwrite(full_source_path, full_dest_path);
                return;
            }
            var file_no_extension = Path.GetFileNameWithoutExtension(fname);
            var file_with_suffix = file_no_extension + DateTime.Now.ToString(pattern);
            full_dest_path = full_dest_path.Replace(file_no_extension, file_with_suffix);
            File.Move(full_source_path, full_dest_path);
        }

        public static void MoveWithNumericSuffix(this IFileUtilities fu, string source, string destination, string fname)
        {
            var full_source_path = source + fname;
            var full_dest_path = destination + fname;
            while (File.Exists(full_dest_path))
                full_dest_path = increment_filename(full_dest_path, ref fname);
            File.Move(full_source_path, full_dest_path);
        }

        public static  void MoveAllWithDatePatternSuffix(this IFileUtilities fu, string source, string destination, string pattern)
        {
            var lst = fu.GetListFromFolder(source, "*", DateTime.Today.AddYears(-2));
            lst.ForEach(f => fu.MoveWithDatePatternSuffix(source, destination, f.FileName, pattern, true));
        }

        public static void MoveWithOverwrite(this IFileUtilities fu, string origin, string destination)
        {
            if(File.Exists(destination))
                File.Delete(destination);
            File.Move(origin, destination);
        }

        public static void MoveAllWithOverwrite(this IFileUtilities fu, string source, string destination)
        {
            var lst = fu.GetListFromFolder(source, "*", DateTime.Today.AddYears(-2));
            lst.ForEach(f => fu.MoveWithOverwrite(f.FullPath, destination + f.FileName));
        }

        public static string EnsurePath(this IFileUtilities futil, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private static string increment_filename(string fullDestPath, ref string originalFileName)
        {
            string original = Path.GetFileNameWithoutExtension(originalFileName);
            string current = Path.GetFileNameWithoutExtension(fullDestPath);
            if (original == current)
                return fullDestPath.Replace(original, original + "01");
            var numStr = current.Replace(original, "");
            var num =  numStr.CastToInt() + 1;
            var incrementedFilename = original + num.ToString("0#");
            originalFileName = incrementedFilename + Path.GetExtension(originalFileName);
            return fullDestPath.Replace(current, incrementedFilename);
        }

        public static IList<FileEntity> GetListFromFolder(this IFileUtilities fu, string folder, string mask)
        {
            if(string.IsNullOrEmpty(mask))
                mask = "*.*";
            return fu.GetListFromFolder(folder.ConcatenateFilePaths(""), mask, DateTime.Today.AddYears(-30));
        }
    } 
}
