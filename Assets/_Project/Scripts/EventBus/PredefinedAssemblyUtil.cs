using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// A utility class, PredefinedAssemblyUtil, provides methods to interact with predefined assemblies.
/// It allows to get all types in the current AppDomain that implement from a specific Interface type.
/// For more details, <see href="https://docs.unity3d.com/2023.3/Documentation/Manual/ScriptCompileOrderFolders.html">visit Unity Documentation</see>
/// </summary>
public static class PredefinedAssemblyUtil {
    /// <summary>
    /// Enum that defines the specific predefined types of assemblies for navigation.
    /// </summary>    
    enum AssemblyType {
        AssemblyCSharp,
        AssemblyCSharpEditor,
        AssemblyCSharpEditorFirstPass,
        AssemblyCSharpFirstPass
    }

    
}