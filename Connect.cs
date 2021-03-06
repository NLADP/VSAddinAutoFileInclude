﻿using System;
using System.Reflection;
using Extensibility;
using EnvDTE;
using EnvDTE80;
namespace FileIncluder
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2
	{
        private Window2 ToolWin;
        private UserControl1 IncludeControl;

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
            _applicationObject.Events.DTEEvents.OnStartupComplete += () => OnStartupComplete();

            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
//                object[] contextGUIDS = new object[] { };
//                Commands2 commands = (Commands2)_applicationObject.Commands;
//                string toolsMenuName = "Tools";

                //Place the command on the tools menu.
                //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
//                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];
//
//                //Find the Tools command bar on the MenuBar command bar:
//                CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
//                CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
//                try
//                {
//                    //Add a command to the Commands collection:
//                    Command command = commands.AddNamedCommand2(_addInInstance, "FileIncluder", "Include files", "Executes the command for FileIncluder", true, 58, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
//
//                    //Add a control for the command to the tools menu:
//                    if ((command != null) && (toolsPopup != null))
//                    {
//                        command.AddControl(toolsPopup.CommandBar, 1);
//                    }
//                }
//                catch (System.ArgumentException)
//                {
//                    //If we are here, then the exception is probably because a command with that name
//                    //  already exists. If so there is no need to recreate the command and we can 
//                    //  safely ignore the exception.
//                }
            }
            else if (connectMode == ext_ConnectMode.ext_cm_AfterStartup)
            {
                CreateToolWindow();
            }
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

        public void OnStartupComplete()
        {
            CreateToolWindow();
        }

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
            CreateToolWindow();
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}

        private void CreateToolWindow()
        {
            var asmPath = Assembly.GetExecutingAssembly().Location;
            const string guid = "{87fae1a4-0fd6-4929-80c8-98d297cb9751}";
            object objTemp = null;

            try
            {
                ToolWin = (Window2)_applicationObject.Windows.Item(guid);
            }
            catch (ArgumentException ex) { }

            if (ToolWin == null)
            {
                ToolWin = (Window2)((Windows2)_applicationObject.Windows).CreateToolWindow2(_addInInstance, asmPath, typeof(UserControl1).FullName, "Include files", guid, ref objTemp);
            }

            IncludeControl = (UserControl1)objTemp;

            ToolWin.Visible = true;
            //	        toolWin.Height = 500;
            //	        toolWin.Width = 400;

            if (IncludeControl != null) IncludeControl.Init(_applicationObject);
        }
		
		private DTE2 _applicationObject;
		private AddIn _addInInstance;
	}
}