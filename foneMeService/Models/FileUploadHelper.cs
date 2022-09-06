using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace foneMeService.Models
{
    public class FileUploadHelper
    {

        public bool IsValidDOC(HttpPostedFileBase File)
        {
            long maxContent = 5242880; //5 MB
            string[] sAllowedExt = new string[] { ".pdf", ".doc", ".docx", ".jpg", ".png", ".jpeg" };

            if (!sAllowedExt.Contains(File.FileName.ToLower().Substring(File.FileName.LastIndexOf('.'))))
            {
                return false;
            }
            if (File.ContentLength > maxContent)
            {
                return false;
            }
            return true;
        }
        public List<string> SaveFileData(HttpPostedFile[] image, string filePath, string fileName)
        {

            // your configuration here...
            string virtualPath = null;
            string imageName = null;
            List<string> listImageNames = new List<string>();
            foreach (var item in image)
            {
                Guid miliSec = Guid.NewGuid();
                //imageName = Regex.Replace(fileName, @"\s", "_") + "_" + miliSec + Path.GetFileName(item.FileName);
                imageName = Regex.Replace(fileName, @"\s", "_") + "_" + miliSec + Path.GetExtension(Path.GetFileName(item.FileName));
                //imageName = orderId + "_" + vendorName + "_" + miliSec + Path.GetExtension(Path.GetFileName(image.FileName));
                virtualPath = filePath + imageName;
                listImageNames.Add(imageName);
                item.SaveAs(virtualPath);
            }

            return listImageNames;
        }
        public List<string> SavePrescriptionFileData(HttpPostedFile[] image, string filePath, string fileName)
        {

            // your configuration here...
            Guid miliSec = Guid.NewGuid();
            string virtualPath = null;
            string imageName = null;
            List<string> listImageNames = new List<string>();


            foreach (var item in image)
            {
                //imageName = Regex.Replace(fileName, @"\s", "_") + "_" + miliSec + Path.GetFileName(item.FileName);
                imageName = Regex.Replace(fileName, @"\s", "_") + "_" + miliSec + Path.GetExtension(Path.GetFileName(item.FileName));
                //imageName = orderId + "_" + vendorName + "_" + miliSec + Path.GetExtension(Path.GetFileName(image.FileName));
                virtualPath = filePath + imageName;
                listImageNames.Add(imageName);
                item.SaveAs(virtualPath);
            }

            return listImageNames;
        }
        public string SaveImagingData(HttpPostedFile image, string fileDirectory, string fileName)
        {

            // your configuration here...
            //string miliSec = DateTime.Now.Ticks.ToString();
            Guid miliSec = Guid.NewGuid();
            string virtualPath = null;
            string imageName = null;

            if (image != null && image.ContentLength > 0)
            {
                imageName = Regex.Replace(fileName, @"\s", "_") + "_" + miliSec + Path.GetExtension(Path.GetFileName(image.FileName));
                //imageName = orderId + "_" + vendorName + "_" + miliSec + Path.GetExtension(Path.GetFileName(image.FileName));
                virtualPath = fileDirectory + imageName;
                image.SaveAs(virtualPath);
            }
            return imageName;
        }

        public string SaveByteArrayImage(byte[] imageByteArray, string fileDirectory, string fileName)
        {

            Guid miliSec = Guid.NewGuid();
            string virtualPath = null;
            string imageName = null;
            if (imageByteArray != null && imageByteArray.Length > 0)
            {
                imageName = Regex.Replace(fileName, @"\s", "_") + "_" + miliSec + Path.GetExtension(Path.GetFileName(fileName));
                virtualPath = fileDirectory + imageName;
                File.WriteAllBytes(virtualPath, imageByteArray);
            }
            
            return imageName;
        }
    }
}