namespace Aver.Visca.CameraController.Common
{
	public class ViscaConnection
	{
		public string IP { get; set; } = "127.0.0.1";
		public int Port { get; set; }

		public ViscaConnection()
		{

		}

		public ViscaConnection(string ip, int port)
		{
			IP = ip;
			Port = port;
		}
	}
}
