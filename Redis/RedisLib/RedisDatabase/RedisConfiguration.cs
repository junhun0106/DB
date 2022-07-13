namespace RedisLib;

using System;
using System.Net.Security;
using System.Security.Authentication;
using StackExchange.Redis;
using StackExchange.Redis.Profiling;

class RedisHost
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
}

/// <summary>
/// The redis configuration
/// </summary>
class RedisConfiguration
{
    private ConfigurationOptions options;
    private string keyPrefix = string.Empty;
    private string password;
    private bool allowAdmin;
    private bool ssl;
    private int connectTimeout = 5000;
    private int syncTimeout = 1000;
    private bool abortOnConnectFail;
    private int database;
    private RedisHost[] hosts = Array.Empty<RedisHost>();
    private ServerEnumerationStrategy serverEnumerationStrategy = new();
    private uint maxValueLength;
    private int poolSize = 5;
    private string[] excludeCommands;
    private string configurationChannel;
    private string connectionString;
    private string serviceName;
    private SslProtocols? sslProtocols;
    private Func<ProfilingSession> profilingSessionProvider;
    private int workCount = Environment.ProcessorCount * 2;
    private ConnectionSelectionStrategy connectionSelectionStrategy = ConnectionSelectionStrategy.LeastLoaded;

    /// <summary>
    /// A RemoteCertificateValidationCallback delegate responsible for validating the certificate supplied by the remote party; note
    /// that this cannot be specified in the configuration-string.
    /// </summary>
    public event RemoteCertificateValidationCallback CertificateValidation;

    /// <summary>
    /// Indicate if the current configuration is the default;
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// The identifier name for the connection
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the every ConnectionSelectionStrategy to use during connection selection.
    /// </summary>
    public ConnectionSelectionStrategy ConnectionSelectionStrategy
    {
        get => this.connectionSelectionStrategy;

        set
        {
            this.connectionSelectionStrategy = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the every ConnectionMultiplexer SocketManager WorkCount
    /// </summary>
    public int WorkCount
    {
        get => this.workCount;

        set
        {
            this.workCount = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the servicename used in case of Sentinel.
    /// </summary>
    public string ServiceName
    {
        get => this.serviceName;

        set
        {
            this.serviceName = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets a value indicating whether get a boolean value that indicates if the cluster is configured for sentinel or not
    /// </summary>
    public bool IsSentinelCluster => !string.IsNullOrEmpty(this.ServiceName);

    /// <summary>
    /// Gets or sets the connection string. In wins over property configuration.
    /// </summary>
    public SslProtocols? SslProtocols
    {
        get => this.sslProtocols;

        set
        {
            this.sslProtocols = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the connection string. In wins over property configuration.
    /// </summary>
    public string ConnectionString
    {
        get => this.connectionString;

        set
        {
            this.connectionString = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the channel to use for broadcasting and listening for configuration change notification.
    /// </summary>
    public string ConfigurationChannel
    {
        get => this.configurationChannel;

        set
        {
            this.configurationChannel = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the key separation prefix used for all cache entries.
    /// </summary>
    public string KeyPrefix
    {
        get => this.keyPrefix;

        set
        {
            this.keyPrefix = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the redis password.
    /// </summary>
    public string Password
    {
        get => this.password;

        set
        {
            this.password = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether admin operations should be allowed.
    /// </summary>
    public bool AllowAdmin
    {
        get => this.allowAdmin;

        set
        {
            this.allowAdmin = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether specify if whether the connection should be encrypted.
    /// </summary>
    public bool Ssl
    {
        get => this.ssl;

        set
        {
            this.ssl = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the time in milliseconds that should be allowed for connection (defaults to 5 seconds unless SyncTimeout is higher).
    /// </summary>
    public int ConnectTimeout
    {
        get => this.connectTimeout;

        set
        {
            this.connectTimeout = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the time in milliseconds that the system should allow for synchronous operations (defaults to 5 seconds).
    /// </summary>
    public int SyncTimeout
    {
        get => this.syncTimeout;

        set
        {
            this.syncTimeout = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether connect/configuration timeouts should be explicitly notified via a TimeoutException.
    /// </summary>
    public bool AbortOnConnectFail
    {
        get => this.abortOnConnectFail;

        set
        {
            this.abortOnConnectFail = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets database Id.
    /// </summary>
    public int Database
    {
        get => this.database;

        set
        {
            this.database = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the host of Redis Servers (The ips or names).
    /// </summary>
    public RedisHost[] Hosts
    {
        get => this.hosts;

        set
        {
            this.hosts = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the strategy to use when executing server wide commands.
    /// </summary>
    public ServerEnumerationStrategy ServerEnumerationStrategy
    {
        get => this.serverEnumerationStrategy;

        set
        {
            this.serverEnumerationStrategy = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets maximal value length which can be set in database.
    /// </summary>
    public uint MaxValueLength
    {
        get => this.maxValueLength;

        set
        {
            this.maxValueLength = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets redis connections pool size.
    /// </summary>
    public int PoolSize
    {
        get => this.poolSize;

        set
        {
            this.poolSize = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets exclude commands.
    /// </summary>
    public string[] ExcludeCommands
    {
        get => this.excludeCommands;

        set
        {
            this.excludeCommands = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets redis Profiler to attach to ConnectionMultiplexer.
    /// </summary>
    public Func<ProfilingSession> ProfilingSessionProvider
    {
        get => this.profilingSessionProvider;

        set
        {
            this.profilingSessionProvider = value;
            this.ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets the Redis configuration options
    /// </summary>
    /// <value>An instanfe of <see cref="ConfigurationOptions" />.</value>
    public ConfigurationOptions ConfigurationOptions
    {
        get
        {
            if (this.options == null)
            {
                ConfigurationOptions newOptions;

                if (!string.IsNullOrEmpty(this.ConnectionString))
                {
                    newOptions = ConfigurationOptions.Parse(this.ConnectionString);
                }
                else
                {
                    newOptions = new()
                    {
                        Ssl = Ssl,
                        AllowAdmin = AllowAdmin,
                        Password = Password,
                        ConnectTimeout = ConnectTimeout,
                        SyncTimeout = SyncTimeout,
                        AbortOnConnectFail = AbortOnConnectFail,
                        ConfigurationChannel = ConfigurationChannel,
                        SslProtocols = sslProtocols,
                        ChannelPrefix = KeyPrefix
                    };

                    if (this.IsSentinelCluster)
                    {
                        newOptions.ServiceName = this.ServiceName;
                        newOptions.CommandMap = CommandMap.Sentinel;
                    }

                    foreach (var redisHost in this.Hosts)
                    {
                        newOptions.EndPoints.Add(redisHost.Host, redisHost.Port);
                    }
                }

                if (this.ExcludeCommands != null)
                {
                    newOptions.CommandMap = CommandMap.Create(
                        new(this.ExcludeCommands),
                        false);
                }

                if (this.WorkCount > 0)
                {
                    newOptions.SocketManager = new(this.GetType().Name, this.WorkCount);
                }

                newOptions.CertificateValidation += CertificateValidation;
                this.options = newOptions;
            }

            return this.options;
        }
    }

    private void ResetConfigurationOptions()
    {
        // this is needed in order to cover this scenario
        // https://github.com/imperugo/StackExchange.Redis.Extensions/issues/165
        this.options = null;
    }
}
