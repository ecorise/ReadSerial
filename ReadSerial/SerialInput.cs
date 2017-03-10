using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Ecorise.Equipment.SerialInput
{
    public class SerialInputEventArgs
    {
        public SerialInputEventArgs(string s) { Text = s; }
        public String Text { get; private set; }
    }

    public class SerialInputDevice : IDisposable
    {

        protected SerialPort serialPort;
        protected StringBuilder sb;
        public delegate void LineReceivedDelegate(object sender, SerialInputEventArgs e);
        public event LineReceivedDelegate LineReceived;

        public SerialInputDevice()
        {
            serialPort = null;
            sb = new StringBuilder(2000);
        }

        public void Open(string serialPortName)
        {
            if (IsOpen)
            {
                Close();
            }

            sb.Clear();

            serialPort = new SerialPort(serialPortName);
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.ReadTimeout = 0;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPortDataReceived);
            serialPort.Open();
        }

        public void Close()
        {
            if (serialPort != null)
            {
                serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialPortDataReceived);
                serialPort.Close();
                serialPort = null;
            }
        }

        public bool IsOpen
        {
            get
            {
                return (serialPort != null) ? serialPort.IsOpen : false;
            }
        }

        public SerialPort ComPort
        {
            get
            {
                return serialPort;
            }

            set
            {
                if (serialPort != value)
                {
                    Close();
                    serialPort = value;
                }
            }
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialPort.BytesToRead > 0)
            {
                int ch = serialPort.ReadChar();

                if ((ch != '\r') && (ch != '\n'))
                {
                    sb.Append((char)ch);
                }

                if (ch == '\n')
                {
                    LineReceived?.Invoke(this, new SerialInputEventArgs(sb.ToString()));
                    sb.Clear();
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Pour detecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    serialPort.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
