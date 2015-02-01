using System;
using System.IO;
using AFPST.Common.Extensions;
using AFPST.Common.Services;
using AFPST.Common.Services.imp;

namespace FTPWrapper
{
    public interface IFTPFileArchiver
    {
        void MoveWithDatePatternSuffix(string source, string destination, 
                                                       string fname, string pattern, bool overwrite);

        void MoveWithNumericSuffix(string source, string destination, string fname);
        void MoveAllWithDatePatternSuffix(string source, string destination, string pattern);
        void MoveWithOverwrite(string origin, string destination);
        void MoveAllWithOverwrite(string origin, string destination);
    }

    public class FTPFileArchiver : IFTPFileArchiver
    {
        private readonly IFileUtilities _utilities;

        public FTPFileArchiver(IFileUtilities utilities)
        {
            _utilities = utilities;
        }

        public void MoveWithDatePatternSuffix(string source, string destination, 
                                              string fname, string pattern, bool overwrite)
        {

            var full_source_path = source + fname;
            var full_dest_path = destination + fname;
            var file_no_extension = Path.GetFileNameWithoutExtension(fname);
            var file_with_suffix = file_no_extension + DateTime.Now.ToString(pattern);
            full_dest_path = full_dest_path.Replace(file_no_extension, file_with_suffix);
            File.Move(full_source_path, full_dest_path);
        }

        public void MoveWithNumericSuffix(string source, string destination, string fname)
        {
            var full_source_path = source + fname;
            var full_dest_path = destination + fname;
            while (File.Exists(full_dest_path))
                full_dest_path = increment_filename(full_dest_path, ref fname);
            File.Move(full_source_path, full_dest_path);
        }

        public  void MoveAllWithDatePatternSuffix(string source, string destination, string pattern)
        {
            var lst = _utilities.GetListFromFolder(source, "*", DateTime.Today.AddYears(-2));
            lst.ForEach(f =>  MoveWithDatePatternSuffix(source, destination, f.FileName, pattern, true));
        }

        public void MoveWithOverwrite(string origin, string destination)
        {
            if(File.Exists(destination))
                File.Delete(destination);
            File.Move(origin, destination);
        }

        public void MoveAllWithOverwrite(string source, string destination)
        {
            var lst = _utilities.GetListFromFolder(source, "*", DateTime.Today.AddYears(-2));
            lst.ForEach(f => MoveWithOverwrite(f.FullPath, destination + f.FileName));
        }

        private static string increment_filename(string full_dest_path, ref string original_file_name)
        {
            string original = Path.GetFileNameWithoutExtension(original_file_name);
            string current = Path.GetFileNameWithoutExtension(full_dest_path);
            if (original == current)
                return full_dest_path.Replace(original, original + "01");
            var numStr = current.Replace(original, "");
            var num = numStr.CastToInt() + 1;
            var incremented_filename = original + num.ToString("0#");
            original_file_name = incremented_filename + Path.GetExtension(original_file_name);
            return full_dest_path.Replace(current, incremented_filename);
        }
    }
}