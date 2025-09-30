using Microsoft.AspNetCore.Components.Forms;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Sam530.Services
{
    public interface IUploadValidatorService
    {
        Task<(bool IsValid, string Message)> ValidateAsync(IBrowserFile file, IEnumerable<string> requiredFiles);
    }

    public class UploadValidatorService : IUploadValidatorService
    {
        public async Task<(bool IsValid, string Message)> ValidateAsync(
        IBrowserFile file,
        IEnumerable<string> requiredFiles)
        {
            var tempPath = Path.GetTempFileName();

            try
            {
                // Guardamos el archivo .gz en temp
                await using (var fs = File.Create(tempPath))
                {
                    await file.OpenReadStream(maxAllowedSize: 200 * 1024 * 1024).CopyToAsync(fs);
                }

                using var stream = File.OpenRead(tempPath);
                using var gzArchive = ArchiveFactory.Open(stream);

                // El gz debería contener un único tar
                var tarEntry = gzArchive.Entries.FirstOrDefault(e => !e.IsDirectory);
                if (tarEntry == null)
                {
                    return (false, "❌ El archivo .gz no contiene ningún .tar.");
                }

                using var tarStream = new MemoryStream();
                tarEntry.WriteTo(tarStream);
                tarStream.Position = 0;

                using var tarArchive = ArchiveFactory.Open(tarStream);

                var foundFiles = new HashSet<string>(
                    tarArchive.Entries
                              .Where(e => !e.IsDirectory)
                              .Select(e => e.Key!.Replace("\\", "/")),
                    StringComparer.OrdinalIgnoreCase);

                var missing = requiredFiles.Where(req => !foundFiles.Contains(req)).ToList();

                if (missing.Count == 0)
                {
                    return (true, "✅ Archivo válido: contiene todos los ficheros requeridos.");
                }
                else
                {
                    return (false, $"❌ Faltan los siguientes ficheros: {string.Join(", ", missing)}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al validar: {ex.Message}");
            }
            finally
            {
                try { File.Delete(tempPath); } catch { }
            }
        }
    }
}
