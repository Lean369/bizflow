using System;
using System.IO;
using System.Net;

public class SimpleHttpServer
{
    private HttpListener _listener;

    public SimpleHttpServer(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Servidor HTTP iniciado...");

        while (true)
        {
            var context = _listener.GetContext();
            ProcessRequest(context);
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // Construir la ruta al archivo solicitado
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FrontEnd/browser", request.Url.AbsolutePath.TrimStart('/'));

        // Si el archivo no existe, servir index.html (para rutas de Angular)
        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FrontEnd/browser", "index.html");
        }

        // Leer el archivo
        byte[] buffer = File.ReadAllBytes(filePath);

        // Determinar el tipo MIME basado en la extensión del archivo
        string mimeType = GetMimeType(filePath);

        // Configurar los encabezados de respuesta
        response.ContentType = mimeType;
        response.ContentLength64 = buffer.Length;

        // Enviar el archivo al cliente
        using (var output = response.OutputStream)
        {
            output.Write(buffer, 0, buffer.Length);
        }
    }

    private string GetMimeType(string filePath)
    {
        // Obtener la extensión del archivo
        string extension = Path.GetExtension(filePath).ToLower();

        // Mapear las extensiones a tipos MIME
        switch (extension)
        {
            case ".html":
                return "text/html";
            case ".css":
                return "text/css";
            case ".js":
                return "application/javascript";
            case ".json":
                return "application/json";
            case ".png":
                return "image/png";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".svg":
                return "image/svg+xml";
            default:
                return "application/octet-stream"; // Tipo genérico
        }
    }
}