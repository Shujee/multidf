﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MultiDF.Views.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MultiDF.Views.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 10.
        ///An AWS CodePipeline in us-east 1 retums &quot;InternalError with the code &quot;JobFailed&quot; when launching a deployment using an artifact from an Amazon S3 bucket in us-west-1
        ///What is causing this error?
        ///B.	The S3 bucket is not in the appropriate region
        ///C.	The S3 bucket is being throttled.
        ///D.	There are insufficient permissions on the artifact in Amazon S3
        ///A.	S3 Transfer Acceleration is not enabled
        ///https://docs.aws.amazon.com/codepipeline/latest/userguide/troubleshooting.html#troubleshooting-reg-1
        ///.
        /// </summary>
        internal static string Q10 {
            get {
                return ResourceManager.GetString("Q10", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 34.
        ///An AWS CodePipeline in us-east-1 returns &quot;InternalError” with the code &quot;JobFailed&quot; when launching a deployment using an artifact from an Amazon S3 bucket in us-west-1.
        ///What is causing this error?
        ///A.	S3 Transfer Acceleration is not enabled.
        ///B.	The S3 bucket is not in the appropriate region.
        ///C.	The S3 bucket is being throttled.
        ///D.	There are insufficient permissions on the artifact in Amazon S3
        ///https://docs.aws.amazon.com/codepipeline/latest/userguide/troubleshooting.html#troubleshooting-reg-1
        ///.
        /// </summary>
        internal static string Q34 {
            get {
                return ResourceManager.GetString("Q34", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1.
        ///A SysOps Administrator is notified that a security vulnerability affects a version of MySQL that is being used with Amazon RDS MySQL
        ///Who is responsible for ensuring that the patch is applied to the MySQL cluster?
        ///A.	The database vendor
        ///B.	The Security department of the SysOps Administrator&apos;s company
        ///C.	AWS
        ///D.	The SysOps Administrator
        ///https://aws.amazon.com/compliance/shared-responsibility-model/?nc1=h_ls
        ///2.
        ///A SysOps Administrator must automate the tagging of new Amazon EC2 instances deployed in  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SampleText {
            get {
                return ResourceManager.GetString("SampleText", resourceCulture);
            }
        }
    }
}
