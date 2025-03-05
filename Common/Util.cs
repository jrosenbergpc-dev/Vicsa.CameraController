namespace Aver.Visca.CameraController.Common
{
	public static class Util
	{
		public static byte ConvertToByteHex(int decimalValue)
		{
			if (decimalValue < 0 || decimalValue > 255)
			{
				throw new ArgumentOutOfRangeException(nameof(decimalValue), "Value must be between 0 and 255.");
			}

			// Return the decimal value as a byte
			return (byte)decimalValue;
		}
	}
}
