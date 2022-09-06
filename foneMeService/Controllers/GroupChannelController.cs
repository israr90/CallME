using System;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using foneMe.DAL;
using foneMe.SL.Interface;
using foneMe.ViewModels.Model;

namespace foneMeService.Controllers
{
    public class GroupChannelController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// GroupChannelController
        /// </summary>
        /// <param name="???"></param>
        public GroupChannelController()
        {
            var connectionStringName = ConfigurationManager.AppSettings["cs:connectionStringName"];
            _unitOfWork = new UnitOfWork(connectionStringName);
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult Index(string name)
        {
            ViewBag.ChannelName = name;
            var group = GetGroupById(name) ?? GetGroupByDeepLink(name);
            return View(group);
        }

        /// <summary>
        /// Preview Channel
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult PreviewChannel(string name)
        {
            ViewBag.ChannelName = name;
            var group = GetGroupById(name) ?? GetGroupByDeepLink(name);
            return View(group);
        }

        private GroupDetailsViewModel GetGroupByDeepLink(string name)
        {
            var model = new GroupDetailsViewModel
            {
                DeepLink = GetCurrentUrl(name),
            };
            
            return _unitOfWork.GroupChannelRepository.GetGroupByDeepLink(model);
        }

        private GroupDetailsViewModel GetGroupById(string name)
        {
            var model = new GroupDetailsViewModel
            {
                GroupID = name,
            };
            var groups = _unitOfWork.GroupChannelRepository.GetSingleGroup(model);
            if (groups == null || !groups.Any())
            {
                return null;
            }

            return groups.FirstOrDefault();
        }

        private string GetCurrentUrl(string name)
        {
            var rootUrlConfig = ConfigurationManager.AppSettings["RootUrl"];
            if (!string.IsNullOrEmpty(rootUrlConfig))
            {
                return $"{rootUrlConfig}/g/{name}";
            }
            
            if (Request.Url != null)
            {
                return Request.Url.GetLeftPart(UriPartial.Authority) + $"/g/{name}";
            }

            return "https://fone.me/g/" + name;
        }
    }
}