namespace PalworldServerManager.Core.ServerProcess;

public record ServerProcessStatus(bool IsRunning, int? ProcessId, DateTime? StartTimeUtc);
