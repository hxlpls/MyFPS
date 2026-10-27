using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;

namespace MyFPS
{
    public partial class Form1 : Form
    {
        // =========================
        // FPS SİSTEMİ
        // =========================

        private int frameCount = 0;
        private int fps = 0;

        private int averageFps = 0;
        private int minimumFps = 0;
        private int maximumFps = 0;
        private int onePercentLow = 0;

        private double frameTime = 0;

        private Stopwatch stopwatch = new Stopwatch();
        private Stopwatch runtimeStopwatch = new Stopwatch();

        private System.Windows.Forms.Timer timer =
            new System.Windows.Forms.Timer();

        private List<int> fpsHistory =
            new List<int>();


        // =========================
        // SİSTEM MONİTÖRÜ
        // =========================

        private PerformanceCounter cpuCounter =
            new PerformanceCounter(
                "Processor",
                "% Processor Time",
                "_Total"
            );

        private System.Windows.Forms.Timer systemTimer =
            new System.Windows.Forms.Timer();

        private float cpuUsage = 0;
        private float ramUsage = 0;
        private float ramGbUsage = 0;
        private float ramGbTotal = 0;
        private float gpuUsage = 0;
        private float gpuTemperature = 0;
        private float gpuMemoryUsed = 0;
        private float gpuMemoryTotal = 0;
        private bool gpuAvailable = false;
        // Ek özellikler: GPU saat/fan/güç ve oturum içi en yüksek değerler.
        private float gpuClockMhz = 0;
        private float gpuFanPercent = -1;
        private float gpuPowerWatts = -1;
        private float peakCpuUsage = 0;
        private float peakRamUsage = 0;
        private float peakGpuTemperature = 0;
        private bool graphPaused = false;
        private List<int> frozenFpsHistory = new List<int>();

        // =========================
        // LABEL'LER
        // =========================

        private Label fpsLabel = new Label();
        private Label averageLabel = new Label();
        private Label minimumLabel = new Label();
        private Label maximumLabel = new Label();
        private Label frameTimeLabel = new Label();
        private Label onePercentLabel = new Label();
        private Label runtimeLabel = new Label();

        private Label systemTitleLabel = new Label();
        private Label cpuLabel = new Label();
        private Label ramLabel = new Label();
        private Label ramGbLabel = new Label();
        private Label gpuLabel = new Label();
        private Label targetStatusLabel = new Label();
        private Label targetFpsLabel = new Label();
        private ComboBox targetFpsComboBox = new ComboBox();

        private Button resetButton = new Button();
        private Button exportButton = new Button();
        private Button pauseButton = new Button();
        private Button themeButton = new Button();
        private Button clearLogButton = new Button();
        private Button summaryButton = new Button();
        private Button graphPauseButton = new Button();

        private bool darkTheme = false;
        private int targetFps = 144;
        private bool systemMonitoringPaused = false;
        private List<string> systemLog = new List<string>();


        // =========================
        // FORM
        // =========================

