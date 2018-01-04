using Microsoft.Xna.Framework;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;

namespace Conesoft.Engine.Accelerometer
{
    interface IAccelerometer
    {
        Vector3 Acceleration { get; }
    }

    namespace Implementation
    {
        namespace Xna
        {
            class Accelerometer : IAccelerometer
            {
                private Windows.Devices.Sensors.Accelerometer acc;
                private AccelerometerReading reading;

                public Accelerometer()
                {
                    acc = Windows.Devices.Sensors.Accelerometer.GetDefault(AccelerometerReadingType.Standard);
                    if (acc != null)
                    {
                        acc.ReadingChanged += Acc_ReadingChanged;
                    }
                }

                private void Acc_ReadingChanged(Windows.Devices.Sensors.Accelerometer sender, AccelerometerReadingChangedEventArgs args)
                {
                    reading = args.Reading;
                }

                public Vector3 Acceleration
                {
                    get
                    {
                        var v = Vector3.Up;
                        if (acc != null)
                        {
                            DisplayInformation displayInfo = DisplayInformation.GetForCurrentView();
                            switch (displayInfo.CurrentOrientation)
                            {
                                case DisplayOrientations.Landscape:
                                    v.X = (float)reading.AccelerationX;
                                    v.Y = (float)reading.AccelerationY;
                                    v.Z = (float)reading.AccelerationZ;
                                    break;
                                case DisplayOrientations.Portrait:
                                    v.Y = -(float)reading.AccelerationX;
                                    v.X = (float)reading.AccelerationY;
                                    v.Z = (float)reading.AccelerationZ;
                                    break;
                                case DisplayOrientations.LandscapeFlipped:
                                    v.X = -(float)reading.AccelerationX;
                                    v.Y = -(float)reading.AccelerationY;
                                    v.Z = (float)reading.AccelerationZ;
                                    break;
                                case DisplayOrientations.PortraitFlipped:
                                    v.Y = (float)reading.AccelerationX;
                                    v.X = -(float)reading.AccelerationY;
                                    v.Z = (float)reading.AccelerationZ;
                                    break;
                            }
                            if (displayInfo.NativeOrientation == DisplayOrientations.Portrait)
                            {
                                v = new Vector3(-v.Y, v.X, v.Z);
                            }
                        }
                        return Vector3.Normalize(v);
                    }
                }
            }
        }
    }
}
