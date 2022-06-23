using System.Text;
using Microsoft.AspNetCore.Mvc;
using dn32.transfer.Models;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.StaticFiles;

namespace dn32.transfer.Controllers;

public class HomeController : Controller
{
#if DEBUG
    private const string DiretorioDeUpload = "/Users/marcelo/Documents/dev/dn32/uploads/";
#else
    private const string DiretorioDeUpload = "/app/uploads";
#endif

    public IActionResult Index()
    {
        var html = @"
<form action='/' method='post' enctype='multipart/form-data'>
Chave: <input name='Chave' type='text'/>
Arquivo: <input type='file' name='Arquivo'/>
<input type='submit'/>
</form>";

        return Content(html, contentType: "text/html");
    }

    [HttpPost]
    public string Index(Model model)
    {
        if (model?.Arquivo == null) return "Arquivo veio vazio";
        if (model.Chave != "4lf0snk3wl5ovmbrcr4vs2rhzgjv5s") return "Chave inválida!";

        var nomeDoArquivo = ObterNomeUnicoDeArquivo(model.Arquivo.FileName);
        var filePath = Path.Combine(DiretorioDeUpload, nomeDoArquivo);
        var dirPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        model.Arquivo.CopyTo(new FileStream(filePath, FileMode.Create));
        var url = Request.GetDisplayUrl();
        var urlCompleta = $"{url}{nomeDoArquivo}";
        return urlCompleta;
    }

    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    [HttpGet("{pasta}/{nome}")]
    public IActionResult Obter(string pasta, string nome)
    {
        // No futuro guardar em memória quando tiverem muitas requisições
        var url = Path.Combine(DiretorioDeUpload, pasta, nome);
        var mime = ObterMime(url);
        var image = System.IO.File.OpenRead(url);
        return File(image, mime);
    }

    private static string ObterMime(string filePath)
    {
        const string defaultContentType = "application/octet-stream";
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = defaultContentType;
        }

        return contentType;
    }

    private static string ObterNomeUnicoDeArquivo(string fileName)
    {
        string nomeDoArquivo;
        do
        {
            var dir = DateTime.Now.ToString("yyyy-MM-dd");
            var extensao = Path.GetExtension(fileName);
            nomeDoArquivo = $"{dir}/{ObterString(10)}{extensao}";
        } while (System.IO.File.Exists(Path.Combine(DiretorioDeUpload, nomeDoArquivo)));

        return nomeDoArquivo;
    }

    private static string ObterString(int length)
    {
        Random random = new();
        const string chars = "abcdefghijklmnopqrstuvwxyz1234567890";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}