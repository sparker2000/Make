using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Make
{
   /// <summary>
   /// A page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class MainPage : Page
   {
      public MainPage()
      {
         this.InitializeComponent();

         // For future use.  Eventually, we want this program to always run in the background so that we can
         // tell Cortana to run commands without having to first launch an app.
         //RegisterCortanaIntegrationTask(); 
      }

      /// <summary>
      /// Probably not the best place to do this, but tutorials show loading the voice commands when the ManePage of the desktop app loads.
      /// </summary>
      private async void Page_Loaded(object sender, RoutedEventArgs e)
      {
         // storage file containing the "Make" voice commands
         Windows.Storage.StorageFile storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync( new Uri("ms-appx:///MakeCortanaCommands.xml"));

         // Install the voice commands
         await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);
      }

      /// <summary>
      /// Registers the background task that makes cortana listen to defined keywords
      /// </summary>
      private void RegisterCortanaIntegrationTask()
      {
         // Tracks whether the background task has been registerd or not.
         bool taskRegistered = false;
         string taskName = "CortanaMake";

         // Search background tasks registrations for the task name
         foreach (KeyValuePair<Guid, IBackgroundTaskRegistration> task in BackgroundTaskRegistration.AllTasks)
         {
            if (task.Value.Name == taskName)
            {
               taskRegistered = true;
               break;
            }
         }

         // Register the task if it hasn't already been registered
         if(!taskRegistered)
         {
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = taskName;
            builder.TaskEntryPoint = "BackgroundComponent"; // This is the name of a background task that needs to get created.  When the task is run, 
                                                            // the public void Run(IBackgroundTaskInstance taskInstance) will be called in this library.  
                                                            // We also have to register this in our app manifest.
            builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
            BackgroundTaskRegistration task = builder.Register();
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCortanaMakeTaskCompleted);
         }
      }

      /// <summary>
      /// Event fires after background task completes.
      /// </summary>
      /// <param name="task"></param>
      /// <param name="args"></param>
      private void OnCortanaMakeTaskCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
      {
         Windows.Storage.ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;
         string key = task.TaskId.ToString();
         string message = settings.Values[key].ToString();
      }
   }
}
