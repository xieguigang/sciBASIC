Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#if netcore5=0 then 
<Assembly: AssemblyTitle("KnowledgeBase module based on Microsoft Bing Search Provider and DOI system")> 
<Assembly: AssemblyDescription("KnowledgeBase module based on Microsoft Bing Search Provider and DOI system")> 
<Assembly: AssemblyCompany("")> 
<Assembly: AssemblyProduct("")> 
<Assembly: AssemblyCopyright("Copyright © sciBASIC 2019")>
<Assembly: AssemblyTrademark("sciBASIC.NET")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("70d0e6a4-4311-4ee6-a646-915b61c7a5a6")> 

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

<Assembly: AssemblyVersion("1.2.0.34")> 
<Assembly: AssemblyFileVersion("1.0.3.0")> 
#end if