        public Form1()
        {
            InitializeComponent();

            // Grafik titremesini azaltmak için çift tamponlama.
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            this.UpdateStyles();

            this.Text = "MyFPS - Sistem ve FPS İzleyici";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(800, 640);
            this.MinimumSize = new Size(816, 679);

            // =========================
            // FPS LABEL
            // =========================

            fpsLabel.Text = "FPS: 0";
            fpsLabel.Font = new Font("Arial", 16);
            fpsLabel.AutoSize = true;
            fpsLabel.Location = new Point(40, 25);
            this.Controls.Add(fpsLabel);


            // =========================
            // AVG
            // =========================

            averageLabel.Text = "AVG: 0";
            averageLabel.Font = new Font("Arial", 16);
            averageLabel.AutoSize = true;
            averageLabel.Location = new Point(230, 25);
            this.Controls.Add(averageLabel);


            // =========================
            // MIN
            // =========================

            minimumLabel.Text = "MIN: 0";
            minimumLabel.Font = new Font("Arial", 16);
            minimumLabel.AutoSize = true;
            minimumLabel.Location = new Point(420, 25);
            this.Controls.Add(minimumLabel);


            // =========================
            // MAX
            // =========================

            maximumLabel.Text = "MAX: 0";
            maximumLabel.Font = new Font("Arial", 16);
            maximumLabel.AutoSize = true;
            maximumLabel.Location = new Point(40, 65);
            this.Controls.Add(maximumLabel);


            // =========================
            // FRAME TIME
            // =========================

            frameTimeLabel.Text = "FRAME: 0 ms";
            frameTimeLabel.Font = new Font("Arial", 16);
            frameTimeLabel.AutoSize = true;
            frameTimeLabel.Location = new Point(230, 65);
            this.Controls.Add(frameTimeLabel);


            // =========================
            // 1% LOW
            // =========================

            onePercentLabel.Text = "1% LOW: 0";
            onePercentLabel.Font = new Font("Arial", 16);
            onePercentLabel.AutoSize = true;
            onePercentLabel.Location = new Point(420, 65);
            this.Controls.Add(onePercentLabel);


            // =========================
            // RUNTIME
            // =========================

            runtimeLabel.Text = "RUNTIME: 00:00:00";
            runtimeLabel.Font = new Font("Arial", 16);
            runtimeLabel.AutoSize = true;
            runtimeLabel.Location = new Point(40, 110);
            this.Controls.Add(runtimeLabel);


            // =========================
            // SYSTEM BAŞLIK
            // =========================

            systemTitleLabel.Text = "SYSTEM";
            systemTitleLabel.Font = new Font("Arial", 16);
            systemTitleLabel.AutoSize = true;
            systemTitleLabel.Location = new Point(40, 150);
            this.Controls.Add(systemTitleLabel);


            // =========================
            // CPU
            // =========================

            cpuLabel.Text = "CPU USAGE: 0%";
            cpuLabel.Font = new Font("Arial", 14);
            cpuLabel.AutoSize = true;
            cpuLabel.Location = new Point(40, 180);
            this.Controls.Add(cpuLabel);


            // =========================
            // RAM
            // =========================

            ramLabel.Text = "RAM USAGE: 0%";
            ramLabel.Font = new Font("Arial", 14);
            ramLabel.AutoSize = true;
            ramLabel.Location = new Point(250, 180);
            this.Controls.Add(ramLabel);
ramGbLabel.Text = "RAM: 0.0 / 0.0 GB";
            ramGbLabel.Font = new Font("Arial", 14);
            ramGbLabel.AutoSize = true;
            ramGbLabel.Location = new Point(475, 180);
            this.Controls.Add(ramGbLabel);

            // Yeni özellik: GPU bilgisi (nvidia-smi üzerinden okunur).
            gpuLabel.Text = "GPU: NVIDIA bilgisi bekleniyor...";
            gpuLabel.Font = new Font("Arial", 9);
            gpuLabel.AutoSize = true;
            gpuLabel.Location = new Point(40, 207);
            this.Controls.Add(gpuLabel);

            // Yeni özellik: Hedef FPS seçimi.
            targetFpsLabel.Text = "Hedef FPS:";
            targetFpsLabel.Font = new Font("Arial", 10);
            targetFpsLabel.AutoSize = true;
            targetFpsLabel.Location = new Point(430, 110);
            this.Controls.Add(targetFpsLabel);

            targetFpsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            targetFpsComboBox.Items.AddRange(new object[] { "60", "120", "144", "180", "240" });
            targetFpsComboBox.SelectedItem = "144";
            targetFpsComboBox.Location = new Point(515, 106);
            targetFpsComboBox.Size = new Size(75, 25);
            targetFpsComboBox.SelectedIndexChanged += TargetFpsComboBox_SelectedIndexChanged;
            this.Controls.Add(targetFpsComboBox);

            targetStatusLabel.Text = "HEDEF FPS: bekleniyor";
            targetStatusLabel.Font = new Font("Arial", 9, FontStyle.Bold);
            targetStatusLabel.AutoSize = true;
            targetStatusLabel.Location = new Point(40, 232);
            this.Controls.Add(targetStatusLabel);



            // =========================
            // YENİ ÖZELLİKLER: BUTONLAR
            // =========================

            resetButton.Text = "İstatistikleri Sıfırla";
            resetButton.Size = new Size(145, 32);
            resetButton.Location = new Point(40, 525);
            resetButton.Click += ResetButton_Click;
            this.Controls.Add(resetButton);

            exportButton.Text = "CSV Dışa Aktar";
            exportButton.Size = new Size(130, 32);
            exportButton.Location = new Point(195, 525);
            exportButton.Click += ExportButton_Click;
            this.Controls.Add(exportButton);

            pauseButton.Text = "CPU/RAM Durdur";
            pauseButton.Size = new Size(145, 32);
            pauseButton.Location = new Point(335, 525);
            pauseButton.Click += PauseButton_Click;
            this.Controls.Add(pauseButton);

            // Yeni özellik: Açık/koyu tema.
            themeButton.Text = "Koyu Temaya Geç";
            themeButton.Size = new Size(145, 32);
            themeButton.Location = new Point(40, 570);
            themeButton.Click += ThemeButton_Click;
            this.Controls.Add(themeButton);

            // Yeni özellik: CSV için biriktirilen ölçüm geçmişini temizle.
            clearLogButton.Text = "Logu Temizle";
            clearLogButton.Size = new Size(130, 32);
            clearLogButton.Location = new Point(195, 570);
            clearLogButton.Click += ClearLogButton_Click;
            this.Controls.Add(clearLogButton);

            // Yeni özellik: Oturum özeti.
            summaryButton.Text = "Oturum Özeti";
            summaryButton.Size = new Size(145, 32);
            summaryButton.Location = new Point(335, 570);
            summaryButton.Click += SummaryButton_Click;
            this.Controls.Add(summaryButton);

            // Ek özellik 5: grafiği dondur/devam ettir (FPS ölçümü arka planda sürer).
            graphPauseButton.Text = "Grafiği Durdur";
            graphPauseButton.Size = new Size(140, 32);
            graphPauseButton.Location = new Point(500, 570);
            graphPauseButton.Click += GraphPauseButton_Click;
            this.Controls.Add(graphPauseButton);

            // =========================
            // FPS GRAPH
            // =========================

            this.Paint += Form1_Paint;


            // =========================
            // FPS TIMER
            // =========================

            stopwatch.Start();
            runtimeStopwatch.Start();

            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();


            // =========================
            // SYSTEM TIMER
            // =========================

            systemTimer.Interval = 1000;
            systemTimer.Tick += SystemTimer_Tick;
            systemTimer.Start();
        }


