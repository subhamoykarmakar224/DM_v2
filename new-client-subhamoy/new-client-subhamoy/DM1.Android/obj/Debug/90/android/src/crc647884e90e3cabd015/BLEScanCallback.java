package crc647884e90e3cabd015;


public class BLEScanCallback
	extends android.bluetooth.le.ScanCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onScanFailed:(I)V:GetOnScanFailed_IHandler\n" +
			"n_onScanResult:(ILandroid/bluetooth/le/ScanResult;)V:GetOnScanResult_ILandroid_bluetooth_le_ScanResult_Handler\n" +
			"";
		mono.android.Runtime.register ("UniversalBeacon.Library.BLEScanCallback, UniversalBeacon.Library.Android", BLEScanCallback.class, __md_methods);
	}


	public BLEScanCallback ()
	{
		super ();
		if (getClass () == BLEScanCallback.class)
			mono.android.TypeManager.Activate ("UniversalBeacon.Library.BLEScanCallback, UniversalBeacon.Library.Android", "", this, new java.lang.Object[] {  });
	}


	public void onScanFailed (int p0)
	{
		n_onScanFailed (p0);
	}

	private native void n_onScanFailed (int p0);


	public void onScanResult (int p0, android.bluetooth.le.ScanResult p1)
	{
		n_onScanResult (p0, p1);
	}

	private native void n_onScanResult (int p0, android.bluetooth.le.ScanResult p1);

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
