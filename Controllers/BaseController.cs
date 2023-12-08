using Barunson.DbContext;
using Barunson.FamilyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Barunson.FamilyWeb.Controllers
{
    /// <summary>
    /// 기본 컨트롤러
    /// </summary>
    public class BaseController : Controller
	{
		protected readonly ILogger _logger;
		protected readonly BarunsonContext _barunsonDb;
		protected readonly BarShopContext _barshopDb;
        protected readonly List<SiteInfo> _siteInfos;
        
		/// <summary>
        /// 참조 사이트 정보
        /// </summary>
        protected SiteInfo RefererSite { get; private set; }

		public BaseController(ILogger logger, BarunsonContext barunsonDb, BarShopContext barshopDb, List<SiteInfo> siteInfos)
		{
			_logger = logger;
			
			_barunsonDb = barunsonDb;
			_barshopDb = barshopDb;
			_siteInfos = siteInfos;
			RefererSite = _siteInfos.First(m => m.Name == "barunsoncard");
		}

		/// <summary>
		/// Action 메소드 시작전 호출
		/// 참조사이트 정보 설정
		/// </summary>
		/// <param name="context"></param>
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			ViewData["Title"] = "바른 ONE 회원가입";

			string? referSite;
			context.HttpContext.Request.Cookies.TryGetValue("REFERER_SITE", out referSite);
			if (!string.IsNullOrEmpty(referSite))
				SetRefererSite(referSite);
			else
				SetRefererSite("barunsoncard");

			base.OnActionExecuting(context);
		}
		/// <summary>
		/// 참조사이트 변경
		/// </summary>
		/// <param name="referSite"></param>
		protected void SetRefererSite(string referSite)
		{
			var lowSiteName = referSite.ToLower();
			if (_siteInfos.Exists(m => m.Name == lowSiteName))
			{
				RefererSite = _siteInfos.First(m => m.Name == lowSiteName);
				Response.Cookies.Append("REFERER_SITE", lowSiteName);
			}
		}

		/// <summary>
		/// 모바일 디바이스 접속 여부
		/// </summary>
		/// <returns></returns>
		protected bool IsMobile()
		{
			bool IsMobile = false;
			string Agent = Request.Headers["User-Agent"].ToString();
			string[] browser = { "iphone", "ipod", "ipad", "android", "blackberry", "windows ce", "nokia", "webos", "opera mini", "sonyericsson", "opera mobi", "iemobile", "windows phone" };
			
			for (int i = 0; i < browser.Length; i++)
			{
				if (Agent.ToLower().Contains(browser[i]))
				{
					IsMobile = true;
					break;
				}
			}
			return IsMobile;
		}
        #region 배너 정보
        protected async Task<List<BannerModel>> GetBanners()
		{
            var result = new List<BannerModel>();
            if (this.RefererSite.BannerId != null)
            {
                var now = DateTime.Now;
                int bannerId = this.RefererSite.BannerId.First();
                if (IsMobile())
                    bannerId = this.RefererSite.BannerId.Last();

                var query = from m in _barshopDb.S4_MD_Choice
                            where m.MD_SEQ == bannerId && m.VIEW_DIV == "Y"
                            && m.START_DATE <= now && m.END_DATE > now
                            orderby m.SORTING_NUM
                            select m;
                var dbItems = await query.ToListAsync();
                foreach (var item in dbItems)
                {
                    result.Add(new BannerModel
                    {
                        ImageUrl = new Uri(item.IMGFILE_PATH),
                        LinkUrl = string.IsNullOrEmpty(item.LINK_URL) ? null : new Uri(item.LINK_URL),
                        LinkTarget = string.IsNullOrEmpty(item.LINK_TARGET) ? "_blank" : item.LINK_TARGET.ToLower(),
                        CardText = item.CARD_TEXT
                    });
                }

            }
            return result;
        }
		#endregion

		#region 기타 공통 함수
		/// <summary>
		/// 우편 번호 분리
		/// </summary>
		/// <param name="postCode"></param>
		/// <returns></returns>
		protected (string, string) SplitPostCode(string postCode)
		{
			if (string.IsNullOrEmpty(postCode))
				return ("", "");
			else if (postCode.Contains('-'))
			{
				var s = postCode.Split(new char[] { '-' });
				return (s[0], s[1]);
			}
			else if (postCode.Length == 5)
				return (postCode.Substring(0, 3), postCode.Substring(3, 2));
			else
				return (postCode, "");

        }

        /// <summary>
        /// Bool 값을 Y or N로 변환,
        /// getNull 이 true일경우 값이 Null 일경우 Null 리턴
        /// </summary>
        /// <param name="value"></param>
        /// <param name="getNull"></param>
        /// <returns></returns>
        protected string? ConvertBoolToYN(bool? value, bool getNull = false)
		{
			if (getNull == true && value == null)
				return null;

			if (value.HasValue && value == true)
				return "Y";
			else
				return "N";
		}
		#endregion

	}
}
