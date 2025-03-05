namespace Aver.Visca.CameraController.Common
{
	public static class ViscaMessages
	{
		private static readonly byte[] UDPHeader = new byte[] { 0x01, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00 };

		public static byte[] HardwarePingMessage()
		{
			return UDPHeader.Concat(new byte[] { 0x81, 0x01, 0x00, 0x01, 0xFF }).ToArray();
		}

		public static byte[] GoHomeMessage()
		{
			return UDPHeader.Concat(new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x02, 0x00, 0xFF }).ToArray();
		}

		public static byte[] RecallPresetMessage(int preset)
		{
			return UDPHeader.Concat(new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x02, Util.ConvertToByteHex(preset), 0xFF }).ToArray();
		}

		public static byte[] SetPresetMessage(int preset)
		{
			return UDPHeader.Concat(new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x01, Util.ConvertToByteHex(preset), 0xFF }).ToArray();
		}

		public static byte[] PowerOnMessage()
		{
			return UDPHeader.Concat(new byte[] { 0x81, 0x01, 0x04, 0x00, 0x02, 0xFF }).ToArray();
		}

		public static byte[] PowerOffMessage()
		{
			return UDPHeader.Concat(new byte[] { 0x81, 0x01, 0x04, 0x00, 0x03, 0xFF }).ToArray();
		}
	}
}
