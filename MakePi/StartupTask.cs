using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace MakePi
{

   public sealed class StartupTask : IBackgroundTask
   {
      public void Run(IBackgroundTaskInstance taskInstance)
      {
         taskInstance.GetDeferral(); // Prevents background task from completing until 1. app is killed or 2. taskInstance.GetDeferral().Complete() is called.
         StartListener();
      }

      private void InitGPIO(int pinNum, out GpioPin pin)
      {
         GpioController gpio = GpioController.GetDefault();

         // GPIO pin initialized correctly.
         GpioOpenStatus pinStatus;
         gpio.TryOpenPin(pinNum, GpioSharingMode.Exclusive, out pin, out pinStatus);
         if (pinStatus == GpioOpenStatus.PinOpened)
         {
            pin.SetDriveMode(GpioPinDriveMode.Output);
            _initializedPins.Add(pinNum, pin);
         }
         else
         {
            pin = _initializedPins[pinNum];
         }
      }

      private Dictionary<int, GpioPin> _initializedPins = new Dictionary<int, GpioPin>();


      private async void StartListener()
      {
         try
         {
            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnection;
            HostName name = new HostName("192.168.1.196");
            await listener.BindEndpointAsync(name, "5656");
         }
         catch (Exception exception)
         {
            // If this is an unknown status it means that the error is fatal and retry will likely fail.
            if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
            {
               throw;
            }
         }
      }

      private async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
      {
         string message = "";
         DataReader reader = new DataReader(args.Socket.InputStream);
         try
         {
            while (true)
            {
               // Read first 4 bytes (length of the subsequent string).
               uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
               if (sizeFieldCount != sizeof(uint))
               {
                  // The underlying socket was closed before we were able to read the whole data.
                  return;
               }

               // Read the string.
               uint stringLength = reader.ReadUInt32();
               uint actualStringLength = await reader.LoadAsync(stringLength);
               if (stringLength != actualStringLength)
               {
                  // The underlying socket was closed before we were able to read the whole data.
                  return;
               }

               // Read the input
               message = reader.ReadString(actualStringLength);
               break;
            }
         }
         catch (Exception exception)
         {
            // If this is an unknown status it means that the error is fatal and retry will likely fail.
            if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
            {
               throw;
            }
         }

         try
         {
            // set the state of the pin based on inputs
            if (!String.IsNullOrWhiteSpace(message))
            {
               int ledPin = Int32.Parse(message.Split(new char[] { ',' })[0].Trim());

               // I'm tired... so for now you only get high and low.
               GpioPinValue state = message.Split(new char[] { ',' })[1].Trim().ToLower() == "on" ? GpioPinValue.High : GpioPinValue.Low;
               
               SetPinState(ledPin, state);
            }

            // For later use with UI logic
            // In order to update the UI, we have to use the thread associated with the UI.
            //await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            //() =>
            //{
            //   if (pinValue == GpioPinValue.High)
            //   {
            //      LED.Fill = redBrush;
            //   }
            //   else
            //   {
            //      LED.Fill = grayBrush;
            //   }
            //});
         }
         catch (Exception ex)
         {
            // Used for debugging
         }
      }

      private void SetPinState(int pinNum, GpioPinValue state)
      {
         GpioPin pin;
         InitGPIO(pinNum, out pin);
         pin.Write(state);
      }
   }
}
