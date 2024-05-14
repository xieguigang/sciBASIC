#Region "Microsoft.VisualBasic::faa0ca9d374af8c746f7922ff359503f, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\FileIO\DocumentPath.vb"

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

    '   Total Lines: 50
    '    Code Lines: 20
    ' Comment Lines: 23
    '   Blank Lines: 7
    '     File Size: 1.77 KB


    '     Class DocumentPath
    ' 
    '         Properties: Filename, Path
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetFullPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLSX.FileIO

    ''' <summary>
    ''' Class to manage XML document paths
    ''' </summary>
    Friend Class DocumentPath

        ''' <summary>
        ''' Gets or sets the Filename
        ''' File name of the document
        ''' </summary>
        Public Property Filename As String

        ''' <summary>
        ''' Gets or sets the Path
        ''' Path of the document
        ''' </summary>
        Public Property Path As String

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DocumentPath"/> class
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DocumentPath"/> class
        ''' </summary>
        ''' <param name="filename">File name of the document.</param>
        ''' <param name="path">Path of the document.</param>
        Public Sub New(filename As String, path As String)
            Me.Filename = filename
            Me.Path = path
        End Sub

        ''' <summary>
        ''' Method to return the full path of the document
        ''' </summary>
        ''' <returns>Full path.</returns>
        Public Function GetFullPath() As String
            Dim lastCharOfPath As Char = Path.Last

            If lastCharOfPath = System.IO.Path.AltDirectorySeparatorChar OrElse lastCharOfPath = System.IO.Path.DirectorySeparatorChar Then
                Return System.IO.Path.AltDirectorySeparatorChar.ToString() & Path & Filename
            Else
                Return System.IO.Path.AltDirectorySeparatorChar.ToString() & Path & System.IO.Path.AltDirectorySeparatorChar.ToString() & Filename
            End If
        End Function
    End Class
End Namespace
