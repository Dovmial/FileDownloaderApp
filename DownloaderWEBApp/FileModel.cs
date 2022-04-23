using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloaderWEBApp
{
    public class FileModel
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string Status { get; private set; }
        public List<string> Tags { get; private set; } = new List<string>();
        public FileModel(string filepath, string status)
        {
            Name = Path.GetFileName(filepath);
            FullPath = filepath;
            Status = status;
        }
        public void AddTag(string tag)
        {
            if(!Tags.Contains(tag))
                Tags.Add(tag);
        }
        public void AddRangeTags(IEnumerable<string> tags)
        {
            foreach(string tag in tags)
                AddTag(tag);
        }
    }
}
