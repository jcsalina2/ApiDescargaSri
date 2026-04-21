using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public sealed class BrowserMobHarRecorder : IDisposable
{
    private readonly string _bmpBatPath;
    private readonly int _controlPort;
    private Process? _bmpProcess;
    private readonly HttpClient _http = new HttpClient();
    public int ProxyPort { get; private set; }

    public BrowserMobHarRecorder(string bmpBatPath, int controlPort = 8080)
    {
        _bmpBatPath = bmpBatPath;
        _controlPort = controlPort;
    }

    private string ControlBaseUrl => $"http://127.0.0.1:{_controlPort}";

    public async Task StartBmpAsync()
    {
        if (!File.Exists(_bmpBatPath))
            throw new FileNotFoundException("No encuentro BrowserMob Proxy en:", _bmpBatPath);

        _bmpProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _bmpBatPath,
                Arguments = $"--port {_controlPort}",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _bmpProcess.Start();

        await EsperarBmpArribaAsync();
    }

    private async Task EsperarBmpArribaAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var resp = await _http.GetAsync(ControlBaseUrl + "/proxy");
                if (resp.IsSuccessStatusCode) return;
            }
            catch { }
            await Task.Delay(250);
        }
        throw new Exception("BrowserMob Proxy no respondió en el puerto " + _controlPort);
    }

    public async Task CreateProxyAsync()
    {
        var resp = await _http.PostAsync(ControlBaseUrl + "/proxy", new StringContent(""));
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        ProxyPort = doc.RootElement.GetProperty("port").GetInt32();
        if (ProxyPort <= 0) throw new Exception("No se pudo obtener el puerto del proxy.");
    }

    public async Task StartHarAsync(string harName = "session", bool captureHeaders = true, bool captureContent = false)
    {
        if (ProxyPort <= 0) throw new InvalidOperationException("Primero CreateProxyAsync().");

        // Enable HAR
        var url = $"{ControlBaseUrl}/proxy/{ProxyPort}/har" +
                  $"?initialPageRef={Uri.EscapeDataString(harName)}" +
                  $"&captureHeaders={(captureHeaders ? "true" : "false")}" +
                  $"&captureContent={(captureContent ? "true" : "false")}";

        var resp = await _http.PutAsync(url, new StringContent(""));
        resp.EnsureSuccessStatusCode();
    }

    public async Task SaveHarAsync(string harFilePath)
    {
        if (ProxyPort <= 0) throw new InvalidOperationException("Proxy no creado.");

        var resp = await _http.GetAsync($"{ControlBaseUrl}/proxy/{ProxyPort}/har");
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();

        var fullPath = Path.GetFullPath(harFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, json, Encoding.UTF8);

        Console.WriteLine("[HAR] Guardado en: " + fullPath);
    }

    public void Dispose()
    {
        try { _http.Dispose(); } catch { }
        try
        {
            if (_bmpProcess != null && !_bmpProcess.HasExited)
                _bmpProcess.Kill(true);
        }
        catch { }
    }
}
