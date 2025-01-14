﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPTestClient : MonoBehaviour
{
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	private NetworkStream stream;
	#endregion
	// Use this for initialization 	
	public static string IP;    //will be passed by scene 0
	public static int PORT = 9999;

	public void Start()
	{
		Debug.Log("MY IP IS: " + IP);
		ConnectToTcpServer();
	}
	// Update is called once per frame

	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		try
		{
			Debug.Log("Trying to connecto to :" + IP + ":" + PORT);
			socketConnection = new TcpClient(IP, PORT);
			//socketConnection = new TcpClient("192.168.60.24", PORT);
			stream = socketConnection.GetStream();
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	public void SendMsg(string msg)
	{
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				string clientMessage = msg;
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(clientMessage);
				byte[] msgSize = Encoding.UTF8.GetBytes(clientMessageAsByteArray.Length.ToString().PadLeft(8, '0'));
				//Debug.Log("msgSize =" + clientMessageAsByteArray.Length.ToString().PadLeft(8, '0'));

				stream.Write(msgSize, 0, msgSize.Length);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				//Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}


	private void SendByStep(byte[] image, int size)
    {
		int mul = size / 8192;
		int reminder = size % 8192;
		int lastStep = 0;
		Debug.Log("imgsize=" + size);
		Debug.Log("mul=" + mul);
		for(int i=0; i<mul; i++)
        {
			stream.Write(image, lastStep, 8192);
			lastStep += 8192;
        }
		stream.Write(image, lastStep, reminder);
    }

	public void SendImage(byte[] image)
	{
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			if (stream.CanWrite)
			{
				//byte[] imgSize = Encoding.UTF8.GetBytes(image.Length.ToString().PadLeft(8, '0'));
				//Debug.Log("imgSize =" + image.Length.ToString().PadLeft(8, '0'));

				//stream.Write(imgSize, 0, imgSize.Length);
				//SendByStep(image, image.Length);
				//Debug.Log("ATTENZIONE, imgSize=" + image.Length.ToString().PadLeft(8, '0') + "image.Length=" + image.Length);
				//stream.Write(image, 0, image.Length);
				var imageBytesStr = Convert.ToBase64String(image);

				string json = "{ \"msg\":" + "\"" + HandTracking.msgToSend + "\""+ "," 
					+ "\"img\":" + "\"" + imageBytesStr + "\"" + "}";

				byte[] jsonSize = Encoding.UTF8.GetBytes(json.Length.ToString().PadLeft(8, '0'));
				//Debug.Log("jsonSize =" + json.Length.ToString().PadLeft(8, '0'));
				stream.Write(jsonSize, 0, jsonSize.Length);
				
				byte[] jsonStringToSend = Encoding.UTF8.GetBytes(json);
				stream.Write(jsonStringToSend, 0, jsonStringToSend.Length);
				//Debug.Log("jsonStringToSend" + jsonStringToSend.Length);
				//SendByStep(jsonStringToSend, jsonStringToSend.Length);
				//Debug.Log(json);
				//Debug.Log("Client sent his image - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
}
