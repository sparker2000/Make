using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Make
{
   /// <summary>
   /// Provides application-specific behavior to supplement the default Application class.
   /// </summary>
   sealed partial class App : Application
   {
      /// <summary>
      /// Initializes the singleton application object.  This is the first line of authored code
      /// executed, and as such is the logical equivalent of main() or WinMain().
      /// </summary>
      public App()
      {
         Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
            Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
            Microsoft.ApplicationInsights.WindowsCollectors.Session);
         this.InitializeComponent();
         this.Suspending += OnSuspending;
      }

      /// <summary>
      /// Override the OnActivated method to use args when app is activated by voice commands
      /// </summary>
      protected override void OnActivated(IActivatedEventArgs args)
      {
         if(args.Kind == ActivationKind.VoiceCommand)
         {
            // Get the args passed in from voice activation
            VoiceCommandActivatedEventArgs commandArgs = args as VoiceCommandActivatedEventArgs;
            
            State ledState;
            int ledNum;
            ParseCommand(commandArgs.Result.Text, out ledNum, out ledState);

            if (ledNum > 0)
            {
               ChangeLedState(ledNum, ledState);
            }
         }

         base.OnActivated(args);
      }

      private void ParseCommand(string text, out int ledNum, out State ledState)
      {
         ledNum = 0;
         ledState = State.Invalid;

         // Get the led number
         string number = text.Split(new char[] { ' ' })[1].Trim();  // second word should always indicate the number

         Dictionary<string, int> integerValues = new Dictionary<string, int>();
         integerValues.Add("two", 2);
         integerValues.Add("2", 2);
         integerValues.Add("three", 3);
         integerValues.Add("3", 3);
         integerValues.Add("four", 4);
         integerValues.Add("4", 4);
         integerValues.Add("five", 5);
         integerValues.Add("5", 5);
         integerValues.Add("six", 6);
         integerValues.Add("6", 6);
         integerValues.Add("seven", 7);
         integerValues.Add("7", 7);
         integerValues.Add("eight", 8);
         integerValues.Add("8", 8);
         integerValues.Add("nine", 9);
         integerValues.Add("9", 9);
         integerValues.Add("ten", 10);
         integerValues.Add("10", 10);

         if(integerValues.ContainsKey(number))
         {
            ledNum = integerValues[number];
         }

         // Get the state
         if (text.Contains("turn on"))
         {
            ledState = State.On;
         }
         else if(text.Contains("turn off"))
         {
            ledState = State.Off;
         }
         else if(text.Contains("toggle"))
         {
            ledState = State.Toggle;
         }
         else if(text.Contains("blink"))
         {
            ledState = State.Blinking;
         }
      }

      private enum State
      {
         Off = 0,
         On = 1,
         Toggle = 2,
         Blinking = 3,
         Invalid = 4
      }
      
      private async void ChangeLedState(int ledNum, State state)
      {
         using (StreamSocket socket = new StreamSocket())
         {
            HostName host = new HostName("192.168.1.196");
            try
            {
               await socket.ConnectAsync(host, "5656");

               using (DataWriter writer = new DataWriter(socket.OutputStream))
               {
                  string stringToSend = String.Format("{0}, {1}", ledNum, state);
                  writer.WriteUInt32(writer.MeasureString(stringToSend));
                  writer.WriteString(stringToSend);
                  await writer.StoreAsync();
               }
            }
            catch (Exception exception)
            {
               // If this is an unknown status it means that the error if fatal and retry will likely fail.
               if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
               {
                  throw;
               }
            }
         }
      }

      /// <summary>
      /// Invoked when the application is launched normally by the end user.  Other entry points
      /// will be used such as when the application is launched to open a specific file.
      /// </summary>
      /// <param name="e">Details about the launch request and process.</param>
      protected override void OnLaunched(LaunchActivatedEventArgs e)
      {

#if DEBUG
         if (System.Diagnostics.Debugger.IsAttached)
         {
            this.DebugSettings.EnableFrameRateCounter = true;
         }
#endif

         Frame rootFrame = Window.Current.Content as Frame;

         // Do not repeat app initialization when the Window already has content,
         // just ensure that the window is active
         if (rootFrame == null)
         {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            rootFrame.NavigationFailed += OnNavigationFailed;

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
               //TODO: Load state from previously suspended application
            }

            // Place the frame in the current Window
            Window.Current.Content = rootFrame;
         }

         if (rootFrame.Content == null)
         {
            // When the navigation stack isn't restored navigate to the first page,
            // configuring the new page by passing required information as a navigation
            // parameter
            rootFrame.Navigate(typeof(MainPage), e.Arguments);
         }
         // Ensure the current window is active
         Window.Current.Activate();
      }

      /// <summary>
      /// Invoked when Navigation to a certain page fails
      /// </summary>
      /// <param name="sender">The Frame which failed navigation</param>
      /// <param name="e">Details about the navigation failure</param>
      void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
      {
         throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
      }

      /// <summary>
      /// Invoked when application execution is being suspended.  Application state is saved
      /// without knowing whether the application will be terminated or resumed with the contents
      /// of memory still intact.
      /// </summary>
      /// <param name="sender">The source of the suspend request.</param>
      /// <param name="e">Details about the suspend request.</param>
      private void OnSuspending(object sender, SuspendingEventArgs e)
      {
         var deferral = e.SuspendingOperation.GetDeferral();
         //TODO: Save application state and stop any background activity
         deferral.Complete();
      }
   }
}
