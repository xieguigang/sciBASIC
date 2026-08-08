#Region "Microsoft.VisualBasic::48799b9243cdc49f78c56dbba5a6ad85, vs_solutions\dev\VisualStudio\sln\FolderWorkspace.vb"

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

    '   Total Lines: 49
    '    Code Lines: 32 (65.31%)
    ' Comment Lines: 8 (16.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (18.37%)
    '     File Size: 1.60 KB


    '     Class FolderWorkspace
    ' 
    '         Properties: Name, Path
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CreateFs, GetCompileFiles, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace sln

    Public Class FolderWorkspace : Implements IProjectWorkspace

        ''' <summary>
        ''' the name of the folder
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String Implements IProjectWorkspace.Name
            Get
                Return Path.BaseName
            End Get
        End Property

        ''' <summary>
        ''' the folder full path
        ''' </summary>
        ''' <returns></returns>
        Public Property Path As String

        Sub New()
        End Sub

        Sub New(dir As String)
            Path = dir
        End Sub

        Public Overrides Function ToString() As String
            Return Path
        End Function

        Public Function GetCompileFiles() As IEnumerable(Of String) Implements IProjectWorkspace.GetCompileFiles
            Return From file As String
                   In (ls - l - r - "*.*" <= Path)
                   Let rel As String = ProjectFiles.GetRelativePath(Path, file)
                   Where Not ProjectFiles.IsExcludedByDefault(rel)
                   Select rel
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateFs(ws As IProjectWorkspace) As FileSystemTree
            Return FileSystemTree.BuildTree(ws.GetCompileFiles)
        End Function
    End Class
End Namespace
