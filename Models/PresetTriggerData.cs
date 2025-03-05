using System.Text.Json.Serialization;

namespace Aver.Visca.CameraController.Models
{
	public class PresetTriggerData
	{
		[JsonPropertyName("ip")]
		public string IPAddress { get; set; } = "127.0.0.1";

		[JsonPropertyName("port")]
		public int Port { get; set; } = 52381;

		[JsonPropertyName("preset")]
		public int Preset { get; set; } = -1;
	}
}
