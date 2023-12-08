using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Barunson.FamilyWeb.Models
{
	/// <summary>
	/// 모든 응답 모델이 기본 클레스
	/// </summary>
	public class ResponseBaseModel
	{
		public ResponseBaseModel(SiteInfo siteInfo) 
		{ 
			RefererSite = siteInfo;
		}
        /// <summary>
        /// 접속 사이트 정보
        /// </summary>
        [ValidateNever] 
		public SiteInfo RefererSite { get; private set; }
	}
	
}
