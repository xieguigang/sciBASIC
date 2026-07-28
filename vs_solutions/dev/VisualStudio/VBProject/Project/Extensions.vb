#Region "Microsoft.VisualBasic::498db22fb6f1746095c620d9eaf9c1c3, vs_solutions\dev\VisualStudio\VBProject\Project\Extensions.vb"

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

    '   Total Lines: 139
    '    Code Lines: 102 (73.38%)
    ' Comment Lines: 20 (14.39%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (12.23%)
    '     File Size: 5.74 KB


    '     Module Extensions
    ' 
    '         Function: AssemblyInfo, (+2 Overloads) EnumerateSourceFiles, ExtractNuGetAssemblyInfo, GetOutputDirectory, GetOutputName
    '                   GetProfile, RootNamespace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.ProjectXml
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq

Namespace VBProj

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' Enumerate all of the vb source files in this vbproj.
        ''' </summary>
        ''' <param name="vbproj"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EnumerateSourceFiles(vbproj As String) As IEnumerable(Of String)
            Dim projDir As String = vbproj.ParentPath.GetFullPath
            Dim doc As XDocument = XDocument.Load(vbproj)
            Dim ns As XNamespace = If(doc.Root Is Nothing, "", doc.Root.Name.Namespace)

            Return ProjectFiles.CollectCompileFiles(doc, ns, projDir)
        End Function

        ''' <summary>
        ''' Enumerate all of the vb source files in this vbproj.
        ''' </summary>
        ''' <param name="vbproj"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EnumerateSourceFiles(vbproj As VBProject,
                                             Optional skipAssmInfo As Boolean = False,
                                             Optional fullName As Boolean = False) As IEnumerable(Of String)

            Dim sourceFolder As String = DirectCast(vbproj, IFileReference).FilePath.ParentPath
            Dim sourceList As IEnumerable(Of String) = vbproj.CompileFiles _
                .SafeQuery _
                .Select(Function(c)
                            Return c.FileName
                        End Function)

            If fullName Then
                Return sourceList.Select(Function(rel) $"{sourceFolder}/{rel}".GetFullPath)
            Else
                Return sourceList
            End If
        End Function

        ''' <summary>
        ''' try to extract the assembly information from the vbproject file
        ''' </summary>
        ''' <param name="vbproj"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AssemblyInfo(vbproj As VBProject) As AssemblyInfo
            If vbproj.IsDotNetCoreSDK Then
                Return vbproj.ExtractNuGetAssemblyInfo
            Else
                With DirectCast(vbproj, IFileReference)
                    If Not .FilePath.FileExists Then
                        Return New AssemblyInfo With {
                            .BuiltTime = Now
                        }
                    Else
                        Return GetAssemblyInfo(.FilePath)
                    End If
                End With
            End If
        End Function

        <Extension>
        Public Function ExtractNuGetAssemblyInfo(netcore As VBProject) As AssemblyInfo
            Dim nuget = netcore.NuGet
            Dim main = netcore.Metadata

            Return New AssemblyInfo With {
                .AssemblyCompany = nuget.Company,
                .AssemblyCopyright = nuget.Copyright,
                .AssemblyDescription = nuget.Description,
                .AssemblyFileVersion = netcore.AssemblyVersion,
                .AssemblyVersion = nuget.Version,
                .AssemblyInformationalVersion = nuget.Version,
                .AssemblyFullName = netcore.AssemblyName,
                .AssemblyProduct = nuget.Product,
                .AssemblyTitle = nuget.Description,
                .AssemblyTrademark = nuget.Company,
                .ComVisible = False,
                .Guid = Guid.NewGuid.ToString,
                .Name = nuget.PackageId,
                .TargetFramework = main.TargetFramework,
                .BuiltTime = Nothing
            }
        End Function

        <Extension>
        Public Function GetOutputDirectory(vbproj As VBProject, profileName$) As String
            Dim profile = vbproj.GetProfile(profileName)
            Dim base$ = DirectCast(vbproj, IFileReference).FilePath.ParentPath
            Dim outputdir = $"{base}/{profile.OutputPath}"

            Return outputdir
        End Function

        <Extension>
        Public Function GetProfile(vbproj As VBProject, name As String) As VBBuildConfiguration
            Return vbproj.Configurations _
                .SafeQuery _
                .Where(Function(c)
                           Dim condition As String = c.Condition

                           If InStr(condition, "$(Configuration)") = 0 AndAlso InStr(condition, "$(Platform)") = 0 Then
                               condition = $"'$(Configuration)|$(Platform)' == '{condition}'"
                           Else
                               condition = condition.Trim
                           End If

                           Return Not condition.StringEmpty AndAlso condition.TextEquals(c.Condition.Trim)
                       End Function) _
                .FirstOrDefault
        End Function

        ''' <summary>
        ''' Get output assembly name
        ''' </summary>
        ''' <param name="vbproj"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetOutputName(vbproj As VBProject) As String
            Return vbproj.AssemblyName
        End Function

        <Extension>
        Public Function RootNamespace(vbproj As VBProject) As String
            Return vbproj.RootNamespace
        End Function
    End Module
End Namespace
