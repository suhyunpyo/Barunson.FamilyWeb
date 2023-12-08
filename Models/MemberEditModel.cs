using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Barunson.FamilyWeb.Models
{
    public static class MemberViewType
    {
        public static string Type1 { get; } = "";
        public static string Type2 { get; } = "Type2";
    }
    /// <summary>
    /// 회원 등록, 수정 모델
    /// </summary>
    public class MemberEditModel
    {
        [ValidateNever]
        public string ViewType { get; set; } = MemberViewType.Type1;

        [ValidateNever]
        public string ValidMessage { get; set; } = "";

        [ValidateNever]
        public SiteInfo RefererSite { get; set; }

        /// <summary>
        /// 개인 식별 인증키
        /// </summary>
        public Guid? CertId { get; set; }


        #region 기본 정보
        /// <summary>
        /// ID
        /// </summary>
        [Required(ErrorMessage = "사용하실 아이디를 입력해주세요.")]
        [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9_.\-]+$", ErrorMessage = "공백 없는 영문, 숫자 포함 6~16자만 입력 가능합니다.")]
        [Display(Name = "아이디", Prompt = "공백 없는 영문, 숫자 포함 6~16자")]
        [StringLength(16, MinimumLength = 6, ErrorMessage = "{0}는 {2}~{1}문자여야 합니다. ")]
        [Remote(action: "CheckUserId", controller: "Member", ErrorMessage = "이미 사용중인 아이디 입니다.")]
        public string UserId { get; set; }


        /// <summary>
        /// 비번
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "비밀번호", Prompt = "영문으로 시작하는 6~16자의 영문+숫자 조합")]
        [Required(ErrorMessage = "비밀번호를 입력해주세요")]
        [StringLength(16, MinimumLength = 6, ErrorMessage = "{0}는 {2}~{1}문자여야 합니다. ")]
        [RegularExpression(@"^[A-Za-z](?=.*[A-Za-z0-9_.!@#$%^&*])(?=.*\d).{5,}$", ErrorMessage = "비밀번호는 영문자로 시작하는 6~16자의 영문, 숫자, _.!@#$%^&*의 특수문자의 조합이여야 합니다.")]
        public string Password { get; set; }

        /// <summary>
        /// 비번확인
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "비밀번호 확인", Prompt = "비밀번호를 한번 더 입력해주세요")]
        [Compare(nameof(Password), ErrorMessage = "입력하신 비밀번호와 일치하지 않습니다.")]
        public string? PasswordConfirm { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        [StringLength(25)]
        [Required]
        [Display(Name = "이 름", Prompt = "휴대폰 인증 시 자동입력됩니다")]
        public string Name { get; set; }

        /// <summary>
        /// 생년월일
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "생년월일")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// 양력,음력
        /// </summary>
        [Required]
        [Display(Name = "양력/음력")]
        public string SolarOrLunar { get; set; } = "S";

        /// <summary>
        /// 우편번호
        /// </summary>
        [Display(Name = "주소")]
        [Required(ErrorMessage = "우편번호 및 주소를 입력해 주세요")]
        public string PostCode { get; set; }
        /// <summary>
        /// 주소
        /// </summary>
        [Display(Name = "주소", Prompt = "동,면,읍 이상")]
        public string? Address { get; set; }
        /// <summary>
        /// 주소상세
        /// </summary>
        [Display(Name = "주소", Prompt = "나머지 주소(직접입력)")]
        [StringLength(50)]
        [Required(ErrorMessage = "상세 주소를 입력해 주세요")]
        public string AddressDetail { get; set; }

        /// <summary>
        /// 유선전화 국번
        /// </summary>
        [Display(Name = "전화번호", Prompt = "국번을 넣어주세요")]
        public string TelNo1 { get; set; } = "02";
        /// <summary>
        /// 유선전화 
        /// </summary>
        [StringLength(4)]
        [RegularExpression(@"^(\d{3,4})$", ErrorMessage = "전화번호 앞자리는 숫자 3~4자리만 입력 가능합니다.")]
        public string? TelNo2 { get; set; }
        /// <summary>
        /// 유선전화
        /// </summary>
        [StringLength(4)]
        [RegularExpression(@"^(\d{4})$", ErrorMessage = "전화번호 뒷자리는 숫자 4자리만 입력 가능합니다.")]
        public string? TelNo3 { get; set; }

        /// <summary>
        /// 휴대폰번호
        /// </summary>
        [Display(Name = "휴대폰번호", Prompt = "국번을 넣어주세요")]
        public string MoTelNo1 { get; set; } = "010";
        /// <summary>
        /// 휴대폰
        /// </summary>
        [StringLength(4)]
        [RegularExpression(@"^(\d{3,4})$", ErrorMessage = "휴대폰번호 앞자리는 숫자 3~4자리만 입력 가능합니다.")]
        public string? MoTelNo2 { get; set; }
        /// <summary>
        /// 휴대폰
        /// </summary>
        [StringLength(4)]
        [RegularExpression(@"^(\d{4})$", ErrorMessage = "휴대폰번호 뒷자리는 숫자 4자리만 입력 가능합니다.")]
        public string? MoTelNo3 { get; set; }

        /// <summary>
        /// 이메일
        /// </summary>
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "이메일 주소가 잘못되었습니다.")]
        [Required(ErrorMessage = "이메일 주소를 입력해 주세요.")]
        [Display(Name = "이메일", Prompt = "이메일을 넣어주세요")]
        [StringLength(100)]
        public string Email { get; set; }
        /// <summary>
        /// 웨딩일
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "예식일", Prompt = "예식일을 넣어주세요")]
        [Required(ErrorMessage = "예식일을 입력해 주세요")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime WeddingDay { get; set; }

        /// <summary>
        /// 예식장 타입
        /// </summary>
        [Display(Name = "예식장")]
        [Required(ErrorMessage = "예식장을 선택해 주세요")]
        public string WeddingHallType { get; set; } = "W";
        /// <summary>
        /// 예식 장소
        /// </summary>
        [Display(Name = "예식 장소", Prompt = "Ex) 루이비스 다산홀")]
        [StringLength(50)]
        public string? WeddingHallName { get; set; }

        #endregion

        #region 기타 선택목록등 선언데이터
                
        /// <summary>
        /// 양력 음력 선택박스
        /// </summary>
        public IEnumerable<SelectListItem> SolarOrLunarSelect
        {
            get
            {
                var list = new List<SelectListItem>
                {
                    new SelectListItem{ Text = "양력", Value = "S" },
                    new SelectListItem{ Text = "음력", Value = "L" }
                };
                return new SelectList(list, "Value", "Text");
            }
        }
        /// <summary>
        /// 유선전화 국번
        /// </summary>
        public IEnumerable<SelectListItem> TelNo1Select
        {
            get
            {
                var list = new List<string> { "02", "031", "032", "033", "041", "042", "043", "044", "051", "052", "053", "054"
                                            ,"055","061","062","063","064","070","080","0130","0502","0504","0505","0506","0507"};
                return new SelectList(list.Select(x => new SelectListItem { Text = x, Value = x }), "Value", "Text");
            }
        }
        /// <summary>
        /// 휴대폰 국번
        /// </summary>
        public IEnumerable<SelectListItem> MoTelNo1Select
        {
            get
            {
                var list = new List<string> { "010", "011", "016", "017", "018", "019"};
                return new SelectList(list.Select(x => new SelectListItem { Text = x, Value = x }), "Value", "Text");
            }
        }
        /// <summary>
        /// 이메일, 사용여부 검토 후 제거... 
        /// </summary>
        public IEnumerable<SelectListItem> EmailFooterSelect
        {
            get
            {
                var list = new List<string> { "직접입력", "gmail.com", "naver.com", "daum.net", "nate.com", "hanmail.net", "hotmail.com" };
                return new SelectList(list.Select(x => new SelectListItem { Text = x, Value = x }), "Value", "Text");
            }
        }
        /// <summary>
        /// 웨딩홀 타입
        /// </summary>
        public IEnumerable<SelectListItem> WeddingHallTypeSelect
        {
            get
            {
                var list = new List<SelectListItem> 
                { 
                    new SelectListItem{ Text = "예식홀", Value = "W" },
                    new SelectListItem{ Text = "호텔", Value = "H" },
                    new SelectListItem{ Text = "종교장소", Value = "C" },
                    new SelectListItem{ Text = "군관련기업", Value = "M" },
                    new SelectListItem{ Text = "기타", Value = "E" }
                };
                return new SelectList(list, "Value", "Text");
            }
        }
        
        #endregion
    }

    /// <summary>
    /// 신규 가입
    /// </summary>
    public class MemberRegModel: MemberEditModel
    {
 
        #region 약관 동의 정보
        /// <summary>
        /// 약관 동의 여부 
        /// </summary>
        public PolicyAgreeModel PolicyAgree { get; set; } = new PolicyAgreeModel();

        /// <summary>
        /// 약관 내용 목록
        /// </summary>
        [ValidateNever] 
        public List<PolicyModel> Policies { get; set; }
        #endregion

        #region 배너 표시 정보

        /// <summary>
        /// 배너 목록
        /// </summary>
        [ValidateNever] 
        public List<BannerModel> Banners { get; set; }

        #endregion
    }

    /// <summary>
    /// 회원 수정
    /// </summary>
    public class MemberModModel : MemberEditModel
    {
        
        [ValidateNever]
        [Display(Name = "바른 ONE 가입일")]
        public DateTime RegisterDate { get; set; }

        [Display(Name = "SMS수신설정")]
        public bool CheckSMS { get; set; } = false;

        [Display(Name = "뉴스레터 및 안내메일 수신설정")]
        public bool CheckEMail { get; set; } = false;

        public string? ReturnUrl { get; set; }

        public bool SaveSuccess { get; set; } = false;
    }

    /// <summary>
    /// 화원 가입 완료 페이지 모델
    /// </summary>
    public class MemberCompleteModel : ResponseBaseModel
    {
        public MemberCompleteModel(SiteInfo siteInfo) : base(siteInfo)
        {
        }
        public bool IsMobile { get; set; } = false;

        public string Message { get; set; } = "";
        public string UserId { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// 회원 탈퇴 모델
    /// </summary>
    public class MemberSecessionModel
    {

        [ValidateNever]
        public SiteInfo RefererSite { get; set; }
        public bool IsMobile { get; set; } = false;
        /// <summary>
        /// 개인 식별 인증키
        /// </summary>
        public Guid? CertId { get; set; }

        /// <summary>
        /// 탈퇴 사유
        /// </summary>
        public List<SelectListItem> SecessionCause { get; set; } = SecessionCauseSelect().ToList();

        /// <summary>
        /// 기타 사유
        /// varchar(500)으로 되어 있어 유니코드 대응 자릿수 설정
        /// </summary>
        [StringLength(240)] 
        public string? EtcComment { get; set; }

        public string? ReturnUrl { get; set; }

        [ValidateNever] 
        public string UserId { get; set; }
              

        [ValidateNever]
        public string ValidMessage { get; set; } = "";

        /// <summary>
        /// 탈퇴 사유 목록
        /// </summary>
        public static IEnumerable<SelectListItem> SecessionCauseSelect()
        {
            
            var list = new List<SelectListItem>
            {
                new SelectListItem{ Text = "배송불만", Value = "117001" },
                new SelectListItem{ Text = "사이트 이용 불만", Value = "117002" },
                new SelectListItem{ Text = "상품의 다양성 / 가격 품질 불만", Value = "117003" },
                new SelectListItem{ Text = "개인정보 유출우려", Value = "117004" },
                new SelectListItem{ Text = "교환 / 환불 불만", Value = "117005" },
                new SelectListItem{ Text = "실질적인 혜택 부족", Value = "117006" },
                new SelectListItem{ Text = "기타사유", Value = "117007" }
            };
            return new SelectList(list, "Value", "Text");
            
        }
    }
    public class MemberSecessionComplete : ResponseBaseModel
    {
        public MemberSecessionComplete(SiteInfo siteInfo) : base(siteInfo)
        {
        }
        public string? ReturnUrl { get; set; }
    }
}
