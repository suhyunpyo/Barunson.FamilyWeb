using Barunson.DbContext;
using Barunson.DbContext.DbModels.BarShop;
using Barunson.FamilyWeb.Models;
using Barunson.FamilyWeb.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Barunson.FamilyWeb.Controllers
{
    public class NcieCheckController : BaseController
	{
		private readonly INiceCPClientService _niceCPClientService;
		public NcieCheckController(ILogger<MemberController> logger, BarunsonContext barunsonDb, BarShopContext barshopDb, List<SiteInfo> siteInfos, INiceCPClientService niceCPClientService)
			: base(logger, barunsonDb, barshopDb, siteInfos)
		{
			_niceCPClientService = niceCPClientService;
		}

		public async Task<IActionResult> Call(NiceRequestCallBackModel reqData)
		{
			var callback = new Uri(Url.ActionLink("CallBack", "NcieCheck", reqData));
			var model = new NiceRequestModel
			{
				NiceCheckUri = new Uri("https://nice.checkplus.co.kr/CheckPlusSafeModel/service.cb"),
				NiceRequest = await _niceCPClientService.GetEncoded(callback)
			};
			return View(model);
		}
		public async Task<IActionResult> CallBack(NiceResponseCallBackModel reqData)
		{
			var model = new NiceResponseStatusModel
			{
				IsSuccess = false,
				Message = "본인인증 실패 다시 시도해 주세요"
			};
			try
			{
				var now = DateTime.Now;
				CultureInfo provider = CultureInfo.InvariantCulture;

				var niceResponse = await _niceCPClientService.GetDecoded(reqData.token_version_id, reqData.enc_data, reqData.integrity_value);
				if (niceResponse != null)
				{
					#region 14세 미만 확인
					var strBirth = niceResponse.birthdate;
					var birth = DateTime.ParseExact(strBirth, "yyyyMMdd", provider);
					var diff = now - birth;
					var diffYear = now.Year - birth.Year;
					if (now < birth.AddYears(diffYear))
						diffYear--;

					if (diffYear < 14)
					{
						model.Message = "14세 미만은 회원가입이 불가능합니다";
						return View(model);
					}
					#endregion

					//가입여부 확인 (di 사용)
					//가입되어 있고, 통합회원인 경우 CompleteForExist
					//가입되어 있고, 통합회원 전환 필요 SignInForExist
					var exitsQuery = from m in _barshopDb.VW_USER_INFO
									 where m.DupInfo == niceResponse.di
									 select new { m.uid, m.INTEGRATION_MEMBER_YORN };
					var existsMember = await exitsQuery.FirstOrDefaultAsync();

					model.IsSuccess = true;

					/* 
						사용 값:
						1. authtype (인증수단)
						2. name (이름)
						3. birthdate (생일)
						4. gender (성별)
						5. nationalinfo (내외국인)
						6. ci, di
						7. mobileno (휴대폰)
					*/
					//USER_CERTIFICATION_LOG 에 저장하고 CertID 를 사용하여 다음단계 진행
					model.CertID = Guid.NewGuid();

					var CertModel = new MemberCertificationModel
					{
						Authtype = niceResponse.authtype,
						Name = niceResponse.name,
						Birthdate = birth,
						Gender = niceResponse.gender,
						MobileNo = niceResponse.mobileno,
						ci = niceResponse.ci,
						di = niceResponse.di,
						nationalinfo = niceResponse.nationalinfo,
						BizCode = reqData.BizCode,
						EventCode = reqData.EventCode,
						Key = reqData.Key
					};
					var CertJson = JsonSerializer.Serialize(CertModel);
					var CertData = Base64Convert.Encoded(CertJson);

					var certLog = new User_Certification_Log
					{
						CertType = "CPCLIENT",
						CertID = model.CertID.Value.ToString(),
						DupInfo = niceResponse.di,
						CertData = CertData,
						RegDate = now
					};
					_barshopDb.User_Certification_Log.Add(certLog);
					await _barshopDb.SaveChangesAsync();

                    //신규 가입 또는 통합회원 전환 필요(전환 하지 않고 생성)
                    if (existsMember == null || existsMember.INTEGRATION_MEMBER_YORN != "Y")
					{
						model.ActionUrl = Url.Action(reqData.NextAction, reqData.NextController, new { certId = model.CertID });
					}
					else
					{
                        model.ActionUrl = Url.Action("CompleteForExist", "Member", new { certId = model.CertID });
					}
				}
			}
			catch 
			{
				model.IsSuccess = false;
			}
			return View(model);
		}
	}
}
