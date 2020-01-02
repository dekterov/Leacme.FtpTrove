// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using FluentFTP;
using Leacme.Lib.FtpTrove;

namespace Leacme.App.FtpTrove {

	public class AppUI {

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library lib = new Library();

		FtpClient client = null;
		string currentLocalDir;

		public AppUI() {

			var lb1 = new ListBox();
			lb1.Width = lb1.Height = 300;
			rootPan.Spacing = 6;

			var blurb1 = App.TextBlock;
			blurb1.TextAlignment = TextAlignment.Center;
			blurb1.Text = "Connect to an FTP server.";

			var partcPanel = App.HorizontalStackPanel;
			partcPanel.HorizontalAlignment = HorizontalAlignment.Center;
			var hostBlurb = App.TextBlock;
			hostBlurb.Text = "Host:";
			var hostField = App.TextBox;
			hostField.Width = 130;

			var portBlurb = App.TextBlock;
			portBlurb.Text = "Port:";
			var portField = App.TextBox;
			portField.Width = 50;
			portField.Text = "21";

			var userBlurb = App.TextBlock;
			userBlurb.Text = "Username:";
			var userField = App.TextBox;

			var passBlurb = App.TextBlock;
			passBlurb.Text = "Password:";
			var passField = App.TextBox;
			passField.PasswordChar = '*';

			var connectBt = App.Button;
			connectBt.Content = "Connect";
			var disconnectBt = App.Button;
			disconnectBt.Content = "Disconnect";
			disconnectBt.IsEnabled = false;

			partcPanel.Children.AddRange(new List<IControl> { hostBlurb, hostField, portBlurb, portField, userBlurb, userField, passBlurb, passField, connectBt, disconnectBt });

			var upDirLocal = App.Button;
			upDirLocal.Content = "Up";
			upDirLocal.Width = 50;
			var ph1 = App.TextBlock;
			var ph2 = App.TextBlock;
			var ph3 = App.TextBlock;
			ph1.Text = ph2.Text = ph3.Text = " ";
			ph1.Width = ph2.Width = 240;
			var ulBt = App.Button;
			ulBt.Content = "Upload >";

			var upDirRemote = App.Button;
			upDirRemote.Content = "Up";
			upDirRemote.Width = 50;
			var dlBt = App.Button;
			dlBt.Content = "< Download";

			upDirLocal.IsEnabled = false;
			ulBt.IsEnabled = false;
			upDirRemote.IsEnabled = false;
			dlBt.IsEnabled = false;

			var partcPanel2 = App.HorizontalStackPanel;
			partcPanel2.HorizontalAlignment = HorizontalAlignment.Center;
			partcPanel2.Children.AddRange(new List<IControl> { upDirLocal, ph1, ulBt, dlBt, ph2, upDirRemote });
			var border1 = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightSlateGray, Width = 900 };
			border1.Child = partcPanel2;

			var localLb = App.DataGrid;
			var remoteLb = App.DataGrid;
			localLb.Width = remoteLb.Width = 430;
			localLb.Height = remoteLb.Height = 350;
			localLb.CanUserReorderColumns = remoteLb.CanUserReorderColumns = true;
			localLb.CanUserResizeColumns = remoteLb.CanUserResizeColumns = true;

			void ShowErrorMessage(string message) {
				string oriBlurb = blurb1.Text;
				blurb1.Text = message;
				DispatcherTimer.RunOnce(() => {
					blurb1.Text = oriBlurb;
				}, new TimeSpan(0, 0, 0, 5, 0));
			}

			upDirRemote.Click += async (z, zz) => {
				List<string> curDirSegs = (await client.GetWorkingDirectoryAsync()).Split('/').ToList();
				if (curDirSegs.Count() >= 0) {
					if (curDirSegs.Count() > 0) {
						await client.SetWorkingDirectoryAsync("/" + string.Join("/", curDirSegs.Take(curDirSegs.Count - 1)));
					} else {
						await client.SetWorkingDirectoryAsync("/");
					}
					remoteLb.Items = (await lib.ListDirAsync(client)).Select(zzz => new { zzz.Type, zzz.Name, zzz.Size, zzz.Created, zzz.Modified, zzz.FullName });
				}
			};

			upDirLocal.Click += (z, zz) => {
				var curDirSegs = new Uri(currentLocalDir);
				if (curDirSegs.Segments.Count() > 1) {
					string newLocDir = Directory.GetParent(curDirSegs.LocalPath).FullName;
					string[] newDirFilesnames = Directory.GetFileSystemEntries(newLocDir);
					localLb.Items = newDirFilesnames.Select(zzz => new FileInfo(zzz)).Select(zzz => new { zzz.Attributes, zzz.Name, Size = !zzz.Attributes.HasFlag(FileAttributes.Directory) ? zzz.Length : 0, zzz.CreationTime, zzz.LastWriteTime, zzz.FullName });
					currentLocalDir = newLocDir;
				}
			};

			remoteLb.CellPointerPressed += async (z, zz) => {
				var selectedItem = remoteLb.Items.Cast<dynamic>().ElementAt(zz.Row.GetIndex());
				if (selectedItem.Type.Equals(FtpFileSystemObjectType.Directory)) {
					await client.SetWorkingDirectoryAsync(selectedItem.FullName);
					remoteLb.Items = (await lib.ListDirAsync(client)).Select(zzz => new { zzz.Type, zzz.Name, zzz.Size, zzz.Created, zzz.Modified, zzz.FullName });
				}
			};

