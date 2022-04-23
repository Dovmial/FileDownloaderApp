using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace DownloaderWEBApp
{
    public partial class Form1 : Form
    {
        private FileLoader _fileLoader;
        private FileHandler _fileHandler;

        private ManualResetEventSlim _limiter = new ManualResetEventSlim(true);
        private CancellationTokenSource _cancelToken;
        public Form1()
        {
            InitializeComponent();
            btnPauseResume.Enabled = false;
            btnCancelation.Enabled = false;

            _fileLoader = new FileLoader();
            _fileHandler = new FileHandler();

            _fileLoader.PercentageChangedEvent += PercentageLoadShowAsync;
            _fileLoader.CompletedDownloadEvent += CompleteDownloadStatusAsync;
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            ResetStatusFileLoad();
            btnPauseResume.Enabled = true;
            btnCancelation.Enabled = true;
            btnDownload.Enabled = false;

            _cancelToken = new CancellationTokenSource();

            if (tbAddress.Text == string.Empty)
            {
                MessageBox.Show("не указан адрес для скачивания");
                ResetStatusFileLoad();
                return;
            }

            string url = tbAddress.Text;
            string filePath;

            saveFileDialog1.Filter = _fileHandler.GetExtensionsFilter(Path.GetExtension(url));
            saveFileDialog1.FileName = Path.GetFileName(url);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog1.FileName;
                
                //проверка существования файла
                int index = await FileHandler.FilesExistsRemoveAsync(filePath);
                if(index != -1)
                    dataGridView1.Rows.RemoveAt(index);

                //Загрузка
                lblStatus.Text = _fileLoader.StatusLoading;
                try
                {
                    await Task.Run(async() => await _fileLoader.LoadAsync(url, filePath, _limiter, _cancelToken.Token));
                }
                catch (OperationCanceledException)
                {
                    lblDownloaded.Text = _fileLoader.StatusCanceled;
                    ResetStatusFileLoad();
                }
                catch(Exception ex)
                {
                    ResetStatusFileLoad();
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    _cancelToken.Dispose();

                    var file = FileHandler.Files.Last();
                    if(file is not null)
                        dataGridView1.Rows.Add(
                            file.Name,
                            string.Join(',', file.Tags),
                            file.Status,
                            "Открыть папку");

                    btnPauseResume.Enabled = false;
                    btnCancelation.Enabled = false;
                    btnDownload.Enabled = true;
                }
            }
        }

        private void btnPauseResume_Click(object sender, EventArgs e)
        {
            if (_limiter.IsSet)
            {
                btnPauseResume.Text = "Продолжить";
                lblStatus.Text = _fileLoader.StatusPause;
                _limiter.Reset(); // пауза
            }
            else
            {
                btnPauseResume.Text = "Пауза";
                lblStatus.Text = _fileLoader.StatusLoading;
                _limiter.Set(); // продолжение
            }
        }

        private async Task PercentageLoadShowAsync(object sender, DownloadProgressArgs e)
        {
            await Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    progressBar1.Value = e.Percentage;
                    lblProgressPercentage.Text = $"{e.Percentage}%";
                    lblDownloaded.Text = $"{e.Mbytes:0.000} MB";
                }));
            });
            
        }

        private async Task CompleteDownloadStatusAsync(object sender, DownloadProgressArgs e)
        {
            await Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    progressBar1.Value = e.Percentage;
                    lblProgressPercentage.Text = $"{e.Percentage}%";
                    lblStatus.Text = _fileLoader.StatusComplete;
                    lblDownloaded.Text = $"{e.Mbytes:0.000} MB";
                }));
            });
            
        }

        private void btnCancelation_Click(object sender, EventArgs e)
        {
            _cancelToken.Cancel();
        }

        private void ResetStatusFileLoad()
        {
            Invoke(new Action(() =>
            {
                lblStatus.Text = "???";
                progressBar1.Value = 0;
                lblProgressPercentage.Text = $"{0}%";
                lblDownloaded.Text = "0 MB";

                btnPauseResume.Enabled = false;
                btnCancelation.Enabled = false;
                btnDownload.Enabled = true;
            }));
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = e.RowIndex;
            if (e.ColumnIndex == 3 && FileHandler.Files.Count > i)
            {
                Process.Start("Explorer.exe", Path.GetDirectoryName(FileHandler.Files[i].FullPath));
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ResetStatusFileLoad();
        }
    }
}
