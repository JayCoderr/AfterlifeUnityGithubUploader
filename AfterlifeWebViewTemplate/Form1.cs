using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AfterlifeUnityGitTool
{
    public partial class Form1 : Form
    {
        public class GitFile
        {
            [JsonPropertyName("name")]
            public string name { get; set; }

            [JsonPropertyName("path")]
            public string path { get; set; }

            [JsonPropertyName("content")]
            public string content { get; set; }
        }
        public class GitHubPayload
        {
            public string user { get; set; }
            public string token { get; set; }
            public string repo { get; set; }
            public string branch { get; set; }
            public bool save { get; set; }

            public string[] files { get; set; }
        }

        // =========================
        // GITHUB STATE
        // =========================
        private static readonly HttpClient http = new HttpClient();

        private string githubUser = "";
        private string githubToken = "";

        private string repoName = "MyRepo";
        private string branch = "main";
        private string directoryPath = "";
        private string visibility = "private";
        private string repoDescription = "";

        // =========================
        // WINDOW DRAG
        // =========================
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        // =========================
        // RESIZE
        // =========================
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private const int resizeAreaSize = 10;

        private List<GitFile> droppedFiles = new List<GitFile>();
        private HttpListener _server;
        public Form1()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(550, 650);

            MouseDown += Form1_MouseDown;

            RunEnsureAsync();
        }

        // =========================
        // INIT
        // =========================
        async void RunEnsureAsync()
        {
            await webView21.EnsureCoreWebView2Async();
            await webView22.EnsureCoreWebView2Async();

            SendToast("WebViews Ready");

            // =========================
            // START LOCAL SERVER
            // =========================

            StartLocalServer();

            SendToast("Local server running @ http://localhost:1337");

            // =========================
            // EVENTS
            // =========================

            webView21.CoreWebView2.WebMessageReceived += WebMessageReceived;
            webView22.CoreWebView2.WebMessageReceived += WebMessageReceived;

            SendToast("Events Attached");

            LoadNavbar();

            // =========================
            // LOAD VIA HTTP (NOT FILE)
            // =========================

            webView22.Source = new Uri("http://localhost:1337/applicationUI.html");
        }
        private void StartLocalServer()
        {
            string uiRoot = Path.Combine(Application.StartupPath, "raw", "applicationDesign");
            string rawRoot = Path.Combine(Application.StartupPath, "raw");

            _server = new HttpListener();
            _server.Prefixes.Add("http://localhost:1337/");
            _server.Start();

            Task.Run(async () =>
            {
                while (_server.IsListening)
                {
                    var ctx = await _server.GetContextAsync();

                    try
                    {
                        string path = ctx.Request.Url.AbsolutePath.TrimStart('/');

                        if (string.IsNullOrEmpty(path))
                            path = "applicationUI.html";

                        string baseRoot;

                        // =========================
                        // ROUTING RULES
                        // =========================

                        if (path.StartsWith("json/") || path.StartsWith("raw/"))
                        {
                            baseRoot = rawRoot;
                        }
                        else
                        {
                            baseRoot = uiRoot;
                        }

                        string fullPath = Path.GetFullPath(Path.Combine(baseRoot, path));

                        if (!fullPath.StartsWith(baseRoot) || !File.Exists(fullPath))
                        {
                            ctx.Response.StatusCode = 404;
                            ctx.Response.Close();
                            continue;
                        }

                        byte[] data = File.ReadAllBytes(fullPath);

                        ctx.Response.ContentType = GetMimeType(fullPath);
                        ctx.Response.OutputStream.Write(data, 0, data.Length);
                        ctx.Response.OutputStream.Close();
                    }
                    catch
                    {
                        ctx.Response.StatusCode = 500;
                        ctx.Response.Close();
                    }
                }
            });
        }
        private string GetMimeType(string file)
        {
            return Path.GetExtension(file).ToLower() switch
            {
                ".html" => "text/html",
                ".js" => "application/javascript",
                ".css" => "text/css",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "text/plain"
            };
        }
        private async void WebMessageReceived(
            object sender,
            CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                // =========================
                // RAW STRING COMMANDS
                // =========================
                try
                {
                    string rawString = args.TryGetWebMessageAsString();

                    if (!string.IsNullOrWhiteSpace(rawString))
                    {
                        switch (rawString.ToLower().Trim())
                        {
                            case "minimize":
                                this.WindowState = FormWindowState.Minimized;
                                return;

                            case "maximize":
                                this.WindowState =
                                    this.WindowState == FormWindowState.Maximized
                                        ? FormWindowState.Normal
                                        : FormWindowState.Maximized;
                                return;

                            case "close":
                                Application.Exit();
                                return;
                        }
                    }
                }
                catch
                {
                    // Ignore non-string messages
                }

                // =========================
                // JSON MESSAGE FLOW
                // =========================
                string rawJson = args.WebMessageAsJson;

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    SendToast("Empty message ❌");
                    return;
                }

                using var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;

                // =========================
                // TYPE CHECK
                // =========================
                if (!root.TryGetProperty("type", out var typeProp) ||
                    typeProp.ValueKind != JsonValueKind.String)
                {
                    SendToast("Missing or invalid type ❌");
                    return;
                }

                string type = typeProp.GetString()?.Trim();

                SendToast("TYPE: " + type);

                if (!string.Equals(type, "github:dropUpload", StringComparison.OrdinalIgnoreCase))
                {
                    SendToast("Ignored type");
                    return;
                }

                // =========================
                // DATA CHECK
                // =========================
                if (!root.TryGetProperty("data", out var data) ||
                    data.ValueKind != JsonValueKind.Object)
                {
                    SendToast("Missing data object ❌");
                    return;
                }

                // =========================
                // SAFE STATE UPDATE
                // =========================
                if (data.TryGetProperty("user", out var userProp) &&
                    userProp.ValueKind == JsonValueKind.String)
                {
                    var val = userProp.GetString();

                    if (!string.IsNullOrWhiteSpace(val))
                        githubUser = val;
                }

                if (data.TryGetProperty("token", out var tokenProp) &&
                    tokenProp.ValueKind == JsonValueKind.String)
                {
                    var val = tokenProp.GetString();

                    if (!string.IsNullOrWhiteSpace(val))
                        githubToken = val;
                }

                if (data.TryGetProperty("repo", out var repoProp) &&
                    repoProp.ValueKind == JsonValueKind.String)
                {
                    var val = repoProp.GetString();

                    if (!string.IsNullOrWhiteSpace(val))
                        repoName = val;
                }

                if (data.TryGetProperty("branch", out var branchProp) &&
                    branchProp.ValueKind == JsonValueKind.String)
                {
                    var val = branchProp.GetString();

                    if (!string.IsNullOrWhiteSpace(val))
                        branch = val;
                }

                if (data.TryGetProperty("directoryPath", out var dirProp) &&
                    dirProp.ValueKind == JsonValueKind.String)
                {
                    directoryPath = dirProp.GetString()?.Trim() ?? "";
                }

                if (data.TryGetProperty("visibility", out var visProp) &&
                    visProp.ValueKind == JsonValueKind.String)
                {
                    visibility = visProp.GetString()?.Trim().ToLower();
                }
                
                if (data.TryGetProperty("repoDescription", out var repoDesc) &&
                repoDesc.ValueKind == JsonValueKind.String)
                {
                    repoDescription = repoDesc.GetString()?.Trim().ToLower();
                }

                if (string.IsNullOrWhiteSpace(branch))
                    branch = "main";

                // =========================
                // VALIDATION
                // =========================
                if (string.IsNullOrWhiteSpace(githubToken))
                {
                    SendToast("GitHub token missing ❌");
                    return;
                }

                if (string.IsNullOrWhiteSpace(repoName))
                {
                    SendToast("Repo name missing ❌");
                    await LogError("repoName empty", "VALIDATION");
                    return;
                }

                SendToast($"Repo OK: {repoName}");

                // =========================
                // FILES SAFE PARSE
                // =========================
                List<GitFile> files = new();

                try
                {
                    if (!data.TryGetProperty("files", out var filesProp))
                    {
                        SendToast("No files received ❌");
                        return;
                    }

                    if (filesProp.ValueKind == JsonValueKind.String)
                    {
                        string raw = filesProp.GetString();

                        files = JsonSerializer.Deserialize<List<GitFile>>(raw) ?? new();
                    }
                    else if (filesProp.ValueKind == JsonValueKind.Array)
                    {
                        files = JsonSerializer.Deserialize<List<GitFile>>(
                            filesProp.GetRawText()
                        ) ?? new();
                    }
                    else
                    {
                        SendToast("Invalid files format ❌");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    await LogError(ex, "FILE PARSE");
                    SendToast("File parse failed ❌");
                    return;
                }

                if (files == null || files.Count == 0)
                {
                    SendToast("Empty drop list ❌");
                    return;
                }

                int folderCount =
                    data.TryGetProperty("folderCount", out var fc) &&
                    fc.ValueKind == JsonValueKind.Number
                        ? fc.GetInt32()
                        : 0;

                SendToast($"Uploading {files.Count} files / {folderCount} folders...");

                // =========================
                // ENSURE REPO
                // =========================
                SendToast("Checking repo...");

                bool repoOk = await EnsureRepositoryAsync(visibility);

                if (!repoOk)
                {
                    SendToast("Repo create/check failed ❌");
                    await LogError("EnsureRepositoryAsync failed", "GITHUB");
                    return;
                }

                SendToast("Repo ready ✔");

                // =========================
                // UPLOAD LOOP
                // =========================
                foreach (var file in files)
                {
                    try
                    {
                        if (file == null || string.IsNullOrWhiteSpace(file.content))
                        {
                            SendToast("Skipping invalid file ❌ " + file?.path);
                            continue;
                        }

                        SendToast("Uploading: " + file.path);

                        string finalPath =
                            string.IsNullOrWhiteSpace(directoryPath)
                                ? file.path
                                : $"{directoryPath.TrimEnd('/')}/{file.path}";

                        await UploadFile(finalPath, file.content);
                    }
                    catch (Exception exFile)
                    {
                        await LogError(exFile, "UPLOAD FILE LOOP");
                    }
                }

                SendToast("Upload Complete ✔");

                LoadSuccessPage();
            }
            catch (Exception ex)
            {
                await LogError(ex, "WebMessageReceived");

                SendToast("Fatal error logged ❌");
            }
        }
        private void SetupGitHubClient()
        {
            http.DefaultRequestHeaders.UserAgent.ParseAdd("AfterlifeTool");
            http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", githubToken);
        }
        private async Task<bool> EnsureRepositoryAsync(string visibility)
        {
            SetupGitHubClient();

            try
            {
                // =========================
                // VALIDATION (CRITICAL)
                // =========================
                if (string.IsNullOrWhiteSpace(githubUser))
                {
                    SendToast("Missing GitHub user ❌");
                    await LogError("githubUser is null/empty", "EnsureRepositoryAsync");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(repoName))
                {
                    SendToast("Missing repo name ❌");
                    await LogError("repoName is null/empty", "EnsureRepositoryAsync");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(githubToken))
                {
                    SendToast("Missing GitHub token ❌");
                    await LogError("githubToken is null/empty", "EnsureRepositoryAsync");
                    return false;
                }

                // =========================
                // CHECK REPO EXISTS
                // =========================
                string repoUrl = $"https://api.github.com/repos/{githubUser}/{repoName}";

                SendToast("Checking repo...");

                var repoRes = await http.GetAsync(repoUrl);

                if (repoRes.IsSuccessStatusCode)
                {
                    SendToast("Repo exists ✔");
                    return true;
                }

                // =========================
                // DEBUG FAILURE RESPONSE
                // =========================
                string errorBody = await repoRes.Content.ReadAsStringAsync();
                await LogError($"Repo check failed: {repoRes.StatusCode}\n{errorBody}", "EnsureRepositoryAsync");

                SendToast($"Repo not found ({repoRes.StatusCode}) → creating...");

                // =========================
                // CREATE REPO
                // =========================
                var createPayload = new
                {
                    name = repoName,
                    description = repoDescription,
                    @private = visibility == "private",
                    auto_init = true
                };

                string json = JsonSerializer.Serialize(createPayload);

                var createRes = await http.PostAsync(
                    "https://api.github.com/user/repos",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                string createBody = await createRes.Content.ReadAsStringAsync();

                if (!createRes.IsSuccessStatusCode)
                {
                    await LogError(
                        $"Repo creation failed: {createRes.StatusCode}\n{createBody}",
                        "EnsureRepositoryAsync"
                    );

                    SendToast("Repo creation failed ❌");
                    return false;
                }

                SendToast("Repo created ✔");
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "EnsureRepositoryAsync");
                MessageBox.Show("Repo check crashed ❌");
                return false;
            }
        }
        private async Task UploadFolderRecursive(string localPath, string repoPath)
        {
            // =========================
            // FILES
            // =========================
            foreach (var file in Directory.GetFiles(localPath))
            {
                string repoFilePath = repoPath + Path.GetFileName(file);

                bool ok = await UploadFile(file, repoFilePath);

                if (!ok)
                    SendToast("FAILED: " + repoFilePath);
            }

            // =========================
            // SUBFOLDERS
            // =========================
            foreach (var dir in Directory.GetDirectories(localPath))
            {
                string folderName = Path.GetFileName(dir);

                await UploadFolderRecursive(
                    dir,
                    repoPath + folderName + "/"
                );
            }
        }
        private async Task<bool> UploadFile(string repoPath, string base64Content)
        {
            try
            {
                SetupGitHubClient();

                // =========================
                // VALIDATION
                // =========================

                if (string.IsNullOrWhiteSpace(githubUser))
                {
                    SendToast("GitHub user missing ❌");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(repoName))
                {
                    SendToast("Repo missing ❌");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(branch))
                {
                    branch = "main";
                }

                if (string.IsNullOrWhiteSpace(repoPath))
                {
                    SendToast("Repo path missing ❌");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(base64Content))
                {
                    SendToast($"Empty base64 ❌ {repoPath}");
                    return false;
                }

                // =========================
                // CLEAN PATH
                // =========================

                repoPath = repoPath
                    .Replace("\\", "/")
                    .TrimStart('/');

                string safePath =
                    Uri.EscapeDataString(repoPath)
                        .Replace("%2F", "/");

                // =========================
                // FILE URL
                // =========================

                string url =
                    $"https://api.github.com/repos/{githubUser}/{repoName}/contents/{safePath}";

                string sha = null;

                // =========================
                // CHECK EXISTING FILE
                // =========================

                try
                {
                    var existing = await http.GetAsync(url);

                    string existingBody =
                        await existing.Content.ReadAsStringAsync();

                    if (existing.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(existingBody);

                        if (doc.RootElement.TryGetProperty("sha", out var shaElement))
                        {
                            sha = shaElement.GetString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await LogError(ex, "Existing File Check");
                }

                // =========================
                // CREATE PAYLOAD
                // =========================

                var payload = new
                {
                    message = $"Upload {repoPath}",
                    content = base64Content,
                    branch = branch,
                    sha = sha
                };

                string jsonPayload =
                    JsonSerializer.Serialize(payload);

                // =========================
                // UPLOAD FILE
                // =========================

                var res = await http.PutAsync(
                    url,
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                );

                string body = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    SendToast($"FAIL {repoPath} ({(int)res.StatusCode})");

                    await LogError(body, "GitHub Upload Failed");
                    return false;
                }

                SendToast($"Uploaded ✔ {repoPath}");

                // =====================================================
                // CREATE LOCAL CONFIG FILE
                // =====================================================

                try
                {
                    // =========================
                    // DIRECTORY ONLY (FIXED)
                    // =========================

                    string directory = Path.GetDirectoryName(repoPath)?
                        .Replace("\\", "/")
                        .Trim('/');

                    string fullPath = string.IsNullOrEmpty(directory)
                        ? ""
                        : directory + "/";

                    // =========================
                    // CONFIG OBJECT
                    // =========================

                    var config = new
                    {
                        lastUploadedFile = repoPath,
                        uploaded = DateTime.UtcNow,

                        branch = branch,

                        repository = repoName,
                        user = githubUser,

                        githubToken = githubToken,

                        repoName = repoName,
                        repoDescription = repoDescription,
                        visibility = visibility,

                        fullPath = fullPath,
                        fileName = Path.GetFileName(repoPath)
                    };

                    string configJson =
                        JsonSerializer.Serialize(
                            config,
                            new JsonSerializerOptions
                            {
                                WriteIndented = true
                            }
                        );

                    // =========================
                    // LOCAL PATH
                    // =========================

                    string configDir = Path.Combine(
                        Application.StartupPath,
                        "raw",
                        "json"
                    );

                    string configPath = Path.Combine(
                        configDir,
                        "config.json"
                    );

                    // =========================
                    // CREATE FOLDER IF NEEDED
                    // =========================

                    Directory.CreateDirectory(configDir);

                    // =========================
                    // WRITE FILE
                    // =========================

                    await File.WriteAllTextAsync(configPath, configJson);

                    SendToast($"Config created ✔ {configPath}");
                }
                catch (Exception ex)
                {
                    await LogError(ex, "Local Config Creation");
                }

                return true;
            }
            catch (Exception ex)
            {
                await LogError(ex, "UploadFile");

                SendToast("Upload exception ❌");
                return false;
            }
        }
        private void OpenBrowserTab(string url)
        {
            Form tab = new Form();

            tab.Width = 1200;
            tab.Height = 800;

            WebView2 webView = new WebView2();
            webView.Dock = DockStyle.Fill;

            tab.Controls.Add(webView);

            tab.Load += async (s, e) =>
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.Navigate(url);
            };

            tab.Show();
        }
        private void LoadSuccessPage()
        {
            string repoUrl =
                $"https://github.com/{githubUser}/{repoName}";

            Task.Delay(1500).ContinueWith(_ =>
            {
                this.Invoke(() =>
                {
                    if (visibility == "public")
                    {
                        OpenBrowserTab(repoUrl);
                    }
                });
            });
            SendToast("Github Was Successful");
            webView22.Source = new Uri("http://localhost:1337/applicationUI.html");
        }
        // =========================
        // UPLOAD FILE TO GITHUB
        // =========================
        private async Task UploadFileToGitHub(string localPath, string repoPath)
        {
            try
            {
                string url =
                    $"https://api.github.com/repos/{githubUser}/{repoName}/contents/{repoPath}";

                byte[] bytes = File.ReadAllBytes(localPath);
                string base64 = Convert.ToBase64String(bytes);

                http.DefaultRequestHeaders.UserAgent.ParseAdd("AfterlifeTool");
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", githubToken);

                // check existing file
                string sha = null;
                var existing = await http.GetAsync(url);

                if (existing.IsSuccessStatusCode)
                {
                    var json = await existing.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    sha = doc.RootElement.GetProperty("sha").GetString();
                }

                var payload = new
                {
                    message = "Upload from tool",
                    content = base64,
                    branch = branch,
                    sha = sha
                };

                string jsonPayload = JsonSerializer.Serialize(payload);

                var response = await http.PutAsync(
                    url,
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                );

                SendToast(response.IsSuccessStatusCode
                    ? "Upload successful ✔"
                    : "Upload failed ❌");
            }
            catch (Exception ex)
            {
                SendToast("Upload error: " + ex.Message);
            }
        }
        private static readonly string ErrorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errorlog.txt");

        private async Task LogError(object ex, string context = "")
        {
            try
            {
                string message = ex switch
                {
                    Exception e => e.ToString(),
                    string s => s,
                    _ => ex?.ToString() ?? "null"
                };

                var msg =
                    "============================\n" +
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
                    (!string.IsNullOrWhiteSpace(context) ? "Context: " + context + "\n" : "") +
                    message +
                    "\n\n";

                File.AppendAllText(ErrorLogPath, msg);
            }
            catch { }

            await Task.CompletedTask;
        }
        // =========================
        // TOAST
        // =========================
        private void SendToast(string msg)
        {
            webView22?.CoreWebView2?.PostWebMessageAsString("toast:" + msg);
        }

        // =========================
        // NAVBAR
        // =========================
        private void LoadNavbar()
        {
            string rawFolder = Path.Combine(Application.StartupPath, "raw");

            var file = Directory.GetFiles(rawFolder, "*.html")
                .OrderByDescending(File.GetLastWriteTime)
                .FirstOrDefault();

            if (file == null) return;

            webView21.CoreWebView2.Navigate("about:blank");
            webView21.CoreWebView2.Navigate(new Uri(file).AbsoluteUri + "?t=" + DateTime.Now.Ticks);
        }

        // =========================
        // DRAG
        // =========================
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        // =========================
        // RESIZE
        // =========================
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            base.WndProc(ref m);

            if (m.Msg != WM_NCHITTEST) return;

            Point p = PointToClient(Cursor.Position);

            bool l = p.X <= resizeAreaSize;
            bool r = p.X >= Width - resizeAreaSize;
            bool t = p.Y <= resizeAreaSize;
            bool b = p.Y >= Height - resizeAreaSize;

            if (t && l) m.Result = (IntPtr)HTTOPLEFT;
            else if (t && r) m.Result = (IntPtr)HTTOPRIGHT;
            else if (b && l) m.Result = (IntPtr)HTBOTTOMLEFT;
            else if (b && r) m.Result = (IntPtr)HTBOTTOMRIGHT;
            else if (l) m.Result = (IntPtr)HTLEFT;
            else if (r) m.Result = (IntPtr)HTRIGHT;
            else if (t) m.Result = (IntPtr)HTTOP;
            else if (b) m.Result = (IntPtr)HTBOTTOM;
            else m.Result = (IntPtr)HTCAPTION;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e); 
        private void panel2_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e); 
        private void panel3_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e); 
        private void panel4_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e); 
        private void panel6_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e);
    }
}