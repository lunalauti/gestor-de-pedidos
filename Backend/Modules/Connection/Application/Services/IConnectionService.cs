namespace Connection.Application.Services
{
    public interface IConnectionService
    {
        Task<bool> TestConnectionAsync();
        Task<ConnectionStatus> GetConnectionStatusAsync();
    }

    public class ConnectionStatus
    {
        public bool IsConnected { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
