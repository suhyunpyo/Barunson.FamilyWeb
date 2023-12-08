using Barunson.DbContext;
using Barunson.DbContext.DbModels.BarShop;
using Barunson.FamilyWeb.Models;
using Barunson.FamilyWeb.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Barunson.FamilyWeb.Controllers
{
    public class MemberController : BaseController
	{
        private readonly IRouletteEventService _rouletteEventService;
        public MemberController(ILogger<MemberController> logger, BarunsonContext barunsonDb, BarShopContext barshopDb, List<SiteInfo> siteInfos, IRouletteEventService rouletteEventService)
			: base(logger, barunsonDb, barshopDb, siteInfos)
		{
            _rouletteEventService = rouletteEventService;
		}
		#region 회원 가입 진입 페이지

		/// <summary>
		/// 회원 가입
		/// </summary>
		/// <returns></returns>
		[Route("[controller]", Order = 0)]
		[Route("Member/Agreement.aspx", Order = 1)]
		[Route("Member/Agreement_New.aspx", Order = 2)]
		public IActionResult Index(string? site_code
			, string? biz_code, string? event_code, string? key)
		{
			if (site_code != null)
			{
				SetRefererSite(site_code);
			}

			var viewName = "Index";
			//모초, Gshop은 최소 동의만 
			if (this.RefererSite.SiteGubun == "GS" || this.RefererSite.SiteGubun == "BM")
				viewName = $"Index{MemberViewType.Type2}";

			var querys = new NiceRequestCallBackModel
			{
				NextAction = "Register",
				NextController = "Member",
				//룰렛 이벤트
				BizCode = biz_code,
				EventCode = event_code,
				Key = key
			};
			var model = new ResponseMemberModel(this.RefererSite)
			{
				NiceCheckUrl = new Uri(Url.ActionLink("Call", "NcieCheck", querys))
			};
			
			return View(viewName, model);
		}

		/// <summary>
		/// Gshop은 다른 경로로 들어옴
		/// </summary>
		/// <returns></returns>
		[Route("Member/GshopAgreement.aspx")]
		public IActionResult GshopAgreement()
        {
            return RedirectToAction("Index", new { site_code = "barunsongshop" });
        }
		#endregion

		#region 공통 내부 함수
		/// <summary>
		/// 나이스 인증 결과 임시 저장 데이터 읽기
		/// </summary>
		/// <param name="certId"></param>
		/// <returns></returns>
		private async Task<MemberCertificationModel> GetMemberCertification(Guid certId)
		{
            //인증 정보 읽기
            var query = from m in _barshopDb.User_Certification_Log
                        where m.CertID == certId.ToString() 
                        select m;
            var item = await query.FirstOrDefaultAsync();
            if (item == null)
                return null;

            var CertData = Base64Convert.Decoded(item.CertData);
            return JsonSerializer.Deserialize<MemberCertificationModel>(CertData);
        }

        /// <summary>
        /// 회원 수정등 회원 고유 정보 임시 저장 데이터 읽기
        /// </summary>
        /// <param name="certId"></param>
        /// <returns></returns>
        private async Task<string?> GetDupInfoCertification(Guid certId)
        {
            var result = default(string?);

            var query = from m in _barshopDb.User_Certification_Log
                        where m.CertID == certId.ToString() 
                        select m;
            var item = await query.FirstOrDefaultAsync();
            if (item != null)
            {
                result = item.DupInfo;
            }
            
            return result;
        }

        /// <summary>
        /// User ID 사용여부 확인
        /// True: 사용중
        /// False: 미사용
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<bool> CheckExistsUserId(string userId)
		{
            var query = from m in _barshopDb.VW_USER_INFO
                        where m.uid == userId
                        select m;
            var count = await query.CountAsync();
            if (count == 0)
            {
                var query2 = from m in _barshopDb.S2_UserBye
                             where m.uid == userId
                             select m;
                count = await query2.CountAsync();
            }
			return count > 0;
        }
		/// <summary>
		/// 약관 목록
		/// </summary>
		/// <returns></returns>
		private async Task<List<PolicyModel>> GetPolices()
		{
			var result = new List<PolicyModel>();

			var now = DateTime.Now.ToString("yyyy-MM-dd");
			var dbItems = new List<PolicyModel>();
			if (this.RefererSite.SiteGubun == "BM") //모초
			{
				var query = from m in _barunsonDb.TB_PolicyInfo
							where m.EndDate.CompareTo(now) > 0 && m.StartDate.CompareTo(now) <= 0
							select new PolicyModel
							{
								Seq = m.Seq,
								Title = m.Title,
								Contents = m.Contents,
								PolicyDivision = m.PolicyDiv
							};

				dbItems = await query.ToListAsync();

			}
			else
			{
				var query = from m in _barshopDb.PolicyInfo
							where m.EndDate.CompareTo(now) > 0 && m.StartDate.CompareTo(now) <= 0
							&& m.SalesGubun == this.RefererSite.SiteGubun
							select new PolicyModel
							{
								Seq = m.Seq,
								Title = m.Title,
								Contents = m.Contents,
								PolicyDivision = m.PolicyDiv
							};

				dbItems = await query.ToListAsync();
			}
            foreach (var gitem in dbItems.GroupBy(g => g.PolicyDivision))
            {
                var item = gitem.First(m => m.Seq == gitem.Max(g => g.Seq));
                result.Add(item);

            }
            return result;
        }
      

        #endregion

        #region 신규 회원 가입 페이지

        #region Private 함수

        /// <summary>
        /// 신규 회원 가입 입력 모델 생성
        /// </summary>
        /// <param name="certId"></param>
        /// <returns></returns>
        private async Task<MemberRegModel?> GetNewMemberEditModel(Guid certId)
        {
            //인증 정보 읽기
            var CertModel = await GetMemberCertification(certId);
            if (CertModel == null)
                return null;

            var model = new MemberRegModel
            {
                CertId = certId,
                Name = CertModel.Name,
                BirthDay = CertModel.Birthdate,
                WeddingDay = DateTime.Today.AddMonths(1)
            };
            if (CertModel.MobileNo.Length == 10)
            {
                model.MoTelNo1 = CertModel.MobileNo.Substring(0, 3);
                model.MoTelNo2 = CertModel.MobileNo.Substring(3, 3);
                model.MoTelNo3 = CertModel.MobileNo.Substring(6, 4);
            }
            else
            {
                model.MoTelNo1 = CertModel.MobileNo.Substring(0, 3);
                model.MoTelNo2 = CertModel.MobileNo.Substring(3, 4);
                model.MoTelNo3 = CertModel.MobileNo.Substring(7, 4);
            }
            return model;
        }

        /// <summary>
        /// S2_UserInfo_BHands 데이터 생성
        /// </summary>
        /// <param name="certInfo"></param>
        /// <param name="model"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private S2_UserInfo_BHands FillUserInfoBHands(MemberCertificationModel certInfo, MemberRegModel model, DateTime now)
        {
            var splitPostCode = SplitPostCode(model.PostCode);
            string siteCode = "SA";

            return new S2_UserInfo_BHands
            {
                site_div = siteCode,

                //기본정보
                uid = model.UserId,
                PWD_BACKUP = null,
                uname = model.Name,
                umail = model.Email,
                jumin = "",
                birth = model.BirthDay?.ToString("yyyy-MM-dd"),
                birth_div = model.SolarOrLunar,
                zip1 = splitPostCode.Item1,
                zip2 = splitPostCode.Item2,
                address = model.Address,
                addr_detail = model.AddressDetail,
                phone1 = model.TelNo1,
                phone2 = model.TelNo2,
                phone3 = model.TelNo3,
                hand_phone1 = model.MoTelNo1,
                hand_phone2 = model.MoTelNo2,
                hand_phone3 = model.MoTelNo3,

                //인증 정보
                AuthType = certInfo.Authtype,
                DupInfo = certInfo.di,
                ConnInfo = certInfo.ci,
                Gender = certInfo.Gender,
                BirthDate = certInfo.Birthdate.ToString("yyyyMMdd"),
                NationalInfo = certInfo.nationalinfo,

                wedd_year = model.WeddingDay.Year.ToString(),
                wedd_month = model.WeddingDay.ToString("MM"),
                wedd_day = model.WeddingDay.ToString("dd"),
                ugubun = null,
                chk_DM = null,
                wedd_hour = "",
                wedd_minute = "",
                wedd_pgubun = model.WeddingHallType,
                wedd_name = model.WeddingHallName ?? "",

                addr_flag = 0,

                mod_date = now,
                reg_date = now,
                inflow_route = this.IsMobile() ? "MOBILE" : "PC",

                //등록 사이트
                REFERER_SALES_GUBUN = this.RefererSite.SiteGubun,
                SELECT_SALES_GUBUN = this.RefererSite.SiteGubun,
                SELECT_USER_ID = model.UserId,

                //이후 동의
                INTEGRATION_MEMBER_YORN = "Y",
                INTERGRATION_DATE = now,
                INTERGRATION_BEFORE_ID = model.UserId,
                USE_YORN = "Y",
                isJehu = "N",
                chk_sms = ConvertBoolToYN(model.PolicyAgree.SMSEMail),
                chk_mailservice = ConvertBoolToYN(model.PolicyAgree.SMSEMail),
                mkt_chk_flag = ConvertBoolToYN(model.PolicyAgree.ThirdParty),

                chk_lgmembership = ConvertBoolToYN(model.PolicyAgree.LGMembership),
                lgmembership_reg_date = model.PolicyAgree.LGMembership ? now : null,
                chk_casamiamembership = ConvertBoolToYN(model.PolicyAgree.CasamiaMembership),
                casamiaship_reg_Date = model.PolicyAgree.CasamiaMembership ? now : null,

                //기타
                isMCardAble = "0",
                is_appSample = "N",

                //이후 제휴 종료
                chk_smembership = "N",
                smembership_chk_flag = "N",
                smembership_reg_date = null,
                smembership_inflow_route = null,
                chk_smembership_per = "N",
                chk_smembership_coop = "N",
                smembership_period = "",
                chk_myomee = "N",
                myomee_reg_date = null,
                chk_iloommembership = "N",
                iloommembership_reg_date = null,
                chk_cuckoosmembership = "N",
                cuckoosship_reg_Date = null,
                chk_ktmembership = "N",
                ktmembership_reg_Date = null,
                chk_hyundaimembership = "N",
                hyundaimembership_reg_Date = null,
            };
        }

        /// <summary>
        /// S2_UserInfo_TheCard 데이터 생성
        /// </summary>
        /// <param name="certInfo"></param>
        /// <param name="model"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private S2_UserInfo_TheCard FillUserInfoTheCard(MemberCertificationModel certInfo, MemberRegModel model, DateTime now)
        {
            var splitPostCode = SplitPostCode(model.PostCode);
            string siteCode = "ST";
            return new S2_UserInfo_TheCard
            {
                site_div = siteCode,

                //기본정보
                uid = model.UserId,
                PWD_BACKUP = null,
                uname = model.Name,
                umail = model.Email,
                jumin = "",
                birth = model.BirthDay?.ToString("yyyy-MM-dd"),
                birth_div = model.SolarOrLunar,
                zip1 = splitPostCode.Item1,
                zip2 = splitPostCode.Item2,
                address = model.Address,
                addr_detail = model.AddressDetail,
                phone1 = model.TelNo1,
                phone2 = model.TelNo2,
                phone3 = model.TelNo3,
                hand_phone1 = model.MoTelNo1,
                hand_phone2 = model.MoTelNo2,
                hand_phone3 = model.MoTelNo3,

                //인증 정보
                AuthType = certInfo.Authtype,
                DupInfo = certInfo.di,
                ConnInfo = certInfo.ci,
                Gender = certInfo.Gender,
                BirthDate = certInfo.Birthdate.ToString("yyyyMMdd"),
                NationalInfo = certInfo.nationalinfo,

                wedd_year = model.WeddingDay.Year.ToString(),
                wedd_month = model.WeddingDay.ToString("MM"),
                wedd_day = model.WeddingDay.ToString("dd"),
                ugubun = null,
                chk_DM = null,
                wedd_hour = "",
                wedd_minute = "",
                wedd_pgubun = model.WeddingHallType,
                wedd_name = model.WeddingHallName ?? "",

                addr_flag = 0,

                mod_date = now,
                reg_date = now,
                inflow_route = this.IsMobile() ? "MOBILE" : "PC",

                //등록 사이트
                REFERER_SALES_GUBUN = this.RefererSite.SiteGubun,
                SELECT_SALES_GUBUN = this.RefererSite.SiteGubun,
                SELECT_USER_ID = model.UserId,

                //이후 동의
                INTEGRATION_MEMBER_YORN = "Y",
                INTERGRATION_DATE = now,
                INTERGRATION_BEFORE_ID = model.UserId,
                USE_YORN = "Y",
                isJehu = "N",
                chk_sms = ConvertBoolToYN(model.PolicyAgree.SMSEMail),
                chk_mailservice = ConvertBoolToYN(model.PolicyAgree.SMSEMail),
                mkt_chk_flag = ConvertBoolToYN(model.PolicyAgree.ThirdParty),

                chk_lgmembership = ConvertBoolToYN(model.PolicyAgree.LGMembership),
                lgmembership_reg_date = model.PolicyAgree.LGMembership ? now : null,
                chk_casamiamembership = ConvertBoolToYN(model.PolicyAgree.CasamiaMembership),
                casamiaship_reg_Date = model.PolicyAgree.CasamiaMembership ? now : null,

                //기타
                isMCardAble = "0",
                is_appSample = "N",

                //이후 제휴 종료
                chk_smembership = "N",
                smembership_chk_flag = "N",
                smembership_reg_date = null,
                smembership_inflow_route = null,
                chk_smembership_per = "N",
                chk_smembership_coop = "N",
                smembership_period = "",
                chk_myomee = "N",
                myomee_reg_date = null,
                chk_iloommembership = "N",
                iloommembership_reg_date = null,
                chk_cuckoosmembership = "N",
                cuckoosship_reg_Date = null,
                chk_ktmembership = "N",
                ktmembership_reg_Date = null,
                chk_hyundaimembership = "N",
                hyundaimembership_reg_Date = null,
            };
        }

        /// <summary>
        /// S2_UserInfo 데이터 생성
        /// </summary>
        /// <param name="siteCode"></param>
        /// <param name="certInfo"></param>
        /// <param name="model"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private S2_UserInfo FillUserInfo(string siteCode, MemberCertificationModel certInfo, MemberRegModel model, DateTime now)
        {
            var splitPostCode = SplitPostCode(model.PostCode);
            return new S2_UserInfo
            {
                site_div = siteCode,
                
                //기본정보
                uid = model.UserId,
                PWD_BACKUP = null,
                uname = model.Name,
                umail = model.Email,
                jumin = "",
                birth = model.BirthDay?.ToString("yyyy-MM-dd"),
                birth_div = model.SolarOrLunar,
                zip1 = splitPostCode.Item1,
                zip2 = splitPostCode.Item2,
                address = model.Address,
                addr_detail = model.AddressDetail,
                phone1 = model.TelNo1,
                phone2 = model.TelNo2,
                phone3 = model.TelNo3,
                hand_phone1 = model.MoTelNo1,
                hand_phone2 = model.MoTelNo2,
                hand_phone3 = model.MoTelNo3,

                //인증 정보
                AuthType = certInfo.Authtype,
                DupInfo = certInfo.di,
                ConnInfo = certInfo.ci,
                Gender = certInfo.Gender,
                BirthDate = certInfo.Birthdate.ToString("yyyyMMdd"),
                NationalInfo = certInfo.nationalinfo,

                wedd_year = model.WeddingDay.Year.ToString(),
                wedd_month = model.WeddingDay.ToString("MM"),
                wedd_day = model.WeddingDay.ToString("dd"),
                ugubun = null,
                chk_DM = null,
                wedd_hour = "",
                wedd_minute = "",
                wedd_pgubun = model.WeddingHallType,
                wedd_name = model.WeddingHallName ?? "",

                addr_flag = 0,

                mod_date = now,
                reg_date = now,
                inflow_route = this.IsMobile() ? "MOBILE" : "PC",

                //등록 사이트
                REFERER_SALES_GUBUN = this.RefererSite.SiteGubun,
                SELECT_SALES_GUBUN = this.RefererSite.SiteGubun,
                SELECT_USER_ID = model.UserId,

                //이후 동의
                INTEGRATION_MEMBER_YORN = "Y",
                INTERGRATION_DATE = now,
                INTERGRATION_BEFORE_ID = model.UserId,
                USE_YORN = "Y",
                isJehu = "N",
                chk_sms = ConvertBoolToYN(model.PolicyAgree.SMSEMail),
                chk_mailservice = ConvertBoolToYN(model.PolicyAgree.SMSEMail),
                mkt_chk_flag = ConvertBoolToYN(model.PolicyAgree.ThirdParty),

                chk_lgmembership = ConvertBoolToYN(model.PolicyAgree.LGMembership),
                lgmembership_reg_date = model.PolicyAgree.LGMembership ? now : null,
                chk_casamiamembership = ConvertBoolToYN(model.PolicyAgree.CasamiaMembership),
                casamiaship_reg_Date = model.PolicyAgree.CasamiaMembership ? now : null,

                //기타
                isMCardAble = "0",
                is_appSample = "N",

                //이후 제휴 종료
                chk_smembership = "N",
                smembership_chk_flag = "N",
                smembership_reg_date = null,
                smembership_inflow_route = null,
                chk_smembership_per = "N",
                chk_smembership_coop = "N",
                smembership_period = "",
                chk_myomee = "N",
                myomee_reg_date = null,
                chk_iloommembership = "N",
                iloommembership_reg_date = null,
                chk_cuckoosmembership = "N",
                cuckoosship_reg_Date = null,
                chk_ktmembership = "N",
                ktmembership_reg_Date = null,
                chk_hyundaimembership = "N",
                hyundaimembership_reg_Date = null,
            };
        }

        /// <summary>
        /// 회원등록, 가입된 회원 완료 모델 생성
        /// 룰렛 이벤트 처리
        /// </summary>
        /// <param name="certId"></param>
        /// <returns></returns>
        private async Task<MemberCompleteModel?> GetMemberCompleteModel(Guid certId)
        {
            var now = DateTime.Now;
            var model = new MemberCompleteModel(this.RefererSite)
            {
                IsMobile = IsMobile()
            };
            var CertModel = await GetMemberCertification(certId);

            if (CertModel == null)
                return null;

            //등록된 회원 정보
            var memberQuery = from m in _barshopDb.VW_USER_INFO
                              where m.DupInfo == CertModel.di
                              select new
                              {
                                  m.uid,
                                  m.uname,
                                  m.HPHONE
                              };
            var member = await memberQuery.FirstOrDefaultAsync();
            if (member == null)
                return null;

            model.UserId = member.uid;
            model.Name = member.uname;

            #region 룰렛 이벤트 API 연동

            if (CertModel.Key != null)
            {
                var rouletteEvent = new RouletteEvent
                {
                    eventCode = CertModel.EventCode ?? "",
                    key = CertModel.Key ?? "",
                    userName = member.uname,
                    userPhone = member.HPHONE.Replace("-", ""),
                    joinCorp = "barunnson"
                };

                var rouletteResult = await _rouletteEventService.Send(rouletteEvent);
                if (rouletteResult.resultCode == 1)
                {
                    //이벤트 응모 내역,,
                    var existsJehu = await (from m in _barshopDb.S2_UserInfo_Jehu where m.UserId == member.uid && m.PartnerCode == "enmad" select m).CountAsync();
                    if (existsJehu == 0)
                    {
                        var jehu = new S2_UserInfo_Jehu
                        {
                            UserId = member.uid,
                            PartnerCode = "enmad",
                            Consent = true,
                            RegDate = now,
                            UpdateDate = now,
                            ExtendData = CertModel.Key
                        };

                        _barshopDb.S2_UserInfo_Jehu.Add(jehu);
                        await _barshopDb.SaveChangesAsync();

                        model.Message = "룰렛 이벤트에 참여해 주셔서 감사합니다.<br />룰렛 이벤트 참여 시 입력하신 휴대전화번호로 경품이 발송됩니다.";
                    }
                    else
                    {
                        model.Message = "이벤트에 응모하신 내역이 있습니다.";
                    }
                }
                else
                {
                    model.Message = $"{rouletteResult.errorMsg}!!<br /> 고객센터에 문의바랍니다.";
                }
            }

            #endregion

            //인증 정보 삭제
            await _barshopDb.User_Certification_Log.Where(m => m.CertID == certId.ToString()).ExecuteDeleteAsync();
                       
            return model;
        }

        #endregion

        /// <summary>
        /// 신규 회원 가입 페이지
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Register(Guid? certId)
		{
			if (certId == null)
				return RedirectToAction("Index");

			var model = await GetNewMemberEditModel(certId.Value);
            if (model == null)
				return RedirectToAction("Index");

			model.RefererSite = this.RefererSite;

            if (this.RefererSite.SiteGubun == "BM" || this.RefererSite.SiteGubun == "GS")
			{
                //모초, Gshop은 최소 동의만 
                model.ViewType = MemberViewType.Type2;
			}
            else
            {
                model.ViewType = MemberViewType.Type1;
                model.PolicyAgree.SMSEMail = false;
                model.PolicyAgree.LGMembership = false;
                model.PolicyAgree.CasamiaMembership = false;
                model.PolicyAgree.ThirdParty = false;
                model.PolicyAgree.ThirdPartyShinhan = false;
                model.PolicyAgree.ThirdPartyTelecom = false;

				model.Banners = await GetBanners();
            }
            model.Policies = await GetPolices();

            return View(model);
		}

        /// <summary>
        /// 신규 회원 가입 저장
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSave(MemberRegModel model)
		{
			model.RefererSite = this.RefererSite;

            //ID 중복 확인
            if (await CheckExistsUserId(model.UserId))
				ModelState.AddModelError(nameof(model.UserId), "이미 사용중인 아이디 입니다.");
				
			//휴대폰 번호
			if (string.IsNullOrEmpty(model.MoTelNo1) || string.IsNullOrEmpty(model.MoTelNo2) || string.IsNullOrEmpty(model.MoTelNo3))
                ModelState.AddModelError(nameof(model.MoTelNo2), "휴대폰 번호를 입력해 주세요");

            //모델 유효성 검사
            if (ModelState.IsValid)
			{
                //전처리.
                if (model.TelNo2 == null) model.TelNo2 = "";
                if (model.TelNo3 == null) model.TelNo3 = "";
                model.PolicyAgree.ThirdParty = (model.PolicyAgree.ThirdPartyShinhan || model.PolicyAgree.ThirdPartyTelecom);

                /* 신규 등록 분석....
				SP_INSERT_INTEGRATION_MEMBER_SIGN_UP_FOR_EXIST_MEMBER_NEW_20230315 프로시져 사용하고 있음. Git 없음

                1. S2_USERINFO_AUTH_INFO에 인증정보 Insert했으나 현재는 값 없음 마지막 등록 2023-05-31 07:57:38.357
				2. DUPINFO 값으로 기존 회원을 읽고 있음.. 회원 인증 단계에서 Dubpinfo를 검사하기 때문에.. 기존회원이 있을 수 없음. 통합회원 전환 관련 같으나.. 전환회원은 다른 페이지로 이동함.
				3. custom_order_history 에 @P_AUTHCODE, @ADDRESS 을 넣고 있는데 왜???? 

				이후 
				SP_INSERT_INTEGRATION_MEMBER_SIGN_UP_NEW_20230315 호출 
				1. 또 DUPINFO 값으로 기존 회원을 읽고 있음 - 약관, 마케팅 동의 내역을 읽고 있으나 신규이기 때문에 값 없음.
				2. 각 테이블에 Update and Insert 함
					- S2_USERINFO_BHANDS
					- S2_USERINFO_THECARD
					- S2_USERINFO   <- SB,SS,BM 중복 등록
				3. S4_EVENT_RAINA 제3자 마케팅 활용 동의 등록 또는 삭제

                이후
                SP_EXEC_TRANSACTION_INFORMATION_ID_TO_INTEGRATION_MEMBER 호출 < ExecuteIntergrationProcessor
                1. 각 테이블에 업데이트
                    REFERER_SALES_GUBUN = @REFERER_SALES_GUBUN
		            ,	SELECT_SALES_GUBUN = CAST(@SELECT_SITE AS VARCHAR(2))
		            ,	SELECT_USER_ID = @SELECT_ID
		            ,	ISJEHU = CASE WHEN @REFERER_SALES_GUBUN = 'B' AND SITE_DIV IN ( 'SA', 'B', 'C' ) THEN 'Y' ELSE ISJEHU END
		            ,	SITE_DIV = CASE WHEN @REFERER_SALES_GUBUN = 'B' AND SITE_DIV IN ( 'SA', 'B', 'C' ) THEN 'B' ELSE SITE_DIV END
                2. INTEGRATION_MEMBER_SIGN_UP_LOG Insert <- 로그인도 아닌데....
                3. Dupinfor 값으로 다시 각 테이블 업데이트
                    INTERGRATION_DATE, INTEGRATION_MEMBER_YORN, INTERGRATION_BEFORE_ID, MOD_DATE
                4. Dupinfor 값으로 기존ID, 새 ID을 읽이 각 주문테이블의 memberid를 업데이트 하고 있음 <- 신규에서는 필요 없음??.

                이후 
                3자 동의 처리 
                1. SP_INSERT_S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT - 3자동의 
                    S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT insert
                    각 테이블 업데이트: mkt_chk_flag
                2. SP_INSERT_MARKETING_AGREEMENT_LOG - 생명,통신 동의
                    S4_MARKETING_AGREEMENT_LOG 테이블 업데이트, insert

                이후
                SP_UPDATE_S2_USERINFO_INFLOW_ROUTE 호출
                1. 각 테이블 업데이트
                    INFLOW_ROUTE = PC or MOBILE
                2. S2_USERINFO_SIGNUP_DEVICE insert

                이후 
                알림톡 발송
                PROC_MEMBER_JOIN_BIZTALK

                이후 
                쿠폰 발급
                SP_EXEC_MEM_REGIST_GIFT_BARUNSONCARD_V3
				*/

                var now = DateTime.Now;
                var certInfo = await GetMemberCertification(model.CertId.Value);
                var isSuccess = false;

				//비번 암호화
				var pwd = await _barshopDb.Database
					.SqlQuery<string?>($"select CONVERT(VARCHAR(200), PWDENCRYPT({model.Password}), 1) as [Value]")
					.SingleOrDefaultAsync();

				Exception dbException = null;

				using (var tran = await _barshopDb.Database.BeginTransactionAsync()) 
				{
					try
					{
                        //회원 생성
						var uBhands = FillUserInfoBHands(certInfo, model, now);
                        uBhands.PWD = pwd;
                        _barshopDb.S2_UserInfo_BHands.Add(uBhands);

                        var theCard = FillUserInfoTheCard(certInfo, model, now);
                        theCard.PWD = pwd;
                        _barshopDb.S2_UserInfo_TheCard.Add(theCard);

                        foreach(var siteCode in new string[] {"SB","SS","BM" })
                        {
                            var userInfo = FillUserInfo(siteCode, certInfo, model, now);
                            userInfo.PWD = pwd;
                            _barshopDb.S2_UserInfo.Add(userInfo);
                        }

                        //S2_USERINFO_SIGNUP_DEVICE 등록,, 신규라도 기존 DupInfo가 있을 수 있음
                        var existsSignupDevice = await (from m in _barshopDb.S2_USERINFO_SIGNUP_DEVICE where m.DUPINFO == certInfo.di select m).CountAsync();
                        if (existsSignupDevice == 0)
                        {
                            var signupDevice = new S2_USERINFO_SIGNUP_DEVICE
                            {
                                DUPINFO = certInfo.di,
                                UID = model.UserId,
                                USER_AGENT = Request.Headers.UserAgent,
                                DEVICE_TYPE = this.IsMobile() ? "MOBILE" : "PC",
                                REG_DATE = now
                            };
                            _barshopDb.S2_USERINFO_SIGNUP_DEVICE.Add(signupDevice);
                        }

                        //S4_EVENT_RAINA 제3자 마케팅 활용 동의
                        if (model.PolicyAgree.ThirdParty)
                        {   
                            var eventRaina = new S4_Event_Raina
                            {
                                uid = model.UserId,
                                company_seq = this.RefererSite.CompaySeq,
                                event_div = "MKEVENT",
                                inflow_route = "MODIFY",
                                reg_date = now,
                            };
                            _barshopDb.S4_Event_Raina.Add(eventRaina);
                        }

                        //3자 동의 처리, S4_MARKETING_AGREEMENT_LOG 기록은 하지 않음.. 문제 발생시 기록해야 함.
                        if (model.PolicyAgree.ThirdParty)
                        {
                            var existsMktAgreeList = await (from m in _barshopDb.S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT where m.UID == model.UserId select m).ToListAsync();

                            if (model.PolicyAgree.ThirdPartyShinhan && !existsMktAgreeList.Any(m => m.MARKETING_TYPE_CODE == model.PolicyAgree.ThirdPartyShinhanCode))
                            {
                                var newThirdPartyShinhan = new S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT
                                {
                                    UID = model.UserId,
                                    MARKETING_TYPE_CODE = model.PolicyAgree.ThirdPartyShinhanCode,
                                    USE_YORN = "Y",
                                    REG_DATE = now
                                };
                                _barshopDb.S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT.Add(newThirdPartyShinhan);
                            }
                            if (model.PolicyAgree.ThirdPartyTelecom && !existsMktAgreeList.Any(m => m.MARKETING_TYPE_CODE == model.PolicyAgree.ThirdPartyTelecomCode))
                            {
                                var newThirdPartyTelecom = new S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT
                                {
                                    UID = model.UserId,
                                    MARKETING_TYPE_CODE = model.PolicyAgree.ThirdPartyTelecomCode,
                                    USE_YORN = "Y",
                                    REG_DATE = now
                                };
                                _barshopDb.S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT.Add(newThirdPartyTelecom);
                            }
                        }

                        await _barshopDb.SaveChangesAsync();
                        await tran.CommitAsync();
                        isSuccess = true;
					}
					catch (Exception ex)
					{
						dbException = ex;
						await tran.RollbackAsync();
					}
				}
                    
                if (isSuccess)
				{
                    //Transaction 외부에서 처리
                    //쿠폰 발급
                    await _barshopDb.Database.ExecuteSqlAsync($"EXECUTE dbo.SP_EXEC_MEM_REGIST_GIFT_BARUNSONCARD_V3 @COMPANY_SEQ={this.RefererSite.CompaySeq}, @UID={model.UserId}, @GIFT_CARD_SEQ=0");

                    var celPhone = $"{model.MoTelNo1}-{model.MoTelNo2}-{model.MoTelNo3}";
                    //알림톡 발송, 모초, Gshop은 보내지 않음.
                    await _barshopDb.Database.ExecuteSqlAsync($"EXECUTE dbo.PROC_MEMBER_JOIN_BIZTALK @SALES_GUBUN={this.RefererSite.SiteGubun}, @COMPANY_SEQ={this.RefererSite.CompaySeq}, @MEMBER_NAME={model.Name}, @MEMBER_HPHONE={celPhone}");

                    //Go to Next page
                    return RedirectToAction("RegisterComplete", new { certId = model.CertId });
                       
                }
                else
				{
                    //DB Error 시 로그 기록
                    if (dbException != null)
                    {
                        var dbLog = new BARUNN_INTEGRATE_USER_CHANGE_PROGRESS_LOG
                        {
                            DUPINFO = certInfo.di,
                            ID = model.UserId,
                            STEP = "RegisterSave",
                            STEP_DESC = dbException.ToString(),
                            REG_DATE = now
                        };
                        _barshopDb.BARUNN_INTEGRATE_USER_CHANGE_PROGRESS_LOG.Add(dbLog);
                        await _barshopDb.SaveChangesAsync();
                    }

					model.ValidMessage = "통합 회원 가입도중 오류가 발생 하였습니다. 다시 시도해 주십시오";
				}
            }
            else
            {
                if (ModelState[nameof(model.BirthDay)].Errors.Count > 0)
                {
                    ModelState[nameof(model.BirthDay)].Errors.Clear();
                    ModelState.AddModelError(nameof(model.BirthDay), "생년월일 날짜가 잘못되었습니다.");
                }
                if (ModelState[nameof(model.WeddingDay)].Errors.Count > 0)
                {
                    ModelState[nameof(model.WeddingDay)].Errors.Clear();
                    ModelState.AddModelError(nameof(model.WeddingDay), "예식일 날짜가 잘못되었습니다.");
                }
                
            }

            //모델 유효성 검사 실패로 모델 바인딩 기본 값을 다시 채워 뷰 페이지 출력
            if (this.RefererSite.SiteGubun == "BM" || this.RefererSite.SiteGubun == "GS")
			{
                model.ViewType = MemberViewType.Type2;
            }
			else
			{
                model.ViewType = MemberViewType.Type1;
                model.Banners = await GetBanners();
            }
            model.Policies = await GetPolices();

            return View("Register",model);
		}

        /// <summary>
        /// 신규 회원 가입 완료.
        /// </summary>
        /// <param name="certId"></param>
        /// <returns></returns>
        public async Task<IActionResult> RegisterComplete(Guid certId)
        {
            var viewType = "";
            if (this.RefererSite.SiteGubun == "BM" || this.RefererSite.SiteGubun == "GS")
            {
                viewType = MemberViewType.Type2;
            }
            var viewName = $"RegisterComplete{viewType}";

			MemberCompleteModel model = null;
			try
            {
                model = await GetMemberCompleteModel(certId);
            }
            catch { }
                        
            if (model == null)
				return RedirectToAction("Index");
            else
			    return View(viewName, model);
        }

        /// <summary>
        /// 이미 가입된 회원,
        /// 룰렛 이벤트 처리 해야 함.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CompleteForExist(Guid? certId)
        {
            if (certId == null)
                return RedirectToAction("Index");

            MemberCompleteModel model = null;
            try
            {
                model = await GetMemberCompleteModel(certId.Value);
            }
            catch { }

            if (model == null)
                return RedirectToAction("Index");
            else
                return View(model);

        }
        #endregion

        #region 회원 수정

        /// <summary>
        /// 회원 정보 수정 
        /// </summary>
        /// <param name="certId"></param>
        /// <param name="returnUrl"></param>
        /// <param name="SiteDiv"></param>
        /// <returns></returns>
        [Route("[controller]/Modify")]
        [Route("Profile-Modify")]
        public async Task<IActionResult> Modify(Guid? certId, string? returnUrl, string? SiteDiv)
        {
            if (certId == null)
                return RedirectToAction("Index");

            try
            {
                // 접속 사이트 구분은 returnurl로 사이트 구분 하고 있음. 
                // 모초는 returnurl 전달하지 않아 barunsoncard로 처리 되고 있음. 수정 필요
                // Gshop은 &ReturnUrl=&SiteDiv=GS 으로 전달
                SiteInfo? rsite = null;
                if (!string.IsNullOrEmpty(SiteDiv))
                {
                    rsite = _siteInfos.FirstOrDefault(m => string.Equals(m.SiteGubun, SiteDiv, StringComparison.InvariantCultureIgnoreCase));
                }
                else if(!string.IsNullOrEmpty(returnUrl))
                {
                    var rUrl = new Uri(returnUrl);
                    rsite = _siteInfos.FirstOrDefault(m => 
                        string.Equals(m.SiteUrl.Host, rUrl.Host, StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(m.MobileSiteUrl.Host, rUrl.Host, StringComparison.InvariantCultureIgnoreCase)
                    );
                   
                } 
                 
                if (rsite != null)
                {
                    SetRefererSite(rsite.Name);
                }


                var dupInfo = await GetDupInfoCertification(certId.Value);
                if (dupInfo == null)
                    return RedirectToAction("Index");

                var model = new MemberModModel
                {
                    RefererSite = this.RefererSite,
                    CertId = certId.Value,
                    ReturnUrl = returnUrl,
                };

                //등록된 회원 정보, DB구조가 중복이 있을 수 있음 최신 등록 사용자 읽음
                var memberQuery = from m in _barshopDb.VW_USER_INFO
                                  where m.DupInfo == dupInfo 
                                  && m.INTEGRATION_MEMBER_YORN == "Y" 
                                  orderby m.reg_date descending
                                  select m;
                var member = await memberQuery.FirstOrDefaultAsync();
                if (member == null)
                    return RedirectToAction("Index");

                model.UserId = member.uid;
                model.Name = member.uname;
                model.BirthDay = string.IsNullOrEmpty(member.BIRTH_DATE) ? null : DateTime.Parse(member.BIRTH_DATE);
                model.SolarOrLunar = member.BIRTH_DATE_TYPE;
                model.PostCode = member.ZIPCODE;
                model.Address = member.address;
                model.AddressDetail = member.addr_detail;
                var tels = member.PHONE.Split('-');
                model.TelNo1 = tels[0];
                model.TelNo2 = tels[1];
                model.TelNo3 = tels[2];
                var htels = member.HPHONE.Split("-");
                model.MoTelNo1 = htels[0];
                model.MoTelNo2 = htels[1];
                model.MoTelNo3 = htels[2];
                model.Email = member.umail;
                model.WeddingDay = DateTime.Parse(member.WEDDING_DAY);
                model.WeddingHallType = member.WEDDING_HALL;
                model.WeddingHallName = member.wedd_name;
                model.CheckSMS = (member.chk_sms == "Y");
                model.CheckEMail = (member.chk_mailservice == "Y");

                model.RegisterDate = member.reg_date;
                
                return View(model);
            }
            catch 
            {
                return RedirectToAction("Index");
            }
        }
        
        /// <summary>
        /// 회원 수정 저장
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifySave(MemberModModel model)
        {
            if (model.CertId == null)
                return RedirectToAction("Index");

            model.RefererSite = this.RefererSite;
            //휴대폰 번호
            if (string.IsNullOrEmpty(model.MoTelNo1) || string.IsNullOrEmpty(model.MoTelNo2) || string.IsNullOrEmpty(model.MoTelNo3))
                ModelState.AddModelError(nameof(model.MoTelNo2), "휴대폰 번호를 입력해 주세요");

            if (ModelState.IsValid)
            {
                //전처리.
                if (model.TelNo2 == null) model.TelNo2 = "";
                if (model.TelNo3 == null) model.TelNo3 = "";

                var now = DateTime.Now;
                var dupInfo = await GetDupInfoCertification(model.CertId.Value);
                var isSuccess = false;

                //비번 암호화
                var pwd = await _barshopDb.Database
                    .SqlQuery<string?>($"select CONVERT(VARCHAR(200), PWDENCRYPT({model.Password}), 1) as [Value]")
                    .SingleOrDefaultAsync();

                Exception dbException = null;

                using (var tran = await _barshopDb.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var splitPostCode = SplitPostCode(model.PostCode);
                        //회원 읽기
                        var uBhands = await (from m in _barshopDb.S2_UserInfo_BHands where m.DupInfo == dupInfo select m).ToListAsync();
                        var theCards = await (from m in _barshopDb.S2_UserInfo_TheCard where m.DupInfo == dupInfo select m).ToListAsync();
                        var userInfos = await (from m in _barshopDb.S2_UserInfo where m.DupInfo == dupInfo select m).ToListAsync();

                        #region 읽은 데이터 모두 업데이트.. 
                        foreach (var item in uBhands)
                        {
                            item.umail = model.Email;
                            item.PWD = pwd;
                            item.zip1 = splitPostCode.Item1;
                            item.zip2 = splitPostCode.Item2;
                            item.address = model.Address;
                            item.addr_detail = model.AddressDetail;
                            item.phone1 = model.TelNo1;
                            item.phone2 = model.TelNo2;
                            item.phone3 = model.TelNo3;
                            item.hand_phone1 = model.MoTelNo1;
                            item.hand_phone2 = model.MoTelNo2;
                            item.hand_phone3 = model.MoTelNo3;
                            item.chk_sms = ConvertBoolToYN(model.CheckSMS);
                            item.chk_mailservice = ConvertBoolToYN(model.CheckEMail);
                            item.birth = model.BirthDay?.ToString("yyyy-MM-dd");
                            item.birth_div = model.SolarOrLunar;
                            item.wedd_year = model.WeddingDay.Year.ToString();
                            item.wedd_month = model.WeddingDay.ToString("MM");
                            item.wedd_day = model.WeddingDay.ToString("dd");
                            item.wedd_pgubun = model.WeddingHallType;
                            item.wedd_name = model.WeddingHallName ?? "";
                            item.mod_date = now;
                        }
                        foreach (var item in theCards)
                        {
                            item.umail = model.Email;
                            item.PWD = pwd;
                            item.zip1 = splitPostCode.Item1;
                            item.zip2 = splitPostCode.Item2;
                            item.address = model.Address;
                            item.addr_detail = model.AddressDetail;
                            item.phone1 = model.TelNo1;
                            item.phone2 = model.TelNo2;
                            item.phone3 = model.TelNo3;
                            item.hand_phone1 = model.MoTelNo1;
                            item.hand_phone2 = model.MoTelNo2;
                            item.hand_phone3 = model.MoTelNo3;
                            item.chk_sms = ConvertBoolToYN(model.CheckSMS);
                            item.chk_mailservice = ConvertBoolToYN(model.CheckEMail);
                            item.birth = model.BirthDay?.ToString("yyyy-MM-dd");
                            item.birth_div = model.SolarOrLunar;
                            item.wedd_year = model.WeddingDay.Year.ToString();
                            item.wedd_month = model.WeddingDay.ToString("MM");
                            item.wedd_day = model.WeddingDay.ToString("dd");
                            item.wedd_pgubun = model.WeddingHallType;
                            item.wedd_name = model.WeddingHallName ?? "";
                            item.mod_date = now;
                        }
                        foreach (var item in userInfos)
                        {
                            item.umail = model.Email;
                            item.PWD = pwd;
                            item.zip1 = splitPostCode.Item1;
                            item.zip2 = splitPostCode.Item2;
                            item.address = model.Address;
                            item.addr_detail = model.AddressDetail;
                            item.phone1 = model.TelNo1;
                            item.phone2 = model.TelNo2;
                            item.phone3 = model.TelNo3;
                            item.hand_phone1 = model.MoTelNo1;
                            item.hand_phone2 = model.MoTelNo2;
                            item.hand_phone3 = model.MoTelNo3;
                            item.chk_sms = ConvertBoolToYN(model.CheckSMS);
                            item.chk_mailservice = ConvertBoolToYN(model.CheckEMail);
                            item.birth = model.BirthDay?.ToString("yyyy-MM-dd");
                            item.birth_div = model.SolarOrLunar;
                            item.wedd_year = model.WeddingDay.Year.ToString();
                            item.wedd_month = model.WeddingDay.ToString("MM");
                            item.wedd_day = model.WeddingDay.ToString("dd");
                            item.wedd_pgubun = model.WeddingHallType;
                            item.wedd_name = model.WeddingHallName ?? "";
                            item.mod_date = now;
                        }
                        #endregion

                        await _barshopDb.SaveChangesAsync();
                        await tran.CommitAsync();
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        dbException = ex;
                        await tran.RollbackAsync();
                    }
                }

                if (isSuccess)
                {
                    model.SaveSuccess = isSuccess;
                    //인증 정보 삭제
                    await _barshopDb.User_Certification_Log.Where(m => m.CertID == model.CertId.Value.ToString()).ExecuteDeleteAsync();
                }
                else
                {
                    //DB Error 시 로그 기록
                    if (dbException != null)
                    {
                        var dbLog = new BARUNN_INTEGRATE_USER_CHANGE_PROGRESS_LOG
                        {
                            DUPINFO = dupInfo,
                            ID = model.UserId,
                            STEP = "ModifySave",
                            STEP_DESC = dbException.Message.ToString(),
                            REG_DATE = now
                        };
                        _barshopDb.BARUNN_INTEGRATE_USER_CHANGE_PROGRESS_LOG.Add(dbLog);
                        await _barshopDb.SaveChangesAsync();
                    }

                    model.ValidMessage = "오류가 발생 하였습니다. 다시 시도해 주십시오";
                }
            }
            else
            {
                if (ModelState[nameof(model.BirthDay)].Errors.Count > 0)
                {
                    ModelState[nameof(model.BirthDay)].Errors.Clear();
                    ModelState.AddModelError(nameof(model.BirthDay), "생년월일 날짜가 잘못되었습니다.");
                }
                if (ModelState[nameof(model.WeddingDay)].Errors.Count > 0)
                {
                    ModelState[nameof(model.WeddingDay)].Errors.Clear();
                    ModelState.AddModelError(nameof(model.WeddingDay), "예식일 날짜가 잘못되었습니다.");
                }
            }

            return View("Modify", model);
        }

        #endregion

        #region 회원 탈퇴

        /// <summary>
        /// 탈퇴 페이지
        /// </summary>
        /// <param name="certId"></param>
        /// <param name="returnUrl"></param>
        /// <param name="SiteDiv"></param>
        /// <returns></returns>
        [Route("[controller]/Secession")]
        [Route("Secession")]
        public async Task<IActionResult> Secession(Guid? certId, string? returnUrl, string? SiteDiv)
        {
            if (certId == null)
                return RedirectToAction("Index");
            // SiteDiv 값으로 사이트 구분
            SiteInfo? rsite = null;
            if (!string.IsNullOrEmpty(SiteDiv))
            {
                rsite = _siteInfos.FirstOrDefault(m => string.Equals(m.SiteGubun, SiteDiv, StringComparison.InvariantCultureIgnoreCase));
            }
            else if (!string.IsNullOrEmpty(returnUrl))
            {
                var rUrl = new Uri(returnUrl);
                rsite = _siteInfos.FirstOrDefault(m => string.Equals(m.SiteUrl.Host, rUrl.Host, StringComparison.InvariantCultureIgnoreCase));
            }

            if (rsite != null)
            {
                SetRefererSite(rsite.Name);
            }
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = this.IsMobile() ? this.RefererSite.MobileSiteUrl.ToString() : this.RefererSite.SiteUrl.ToString();
            }

            var model = new MemberSecessionModel()
            {
                RefererSite = this.RefererSite,
                IsMobile = this.IsMobile(),
                CertId = certId.Value,
                ReturnUrl = returnUrl,
            };

            try
            {
                var dupInfo = await GetDupInfoCertification(certId.Value);
                if (dupInfo == null)
                    return Redirect(model.ReturnUrl);

                //등록된 회원 정보 읽음
                var memberQuery = from m in _barshopDb.VW_USER_INFO
                                  where m.DupInfo == dupInfo
                                  && m.INTEGRATION_MEMBER_YORN == "Y"
                                  orderby m.reg_date descending
                                  select new { m.uid, m.uname };
                var member = await memberQuery.FirstOrDefaultAsync();
                if (member == null)
                    return Redirect(model.ReturnUrl);

                model.UserId = member.uid;

                return View(model);
            }
            catch
            {
                return Redirect(model.ReturnUrl);
            }
        }

        /// <summary>
        /// 탈퇴 저장
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SecessionSave(MemberSecessionModel model)
        {
            if (model.CertId == null)
                return RedirectToAction("Index");

            model.RefererSite = this.RefererSite;
            if (!model.SecessionCause.Any(m => m.Selected))
            {
                ModelState.AddModelError(nameof(model.SecessionCause), "탈퇴 사유를 선택해주세요.");
                model.ValidMessage = "탈퇴 사유를 선택해주세요.";
            }

            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var dupInfo = await GetDupInfoCertification(model.CertId.Value);
                var isSuccess = false;
                Exception dbException = null;

                using (var tran = await _barshopDb.Database.BeginTransactionAsync())
                {
                    try
                    {
                        //등록된 회원 정보 읽음
                        var memberQuery = from m in _barshopDb.VW_USER_INFO
                                          where m.DupInfo == dupInfo
                                          && m.INTEGRATION_MEMBER_YORN == "Y"
                                          orderby m.reg_date descending
                                          select new { m.uid, m.uname,  m.site_div };
                        var members = await memberQuery.ToListAsync();
                        var uid = members.First().uid;

                        foreach(var cause in model.SecessionCause.Where((m => m.Selected))) 
                        {
                            var useByeSecessionCause = new S2_USERBYE_SECESSION_CAUSE
                            {
                                DUP_INFO = dupInfo,
                                UID = uid,
                                SITE_DIV = this.RefererSite.SiteGubun,
                                SECESSION_CAUSE_CODE = cause.Value,
                                ETC_COMMENT = (cause.Value == "117007") ? model.EtcComment : "",
                                REG_DATE = now
                            };
                            _barshopDb.S2_USERBYE_SECESSION_CAUSE.Add(useByeSecessionCause);
                        }
                           
                        foreach (var member in members)
                        {
                            var userBye = new S2_UserBye
                            {
                                uid = member.uid,
                                reason_1 = "N",
                                reason_2 = "N",
                                reason_3 = "N",
                                reason_4 = "N",
                                reason_5 = "N",
                                reason_6 = "N",
                                reason_7 = "N",
                                reason_txt = "",
                                company_seq = _siteInfos.FirstOrDefault(m => m.SiteGubun == member.site_div)?.CompaySeq,
                                DupInfo = dupInfo,
                                reg_date = now
                            };
                            _barshopDb.S2_UserBye.Add(userBye);
                        }
                        await _barshopDb.SaveChangesAsync();

                        await _barshopDb.S2_UserInfo.Where(m => m.DupInfo == dupInfo).ExecuteDeleteAsync();
                        await _barshopDb.S2_UserInfo_BHands.Where(m => m.DupInfo == dupInfo).ExecuteDeleteAsync();
                        await _barshopDb.S2_UserInfo_TheCard.Where(m => m.DupInfo == dupInfo).ExecuteDeleteAsync();
                        await _barshopDb.S2_UserInfo_Jehu.Where(m => m.UserId == uid).ExecuteDeleteAsync();

                        await tran.CommitAsync();
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        dbException = ex;
                        await tran.RollbackAsync();
                    }
                }

                if (isSuccess)
                {       
                    //인증 정보 삭제
                    await _barshopDb.User_Certification_Log.Where(m => m.CertID == model.CertId.Value.ToString()).ExecuteDeleteAsync();
                    return RedirectToAction("SecessionComplete", new { returnUrl = model.ReturnUrl });
                }
                else
                {
                    //DB Error 시 로그 기록
                    if (dbException != null)
                    {
                        var dbLog = new BARUNN_INTEGRATE_USER_CHANGE_PROGRESS_LOG
                        {
                            DUPINFO = dupInfo,
                            ID = model.UserId,
                            STEP = "SecessionSave",
                            STEP_DESC = dbException.Message.ToString(),
                            REG_DATE = now
                        };
                        _barshopDb.BARUNN_INTEGRATE_USER_CHANGE_PROGRESS_LOG.Add(dbLog);
                        await _barshopDb.SaveChangesAsync();
                    }

                    model.ValidMessage = "오류가 발생 하였습니다. 다시 시도해 주십시오";
                }
            }

            return View("Secession", model);
        }

        /// <summary>
        /// 회원탈퇴 완료 페이지
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public IActionResult SecessionComplete(string? returnUrl)
        {
            //Ghshop은 Redirect
            if (this.RefererSite.SiteGubun == "GS")
            {
                return Redirect("https://barunsongshop.com/exec/front/Member/logout/");
            }
            var model = new MemberSecessionComplete(this.RefererSite) { ReturnUrl = returnUrl };
            return View(model);
        }
    
        #endregion

        #region 회원 조회, 로그인
        /// <summary>
        /// 회원 로그인, 조회
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="authcode">사용안함</param>
        /// <param name="output">json or xml</param>
        /// <returns></returns>
        [Route("[controller]/SelectUserInfo", Order = 0)]
        [Route("Service/Member/SelectUserInfo.ashx", Order = 1)]
        public async Task<IActionResult> SelectUserInfo(string? userId, string? password, string? authcode, string output)
        {
            var model = new MemberInfo
            {
                Success = false
            };
            try
            {
                var query = _barshopDb.VW_USER_INFO
                    .FromSql($"Select * From VW_USER_INFO Where UID={userId} And PWDCOMPARE({password}, TRY_CONVERT(VARBINARY(200), PWD, 1)) = 1");

                var user = await query.FirstOrDefaultAsync();
                if (user != null)
                {
                    model = new MemberInfo
                    {
                        Success = true,
                        UserId = user.uid,
                        UserName = user.uname,
                        AuthCode = user.DupInfo,
                        Email = user.umail,
                        PostCode = user.ZIPCODE,
                        Address = user.address,
                        AddressDetail = user.addr_detail,
                        Phone = user.PHONE,
                        MobilePhone = user.HPHONE,
                        IsIntergrationMemberYorN = (user.INTEGRATION_MEMBER_YORN == "Y"),
                        AllowSMS = (user.chk_sms == "Y"),
                        AllowEMail = (user.chk_mailservice == "Y"),
                    };
                }
            }
            catch { }

            if (output == "json")

                return Json(model);
            else
            {
                return new ContentResult
                {
                    Content = GetXMLString(model),
                    ContentType = "text/xml",
                    StatusCode = 200
                };
            }
        }

        /// <summary>
        /// 회원 조회 결과 XML 출력
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetXMLString(MemberInfo model)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<ROOT>");
            sb.AppendLine($"<SUCCESS>{model.Success.ToString().ToUpper()}</SUCCESS>");
            sb.AppendLine("<RESULT>");

            sb.AppendLine("<ELEMENT>");
            sb.AppendLine($"<AUTH_CODE><![CDATA[{model.AuthCode}]]></AUTH_CODE>");
            sb.AppendLine($"<USER_ID><![CDATA[{model.UserId}]]></USER_ID>");
            sb.AppendLine($"<USER_NAME><![CDATA[{model.UserName}]]></USER_NAME>");
            sb.AppendLine($"<USER_EMAIL><![CDATA[{model.Email}]]></USER_EMAIL>");
            sb.AppendLine($"<ZIPCODE>{model.PostCode}</ZIPCODE>");
            sb.AppendLine($"<ADDRESS><![CDATA[{model.Address}]]></ADDRESS>");
            sb.AppendLine($"<ADDRESS_DETAIL><![CDATA[{model.AddressDetail}]]></ADDRESS_DETAIL>");
            sb.AppendLine($"<PHONE>{model.Phone}</PHONE>");
            sb.AppendLine($"<CELLPHONE>{model.MobilePhone}</CELLPHONE>");
            sb.AppendLine($"<IS_INTERGRATION_MEMBER>{model.IsIntergrationMemberYorN.ToString().ToUpper()}</IS_INTERGRATION_MEMBER>");
            sb.AppendLine($"<ALLOWSMS>{model.AllowSMS.ToString().ToUpper()}</ALLOWSMS>");
            sb.AppendLine($"<ALLOWMAILING>{model.AllowEMail.ToString().ToUpper()}</ALLOWMAILING>");
            sb.AppendLine("</ELEMENT>");

            sb.AppendLine("</RESULT>");
            sb.AppendLine("</ROOT>");

            return sb.ToString();
        }
        #endregion

        #region 공통 API

        /// <summary>
        /// User ID 중복 확인
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IActionResult> CheckUserId(string UserId)
        {
            var exsits = await CheckExistsUserId(UserId);
            return Json(!exsits);
        }

        #endregion

        #region 약관 히스토리

        /// <summary>
        /// 약환 히스토리 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> PolicyHistory(PolicyHistoryModel model)
        {
            model.RefererSite = RefererSite;
            List<PolicyModel> historyItems;
            if (this.RefererSite.SiteGubun == "BM") //모초
            {
                var historyQuery = from m in _barunsonDb.TB_PolicyInfo
                                   where m.PolicyDiv == model.PolicyDivision
                                   orderby m.Seq descending
                                   select new PolicyModel { Seq = m.Seq, StartDate = m.StartDate, EndDate = m.EndDate, Contents = m.Contents, PolicyDivision = m.PolicyDiv, Title = m.Title };
                historyItems = await historyQuery.ToListAsync();
            }
            else
            {
                var historyQuery = from m in _barshopDb.PolicyInfo
                                   where m.SalesGubun == this.RefererSite.SiteGubun
                                   && m.PolicyDiv == model.PolicyDivision
                                   orderby m.Seq descending
                                   select new PolicyModel { Seq = m.Seq, StartDate = m.StartDate, EndDate = m.EndDate, Contents = m.Contents, PolicyDivision = m.PolicyDiv, Title = m.Title };
                historyItems = await historyQuery.ToListAsync();
            }
           
            if (historyItems.Count > 0)
            {
                model.SelectSeqs = new SelectList(
                    historyItems.Select(x => new SelectListItem { Text = $"{x.StartDate.Replace("-", ".")} 시행", Value = x.Seq.ToString() })
                    , "Value", "Text");

                if (model.Seq == 0)
                {
                    var endDate = DateTime.Now.ToString("yyyy-MM-dd");
                    var maxSeq = historyItems.Where(m => m.EndDate.CompareTo(endDate) < 0).OrderByDescending(m => m.Seq).FirstOrDefault();
                    if (maxSeq != null)
                        model.Seq = maxSeq.Seq;
                    else
                        model.Seq = historyItems.Max(x => x.Seq);
                }
                var selectItem = historyItems.Single(x => x.Seq == model.Seq);
                model.Title = model.PolicyDivision == "P" ? "개인정보 보호방침" : "이용약관";
                model.Contents = selectItem.Contents;
            }
            return View(model);
        }
        #endregion

        /// <summary>
        /// 통합회원 전환 (사용안함, 필요할경우 개발 해야 함)
        /// </summary>
        /// <returns></returns>
        public IActionResult SignInForExist(Guid? certId)
        {
            return RedirectToAction("Index", "Home");
        }

    }
}

