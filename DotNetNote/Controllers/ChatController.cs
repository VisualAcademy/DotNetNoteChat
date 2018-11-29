using DotNetNote.Data;
using DotNetNote.Services;
using DotNetNote.Services.Interfaces;
using DotNetNote.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;

namespace DotNetNote.Controllers
{
    public class ChatController : Controller
    {
        private IHostingEnvironment _environment;
        private readonly IStorageManager _storageManager;
        private readonly AppKeyConfig _appKeyConfig;
        private readonly IEmailManager _emailManager;
        private readonly ApplicationDbContext _dbContext;

        public ChatController(IHostingEnvironment environment, IStorageManager storageManager, IOptions<AppKeyConfig> appKeyConfig, IEmailManager emailManager, ApplicationDbContext dbContext)
        {
            _environment = environment;
            _storageManager = storageManager;
            _appKeyConfig = appKeyConfig.Value;
            _emailManager = emailManager;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Photo()
        {
            var chats = _dbContext.Chats.OrderByDescending(c => c.Id).ToList(); 

            return View(chats); 
        }

        [HttpPost]
        [Consumes("application/json", "multipart/form-data")]
        // files 매개변수 이름은 <input type="file" name="files" /> 
        public async Task<IActionResult> Post(ICollection<IFormFile> files)
        {
            if (!_appKeyConfig.AzureStorageEnable)
            {
                // 파일을 업로드할 폴더
                var uploadFolder = Path.Combine(_environment.WebRootPath, "files");

                string fileName = "";
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // 파일명 
                        fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"'));

                        using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }
                }

                string url = "/files/";

                return Ok(new { url = $"<img src=\"{url}/{fileName}\" />" }); 
            }
            else
            {
                byte[] byteArray;
                string newFileName = "";
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        using (var stream = new MemoryStream())
                        {
                            await formFile.CopyToAsync(stream);
                            byteArray = stream.ToArray();
                            var folderPath = _storageManager.GetFolderPath("Chat", "Photo", "Files");
                            newFileName = await _storageManager.UploadAsync(byteArray, formFile.FileName, folderPath, false);

                            if (!string.IsNullOrEmpty(newFileName))
                            {
                                // 데이터베이스에 파일 이름 저장 영역 
                            }
                        }
                    }
                }

                return Ok(new { url = $"<img src=\"/Chat/Download?fileName={newFileName}\" />" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            var folderPath = _storageManager.GetFolderPath("Chat", "Photo", "Files");
            var fileBytes = await _storageManager.DownloadAsync(fileName, folderPath);

            if (fileBytes == null)
            {
                return NotFound();
            }

            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
