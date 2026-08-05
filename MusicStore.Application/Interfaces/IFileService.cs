using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadProductImageAsync(IFormFile file, int productId);


        Task DeleteFileAsync(string filePath);


        Task DeleteProductFolderAsync(int productId);
    }
}
