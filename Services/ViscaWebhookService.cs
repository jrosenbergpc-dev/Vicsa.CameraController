using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Aver.Visca.CameraController.Models;
using Aver.Visca.CameraController.Services;
using Aver.Visca.CameraController.Common;
using System.Text;

public class ViscaWebhookService
{
	private readonly ILogger<ViscaWebhookService> _logger;
	private const string API_KEY = "ss1234"; // Store securely in real applications

	public ViscaWebhookService(ILogger<ViscaWebhookService> logger)
	{
		_logger = logger;
	}

	public async Task<bool> HasValidAPIKey(HttpContext context)
	{
		// Check if API key header exists and is correct
		if (!context.Request.Headers.TryGetValue("x-api-key", out var apiKey) || apiKey != API_KEY)
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;

			// Log received headers for debugging
			StringBuilder headersSent = new StringBuilder();
			foreach (var header in context.Request.Headers)
			{
				headersSent.Append($"{header.Key}: {header.Value}, ");
			}

			//this was added for testing remove for production do not want to leak out API key on a bad request haha
			//await context.Response.WriteAsync($"Unauthorized, headers received: {headersSent} trying to match APIKEY={API_KEY}");

			return false; // Exit function early if key is invalid
		}

		// API key is valid
		return true;
	}

	public async Task<ValidViscaPayload<T>> HasValidJsonBody<T>(HttpContext context)
	{
		ValidViscaPayload<T> payload = new ValidViscaPayload<T>();

		payload.JsonBody = await context.Request.ReadFromJsonAsync<T>();

		bool isValid = payload.JsonBody != null;

		payload.IsValid = isValid;

		if (!isValid)
		{
			payload.StatusCode = StatusCodes.Status400BadRequest;
			payload.ErrorMessage = "Invalid request payload.";
		}

		return payload;
	}

	public void RegisterRoutes(WebApplication app)
	{
		app.MapPost("/webhook", async (HttpContext context) =>
		{
			if (await HasValidAPIKey(context))
			{
				ValidViscaPayload<TestData> viscaPayload = await HasValidJsonBody<TestData>(context);

				if (viscaPayload.IsValid)
				{
					_logger.LogInformation("Received webhook: {Data}", JsonSerializer.Serialize(viscaPayload.JsonBody));

					await context.Response.WriteAsJsonAsync(new { Message = "Webhook received successfully data: " + viscaPayload.JsonBody });
				}
				else
				{
					context.Response.StatusCode = viscaPayload.StatusCode;
					await context.Response.WriteAsync(viscaPayload.ErrorMessage ?? "MAJOR ERROR");
					return;
				}
			}
		});

		app.MapPost("/visca/trigger/preset", async (HttpContext context) =>
		{
			if (await HasValidAPIKey(context))
			{
				ValidViscaPayload<PresetTriggerData> viscaPayload = await HasValidJsonBody<PresetTriggerData>(context);

				if (viscaPayload.IsValid)
				{
					Tuple<string, bool> result = await TriggerCameraPreset(viscaPayload.JsonBody);

					if (result.Item2)
					{
						// Respond with success
						context.Response.StatusCode = StatusCodes.Status200OK; // OK
						await context.Response.WriteAsync(result.Item1);
					}
					else
					{
						// Respond with failure
						context.Response.StatusCode = StatusCodes.Status417ExpectationFailed;
						await context.Response.WriteAsync($"Preset trigger request failed to process, Reason: {result.Item1}");
					}
				}
				else
				{
					context.Response.StatusCode = viscaPayload.StatusCode;
					await context.Response.WriteAsync(viscaPayload.ErrorMessage ?? "MAJOR PRESET ERROR");
					return;
				}
			}
		});

		app.MapPost("/visca/save/preset", async (HttpContext context) =>
		{
			if (await HasValidAPIKey(context))
			{
				ValidViscaPayload<PresetTriggerData> viscaPayload = await HasValidJsonBody<PresetTriggerData>(context);

				if (viscaPayload.IsValid)
				{
					Tuple<string, bool> result = await SaveCameraPreset(viscaPayload.JsonBody);

					if (result.Item2)
					{
						// Respond with success
						context.Response.StatusCode = StatusCodes.Status200OK; // OK
						await context.Response.WriteAsync(result.Item1);
					}
					else
					{
						// Respond with failure
						context.Response.StatusCode = StatusCodes.Status417ExpectationFailed;
						await context.Response.WriteAsync($"Save Preset request failed to process, Reason: {result.Item1}");
					}
				}
				else
				{
					context.Response.StatusCode = viscaPayload.StatusCode;
					await context.Response.WriteAsync(viscaPayload.ErrorMessage ?? "MAJOR PRESET ERROR");
					return;
				}
			}
		});
	}

	// Internal function to trigger camera preset
	private async Task<Tuple<string, bool>> TriggerCameraPreset(PresetTriggerData? data)
	{
		string returnMsg = string.Empty;
		bool bSuccess = false;
		// Here, we would call the ViscaCameraManager to send the trigger to the camera over UDP.
		ViscaCameraController cameraManager = new ViscaCameraController();
		ViscaHardwareStatus status = cameraManager.Connect(new ViscaConnection(data.IPAddress, data.Port));

		if (status == ViscaHardwareStatus.Connected && cameraManager.IsConnected)
		{
			await cameraManager.TriggerPreset(data.Preset);
			bSuccess = true;
			returnMsg = $"Camera preset {data.Preset} triggered on device {data.IPAddress}:{data.Port}";
			_logger.LogInformation("Camera preset {Preset} triggered on device {IPAddress}:{Port}", data.Preset, data.IPAddress, data.Port);
		}
		else if (status == ViscaHardwareStatus.ValidationFailed)
		{
			returnMsg = "Camera Validation Failed!";
			_logger.LogInformation("Camera Validation Failed!");
		}
		else if (status == ViscaHardwareStatus.NoResponse)
		{
			returnMsg = $"No Response from Camera at {data.IPAddress}:{data.Port}";
			_logger.LogInformation($"No Response from Camera at {data.IPAddress}:{data.Port}");
		}
		else if (status == ViscaHardwareStatus.Error)
		{
			returnMsg = "Error";
			_logger.LogInformation("Camera Trigger Preset ERROR");
		}
		else
		{
			_logger.LogInformation("Failed to connect to Camera!");
		}

		return new Tuple<string, bool>(returnMsg, bSuccess);
	}

	private async Task<Tuple<string,bool>> SaveCameraPreset(PresetTriggerData? data)
	{
		string returnMsg = string.Empty;
		bool bSuccess = false;

		ViscaCameraController cameraManager = new ViscaCameraController();
		ViscaHardwareStatus status = cameraManager.Connect(new ViscaConnection(data.IPAddress, data.Port));

		if (status == ViscaHardwareStatus.Connected && cameraManager.IsConnected)
		{
			await cameraManager.SetPreset(data.Preset);
			bSuccess = true;
			returnMsg = $"Camera preset {data.Preset} as been updated to new PTZ coordinates on device {data.IPAddress}:{data.Port}";
			_logger.LogInformation("Camera preset {Preset} triggered on {IPAddress}:{Port}", data.Preset, data.IPAddress, data.Port);
		}
		else if (status == ViscaHardwareStatus.ValidationFailed)
		{
			returnMsg = "Camera Validation Failed!";
			_logger.LogInformation("Camera Validation Failed!");
		}
		else if (status == ViscaHardwareStatus.NoResponse)
		{
			returnMsg = $"No Response from Camera at {data.IPAddress}:{data.Port}";
			_logger.LogInformation($"No Response from Camera at {data.IPAddress}:{data.Port}");
		}
		else if (status == ViscaHardwareStatus.Error)
		{
			returnMsg = "Error";
			_logger.LogInformation("Camera Save Preset ERROR");
		}
		else
		{
			_logger.LogInformation("Failed to connect to Camera!");
		}

		return new Tuple<string, bool>(returnMsg, bSuccess);
	}
}

