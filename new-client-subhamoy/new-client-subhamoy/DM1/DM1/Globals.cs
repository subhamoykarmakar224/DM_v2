using DM1.utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DM1
{
    public static class Globals
    {
        public static String URI_NAME = "http://192.168.0.4:18001"; // Modifiable
        public static String URI_SENSOR = "http://192.168.0.4:18001"; // Modifiable
        public static String UUID = "00000000-0000-0000-0000-000000000001";
        public static String beaconID = "edu.uci";
        public static String KEY_SP_PACKEY_ID = "sp_packet_id";

        public static String[] strBeaconArray = new String[1000];
        public static String[] strBluetoothArray = new String[1000];//buffer
        public static int strBeaconCnt = 0;
        public static int strBluetoothCnt = 0;
        public static string strMacAddy = "";
        public static string strNative = "";
        public static string strRSSI = "";
        public static string strLat = "";
        public static string strLon = "";
        public static string strAdv = "";
        // ihe: to modify: add more data type
        public static int RecordNumPacket = 2;//number of records in a packet
        public static int packetID = SharedPreferenceController.sharedPrefGetValue(KEY_SP_PACKEY_ID); //packet ID
        public static String packet = "";//to packet records json into a packet
        public static int RecordCount = 0;//number of records in a temp packet
        public static bool first_packet = true;
        public static int delivery_type = 0;
        public static bool timeOut = true;

        public static bool[] ifOccupiedBeacon = new bool[1000];
        public static bool[] ifOccupiedBluetooth = new bool[1000];
        public static int OccupiedBeaconNum = 0;//to indicate if the buffer is full
        public static int OccupiedBluetoothNum = 0;

        public static bool isDelievedSuccess = true;

        public static int timeoutThreshold = 60 * 1000;//miliseconds = 1 minute

        public static DateTime starttime;//generate delievery packet
        public static Random rnd = new Random();//random seeed to generate delievery packet

        //head and tail of queue
        public static int beacontail = 0;
        public static int beaconhead = 0;
        public static int bluetoothtail = 0;
        public static int bluetoothhead = 0;
    }
}
