using System.Text;

namespace Barunson.FamilyWeb.Service
{
	public static class Base64Convert
	{
		public static string Encoded(string data)
		{
			var byteArr = Encoding.UTF8.GetBytes(data);
			return Convert.ToBase64String(byteArr);
		}
		public static string Decoded(string data)
		{
			var byteArr = Convert.FromBase64String(data);
			return Encoding.UTF8.GetString(byteArr);
		}
	}
}