        // =========================
        // FPS TIMER
        // =========================

        private void Timer_Tick(object? sender, EventArgs e)
        {
            frameCount++;


            // RUNTIME

            TimeSpan runtime = runtimeStopwatch.Elapsed;

            int hours = (int)runtime.TotalHours;

            string runtimeText =
                $"RUNTIME: {hours:00}:{runtime.Minutes:00}:{runtime.Seconds:00}";

            // Aynı yazıyı her 16 ms'de tekrar atayıp gereksiz çizim oluşturma.
            if (runtimeLabel.Text != runtimeText)
                runtimeLabel.Text = runtimeText;


            // FPS

            if (stopwatch.ElapsedMilliseconds >= 1000)
            {
                fps = frameCount;


                if (fps > 0)
                {
                    frameTime = 1000.0 / fps;
                }


                fpsHistory.Add(fps);


                if (fpsHistory.Count > 100)
                {
                    fpsHistory.RemoveAt(0);
                }


                CalculateStatistics();


                fpsLabel.Text = $"FPS: {fps}";
                averageLabel.Text = $"AVG: {averageFps}";
                minimumLabel.Text = $"MIN: {minimumFps}";
                maximumLabel.Text = $"MAX: {maximumFps}";
                frameTimeLabel.Text = $"FRAME: {frameTime:F1} ms";
                onePercentLabel.Text = $"1% LOW: {onePercentLow}";


                frameCount = 0;

                stopwatch.Restart();
            }


            // Yalnızca grafik bölgesini yenile; bütün pencereyi sürekli çizme.
            this.Invalidate(new Rectangle(25, 225, 720, 290));
        }


        // =========================
        // CPU + RAM TIMER
        // =========================

