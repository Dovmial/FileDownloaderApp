using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloaderWEBApp
{
    class FileHandler
    {
        //todo заполнение списка
        public static List<FileModel> Files { get; private set; } = new List<FileModel>();
        public static async Task<int> FilesExistsRemoveAsync(string filePath)
        {
           return await Task.Run(() =>
            {
                var index = Files.FindIndex(p => p.FullPath == filePath);
                if(index != -1)
                    Files.RemoveAt(index);
                return index;
            });
        }
        public string GetExtensionsFilter(string extension)
        {
            switch (extension)
            {
                case ".txt": return FilterTXT;
                case ".mp4": return FilterMP4;
                case ".mp3": return FilterMP3;
                default: return FilterAny;
            }
        }
        
        private string FilterTXT = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
        private string FilterMP4 = "Видео файлы (*.mp4)|*.mp4|Все файлы (*.*)|*.*";
        private string FilterMP3 = "Аудио файлы (*.mp3)|*.mp3|Все файлы (*.*)|*.*";
        private string FilterAny = "Все файлы (*.*)|*.*";

    }
}
