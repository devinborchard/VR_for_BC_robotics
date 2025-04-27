using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

public class PythonConnection : MonoBehaviour
{
    public string server = "127.0.0.1";  // IP address of the Python server
    public int port = 65432;   
    // Start is called before the first frame update
    public string SendStateToPython(string message)
    {
        // UnityEngine.Debug.Log($"Sending: {message}");
        try
        {
            using (TcpClient client = new TcpClient(server, port))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                // Send message
                stream.Write(data, 0, data.Length);
                // Debug.Log("Sent: " + message);

                // Read response
                byte[] buffer = new byte[4096];  // Large enough to receive model output
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                return response;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Python connection error: " + e.Message);
            return null;
        }
    }
}
