using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class BasculaReader
    {
        private SerialPort port;

        public bool Open(string portName, int baudRate, int parity, int dataBits, int stopBits)
        {
            port = new SerialPort(portName, baudRate, (Parity)parity, dataBits, (StopBits)stopBits);
            try
            {
                port.Open();
                if (port.IsOpen)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public void SetReadTimeout(int readTimeout)
        {
            if (port != null)
            {
                port.ReadTimeout = readTimeout;
            }
        }

        public void SetWriteTimeout(int writeTimeout)
        {
            if (port != null)
            {
                port.WriteTimeout = writeTimeout;
            }
        }

        public void SetHandshake(int handshake)
        {
            if (port != null)
            {
                port.Handshake = (Handshake)handshake;
            }
        }

        public void Close()
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
        }

        public string Read(bool onDemand, string readCommand)
        {
            string result = "";
            if (port != null && port.IsOpen)
            {
                try
                {
                    if (onDemand)
                    {
                        port.WriteLine(readCommand);
                    }

                    result = port.ReadLine();
                }
                catch (Exception)
                {
                    result = "error";
                }
            }

            return result;
        }

        public bool IsOpen()
        {
            return port != null && port.IsOpen;
        }
    }
}