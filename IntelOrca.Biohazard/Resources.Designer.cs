﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IntelOrca.Biohazard {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("IntelOrca.Biohazard.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to {
        ///  &quot;outside&quot;: [
        ///    &quot;main00&quot;,
        ///    &quot;main00_2&quot;,
        ///    &quot;main2d&quot;,
        ///    &quot;main2e&quot;,
        ///    &quot;main2f&quot;
        ///  ],
        ///  &quot;creepy&quot;: [
        ///    &quot;main01&quot;,
        ///    &quot;main02&quot;,
        ///    &quot;main03&quot;,
        ///    &quot;main05&quot;,
        ///    &quot;main06&quot;,
        ///    &quot;main07&quot;,
        ///    &quot;main08&quot;,
        ///    &quot;main0e&quot;,
        ///    &quot;main13&quot;,
        ///    &quot;main17&quot;,
        ///    &quot;main18&quot;,
        ///    &quot;main1f&quot;,
        ///    &quot;main22&quot;,
        ///    &quot;main24&quot;,
        ///    &quot;main25&quot;,
        ///    &quot;main27&quot;,
        ///    &quot;main29&quot;,
        ///    &quot;main2b&quot;,
        ///    &quot;main2c&quot;,
        ///    &quot;main30&quot;,
        ///    &quot;main31&quot;,
        ///    &quot;sub01&quot;,
        ///    &quot;sub05&quot;,
        ///    &quot;sub0b&quot;,
        ///    &quot;sub10&quot;,
        ///    &quot;sub11&quot;,
        ///    &quot;sub14&quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string bgm {
            get {
                return ResourceManager.GetString("bgm", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] checksum {
            get {
                object obj = ResourceManager.GetObject("checksum", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;beginEndRooms&quot;: [
        ///    {
        ///      &quot;start&quot;: &quot;100&quot;,
        ///      &quot;end&quot;: &quot;617&quot;,
        ///      &quot;scenario&quot;: 0
        ///    },
        ///    {
        ///      &quot;start&quot;: &quot;104&quot;,
        ///      &quot;end&quot;: &quot;702&quot;,
        ///      &quot;scenario&quot;: 1,
        ///      &quot;doorRando&quot;: false
        ///    },
        ///    {
        ///      &quot;start&quot;: &quot;104&quot;,
        ///      &quot;end&quot;: &quot;700&quot;,
        ///      &quot;scenario&quot;: 1,
        ///      &quot;doorRando&quot;: true
        ///    }
        ///  ],
        ///  &quot;rooms&quot;: {
        ///    &quot;100&quot;: {
        ///      &quot;doors&quot;: [
        ///        {
        ///          &quot;target&quot;: &quot;101&quot;,
        ///          &quot;noReturn&quot;: true
        ///        }
        ///      ],
        ///      &quot;enemies&quot;: [
        ///        {
        ///          &quot;nop&quot;: [ 4888 ]
        ///        }
        ///      ]
        ///    },
        ///    [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string rdt {
            get {
                return ResourceManager.GetString("rdt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap title_bg {
            get {
                object obj = ResourceManager.GetObject("title_bg", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;pl0/voice/stage0/v001.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v002.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v003.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v004.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v005.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v006.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v007.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v008.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v009.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v010.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v011.sap&quot;: &quot;narrator&quot;,
        ///  &quot;pl0/voice/stage0/v012. [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string voice {
            get {
                return ResourceManager.GetString("voice", resourceCulture);
            }
        }
    }
}
