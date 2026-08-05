using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MusicStore.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Infrastructure.Services
{
    public class FileService:IFileService
    {
        private readonly IWebHostEnvironment _environment;


        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }










        public async Task<string> UploadProductImageAsync(IFormFile file, int productId)
        {
            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "products",
                productId.ToString()
                );
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }



            var fileName=Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);


            var FilePath= Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(FilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            return $"/uploads/products/{productId}/{fileName}";

        }



        public Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(
                _environment.WebRootPath,
                filePath.TrimStart('/')
            );


            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }


            return Task.CompletedTask;
        }

        public Task DeleteProductFolderAsync(int productId)
        {
            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "products",
                productId.ToString()
            );


            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }


            return Task.CompletedTask;
        }
    }
}
