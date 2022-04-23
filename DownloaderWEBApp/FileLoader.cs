using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DownloaderWEBApp
{
    public class FileLoader
    {

        public string StatusPause { get; private set; } = "Пауза";
        public string StatusCanceled { get; private set; } = "Отмена";
        public string StatusLoading { get; private set; } = "Загрузка...";
        public string StatusComplete { get; private set; } = "Файл загружен";

        //Events

        public delegate Task PercentageChanged(object sender, DownloadProgressArgs e);
        public event PercentageChanged PercentageChangedEvent;

        public delegate Task CompetedDownload(object sender, DownloadProgressArgs e);
        public event CompetedDownload CompletedDownloadEvent;

        public FileLoader()
        {
        }

        private async Task<long> GetFileSizeAsync(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            if (request is not null)
            {
                using (var response = await request.GetResponseAsync())
                {
                    return long.Parse(response.Headers.Get("Content-Length"));
                }
            }
            return 0L;
        }

        public async Task LoadAsync(string url, string filePath, ManualResetEventSlim limiter, CancellationToken token)
        {
            var request = WebRequest.CreateHttp(url);
            if (request is not null)
            {
                long bytesLoaded = 0L;
                long fileSize = await GetFileSizeAsync(url);

                using (var response = request.GetResponse())
                using (var remoteStream = response.GetResponseStream())
                using (var file = File.OpenWrite(filePath))
                {
                    byte[] buffer = new byte[1024];
                    short bytesRead = default;
                    double percentage = .0;

                    do
                    {
                        //пауза
                        limiter.Wait();

                        //остановка
                        if (token.IsCancellationRequested)
                        {
                            FileHandler.Files.Add(new FileModel(filePath, StatusCanceled));
                            token.ThrowIfCancellationRequested();
                        }

                        bytesRead = (short)remoteStream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                            break;

                        bytesLoaded += bytesRead;

                      // var percentageOld = percentage;
                        percentage = Math.Round((bytesLoaded / (double)fileSize)*100.0, 0);

                        file.Write(buffer, 0, bytesRead);

                        /*
                        if ((int)percentage % 5 == 0 && percentage - percentageOld > 1.0)
                        {
                        */
                        await Task.Run(() =>
                        {
                            PercentageChangedEvent?.Invoke(
                                                            this,
                                                            new DownloadProgressArgs(bytesLoaded, (int)percentage));
                        });
                            

                        //}
                    } while (true);

                    FileHandler.Files.Add(new FileModel(filePath, StatusComplete));
                    CompletedDownloadEvent?.Invoke(
                        this,
                        new DownloadProgressArgs(bytesLoaded, 100));
                }
            }
        }
    }
}
