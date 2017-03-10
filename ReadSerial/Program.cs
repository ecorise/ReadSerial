using Ecorise.Equipment.SerialInput;
using Ecorise.Utils;
using System;
using System.Text;
using System.Threading;

namespace ReadSerial
{
    public enum Mode
    {
        Init,
        Running,
        Exit
    };

    public class Program
    {
        SerialInputDevice serialInputDevice;

        private Logger log = new Logger("Lux");
        private StringBuilder sbLog = new StringBuilder();
        private StringBuilder sbConsole = new StringBuilder();

        private string serialInputDeviceComPort = "com7";

        void Run()
        {
            Output(log, "Date et heure\tFull\tIR\tVisible\tLux\tUV light level\tVis\tIR\tUV index");
            Output(log, "", true);

            Mode mode = Mode.Init;

            while (mode != Mode.Exit)
            {
                if (mode == Mode.Running)
                {
                }
                else if (mode == Mode.Init)
                {
                    serialInputDevice = new SerialInputDevice();
                    serialInputDevice.Open(serialInputDeviceComPort);

                    if (serialInputDevice.IsOpen)
                    {
                        Console.WriteLine($"Le port de communication {serialInputDeviceComPort} est ouvert.");
                        Console.WriteLine("");

                        serialInputDevice.LineReceived += SerialLineReceived;

                        mode = Mode.Running;
                    }
                    else
                    {
                        DateTime now = DateTime.UtcNow;
                        LogDateTime(now);
                        Output(log, String.Format($"Impossible d'ouvrir le port de communication {serialInputDeviceComPort} !"), true);
                        Thread.Sleep(500);
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

        private void Output(Logger log, string s, bool endOfLine = false, bool doNotDisplayOnConsole = false)
        {
            sbLog.Append(s);
            if (!doNotDisplayOnConsole) sbConsole.Append(s);

            if (endOfLine)
            {
                Console.WriteLine(sbConsole.ToString());
                log.Log(sbLog.ToString());
                sbLog.Clear();
                sbConsole.Clear();
            }
        }

        private void LogDateTime(DateTime utcNow)
        {
            DateTime now = utcNow.ToLocalTime();
            Output(log, String.Format("{0:00}.{1:00}.{2:0000} {3:00}:{4:00}:{5:00}\t", now.Day, now.Month, now.Year, now.Hour, now.Minute, now.Second));
        }

        public void SerialLineReceived(object sender, SerialInputEventArgs e)
        {
            LogDateTime(DateTime.UtcNow);
            Output(log, e.Text, true);
        }

        public static void Main()
        {
            // Added to prevent sleep mode. See http://stackoverflow.com/questions/6302185/how-to-prevent-windows-from-entering-idle-state
            uint fPreviousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);

            if (fPreviousExecutionState == 0)
            {
                Console.WriteLine("SetThreadExecutionState failed.");
            }

            try
            {
                new Program().Run();
            }
            finally
            {
                // Restore previous state
                NativeMethods.SetThreadExecutionState(fPreviousExecutionState);
            }
        }
    }
}
