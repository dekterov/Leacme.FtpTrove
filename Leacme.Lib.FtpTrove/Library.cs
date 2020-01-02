// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentFTP;

namespace Leacme.Lib.FtpTrove {

	public class Library {

		public Library() {

		}

		/// <summary>
		/// Connects to the FTP server and returns the client.
		/// /// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<FtpClient> ConnectToFtpAsync(string host, int port, string username, string password) {
			FtpClient client = new FtpClient(host, port, username, password);
			await client.ConnectAsync();
			return client;
		}

		/// <summary>
		/// List the current working directory on the FTP server.
		/// /// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public async Task<List<FtpListItem>> ListDirAsync(FtpClient client) {
			List<FtpListItem> items = new List<FtpListItem>();
			items = (await client.GetListingAsync(await client.GetWorkingDirectoryAsync())).ToList();
			return items;
		}

		/// <summary>
		/// Download a file from the FTP server.
		/// /// </summary>
		/// <param name="client"></param>
		/// <param name="localDirectory">Absolute local folder path.</param>
		/// <param name="filename">Name of file to download from the server current working directory.</param>
		/// <returns></returns>
		public async Task<bool> DownloadFileAsync(FtpClient client, Uri localDirectory, string filename) {
			if (!Directory.Exists(localDirectory.LocalPath)) {
				throw new ArgumentException("Not a valid local directory.");
			}
			bool success;
			success = await client.DownloadFileAsync(Path.Combine(localDirectory.LocalPath, filename), Path.Combine(client.GetWorkingDirectory(), filename));
			return success;
		}

		/// <summary>
		/// Upload file to the FTP server.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="filePath">File path to local file to upload.</param>
		/// <returns></returns>
		public async Task<bool> UploadFileAsync(FtpClient client, Uri filePath) {
			if (!File.Exists(filePath.LocalPath)) {
				throw new ArgumentException("Not a valid file.");
			}
			bool success;
			success = await client.UploadFileAsync(filePath.LocalPath, Path.Combine(client.GetWorkingDirectory(), Path.GetFileName(filePath.LocalPath)));
			return success;
		}

		/// <summary>
		/// Disconnect from the FTP server.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public async Task DisconnectFtpAsync(FtpClient client) {
			await client.DisconnectAsync();
			client.Dispose();
		}

		/// <summary>
		/// Change current working directory of the FTP server.
		/// The upload/download operations rely on the current working directory of the server.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="dir">Directory to set the server to. Root directory is "/".</param>
		/// <returns></returns>
		public async Task ChangeFtpDirAsync(FtpClient client, Uri dir) {
			await client.SetWorkingDirectoryAsync(dir.LocalPath);
		}

	}
}