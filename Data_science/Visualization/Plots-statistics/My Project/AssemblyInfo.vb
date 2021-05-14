Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#if netcore5=0 then 
<Assembly: AssemblyTitle("Chartting plots for statistics analysis")>
<Assembly: AssemblyDescription("Chartting plots for statistics analysis")>
<Assembly: AssemblyCompany("Microsoft")>
<Assembly: AssemblyProduct("Plots.Statistics")>
<Assembly: AssemblyCopyright("Copyright © Microsoft 2017")>
<Assembly: AssemblyTrademark("sciBASIC")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("4263582d-525c-4b83-a9fa-8eb4d9ea62ac")>

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("1.0.0.0")>
<Assembly: AssemblyFileVersion("1.0.0.0")>
#end if