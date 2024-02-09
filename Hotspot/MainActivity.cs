using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Net.Wifi;
using Android.Net.Wifi.P2p;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace Hotspot
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TextView hotspotCountView, status;
        Button refreshButton;
        private int count { get; set; }
        private int CountDevices { get { return count; } set { if (value < 0) { count = 0; } } }
        private int percentage { get; set; }
        private List<string> ips = new List<string> { "192.168.219.", "192.168.43." };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            hotspotCountView = FindViewById<TextView>(Resource.Id.hotspotCount);
            status = FindViewById<TextView>(Resource.Id.statusText);
            refreshButton = FindViewById<Button>(Resource.Id.refreshButton);
            refreshButton.Clickable = false;
            Load();
            hotspotCountView.Text = "Connected devices: " + CountDevices;
            refreshButton.Click += (sender, e) => {
                Refresh();
            };

        }
        private async void Load()
        {
            await getCount();
        }
        private async Task getCount()
        {
            CountDevices = 0;
            CountDevices = await ScanForConnectedDevicesAsync() - 1;
            hotspotCountView.Text = $"Connected devices: {CountDevices}";
            status.Text = "Status: Completed";
            refreshButton.Clickable = true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public async void Refresh()
        {
            refreshButton.Clickable = false;
            hotspotCountView.Text = "Connected devices: 0";
            status.Text = "Status: Waiting...";
            await getCount();
        }


        public async Task<int> ScanForConnectedDevicesAsync()
        {
            int count = 0;
            percentage = 0;
            foreach (string currentIp in ips)
            {
                List<Task<bool>> tasks = new List<Task<bool>>();
                for (int i = 1; i <= 254; i++)
                {
                    string ip = currentIp + i.ToString();
                    tasks.Add(PingDeviceAsync(ip));
                }

                var results = await Task.WhenAll(tasks);
                count += results.Count(result => result);
            }
            return count;
        }

        private async Task<bool> PingDeviceAsync(string ipAddress)
        {
            using (var ping = new System.Net.NetworkInformation.Ping())
            {
                try
                {
                    var reply = await ping.SendPingAsync(ipAddress, 1000); 
                                                                          
                    Console.WriteLine($"Ping to {ipAddress} {(reply.Status == System.Net.NetworkInformation.IPStatus.Success ? "successful" : "failed")}.");

                    status.Text = "Status: Waiting... " + (Math.Round(percentage / Convert.ToDouble(254 * ips.Count), 2) * 100).ToString() + '%';
                    percentage++;
                    return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
                }
                catch
                {
                    return false;
                }
            }
        }

    }
}
