using Aver.Visca.CameraController;
using System.ServiceProcess;

public static class Program
{
	public static async Task Main(string[] args)
	{
		ServiceBase[] ServicesToRun;
		ServicesToRun = new ServiceBase[]
		{
				new Service()
		};
		ServiceBase.Run(ServicesToRun);
	}
}