using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace DM1.utils
{
    class SharedPreferenceController
    {
        public static void sharedPrefAddValue(string key, int value)
        {
            Preferences.Set(key, value);
        }

        public static int sharedPrefGetValue(string key)
        {
            return Preferences.Get(key, 0);
        }

        public static void sharedPrefRemoveValue(string key)
        {
            Preferences.Remove(key);
        }

        public static void sharedPrefClearAllData()
        {
            Preferences.Clear();
        }
    }
}
