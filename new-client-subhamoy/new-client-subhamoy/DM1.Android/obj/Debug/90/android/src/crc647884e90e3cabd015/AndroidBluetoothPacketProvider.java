package crc647884e90e3cabd015;


public class AndroidBluetoothPacketProvider
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("UniversalBeacon.Library.AndroidBluetoothPacketProvider, UniversalBeacon.Library.Android", AndroidBluetoothPacketProvider.class, __md_methods);
	}


	public AndroidBluetoothPacketProvider ()
	{
		super ();
		if (getClass () == AndroidBluetoothPacketProvider.class)
			mono.android.TypeManager.Activate ("UniversalBeacon.Library.AndroidBluetoothPacketProvider, UniversalBeacon.Library.Android", "", this, new java.lang.Object[] {  });
	}

	public AndroidBluetoothPacketProvider (android.content.Context p0)
	{
		super ();
		if (getClass () == AndroidBluetoothPacketProvider.class)
			mono.android.TypeManager.Activate ("UniversalBeacon.Library.AndroidBluetoothPacketProvider, UniversalBeacon.Library.Android", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
