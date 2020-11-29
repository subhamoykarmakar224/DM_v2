using System;
using System.Collections.Generic;
using System.Text;

namespace DM1
{
    class BluetoothOther
    {
        public string clientID { get; set; }
        public string type { get; set; }
        public string timestamp { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string native { get; set; }
        public string rssi { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string advRecord { get; set; }


        public override string ToString()
        {
            return string.Format(
                "device_name:{0}" + "," + "type:{1}" + "," + "timestamp:{2}" + ", " + "name:{3}" + "id:{4}" + "," + "native:{5}" + "," + "rssi:{6}" + "," + "lat:{7}" + ", " + "lon:{8}" + "," + "advrecord:{9}",
                clientID, type, timestamp, name, id, native, rssi, lat, lon, advRecord);
        }

    }
}
