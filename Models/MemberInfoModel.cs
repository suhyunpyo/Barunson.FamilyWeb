using System.ComponentModel.DataAnnotations;

namespace Barunson.FamilyWeb.Models
{
    /// <summary>
    /// 회원 정보 모델
    /// XML 또는 Json 출력
    /// </summary>
    public class MemberInfo
    {
        public bool Success { get; set; }= false;
        /// <summary>
        /// ID
        /// </summary>
        public string UserId { get; set; } = "";

        /// <summary>
        /// 이름
        /// </summary>
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";
        public string MobilePhone { get; set; } = "";
        public string PostCode { get; set; } = "";
        public string Address { get; set; } = "";
        public string AddressDetail { get; set; } = "";
        public bool IsIntergrationMemberYorN { get; set; } = false;
        public bool AllowSMS { get; set; } = false;
        public bool AllowEMail { get; set; } = false;

        /// <summary>
        /// DupInfo
        /// </summary>
        public string AuthCode { get; set; } = "";

    }
}
