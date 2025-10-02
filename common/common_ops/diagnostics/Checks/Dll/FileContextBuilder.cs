using System;
using System.Diagnostics;
using System.IO;

namespace common_ops.diagnostics.Checks.Dll
{
    public class FileContextBuilder
    {
        public SFileContext GetFileData(string fileFullName)
        {
            try
            {
                return new SFileContext(
                    fileFullName,
                    GetFileData_Version(fileFullName),
                    GetFileData_CreationTime(fileFullName),
                    GetFileData_LastModifiedTime(fileFullName)
                );
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Version GetFileData_Version(string file)
        {
            if (File.Exists(file))
            {
                FileVersionInfo birokratVersionInfo = FileVersionInfo.GetVersionInfo(file);
                return new Version(birokratVersionInfo.ProductVersion);
            }
            throw new FileNotFoundException(file);
        }

        public DateTime GetFileData_CreationTime(string file)
        {
            if (File.Exists(file))
                return File.GetCreationTime(file);

            throw new FileNotFoundException(file);
        }

        public DateTime GetFileData_LastModifiedTime(string file)
        {
            if (File.Exists(file))
                return File.GetLastWriteTime(file);

            throw new FileNotFoundException(file);
        }
    }

    public struct SFileContext
    {
        internal DateTime CreationTime;
        internal DateTime ModifiedTime;
        internal string Name;
        internal string FullName;
        internal Version Version;

        public SFileContext(string fullName, Version version, DateTime creationTime, DateTime modifiedTime)
        {
            Name = Path.GetFileName(fullName);
            FullName = fullName;
            Version = version;
            CreationTime = creationTime;
            ModifiedTime = modifiedTime;
        }
    }
}
