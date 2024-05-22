#Region "Microsoft.VisualBasic::6319a648cf5d1657690627c0345c2000, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\NetCoreApp\deps.json.vb"

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

    '   Total Lines: 242
    '    Code Lines: 161 (66.53%)
    ' Comment Lines: 47 (19.42%)
    '    - Xml Docs: 65.96%
    ' 
    '   Blank Lines: 34 (14.05%)
    '     File Size: 10.26 KB


    '     Class deps
    ' 
    '         Properties: compilationOptions, libraries, runtimeTarget, targets
    ' 
    '         Function: GetByFileNameFallback, GetDepsJsonfile, GetDllFileAuto, GetReferenceProject, LoadAssemblyOrCache
    '                   LoadDependencies, RetriveLoadedAssembly
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
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices.Development.NetCoreApp

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
            Dim assemblyKey2 As String = $"{info.Name}/{info.AssemblyVersion}"
            Dim targets As Dictionary(Of String, target) = Me.targets(runtimeTarget.name)
            Dim packageTarget As target = If(
                targets.TryGetValue(assemblyKey),
                targets.TryGetValue(assemblyKey2)
            )

            If packageTarget Is Nothing Then
                packageTarget = GetByFileNameFallback(package, targets)
            End If

            ' The given key 'roxygenNet/1.0.0+88489749f53dd4380c4c99434e7cee2e21e5ae29' was not present in the dictionary.
            ' why missing key?
            If packageTarget Is Nothing Then
                Return
            End If

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

        Private Function GetByFileNameFallback(pkg As Assembly, targets As Dictionary(Of String, target)) As target
            Dim libname As String = pkg.ManifestModule.Name

            For Each libmod As target In targets.Values
                If libmod.LibraryFile = libname Then
                    Return libmod
                End If
            Next

            Return Nothing
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
        ''' <returns>this function maybe returns nothing if the error happends
        ''' andalso parameter value of <paramref name="strict"/> set to FALSE.</returns>
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
        Public Shared Sub TryHandleNetCore5AssemblyBugs(package As Type, external_libloc As String())
            Call TryHandleNetCore5AssemblyBugs(package.Assembly, external_libloc)
        End Sub

        Private Shared Function GetDepsJsonfile(package As Assembly, moduleName As String) As String
            Dim home As String = package.Location.ParentPath
            Dim depsJson As String = $"{home}/{moduleName}.deps.json"

            If depsJson.FileLength > 0 Then
                Return depsJson
            Else
                ' find for external package of R# packages
                Return $"{App.HOME}/{moduleName}.deps.json"
            End If
        End Function

        ''' <summary>
        ''' handling of the bugs on .NET 5 runtime
        ''' missing assembly when load reference type module
        ''' </summary>
        ''' <param name="package"></param>
        Public Shared Sub TryHandleNetCore5AssemblyBugs(package As Assembly, external_libloc As String())
            Dim moduleName As String = package.GetName.Name
            Dim depsJson As String = GetDepsJsonfile(package, moduleName)
            Dim deps As deps = If(depsJson.FileExists, depsJson.LoadJsonFile(Of deps), Nothing)
            Dim libdir As String = package.Location.ParentPath

            If deps Is Nothing Then
                Call $"{depsJson} is missing or format incorrect!".Warning
                Return
            End If

            Dim referenceList As NamedValue(Of String)() = deps _
                .GetReferenceProject _
                .Where(Function(pkg) pkg.Name <> moduleName) _
                .ToArray
            Dim dependencies As Dictionary(Of String, NamedValue(Of runtime)) = deps _
                .LoadDependencies(package) _
                .ToDictionary(Function(d)
                                  Return d.Name
                              End Function)

            For Each project As NamedValue(Of String) In referenceList
                Dim dllFileName As NamedValue(Of runtime) = GetDllFileAuto(dependencies, project)
                Dim dllName As String

                If dllFileName.Description.StringEmpty Then
                    dllName = Strings.Trim(project.Name).Split("/"c).First & ".dll"

                    ' not found in deps.json?
                    Call $"no dll runtime module was found: {project.Description}?".Warning
                Else
                    ' 由于.net5环境下没有办法将dll自动生成在library文件夹之中
                    ' 所以在这里就直接在应用程序文件夹之中查找了
                    dllName = dllFileName.Description
                End If

                Dim dllfile As String
                Dim hit As Boolean = False

                For Each libpath As String In external_libloc.JoinIterates(New String() {App.HOME, libdir})
                    dllfile = $"{libpath}/{dllName}"

                    If dllfile.FileExists Then
                        hit = Not LoadAssemblyOrCache(dllfile, strict:=False) Is Nothing

                        ' 20240403
                        '
                        ' exit current loop for load next 
                        ' project dependency module
                        ' on hit context
                        If hit Then
                            Exit For
                        End If
                    End If
                Next

                If Not hit Then
                    Call $"missing assembly file: {dllName}...".Warning
                End If
            Next
        End Sub

        Private Shared Function GetDllFileAuto(deps As Dictionary(Of String, NamedValue(Of runtime)), proj As NamedValue(Of String))
            If deps.ContainsKey(proj.Description) Then
                Return deps(proj.Description)
            Else
                Dim key1 As String = $"{proj.Name}.Reference/{proj.Value}"
                Dim key2 As String = key1 & ".0"

                If deps.ContainsKey(key1) Then
                    Return deps(key1)
                Else
                    Return deps.TryGetValue(key2)
                End If
            End If
        End Function
    End Class
End Namespace
