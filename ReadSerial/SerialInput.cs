using System;
using System.IO.Ports;
using System.Text;

namespace Ecorise.Equipment.SerialInput
{
    public class SerialInputEventArgs : EventArgs
    {
        public SerialInputEventArgs(string s) { Text = s; }
        public String Text { get; private set; }
    }

    public class SerialInputDevice : IDisposable
    {
        protected SerialPort serialPort;
        private string portName;
        protected StringBuilder sb;
        public event EventHandler<SerialInputEventArgs> LineReceived;

        public SerialInputDevice()
        {
            sb = new StringBuilder(2000);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Supprimer les objets avant la mise hors de portée")]
        public bool Open(string serialPortName)
        {
            if (IsOpen && (serialPortName != portName))
            {
                Close();
            }

            portName = serialPortName;
            sb.Clear();

            try
            {
                serialPort = new SerialPort(serialPortName)
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    ReadTimeout = 500
                };

                serialPort.DataReceived += SerialPortDataReceived;

                serialPort.Open();
            }
            catch (System.IO.IOException)
            {
                // Ignore "com port does not exist"
                serialPort.Dispose();
                serialPort = null;
                return false;
            }

            return serialPort.IsOpen;
        }

        protected void Reopen()
        {
            Close();
            Open(portName);
        }

        public void Close()
        {
            if (serialPort != null)
            {
                serialPort.DataReceived -= SerialPortDataReceived;
                serialPort.Close();
                serialPort = null;
            }
        }

        public bool IsOpen => (serialPort != null) ? serialPort.IsOpen : false;

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialPort.BytesToRead > 0)
            {
                try
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
                catch (InvalidOperationException)
                {
                    // Port closed
                    try
                    {
                        Reopen();
                    }
                    catch (InvalidOperationException)
                    {
                        // Ignore "com port does not exist"
                        return;
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    serialPort?.Close();
                    portName = null;
                    LineReceived = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
