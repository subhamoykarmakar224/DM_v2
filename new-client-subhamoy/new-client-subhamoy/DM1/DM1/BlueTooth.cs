using System;
using System.Collections.Generic;
using System.Text;

namespace DM1
{
    class BlueTooth
    {
        //  public int Id { get; set; }
        public string sensor_id { get; set; }
        public string type { get; set; }
        public string timestamp { get; set; }
        public string name { get; set; }
        public string mac { get; set; }
        

        public override string ToString()
        {
            return string.Format(
                "sensor_id:{0}" + "," + "type:{1}" + "," + "timestamp:{2}" + ", " + "name:{3}" + "," + "mac:{4}",
                sensor_id, type, timestamp, name, mac);
        }
    }
}
