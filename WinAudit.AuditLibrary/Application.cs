﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;

namespace WinAudit.AuditLibrary
{
    public abstract class Application : IDisposable
    {
        #region Public abstract properties

        public abstract string ApplicationId { get; }

        public abstract string ApplicationLabel { get; }

        public abstract OSSIndexHttpClient HttpClient { get; }

        public abstract Dictionary<string, FileSystemInfo> ApplicationFileSystemMap { get; }

        public abstract List<string> RequiredFileLocations { get; }

        public abstract List<string> RequiredDirectoryLocations { get; }
        
        #endregion

        #region Public abstract methods
        public abstract Dictionary<string, PackageSource> GetModules();
        #endregion

        #region Public properties
      
        public Dictionary<string, PackageSource> Modules { get; set; } 

        public Task<Dictionary<string, IEnumerable<OSSIndexQueryObject>>> ModulesTask;

        public Dictionary<string, object> ApplicationOptions { get; set; } = new Dictionary<string, object>();

        #endregion

        #region Constructors

        public Application() { }

        public Application(Dictionary<string, object> application_options)
        {
            if (ReferenceEquals(application_options, null)) throw new ArgumentNullException("application_options");
            this.ApplicationOptions = application_options;
            foreach (string f in RequiredFileLocations.Where(s => s.EndsWith("File")).Except(this.ApplicationFileSystemMap.Keys))
            {
                if (!this.ApplicationOptions.ContainsKey(f))
                {
                    throw new ArgumentException(string.Format("The required application file {0} was not specified.", f), "application_options");
                }
                else if (!File.Exists((string) ApplicationOptions[f]))
                {
                    throw new ArgumentException(string.Format("The required application file {0} was not found.", f), "application_options");
                }
                else
                {
                    this.ApplicationFileSystemMap.Add(f, new FileInfo((string) this.ApplicationOptions[f]));
                }


            }

            foreach (string d in RequiredDirectoryLocations.Where(s => s.EndsWith("Directory")).Except(this.ApplicationFileSystemMap.Keys))
            {
                if (!this.ApplicationOptions.ContainsKey(d))
                {
                    throw new ArgumentException(string.Format("The required application directory {0} was not specified.", d), "application_options");
                }
                else if (!Directory.Exists((string) ApplicationOptions[d]))
                {
                    throw new ArgumentException(string.Format("The required application directory {0} was not found.", d), "application_options");
                }
                else
                {
                    this.ApplicationFileSystemMap.Add(d, new DirectoryInfo((string)this.ApplicationOptions[d]));
                }

            }

        }
        #endregion

        #region Static methods
        public static string CombinePaths(params string[] paths)
        {
            return Path.Combine(paths);
        }
        #endregion

        #region Private properties

        #endregion

        #region Private fields

        public Task<Dictionary<string, IEnumerable<OSSIndexQueryObject>>> _ModulesTask;

        #endregion

        #region Private methods
        #endregion


        #region Disposer
        private bool IsDisposed { get; set; }
        /// <summary> 
        /// /// Implementation of Dispose according to .NET Framework Design Guidelines. 
        /// /// </summary> 
        /// /// <remarks>Do not make this method virtual. 
        /// /// A derived class should not be able to override this method. 
        /// /// </remarks>         
        public void Dispose()
        {
            Dispose(true); // This object will be cleaned up by the Dispose method. // Therefore, you should call GC.SupressFinalize to // take this object off the finalization queue // and prevent finalization code for this object // from executing a second time. // Always use SuppressFinalize() in case a subclass // of this type implements a finalizer. GC.SuppressFinalize(this); }
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            // TODO If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource. 
            try
            {
                if (!this.IsDisposed)
                {
                    // Explicitly set root references to null to expressly tell the GarbageCollector 
                    // that the resources have been disposed of and its ok to release the memory 
                    // allocated for them. 
                    if (isDisposing)
                    {

                        if (ModulesTask.Status == TaskStatus.RanToCompletion ||
                            ModulesTask.Status == TaskStatus.Faulted || ModulesTask.Status == TaskStatus.Canceled)
                        {

                            ModulesTask.Dispose();
                            ModulesTask = null;
                        }
                    }
                }
            }
            finally
            {
                this.IsDisposed = true;
            }
        }
        #endregion

    }
}
