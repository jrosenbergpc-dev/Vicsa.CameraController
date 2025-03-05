namespace Aver.Visca.CameraController.Models
{
	public class ValidViscaPayload<T>
	{
		public bool IsValid { get; set; }
		public T? JsonBody { get; set; }
		public int StatusCode { get; set; }
		public string? ErrorMessage { get; set; }
	}
}
