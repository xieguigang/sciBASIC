#Region "Microsoft.VisualBasic::9453992f24a80c01ef174c4955c953ef, vs_solutions\dev\vs_PDB\Extensions.vb"

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

    '   Total Lines: 82
    '    Code Lines: 46 (56.10%)
    ' Comment Lines: 21 (25.61%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 15 (18.29%)
    '     File Size: 3.42 KB


    ' Module Extensions
    ' 
    '     Function: (+2 Overloads) PointLocal2Github, ToRelativePath
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.Models

''' <summary>
''' Extension helpers for the unified <see cref="PDB"/> model.
''' </summary>
Public Module Extensions

    ''' <summary>
    ''' Remap the local source-file paths extracted from a PDB to GitHub blob URLs. The relative
    ''' path inside the repository is obtained by locating <paramref name="repoName"/> inside the
    ''' absolute path; if that fails it falls back to stripping the drive letter. The resulting
    ''' URL is stored in <see cref="SourceDocument.GitHubUrl"/> for every document.
    ''' </summary>
    ''' <returns>The same <see cref="PDB"/> instance (so calls can be chained).</returns>
    <Extension>
    Public Function PointLocal2Github(pdb As PDB, userName$, repoName$, commitID$) As PDB
        Return PointLocal2Github(pdb, userName, repoName, commitID, Nothing)
    End Function

    ''' <summary>
    ''' Overload that remaps using an explicit local repository root: the <paramref name="localRoot"/>
    ''' prefix is stripped first for the most accurate relative path, then the <paramref name="repoName"/>
    ''' matching / drive fallback is applied.
    ''' </summary>
    <Extension>
    Public Function PointLocal2Github(pdb As PDB, userName$, repoName$, commitID$, localRoot As String) As PDB
        If pdb Is Nothing Then
            Return Nothing
        End If

        Dim base As String = $"https://github.com/{userName}/{repoName}/blob/{commitID}/"

        For Each doc As SourceDocument In pdb.SourceDocuments
            Dim rel As String = ToRelativePath(doc.FilePath, repoName, localRoot)

            If rel.Length > 0 Then
                doc.GitHubUrl = base & rel
            End If
        Next

        Return pdb
    End Function

    ''' <summary>
    ''' Compute the repository-relative path (forward slashes) for a local absolute path.
    ''' </summary>
    Private Function ToRelativePath(filePath As String, repoName As String, localRoot As String) As String
        If String.IsNullOrEmpty(filePath) Then
            Return ""
        End If

        Dim path As String = filePath.Replace("\", "/")

        ' 1) explicit local root wins when it is a prefix.
        If Not String.IsNullOrEmpty(localRoot) Then
            Dim root As String = localRoot.Replace("\", "/").TrimEnd("/"c)

            If path.StartsWith(root, StringComparison.OrdinalIgnoreCase) Then
                Return path.Substring(root.Length).TrimStart("/"c)
            End If
        End If

        ' 2) match the repository name (ignore '#' differences, e.g. a local "sciBASIC#" checkout).
        If Not String.IsNullOrEmpty(repoName) Then
            Dim np As String = path.Replace("#", "")
            Dim nr As String = repoName.Replace("#", "")
            Dim idx As Integer = np.IndexOf(nr, StringComparison.OrdinalIgnoreCase)

            If idx >= 0 Then
                Return np.Substring(idx + nr.Length).TrimStart("/"c)
            End If
        End If

        ' 3) fallback: drop the drive letter (e.g. "g:/GCModeller/..." -> "GCModeller/...").
        If path.Length >= 2 AndAlso path(1) = ":"c Then
            Return path.Substring(2).TrimStart("/"c)
        End If

        Return path.TrimStart("/"c)
    End Function
End Module
