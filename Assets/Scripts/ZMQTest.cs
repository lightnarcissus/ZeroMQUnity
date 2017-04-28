using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using UnityEngine.UI;
public class ZMQTest : MonoBehaviour {

	//ZMQPlugin imports
	[DllImport ("ZMQPlugin")]
	private static extern int ZMQConnect(string hostAddress);


	[DllImport ("ZMQPlugin")]
	private static extern IntPtr ZMQReceive();


	[DllImport ("ZMQPlugin")]
	private static extern void ZMQSend(string message,int length);


	[DllImport ("ZMQPlugin")]
	private static extern void ZMQClose();


	public InputField inputAddress;
	public InputField connectionPort;

	public InputField sendMessageField;

	ThreadedServer myServer;

	void Start () {
		myServer = new ThreadedServer();
		UnityEngine.Debug.Log ("starting");
		myServer.Start();
	}

	// Update is called once per frame
	void Update () {

	}

	public void Connect()
	{
		UnityEngine.Debug.Log ("connecting to: " + inputAddress.ToString () + " at port " + connectionPort.ToString ());
		string connectAddress = "tcp://" + inputAddress.ToString () + ":" + connectionPort.ToString ();
		myServer.Connect(connectAddress);
	}

	public void SendMessage()
	{
		myServer.SendMessage (sendMessageField.ToString());
	}

	void OnApplicationQuit()
	{
		myServer.End ();
	}

	public class ThreadedServer : ThreadedJob{
		public bool isRunning = false;

		public bool isServerConnected = false;
		public bool isSynced = false;
		public bool canStartGame = false;
		Stopwatch clockAlignmentStopwatch;
		//int numClockAlignmentTries = 0;
		//const int timeBetweenClockAlignmentTriesMS = 500;//500; //half a second
		//const int maxNumClockAlignmentTries = 120; //for a total of 60 seconds of attempted alignment
		public bool canSendMessage=false;
		public bool shouldConnect=false;
		int index=0;
		bool canSend=false;
		private string hostAddress="tcp://127.0.0.1:8888";

		public string messagesToSend = "";
		string incompleteMessage = "";


		public ThreadedServer(){

		}

		protected override void ThreadFunction()
		{
			if (shouldConnect) {
				isRunning = true;
				int connectionStatus = ZMQConnect (hostAddress);
				PrintSomething ("connection status " + connectionStatus); 
			}
			// Do your threaded task. DON'T use the Unity API here
			while (isRunning) {

					string message = Marshal.PtrToStringAnsi (ZMQReceive ());
					//				char msg=zmqreceive();
					//				PrintSomething(message);
					if (message != "none") {
						PrintSomething ("received: " + message.ToString ());
						PrintSomething ("length: " + message.ToCharArray ().Length.ToString ());
					}
				if (canSend) {
					string send_message = messagesToSend;
					PrintSomething("sending: " + send_message);
					ZMQSend (send_message,512);
					canSend = false;
				}
			}
			ShutDownMessage ();
		}

		void PrintSomething(string msg){
			UnityEngine.Debug.Log (msg);
		}
		public void Connect(string address)
		{
			hostAddress = address;
		}

		public void SendMessage(string msg)
		{
			messagesToSend = msg;
			canSend = true;
		}

		public void End()
		{
			isRunning = false;
		}

		void ShutDownMessage()
		{
			ZMQClose ();
			UnityEngine.Debug.Log ("shutting down");
		}
	}
	
}
