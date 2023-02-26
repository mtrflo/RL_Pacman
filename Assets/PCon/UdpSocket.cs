/*
Created by Youssef Elashry to allow two-way communication between Python3 and Unity to send and receive strings

Feel free to use this in your individual or commercial projects BUT make sure to reference me as: Two-way communication between Python 3 and Unity (C#) - Y. T. Elashry
It would be appreciated if you send me how you have used this in your projects (e.g. Machine Learning) at youssef.elashry@gmail.com

Use at your own risk
Use under the Apache License 2.0

Modified by: 
Youssef Elashry 12/2020 (replaced obsolete functions and improved further - works with Python as well)
Based on older work by Sandra Fang 2016 - Unity3D to MATLAB UDP communication - [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
*/

using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;
using System.Threading.Tasks;
using UnityEditor.PackageManager;

public class UdpSocket : MonoBehaviour
{
    public static UdpSocket me;
    [HideInInspector] public bool isTxStarted = false;

    [SerializeField] string IP = "127.0.0.1"; // local host
    [SerializeField] int rxPort = 8000; // port to receive data from Python on
    [SerializeField] int txPort = 8001; // port to send data to Python on

    // Create necessary UdpClient objects
    UdpClient client;
    static IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    PythonTest pythonTest;

    public Action<string> OnReceived;

    //IEnumerator SendDataCoroutine() // DELETE THIS: Added to show sending data from Unity to Python via UDP
    //{
    //    while (true)
    //    {
    //        SendData("Sent from Unity: " + i.ToString());
    //        i++;
    //        yield return new WaitForSeconds(1f);
    //    }
    //}

    public async Task<UdpReceiveResult> SendAndGetData(string message) // Use to send data to Python
    {
        string text;
        //if (client == null)
        //    return null;
        //try
        //{
            Profiler.BeginSample("SendAndGetData");
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.
            client.Send(data, data.Length, remoteEndPoint);
            Profiler.EndSample();
            client.BeginReceive(OnReceived,socket);
            
            //await Task.Yield();return text;
        //}
        //catch (Exception err)
        //{
        //    print(err.ToString());
        //    return await Task.Yield();
        //}
    }
    IPEndPoint anyIP;
    void Awake()
    {
        anyIP = new IPEndPoint(IPAddress.Any, 0);

        if (me == null)
            me = this;
        if (remoteEndPoint != null) { return; }
        // Create remote endpoint (to Matlab) 
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        // Create local client
        client = new UdpClient(rxPort);

        // local endpoint define (where messages are received)
        // Create a new thread for reception of incoming messages
        //receiveThread = new Thread(new ThreadStart(ReceiveData));
        //receiveThread.IsBackground = true;
        //receiveThread.Start();
        //StartCoroutine(ReceiveData());
        // Initialize (seen in comments window)
        print("UDP Comms Initialised");

        //StartCoroutine(SendDataCoroutine()); // DELETE THIS: Added to show sending data from Unity to Python via UDP
        OnReceived = (s) => { };
    }

    private void Start() 
    {
        pythonTest = FindObjectOfType<PythonTest>(); // Instead of using a public variable
    }

    // Receive data, update packets received
    private IEnumerator ReceiveData()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        string text;
        WaitForSeconds wfs = new WaitForSeconds(2f);
        while (true)
        {
            yield return wfs;
            try
            {
                //
                byte[] data = client.Receive(ref anyIP);
                text = Encoding.UTF8.GetString(data);
                //print(">> " + text);
                ProcessInput(text);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    private void ProcessInput(string input)
    {
        // PROCESS INPUT RECEIVED STRING HERE
        //pythonTest.UpdatePythonRcvdText(input); // Update text by string received from python

        //if (!isTxStarted) // First data arrived so tx started
        //{
        //    isTxStarted = true;
        //}
        //pythonTest.SendToPython();
        OnReceived.Invoke(input);
    }

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        //if (receiveThread != null)
        //    receiveThread.Abort();

        //client.Close();
    }
    private void OnApplicationQuit()
    {
        client.Close();
    }
}