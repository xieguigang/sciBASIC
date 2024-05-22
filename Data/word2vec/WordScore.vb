#Region "Microsoft.VisualBasic::8e9838375a586e46e50ef25f7e26e1ba, Data\word2vec\WordScore.vb"

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

    '   Total Lines: 27
    '    Code Lines: 20 (74.07%)
    ' Comment Lines: 3 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (14.81%)
    '     File Size: 719 B


    ' Class WordScore
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CompareTo, ToString
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' the word score of the vector model
''' </summary>
Public Class WordScore : Implements IComparable(Of WordScore)

    Public name As String
    Public score As Single

    Public Sub New(name As String, score As Single)
        Me.name = name
        Me.score = score
    End Sub

    Public Overrides Function ToString() As String
        Return name & vbTab & score
    End Function

    Public Function CompareTo(o As WordScore) As Integer Implements IComparable(Of WordScore).CompareTo
        If score = o.score Then
            Return 0
        ElseIf score < o.score Then
            Return 1
        Else
            Return -1
        End If
    End Function
End Class