        private void SystemTimer_Tick(object? sender, EventArgs e)
        {
            // CPU kullanımını oku. İlk NextValue() çağrısı bazen 0 dönebilir.
            cpuUsage = Math.Clamp(cpuCounter.NextValue(), 0f, 100f);

            // Fiziksel RAM miktarını Windows'tan oku.
            ComputerInfo computerInfo = new ComputerInfo();
            double totalBytes = computerInfo.TotalPhysicalMemory;
            double availableBytes = computerInfo.AvailablePhysicalMemory;

            if (totalBytes > 0)
            {
                ramGbTotal = (float)(totalBytes / 1073741824.0);
                ramGbUsage = (float)((totalBytes - availableBytes) / 1073741824.0);
                ramUsage = (float)Math.Clamp(
                    (totalBytes - availableBytes) / totalBytes * 100.0,
                    0.0,
                    100.0
                );
            }

            cpuLabel.Text = $"CPU USAGE: {cpuUsage:F0}%";
            ramLabel.Text = $"RAM USAGE: {ramUsage:F0}%";
            ramGbLabel.Text = $"RAM: {ramGbUsage:F1} / {ramGbTotal:F1} GB";

            // Ek özellik 4: oturum boyunca görülen en yüksek sistem değerlerini tut.
            peakCpuUsage = Math.Max(peakCpuUsage, cpuUsage);
            peakRamUsage = Math.Max(peakRamUsage, ramUsage);

            // Yeni özellik: NVIDIA GPU kullanımı, sıcaklığı ve VRAM bilgisi.
            UpdateGpuInformation();
            if (gpuAvailable)
                peakGpuTemperature = Math.Max(peakGpuTemperature, gpuTemperature);

            // Yeni özellik: FPS değerini seçilen hedefle karşılaştır.
            if (fps >= targetFps)
            {
                targetStatusLabel.Text = $"HEDEF FPS: ULAŞILDI ({fps}/{targetFps})";
                targetStatusLabel.ForeColor = Color.LimeGreen;
            }
            else
            {
                targetStatusLabel.Text = $"HEDEF FPS: ALTINDA ({fps}/{targetFps})";
                targetStatusLabel.ForeColor = Color.DarkOrange;
            }

            // Özellik 1: CPU ve RAM için kullanım seviyesine göre uyarı rengi.
            cpuLabel.ForeColor = GetUsageColor(cpuUsage);
            ramLabel.ForeColor = GetUsageColor(ramUsage);

            // Özellik 2: CSV raporu için ölçüm geçmişini kaydet.
            string logLine = string.Join(",",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                fps.ToString(CultureInfo.InvariantCulture),
                cpuUsage.ToString("F1", CultureInfo.InvariantCulture),
                ramUsage.ToString("F1", CultureInfo.InvariantCulture),
                ramGbUsage.ToString("F2", CultureInfo.InvariantCulture),
                ramGbTotal.ToString("F2", CultureInfo.InvariantCulture),
                gpuAvailable ? gpuUsage.ToString("F1", CultureInfo.InvariantCulture) : "N/A",
                gpuAvailable ? gpuTemperature.ToString("F1", CultureInfo.InvariantCulture) : "N/A",
                gpuAvailable ? gpuMemoryUsed.ToString("F1", CultureInfo.InvariantCulture) : "N/A",
                gpuAvailable ? gpuMemoryTotal.ToString("F1", CultureInfo.InvariantCulture) : "N/A",
                gpuAvailable ? gpuClockMhz.ToString("F0", CultureInfo.InvariantCulture) : "N/A",
                gpuFanPercent >= 0 ? gpuFanPercent.ToString("F0", CultureInfo.InvariantCulture) : "N/A",
                gpuPowerWatts >= 0 ? gpuPowerWatts.ToString("F1", CultureInfo.InvariantCulture) : "N/A");
            systemLog.Add(logLine);

            this.Invalidate(new Rectangle(25, 225, 720, 290));
        }

        private void TargetFpsComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (int.TryParse(targetFpsComboBox.SelectedItem?.ToString(), out int selectedFps))
            {
                targetFps = selectedFps;
                this.Invalidate();
            }
        }

