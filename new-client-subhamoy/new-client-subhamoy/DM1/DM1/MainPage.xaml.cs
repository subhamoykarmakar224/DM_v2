using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Net.Http;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using Plugin.BLE;
using Plugin.Connectivity;
using Newtonsoft.Json.Linq;
//using CoreLocation;
//using Foundation;
using System.Net.NetworkInformation;
//using Android.Hardware;
//using Java.Util;
//using Android.Provider;
using DM1.utils;


namespace DM1
{
    public partial class MainPage : ContentPage
    {
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        //CLLocationManager locationManager;
        //NSUuid beaconUUID;
        //CLBeaconRegion beaconRegion;

        public MainPage()
        {
            InitializeComponent();

            InitializeParameter();
            ButtonStop.IsEnabled = false;
            if (Device.RuntimePlatform == "Android")
            {
                ble = CrossBluetoothLE.Current;
                adapter = ble.Adapter;
                deviceList = new ObservableCollection<IDevice>();
            }
            else
            {
                // locationManager = new CLLocationManager();
                // locationManager.RequestWhenInUseAuthorization();
            }
        }

        public static void InitializeParameter()
        {
            Globals.starttime = DateTime.Now;
        }

        private async void OnButtonClickedRunDataCollection(object sender, EventArgs e)
        {
            if (SensorID.Text == null)
            {
                if (DeviceInfo.Name.ToString() != "")
                {
                    SensorID.Text = DeviceInfo.Name.ToString();
                }
                else
                {
                    Statuslabel.Text = "Plesae Enter a Valid email or Semantic Entity ID";
                    return;
                }
            }
            Button button = (Button)sender;
            ButtonRun.IsEnabled = false;
            ButtonStop.IsEnabled = true;
            Status1label.Text = "Bluetooth Scanning Started";
            GetObservations();

            //  var state = ble.State;



            // if (!(ble.IsOn))
            //  {
            //      BluetoothStatuslabel.Text = "Turn on your Bluetooth";
            //      BluetoothStatuslabel.TextColor = Xamarin.Forms.Color.Red;
            //      return;
            //  }

            //  BluetoothStatuslabel.Text = "Bluetooth Status: " + state;

            // deviceList.Clear();
            if (Device.RuntimePlatform == "iOS")
            {
                /*
                GetGPS();
               
                 

                try
                {
                    locationManager = new CLLocationManager();
                    locationManager.RequestWhenInUseAuthorization();
                    beaconUUID = new NSUuid(Globals.UUID);
                    beaconRegion = new CLBeaconRegion(beaconUUID, Globals.beaconID);

                    locationManager.DidRangeBeacons += (object sender1, CLRegionBeaconsRangedEventArgs e1) =>
                    {
                        BluetoothStatuslabel.Text = "We found " + e1.Beacons.Length + " beacons";
                        if (e1.Beacons.Length > 0)
                        {
                            for (var i = 0; i < e1.Beacons.Length; i++)
                            {
                                CLBeacon beacon = e1.Beacons[i];

                                var major = (int)beacon.Major;
                                var minor = (int)beacon.Minor;
                                SendLabel.Text = "Major: " + major.ToString() + "_Minor: " + minor.ToString();
                                DeviceMaclabel.Text = major.ToString() + "_" + minor.ToString();
                                DeviceNamelabel.Text = DeviceInfo.Name.ToString();
                                BluetoothDiscovered();

                                Globals.strRSSI = beacon.Rssi.ToString();
                                Globals.strAdv = "";
                                Globals.strNative = "";
                                Globals.strMacAddy = beacon.Description.ToString();
                                FoundOther();
                            }
                        }



                    };
                    locationManager.StartRangingBeacons(beaconRegion);
                }
                catch
                {
                    Status1label.Text = "Error on IOS beacon parse";
                }*/
            }
            else
            {
                adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.LowLatency;
                //  string SECURE_SETTINGS_BLUETOOTH_ADDRESS = "bluetooth_address";


                //Globals.strMacAddy = Plugin.BLE.CrossBluetoothLE.Current.Adapter.ToString();

                //This used to work, new API just returns: "02:00:00:00:00:00"
                //Privacy preservation?
                // BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                // string strMacAddy1 = mBluetoothAdapter.Address;

                // I cannot get permissions for this one, either
                // var linkCode = Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, "bluetooth_address");

                //   No permission for this either, eveon though I used READ_PRIVILEGED_PHONE_STATE
                //   var strSerialNumber = Build.GetSerial();


                // Get a unique ID here, becuase bluettoh MAC addy is not available
                //  try
                //   {
                //       var strNumber1 = Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                //       if (strNumber1 != "" && Globals.strMacAddy == "")
                //        {
                //           Globals.strMacAddy = strNumber1;
                //       }

                //     var strNumber = Build.Serial.ToString();
                //      if (strNumber != "" && Globals.strMacAddy == "")
                //      {
                //          Globals.strMacAddy = strNumber;
                //      }
                //  }
                //  catch
                //      {
                //Something wet wrong with ID, pull some shit out here
                //      var rand = new Random();
                //      Globals.strMacAddy = (rand.Next(101).ToString() + "User1@tippers.edu");
                //      }

                //Get MAC addy from the WiFi
                //   string temp = getMacAddr();

                await StartSeach();

                Device.StartTimer(TimeSpan.FromSeconds(5), () =>
                {
                    StartSeach();
                    return true;

                });
                // Status1label.Text = uriEntry.Text; 
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;


            SensorID.IsEnabled = true;
            //TypeID.IsEnabled = true;


        }
        
        async void ButtonMessage_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MessagePage());
            // {
            //  BindingContext = new Note()
            //  });

        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                Globals.URI_NAME = (string)picker.ItemsSource[selectedIndex];
                Globals.URI_SENSOR = (string)picker.ItemsSource[selectedIndex];
                Status1label.Text = "uri: " + (string)picker.ItemsSource[selectedIndex]; ;
            }
        }


        private void OnButtonClickedStopDataCollection(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            adapter.StopScanningForDevicesAsync();
            Status1label.Text = "Bluetooth Scanning Stopped";
            ButtonRun.IsEnabled = true;
            ButtonStop.IsEnabled = false;
        }
        
        private void Entry_Completed(object sender, EventArgs e)
        {

            Globals.strMacAddy = SensorID.Text;
            SensorID.IsEnabled = false;
            //TypeID.IsEnabled = false;
        }
        
        async void GetObservations()// ihe: to modify
        {
            try
            {
                HttpClient client;
                client = new HttpClient();
                var uri = Globals.URI_SENSOR + "/observationhandler/observations/testconnection";
                string response = await client.GetStringAsync(uri);
                Statuslabel.Text = "Connected to Servers!" + uri;
            }
            catch
            {
                Statuslabel.Text = "System.IO exception error connecting to:" + Globals.URI_SENSOR;
            }
        }
        
        async void BluetoothDiscovered()// ihe: to modify
        {
            using (var client = new HttpClient())

            {
                try
                {
                    // send a GET request
                    var uri = getURISendPacket();
                    var json = makeBeaconRecord();

                    if (ConstructPacket(json) == 1)//construct a packet successfully
                    {
                        //  send a POST request                      
                        SendLabel.Text = "Send: 5";

                        //Do I have networking?
                        if (DoIHaveInternet())
                        {
                            SendLabel.Text = "Connect to network!";
                            //send current packet to server

                            //first send all the packets in buffer in order
                            Buffer2ServerBeacon();

                            //after that, send current packet
                            sendPacket(Globals.packet);
                        }
                        else
                        {
                            StoreInBufferBeacon();
                        }

                        resetPacket();
                    }

                }
                catch
                {
                    Statuslabel.Text = "System.IO error (Bluetooth) connecting to Server";
                }
            }

        }
        
        private async Task StartSeach()
        {
            await adapter.StartScanningForDevicesAsync();
            adapter.DeviceDiscovered += async (s, a) =>
            {
                try
                {
                    Globals.strAdv = "";
                    var junk = a.Device.AdvertisementRecords.ToArray();
                    for (var i = 0; i <= (a.Device.AdvertisementRecords.Count - 1); i++)
                    {
                        Globals.strAdv = Globals.strAdv + "     " + junk[i].ToString();
                    }
                    GetGPS();
                    deviceList.Add(a.Device);
                    Status1label.Text = "Found device: " + a.Device.Name + ' ' + a.Device.Id;
                    DeviceMaclabel.Text = a.Device.Id.ToString();
                    Globals.strMacAddy = a.Device.Id.ToString();
                    Globals.strNative = a.Device.NativeDevice.ToString();
                    Globals.strRSSI = a.Device.Rssi.ToString();

                    DeviceNamelabel.Text = DeviceInfo.Name.ToString();

                    FoundOther();

                    if ((a.Device.Name != null && a.Device.AdvertisementRecords.Count == 2) || (a.Device.Name.ToString().Contains("CBPeripheral")))
                    {
                        var temp1 = a.Device.AdvertisementRecords;
                        //  string strMajorMinor = GetMajorMinor(a.Device.AdvertisementRecords);
                        var myarray = a.Device.AdvertisementRecords.ToArray();
                        var major = myarray[1];
                        var major1 = major.ToString();
                        string[] words = major1.Split('-');
                        int intCnt = words.Count();
                        int intMajor1 = Convert.ToInt32(((words[intCnt - 5]) + (words[intCnt - 4])), 16);
                        string strMajor = intMajor1.ToString();

                        int intMinor1 = Convert.ToInt32(((words[intCnt - 3]) + (words[intCnt - 2])), 16);
                        string strMinor = intMinor1.ToString();
                        DeviceMaclabel.Text = strMajor + '_' + strMinor;
                        BluetoothDiscovered();
                    }
                }
                catch
                {
                    BluetoothStatuslabel.Text = "Could not parse advertisement";
                }
            };
        }
        
        public bool DoIHaveInternet()
        {
            return CrossConnectivity.Current.IsConnected;
            //return true;
        }
        
        public static String getMacAddr()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.Name.Equals("en0") || networkInterface.Name.Equals("wlan0"))
                {
                    var hw = networkInterface.GetPhysicalAddress();
                    return string.Join(":", (from ma in hw.GetAddressBytes() select ma.ToString("X2")).ToArray());
                }

            }
            return " 02:00:00:00:00:00";
        }

        void FoundOther()// ihe: to modify
        {
                    // send a GET request  
              var json = makeBluetoothRecord();
              if (ConstructPacket(json) == 1)//construct a packet successfully
              {
                   //  send a POST request                      
                   SendLabel.Text = "Send: 7";
                   //Do I have networking?
                   if (DoIHaveInternet())
                   {
                        SendLabel.Text = "Connect to network!";
                        Buffer2ServerBluetooth();
                        sendPacket(Globals.packet);
                   }
                   else
                   {
                        StoreInBufferBluetooth();
                   }
                   resetPacket();
              }
        }

        async void GetGPS()
        {
            bool badGPS = false;
            //get some location data here

            try
            {
                var location2 = await Geolocation.GetLastKnownLocationAsync();
                if (location2 != null)
                {
                    Globals.strLat = "" + location2.Latitude;
                    Globals.strLon = "" + location2.Longitude;
                }
            }
            catch
            {
                BluetoothStatuslabel.Text = "Problem getting GPS";
                badGPS = true;
            }

            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    Globals.strLat = "" + location.Latitude.ToString();
                    Globals.strLon = "" + location.Longitude.ToString();
                }
                else
                {
                    badGPS = true;
                }
            }
            catch
            {
                BluetoothStatuslabel.Text = "Problem getting GPS";
                badGPS = true;
            }
            if (badGPS)
            {
                BluetoothStatuslabel.Text = "Try to get last known location";
                try
                {
                    var location1 = await Geolocation.GetLastKnownLocationAsync();
                    if (location1 != null)
                    {
                        Globals.strLat = "" + location1.Latitude;
                        Globals.strLon = "" + location1.Longitude;
                    }
                }
                catch (Exception)
                {
                    BluetoothStatuslabel.Text = "Cannot get Last Known location";
                }
            }

        }

        String getURISendPacket()
        {
            var uri = Globals.URI_NAME + "/observationhandler/observations/add";
            return uri;
        }

        int ConstructPacket(String json)//add records into a packet, stops when reaching the RecordNum limit, 1 means successfully make up a packet
        {
            int flag = 0;
            Globals.packet += json;
            Globals.RecordCount++;

            if (Globals.RecordCount == Globals.RecordNumPacket)//successsful
            {
                flag = 1;
                //merge other information to construct a packet json
                string jsonPacket = Globals.packet;
                jsonPacket = "[" + jsonPacket + "],";
                jsonPacket = "\"observations\":" + jsonPacket;
                jsonPacket += "\"packet_id\":" + Globals.packetID  + ",";
                if (Globals.first_packet)
                {
                    jsonPacket += "\"first_packet\":true" + ",";
                }
                else
                {
                    jsonPacket += "\"first_packet\":false" + ",";
                }
                Globals.delivery_type = generateDeliveryType();
                jsonPacket += "\"delivery_type\":" + Globals.delivery_type + ",";
                jsonPacket += "\"device_id\":" + "\"" + DeviceMaclabel.Text + "\"" + ",";//device_id
                jsonPacket += "\"timestamp\":" + "\"" + DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss") + "\"";//timestamp
                jsonPacket = "{" + jsonPacket + "}";
                Globals.packet = jsonPacket;
                Globals.packetID ++;
                SharedPreferenceController.sharedPrefAddValue(Globals.KEY_SP_PACKEY_ID, Globals.packetID);
            }
            else
            {
                Globals.packet += ",";
            }

            Globals.first_packet = false;
            //ihe: add time-out later
            return flag;
        }

        void resetPacket()//reset a packet to be null
        {
            Globals.packet = "";
            Globals.RecordCount = 0;
        }

        String makeBeaconRecord()//construct a beacon record to json
        {
            var newPost = new BlueTooth
            {
                //  sensor_id = "BO_Bluetooth_Reading",
                sensor_id = DeviceMaclabel.Text,
                type = "5",
                timestamp = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
                name = DeviceNamelabel.Text,
                mac = SensorID.Text
            };

            //  latlabel.Text = "made it to json newpost in Gyro";
            var json2 = "[{\"sensor_id\":" + "\"" + newPost.sensor_id + "\"" + ",";
            json2 = json2 + "\"type\":" + "\"" + newPost.type + "\"" + ",";
            json2 = json2 + "\"timestamp\":" + "\"" + newPost.timestamp + "\"" + ",";
            json2 = json2 + "\"payload\":{\"name\":" + "\"" + newPost.name + "\"" + ",";
            json2 = json2 + "\"client_id\":" + "\"" + newPost.mac + "\"";
            json2 = json2 + "}}]";

            return json2;
        }

        String makeBluetoothRecord()//construct a general bluetooth record to json
        {
            var newPost = new BluetoothOther
            {
                clientID = SensorID.Text,//correspond to sensor_id in tippers/observation
                name = DeviceNamelabel.Text,
                type = "7",
                timestamp = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),

                id = Globals.strMacAddy,
                native = Globals.strNative,
                rssi = Globals.strRSSI,
                lat = Globals.strLat,
                lon = Globals.strLon,
                advRecord = Globals.strAdv
            };

            //  latlabel.Text = "made it to json newpost in Gyro";
            var json2 = "{\"sensor_id\":" + "\"" + newPost.clientID + "\"" + ",";
            json2 = json2 + "\"name\":" + "\"" + newPost.name + "\"" + ",";
            json2 = json2 + "\"type\":" + "\"" + newPost.type + "\"" + ",";
            json2 = json2 + "\"timestamp\":" + "\"" + newPost.timestamp + "\"" + ",";
            json2 = json2 + "\"payload\":{\"id\":" + "\"" + newPost.id + "\"" + ",";
            json2 = json2 + "\"native\":" + "\"" + newPost.native + "\"" + ",";
            json2 = json2 + "\"rssi\":" + "\"" + newPost.rssi + "\"" + ",";
            json2 = json2 + "\"lat\":" + "\"" + newPost.lat + "\"" + ",";
            json2 = json2 + "\"lon\":" + "\"" + newPost.lon + "\"" + ",";
            json2 = json2 + "\"advrecord\":" + "\"" + newPost.advRecord + "\"";
            json2 = json2 + "}}";

            return json2;
        }

        async void Buffer2ServerBeacon()//send beacon packets in buffer to server
        {//notice the async type here: to see if it can cause error
            string uri = getURISendPacket();
            var client = new HttpClient();
            int begin, end;

            if (Globals.beacontail == Globals.beaconhead)
            {
                return;
            }

            if (Globals.beacontail < Globals.beaconhead)
            {
                begin = Globals.beacontail;
                end = Globals.beaconhead;
                for (int i = begin; i < end; i++)
                {
                    sendPacket(Globals.strBeaconArray[i]);
                    //maintain variables
                    Globals.ifOccupiedBeacon[i] = false;
                    Globals.OccupiedBeaconNum -= 1;
                    Globals.beacontail = i + 1;
                    if (Globals.beacontail == 1000)
                    {
                        Globals.beacontail = 0;
                    }
                }
            }
            else
            {
                begin = Globals.beacontail;
                end = 1000;

                for (int i = begin; i < end; i++)
                {
                    sendPacket(Globals.strBeaconArray[i]);

                    //maintain variables
                    Globals.ifOccupiedBeacon[i] = false;
                    Globals.OccupiedBeaconNum -= 1;
                    Globals.beacontail = i + 1;
                    if (Globals.beacontail == 1000)
                    {
                        Globals.beacontail = 0;
                    }
                }

                begin = 0;
                end = Globals.beaconhead;

                for (int i = begin; i < end; i++)
                {
                    sendPacket(Globals.strBeaconArray[i]);

                    //maintain variables
                    Globals.ifOccupiedBeacon[i] = false;
                    Globals.OccupiedBeaconNum -= 1;
                    Globals.beacontail = i + 1;
                    if (Globals.beacontail == 1000)
                    {
                        Globals.beacontail = 0;
                    }
                }
            }
        }

        void StoreInBufferBeacon()//store packets in buffer
        {
            if (Globals.beaconhead == 1000)//round-robin
            {
                Globals.beaconhead = 0;
            }
            if (Globals.OccupiedBeaconNum >= 1000) return;//the buffer is full

            int pos = 0;

            //find next empty cell to store packet
            for (int i = Globals.beaconhead; i < 1000; i++)
            {
                if (!Globals.ifOccupiedBeacon[i])
                {
                    pos = i;
                    break;
                }
            }
            Globals.strBeaconArray[pos] = Globals.packet;


            Globals.ifOccupiedBeacon[pos] = true;
            Globals.beaconhead = pos + 1;
            Globals.OccupiedBeaconNum += 1;
        }

        void Buffer2ServerBluetooth()
        {//ihe: notice the async type here: to see if it can cause error

            int begin, end;

            if(Globals.bluetoothtail == Globals.bluetoothhead)//queue is empty
            {
                return;
            }
          
            if(Globals.bluetoothtail < Globals.bluetoothhead)//tail   head  -> 1000
            {
                begin = Globals.bluetoothtail;
                end = Globals.bluetoothhead;
                for (int i = begin; i < end; i++)
                {
                    if (Globals.isDelievedSuccess)
                    {
                        sendPacket(Globals.strBluetoothArray[i]);
                        //maintain variables
                        Globals.ifOccupiedBluetooth[i] = false;
                        Globals.OccupiedBluetoothNum -= 1;
                        Globals.bluetoothtail = i + 1;
                        if (Globals.bluetoothtail == 1000)
                        {
                            Globals.bluetoothtail = 0;
                        }
                    }
             
                }
            }
            else//  head  tail
            {
                begin = Globals.bluetoothtail;
                end = 1000;

                for (int i = begin; i < end; i++)
                {
                    if (Globals.isDelievedSuccess)
                    {
                        sendPacket(Globals.strBluetoothArray[i]);

                        //maintain variables
                        Globals.ifOccupiedBluetooth[i] = false;
                        Globals.OccupiedBluetoothNum -= 1;
                        Globals.bluetoothtail = i + 1;
                        if (Globals.bluetoothtail == 1000)
                        {
                            Globals.bluetoothtail = 0;
                        }
                    }
                }

                begin = 0;
                end = Globals.bluetoothhead;

                for (int i = begin; i< end; i++)
                {
                    if (Globals.isDelievedSuccess)
                    {
                        sendPacket(Globals.strBluetoothArray[i]);

                        //maintain variables
                        Globals.ifOccupiedBluetooth[i] = false;
                        Globals.OccupiedBluetoothNum -= 1;
                        Globals.bluetoothtail = i + 1;
                        if (Globals.bluetoothtail == 1000)
                        {
                            Globals.bluetoothtail = 0;
                        }
                    }
                }
            }
            
        }

        void StoreInBufferBluetooth()//store packets in buffer
        {
            if(Globals.bluetoothhead == 1000)//round-robin
            {
                Globals.bluetoothhead = 0;
            }
            if (Globals.OccupiedBluetoothNum >= 1000) return;//the buffer is full

            int pos = 0;

            //find next empty cell to store packet
            for (int i = Globals.bluetoothhead; i < 1000; i++)
            {
                if (!Globals.ifOccupiedBluetooth[i])
                {
                    pos = i;
                    break;
                }
            }
            Globals.strBluetoothArray[pos] = Globals.packet;


            Globals.ifOccupiedBluetooth[pos] = true;
            Globals.bluetoothhead  = pos + 1;
            Globals.OccupiedBluetoothNum += 1;
        }

        async void sendPacket(String packet)
        {
            string uri = getURISendPacket();

            var client = new HttpClient();

            String json = @packet;
            dynamic ob = JValue.Parse(json);
            int delivery_type = ob.delivery_type;
            int packetID = ob.packet_id;

            Globals.isDelievedSuccess = true;

            if (delivery_type == 0)//at most once
            {
                var contentarray = new StringContent(packet, Encoding.Default, "application/json");
                await client.PostAsync(uri, contentarray);
                Status1label.Text = "Good POST of OTHER Bluetooth ARRAY data to Server" + packetID.ToString();
            }
            else if(delivery_type == 1)//exactly once
            {
                var contentarray = new StringContent(packet, Encoding.Default, "application/json");
                var result = await client.PostAsync(uri, contentarray);
                if (result.IsSuccessStatusCode)
                {
                    Status1label.Text = "Good POST of OTHER Bluetooth ARRAY data to Server" + packetID.ToString();
                }//Globals.isDelievedSuccess = false will stop the process of the other sending
                else
                {
                    Globals.isDelievedSuccess = false;
                    while (!result.StatusCode.Equals(200) && !result.StatusCode.Equals(208))//keep sending until success
                    {
                        result = await client.PostAsync(uri, contentarray);
                    }
                    Globals.isDelievedSuccess = true;
                    Status1label.Text = "Good POST of OTHER Bluetooth ARRAY data to Server" + packetID.ToString();
                }
            }
            else if(delivery_type == 2)//at least once
            {
                var contentarray = new StringContent(packet, Encoding.Default, "application/json");
                var result = await client.PostAsync(uri, contentarray);
                result.EnsureSuccessStatusCode();
                // handling the answer  
                var resultString = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                {
                    Status1label.Text = "Good POST of OTHER Bluetooth ARRAY data to Server" + packetID.ToString();
                }
                else
                {
                    while (!result.IsSuccessStatusCode)//keep sending until success
                    {
                        result = await client.PostAsync(uri, contentarray);
                    }
                    Status1label.Text = "Good POST of OTHER Bluetooth ARRAY data to Server" + packetID.ToString();
                }
            }
        }

        public int generateDeliveryType()
        {
            TimeSpan duration = DateTime.Now - Globals.starttime;
            if (Globals.first_packet)
            {
                return 1;
            }
            if(duration.TotalMinutes == 30)
            {
                return 1;//exactly once
            }
            else
            {
                if(Globals.rnd.Next(0,2) == 0)
                {
                    return 0;//at most
                }
                else
                {
                    return 2;//at least once
                }
            }
        }

    }   
}
