Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#If netcore5 = 0 Then
<Assembly: AssemblyTitle("Excel I/O corelib writen in VisualBasic language")>
<Assembly: AssemblyDescription("Excel I/O corelib writen in VisualBasic language")>
<Assembly: AssemblyCompany("")>
<Assembly: AssemblyProduct("Excel")>
<Assembly: AssemblyCopyright("Copyright © I@xieguigang.me 2017")>
<Assembly: AssemblyTrademark("sciBASIC#")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("888edfb9-1279-4a09-953f-f5eda2525657")>

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