        // NVIDIA sürücüsü kuruluysa nvidia-smi üzerinden GPU bilgilerini okur.
        private void UpdateGpuInformation()
        {
            try
            {
                using Process process = new Process();
                process.StartInfo.FileName = "nvidia-smi";
                process.StartInfo.Arguments = "--query-gpu=utilization.gpu,temperature.gpu,memory.used,memory.total,clocks.gr,fan.speed,power.draw --format=csv,noheader,nounits";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(1500);

                string[] values = output.Split(',');
                if (values.Length >= 4 &&
                    float.TryParse(values[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float usage) &&
                    float.TryParse(values[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float temperature) &&
                    float.TryParse(values[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float memoryUsed) &&
                    float.TryParse(values[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float memoryTotal))
                {
                    gpuUsage = usage;
                    gpuTemperature = temperature;
                    gpuMemoryUsed = memoryUsed;
                    gpuMemoryTotal = memoryTotal;
                    // clocks/fan/power bazı dizüstü GPU'larda N/A dönebilir; ana veriyi bozmasın.
                    gpuClockMhz = ReadOptionalGpuValue(values, 4, 0);
                    gpuFanPercent = ReadOptionalGpuValue(values, 5, -1);
                    gpuPowerWatts = ReadOptionalGpuValue(values, 6, -1);
                    gpuAvailable = true;
                    string fanText = gpuFanPercent >= 0 ? $"Fan {gpuFanPercent:F0}%" : "Fan N/A";
                    string powerText = gpuPowerWatts >= 0 ? $"Güç {gpuPowerWatts:F1} W" : "Güç N/A";
                    string clockText = gpuClockMhz > 0 ? $"Saat {gpuClockMhz:F0} MHz" : "Saat N/A";
                    gpuLabel.Text = $"GPU: {gpuUsage:F0}% | {gpuTemperature:F0}°C | VRAM {gpuMemoryUsed:F0}/{gpuMemoryTotal:F0} MiB | {clockText} | {fanText} | {powerText}";
                    gpuLabel.ForeColor = GetUsageColor(gpuUsage);
                }
                else
                {
                    gpuAvailable = false;
                    gpuLabel.Text = "GPU: Bilgi okunamadı (nvidia-smi)";
                    gpuLabel.ForeColor = Color.DarkOrange;
                }
            }
            catch
            {
                gpuAvailable = false;
                gpuLabel.Text = "GPU: NVIDIA bilgisi alınamadı";
                gpuLabel.ForeColor = Color.DarkOrange;
            }
        }

        // GPU sürücüsü bir alan için N/A döndürürse diğer GPU verilerini göstermeye devam et.
        private float ReadOptionalGpuValue(string[] values, int index, float fallback)
        {
            if (index < values.Length &&
                float.TryParse(values[index].Trim(), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float result))
                return result;

            return fallback;
        }

        // Ek özellik 5: grafiği dondurur; FPS ve sistem ölçümleri çalışmayı sürdürür.
        private void GraphPauseButton_Click(object? sender, EventArgs e)
        {
            graphPaused = !graphPaused;
            if (graphPaused)
            {
                frozenFpsHistory = new List<int>(fpsHistory);
                graphPauseButton.Text = "Grafiğe Devam Et";
            }
            else
            {
                graphPauseButton.Text = "Grafiği Durdur";
            }

            this.Invalidate(new Rectangle(25, 225, 720, 290));
        }

        // Yeni özellik: açık/koyu tema.
        private void ThemeButton_Click(object? sender, EventArgs e)
        {
            darkTheme = !darkTheme;
            this.BackColor = darkTheme ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            themeButton.Text = darkTheme ? "Açık Temaya Geç" : "Koyu Temaya Geç";

            Color textColor = darkTheme ? Color.White : Color.Black;
            foreach (Control control in this.Controls)
            {
                if (control is Label)
                    control.ForeColor = textColor;
                else if (control is Button || control is ComboBox)
                {
                    control.BackColor = darkTheme ? Color.FromArgb(55, 55, 55) : SystemColors.Control;
                    control.ForeColor = textColor;
                }
            }

            // CPU/RAM/GPU uyarı renklerini tema değişiminden sonra koru.
            cpuLabel.ForeColor = GetUsageColor(cpuUsage);
            ramLabel.ForeColor = GetUsageColor(ramUsage);
            if (gpuAvailable) gpuLabel.ForeColor = GetUsageColor(gpuUsage);
            targetStatusLabel.ForeColor = fps >= targetFps ? Color.LimeGreen : Color.DarkOrange;
            this.Invalidate();
        }

        // Yeni özellik: yalnızca yeni CSV ölçümlerini temizler.
        private void ClearLogButton_Click(object? sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show(
                "Biriktirilen CSV ölçüm geçmişi silinsin mi?",
                "MyFPS - Logu Temizle",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer == DialogResult.Yes)
            {
                systemLog.Clear();
                MessageBox.Show("Ölçüm geçmişi temizlendi.", "MyFPS",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Yeni özellik: o ana kadarki FPS ve sistem durumunu özetler.
        private void SummaryButton_Click(object? sender, EventArgs e)
        {
            TimeSpan runtime = runtimeStopwatch.Elapsed;
            string gpuText = gpuAvailable
                ? $"{gpuUsage:F0}% kullanım, {gpuTemperature:F0}°C, VRAM {gpuMemoryUsed:F0}/{gpuMemoryTotal:F0} MiB"
                : "GPU bilgisi kullanılamıyor";

            string summary =
                $"Çalışma süresi: {(int)runtime.TotalHours:00}:{runtime.Minutes:00}:{runtime.Seconds:00}\n" +
                $"Anlık FPS: {fps}\nOrtalama FPS: {averageFps}\nMinimum FPS: {minimumFps}\n" +
                $"Maksimum FPS: {maximumFps}\n1% Low: {onePercentLow}\nFrame Time: {frameTime:F1} ms\n\n" +
                $"CPU: {cpuUsage:F0}% (oturum zirvesi {peakCpuUsage:F0}%)\n" +
                $"RAM: {ramGbUsage:F1}/{ramGbTotal:F1} GB ({ramUsage:F0}%, zirve {peakRamUsage:F0}%)\n" +
                $"GPU: {gpuText}\n" +
                $"GPU saat hızı: {(gpuClockMhz > 0 ? $"{gpuClockMhz:F0} MHz" : "N/A")}\n" +
                $"GPU fanı: {(gpuFanPercent >= 0 ? $"{gpuFanPercent:F0}%" : "N/A")} | Güç: {(gpuPowerWatts >= 0 ? $"{gpuPowerWatts:F1} W" : "N/A")}\n" +
                $"Oturumda en yüksek GPU sıcaklığı: {peakGpuTemperature:F0}°C\n" +
                $"Grafik: {(graphPaused ? "Donduruldu" : "Canlı")}\n" +
                $"CSV kayıt sayısı: {systemLog.Count}";

            MessageBox.Show(summary, "MyFPS - Oturum Özeti",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Color GetUsageColor(float usage)
        {
            if (usage >= 90f)
                return Color.Red;
            if (usage >= 75f)
                return Color.DarkOrange;
            return Color.LimeGreen;
        }

        // Özellik 3: FPS istatistiklerini sıfırla.
        private void ResetButton_Click(object? sender, EventArgs e)
        {
            fpsHistory.Clear();
            frameCount = 0;
            fps = 0;
            averageFps = 0;
            minimumFps = 0;
            maximumFps = 0;
            onePercentLow = 0;
            frameTime = 0;
            peakCpuUsage = cpuUsage;
            peakRamUsage = ramUsage;
            peakGpuTemperature = gpuAvailable ? gpuTemperature : 0;
            frozenFpsHistory.Clear();

            fpsLabel.Text = "FPS: 0";
            averageLabel.Text = "AVG: 0";
            minimumLabel.Text = "MIN: 0";
            maximumLabel.Text = "MAX: 0";
            frameTimeLabel.Text = "FRAME: 0 ms";
            onePercentLabel.Text = "1% LOW: 0";
            stopwatch.Restart();
            this.Invalidate();
        }

        // Özellik 4: Sistem ölçümlerini CSV dosyasına aktar.
        private void ExportButton_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "MyFPS sistem raporunu kaydet";
            dialog.Filter = "CSV dosyası (*.csv)|*.csv";
            dialog.FileName = "MyFPS_Rapor.csv";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            using StreamWriter writer = new StreamWriter(dialog.FileName, false);
            writer.WriteLine("Tarih,FPS,CPU_Yuzde,RAM_Yuzde,KullanilanRAM_GiB,ToplamRAM_GiB,GPU_Yuzde,GPU_Sicaklik_C,VRAM_Kullanilan_MiB,VRAM_Toplam_MiB,GPU_Saat_MHz,GPU_Fan_Yuzde,GPU_Guc_W");
            foreach (string line in systemLog)
                writer.WriteLine(line);

            MessageBox.Show("CSV raporu kaydedildi.", "MyFPS",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Özellik 5: CPU/RAM izlemeyi duraklat veya devam ettir.
        private void PauseButton_Click(object? sender, EventArgs e)
        {
            systemMonitoringPaused = !systemMonitoringPaused;

            if (systemMonitoringPaused)
            {
                systemTimer.Stop();
                pauseButton.Text = "CPU/RAM Devam Et";
            }
            else
            {
                systemTimer.Start();
                pauseButton.Text = "CPU/RAM Durdur";
            }
        }

        // =========================
        // FPS İSTATİSTİKLERİ
        // =========================

        private void CalculateStatistics()
        {
            if (fpsHistory.Count == 0)
            {
                return;
            }


            int total = 0;

            int min = fpsHistory[0];

            int max = fpsHistory[0];


            foreach (int value in fpsHistory)
            {
                total += value;


                if (value < min)
                {
                    min = value;
                }


                if (value > max)
                {
                    max = value;
                }
            }


            averageFps =
                total / fpsHistory.Count;

            minimumFps = min;

            maximumFps = max;


            CalculateOnePercentLow();
        }


        // =========================
        // 1% LOW
        // =========================

        private void CalculateOnePercentLow()
        {
            if (fpsHistory.Count < 10)
            {
                onePercentLow = minimumFps;

                return;
            }


            List<int> sorted =
                new List<int>(fpsHistory);


            sorted.Sort();


            int count =
                Math.Max(1, sorted.Count / 100);


            int total = 0;


            for (int i = 0; i < count; i++)
            {
                total += sorted[i];
            }


            onePercentLow =
                total / count;
        }


        // =========================
        // FPS GRAPH
        // =========================

        private void Form1_Paint(
            object? sender,
            PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;


            int graphX = 80;
            int graphY = 270;
            int graphWidth = 650;
            int graphHeight = 230;
            int plotHeight = graphHeight - 35; // Alt bölüm RAM çubuğu için ayrıldı.

            int maxFps = 240;


            // GRAPH SINIRI

            using (Pen borderPen =
                new Pen(Color.White, 2))
            {
                graphics.DrawRectangle(
                    borderPen,
                    graphX,
                    graphY,
                    graphWidth,
                    graphHeight
                );
            }


            // Yeni özellik: seçilen hedef FPS için grafikte kesikli yatay çizgi.
            int targetY = graphY + plotHeight - (int)((float)targetFps / maxFps * plotHeight);
            using (Pen targetPen = new Pen(Color.DodgerBlue, 2))
            {
                targetPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                graphics.DrawLine(targetPen, graphX, targetY, graphX + graphWidth, targetY);
                using Font targetFont = new Font("Arial", 8, FontStyle.Bold);
                using Brush targetBrush = new SolidBrush(Color.DodgerBlue);
                graphics.DrawString($"Hedef {targetFps} FPS", targetFont, targetBrush, graphX + graphWidth - 90, targetY - 14);
            }

            // CPU kullanım çubuğu: FPS grafiğinin hemen üstünde.
            int cpuBarX = 40;
            int cpuBarY = 250;
            int cpuBarWidth = 300;
            int cpuBarHeight = 12;
            int cpuFilledWidth = (int)(cpuBarWidth * Math.Clamp(cpuUsage, 0f, 100f) / 100f);

            using (Brush cpuBackground = new SolidBrush(Color.LightGray))
            using (Brush cpuFill = new SolidBrush(GetUsageColor(cpuUsage)))
            using (Pen cpuBorder = new Pen(Color.Gray))
            {
                graphics.FillRectangle(cpuBackground, cpuBarX, cpuBarY, cpuBarWidth, cpuBarHeight);
                if (cpuFilledWidth > 0)
                    graphics.FillRectangle(cpuFill, cpuBarX, cpuBarY, cpuFilledWidth, cpuBarHeight);
                graphics.DrawRectangle(cpuBorder, cpuBarX, cpuBarY, cpuBarWidth, cpuBarHeight);
            }

            // RAM kullanım çubuğu mevcut FPS grafik çerçevesinin İÇİNDE.
            int ramBarX = graphX + 15;
            int ramBarY = graphY + graphHeight - 24;
            int ramBarWidth = graphWidth - 30;
            int ramBarHeight = 12;
            int ramFilledWidth = (int)(ramBarWidth * Math.Clamp(ramUsage, 0f, 100f) / 100f);

            using (Brush ramBackground = new SolidBrush(Color.LightGray))
            using (Brush ramFill = new SolidBrush(GetUsageColor(ramUsage)))
            using (Pen ramBorder = new Pen(Color.Gray))
            using (Brush ramTextBrush = new SolidBrush(Color.Black))
            using (Font ramFont = new Font("Arial", 8, FontStyle.Bold))
            {
                graphics.FillRectangle(ramBackground, ramBarX, ramBarY, ramBarWidth, ramBarHeight);
                if (ramFilledWidth > 0)
                    graphics.FillRectangle(ramFill, ramBarX, ramBarY, ramFilledWidth, ramBarHeight);
                graphics.DrawRectangle(ramBorder, ramBarX, ramBarY, ramBarWidth, ramBarHeight);
                graphics.DrawString(
                    $"RAM {ramGbUsage:F1}/{ramGbTotal:F1} GB ({ramUsage:F0}%)",
                    ramFont, ramTextBrush, ramBarX, ramBarY - 14);
            }

            // FPS ÇİZGİLERİ

            using (Font font =
                new Font("Arial", 10))

            using (Brush brush =
                new SolidBrush(darkTheme ? Color.White : Color.Black))

            using (Pen gridPen =
                new Pen(Color.Gray, 1))
            {
                DrawFpsLine(
                    graphics,
                    gridPen,
                    brush,
                    font,
                    60,
                    graphX,
                    graphY,
                    graphWidth,
                    plotHeight,
                    maxFps
                );


                DrawFpsLine(
                    graphics,
                    gridPen,
                    brush,
                    font,
                    120,
                    graphX,
                    graphY,
                    graphWidth,
                    plotHeight,
                    maxFps
                );


                DrawFpsLine(
                    graphics,
                    gridPen,
                    brush,
                    font,
                    144,
                    graphX,
                    graphY,
                    graphWidth,
                    plotHeight,
                    maxFps
                );


                DrawFpsLine(
                    graphics,
                    gridPen,
                    brush,
                    font,
                    180,
                    graphX,
                    graphY,
                    graphWidth,
                    plotHeight,
                    maxFps
                );


                DrawFpsLine(
                    graphics,
                    gridPen,
                    brush,
                    font,
                    240,
                    graphX,
                    graphY,
                    graphWidth,
                    plotHeight,
                    maxFps
                );
            }


            // GRAPH ÇİZ: duraklatıldığında kaydedilmiş kopyayı kullan.
            List<int> historyToDraw = graphPaused ? frozenFpsHistory : fpsHistory;
            if (historyToDraw.Count < 2)
            {
                return;
            }

            using (Pen graphPen = new Pen(Color.Lime, 2))
            {
                for (int i = 1; i < historyToDraw.Count; i++)
                {
                    int previousFps = historyToDraw[i - 1];
                    int currentFps = historyToDraw[i];


                    if (previousFps > maxFps)
                    {
                        previousFps = maxFps;
                    }


                    if (currentFps > maxFps)
                    {
                        currentFps = maxFps;
                    }


                    float previousX =
                        graphX +
                        ((float)(i - 1) / 99) *
                        graphWidth;


                    float currentX =
                        graphX +
                        ((float)i / 99) *
                        graphWidth;


                    float previousY =
                        graphY +
                        plotHeight -
                        ((float)previousFps / maxFps) *
                        plotHeight;


                    float currentY =
                        graphY +
                        plotHeight -
                        ((float)currentFps / maxFps) *
                        plotHeight;


                    graphics.DrawLine(
                        graphPen,
                        previousX,
                        previousY,
                        currentX,
                        currentY
                    );
                }
            }
        }


        // =========================
        // GRAPH ÇİZGİSİ
        // =========================

        private void DrawFpsLine(
            Graphics graphics,
            Pen pen,
            Brush brush,
            Font font,
            int fpsValue,
            int graphX,
            int graphY,
            int graphWidth,
            int graphHeight,
            int maxFps)
        {
            float y =
                graphY +
                graphHeight -
                ((float)fpsValue / maxFps) *
                graphHeight;


            graphics.DrawLine(
                pen,
                graphX,
                y,
                graphX + graphWidth,
                y
            );


            graphics.DrawString(
                fpsValue.ToString(),
                font,
                brush,
                graphX - 35,
                y - 8
            );
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer.Stop();
            systemTimer.Stop();
            timer.Dispose();
            systemTimer.Dispose();
            cpuCounter.Dispose();
            base.OnFormClosed(e);
        }
    }
}
