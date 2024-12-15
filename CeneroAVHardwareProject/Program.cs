using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace AVDeviceMonitor
{
    class Program
    {
        // HttpClient used for API requests
        private static readonly HttpClient httpClient = new HttpClient();
        // Max retries for failed connection attempts
        private static readonly int maxRetries = 3;
        // Delay in milliseconds before retrying a failed connection
        private static readonly int retryDelay = 2000;

        static async Task Main(string[] args)
        {
            // Starting message for the AV Device Monitor
            Console.WriteLine("Starting AV Device Monitor...");

            // Retrieve and display device configuration on startup
            await RetrieveDeviceConfiguration();

            // Test connectivity to the HaveIBeenPwned API and store the result
            bool isConnected = await TestApiConnectivity();

            if (isConnected)
            {
                // Successful connection to the API
                Console.WriteLine("[SUCCESS] Connected to the API.");

                // Start heartbeat monitoring in a separate task
                Console.WriteLine("Starting heartbeat monitoring...");
                Task.Run(StartHeartbeatMonitoring);

                // Start real-time status monitoring
                Console.WriteLine("Starting real-time status monitoring...");
                await StartRealTimeStatusMonitoring();
            }
            else
            {
                // Failed to connect to the API
                Console.WriteLine("[ERROR] Failed to connect to the API.");
            }
        }

        // Method to test connectivity to the API with retry logic
        private static async Task<bool> TestApiConnectivity()
        {
            int retries = 0;
            while (retries < maxRetries)
            {
                try
                {
                    // Send a GET request to the API
                    string apiUrl = "https://haveibeenpwned.com/api/v2";
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    // Check if the API request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Response: {response.StatusCode}");
                        return true;
                    }
                    else
                    {
                        // Log non-successful status code from the API
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return false;
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // Handle HTTP request errors (e.g., connection issues)
                    Console.WriteLine($"[ERROR] HTTP Request Error: {httpEx.Message}");
                    retries++;
                }
                catch (TimeoutException timeoutEx)
                {
                    // Handle timeout errors
                    Console.WriteLine($"[ERROR] Timeout Error: {timeoutEx.Message}");
                    retries++;
                }
                catch (SocketException socketEx)
                {
                    // Handle socket errors (e.g., no network connection)
                    Console.WriteLine($"[ERROR] Socket Error: {socketEx.Message}");
                    retries++;
                }
                catch (Exception ex)
                {
                    // Catch other general exceptions
                    Console.WriteLine($"[ERROR] General Error: {ex.Message}");
                    retries++;
                }

                // Retry after a delay if connection failed
                if (retries < maxRetries)
                {
                    Console.WriteLine("[INFO] Retrying connection...");
                    await Task.Delay(retryDelay);
                }
            }

            // Return false if after all retries, connection still fails
            return false;
        }

        // Method to continuously monitor the device's heartbeat (status check every 5 minutes)
        private static async Task StartHeartbeatMonitoring()
        {
            const int heartbeatInterval = 300000; // 5 minutes in milliseconds

            while (true)
            {
                try
                {
                    // Send heartbeat (status check) to the API
                    Console.WriteLine($"[{DateTime.Now}] Sending heartbeat...");
                    bool isOnline = await TestApiConnectivity();

                    // Display the status based on the heartbeat result
                    if (isOnline)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;  // Green for Online
                        Console.WriteLine($"[{DateTime.Now}] Status: ONLINE");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;  // Red for Offline
                        Console.WriteLine($"[{DateTime.Now}] Status: OFFLINE");
                    }
                }
                catch (Exception ex)
                {
                    // Handle errors during heartbeat monitoring
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] Error during heartbeat monitoring: {ex.Message}");
                }
                finally
                {
                    // Reset console text color after each loop iteration
                    Console.ResetColor();
                }

                // Wait for the heartbeat interval before checking again
                await Task.Delay(heartbeatInterval);
            }
        }

        // Method to retrieve and display the device configuration (e.g., IP, MAC, Serial Number)
        private static async Task RetrieveDeviceConfiguration()
        {
            try
            {
                // Log message for configuration retrieval
                Console.WriteLine("Retrieving device configuration...");

                // Simulate fetching local device information (e.g., IP Address, MAC Address)
                string ipAddress = GetLocalIPAddress();
                string macAddress = GetMacAddress();
                string serialNumber = "SN123456789"; // Simulated serial number for the device

                // Display the retrieved device configuration details
                Console.WriteLine("Device Configuration:");
                Console.WriteLine($"IP Address: {ipAddress}");
                Console.WriteLine($"MAC Address: {macAddress}");
                Console.WriteLine($"Serial Number: {serialNumber}");

                // Simulate a delay for device configuration retrieval
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                // Handle any errors during configuration retrieval
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Failed to retrieve device configuration: {ex.Message}");
                Console.ResetColor();
            }
        }

        // Method to continuously monitor the device's real-time status (status check every 10 seconds)
        private static async Task StartRealTimeStatusMonitoring()
        {
            const int statusCheckInterval = 10000; // 10 seconds in milliseconds
            string testEmail = "test@example.com"; // Test email used to simulate real-time status check

            while (true)
            {
                try
                {
                    // Display current time when checking the real-time status
                    Console.WriteLine($"[{DateTime.Now}] Checking real-time status...");

                    // API endpoint to check if the email has been breached
                    string apiUrl = $"https://haveibeenpwned.com/api/v2/breachedaccount/{testEmail}";
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    // Process the response and display status
                    if (response.IsSuccessStatusCode)
                    {
                        // Green color for detected breach
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[{DateTime.Now}] Status: Breached Account Detected for {testEmail}!");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Yellow color for no breach detected
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[{DateTime.Now}] Status: No Breach Detected for {testEmail}.");
                    }
                    else
                    {
                        // Red color for other types of errors or unknown status
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{DateTime.Now}] Status: Unable to Determine. Response Code: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // Handle HTTP request errors (e.g., connectivity issues)
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] HTTP Request Error: {httpEx.Message}");
                }
                catch (TimeoutException timeoutEx)
                {
                    // Handle timeout errors when the API takes too long to respond
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] Timeout Error: {timeoutEx.Message}");
                }
                catch (Exception ex)
                {
                    // Catch all general errors during real-time status monitoring
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] Error during real-time status monitoring: {ex.Message}");
                }
                finally
                {
                    // Reset color to default after processing each status check
                    Console.ResetColor();
                }

                // Wait for the status check interval before checking again
                await Task.Delay(statusCheckInterval);
            }
        }

        // Method to retrieve the local machine's IP address
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // Return the first valid IPv4 address
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "Not found"; // Return a placeholder if no valid IP is found
        }

        // Method to retrieve the local machine's MAC address
        private static string GetMacAddress()
        {
            var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces)
            {
                // Check if the network interface is up and return the MAC address
                if (networkInterface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                {
                    return networkInterface.GetPhysicalAddress().ToString();
                }
            }
            return "Not found"; // Return a placeholder if no MAC address is found
        }
    }
}
