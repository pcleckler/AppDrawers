using System;
using System.IO;
using System.Text;

namespace AppDrawers
{
    public class Logger : IDisposable
    {
        private FileStream logStream = null;
        private StreamWriter writer = null;

        public Logger(string filename)
        {
            this.logStream = new FileStream(filename, FileMode.Append);

            this.writer = new StreamWriter(logStream, Encoding.UTF8);
        }

        public void Dispose()
        {
            try { this.logStream.Close(); } finally { }
        }

        public void LogMessage(string message)
        {
            lock (this.writer)
            {
                this.writer.Write($"{DateTime.Now} - {message}\r\n");
                this.writer.Flush();
            }
        }
    }
}