/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" 
 * As long as you retain this notice you can do whatever you want with this 
 * stuff. If you meet an employee from Windward meet some day, and you think
 * this stuff is worth it, you can buy them a beer in return. Windward Studios
 * ----------------------------------------------------------------------------
 */

using System;
using System.Windows.Forms;
using log4net;
using log4net.Config;

namespace Windwardopolis2
{
	internal static class Program
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (Program));
		public static int exitCode = 0;

		/// <summary>
		///     The main entry point for the application.
		/// </summary>
		[STAThread]
		private static int Main()
		{
			XmlConfigurator.Configure();
			log.Info("***** Windwardopolis II starting *****");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainWindow());
			return exitCode;
		}
	}
}