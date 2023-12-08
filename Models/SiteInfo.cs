namespace Barunson.FamilyWeb.Models
{
	public class SiteInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Brand { get; set; } = string.Empty;
		public string CallBack { get; set; } = string.Empty;
		public int CompaySeq { get; set; }
		public string SiteGubun { get; set; } = string.Empty;

		public Uri SiteUrl { get; set; } = new Uri("https://www.barunsoncard.com");
        public Uri? SignInUrl { get; set; } = new Uri("https://www.barunsoncard.com/member/login.asp");
		public Uri? FindPWUrl { get; set; } = new Uri("https://www.barunsoncard.com/member/find_pw.asp");

		public Uri MobileSiteUrl { get; set; } = new Uri("https://m.barunsoncard.com");
        public Uri? MobileSignInUrl { get; set; } = new Uri("https://m.barunsoncard.com/member/login.asp");
		public Uri? MobileFindPWUrl { get; set; } = new Uri("https://m.barunsoncard.com/member/find_pw.asp");

		public Uri LogUrl { get; set; } = new Uri("https://static.barunsoncard.com/barunnfamily/logo/barunsoncard.svg");
		public bool IsShow { get; set; } = true;

		/// <summary>
		/// 배너 표시. 배열 2자리로 고정, 0: PC용, 1: 모바일용
		/// </summary>
		public int[]? BannerId { get; set; } = null;

		public string? Description {  get; set; }

	}
}
