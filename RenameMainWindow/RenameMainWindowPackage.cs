using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace RenameMainWindow
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(RenameMainWindowPackage.PackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class RenameMainWindowPackage : AsyncPackage
    {
        private const string SolutionUserOptionKey = "RenameMainWindowUserOptionKey";



        /// <summary>
        /// RenameMainWindowPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "7b5d1430-ce7a-4463-a86a-2790d6e2c170";

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            AddOptionKey(SolutionUserOptionKey);

            Configuration = LoadConfiguration();

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Since this package might not be initialized until after a solution has finished loading,
            // we need to check if a solution has already been loaded and then handle it.
            bool isSolutionLoaded = await IsSolutionLoadedAsync();


            if (isSolutionLoaded)
            {
                HandleOpenSolution();
            }

            // Listen for subsequent solution events
            SolutionEvents.OnAfterBackgroundSolutionLoadComplete += HandleOpenSolution;
        }

        private RenameWindowConfiguration Configuration { get; set; }

        private static RenameWindowConfiguration LoadConfiguration()
        {
            string value = Environment.GetEnvironmentVariable("VS2019_SOLUTIONS");
            if (File.Exists(value))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RenameWindowConfiguration));
                using (XmlReader reader = XmlReader.Create(value))
                {
                    RenameWindowConfiguration configuration = (RenameWindowConfiguration)serializer.Deserialize(reader);
                    return configuration;
                }
            }
            return RenameWindowConfiguration.Empty;
        }

        private async Task<bool> IsSolutionLoadedAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            if (solService == null)
            {
                throw new InvalidOperationException();
            }
            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));
            return value is bool isSolOpen && isSolOpen;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<Pending>")]
        private async void HandleOpenSolution(object sender = null, EventArgs e = null)
        {
            // Handle the open solution and try to do as much work
            // on a background thread as possible

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2 dte = (DTE2)await GetServiceAsync(typeof(EnvDTE.DTE));

            if (dte != null && GetService(typeof(SVsSolution)) is IVsSolution solService)
            {
                solService.GetProperty((int)__VSPROPID.VSPROPID_SolutionFileName, out object solutionFileName);

                SolutionInformation solutionInformation = Configuration.Find((string)solutionFileName);
                if (solutionInformation != null)
                {
                    SetMainWindowCaption(solutionInformation.DisplayName);
                }
            }
        }

        private static void SetMainWindowCaption(string title)
        {
            Window mainWindow = Application.Current.MainWindow;
            FrameworkElement element = FindChild<FrameworkElement>(mainWindow, "PART_SolutionNameTextBlock");
            PropertyInfo property = element.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
            if(property != null)
            {
                property.SetValue(element, title);
            }
        }

        public static T FindChild<T>(DependencyObject parent, string elementName) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int index = 0; index < count; ++index)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, index);
                if (child != null && child is FrameworkElement element)
                {
                    if (element.Name == elementName)
                    {
                        return element as T;
                    }
                }
                T found = FindChild<T>(child, elementName);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        protected override void OnLoadOptions(string key, Stream stream)
        {
            base.OnLoadOptions(key, stream);
        }

        protected override void OnSaveOptions(string key, Stream stream)
        {
            base.OnSaveOptions(key, stream);
        }

    }
}
