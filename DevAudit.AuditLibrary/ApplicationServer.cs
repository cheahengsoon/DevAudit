﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevAudit.AuditLibrary
{
    public abstract class ApplicationServer : Application
    {
        #region Public abstract properties
        public abstract string ServerId { get; }

        public abstract string ServerLabel { get; }

        public abstract Dictionary<string, string> OptionalFileLocations { get; }

        public abstract Dictionary<string, string> OptionalDirectoryLocations { get; }

        public abstract string DefaultConfigurationFile { get; }
        #endregion

        #region Public abstract methods
        public abstract string GetVersion();
        public abstract Dictionary<string, object> GetConfiguration();
        #endregion

        #region Public properties
        public Dictionary<string, FileSystemInfo> ServerFileSystemMap { get; } = new Dictionary<string, FileSystemInfo>();

        public FileInfo ConfigurationFile
        {
            get
            {
                return (FileInfo)this.ServerFileSystemMap["ConfigurationFile"];
            }
        }

        public new DirectoryInfo RootDirectory
        {
            get
            {
                return (DirectoryInfo)this.ServerFileSystemMap["RootDirectory"];
            }
        }

        public string Version { get; set; }

        public Dictionary<string, object> Configuration { get; set; }

        public Task<Dictionary<string, object>> GetConfigurationTask { get; }

        public Dictionary<string, object> ServerOptions { get; set; } = new Dictionary<string, object>();
        #endregion

        #region Constructors
        public ApplicationServer() { } 

        public ApplicationServer(Dictionary<string, object> server_options) : base(server_options)
        {
            if (ReferenceEquals(server_options, null)) throw new ArgumentNullException("server_options");
            this.ServerOptions = server_options;

            if (!this.ServerOptions.ContainsKey("RootDirectory"))
            {
                throw new ArgumentException(string.Format("The root server directory was not specified."), "server_options");
            }
            else if (!Directory.Exists((string)this.ServerOptions["RootDirectory"]))
            {
                throw new ArgumentException(string.Format("The root server directory {0} was not found.", this.ServerOptions["RootDirectory"]), "server_options");
            }
            else
            {
                this.ServerFileSystemMap.Add("RootDirectory", new DirectoryInfo((string)this.ServerOptions["RootDirectory"]));
            }

            if (!this.ServerOptions.ContainsKey("ConfigurationFile") && string.IsNullOrEmpty(this.DefaultConfigurationFile))
            {
                throw new ArgumentException(string.Format("The server configuration file was not specified and no default configuration file can be used."), "server_options");
            }
            else if (!this.ServerOptions.ContainsKey("ConfigurationFile") && !string.IsNullOrEmpty(this.DefaultConfigurationFile))
            {
                string cf;
                if (this.DefaultConfigurationFile.First() != Path.DirectorySeparatorChar)
                {
                    cf = Path.Combine(this.RootDirectory.FullName, this.DefaultConfigurationFile);
                }
                else
                {
                    cf = this.DefaultConfigurationFile;
                }
                if (File.Exists(cf))
                {
                    this.ServerFileSystemMap.Add("ConfigurationFile", new FileInfo(cf));
                }
                else
                {
                    throw new ArgumentException(string.Format("The server configuration file was not specified and the default configuration file {0} could not be found.", cf), "server_options");
                }
            }
            else 
            {
                string cf = (string) this.ServerOptions["ConfigurationFile"];
                if (cf.StartsWith("@"))
                {
                    cf = Path.Combine(this.RootDirectory.FullName, cf.Substring(1));
                }
                if (File.Exists(cf))
                {
                    this.ServerFileSystemMap.Add("ConfigurationFile", new FileInfo(cf));
                }
                else
                {
                    throw new ArgumentException(string.Format("The server configuration file {0} was not found.", cf), "server_options");
                }           
            }

            foreach (string f in RequiredFileLocations.Keys)
            {
                if (!this.ServerOptions.ContainsKey(f))
                {
                    if (string.IsNullOrEmpty(RequiredFileLocations[f]))
                    {
                        throw new ArgumentException(string.Format("The required server file {0} was not specified and no default path exists.", f), "server_options");
                    }
                    else
                    {
                        if (this.RootDirectory.GetFiles(RequiredFileLocations[f]).FirstOrDefault() == null)
                        {
                            throw new ArgumentException(string.Format("The default path {0} for required server file {1} does not exist.",
                                RequiredFileLocations[f], f), "RequiredFileLocations");
                        }
                        else
                        {
                            this.ServerFileSystemMap.Add(f, this.RootDirectory.GetFiles(RequiredFileLocations[f]).First());
                        }
                    }

                }
                else if (this.RootDirectory.GetFiles((string)ServerOptions[f]).FirstOrDefault() == null)
                {
                    throw new ArgumentException(string.Format("The required server file {0} was not found.", f), "server_options");
                }
                else
                {
                    this.ServerFileSystemMap.Add(f, this.RootDirectory.GetFiles((string)ServerOptions[f]).First());
                }
            }

            foreach (string d in RequiredDirectoryLocations.Keys)
            {
                if (!this.ServerOptions.ContainsKey(d))
                {
                    if (string.IsNullOrEmpty(RequiredDirectoryLocations[d]))
                    {
                        throw new ArgumentException(string.Format("The required server directory {0} was not specified and no default path exists.", d), "server_options");
                    }
                    else
                    {
                        if (this.RootDirectory.GetDirectories(RequiredDirectoryLocations[d]).FirstOrDefault() == null)
                        {
                            throw new ArgumentException(string.Format("The default path {0} for required server directory {1} does not exist.",
                                RequiredDirectoryLocations[d], d), "RequiredDirectoryLocations");
                        }
                        else
                        {
                            this.ServerFileSystemMap.Add(d, this.RootDirectory.GetDirectories(RequiredDirectoryLocations[d]).First());
                        }
                    }

                }
                else if (this.RootDirectory.GetDirectories((string)ServerOptions[d]).FirstOrDefault() == null)
                {
                    throw new ArgumentException(string.Format("The required server directory {0} was not found.", d), "server_options");
                }
                else
                {
                    this.ServerFileSystemMap.Add(d, this.RootDirectory.GetDirectories((string)ServerOptions[d]).First());
                }
            }
        }
        #endregion
    }
}