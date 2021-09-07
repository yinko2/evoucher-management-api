using System;
using System.IO;
using System.Linq;
using eVoucherAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using eVoucherAPI.Util;

namespace eVoucherAPI.Controllers
{
    [Route("api/[controller]")]
    public class FileServiceController : BaseController
    {
        public FileServiceController(IRepositoryWrapper repositoryWrapper, IConfiguration configuration) : base(repositoryWrapper, configuration)
        {
        }

        [HttpGet("evoucherphoto/{name}", Name = "evoucherphoto")]
        public string eVoucherPhoto(string name)
        {
            try
            {
                int userId = int.Parse(_tokenData.UserID);
                string baseDirectory = _configuration.GetSection("appSettings:uploadPath").Value;
                string[] allowext = _configuration.GetSection("appSettings:allowExtension").Get<string[]>();
                string uploadDirectory = _configuration.GetSection("appSettings:eVoucherPhoto").Value;

                string folderPath = "";
                
                if (uploadDirectory != null)
                {
                    folderPath = baseDirectory + uploadDirectory;
                }
                else
                {
                    throw new Exception("Invalid Function Path " + uploadDirectory); 
                }


                if(folderPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path " + folderPath);

                // find extension by file name
                string existingFile = Directory.EnumerateFiles(folderPath, name.ToString() + ".*").FirstOrDefault();
                string extension = "";
                if (!string.IsNullOrEmpty(existingFile))
                {
                    extension = Path.GetExtension(existingFile).ToLower().TrimStart('.');
                    if (allowext.Contains(extension))
                    {
                        byte[] m_Bytes = ReadToEnd(new FileStream(existingFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                        Response.ContentType = "application/json";
                        string imageBase64Data = Convert.ToBase64String(m_Bytes);
                        string imageDataURL = string.Format("data:image/" + extension + ";base64,{0}", imageBase64Data);
                        return imageDataURL;
                    }
                    else
                        throw new Exception("Invalid File Extension " + extension);
                }
                else {
                    Globalfunction.WriteSystemLog("File Not Found " + folderPath);
                    return "";
                }
            }
            catch (Exception ex)
            {
                Globalfunction.WriteSystemLog("DownLoadFile: " + ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Download File Error";
            }

        }

        [HttpPost("Upload/Temp")]
        public string FileUploadTemp(string enFile)
        {
            //Task.Delay(2000).Wait();   //to test loading icon
            Response.ContentType = "application/json";
            try
            {
                var files = Request.Form.Files;
                if (files.Count > 0)
                {
                    // Save the file
                    var file = files[0];
                    if (file.Length > 0)
                    {
                        enFile = Encryption.DecryptClient_String(enFile);  //client side decryption
                        if(enFile != file.FileName)
                            throw new Exception("Invalid file name " + enFile);

                        string uploadfilename = enFile;

                        int lstindex = uploadfilename.LastIndexOf(".");
                        string ext = uploadfilename.Substring(lstindex + 1, uploadfilename.Length - (lstindex + 1)).ToLower();

                        string fullPath = "";
                        string baseDirectory = _configuration.GetSection("appSettings:uploadPath").Value;
                        string folderPath = _configuration.GetSection("appSettings:uploadTempPath").Value;
                        string[] allowext = _configuration.GetSection("appSettings:allowExtension").Get<string[]>();

                        if (!allowext.Contains(ext))
                        {
                            throw new Exception("Invalid File Extension " + ext);
                        }

                        folderPath = baseDirectory + folderPath;
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }


                        string filename = Guid.NewGuid().ToString();
                        fullPath = folderPath + filename + "." + ext.ToLower();

                        if(fullPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                            throw new Exception("Invalid path " + fullPath);

                        using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
                        {
                            file.CopyTo(fileStream);
                        }
                        return filename + "." + ext.ToLower();
                    }
                    
                    throw new Exception("Invalid File.");
                }
                else
                    throw new Exception("No File.");

            }
            catch (Exception ex)
            {
                Globalfunction.WriteSystemLog("FileUploadTemp: " + ex.Message);
                Response.StatusCode = 400;
                return "Fail to Upload";
            }
        }

        [HttpPost("Upload/TempRemove")]
        public string FileUploadTempRemove(string fileNames)
        {
            Response.ContentType = "application/json";
            try
            {
                if(fileNames == null || fileNames == "")
                    throw new Exception("Invalid temp file");

                string uploadfilename = fileNames;
                int lstindex = uploadfilename.LastIndexOf(".");
                string ext = uploadfilename.Substring(lstindex + 1, uploadfilename.Length - (lstindex + 1)).ToLower();
                
                string fullPath = "";
                string baseDirectory = _configuration.GetSection("appSettings:uploadPath").Value;
                string folderPath = _configuration.GetSection("appSettings:uploadTempPath").Value;
                string[] allowext = _configuration.GetSection("appSettings:allowExtension").Get<string[]>();

                fullPath = baseDirectory + folderPath + fileNames;
                if(fullPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path " + fileNames);

                if (!allowext.Contains(ext))
                {
                    throw new Exception("Invalid File Extension " + fileNames);
                }

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    return "";
                }
                else
                {
                    throw new Exception("File not found " + fullPath);
                }
            }
            catch (Exception ex)
            {
                Globalfunction.WriteSystemLog("FileUploadTempRemove: " + ex.Message);
                Response.StatusCode = 400;
                return "Fail to Remove ";
            }
        }

        public static byte[] ReadToEnd(System.IO.Stream inputStream)
        {
            if (!inputStream.CanRead)
            {
                throw new ArgumentException();
            }

            // This is optional
            if (inputStream.CanSeek)
            {
                inputStream.Seek(0, SeekOrigin.Begin);
            }

            byte[] output = new byte[inputStream.Length];
            int bytesRead = inputStream.Read(output, 0, output.Length);
            inputStream.Dispose();//.Close();
            return output;
        }
    }
}