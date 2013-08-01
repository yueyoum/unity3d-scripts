using UnityEngine;
using System.Collections;

using System;
using System.Net;
using System.Net.Sockets;

using Google.ProtocolBuffers;

/* This is a demo, to show how to using Socket in Unity3d
 * and how to serialize data with Google protobuf.
 * So, Do not care about the logic.
 */


public class NetWorking : MonoBehaviour {
	public string ip = "192.168.137.98";
	public int port = 8888;
	
	private Socket socket;
	private bool connected = false;
	
	// for test
	private int id = 1;
	

	// Use this for initialization
	void Start () {
		IPAddress ipAddress = IPAddress.Parse(ip);
		socket = new Socket(
			AddressFamily.InterNetwork,
			SocketType.Stream,
			ProtocolType.Tcp
			);
		
		try {
			socket.Connect(new IPEndPoint(ipAddress, port));
			// In my test,  Connect method NEVER thrown an exception even
			// there were wrong ip, port.
			// So, for determine whether we have connected to the server
			// we must do some IO opprate. means send and recv.
			// Actually, There is necessary send data here,
			// For verification or something else
			
			byte[] data = PackData(id++);
			SockSend(data);
			
			byte[] recv = SockRecv();
			
			CodeBattle.Marine marine = CodeBattle.Marine.ParseFrom(recv);
			print (marine);
			connected = true;
		}
		catch {
			print("NewWorking NOT work!");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!connected) return;
		
		// Wating for Param#1 Microseconds to check is there any data send from server.
		// 1 second == 1000 Millisecond == 1000 * 1000 Microseconds
		if( socket.Poll(10000, SelectMode.SelectRead) ) {
			try {
				byte[] data = SockRecv();
				CodedInputStream stream = CodedInputStream.CreateInstance(data);
				CodeBattle.Marine marine = CodeBattle.Marine.ParseFrom(stream);
				print ("Receive: " + marine);
			}
			catch (Exception e) {
				print(e);
				connected = false;
				return;
			}
		}
		
		if(Input.GetKeyDown(KeyCode.A)) {
			byte[] data = PackData(id++);
			try {
				SockSend(data);
			}
			catch {
				print ("Socket Send Error");
			}
		}
	}
	

	
	void SockSend (byte[] data) {
		socket.Send(data, data.Length, SocketFlags.None);
	}
	
	byte[] SockRecv () {
		byte[] lenBytes = new byte[4];
		int rec = socket.Receive(lenBytes, 4, SocketFlags.None);
		if (rec == 0) {
			throw new Exception("Remote Closed the connection");
		}
		
		int len =  IPAddress.NetworkToHostOrder( BitConverter.ToInt32(lenBytes, 0) );
		byte[] data = new byte[len];
		rec = socket.Receive(data, len, SocketFlags.None);
		if (rec == 0) {
			throw new Exception("Remote Closed the connection");
		}
		return data;
	}

	
	
	byte[] PackData (int id) {
		CodeBattle.Marine.Builder marineBuilder = new CodeBattle.Marine.Builder();
		CodeBattle.Position.Builder positionBuilder = new CodeBattle.Position.Builder();

		marineBuilder.Id = id;
		marineBuilder.Hp = 99;
		
		positionBuilder.X = 1;
		positionBuilder.Y = 2;
		positionBuilder.Z = 3;
		
		marineBuilder.Position = positionBuilder.BuildPartial();
		
		CodeBattle.Marine marine = marineBuilder.BuildPartial();
		
		byte[] buffer = new byte[marine.SerializedSize];
		CodedOutputStream stream = CodedOutputStream.CreateInstance(buffer);
		marine.WriteTo(stream);
		
		byte[] binary = new byte[buffer.Length + 4];
		
		int len = IPAddress.HostToNetworkOrder( buffer.Length );
		byte[] lenBytes = BitConverter.GetBytes(len);
		lenBytes.CopyTo(binary, 0);
		buffer.CopyTo(binary, 4);
		
		return binary;
	}
}
