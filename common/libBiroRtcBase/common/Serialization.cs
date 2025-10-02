using Newtonsoft.Json;

namespace si.birokrat.rtc.common {
	public static class Serialization {
		#region -- settings --
		private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() {
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
			DateTimeZoneHandling = DateTimeZoneHandling.Utc
		};
		#endregion
		#region -- methods --
		public static string Serialize(object obj) =>
			JsonConvert.SerializeObject(obj, jsonSerializerSettings);
		public static T Deserialize<T>(string json) =>
			JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
		#endregion
	}
}
