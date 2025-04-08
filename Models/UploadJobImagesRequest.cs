using Microsoft.AspNetCore.Http;

namespace NixersDB.Models
{
    public class UploadJobImageRequest
    {
        public IFormFile File { get; set; }

         public string FileName { get; set; }
        
    }
}

// to stop the swagger crash, as it does not support IFormFile