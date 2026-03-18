using System;
using System.Collections.Generic;

namespace StreamingPlatform.Services
{
    public sealed class PlatformManager
    {
        private static readonly Lazy<PlatformManager> _instance =
            new Lazy<PlatformManager>(() => new PlatformManager());

           public static PlatformManager Instance => _instance.Value;

        private PlatformManager()
        {
            _startTime = DateTime.Now;
            _connectionId = Guid.NewGuid().ToString().Substring(0, 8);
            _isConnected = true;
            _actionLog = new List<string>();
            _config = new Dictionary<string, string>
            {
                { "MaxConcurrentStreams", "4" },
                { "DefaultQuality", "1080p" },
                { "Language", "ro-RO" },
                { "Region", "Moldova" },
                { "AutoPlay", "true" }
            };

            Log("PlatformManager inițializat. Conexiune stabilită.");
        }

        private readonly string _connectionId;
        private bool _isConnected;
        private readonly DateTime _startTime;

        public string ConnectionId => _connectionId;
        public bool IsConnected => _isConnected;
        public TimeSpan Uptime => DateTime.Now - _startTime;

        public void Connect()
        {
            if (!_isConnected)
            {
                _isConnected = true;
                Log("Conexiune restabilită.");
            }
        }

        public void Disconnect()
        {
            if (_isConnected)
            {
                _isConnected = false;
                Log("Conexiune închisă.");
            }
        }

        private readonly Dictionary<string, string> _config;

        public string GetConfig(string key)
        {
            return _config.ContainsKey(key) ? _config[key] : null;
        }

        public void SetConfig(string key, string value)
        {
            _config[key] = value;
            Log($"Configurație actualizată: {key} = {value}");
        }

        public IReadOnlyDictionary<string, string> GetAllConfig()
        {
            return _config;
        }

        private readonly List<string> _actionLog;
        private readonly object _logLock = new object();

        public void Log(string action)
        {
            lock (_logLock)  
            {
                string entry = $"[{DateTime.Now:HH:mm:ss}] {action}";
                _actionLog.Add(entry);
            }
        }

        public IReadOnlyList<string> GetLog()
        {
            lock (_logLock)
            {
                return _actionLog.AsReadOnly();
            }
        }

        public int GetLogCount()
        {
            lock (_logLock)
            {
                return _actionLog.Count;
            }
        }

        private int _totalStreams;
        private int _totalUsers;
        private readonly object _statsLock = new object();

        public void IncrementStreams()
        {
            lock (_statsLock)
            {
                _totalStreams++;
                Log($"Stream nou. Total: {_totalStreams}");
            }
        }

        public void IncrementUsers()
        {
            lock (_statsLock)
            {
                _totalUsers++;
                Log($"Utilizator nou. Total: {_totalUsers}");
            }
        }

        public int TotalStreams
        {
            get { lock (_statsLock) { return _totalStreams; } }
        }

        public int TotalUsers
        {
            get { lock (_statsLock) { return _totalUsers; } }
        }

        public string GetStatus()
        {
            return $"PlatformManager Status:\n" +
                   $"  Connection ID: {_connectionId}\n" +
                   $"  Conectat: {(_isConnected ? "Da" : "Nu")}\n" +
                   $"  Uptime: {Uptime.TotalSeconds:F0} secunde\n" +
                   $"  Total stream-uri: {TotalStreams}\n" +
                   $"  Total utilizatori: {TotalUsers}\n" +
                   $"  Acțiuni loggate: {GetLogCount()}\n" +
                   $"  Configurații: {_config.Count}";
        }
    }
}