			localLb.CellPointerPressed += async (z, zz) => {
				var selectedItem = localLb.Items.Cast<dynamic>().ElementAt(zz.Row.GetIndex());
				if (selectedItem.Attributes.HasFlag(FileAttributes.Directory)) {
					string[] newDirFilesnames = Directory.GetFileSystemEntries(selectedItem.FullName);

					await client.GetWorkingDirectoryAsync();

					localLb.Items = newDirFilesnames.Select(zzz => new FileInfo(zzz)).Select(zzz => new { zzz.Attributes, zzz.Name, Size = !zzz.Attributes.HasFlag(FileAttributes.Directory) ? zzz.Length : 0, zzz.CreationTime, zzz.LastWriteTime, zzz.FullName });
					currentLocalDir = selectedItem.FullName;
				}
			};

			ph3.Width = 20;

			var partcPanel3 = App.HorizontalStackPanel;
			partcPanel3.HorizontalAlignment = HorizontalAlignment.Center;
			partcPanel3.Children.AddRange(new List<IControl> { localLb, ph3, remoteLb });
			var border2 = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightSlateGray, Width = 900 };
			border2.Child = partcPanel3;

			rootPan.Children.AddRange(new List<IControl> { blurb1, partcPanel, border1, border2 });

			dlBt.Click += async (z, zz) => {
				try {
					foreach (var item in remoteLb.SelectedItems) {
						((App)Application.Current).LoadingBar.IsIndeterminate = true;
						await lib.DownloadFileAsync(client, new Uri(currentLocalDir), ((dynamic)item).Name);
						((App)Application.Current).LoadingBar.IsIndeterminate = false;
						localLb.Items = Directory.GetFileSystemEntries(Directory.GetCurrentDirectory()).Select(zzz => new FileInfo(zzz)).Select(zzz => new { zzz.Attributes, zzz.Name, Size = !zzz.Attributes.HasFlag(FileAttributes.Directory) ? zzz.Length : 0, zzz.CreationTime, zzz.LastWriteTime, zzz.FullName }).ToList();
					}
				} catch (Exception e) {
					((App)Application.Current).LoadingBar.IsIndeterminate = false;
					ShowErrorMessage(e.Message);
				}
			};

			ulBt.Click += async (z, zz) => {
				try {
					foreach (var item in localLb.SelectedItems) {
						((App)Application.Current).LoadingBar.IsIndeterminate = true;
						await lib.UploadFileAsync(client, new Uri(((dynamic)item).FullName));
						((App)Application.Current).LoadingBar.IsIndeterminate = false;
						remoteLb.Items = (await lib.ListDirAsync(client)).Select(zzz => new { zzz.Type, zzz.Name, zzz.Size, zzz.Created, zzz.Modified, zzz.FullName });
					}
				} catch (Exception e) {
					((App)Application.Current).LoadingBar.IsIndeterminate = false;
					ShowErrorMessage(e.Message);
				}
			};


			connectBt.Click += async (z, zz) => {
				try {
					if (string.IsNullOrWhiteSpace(hostField.Text)) {
						throw new ArgumentNullException();
					}
					((App)Application.Current).LoadingBar.IsIndeterminate = true;

					client = await lib.ConnectToFtpAsync(hostField.Text, int.Parse(portField.Text), userField.Text, passField[TextBox.TextProperty].ToString());
					remoteLb.Items = (await lib.ListDirAsync(client)).Select(zzz => new { zzz.Type, zzz.Name, zzz.Size, zzz.Created, zzz.Modified, zzz.FullName });
					localLb.Items = Directory.GetFileSystemEntries(Directory.GetCurrentDirectory()).Select(zzz => new FileInfo(zzz)).Select(zzz => new { zzz.Attributes, zzz.Name, Size = !zzz.Attributes.HasFlag(FileAttributes.Directory) ? zzz.Length : 0, zzz.CreationTime, zzz.LastWriteTime, zzz.FullName }).ToList();

					currentLocalDir = Directory.GetCurrentDirectory();

					connectBt.IsEnabled = false;
					disconnectBt.IsEnabled = true;
					hostField.IsEnabled = false;
					portField.IsEnabled = false;
					userField.IsEnabled = false;
					passField.IsEnabled = false;
					upDirLocal.IsEnabled = true;
					ulBt.IsEnabled = true;
					upDirRemote.IsEnabled = true;
					dlBt.IsEnabled = true;

					((App)Application.Current).LoadingBar.IsIndeterminate = false;

				} catch (Exception e) {
					((App)Application.Current).LoadingBar.IsIndeterminate = false;

					if (e is ArgumentNullException || e is ArgumentException || e is FormatException) {
						ShowErrorMessage("Empty or invalid connection parameter(s).");
					} else {
						ShowErrorMessage(e.Message);
					}
				}
			};

			disconnectBt.Click += (z, zz) => {
				client.Disconnect();
				client.Dispose();

				connectBt.IsEnabled = true;
				disconnectBt.IsEnabled = false;
				hostField.IsEnabled = true;
				portField.IsEnabled = true;
				userField.IsEnabled = true;
				passField.IsEnabled = true;
				upDirLocal.IsEnabled = false;
				ulBt.IsEnabled = false;
				upDirRemote.IsEnabled = false;
				dlBt.IsEnabled = false;
				localLb.Items = null;
				remoteLb.Items = null;
			};
		}
	}
}