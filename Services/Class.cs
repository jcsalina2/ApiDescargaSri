using OpenQA.Selenium;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

public static class NetworkLogHelper
{
    public static void GuardarNetworkLogs(IWebDriver webDriver, string rutaTxt)
    {
        var perfLogs = webDriver.Manage().Logs.GetLog(LogType.Performance);

        var sb = new StringBuilder();

        foreach (var e in perfLogs)
        {
            if (TryFormatNetworkLine(e.Message, out var linea))
                sb.AppendLine(linea);
        }

        var dir = Path.GetDirectoryName(rutaTxt);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(rutaTxt, sb.ToString(), Encoding.UTF8);
    }

    public static void AppendNetworkLogs(IWebDriver webDriver, string rutaTxt)
    {
        var perfLogs = webDriver.Manage().Logs.GetLog(LogType.Performance);

        var sb = new StringBuilder();

        foreach (var e in perfLogs)
        {
            if (TryFormatNetworkLine(e.Message, out var linea))
                sb.AppendLine(linea);
        }

        var dir = Path.GetDirectoryName(rutaTxt);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.AppendAllText(rutaTxt, sb.ToString(), Encoding.UTF8);
    }

    public static bool TryFormatNetworkLine(string performanceMessageJson, out string linea)
    {
        linea = "";

        try
        {
            using var doc = JsonDocument.Parse(performanceMessageJson);

            if (!doc.RootElement.TryGetProperty("message", out var msgEl))
                return false;

            // En tu caso "message" ya es Object con {method, params}
            if (msgEl.ValueKind != JsonValueKind.Object)
                return false;

            if (!msgEl.TryGetProperty("method", out var methodEl))
                return false;

            var method = methodEl.GetString() ?? "";

            msgEl.TryGetProperty("params", out var p);

            // =========================
            // REQ
            // =========================
            if (method == "Network.requestWillBeSent")
            {
                string url = "";
                string httpMethod = "";
                string type = "";

                if (p.ValueKind == JsonValueKind.Object)
                {
                    if (p.TryGetProperty("request", out var req) && req.ValueKind == JsonValueKind.Object)
                    {
                        if (req.TryGetProperty("url", out var urlEl)) url = urlEl.GetString() ?? "";
                        if (req.TryGetProperty("method", out var hmEl)) httpMethod = hmEl.GetString() ?? "";
                    }

                    // type a veces viene como "Document", "XHR", "Fetch", etc.
                    if (p.TryGetProperty("type", out var tEl)) type = tEl.GetString() ?? "";
                }

                if (string.IsNullOrWhiteSpace(url)) return false;

                linea = $"REQ [{type}] {httpMethod} {url}";
                return true;
            }

            // =========================
            // RES
            // =========================
            if (method == "Network.responseReceived")
            {
                string url = "";
                string status = "";
                string mime = "";
                string type = "";

                if (p.ValueKind == JsonValueKind.Object)
                {
                    if (p.TryGetProperty("type", out var tEl)) type = tEl.GetString() ?? "";

                    if (p.TryGetProperty("response", out var resp) && resp.ValueKind == JsonValueKind.Object)
                    {
                        if (resp.TryGetProperty("url", out var urlEl)) url = urlEl.GetString() ?? "";
                        if (resp.TryGetProperty("status", out var stEl)) status = stEl.ToString();
                        if (resp.TryGetProperty("mimeType", out var mtEl)) mime = mtEl.GetString() ?? "";
                    }
                }

                if (string.IsNullOrWhiteSpace(url)) return false;

                linea = $"RES [{type}] {status} {mime} {url}";
                return true;
            }

            // =========================
            // FAIL (útil cuando SRI bloquea algo)
            // =========================
            if (method == "Network.loadingFailed")
            {
                string requestId = "";
                string errorText = "";
                bool canceled = false;

                if (p.ValueKind == JsonValueKind.Object)
                {
                    if (p.TryGetProperty("requestId", out var rid)) requestId = rid.GetString() ?? "";
                    if (p.TryGetProperty("errorText", out var et)) errorText = et.GetString() ?? "";
                    if (p.TryGetProperty("canceled", out var c)) canceled = c.GetBoolean();
                }

                linea = $"FAIL requestId={requestId} canceled={canceled} error={errorText}";
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
