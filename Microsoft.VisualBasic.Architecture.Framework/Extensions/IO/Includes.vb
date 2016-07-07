#Region "baecd7c40fa690f33980306fa67e881f, ..\Microsoft.VisualBasic.Architecture.Framework\Extensions\IO\Includes.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileIO

    ''' <summary>
    ''' File includes search tools
    ''' </summary>
    Public Class Includes

        Dim __innerDIRs As List(Of String)

        Sub New(ParamArray DIR As String())
            __innerDIRs = New List(Of String)(DIR)
        End Sub

        ''' <summary>
        ''' Add includes directory into the search path.
        ''' </summary>
        ''' <param name="DIR"></param>
        Public Sub Add(DIR As String)
            __innerDIRs += DIR
        End Sub

        ''' <summary>
        ''' Get the absolutely file path from the includes file's relative path.
        ''' </summary>
        ''' <param name="relPath"></param>
        ''' <returns></returns>
        Public Function GetPath(relPath As String) As String
            For Each DIR As String In __innerDIRs
                Dim path As String = DIR & "/" & relPath
                path = FileIO.FileSystem.GetFileInfo(path).FullName
                If path.FileExists Then
                    Return path
                End If
            Next

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return "Searchs in directories: " & __innerDIRs.GetJson
        End Function
    End Class
End Namespace
