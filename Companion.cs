using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

static class Gcta
{
    static readonly string Local = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GCTA");
    static readonly HttpClient Discord = new HttpClient(new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false
    }) { Timeout = TimeSpan.FromSeconds(20) };
    const string SettingsFile = "parametres.dat";
    const string CompanionMutex = @"Local\GCTA.Companion";
    static readonly string InstalledExe = Path.Combine(Local, "GCTA.exe");

    static string ShortcutPath(Environment.SpecialFolder folder)
    {
        return Path.Combine(Environment.GetFolderPath(folder), "GCTA Companion.lnk");
    }

    static void SetShortcut(string path, bool enabled)
    {
        if (File.Exists(path) == enabled) return;
        if (!enabled)
        {
            File.Delete(path);
            return;
        }
        if (!File.Exists(InstalledExe)) throw new FileNotFoundException();
        dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell", true));
        dynamic shortcut = shell.CreateShortcut(path);
        shortcut.TargetPath = InstalledExe;
        shortcut.WorkingDirectory = Local;
        shortcut.IconLocation = InstalledExe + ",0";
        shortcut.Save();
    }

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            if (args.Length > 0 && args[0] == "--self-test")
            {
                SelfTest();
                return;
            }
            if (args.Length > 0 && args[0] == "--install")
            {
                Install();
                return;
            }
            bool firstInstance;
            using (var mutex = new System.Threading.Mutex(true, CompanionMutex, out firstInstance))
            {
                if (!firstInstance)
                {
                    MessageBox.Show("GCTA est déjà lancé. Regarde les icônes près de l’horloge.", "GCTA");
                    return;
                }
                Application.Run(new Companion());
            }
        }
        catch (Exception ex)
        {
            if (args.Length > 0 && args[0] == "--self-test")
            {
                Environment.ExitCode = 1;
                return;
            }
            MessageBox.Show("Opération impossible. Type d’erreur : " + ex.GetType().Name, "GCTA");
        }
    }

    static void Install()
    {
        var gamePathFile = Path.Combine(Local, "wow.txt");
        var update = File.Exists(InstalledExe) || File.Exists(gamePathFile);
        var game = File.Exists(gamePathFile) ? File.ReadAllText(gamePathFile).Trim() : null;
        if (update && MessageBox.Show(
            "GCTA est déjà installé. Veux-tu le mettre à jour en gardant tes webhooks, tes options Windows et la position du bouton dans WoW ?",
            "Mise à jour de GCTA", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            return;

        if (!IsForeverFolder(game))
        {
            game = PickForeverFolder();
            if (game == null) return;
        }

        while (true)
        {
            bool ready;
            using (var mutex = new System.Threading.Mutex(true, CompanionMutex, out ready))
            {
                if (ready)
                {
                    InstallFiles(game, gamePathFile);
                    break;
                }
            }
            if (MessageBox.Show("Quitte GCTA Companion depuis son icône près de l’horloge, puis clique sur OK.",
                "GCTA", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                return;
        }
        System.Diagnostics.Process.Start(InstalledExe);
    }

    static bool IsForeverFolder(string path)
    {
        return !string.IsNullOrEmpty(path)
            && string.Equals(Path.GetFileName(path.TrimEnd('\\', '/')), "_forever_", StringComparison.OrdinalIgnoreCase)
            && File.Exists(Path.Combine(path, "Wow.exe"));
    }

    static string PickForeverFolder()
    {
        using (var picker = new FolderBrowserDialog())
        {
            picker.Description = "Choisis le dossier World of Warcraft (ou directement _forever_). Ferme WoW avant de continuer.";
            picker.ShowNewFolderButton = false;
            while (picker.ShowDialog() == DialogResult.OK)
            {
                var game = IsForeverFolder(picker.SelectedPath) ? picker.SelectedPath : Path.Combine(picker.SelectedPath, "_forever_");
                if (IsForeverFolder(game)) return game;
                MessageBox.Show("Client Forever introuvable. Sélectionne le dossier qui contient _forever_.", "GCTA");
            }
        }
        return null;
    }

    static void InstallFiles(string game, string gamePathFile)
    {
        var source = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var files = new[] { "GCTA.toc", "GCTA.lua", "Orb.tga" };
        foreach (var file in files)
            if (!File.Exists(Path.Combine(source, "GCTA", file)))
                throw new FileNotFoundException();
        Directory.CreateDirectory(Local);
        var addon = Path.Combine(game, "Interface", "AddOns", "GCTA");
        Directory.CreateDirectory(addon);
        foreach (var file in files)
            File.Copy(Path.Combine(source, "GCTA", file), Path.Combine(addon, file), true);
        File.Copy(Assembly.GetExecutingAssembly().Location, InstalledExe, true);
        if (!File.Exists(gamePathFile) || File.ReadAllText(gamePathFile).Trim() != game)
            File.WriteAllText(gamePathFile, game);
        var launcher = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "GCTA Companion.cmd");
        if (!File.Exists(launcher))
            File.WriteAllText(launcher, "@echo off\r\nstart \"\" \"%LOCALAPPDATA%\\GCTA\\GCTA.exe\"\r\n", Encoding.ASCII);
    }

    static bool ValidWebhook(string value)
    {
        return Regex.IsMatch(value, @"\Ahttps://discord\.com/api/webhooks/[0-9]{1,24}/[A-Za-z0-9_-]{20,150}\z",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    static Rectangle FindCard(Bitmap image)
    {
        int cx = image.Width / 2, cy = image.Height / 2;
        // The card is centered in UIParent. Infer the scale from the four
        // markers' known offsets and confirm their color on a second ring.
        for (int dx = 65; dx < cx - 4; dx++)
        {
            int dy = (int)Math.Round(dx * 84.0 / 252);
            if (dy >= cy - 4) break;
            if (!MarkersMatch(image, cx, cy, dx, dy)) continue;
            int sample = Math.Max(2, dx / 54);
            if (!MarkersMatch(image, cx, cy, dx + sample, dy + sample)
                || !MarkersMatch(image, cx, cy, dx - sample, dy - sample))
                continue;
            // Stay inside the markers. Even rounding at fractional UI scale
            // cannot expose pixels outside the card.
            int inset = Math.Max(3, dx / 28);
            var crop = Rectangle.FromLTRB(cx - dx + inset, cy - dy + inset,
                cx + dx - inset, cy + dy - inset);
            if (crop.Width >= 120 && crop.Height >= 30) return crop;
        }
        return Rectangle.Empty;
    }

    static bool MarkersMatch(Bitmap image, int cx, int cy, int dx, int dy)
    {
        if (cx + dx >= image.Width || cy + dy >= image.Height || cx - dx < 0 || cy - dy < 0)
            return false;
        var magenta = image.GetPixel(cx - dx, cy - dy);
        var cyan = image.GetPixel(cx + dx, cy - dy);
        var yellow = image.GetPixel(cx - dx, cy + dy);
        var green = image.GetPixel(cx + dx, cy + dy);
        // WoW captures can mute the colors. Check their pattern and contrast
        // across all four corners at each sample, without fixed RGB levels.
        return Math.Min(magenta.R, yellow.R) - Math.Max(cyan.R, green.R) >= 12
            && Math.Min(cyan.G, Math.Min(yellow.G, green.G)) - magenta.G >= 25
            && Math.Min(magenta.B, cyan.B) - Math.Max(yellow.B, green.B) >= 25;
    }

    static byte[] Extract(string path)
    {
        using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            if (input.Length > 48 * 1024 * 1024) return null;
            using (var image = new Bitmap(input))
            {
                if (image.Width > 10000 || image.Height > 6000) return null;
                var crop = FindCard(image);
                if (crop.IsEmpty) return null;
                using (var card = image.Clone(crop, PixelFormat.Format24bppRgb))
                using (var output = new MemoryStream())
                {
                    card.Save(output, ImageFormat.Png);
                    return output.ToArray();
                }
            }
        }
    }

    static async Task<bool> Send(string webhook, byte[] card)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            using (var form = new MultipartFormDataContent())
            {
                const string payload = "{\"content\":\"**GCTA · Nouvelle capture**\",\"allowed_mentions\":{\"parse\":[]}}";
                form.Add(new StringContent(payload, Encoding.UTF8, "application/json"), "payload_json");
                var attachment = new ByteArrayContent(card);
                attachment.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                form.Add(attachment, "files[0]", "gcta.png");
                using (var response = await Discord.PostAsync(webhook + "?wait=true", form))
                {
                    if ((int)response.StatusCode != 429 || attempt != 0)
                        return response.IsSuccessStatusCode;
                    var retry = response.Headers.RetryAfter;
                    if (retry == null || !retry.Delta.HasValue || retry.Delta.Value.TotalSeconds > 60)
                        return false;
                    await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(100, retry.Delta.Value.TotalMilliseconds)));
                }
            }
        }
        return false;
    }

    sealed class Companion : ApplicationContext
    {
        readonly NotifyIcon tray;
        readonly Icon orbIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        readonly Timer timer = new Timer { Interval = 1000 };
        readonly string screenshots;
        readonly Dictionary<string, long> seen = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        readonly string settings = Path.Combine(Local, SettingsFile);
        string[] webhooks = new string[0];
        bool working;
        bool paused;

        public Companion()
        {
            Directory.CreateDirectory(Local);
            screenshots = Path.Combine(File.ReadAllText(Path.Combine(Local, "wow.txt")).Trim(), "Screenshots");
            if (File.Exists(settings))
            {
                var data = ProtectedData.Unprotect(File.ReadAllBytes(settings), null, DataProtectionScope.CurrentUser);
                webhooks = Encoding.UTF8.GetString(data).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string url in webhooks)
                    if (!ValidWebhook(url)) throw new InvalidDataException();
            }
            var menu = new ContextMenuStrip();
            menu.Items.Add("Réglages", null, delegate { Configure(); });
            var pauseItem = new ToolStripMenuItem("Pause");
            pauseItem.Click += delegate
            {
                paused = !paused;
                pauseItem.Checked = paused;
                if (!paused) MarkExisting();
                Status(paused ? "En pause." : "Prêt.", false);
            };
            menu.Items.Add(pauseItem);
            menu.Items.Add("Ouvrir les captures", null, delegate
            {
                if (Directory.Exists(screenshots)) System.Diagnostics.Process.Start("explorer.exe", "\"" + screenshots + "\"");
                else Status("Le dossier Screenshots apparaîtra après la première capture WoW.", true);
            });
            menu.Items.Add("Quitter", null, delegate { ExitThread(); });
            tray = new NotifyIcon
            {
                Icon = orbIcon,
                Text = "GCTA Companion",
                ContextMenuStrip = menu,
                Visible = true
            };
            tray.DoubleClick += delegate { Configure(); };
            MarkExisting();
            timer.Tick += async delegate { await CheckFiles(); };
            timer.Start();
            if (webhooks.Length == 0) Configure();
            Status(webhooks.Length == 0 ? "Mode local. Aucun envoi Discord." : "Prêt.", true);
        }

        IEnumerable<FileInfo> Captures()
        {
            if (!Directory.Exists(screenshots)) yield break;
            foreach (var file in new DirectoryInfo(screenshots).EnumerateFiles())
            {
                string ext = file.Extension.ToLowerInvariant();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png") yield return file;
            }
        }

        void MarkExisting()
        {
            foreach (var file in Captures()) seen[file.FullName] = file.LastWriteTimeUtc.Ticks;
        }

        async Task CheckFiles()
        {
            if (working || paused) return;
            working = true;
            try
            {
                foreach (var file in Captures())
                {
                    if (paused) break;
                    long stamp = file.LastWriteTimeUtc.Ticks, previous;
                    if (seen.TryGetValue(file.FullName, out previous) && previous == stamp) continue;
                    if (DateTime.UtcNow - file.LastWriteTimeUtc < TimeSpan.FromSeconds(1)) continue;
                    byte[] card;
                    try { card = await Task.Run(() => Extract(file.FullName)); }
                    catch (IOException) { continue; }
                    catch (ArgumentException) { card = null; }
                    seen[file.FullName] = stamp;
                    if (card == null || paused) continue;
                    var destinations = webhooks;
                    if (destinations.Length == 0)
                    {
                        File.WriteAllBytes(Path.Combine(Local, "Derniere-carte.png"), card);
                        Status("Fiche détectée et recadrée. Aucun webhook configuré.", true);
                        continue;
                    }
                    var failed = new List<string>();
                    for (int i = 0; i < destinations.Length; i++)
                    {
                        bool sent;
                        try { sent = await Send(destinations[i], card); }
                        catch (HttpRequestException) { sent = false; }
                        catch (TaskCanceledException) { sent = false; }
                        if (!sent) failed.Add((i + 1).ToString());
                    }
                    Status(failed.Count == 0 ? "Carte publiée dans " + destinations.Length + " salon(s)."
                        : "Envoi non confirmé pour les destinations : " + string.Join(", ", failed), true);
                }
            }
            catch (Exception ex)
            {
                Status("Erreur de capture : " + ex.GetType().Name, false);
            }
            finally { working = false; }
        }

        void Status(string text, bool show)
        {
            tray.Text = text.Length <= 63 ? text : text.Substring(0, 60) + "...";
            if (show) tray.ShowBalloonTip(2500, "GCTA", text, ToolTipIcon.Info);
        }

        void Configure()
        {
            bool previousPause = paused;
            paused = true;
            try
            {
                using (var form = new Form { Text = "GCTA · Réglages", ClientSize = new Size(480, 330), Icon = orbIcon,
                    StartPosition = FormStartPosition.CenterScreen, FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, MinimizeBox = false })
                {
                    form.Controls.Add(new Label { Text = "Dossier des captures WoW", Left = 15, Top = 15, AutoSize = true });
                    form.Controls.Add(new TextBox { Text = screenshots, ReadOnly = true, Left = 15, Top = 39, Width = 450 });
                    form.Controls.Add(new Label { Text = "Webhooks Discord · dans l’ordre des destinations", Left = 15, Top = 76, AutoSize = true });
                    var destinations = new ListBox { Left = 15, Top = 100, Width = 450, Height = 70 };
                    var pending = new List<string>(webhooks);
                    foreach (string url in pending) destinations.Items.Add("Destination " + (destinations.Items.Count + 1));
                    var token = new TextBox { Left = 15, Top = 180, Width = 347, UseSystemPasswordChar = true, MaxLength = 240 };
                    var add = new Button { Text = "Ajouter", Left = 370, Top = 178, Width = 95 };
                    add.Click += delegate
                    {
                        string url = token.Text.Trim();
                        if (!ValidWebhook(url) || pending.Contains(url) || pending.Count >= 10)
                        {
                            MessageBox.Show(form, "Colle une URL Discord valide et unique. Maximum : 10 destinations.", "GCTA");
                            return;
                        }
                        pending.Add(url);
                        destinations.Items.Add("Destination " + pending.Count);
                        token.Clear();
                    };
                    var remove = new Button { Text = "Supprimer", Left = 15, Top = 225, Width = 95 };
                    remove.Click += delegate
                    {
                        if (destinations.SelectedIndex < 0) return;
                        pending.RemoveAt(destinations.SelectedIndex);
                        destinations.Items.Clear();
                        for (int i = 1; i <= pending.Count; i++) destinations.Items.Add("Destination " + i);
                    };
                    var startupPath = ShortcutPath(Environment.SpecialFolder.Startup);
                    var desktopPath = ShortcutPath(Environment.SpecialFolder.DesktopDirectory);
                    var startup = new CheckBox { Text = "Démarrer avec le PC", Left = 15, Top = 261,
                        AutoSize = true, Checked = File.Exists(startupPath) };
                    var desktop = new CheckBox { Text = "Créer un raccourci bureau", Left = 15, Top = 290,
                        AutoSize = true, Checked = File.Exists(desktopPath) };
                    var save = new Button { Text = "Enregistrer", Left = 348, Top = 287, Width = 117 };
                    save.Click += delegate
                    {
                        if (token.TextLength > 0)
                        {
                            MessageBox.Show(form, "Clique sur Ajouter pour enregistrer le lien saisi.", "GCTA");
                            return;
                        }
                        try
                        {
                            var data = Encoding.UTF8.GetBytes(string.Join("\n", pending));
                            File.WriteAllBytes(settings, ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser));
                            webhooks = pending.ToArray();
                            SetShortcut(startupPath, startup.Checked);
                            SetShortcut(desktopPath, desktop.Checked);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(form, "Impossible de terminer l’enregistrement. Type d’erreur : " + ex.GetType().Name, "GCTA");
                            return;
                        }
                        form.Close();
                    };
                    form.Controls.AddRange(new Control[] { destinations, token, add, remove, startup, desktop, save });
                    form.ShowDialog();
                }
            }
            finally { paused = previousPause; }
        }

        protected override void ExitThreadCore()
        {
            timer.Dispose();
            tray.Visible = false;
            tray.Dispose();
            orbIcon.Dispose();
            base.ExitThreadCore();
        }
    }

    static void SelfTest()
    {
        if (!ValidWebhook("https://discord.com/api/webhooks/123456/abcdefghijklmnopqrst")
            || ValidWebhook("https://discord.com.example/api/webhooks/123456/abcdefghijklmnopqrst")
            || ValidWebhook("https://discord.com/api/webhooks/123456/abcdefghijklmnopqrst?x=y"))
            throw new Exception();
        foreach (double scale in new[] { 0.63, 1.0, 1.44, 2.0 })
        {
            using (var image = new Bitmap(1920, 1080))
            {
                if (!FindCard(image).IsEmpty) throw new Exception();
                int w = (int)Math.Round(520 * scale), h = (int)Math.Round(184 * scale);
                int x = 960 - w / 2, y = 540 - h / 2, m = (int)Math.Round(16 * scale);
                using (var g = Graphics.FromImage(image))
                {
                    g.FillRectangle(Brushes.Black, x, y, w, h);
                    g.FillRectangle(Brushes.Magenta, x, y, m, m);
                    g.FillRectangle(Brushes.Cyan, x + w - m, y, m, m);
                    g.FillRectangle(Brushes.Yellow, x, y + h - m, m, m);
                    g.FillRectangle(Brushes.Lime, x + w - m, y + h - m, m, m);
                }
                var crop = FindCard(image);
                if (crop.IsEmpty || !new Rectangle(x, y, w, h).Contains(crop)) throw new Exception();
                var frame = Rectangle.FromLTRB(
                    x + (int)Math.Floor(24 * scale), y + (int)Math.Floor(22 * scale),
                    x + w - (int)Math.Floor(24 * scale), y + h - (int)Math.Floor(22 * scale));
                if (!crop.Contains(frame)) throw new Exception();
                using (var encoded = new MemoryStream())
                {
                    image.Save(encoded, ImageFormat.Jpeg);
                    encoded.Position = 0;
                    using (var decoded = new Bitmap(encoded))
                        if (!FindCard(decoded).Contains(frame)) throw new Exception();
                }
            }
        }
    }
}
