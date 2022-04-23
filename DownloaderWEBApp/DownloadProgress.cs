using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloaderWEBApp
{
    public class DownloadProgressArgs
    {
        public double Mbytes { get; private set; }
        public int Percentage { get; private set; }
        public DownloadProgressArgs(long bytes, int percentage)
        {
            Mbytes = bytes / 1024.0 / 1024.0;
            Percentage = percentage;
        }
    }
}
