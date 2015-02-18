using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VitML.ImageRenderer.Extensions
{
    public static class IOHelper
    {

        public static bool IsFileLocked(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            if (!fi.Exists) return false;
            return IsFileLocked(fi);
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            //file is not locked
            return false;
        }
    }
}
