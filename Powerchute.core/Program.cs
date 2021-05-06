using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Powerchute.core
{
    class Program
    {
        static void Main(string[] args)
        {
            int portNumber = 3052;
            Console.WriteLine("!!!Dont forget to add the ip address of this device to your apc under UPS>Powerchute>clients!!!");
            UdpClient udpClient = new UdpClient(portNumber);
            IPEndPoint pEndPoint = new IPEndPoint(IPAddress.Any, portNumber);
            Console.WriteLine("Waiting for packets...");
            while (true)
            {
                byte[] recieveByte = udpClient.Receive(ref pEndPoint);
                Console.WriteLine(string.Concat("Recieved broadcast from: ", pEndPoint.Address.ToString()));
                APCInfo info = new APCInfo();
                string[] apcData = Encoding.ASCII.GetString(recieveByte).Split('\n');

                foreach (string data in apcData)
                {
                    string[] values = data.Split('=');
                    if (values.Length > 1)
                    {
                        string str = values[0];
                        switch (str)
                        {
                            case "42":
                                {
                                    decimal.TryParse(values[1], NumberStyles.Number , new CultureInfo("en-US"), out info.BatteryVolate);
                                    break;
                                }
                            case "43":
                                {
                                    decimal.TryParse(values[1], NumberStyles.Number, new CultureInfo("en-US"), out info.InternalTemperature);
                                    break;
                                }
                            case "4C":
                                {
                                    decimal.TryParse(values[1], NumberStyles.Number, new CultureInfo("en-US"), out info.InputLineVoltage);
                                    break;
                                }
                            case "50":
                                {
                                    decimal.TryParse(values[1], NumberStyles.Number, new CultureInfo("en-US"), out info.PowerLoad);
                                    break;
                                }
                            case "51":
                                {
                                    int.TryParse(values[1], out info.StatusFlag);
                                    info.Flag = (APCInfo.StatusFlags)info.StatusFlag;
                                    break;
                                }
                            case "66":
                                {
                                    decimal.TryParse(values[1], NumberStyles.Number, new CultureInfo("en-US"), out info.BatteryLevel);
                                    break;
                                }
                            case "6A":
                                {
                                    values[1] = values[1].Remove((int)values.Length - 1, 1);
                                    int.TryParse(values[1], out info.EstimatedRuntime);
                                    break;
                                }
                        }
                    }
                }
                if (info.StatusFlag != 0)
                {
                    Console.WriteLine(string.Concat("Battery voltage: ", info.BatteryVolate.ToString("0.0", CultureInfo.InvariantCulture) + " V"));
                    Console.WriteLine(string.Concat("Internal Temperature: ", info.InternalTemperature.ToString("0.0", CultureInfo.InvariantCulture) + " °C"));
                    Console.WriteLine(string.Concat("Inputline Voltage: ", info.InputLineVoltage.ToString("0.0", CultureInfo.InvariantCulture) + " V"));
                    Console.WriteLine(string.Concat("Power load: ", info.PowerLoad.ToString("0.0", CultureInfo.InvariantCulture) +  " %"));
                    Console.WriteLine(string.Concat("Status: ", info.Flag.ToString()));
                    Console.WriteLine(string.Concat("Battery level: ", info.BatteryLevel.ToString("0.0", CultureInfo.InvariantCulture) + " %"));
                    Console.WriteLine(string.Concat("Estimated runtime: ", info.EstimatedRuntime.ToString(), " m"));
                }
                if (info.Flag == Program.APCInfo.StatusFlags.OnBattery_OK)
                {
                }
                if (info.Flag == Program.APCInfo.StatusFlags.OnBattery_Low)
                {
                }
            }
        }

        private struct APCInfo
        {
            public decimal BatteryVolate;

            public decimal InternalTemperature;

            public decimal InputLineVoltage;

            public decimal PowerLoad;

            public int StatusFlag;

            public StatusFlags Flag;

            public decimal BatteryLevel;

            public int EstimatedRuntime;

            public enum StatusFlags
            {
                OnLine_OK = 8,
                OnBattery_OK = 10,
                OnBattery_Low = 50
            }
        }
    }
}
