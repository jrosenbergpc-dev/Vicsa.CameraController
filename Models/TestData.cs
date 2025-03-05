using System.Text.Json.Serialization;

namespace Aver.Visca.CameraController.Models
{
	public class TestData
	{
		[JsonPropertyName("data")]
		public string Data { get; set; }
	}
}
