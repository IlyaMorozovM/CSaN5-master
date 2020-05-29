using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using System.Text.Json;

namespace CSaN5.Controllers
{
    [ApiController]
    [Route("[controller]/path/to")]
    public class FileStorageController : ControllerBase
    {
        private readonly ILogger<FileStorageController> logger;
        private string root = @"D:\MyFiles";

        public FileStorageController(ILogger<FileStorageController> logger)
        {
            this.logger = logger;
        }

        private bool isFile(string mayBeFile)
        {
            try
            {
                if (mayBeFile == null)
                    return false;
                int startIndex = mayBeFile.LastIndexOf("/") + 1;
                string substr = mayBeFile.Substring(startIndex, mayBeFile.Length - startIndex);
                if ((substr.Contains(".")) && (substr.IndexOf(".") == substr.LastIndexOf(".")))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }


        [HttpGet()]
        [HttpGet("{*filename}")]
        public ActionResult GetFile(string filename)
        {
            if (isFile(filename))
            {
                try
                {
                    string fullpath = root + @"\" + filename;
                    FileStream fStream = new FileStream(fullpath, FileMode.Open);
                    return File(fStream, "application/unknown", filename);
                }
                catch 
                { 
                    return BadRequest(); 
                }
            }
            else
            {
                string directoryname = filename;
                try
                {
                    IReadOnlyCollection<string> files = FileSystem.GetFiles(root + @"\" + directoryname);
                    IReadOnlyCollection<string> directories = FileSystem.GetDirectories(root + @"\" + directoryname);
                    List<string> content = new List<string>(directories);
                    content.AddRange(files);
                    return new JsonResult(content, new JsonSerializerOptions { });
                }
                catch 
                { 
                    return BadRequest(); 
                }

            }
        }

        [HttpHead("{*filename}")]
        public ActionResult GetFileInfo(string filename)
        {
            try
            {
                string fullpath = root + @"\" + filename;
                string fileInfo = FileSystem.GetFileInfo(fullpath).ToString();
                FileStream fStream = new FileStream(fullpath, FileMode.Open);
                Response.Headers.Add("FullName", fileInfo);
                return Ok();
            }
            catch 
            { 
                return NotFound(); 
            }
        }

        [HttpDelete("{*filename}")]
        public ActionResult DeleteFile(string filename)
        {
            try
            {
                if (isFile(filename))
                {
                    FileSystem.DeleteFile(root + @"\" + filename);
                }
                else
                {
                    FileSystem.DeleteDirectory(root + @"\" + filename,DeleteDirectoryOption.DeleteAllContents);
                }
                return Ok("Успешно удалено!");
            }
            catch 
            { 
                return NotFound(); 
            }
        }

        [HttpPut("{*filename}")]
        public ActionResult Put(IFormFileCollection uploads, string filename)
        {
            if (uploads.Count == 1)
            {
                return TryPut(uploads, filename);
            }
            else
            {
                return BadRequest();
            }
        }

        private ActionResult TryPut(IFormFileCollection uploads, string filename)
        {
            string path = uploads[0].FileName;
            try
            {
                using (var fileStream = new FileStream(root + @"/" + filename, FileMode.Create))
                {
                    uploads[0].CopyTo(fileStream);
                }
                return Ok("Успешно перезаписано!");
            }
            catch { return NotFound(); }
        }
    }
}
