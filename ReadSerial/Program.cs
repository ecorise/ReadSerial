using System;
using System.Text;
using System.Threading;
using Ecorise.Equipment.SerialInput;
using Ecorise.Utils;

namespace ReadSerial
{
    public class Program : IDisposable
    {
        enum Mode
        {
            Init,
            Running,
            Exit
        };

        private string serialInputDeviceComPort = "com7";
        private SerialInputDevice serialInputDevice;
        private Logger log = new Logger("Lux");

        void Run()
        {
            log.Log("Date et heure\tFull\tIR\tVisible\tLux\tUV light level\tVis\tIR\tUV index\n");

            Mode mode = Mode.Init;

            while (mode != Mode.Exit)
            {
                if (mode == Mode.Running)
                {
                    if (!serialInputDevice.IsOpen)
                    {
                        mode = Mode.Init;
                    }
                }
                else if (mode == Mode.Init)
                {
                    serialInputDevice = new SerialInputDevice();

                    if (serialInputDevice.Open(serialInputDeviceComPort))
                    {
                        serialInputDevice.LineReceived += SerialLineReceived;
                        mode = Mode.Running;
                    }
                    else
                    {
                        DateTime now = DateTime.UtcNow;
                        log.LogDateTime(now);
                        log.Log($"\tImpossible d'ouvrir le port de communication {serialInputDeviceComPort} !\n");
                        Thread.Sleep(900);
                    }
                }

                if (Console.KeyAvailable)
                {
                    char ch = Console.ReadKey(true).KeyChar;

                    switch (ch)
                    {
                        case 'q':
                            mode = Mode.Exit;
                            break;

                        default:
                            Console.WriteLine("Appuyez sur q pour quitter...");
                            break;
                    }

                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                    }
                }

                Thread.Sleep(100);
            }

            serialInputDevice.LineReceived -= SerialLineReceived;
            serialInputDevice?.Close();
        }

        public void SerialLineReceived(object sender, SerialInputEventArgs e)
        {
            log.LogDateTime(DateTime.UtcNow);
            log.Log("\t{0}\n", e.Text);
        }

        public static void Main()
        {
            using (new PreventSleepMode())
            {
                using (Program program = new Program())
                {
                    program.Run();
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
                    serialInputDevice.Close();
                    serialInputDevice.Dispose();
                    serialInputDevice = null;
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
