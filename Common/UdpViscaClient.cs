using System.Net.Sockets;
using System.Net;

namespace Aver.Visca.CameraController.Common
{
	internal class UdpViscaClient
	{
		private UdpClient? _udpClient;
		private string? _ipAddress;
		private int _port;
		private bool _isConnected;

		private ViscaHardwareStatus _currentStatus;
		private string? _statusString;

		public bool IsConnected => _isConnected;

		public event EventHandler<Tuple<ViscaHardwareStatus, string>>? ConnectionStatusChanged;

		public UdpViscaClient()
		{

		}

		public ViscaHardwareStatus Connect(ViscaConnection connection)
		{
			_udpClient = new UdpClient();
			_ipAddress = connection.IP;
			_port = connection.Port;
			_isConnected = false;

			Task.Run(() => CheckConnectionAsync()).Wait(); // Run sync in constructor

			return _currentStatus;
		}

		private Task SetCurrentStatus(ViscaHardwareStatus status, string optionalData = "")
		{
			_currentStatus = status;
			_statusString = optionalData;

			ConnectionStatusChanged?.Invoke(this, new Tuple<ViscaHardwareStatus, string>(status, optionalData));

			return Task.CompletedTask;
		}

		private async Task CheckConnectionAsync()
		{
			try
			{
				await SetCurrentStatus(ViscaHardwareStatus.SendingValidation);

				IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);

				await _udpClient.SendAsync(ViscaMessages.HardwarePingMessage(), ViscaMessages.HardwarePingMessage().Length, remoteEndPoint);

				await SetCurrentStatus(ViscaHardwareStatus.ValidationSent);

				// Set up receiver with a small timeout
				_udpClient.Client.ReceiveTimeout = 5000;
				var receiveTask = _udpClient.ReceiveAsync();
				if (await Task.WhenAny(receiveTask, Task.Delay(5000)) == receiveTask)
				{
					UdpReceiveResult result = receiveTask.Result;
					byte[] response = result.Buffer;

					await SetCurrentStatus(ViscaHardwareStatus.ValidationReceived, BitConverter.ToString(response));

					if (response.Length > 1 && (response[8] == 0x90 || response[8] == 0xA0)) // Check position of response address
					{
						_isConnected = true;
						await SetCurrentStatus(ViscaHardwareStatus.Connected);
						return;
					}
					else
					{
						await SetCurrentStatus(ViscaHardwareStatus.ValidationFailed, $"[Unexpected device response, Device Validation Failed. Sent: {BitConverter.ToString(ViscaMessages.HardwarePingMessage())} Received: {BitConverter.ToString(result.Buffer)}]");
						return;
					}
				}
				else
				{
					await SetCurrentStatus(ViscaHardwareStatus.NoResponse);
					return;
				}
			}
			catch (Exception ex)
			{
				await SetCurrentStatus(ViscaHardwareStatus.Error, ex.Message);
				return;
			}
		}

		public async Task SendDataAsync(byte[] data)
		{
			if (!_isConnected)
			{
				Console.WriteLine(">> [Cannot send data: No active connection.]");
				return;
			}

			try
			{
				IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
				await _udpClient.SendAsync(data, data.Length, remoteEndPoint);
				Console.WriteLine(">> [Command sent successfully.]");
			}
			catch (Exception ex)
			{
				Console.WriteLine($">> [Error sending command: {ex.Message}]");
			}
		}

		public void Close()
		{
			_udpClient.Close();
			_isConnected = false;
			Console.WriteLine(">> [UDP client closed.]");
		}
	}
}
