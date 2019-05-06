#Region "Microsoft.VisualBasic::4080080483d8b5d1fb4c546f0da75c08, vs_solutions\dev\VisualStudio\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: AssemblyInfo, (+2 Overloads) EnumerateSourceFiles, GetOutputDirectory, GetOutputName, RootNamespace
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

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
    Public Function EnumerateSourceFiles(vbproj As Project, Optional skipAssmInfo As Boolean = False) As IEnumerable(Of String)
        Return vbproj _
            .ItemGroups _
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
            .IteratesALL _
            .Where(Function(vb)
                       If skipAssmInfo Then
                           Return vb <> Development.AssemblyInfo.ProjectFile
                       Else
                           Return True
                       End If
                   End Function)
    End Function

    <Extension>
    Public Function AssemblyInfo(vbproj As Project) As AssemblyInfo
        With DirectCast(vbproj, IFileReference)
            If Not .FilePath.FileExists Then
                Return New AssemblyInfo
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
