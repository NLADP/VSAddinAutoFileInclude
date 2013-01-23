using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;

namespace AutoFileInclude
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2
	{
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Dictionary<FileSystemWatcher, Project> _watchers = new Dictionary<FileSystemWatcher, Project>();
        private Queue<QueueItem> _pendingItems = new Queue<QueueItem>();
        private bool _isListening;

		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;
			
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
            if (_applicationObject.Solution == null || !_applicationObject.Solution.IsOpen)
            {
                _applicationObject.Events.SolutionEvents.Opened += SolutionEventsOnOpened;
            }
            else
            {
                SolutionEventsOnOpened();
            }
		}

        private void SolutionEventsOnOpened()
        {
            if (_applicationObject.Solution == null) throw new InvalidOperationException("No solution");

            foreach (var project in _applicationObject.Solution.Projects.Cast<Project>())
            {
                var items = project.ProjectItems;

                foreach (var projectItem in items.Cast<ProjectItem>().Where(i => i.Kind == VSConstants.ItemTypeGuid.PhysicalFolder_string))
                {
                    var folder = (string)projectItem.Properties.Item("FullPath").Value;

                    var watcher = new FileSystemWatcher(folder, "*.cs");

                    watcher.Created += WatcherOnCreated;
                    watcher.NotifyFilter = NotifyFilters.FileName;
                    watcher.EnableRaisingEvents = true;

                    _watchers.Add(watcher, project);
                }
            }
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            if (!_isListening)
            {
                _applicationObject.Events.WindowEvents.WindowActivated += (focus, lostFocus) => { if (focus.Type == vsWindowType.vsWindowTypeSolutionExplorer) ProccessQueue(); };
                _isListening = true;
            }

            //            Print("File created: {0}, {1}, {2}", fileSystemEventArgs.ChangeType, fileSystemEventArgs.Name, fileSystemEventArgs.FullPath);

            var project = _watchers[(FileSystemWatcher)sender];

            _pendingItems.Enqueue(new QueueItem { Project = project, Args = fileSystemEventArgs });
        }

        private void ProccessQueue()
        {
            if (_pendingItems.Count == 0) return;

            var timer = new Timer {Interval = 200};
            timer.Tick += (sender, args) =>
                {
                    timer.Stop();

                    var files = string.Join(Environment.NewLine, from item in _pendingItems select item.Args.Name);

                    if (MessageBox.Show("Files added, you want to include them in the project?" + Environment.NewLine + Environment.NewLine + files, "Include files??",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        while (_pendingItems.Count > 0)
                        {
                            var item = _pendingItems.Dequeue();
                            AddFile(item.Project, item.Args);
                        }
                    }

                    timer = null;
                };
            timer.Start();
        }

        private void AddFile(Project project, FileSystemEventArgs fileSystemEventArgs)
        {
            var newItem = project.ProjectItems.AddFromFile(fileSystemEventArgs.FullPath);

//            _applicationObject.ToolWindows.SolutionExplorer.GetItem(GetSolutionPath(newItem, true)).Select(vsUISelectionType.vsUISelectionTypeSelect);

            //            _applicationObject.Windows.Item(Constants.vsWindowKindSolutionExplorer).Activate();
        }



		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
	}

    internal class QueueItem
    {
        public Project Project { get; set; }
        public FileSystemEventArgs Args { get; set; }
    }
}