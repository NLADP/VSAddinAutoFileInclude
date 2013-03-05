using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;

namespace FileIncluder
{
    public class FileWatcher
    {
        public event ChangedHandler Changed;

        public delegate void ChangedHandler(IncludeItem item);

        private Dictionary<FileSystemWatcher, Project> _watchers = new Dictionary<FileSystemWatcher, Project>();

        public FileWatcher(Solution solution)
        {
            foreach (var project in solution.Projects.Cast<Project>())
            {
                try
                {
                    if (project.Properties == null) continue;

                    var p = (string) project.Properties.Item("FullPath").Value;

                    // check if it doesnt crash on getting items
                    if (project.ProjectItems == null) continue;

                    CreateWatcher(project, p);
                }
                catch {}
            }
        }

        private void CreateWatcher(Project project, string folder)
        {
            var watcher = new FileSystemWatcher(folder, "*.cs");

            watcher.Created += WatcherOnCreated;
            watcher.NotifyFilter = NotifyFilters.FileName;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            _watchers.Add(watcher, project);
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var project = _watchers[(FileSystemWatcher)sender];

            foreach (ProjectItem projectItem in project.ProjectItems)
            {
                var fileName = projectItem.FileNames[0];
                if (fileName == fileSystemEventArgs.FullPath) 
                    return;
            }

            var item = new IncludeItem {Project = project.Name, ProjectId = project.UniqueName, File = fileSystemEventArgs.Name, FullPath = fileSystemEventArgs.FullPath};

            if (Changed != null) Changed.Invoke(item);
        }
    }
}
