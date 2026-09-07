using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SLCDM.Application.Features.Asignaciones;

internal static class DocxPdfConverter
{
    public static byte[] Convertir(byte[] docx)
    {
        var dir = Path.Combine(Path.GetTempPath(), "slcdm-acta-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var docxPath = Path.Combine(dir, "acta.docx");
        var pdfPath = Path.Combine(dir, "acta.pdf");
        File.WriteAllBytes(docxPath, docx);

        try
        {
            if (TryWord(docxPath, pdfPath) || TryLibreOffice(docxPath, dir, pdfPath))
            {
                return File.ReadAllBytes(pdfPath);
            }

            throw new InvalidOperationException(
                "No se pudo convertir el acta a PDF. Instalá Microsoft Word o LibreOffice en esta PC (la API los usa en segundo plano).");
        }
        finally
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }

    private static bool TryWord(string docxPath, string pdfPath)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            object? app = null;
            object? docs = null;
            object? doc = null;
            try
            {
                var type = Type.GetTypeFromProgID("Word.Application");
                if (type is null)
                {
                    return;
                }

                app = Activator.CreateInstance(type);
                if (app is null)
                {
                    return;
                }

                Set(app, "Visible", false);
                Set(app, "DisplayAlerts", 0);
                docs = Get(app, "Documents");
                doc = Call(docs!, "Open", docxPath, false, true);
                Call(doc!, "SaveAs2", pdfPath, 17);
                Call(doc!, "Close", false);
                Call(app, "Quit", false);
            }
            catch (Exception ex)
            {
                error = ex;
                TryQuit(app);
            }
            finally
            {
                Release(doc);
                Release(docs);
                Release(app);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!thread.Join(TimeSpan.FromSeconds(90)))
        {
            return false;
        }

        return error is null && File.Exists(pdfPath);
    }

    private static bool TryLibreOffice(string docxPath, string outDir, string pdfPath)
    {
        var soffice = new[]
        {
            @"C:\Program Files\LibreOffice\program\soffice.exe",
            @"C:\Program Files (x86)\LibreOffice\program\soffice.exe"
        }.FirstOrDefault(File.Exists);

        if (soffice is null)
        {
            return false;
        }

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = soffice,
            Arguments = $"--headless --norestore --convert-to pdf --outdir \"{outDir}\" \"{docxPath}\"",
            CreateNoWindow = true,
            UseShellExecute = false
        });

        if (process is null)
        {
            return false;
        }

        if (!process.WaitForExit(90_000))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            return false;
        }

        var generated = Path.Combine(outDir, "acta.pdf");
        if (File.Exists(generated) && !string.Equals(generated, pdfPath, StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(generated, pdfPath, overwrite: true);
        }

        return File.Exists(pdfPath);
    }

    private static object? Get(object target, string name) =>
        target.GetType().InvokeMember(name, BindingFlags.GetProperty, null, target, null);

    private static void Set(object target, string name, object value) =>
        target.GetType().InvokeMember(name, BindingFlags.SetProperty, null, target, [value]);

    private static object? Call(object target, string name, params object[] args) =>
        target.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, target, args);

    private static void TryQuit(object? app)
    {
        if (app is null)
        {
            return;
        }

        try
        {
            Call(app, "Quit", false);
        }
        catch (COMException)
        {
        }
    }

    private static void Release(object? com)
    {
        if (com is not null && Marshal.IsComObject(com))
        {
            Marshal.FinalReleaseComObject(com);
        }
    }
}
