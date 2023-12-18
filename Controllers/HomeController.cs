using Barunson.DbContext;
using Barunson.FamilyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Barunson.FamilyWeb.Controllers
{
    public class HomeController : BaseController
	{

        public HomeController(ILogger<HomeController> logger, BarunsonContext barunsonDb, BarShopContext barshopDb, List<SiteInfo> siteInfos, SiteConfig siteConfig) 
            : base(logger, barunsonDb, barshopDb, siteInfos, siteConfig)
        {
            
        }
		/// <summary>
		/// 통합회원 Home page
		/// DupInfo or certID 값이 query로 전달될경우 통합회원 전환필요한 회원이나, 구현하지 않음.  
		/// </summary>
		/// <param name="site_code"></param>
		/// <returns></returns>
		public IActionResult Index(string? DupInfo, string? CertID)
        {
            var model = new ResponseBaseModel(this.RefererSite);
			if (!string.IsNullOrEmpty(DupInfo))
				return RedirectToAction("SignInForExist", "Member", new { DupInfo = DupInfo });
			else if (!string.IsNullOrEmpty(CertID))
				return RedirectToAction("SignInForExist", "Member", new { CertID = CertID });

			return View(model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}