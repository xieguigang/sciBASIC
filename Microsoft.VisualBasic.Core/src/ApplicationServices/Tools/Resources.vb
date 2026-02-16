#Region "Microsoft.VisualBasic::1a12449b5df017d4005bb825f417e6be, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\Resources.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 350
'    Code Lines: 129 (36.86%)
' Comment Lines: 189 (54.00%)
'    - Xml Docs: 41.80%
' 
'   Blank Lines: 32 (9.14%)
'     File Size: 17.02 KB


'     Class ResourcesSatellite
' 
'         Properties: FileName, MyResource, Resources
' 
'         Constructor: (+5 Overloads) Sub New
' 
'         Function: DirectLoadFrom, findResourceAssemblyFileName, (+2 Overloads) GetObject, (+2 Overloads) GetStream, (+3 Overloads) GetString
'                   LoadMy
' 
'         Sub: doLoad, resourceAssemblyParser
' 
' 
' /********************************************************************************/

#End Region

Imports System.Composition
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Resources
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices

    ''' <summary>
    ''' Represents a resource manager that provides convenient access to culture-specific
    ''' resources at run time.Security Note: Calling methods in this class with untrusted
    ''' data is a security risk. Call the methods in the class only with trusted data.
    ''' For more information, see Untrusted Data Security Risks.
    ''' (资源卫星程序集)
    ''' </summary>
    <Export(GetType(ResourceManager))> Public Class ResourcesSatellite

        ''' <summary>
        ''' The file path of the resources satellite assembly.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FileName As String
        ''' <summary>
        ''' <see cref="System.Resources.ResourceManager"/> object in the satellite assembly.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Resources As ResourceManager

        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     The name parameter is null.
        '
        '   T:System.Resources.MissingManifestResourceException:
        '     No usable set of localized resources has been found, and there are no default
        '     culture resources. For information about how to handle this exception, see the
        '     "Handling MissingManifestResourceException and MissingSatelliteAssemblyException
        '     Exceptions" section in the System.Resources.ResourceManager class topic.
        '
        '   T:System.Resources.MissingSatelliteAssemblyException:
        '     The default culture's resources reside in a satellite assembly that could not
        '     be found. For information about how to handle this exception, see the "Handling
        '     MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions"
        '     section in the System.Resources.ResourceManager class topic.

        ''' <summary>
        ''' Returns the value of the specified non-string resource.
        ''' </summary>
        ''' <param name="name">The name of the resource to get.</param>
        ''' <returns>The value of the resource localized for the caller's current culture settings.
        ''' If an appropriate resource set exists but name cannot be found, the method returns
        ''' null.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function GetObject(name As String) As Object
            Return Resources.GetObject(name)
        End Function

        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     The name parameter is null.
        '
        '   T:System.Resources.MissingManifestResourceException:
        '     No usable set of resources have been found, and there are no default culture
        '     resources. For information about how to handle this exception, see the "Handling
        '     MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions"
        '     section in the System.Resources.ResourceManager class topic.
        '
        '   T:System.Resources.MissingSatelliteAssemblyException:
        '     The default culture's resources reside in a satellite assembly that could not
        '     be found. For information about how to handle this exception, see the "Handling
        '     MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions"
        '     section in the System.Resources.ResourceManager class topic.

        ''' <summary>
        ''' Gets the value of the specified non-string resource localized for the specified
        ''' culture.
        ''' </summary>
        ''' <param name="name">The name of the resource to get.</param>
        ''' <param name="culture">The culture for which the resource is localized. If the resource is not localized
        ''' for this culture, the resource manager uses fallback rules to locate an appropriate
        ''' resource.If this value is null, the System.Globalization.CultureInfo object is
        ''' obtained by using the System.Globalization.CultureInfo.CurrentUICulture property.</param>
        ''' <returns>The value of the resource, localized for the specified culture. If an appropriate
        ''' resource set exists but name cannot be found, the method returns null.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function GetObject(name As String, culture As CultureInfo) As Object
            Return Resources.GetObject(name, culture)
        End Function

        ' Exceptions:
        '   T:System.InvalidOperationException:
        '     The value of the specified resource is not a System.IO.MemoryStream object.
        '
        '   T:System.ArgumentNullException:
        '     name is null.
        '
        '   T:System.Resources.MissingManifestResourceException:
        '     No usable set of resources is found, and there are no default resources. For
        '     information about how to handle this exception, see the "Handling MissingManifestResourceException
        '     and MissingSatelliteAssemblyException Exceptions" section in the System.Resources.ResourceManager
        '     class topic.
        '
        '   T:System.Resources.MissingSatelliteAssemblyException:
        '     The default culture's resources reside in a satellite assembly that could not
        '     be found. For information about how to handle this exception, see the "Handling
        '     MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions"
        '     section in the System.Resources.ResourceManager class topic.

        ''' <summary>
        ''' Returns an unmanaged memory stream object from the specified resource.
        ''' </summary>
        ''' <param name="name">The name of a resource.</param>
        ''' <returns>An unmanaged memory stream object that represents a resource .</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ComVisible(False)>
        Public Function GetStream(name As String) As UnmanagedMemoryStream
            Return Resources.GetStream(name)
        End Function

        ' Exceptions:
        '   T:System.InvalidOperationException:
        '     The value of the specified resource is not a System.IO.MemoryStream object.
        '
        '   T:System.ArgumentNullException:
        '     name is null.
        '
        '   T:System.Resources.MissingManifestResourceException:
        '     No usable set of resources is found, and there are no default resources. For
        '     information about how to handle this exception, see the "Handling MissingManifestResourceException
        '     and MissingSatelliteAssemblyException Exceptions" section in the System.Resources.ResourceManager
        '     class topic.
        '
        '   T:System.Resources.MissingSatelliteAssemblyException:
        '     The default culture's resources reside in a satellite assembly that could not
        '     be found. For information about how to handle this exception, see the "Handling
        '     MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions"
        '     section in the System.Resources.ResourceManager class topic.

        ''' <summary>
        ''' Returns an unmanaged memory stream object from the specified resource, using
        ''' the specified culture.
        ''' </summary>
        ''' <param name="name">The name of a resource.</param>
        ''' <param name="culture">An object that specifies the culture to use for the resource lookup. If culture
        ''' is null, the culture for the current thread is used.</param>
        ''' <returns>An unmanaged memory stream object that represents a resource.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ComVisible(False)>
        Public Function GetStream(name As String, culture As CultureInfo) As UnmanagedMemoryStream
            Return Resources.GetStream(name, culture)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetString(name As String, encoding As Encodings) As String
            Dim stream As New StreamReader(GetStream(name), encoding.CodePage)
            Return stream.ReadToEnd
        End Function

        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     The name parameter is null.
        '
        '   T:System.InvalidOperationException:
        '     The value of the specified resource is not a string.
        '
        '   T:System.Resources.MissingManifestResourceException:
        '     No usable set of resources has been found, and there are no resources for the
        '     default culture. For information about how to handle this exception, see the
        '     "Handling MissingManifestResourceException and MissingSatelliteAssemblyException
        '     Exceptions" section in the System.Resources.ResourceManager class topic.
        '
        '   T:System.Resources.MissingSatelliteAssemblyException:
        '     The default culture's resources reside in a satellite assembly that could not
        '     be found. For information about how to handle this exception, see the "Handling
        '     MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions"
        '     section in the System.Resources.ResourceManager class topic.

        ''' <summary>
        ''' Returns the value of the specified string resource.
        ''' </summary>
        ''' <param name="name">The name of the resource to retrieve.</param>
        ''' <returns>The value of the resource localized for the caller's current UI culture, or null
        ''' if name cannot be found in a resource set.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function GetString(name As String) As String
            Return Resources.GetString(name)
        End Function

        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     The name parameter is null.
        '
        '   T:System.InvalidOperationException:
        '     The value of the specified resource is not a string.
        '
        '   T:System.Resources.MissingManifestResourceException:
        '     No usable set of resources has been found, and there are no resources for a default
        '     culture. For information about how to handle this exception, see the "Handling
        '     MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions"
        '     section in the System.Resources.ResourceManager class topic.
        '
        '   T:System.Resources.MissingSatelliteAssemblyException:
        '     The default culture's resources reside in a satellite assembly that could not
        '     be found. For information about how to handle this exception, see the "Handling
        '     MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions"
        '     section in the System.Resources.ResourceManager class topic.

        ''' <summary>
        ''' Returns the value of the string resource localized for the specified culture.
        ''' </summary>
        ''' <param name="name">The name of the resource to retrieve.</param>
        ''' <param name="culture">An object that represents the culture for which the resource is localized.</param>
        ''' <returns>The value of the resource localized for the specified culture, or null if name
        ''' cannot be found in a resource set.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function GetString(name As String, culture As CultureInfo) As String
            Return Resources.GetString(name, culture)
        End Function

        Sub New()
            Call Me.New(Assembly.GetExecutingAssembly)
        End Sub

        ''' <summary>
        ''' Load external resource data from current module's satellite assembly.
        ''' </summary>
        ''' <param name="type"></param>
        Sub New(type As Type)
            Call Me.New(type.Assembly)
        End Sub

        ''' <summary>
        ''' 默认是<see cref="App.HOME"/>/Resources/assmFile
        ''' </summary>
        ''' <param name="assm"></param>
        Sub New(assm As Assembly)
            Call Me.New(findResourceAssemblyFileName(assm))
        End Sub

        Sub New(dll As String)
            Call resourceAssemblyParser(fileName:=FileIO.FileSystem.GetFileInfo(dll).FullName)
        End Sub

        Private Shared Function findResourceAssemblyFileName(assm As Assembly) As String
            Dim resourceName As String = "Resources/" & FileIO.FileSystem.GetFileInfo(assm.Location).Name
            Dim dllFile As String = $"{assm.Location.ParentPath}/{resourceName}"

            If Not dllFile.FileExists Then
                dllFile = $"{App.CurrentDirectory}/{resourceName}"
            End If
            If Not dllFile.FileExists Then
                dllFile = $"{App.HOME}/{resourceName}"
            End If
            If Not dllFile.FileExists Then
                Throw New EntryPointNotFoundException("missing assembly resource module: " & dllFile)
            Else
                Return FileIO.FileSystem.GetFileInfo(dllFile).FullName
            End If
        End Function

        Private Sub resourceAssemblyParser(fileName As String)
            If fileName.FileExists Then
                Call Assembly.LoadFile(fileName).DoCall(AddressOf doLoad)
            Else
                Throw New DllNotFoundException($"Missing required resources satellite assembly file: {fileName.FileName}!")
            End If

            _FileName = fileName
        End Sub

        Private Sub doLoad(assm As Assembly)
#If NET_40 = 0 Then
            Dim resource As Type = GetType(ExportAttribute)
            Dim resourceMgr As Type = LinqAPI.DefaultFirst(Of Type) _
 _
                () <= From type As Type
                      In assm.GetTypes
                      Let exp As ExportAttribute = type.GetCustomAttribute(resource)
                      Where Not exp Is Nothing AndAlso
                          exp.ContractType.Equals(GetType(ResourceManager))
                      Select type

            If Not resourceMgr Is Nothing Then
                Dim container = LinqAPI.DefaultFirst(Of PropertyInfo) _
 _
                    () <= From prop As PropertyInfo
                          In resourceMgr.GetProperties(BindingFlags.Public Or BindingFlags.Static)
                          Let exp As ExportAttribute = prop.GetCustomAttribute(resource)
                          Where prop.CanRead AndAlso
                              Not exp Is Nothing AndAlso
                              exp.ContractType.Equals(GetType(ResourceManager))
                          Select prop

                If Not container Is Nothing Then
                    Dim mgr = container.GetValue(Nothing, Nothing)
                    _Resources = DirectCast(mgr, ResourceManager)
                End If
            End If
#End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="my">null</param>
        ''' <param name="assm"></param>
        Private Sub New(my As Type, assm As Assembly)
            Call doLoad(assm)
        End Sub

        ''' <summary>
        ''' 从自身的程序集之中加载数据
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function LoadMy() As ResourcesSatellite
            Return New ResourcesSatellite(Nothing, Assembly.GetExecutingAssembly)
        End Function

        Public Shared Function DirectLoadFrom(assm As Assembly) As ResourcesSatellite
            Return New ResourcesSatellite(Nothing, assm)
        End Function

        ''' <summary>
        ''' Returns the cached ResourceManager instance used by this class.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Export(GetType(ResourceManager))>
        Public Shared ReadOnly Property MyResource As ResourceManager
            Get
                Return My.Resources.ResourceManager
            End Get
        End Property
    End Class
End Namespace
