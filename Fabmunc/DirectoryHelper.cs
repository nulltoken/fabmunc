using System;
using System.Diagnostics;
using System.IO;

namespace Fabmunc
{
    // From https://github.com/nulltoken/libgit2sharp/blob/vNext/LibGit2Sharp.Tests/TestHelpers/DirectoryHelper.cs

    public static class DirectoryHelper
    {
        public static void DeleteSubdirectories(string parentPath)
        {
            string[] dirs = Directory.GetDirectories(parentPath);
            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }
        }

        public static void DeleteDirectory(string directoryPath)
        {
            // From http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true/329502#329502

            if (!Directory.Exists(directoryPath))
            {
                Trace.WriteLine(
                    string.Format("Directory '{0}' is missing and can't be removed.",
                        directoryPath));

                return;
            }

            string[] files = Directory.GetFiles(directoryPath);
            string[] dirs = Directory.GetDirectories(directoryPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);
            try
            {
                Directory.Delete(directoryPath, false);
            }
            catch (IOException)
            {
                Trace.WriteLine(string.Format("{0}The directory '{1}' could not be deleted!" +
                                              "{0}Most of the time, this is due to an external process accessing the files in the temporary repositories created during the test runs, and keeping a handle on the directory, thus preventing the deletion of those files." +
                                              "{0}Known and common causes include:" +
                                              "{0}- Windows Search Indexer" +
                                              "{0}- Antivirus{0}",
                    Environment.NewLine, Path.GetFullPath(directoryPath)));
            }
        }
    }
}
