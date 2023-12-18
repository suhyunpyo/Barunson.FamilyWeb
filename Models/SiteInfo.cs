namespace Barunson.FamilyWeb.Models
{
	/// <summary>
	/// 통합 회원 가입 사이트 모델
	/// appsettings 구성 파일에 정의
	/// </summary>
	public class SiteInfo
	{
		/// <summary>
		/// 사이트 명(영문)
		/// </summary>
		public string Name { get; set; } = string.Empty;
		/// <summary>
		/// 사이트 명(한글)
		/// </summary>
		public string Brand { get; set; } = string.Empty;
		/// <summary>
		/// 대표 전화번호
		/// </summary>
		public string CallBack { get; set; } = string.Empty;
		/// <summary>
		/// 회사 고유 번호
		/// </summary>
		public int CompaySeq { get; set; }
		/// <summary>
		/// 사이트 코드 2자리 문자
		/// </summary>
		public string SiteGubun { get; set; } = string.Empty;

		/// <summary>
		/// PC 사이트 대표 URL
		/// </summary>
		public Uri SiteUrl { get; set; } = new Uri("https://www.barunsoncard.com");
		/// <summary>
		/// PC 사이트 로그인 URL
		/// </summary>
        public Uri? SignInUrl { get; set; } = new Uri("https://www.barunsoncard.com/member/login.asp");
		/// <summary>
		/// PC 사이트 비번찾기 URL
		/// </summary>
		public Uri? FindPWUrl { get; set; } = new Uri("https://www.barunsoncard.com/member/find_pw.asp");

        /// <summary>
        /// MO 사이트 대표 URL
        /// </summary>
        public Uri MobileSiteUrl { get; set; } = new Uri("https://m.barunsoncard.com");
        /// <summary>
        /// MO사이트 로그인 URL
        /// </summary>
        public Uri? MobileSignInUrl { get; set; } = new Uri("https://m.barunsoncard.com/member/login.asp");
        /// <summary>
        /// MO사이트 비번찾기 URL
        /// </summary>
        public Uri? MobileFindPWUrl { get; set; } = new Uri("https://m.barunsoncard.com/member/find_pw.asp");

		/// <summary>
		/// 사이트 로고 이미지 URL
		/// </summary>
		public Uri LogUrl { get; set; } = new Uri("https://static.barunsoncard.com/barunnfamily/logo/barunsoncard.svg");
		/// <summary>
		/// 패밀리 사이트 베너 노출 여부
		/// </summary>
		public bool IsShow { get; set; } = true;

		/// <summary>
		/// 배너 표시. 배열 2자리로 고정, 0: PC용, 1: 모바일용
		/// </summary>
		public int[]? BannerId { get; set; } = null;
		/// <summary>
		/// 사이트 설명
		/// </summary>
		public string? Description {  get; set; }

	}
}
