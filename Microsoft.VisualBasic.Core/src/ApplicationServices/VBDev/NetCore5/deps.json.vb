#Region "Microsoft.VisualBasic::ccdc43f79ee79a54cd9ba4090539f646, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\NetCore5\deps.json.vb"

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

    '   Total Lines: 150
    '    Code Lines: 95
    ' Comment Lines: 37
    '   Blank Lines: 18
    '     File Size: 6.50 KB


    '     Class deps
    ' 
    '         Properties: compilationOptions, libraries, runtimeTarget, targets
    ' 
    '         Function: GetReferenceProject, LoadAssemblyOrCache, LoadDependencies, RetriveLoadedAssembly
    ' 
    '         Sub: (+2 Overloads) TryHandleNetCore5AssemblyBugs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices.Development.NetCore5

    ''' <summary>
    ''' read deps.json for .net 5 assembly
    ''' </summary>
    Public Class deps

        ''' <summary>
        ''' ".NETCoreApp,Version=v5.0"
        ''' </summary>
        ''' <returns></returns>
        Public Property runtimeTarget As frameworkTarget
        Public Property compilationOptions As compilationOptions
        Public Property targets As Dictionary(Of String, Dictionary(Of String, target))
        Public Property libraries As Dictionary(Of String, library)

        Public Iterator Function LoadDependencies(package As Assembly) As IEnumerable(Of NamedValue(Of runtime))
            Dim info As AssemblyInfo = package.FromAssembly
            Dim assemblyKey As String = $"{info.Name}/{info.AssemblyInformationalVersion}"
            Dim targets As Dictionary(Of String, target) = Me.targets(runtimeTarget.name)
            Dim packageTarget As target = targets(assemblyKey)
            Dim dependencies = packageTarget.dependencies
            Dim dllFile As String

            For Each project In dependencies
                assemblyKey = $"{project.Key}/{project.Value}"
                packageTarget = targets(assemblyKey)
                dllFile = packageTarget.LibraryFile

                If Not dllFile.StringEmpty Then
                    Yield New NamedValue(Of runtime) With {
                        .Name = assemblyKey,
                        .Value = packageTarget.runtime(dllFile),
                        .Description = dllFile
                    }
                End If
            Next
        End Function

        ''' <summary>
        ''' get list of project reference name
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetReferenceProject() As IEnumerable(Of NamedValue(Of String))
            Return From entry As KeyValuePair(Of String, library)
                   In libraries
                   Let ref As library = entry.Value
                   Where ref.type = "project"
                   Select entry.Key.GetTagValue("/")
        End Function

        ''' <summary>
        ''' returns a list of file path loaded assembly files
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function RetriveLoadedAssembly() As IEnumerable(Of String)
            Return From assembly As Assembly
                   In AppDomain.CurrentDomain.GetAssemblies()
                   Where Not assembly.IsDynamic
                   Select assembly.Location
        End Function

        ''' <summary>
        ''' load assembly from a given file path
        ''' </summary>
        ''' <param name="dllFile">full path</param>
        ''' <returns></returns>
        Public Shared Function LoadAssemblyOrCache(dllFile As String, Optional strict As Boolean = True) As Assembly
            Dim dllFullName As String = dllFile.FileName
            Dim result As New Value(Of Assembly)

            ' R# identical package dll file in different file location
            ' so just test with dll file name
            Dim queryLoaded = From assembly As Assembly
                              In AppDomain.CurrentDomain.GetAssemblies()
                              Where Not assembly.IsDynamic
                              Where assembly.Location.FileName = dllFullName
                              Select assembly

            If (result = queryLoaded.FirstOrDefault) Is Nothing Then
                Try
                    ' not loaded yet
                    Return Assembly.LoadFrom(dllFile.GetFullPath)
                Catch ex As Exception
                    ex = New InvalidProgramException($"error while loading dll file: " & dllFile, ex)

                    If strict Then
                        Throw ex
                    Else
                        Return App.LogException(ex)
                    End If
                End Try
            Else
                Return result
            End If
        End Function

        ''' <summary>
        ''' handling of the bugs on .NET 5 runtime
        ''' missing assembly when load reference type module
        ''' </summary>
        ''' <param name="package"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub TryHandleNetCore5AssemblyBugs(package As Type)
            Call TryHandleNetCore5AssemblyBugs(package.Assembly)
        End Sub

        ''' <summary>
        ''' handling of the bugs on .NET 5 runtime
        ''' missing assembly when load reference type module
        ''' </summary>
        ''' <param name="package"></param>
        Public Shared Sub TryHandleNetCore5AssemblyBugs(package As Assembly)
            Dim home As String = package.Location.ParentPath
            Dim moduleName As String = package.GetName.Name
            Dim depsJson As String = $"{home}/{moduleName}.deps.json"
            Dim deps As deps = If(depsJson.FileExists, depsJson.LoadJsonFile(Of deps), Nothing)

            If deps Is Nothing Then
                Call $"{depsJson} is missing or format incorrect!".Warning
                Return
            End If

            Dim referenceList As NamedValue(Of String)() = deps _
                .GetReferenceProject _
                .Where(Function(pkg) pkg.Name <> moduleName) _
                .ToArray
            Dim dependencies = deps.LoadDependencies(package).ToDictionary(Function(d) d.Name)

            For Each project As NamedValue(Of String) In referenceList
                Dim dllFileName As NamedValue(Of runtime) = dependencies.TryGetValue(project.Description)

                If dllFileName.Description.StringEmpty Then
                    Call $"no dll file module of: {project.Description}?".Warning
                Else
                    ' 由于.net5环境下没有办法将dll自动生成在library文件夹之中
                    ' 所以在这里就直接在应用程序文件夹之中查找了
                    Dim dllName As String = $"{home}/{dllFileName.Description}"

                    If dllName.FileExists Then
                        Call LoadAssemblyOrCache(dllName)
                    Else
                        Call $"missing assembly file: {dllName}...".Warning
                    End If
                End If
            Next
        End Sub
    End Class
End Namespace
