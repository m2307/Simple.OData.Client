﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Simple.OData.Client.Tests.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class XmlSamples {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal XmlSamples() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Simple.OData.Client.Tests.Properties.XmlSamples", typeof(XmlSamples).Assembly);
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
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///&lt;statuses&gt;
        ///&lt;status&gt;
        ///&lt;created_at&gt;Tue Apr 07 22:52:51 +0000 2009&lt;/created_at&gt;
        ///&lt;id&gt;1472669360&lt;/id&gt;
        ///&lt;text&gt;Tweet one.&lt;/text&gt;
        ///&lt;source&gt;&lt;a href=&quot;http://www.tweetdeck.com/&quot;&gt;TweetDeck&lt;/a&gt;&lt;/source&gt;
        ///&lt;truncated&gt;false&lt;/truncated&gt;
        ///&lt;in_reply_to_status_id&gt;&lt;/in_reply_to_status_id&gt;
        ///&lt;in_reply_to_user_id&gt;&lt;/in_reply_to_user_id&gt;
        ///&lt;favorited&gt;false&lt;/favorited&gt;
        ///&lt;in_reply_to_screen_name&gt;&lt;/in_reply_to_screen_name&gt;
        ///&lt;user&gt;
        ///&lt;id&gt;1401881&lt;/id&gt;
        ///&lt;name&gt;Doug Williams&lt;/name&gt;
        ///&lt;screen_name&gt;dougw [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string TwitterStatusesSample {
            get {
                return ResourceManager.GetString("TwitterStatusesSample", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;root xmlns=&quot;http://schemas.next.lib/tests&quot;&gt;
        ///  &lt;child&gt;
        ///    &lt;sub&gt;Foo&lt;/sub&gt;
        ///  &lt;/child&gt;  
        ///  &lt;child&gt;
        ///    &lt;sub&gt;Bar&lt;/sub&gt;
        ///  &lt;/child&gt;  
        ///&lt;/root&gt;.
        /// </summary>
        internal static string XmlWithDefaultNamespace {
            get {
                return ResourceManager.GetString("XmlWithDefaultNamespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;root&gt;
        ///  &lt;child&gt;
        ///    &lt;sub&gt;Foo&lt;/sub&gt;
        ///  &lt;/child&gt;  
        ///  &lt;child&gt;
        ///    &lt;sub&gt;Bar&lt;/sub&gt;
        ///  &lt;/child&gt;  
        ///&lt;/root&gt;.
        /// </summary>
        internal static string XmlWithNoNamespace {
            get {
                return ResourceManager.GetString("XmlWithNoNamespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;root xmlns:c=&quot;http://schemas.next.lib/tests&quot;&gt;
        ///  &lt;c:child&gt;
        ///    &lt;c:sub&gt;Foo&lt;/c:sub&gt;
        ///  &lt;/c:child&gt;  
        ///  &lt;c:child&gt;
        ///    &lt;c:sub&gt;Bar&lt;/c:sub&gt;
        ///  &lt;/c:child&gt;  
        ///&lt;/root&gt;.
        /// </summary>
        internal static string XmlWithPrefixedNamespace {
            get {
                return ResourceManager.GetString("XmlWithPrefixedNamespace", resourceCulture);
            }
        }
    }
}
