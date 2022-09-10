#Region "Microsoft.VisualBasic::43fb47880c285560d36b560f5f4ac2a2, sciBASIC#\vs_solutions\dev\VisualStudio\vbproj\Extensions.vb"

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

    '   Total Lines: 147
    '    Code Lines: 115
    ' Comment Lines: 15
    '   Blank Lines: 17
    '     File Size: 6.19 KB


    '     Module Extensions
    ' 
    '         Function: AssemblyInfo, (+2 Overloads) EnumerateSourceFiles, GetOutputDirectory, GetOutputName, legacyProjectSource
    '                   newDotNetSDKProjectSource, RootNamespace, vbfileFilter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.vbproj
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.vbproj.Xml
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq

Namespace vbproj

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' Enumerate all of the vb source files in this vbproj.
        ''' </summary>
        ''' <param name="vbproj"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EnumerateSourceFiles(vbproj As String) As IEnumerable(Of String)
            Return vbproj.LoadXml(Of Project).EnumerateSourceFiles
        End Function

        ''' <summary>
        ''' Enumerate all of the vb source files in this vbproj.
        ''' </summary>
        ''' <param name="vbproj"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EnumerateSourceFiles(vbproj As Project,
                                             Optional skipAssmInfo As Boolean = False,
                                             Optional fullName As Boolean = False) As IEnumerable(Of String)

            Dim sourceFolder As String = DirectCast(vbproj, IFileReference).FilePath.ParentPath
            Dim sourceList As IEnumerable(Of String)

            If vbproj.IsDotNetCoreSDK Then
                sourceList = vbproj.newDotNetSDKProjectSource
            Else
                sourceList = vbproj.legacyProjectSource
            End If

            Return sourceList.vbfileFilter(sourceFolder, fullName, skipAssmInfo)
        End Function

        <Extension>
        Private Function newDotNetSDKProjectSource(vbproj As Project) As IEnumerable(Of String)
            Dim sourceDir As String = DirectCast(vbproj, IFileReference).FilePath _
                .ParentPath _
                .Replace("\", "/")
            Dim files As IEnumerable(Of String) = ls - l - r - "*.vb" <= sourceDir
            Dim relative As String() = files _
                .Select(Function(path)
                            Return path.Replace("\", "/").Replace(sourceDir, "")
                        End Function) _
                .ToArray

            Return relative
        End Function

        <Extension>
        Private Function legacyProjectSource(vbproj As Project) As IEnumerable(Of String)
            Dim itemList As ItemGroup() = vbproj.ItemGroups
            Dim sourceList As IEnumerable(Of String) = itemList _
                .Where(Function(items) Not items.Compiles.IsNullOrEmpty) _
                .Select(Function(items)
                            Return items.Compiles _
                               .Where(Function(vb)
                                          Return Not True = vb.AutoGen.ParseBoolean
                                      End Function) _
                               .Select(Function(vb)
                                           Return vb.Include.Replace("%28", "(").Replace("%29", ")")
                                       End Function)
                        End Function) _
                .IteratesALL

            Return sourceList
        End Function

        <Extension>
        Private Function vbfileFilter(sourcefiles As IEnumerable(Of String),
                                      sourceFolder As String,
                                      fullName As Boolean,
                                      skipAssmInfo As Boolean) As IEnumerable(Of String)
            Return sourcefiles _
                .Select(Function(relative)
                            If fullName Then
                                Return $"{sourceFolder}/{relative}"
                            Else
                                Return relative
                            End If
                        End Function) _
                .Where(Function(vb)
                           If skipAssmInfo Then
                               Return Not vb.EndsWith(Development.AssemblyInfo.ProjectFile)
                           Else
                               Return True
                           End If
                       End Function)
        End Function

        <Extension>
        Public Function AssemblyInfo(vbproj As Project) As AssemblyInfo
            With DirectCast(vbproj, IFileReference)
                If Not .FilePath.FileExists Then
                    Return New AssemblyInfo With {
                        .BuiltTime = Now
                    }
                Else
                    Return GetAssemblyInfo(.FilePath)
                End If
            End With
        End Function

        <Extension>
        Public Function GetOutputDirectory(vbproj As Project, profileName$) As String
            Dim profile = vbproj.GetProfile(profileName)
            Dim base$ = DirectCast(vbproj, IFileReference).FilePath.ParentPath
            Dim outputdir = $"{base}/{profile.OutputPath}"

            Return outputdir
        End Function

        ''' <summary>
        ''' Get output assembly name
        ''' </summary>
        ''' <param name="vbproj"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetOutputName(vbproj As Project) As String
            Return vbproj.PropertyGroups _
                .FirstOrDefault(Function(p)
                                    Return Not p.AssemblyName.StringEmpty
                                End Function) _
                .AssemblyName
        End Function

        <Extension>
        Public Function RootNamespace(vbproj As Project) As String
            Return vbproj.PropertyGroups _
                .FirstOrDefault(Function(p)
                                    Return Not p.RootNamespace.StringEmpty
                                End Function) _
                .RootNamespace
        End Function
    End Module
End Namespace
