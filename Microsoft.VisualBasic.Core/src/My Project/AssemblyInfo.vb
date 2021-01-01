Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

#If netcore5 = 0 Then

<Assembly: AssemblyTitle("sciBASIC.NET Framework: Core runtime for Microsoft VisualBasic 2020")>
<Assembly: AssemblyDescription("sciBASIC.NET Framework: Core runtime for Microsoft VisualBasic 2020")>
<Assembly: AssemblyCompany("sciBASIC.NET")>
<Assembly: AssemblyProduct("Microsoft.VisualBasic")>
<Assembly: AssemblyCopyright("Copyright © <xie.guigang@live.com>, GPL3 Licensed 2020")>
<Assembly: AssemblyTrademark("Microsoft VisualBasic")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("390cf8ae-fe48-414a-be09-92bdb8c4011b")> 

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

<Assembly: AssemblyVersion("4.7.*")> 
<Assembly: AssemblyFileVersion("2.13.*")> 

#End If