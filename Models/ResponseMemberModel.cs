namespace Barunson.FamilyWeb.Models
{
	/// <summary>
	/// 회원 가입 진입 뷰 모델
	/// </summary>
	public class ResponseMemberModel : ResponseBaseModel
	{
		public ResponseMemberModel(SiteInfo siteInfo) 
			: base(siteInfo)
		{ 
		}
		
		public Uri NiceCheckUrl { get; set; }
		
	}

	/// <summary>
	/// 인증완료후 개인정보 모델
	/// </summary>
	public class MemberCertificationModel
	{
		/// <summary>
		/// 인증수단
		/// M	휴대폰인증		
		/// C 카드본인확인
		/// X 공동인증서
		/// F 금융인증서
		/// S PASS인증서
		/// </summary>		
		public string Authtype { get; set; }
		/// <summary>
		/// 이름
		/// </summary>		
		public string Name { get; set; }
		/// <summary>
		/// 생년월일
		/// </summary>		
		public DateTime Birthdate { get; set; }
		/// <summary>
		/// 성별 0:여성, 1:남성
		/// </summary>		
		public string Gender { get; set; }
		/// <summary>
		/// 휴대폰 번호(휴대폰 인증 시)
		/// </summary>		
		public string MobileNo { get; set; }
		/// <summary>
		/// 개인 식별 코드(CI)
		/// </summary>		
		public string ci { get; set; }
		/// <summary>
		/// 개인 식별 코드(di)
		/// </summary>		
		public string di { get; set; }
        /// <summary>
        /// 내외국인 0:내국인, 1:외국인
        /// </summary>		
        public string nationalinfo { get; set; }

        /// <summary>
        /// 룰렛 이밴트, 없을수 있음
        /// </summary>
        public string? BizCode { get; set; }
		public string? EventCode { get; set; }
		public string? Key { get; set; }
	}
}
