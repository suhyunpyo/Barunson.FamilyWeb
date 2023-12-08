namespace Barunson.FamilyWeb.Models
{
	/// <summary>
	/// Nice 인증 결과
	/// </summary>
	public class NiceCPCResponseData
	{
		/// <summary>
		/// 결과코드 result_code가 성공(0000)일 때만 전달
		/// </summary>		
		public string resultcode { get; set; }
		/// <summary>
		/// [필수] 서비스 요청 고유 번호
		/// </summary>		
		public string requestno { get; set; }
		/// <summary>
		/// 암호화 일시(YYYYMMDDHH24MISS)
		/// </summary>		
		public string enctime { get; set; }
		/// <summary>
		/// [필수] 암호화토큰요청 API에 응답받은 site_code
		/// </summary>		
		public string sitecode { get; set; }
		/// <summary>
		/// 응답고유번호
		/// </summary>		
		public string responseno { get; set; }
		/// <summary>
		/// 인증수단
		/// M	휴대폰인증		
		/// C 카드본인확인
		/// X 공동인증서
		/// F 금융인증서
		/// S PASS인증서
		/// </summary>		
		public string authtype { get; set; }
		/// <summary>
		/// 이름
		/// </summary>		
		public string name { get; set; }
		/// <summary>
		/// UTF8로 URLEncoding된 이름 값
		/// </summary>		
		public string utf8_name { get; set; }
		/// <summary>
		/// 생년월일 8자리
		/// </summary>		
		public string birthdate { get; set; }
		/// <summary>
		/// 성별 0:여성, 1:남성
		/// </summary>		
		public string gender { get; set; }
		/// <summary>
		/// 내외국인 0:내국인, 1:외국인
		/// </summary>		
		public string nationalinfo { get; set; }
		/// <summary>
		/// 이통사 구분(휴대폰 인증 시)
		/// 1	SK텔레콤		
		/// 2	KT		
		/// 3	LGU+		
		/// 5	SK텔레콤 알뜰폰		
		/// 6	KT 알뜰폰		
		/// 7	LGU+ 알뜰폰
		/// </summary>		
		public string mobileco { get; set; }
		/// <summary>
		/// 휴대폰 번호(휴대폰 인증 시)
		/// </summary>		
		public string mobileno { get; set; }
		/// <summary>
		/// 개인 식별 코드(CI)
		/// </summary>		
		public string ci { get; set; }
		/// <summary>
		/// 개인 식별 코드(di)
		/// </summary>		
		public string di { get; set; }
		/// <summary>
		/// 사업자번호(법인인증서 인증시)
		/// </summary>		
		public string businessno { get; set; }
		/// <summary>
		/// 인증 후 전달받을 데이터 세팅 (요청값 그대로 리턴)
		/// </summary>		
		public string receivedata { get; set; } = "";
	}

	public class NiceCryptoResponse
	{
		/// <summary>
		/// 사용한 토큰의 버전 아이디. 복호화시 필요한 키값
		/// </summary>
		public string TokenVersionId { get; set; }
		/// <summary>
		/// 암호화된 데이터
		/// </summary>
		public string EncData { get; set; }
		/// <summary>
		/// 암호화 무결성 값
		/// </summary>
		public string IntegrityValue { get; set; }
	}

	/// <summary>
	/// Nice 인증후 리턴 받는 URL의 Query 값
	/// </summary>
	public class NiceRequestCallBackModel
	{
		/// <summary>
		/// 인증성공시 이동할 경로
		/// </summary>
		public string NextAction { get; set; }
		public string NextController { get; set; }

		/// <summary>
		/// 룰렛 이밴트
		/// </summary>
		public string? BizCode { get; set; }
		public string? EventCode { get; set; }
		public string? Key { get; set; }

	}
	/// <summary>
	/// Nice에서 인증 완료 후 Callback 전달
	/// </summary>
	public class NiceResponseCallBackModel: NiceRequestCallBackModel
	{
        public string? enc_data { get; set; }
		public string? integrity_value { get; set; }
		public string? token_version_id { get; set; }
	}
	/// <summary>
	/// Nice 인증하기 위한 요청 암호 데이터 
	/// </summary>
	public class NiceRequestModel
	{
		public Uri NiceCheckUri { get; set; }

		
		public NiceCryptoResponse NiceRequest { get; set; }
	}

	public class NiceResponseStatusModel
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }

		public string? ActionUrl { get; set; }
		public Guid? CertID { get; set; }
	}

}
