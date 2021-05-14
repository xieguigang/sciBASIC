Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#If netcore5 = 0 Then
<Assembly: AssemblyTitle("Network graph data render engine")>
<Assembly: AssemblyDescription("Network graph data render engine")>
<Assembly: AssemblyCompany("GPL3")>
<Assembly: AssemblyProduct("sciBASIC.NET")>
<Assembly: AssemblyCopyright("Copyright © I@xieguigang.me 2019")>
<Assembly: AssemblyTrademark("sciBASIC.NET")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("ce1eaf18-06c9-4356-93b0-c7a37258d3a5")> 

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

<Assembly: AssemblyVersion("4.1230.*")> 
<Assembly: AssemblyFileVersion("3.110.*")> 
#end if