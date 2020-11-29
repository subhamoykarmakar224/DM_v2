package com.rohitsriram.datalayer.app_classes;

import android.arch.lifecycle.Observer;
import android.arch.lifecycle.ViewModelProviders;
import android.os.AsyncTask;
import android.support.annotation.Nullable;
import android.support.v7.app.AppCompatActivity;
import android.content.BroadcastReceiver;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.widget.Button;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.support.v4.content.LocalBroadcastManager;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;

import java.io.IOException;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.io.Writer;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Calendar;


import com.google.android.gms.wearable.Node;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.Tasks;
import com.google.android.gms.wearable.Wearable;
import com.google.gson.JsonArray;
import com.rohitsriram.datalayer.accelerometer_table.Accelerometer;
import com.rohitsriram.datalayer.accelerometer_table.AccelerometerDao;
import com.rohitsriram.datalayer.bluetooth_table.Bluetooth;
import com.rohitsriram.datalayer.bluetooth_table.BluetoothDao;
import com.rohitsriram.datalayer.database_classes.DataRoomDatabase;
import com.rohitsriram.datalayer.gravity_table.GravityDao;
import com.rohitsriram.datalayer.gravity_table.Gravity_data;
import com.rohitsriram.datalayer.gyroscope_table.Gyroscope;
import com.rohitsriram.datalayer.gyroscope_table.GyroscopeDao;
import com.rohitsriram.datalayer.heart_rate_table.HeartRate;
import com.rohitsriram.datalayer.database_classes.DataViewModel;
import com.rohitsriram.datalayer.R;
import com.rohitsriram.datalayer.heart_rate_table.HeartRateDao;
import com.rohitsriram.datalayer.lin_acc_table.LinearAccDao;
import com.rohitsriram.datalayer.lin_acc_table.LinearAcceleration;
import com.rohitsriram.datalayer.mag_field_table.MagFieldDao;
import com.rohitsriram.datalayer.mag_field_table.MagneticField;
import com.rohitsriram.datalayer.pending_table.Pending;
import com.rohitsriram.datalayer.pending_table.PendingDao;
import com.rohitsriram.datalayer.steps_table.Steps;
import com.rohitsriram.datalayer.steps_table.StepsDao;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.concurrent.ExecutionException;

import javax.net.ssl.HttpsURLConnection;

import static com.rohitsriram.datalayer.database_classes.DataRoomDatabase.INSTANCE;

//This version can select multiple sensors with intervals for each sensors
//It also receives all the data from all the sensors
//Then it stores that data in the specific database
public class MainActivity extends AppCompatActivity  {

    //member variables
    Button talkbutton;
    TextView textview;
    EditText editTextIP;
    EditText editTextPort;
    private static final String TAG = "MainActivity";
    private DataViewModel mDataViewModel;
    private ArrayList<Integer> plotData = new ArrayList<>();
    private ArrayList<DataModel> chosenWatchSensors = new ArrayList<>();

    private String protocol = "http://";
    private String ip_address = "128.195.53.163:";
    private String port = "1080";
    private String endpoint = "/observation";
    int count = -1;

    //Setup; Getting chosenSensor, Detecting if there is a change in Database
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        //layout setup
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        talkbutton = findViewById(R.id.talkButton);
        textview = findViewById(R.id.textView);
        editTextIP = findViewById(R.id.IP);
        editTextPort = findViewById(R.id.Port);

        //Get checked sensors from the Watch Sensors class
        Intent intent = getIntent();
        if(intent.hasExtra("Chosen Watch Sensors")){
            ArrayList<DataModel> watchSensors;
            watchSensors = intent.getExtras().getParcelableArrayList("Chosen Watch Sensors");
            for(DataModel x: watchSensors){
                if(x.checked == true && !x.interval.equals("0")){
                    Log.i(TAG, "WE GOT SENSOR " + x.name + " " + x.interval);
                    chosenWatchSensors.add(x);
                }
            }

        }
        else{
            Log.i(TAG, "NO WATCH EXTRAS");
        }

