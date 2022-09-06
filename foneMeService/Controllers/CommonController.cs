using foneMe.SL.Entities;
using foneMe.SL.Interface;
using foneMeService.Identity;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace foneMeService.Controllers
{
    public class CommonController : Controller
    {
        public ActionResult DisplayImageById(Guid fileId)
        {
            try
            {
                if (fileId == null || fileId == Guid.Empty) return null;
                foneMeEntities db = new foneMeEntities();
                var objUser = db.Users.Where(x => x.UserId == fileId)?.FirstOrDefault();
                if (objUser!=null)
                {
                    var userImageProfilePath = db.Profiles.Where(x => x.ShortName == "USRIMGPTH")?.FirstOrDefault()?.Name;
                    MemoryStream workStream = new MemoryStream();
                    string contentType = MimeMapping.GetMimeMapping(objUser.ImageURL);
                    var completeFilePath = userImageProfilePath + objUser.ImageURL;
                    byte[] byteInfo = System.IO.File.ReadAllBytes(completeFilePath);
                    workStream.Write(byteInfo, 0, byteInfo.Length);
                    workStream.Position = 0;
                    return new FileStreamResult(workStream, contentType);
                }
                else
                {
                    return null;
                }
              
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult DisplayImageBytes(Guid fileId)
        {
            try
            {
                if (fileId == null || fileId == Guid.Empty) return null;
                foneMeEntities db = new foneMeEntities();
                var objUser = db.Users.Where(x => x.UserId == fileId)?.FirstOrDefault();
                if (objUser != null)
                {
                    // var userImageProfilePath = db.Profiles.Where(x => x.ShortName == "USRIMGPTH")?.FirstOrDefault()?.Name;
                    MemoryStream workStream = new MemoryStream();
                    string contentType = MimeMapping.GetMimeMapping(objUser.ImageURL);
                    // var completeFilePath = userImageProfilePath + objUser.ImageURL;
                    // byte[] byteInfo = System.IO.File.ReadAllBytes(completeFilePath);
                    byte[] byteInfo = objUser.ImageBytes;
                    workStream.Write(byteInfo, 0, byteInfo.Length);
                    workStream.Position = 0;
                    return new FileStreamResult(workStream, contentType);
                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}