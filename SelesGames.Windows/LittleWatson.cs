using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;

namespace SelesGames.Phone
{
    public class LittleWatson
    {
        const string filename = "LittleWatson.txt";

        public static void LogException(Exception ex, string extra)
        {
            Exception e = ex;
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    TryDeleteFile(store);

                    using (var writeStream = store.CreateFile(filename))
                    using (var writer = new StreamWriter(writeStream))
                    {
                        writer.WriteLine(extra);

                        while (e != null)
                        {
                            writer.WriteLine(string.Format("Exception of type: {0}", ex.GetType().Name));
                            writer.WriteLine(string.Format("\r\nException Message\r\n:{0}", ex.Message));
                            writer.WriteLine(string.Format("\r\nStackTrace:\r\n{0}", ex.StackTrace));
                            writer.WriteLine();

                            e = ex.InnerException;
                        }
                    }
                }
            }
            catch { }
        }

        public static void LogPreviousExceptionIfPresent(Action<string> reportFunc)
        {
            string contents = null;

            try
            {

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(filename))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            contents = reader.ReadToEnd();
                        }
                        TryDeleteFile(store);
                    }
                }
                if (contents != null)
                {
                    if (MessageBox.Show("A problem occurred the last time you ran this application. Would you like to send an email to report it?", "Problem Report", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        TryDeleteFile(IsolatedStorageFile.GetUserStoreForApplication()); // line added 1/15/2011
                        reportFunc(contents);
                    }
                }
            }
            catch {}
        }

        static void TryDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(filename);
            }
            catch { }
        }
    }
}