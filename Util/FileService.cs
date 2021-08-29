using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace eVoucherAPI.Util
{
    public class FileService
    {
        public static Boolean MoveTempFile(string functionname, string filename, string tempfilename, IConfiguration _configuration)
        {
            try {
                string baseDirectory = _configuration.GetSection("appSettings:uploadPath").Value;
                string tempfolderPath = baseDirectory + _configuration.GetSection("appSettings:uploadTempPath").Value;
                string[] allowext = _configuration.GetSection("appSettings:allowExtension").Get<string[]>();                
                string[] allowfunction = _configuration.GetSection("appSettings:allowFunction").Get<string[]>();

                if (!allowfunction.Contains(functionname)) {
                    throw new Exception("Function Name Not Allow : " + functionname);  
                }
                // tempfilename = Encryption.Decrypt_String(tempfilename);
                string fullPath = "";
                string tempfullPath = tempfolderPath + tempfilename;
                fullPath = baseDirectory + _configuration.GetSection("appSettings:" + functionname).Value;
                
                if(fullPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path " + fullPath);

                if(tempfullPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path " + tempfullPath);

                if (!Directory.Exists(fullPath))
                {
                    throw new Exception("Folder path not found" + fullPath);
                }

                string ext = GetFileExtension(tempfilename).ToLower().TrimStart('.');
                if (!allowext.Contains(ext))
                    throw new Exception("Invalid File Extension: " + tempfilename);

                string filefullPath = fullPath + filename + '.' + ext;
                if(filefullPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path " + filefullPath);
                
                File.Move(tempfullPath, filefullPath);
                return true;
            }
            catch(Exception ex) {
                Globalfunction.WriteSystemLog("MoveTempFile: " + ex.Message);
                return false;
            }
        }

        public static Boolean CopyTempFileDir(string functionname, string dirname, string tempdirname, IConfiguration _configuration)
        {
            try {

                string baseDirectory = _configuration.GetSection("appSettings:uploadPath").Value;

                string tempfolderPath = baseDirectory + _configuration.GetSection("appSettings:uploadTempPath").Value;
                string[] allowext = _configuration.GetSection("appSettings:allowExtension").Get<string[]>();                
                string[] allowfunction = _configuration.GetSection("appSettings:allowFunction").Get<string[]>();

                if (!allowfunction.Contains(functionname)) {
                    throw new Exception("Function Name Not Allow : " + functionname);  
                }

                string fullPath = "";
                fullPath = baseDirectory + _configuration.GetSection("appSettings:" + functionname).Value;
                // tempdirname = Encryption.Decrypt_String(tempdirname);
                tempfolderPath = tempfolderPath + tempdirname;
                fullPath = fullPath + dirname;

                if(fullPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path " + fullPath);

                if(tempfolderPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path " + tempfolderPath);

                if (!Directory.Exists(tempfolderPath))
                {
                    throw new Exception("Invalid temp folder path " + tempfolderPath);
                }              

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                string[] files = System.IO.Directory.GetFiles(tempfolderPath);
                string fileName;
                string destFile;

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    string ext = Path.GetExtension(s).ToLower().TrimStart('.');
                    if (!allowext.Contains(ext))
                        throw new Exception("Invalid File Extension: " + s);

                    fileName = System.IO.Path.GetFileName(s);
                    destFile = fullPath + System.IO.Path.DirectorySeparatorChar + fileName;
                    System.IO.File.Move(s, destFile, true);
                }
                Directory.Delete(tempfolderPath);
                return true;
            }
            catch (Exception ex)
            {
                Globalfunction.WriteSystemLog("CopyTempFileDir: " + ex.Message);
                return false;
            }
        }

        public static Boolean DeleteFileNameOnly(string functionname, string filename, IConfiguration _configuration)
        {
            try {
                string fullPath = "";
                string baseDirectory = _configuration.GetSection("appSettings:uploadPath").Value;
                string[] allowext = _configuration.GetSection("appSettings:allowExtension").Get<string[]>();                
                string[] allowfunction = _configuration.GetSection("appSettings:allowFunction").Get<string[]>();

                if (!allowfunction.Contains(functionname)) {
                    throw new Exception("Function Name Not Allow : " + functionname);  
                }

                fullPath = baseDirectory + _configuration.GetSection("appSettings:" + functionname).Value;
                
                
                if(fullPath.IndexOf("..") >= 0 || filename.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path " + fullPath + "," + filename);

                string existingFile = Directory.EnumerateFiles(fullPath, filename + ".*").FirstOrDefault();
                
                if (!string.IsNullOrEmpty(existingFile)) {
                    string ext = Path.GetExtension(existingFile).ToLower().TrimStart('.');
                    if (!allowext.Contains(ext))
                        throw new Exception("Invalid File Extension: " + existingFile);

                    System.IO.File.Delete(existingFile);
                }
                else
                    throw new Exception("File not found under: " + fullPath + "/" + filename);
                
                return true;
            }
            catch (Exception ex) {
                Globalfunction.WriteSystemLog("DeleteFileNameOnly: " + ex.Message);
                return false;
            }
            
        }

        
        public static Boolean DeleteDir(string functionname, string dirname, IConfiguration _configuration)
        {
            try {
                string fullPath = "";
                
                string baseDirectory = _configuration.GetSection("appSettings:uploadPath").Value;
                string[] allowext = _configuration.GetSection("appSettings:allowExtension").Get<string[]>();                
                string[] allowfunction = _configuration.GetSection("appSettings:allowFunction").Get<string[]>();

                if (!allowfunction.Contains(functionname)) {
                    throw new Exception("Function Name Not Allow : " + functionname);  
                }

                fullPath = baseDirectory +  _configuration.GetSection("appSettings:" + functionname).Value + System.IO.Path.DirectorySeparatorChar + dirname;
                
                if(fullPath.IndexOf("..") >= 0)  //if found .. in the file name or path
                    throw new Exception("Invalid path: " + fullPath);

                //delete all files (with allow extension) under the folder
                IEnumerable<string> existingFile = Directory.EnumerateFiles(fullPath, "*.*");
                foreach (string currentFile in existingFile)
                {
                    string ext = Path.GetExtension(currentFile).ToLower().TrimStart('.');
                    if (!allowext.Contains(ext))
                        throw new Exception("Invalid File Extension: " + currentFile);
                    else
                        File.Delete(currentFile);
                }
                
                if(Directory.Exists(fullPath)) {
                    Directory.Delete(fullPath, false); //delete without recursive, if some file left, it will trigger error. 
                }
                else {
                    throw new Exception("Folder Path not found: " + fullPath);
                }
                
                return true;
            }
            catch (Exception ex) {
                Globalfunction.WriteSystemLog("DeleteDir: " + ex.Message);
                return false;
            }
            
        }

        public static string GetFileExtension(string FileName)
        {
            string uploadfilename = FileName;
            int lstindex = uploadfilename.LastIndexOf(".");
            string ext = uploadfilename.Substring(lstindex + 1, uploadfilename.Length - (lstindex + 1));
            return ext;
        }
    }
}	