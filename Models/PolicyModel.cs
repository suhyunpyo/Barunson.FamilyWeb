using Barunson.FamilyWeb.Attribute;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Barunson.FamilyWeb.Models
{
    /// <summary>
    /// 약관 내용
    /// </summary>
    public class PolicyModel
	{

		public string PolicyDivision { get; set; }

        public string Title { get; set; }
		public string Contents { get; set; }
		public int Seq { get; set; }

        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
    }

	/// <summary>
	/// 동의 정보 
	/// </summary>
	public class PolicyAgreeModel
    {
        /// <summary>
        /// 모든 약관 동의 여부
        /// </summary>
        [Display(Name = "전체 동의")]
        public bool All { get; set; } = false;

        #region 필수

        [Display(Name = "만 14세 이상입니다.")]
        [MustbetrueAttribute(ErrorMessage = "만14세 이상 고객 가입에 동의하여 주십시오.")]
        public bool Age { get; set; } = false;

        [Display(Name = "이용약관 동의")]
        [MustbetrueAttribute(ErrorMessage = "회원이용약관에 동의하여 주십시오.")]
        public bool User { get; set; } = false;

        [Display(Name = "개인정보 수집 이용 동의")]
        [MustbetrueAttribute(ErrorMessage = "개인정보 수집이용에 동의하여 주십시오.")]
        public bool Essential { get; set; } = false;

        #endregion

        #region 선택 및 기타 이벤트,제휴 등

        [Display(Name = "이벤트 및 서비스안내 수신동의")]
        public bool SMSEMail { get; set; } = false;

        #endregion

        #region 기타 이벤트,제휴 등

        [Display(Name = "LG전자 멤버십 전체 이용 약관 동의")]
        public bool LGMembership { get; set; } = false;

        [Display(Name = "까사미아 멤버십 전체 이용약관 동의")]
        public bool CasamiaMembership { get; set; } = false;


        [Display(Name = "제3자 마케팅 활용동의")]
        public bool ThirdParty { get; set; } = false;

        [Display(Name = "라이프")]
        public bool ThirdPartyShinhan { get; set; } = false;
        public string ThirdPartyShinhanCode { get; } = "119006";

        [Display(Name = "통신")]
        public bool ThirdPartyTelecom { get; set; } = false;
        public string ThirdPartyTelecomCode { get; } = "119001";
        #endregion

    }

    /// <summary>
    /// 약관 히스토리 보기
    /// </summary>
    public class PolicyHistoryModel: PolicyModel
    {
        public SiteInfo RefererSite { get; set; }
        /// <summary>
        /// 번호 선택 박스
        /// </summary>
        public IEnumerable<SelectListItem>? SelectSeqs { get; set; }

    }
}
