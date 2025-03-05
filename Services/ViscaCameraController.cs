using Aver.Visca.CameraController.Common;

namespace Aver.Visca.CameraController.Services
{
	internal class ViscaCameraController
	{
		private UdpViscaClient _client = new UdpViscaClient();

		public int PanSpeed { get; private set; } = 0;
		public int TiltSpeed { get; private set; } = 0;
		public int ZoomSpeed { get; private set; } = 0;
		public bool IsConnected { get; private set; }

		public event EventHandler? CameraConnected;

		public ViscaCameraController()
		{
			_client.ConnectionStatusChanged += _client_ConnectionStatusChanged;
		}

		private void _client_ConnectionStatusChanged(object? sender, Tuple<ViscaHardwareStatus, string> e)
		{
			
		}

		public ViscaHardwareStatus Connect(ViscaConnection connection)
		{
			Console.WriteLine($">> [Attempting Connection to Camera with the following information.]{Environment.NewLine}===================================================================================={Environment.NewLine}[Address:{connection.IP}]{Environment.NewLine}[Port:{connection.Port}]{Environment.NewLine}Please wait...{Environment.NewLine}");

			ViscaHardwareStatus status = _client.Connect(connection);

			if (_client.IsConnected)
			{
				IsConnected = true;
				CameraConnected?.Invoke(this, new EventArgs());
			}

			return status;
		}

		public async Task TriggerPreset(int preset)
		{
			if (_client.IsConnected)
			{
				Console.WriteLine($">> [Triggering Preset: #{preset.ToString("00")}]");
				await _client.SendDataAsync(ViscaMessages.RecallPresetMessage(preset));
			}
		}

		public async Task SetPreset(int preset)
		{
			if (_client.IsConnected)
			{
				Console.WriteLine($">> [Saving Current Position to Preset: #{preset.ToString("00")}]");
				await _client.SendDataAsync(ViscaMessages.SetPresetMessage(preset));
			}
		}

		public async Task GoToHomePosition()
		{
			Console.WriteLine($">> [Requesting Camera to return to the Home Position.]");
			await _client.SendDataAsync(ViscaMessages.GoHomeMessage());
		}

		public async Task PowerOn()
		{
			if (_client.IsConnected)
			{
				Console.WriteLine($">> [Sending Power On Command to Camera...]");
				await _client.SendDataAsync(ViscaMessages.PowerOnMessage());
			}
		}

		public async Task PowerOff()
		{
			if (_client.IsConnected)
			{
				Console.WriteLine($">> [Sending Power Off Command to Camera...]");
				await _client.SendDataAsync(ViscaMessages.PowerOffMessage());
			}
		}

		public void Disconnect()
		{
			if (_client != null)
			{
				if (_client.IsConnected)
				{
					_client.Close();
				}
			}
		}
	}
}