        //Setup database in MainActivity and get data from database
        mDataViewModel = ViewModelProviders.of(this).get(DataViewModel.class);



        //This part of the code checks if the database has been updated, and if it has
        //it updates the UI with the new data
        mDataViewModel.getAllWords().observe(this, new Observer<List<MagneticField>>() {
            @Override
            public void onChanged(@Nullable final List<MagneticField> data) {

                for(int i = data.size() - 1; i > 0; i--){
                    String magData = data.get(i).getX();
                    if(plotData.size() < 10 && !plotData.contains(Integer.valueOf(magData))){
                        if(Integer.valueOf(magData) < 100 && Integer.valueOf(magData) > -100){
                            plotData.add(Integer.valueOf(magData));
                        }

                    }
                }

            }
        });

        //This registers the code that listens for data from watch
        /*IntentFilter messageFilter = new IntentFilter(Intent.ACTION_SEND);
        Receiver messageReceiver = new Receiver();*/
        LocalBroadcastManager.getInstance(this).registerReceiver(mPhoneReceiver, new IntentFilter(Intent.ACTION_SEND));

        LocalBroadcastManager.getInstance(this).registerReceiver(mReceiver, new IntentFilter("PHONEDATA"));

        LocalBroadcastManager.getInstance(this).registerReceiver(bluetoothReceiver, new IntentFilter("BLUETOOTHDATA"));

    }

    //Receives bluetooth data
    private BroadcastReceiver bluetoothReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            //Log.i(TAG, "RECEIVED BLUETOOTH DATA IN RECEIVER");
            String sensorData = intent.getExtras().getString("PhoneData");
            String[] sensorDataArray = sensorData.split("\\s+");
            String time = sensorDataArray[0] + " " +  sensorDataArray[1];
            String name = sensorDataArray[2];
            String mac = sensorDataArray[3];
            try {
                JSONArray bt = bluetoothToJson(name, mac, time);
                Log.i(TAG, bt.toString());
                bluetoothToTIPPERS(bt.toString(), sensorData);
            } catch (JSONException e) {
                e.printStackTrace();
            }

        }
    };

    //Receives sensor data from phone
    private BroadcastReceiver mReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            Log.i(TAG, "RECEIVED PHONE DATA IN RECEIVER");
            String sensorData = intent.getExtras().getString("PhoneData");
            String[] sensorDataArray = sensorData.split("\\s+");
            String sensorName = sensorDataArray[3];

            String timestampedData = addTimestamp(sensorData);
            try {
                Log.i(TAG, "Received data from phone for: " + sensorName);
                insertIntoDB(sensorName, timestampedData);
            } catch (JSONException e) {
                Log.e(TAG, "JSON ERROR IN onReceive");
            }
        }
    };


    //Contains code for the EditText that takes in the IP and port
    public void onResume(){
        super.onResume();


        //This part of the code detects if the TextField has been changed and
        //gives us the data if it has
        TextWatcher textWatcher = new TextWatcher() {

            @Override
            public void beforeTextChanged(CharSequence charSequence, int i, int i1, int i2) {

            }

            @Override
            public void onTextChanged(CharSequence charSequence, int i, int i1, int i2) {

            }

            //
            @Override
            public void afterTextChanged(Editable editable) {
                //here, after we introduced something in the EditText we get the string from it
                ip_address = editable.toString();
            }
        };

        TextWatcher textWatcher1 = new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {

            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {

            }

            @Override
            public void afterTextChanged(Editable s) {
                port = s.toString();
            }
        };


        editTextIP.addTextChangedListener(textWatcher);
        editTextPort.addTextChangedListener(textWatcher1);

    }

    private BroadcastReceiver mPhoneReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            if(intent.hasExtra("messageFromWatch")){
                String sensorName = "";
                String sensorData = intent.getExtras().getString("messageFromWatch");
                String[] sensorDataArray = sensorData.split("\\s+");
                if(sensorDataArray.length >= 4){
                    sensorName = sensorDataArray[3];
                }
                String timestampedData = addTimestamp(sensorData);
                try {
                Log.i(TAG, "Received data from watch for: " + sensorName + " " +
                        String.valueOf(count));
                count++;
                insertIntoDB(sensorName, timestampedData);
                } catch (JSONException e) {
                    Log.e(TAG, "JSON ERROR IN onReceive");
                }

            }
        }
    };


    //Adds timestamp to data
    public String addTimestamp(String sensorData){
        Date currentTime = Calendar.getInstance().getTime();
        String pattern = "YYYY-MM-dd HH:mm:ss";
        DateFormat df = new SimpleDateFormat(pattern);
        String temp = df.format(currentTime);
        String test  = sensorData + " " + temp;
        return test;
    }

    //Inserts Data into SQLite DB
    public void insertIntoDB(String sensorName, String sensorData) throws JSONException {
        //PUTS DATA INTO DATABASE
        String[] elements = sensorData.split("\\s+");
        String x = "", y = "", z = "", sensor_type = "", timestamp = "";
        if(elements.length >= 6){
            x = elements[0];
            y = elements[1];
            z = elements[2];
            sensor_type = elements[3];
            timestamp = elements[4] + " " + elements[5];
        }

        if(sensorName.equals("Heart")){
            Log.i(TAG, "In Heart: " + sensorData);
            HeartRate heartRate = new HeartRate(timestamp, x, y, z, sensor_type);
            JSONArray hr = heartRateToJson(x, timestamp);
            sendToTippers(hr.toString(), sensorData);
            Log.i(TAG, hr.toString());
            mDataViewModel.insert(heartRate);
        }
        else if(sensorName.equals("Steps")){
            Log.i(TAG, "In Steps: " + sensorData);
            Steps steps = new Steps(timestamp, x, y, z, sensor_type);
            JSONArray st = stepsToJson(x, timestamp);
            sendToTippers(st.toString(), sensorData);
            Log.i(TAG, st.toString());
            mDataViewModel.insert(steps);
        }
        else if(sensorName.equals("Linear")){
            Log.i(TAG, "In Linear: " + sensorData);
            LinearAcceleration linearAcceleration = new LinearAcceleration(timestamp, x, y, z, sensor_type);
            JSONArray la = linAccToJson(x, y, z, timestamp);
            sendToTippers(la.toString(), sensorData);
            Log.i(TAG, la.toString());
            mDataViewModel.insert(linearAcceleration);
        }
        else if(sensorName.equals("Accelerometer")){
            Log.i(TAG, "In Accelerometer: " + sensorData);
            Accelerometer accelerometer = new Accelerometer(timestamp, x, y, z, sensor_type);
            JSONArray ac = accToJson(x, y, z, timestamp);
            sendToTippers(ac.toString(), sensorData);
            Log.i(TAG, ac.toString());
            mDataViewModel.insert(accelerometer);
        }
        else if(sensorName.equals("Gyroscope")){
            Log.i(TAG, "In Gyro: " + sensorData);
            Gyroscope gyroscope = new Gyroscope(timestamp, x, y, z, sensor_type);
            JSONArray temp = gyroToJson(x, y, z, timestamp);
            sendToTippers(temp.toString(), sensorData);
            //Log.i(TAG, temp.toString());
            mDataViewModel.insert(gyroscope);
        }
        else if(sensorName.equals("Gravity")){
            Log.i(TAG, "In Gravity: " + sensorData);
            Gravity_data gravity_data = new Gravity_data(timestamp, x, y, z, sensor_type);
            JSONArray gr = gravityToJson(x, y, z, timestamp);
            sendToTippers(gr.toString(), sensorData);
            //Log.i(TAG, gr.toString());
            mDataViewModel.insert(gravity_data);

        }
        else if(sensorName.equals("Magnetic")){
            Log.i(TAG, "In Magnetic: " + sensorData);
            MagneticField magField = new MagneticField(timestamp, x, y, z, sensor_type);
            JSONArray mf = magneticToJson(x, y, z, timestamp);
            sendToTippers(mf.toString(), sensorData);
            //Log.i(TAG, mf.toString());
            mDataViewModel.insert(magField);
        }

        else{
            Log.i(TAG, "Error choosing database " + sensorName);
        }
    }

    //Sends JSON data to TIPPERS
    public void sendToTippers(final String data, final String original){

        AsyncTask.execute(new Runnable() {
            @Override
            public void run() {
                String[] elements = original.split("\\s+");
                String x = "", y = "", z = "", sensor_type = "", timestamp = "";
                if(elements.length >= 6) {
                    x = elements[0];
                    y = elements[1];
                    z = elements[2];
                    sensor_type = elements[3];
                    timestamp = elements[4] + " " + elements[5];
                }

                HttpURLConnection myConnection = null;
                URL httpbinEndpoint;
                try {
                    httpbinEndpoint = new URL(protocol+ip_address+port+endpoint);
                    myConnection
                            = (HttpURLConnection) httpbinEndpoint.openConnection();
                    myConnection.setRequestMethod("POST");
                    myConnection.setRequestProperty("Content-Type", "application/json");
                    myConnection.setRequestProperty("Accept", "application/json");
                    // Enable writing
                    myConnection.setDoOutput(true);

                    byte[] out = data.getBytes(StandardCharsets.UTF_8);
                    int length = out.length;
                    myConnection.setFixedLengthStreamingMode(length);
                    myConnection.connect();

                    try(OutputStream os = myConnection.getOutputStream()) {
                        os.write(out);
                    }
                    int responseCode = myConnection.getResponseCode();
                    if (responseCode != HttpURLConnection.HTTP_OK) {
                        Log.i(TAG, "HTTP error code: " + responseCode);
                    }
                    if (myConnection.getResponseCode() == 200) {
                        // Success
                        Log.i(TAG, "SENT TO TIPPERS");
                        sendPending();
                    } else {
                        Log.i(TAG, "ERROR CODE: " + myConnection.getResponseCode());
                        // Error handling code goes here
                    }



                } catch (MalformedURLException e) {
                    e.printStackTrace();
                    Pending pending = new Pending(timestamp, x, y, z, sensor_type);
                    mDataViewModel.insert(pending);
                    Log.i(TAG, "Connection Failed Malformed");
                } catch (ProtocolException e) {
                    e.printStackTrace();
                    Pending pending = new Pending(timestamp, x, y, z, sensor_type);
                    mDataViewModel.insert(pending);
                    Log.i(TAG, "Connection Failed Protocol");
                } catch (IOException e) {
                    Pending pending = new Pending(timestamp, x, y, z, sensor_type);
                    mDataViewModel.insert(pending);
                    Log.i(TAG, "IO ERROR MESSAGE: " + e.getMessage());
                }catch (Exception e){
                    Pending pending = new Pending(timestamp, x, y, z, sensor_type);
                    mDataViewModel.insert(pending);
                    Log.i(TAG, "Connection Failed Something");
                }
                finally {
                            if(myConnection != null){
                                myConnection.disconnect();
                            }
                }

            }
        });
    }

    //sends the pending data to the TIPPERS server
    public void sendPending() throws JSONException {
        List<Pending> unsent_data = mDataViewModel.getPendingData();
        ArrayList<String> jsonData = new ArrayList<>();
        ArrayList<String> originalData = new ArrayList<>();

        for(Pending pending : unsent_data){
            String data = pending.getX() + " " + pending.getY() + " " + pending.getZ() + " " +
                    pending.getSensor_type() + " " + pending.getTime();
            originalData.add(data);
            JSONArray json = convertToJSON(pending.getSensor_type(), data);
            jsonData.add(json.toString());

        }

        new PopulateDbAsync(INSTANCE).execute();

        for(int i = 0; i < jsonData.size(); i++){
            sendToTippers(jsonData.get(i), originalData.get(i));
        }
    }

    public JSONArray convertToJSON(String sensorName, String sensorData) throws JSONException {
        String[] elements = sensorData.split("\\s+");
        String x = "", y = "", z = "", sensor_type = "", timestamp = "";
        if(elements.length >= 6){
            x = elements[0];
            y = elements[1];
            z = elements[2];
            timestamp = elements[4] + " " + elements[5];
        }

        if(sensorName.equals("Heart")){
            JSONArray hr = heartRateToJson(x, timestamp);
            return hr;
        }
        else if(sensorName.equals("Steps")){
            JSONArray st = stepsToJson(x, timestamp);
            return st;
        }
        else if(sensorName.equals("Linear")){
            JSONArray la = linAccToJson(x, y, z, timestamp);
            return la;
        }
        else if(sensorName.equals("Accelerometer")){

            JSONArray ac = accToJson(x, y, z, timestamp);
            return ac;
        }
        else if(sensorName.equals("Gyroscope")){
            JSONArray temp = gyroToJson(x, y, z, timestamp);
            return temp;
        }
        else if(sensorName.equals("Gravity")){
            JSONArray gr = gravityToJson(x, y, z, timestamp);
            return gr;
        }
        else if(sensorName.equals("Magnetic")){
            JSONArray mf = magneticToJson(x, y, z, timestamp);
            return mf;
        }
        else if(sensorName.equals("Bluetooth")){
            String[] temp = sensorData.split("\\s+");
            String time = temp[0] + temp[1];
            String name = temp[2];
            String mac = temp[3];
            JSONArray bt = bluetoothToJson(name, mac, time);
            return bt;
        }
        else{
            Log.i(TAG, "Error choosing database " + sensorName);
            return new JSONArray();
        }
    }

    //Sends bluetooth data to TIPPERS
    public void bluetoothToTIPPERS(final String data, final String original){

        AsyncTask.execute(new Runnable() {
            @Override
            public void run() {
                String[] elements = original.split("\\s+");
                String time = elements[0] + elements[1];
                String name = elements[2];
                String mac = elements[3];
                String sensor_type = "Bluetooth";
                URL httpbinEndpoint = null;
                HttpURLConnection myConnection = null;
                try {
                    String myData = data;
                    httpbinEndpoint = new URL(protocol + ip_address + port + endpoint);
                    Log.i("url", "URL: " + protocol + ip_address + port + endpoint);
                    myConnection = (HttpURLConnection) httpbinEndpoint.openConnection();
                    myConnection.setRequestMethod("POST");
                    // Enable writing
                    myConnection.setDoOutput(true);
                    myConnection.setRequestProperty("Content-Type", "application/json");
                    myConnection.setRequestProperty("Accept", "application/json");


                    myConnection.getOutputStream().write(myData.getBytes());
                    // Write the data
                    if (myConnection.getResponseCode() == 200) {
                        // Success
                        Log.i(TAG, "SENT TO TIPPERS");
                        //Send pending data
                        sendPending();

                    } else {
                        // Error handling code goes here
                    }
                    //myConnection.disconnect();

                } catch (MalformedURLException e) {
                    e.printStackTrace();
                    Pending pending = new Pending(time, name, mac, "NULL", sensor_type);
                    mDataViewModel.insert(pending);
                    Log.i(TAG, "Connection Failed Malformed");
                } catch (ProtocolException e) {
                    Pending pending = new Pending(time, name, mac, "NULL", sensor_type);
                    mDataViewModel.insert(pending);
                    e.printStackTrace();
                    Log.i(TAG, "Connection Failed Protocol");
                } catch (IOException e) {
                    Pending pending = new Pending(time, name, mac, "NULL", sensor_type);
                    mDataViewModel.insert(pending);
                    Log.i(TAG, e.getMessage());
                    Log.i(TAG, "Connection Failed IO");
                } catch (Exception e) {
                    Pending pending = new Pending(time, name, mac, "NULL", sensor_type);
                    mDataViewModel.insert(pending);
                    Log.i(TAG, "Connection Failed Something");
                } finally {
                    if (myConnection != null) {
                        myConnection.disconnect();
                    }
                }
            }
        });
    }

    //Turns data to JSON
    public JSONArray gyroToJson(String x, String y, String z, String timestamp) throws JSONException {
        JSONObject gyro = new JSONObject();
        gyro.put("type", "BO_Gyroscope_Reading");
        gyro.put("sensor", "6");
        JSONObject data = new JSONObject();
        data.put("gyro_x", x);
        data.put("gyro_y", y);
        data.put("gyro_z", z);
        data.put("timestamp", timestamp);
        JSONArray payload = new JSONArray();
        payload.put(data);
        gyro.put("payload", payload);
        JSONArray result = new JSONArray();
        result.put(gyro);

        return result;
    }

    //Turns data to JSON
    public JSONArray heartRateToJson(String x, String timestamp) throws JSONException{
        JSONObject gyro = new JSONObject();
        gyro.put("type", "BO_HeartRate_Reading");
        gyro.put("sensor", "7");
        JSONObject data = new JSONObject();
        data.put("heart_rate", x);
        data.put("timestamp", timestamp);
        JSONArray payload = new JSONArray();
        payload.put(data);
        gyro.put("payload", payload);
        JSONArray result = new JSONArray();
        result.put(gyro);

        return result;
    }

    //Turns data to JSON
    public JSONArray stepsToJson(String x, String timestamp) throws JSONException{
        JSONObject gyro = new JSONObject();
        gyro.put("type", "BO_Steps_Reading");
        gyro.put("sensor", "8");
        JSONObject data = new JSONObject();
        data.put("steps", x);
        data.put("timestamp", timestamp);
        JSONArray payload = new JSONArray();
        payload.put(data);
        gyro.put("payload", payload);
        JSONArray result = new JSONArray();
        result.put(gyro);

        return result;
    }

    //Turns data to JSON
    public JSONArray accToJson(String x, String y, String z, String timestamp) throws JSONException {
        JSONObject gyro = new JSONObject();
        gyro.put("type", "BO_Accelerometer_Reading");
        gyro.put("sensor", "5");
        JSONObject data = new JSONObject();
        data.put("acc_x", x);
        data.put("acc_y", y);
        data.put("acc_z", z);
        data.put("timestamp", timestamp);
        JSONArray payload = new JSONArray();
        payload.put(data);
        gyro.put("payload", payload);
        JSONArray result = new JSONArray();
        result.put(gyro);

        return result;
    }

    //Turns data to JSON
    public JSONArray linAccToJson(String x, String y, String z, String timestamp) throws JSONException {
        JSONObject gyro = new JSONObject();
        gyro.put("type", "BO_Linear_Acceleration_Reading");
        gyro.put("sensor", "9");
        JSONObject data = new JSONObject();
        data.put("linAcc_x", x);
        data.put("linAcc_y", y);
        data.put("linAcc_z", z);
        data.put("timestamp", timestamp);
        JSONArray payload = new JSONArray();
        payload.put(data);
        gyro.put("payload", payload);
        JSONArray result = new JSONArray();
        result.put(gyro);

        return result;
    }

    //Turns data to JSON
    public JSONArray gravityToJson(String x, String y, String z, String timestamp) throws JSONException {
        JSONObject gyro = new JSONObject();
        gyro.put("type", "BO_Gravity_Reading");
        gyro.put("sensor", "10");
        JSONObject data = new JSONObject();
        data.put("grav_x", x);
        data.put("grav_y", y);
        data.put("grav_z", z);
        data.put("timestamp", timestamp);
        JSONArray payload = new JSONArray();
        payload.put(data);
        gyro.put("payload", payload);
        JSONArray result = new JSONArray();
        result.put(gyro);

        return result;
    }

    //Turns data to JSON
    public JSONArray magneticToJson(String x, String y, String z, String timestamp) throws JSONException {
        JSONObject gyro = new JSONObject();
        gyro.put("type", "BO_Magnetic_Reading");
        gyro.put("sensor", "11");
        JSONObject data = new JSONObject();
        data.put("mag_x", x);
        data.put("mag_y", y);
        data.put("mag_z", z);
        data.put("timestamp", timestamp);
        JSONArray payload = new JSONArray();
        payload.put(data);
        gyro.put("payload", payload);
        JSONArray result = new JSONArray();
        result.put(gyro);

        return result;
    }

    //Turns data to JSON
    public JSONArray bluetoothToJson(String name, String mac, String timestamp) throws JSONException {
        JSONObject gyro = new JSONObject();
        gyro.put("type", "BO_Bluetooth_Reading");
        gyro.put("sensor", "23");
        JSONObject data = new JSONObject();
        data.put("name", name);
        data.put("mac", mac);
        data.put("timestamp", timestamp);
        JSONArray payload = new JSONArray();
        payload.put(data);
        gyro.put("payload", payload);
        JSONArray result = new JSONArray();
        result.put(gyro);

        return result;
    }


    //This part sends the chosen sensors to the watch
    // and requests data from those sensors
    public void talkClick(View v) {
        for(DataModel x : chosenWatchSensors){
            new NewThread("/my_path", x.interval + " " + x.name).start();
            Log.i(TAG, "THREAD STARTED: " + x.name);
        }

    }

    //This is the "Go to Graph" button
    //Starts the SimpleXTPlotActivity
    public void plotClick(View view) {
        Intent intent = new Intent(MainActivity.this, SimpleXTPlotActivity.class);
        intent.putIntegerArrayListExtra("HeartRates", plotData);
        startActivity(intent);
    }

    //Starts the activity which lets you choose a sensor
    public void findWatchSensors(View view) {
        Intent intent = new Intent(MainActivity.this, WatchSensors.class);
        startActivity(intent);
    }

    //This starts the activity which lets you select sensors from the phone
    public void findPhoneSensors(View view){
        Intent intent = new Intent(MainActivity.this, PhoneSensors.class);
        startActivity(intent);
    }


    //We don't want to interrupt the main thread when sending the message so
    //we create a new thread to handle sending the message to the watch
    //This method is called in talkClick
    class NewThread extends Thread {
        String path;
        String message;

        NewThread(String p, String m) {
            path = p;
            message = m;
        }


        public void run() {

            Task<List<Node>> wearableList =
                    Wearable.getNodeClient(getApplicationContext()).getConnectedNodes();
            try {

                List<Node> nodes = Tasks.await(wearableList);
                for (Node node : nodes) {
                    Task<Integer> sendMessageTask =
                            Wearable.getMessageClient(MainActivity.this).sendMessage(node.getId(), path, message.getBytes());

                    try {

                        Integer result = Tasks.await(sendMessageTask);

                    } catch (ExecutionException exception) {

                        //TO DO: Handle the exception//

                    } catch (InterruptedException exception) {

                    }

                }

            } catch (ExecutionException exception) {

                //TO DO: Handle the exception//

            } catch (InterruptedException exception) {

                //TO DO: Handle the exception//
            }

        }
    }

    //This is used to delete the elements of the Pending table once all of
    //the pending content has been sent
    private static class PopulateDbAsync extends AsyncTask<Void, Void, Void> {

        private PendingDao mPendingDao;

        PopulateDbAsync(DataRoomDatabase db) {
            mPendingDao = db.pendingDao();
        }

        @Override
        protected Void doInBackground(final Void... params) {
            mPendingDao.deleteAll();
            return null;
        }
    }

}
