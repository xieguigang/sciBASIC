Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#if netcore5=0 then 
<Assembly: AssemblyTitle("Github API")>
<Assembly: AssemblyDescription("https://developer.github.com/v3/")>
<Assembly: AssemblyCompany("gpl3")>
<Assembly: AssemblyProduct("GithubAPI")>
<Assembly: AssemblyCopyright("Copyright © gpl3 2016")>
<Assembly: AssemblyTrademark("Microsoft VisualBasic")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("57acab62-1194-4633-87f0-67af0ae41ec6")> 

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