
namespace Aver.Visca.CameraController.Common
{
	public enum ViscaHardwareStatus
	{
		None = 0,
		SendingValidation,
		ValidationSent,
		ValidationReceived,
		Connected,
		ValidationFailed,
		NoResponse,
		Error
	}
}
