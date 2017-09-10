#Region "Microsoft.VisualBasic::76c245de0d1a4b3d4d720d5dfec3520d, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\StringHelpers\IEqualityComparer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports Microsoft.VisualBasic.Text.Levenshtein

''' <summary>
''' thresholds:
''' 
''' + 0: text equals
''' + 1: binary equals
''' + (0, 1): similarity threshold
''' </summary>
Public Class StringEqualityHelper : Implements IEqualityComparer(Of String)

    ''' <summary>
    ''' + 0: text equals
    ''' + 1: binary equals
    ''' + (0, 1): similarity threshold
    ''' </summary>
    Dim threshold#
    Dim compare As IEquals(Of String)

    ''' <summary>
    ''' thresholds:
    ''' 
    ''' + 0: text equals
    ''' + 1: binary equals
    ''' + (0, 1): similarity threshold
    ''' </summary>
    Sub New(threshold#)
        Me.threshold = threshold

        If threshold = 0R Then
            compare = Function(s1, s2) s1.TextEquals(s2)
        ElseIf threshold = 1 Then
            compare = Function(s1, s2) s1 = s2
        Else
            compare = Function(s1, s2)
                          With LevenshteinDistance.ComputeDistance(s1, s2)
                              Return?.MatchSimilarity >= threshold
                          End With
                      End Function
        End If
    End Sub

    Public Shared ReadOnly Property TextEquals As New StringEqualityHelper(0)
    Public Shared ReadOnly Property BinaryEquals As New StringEqualityHelper(1)

    Public Overrides Function ToString() As String
        If threshold = 0R Then
            Return "Text Equals Of the Two String"
        ElseIf threshold = 1.0R Then
            Return "Binary Equals Of the Two String"
        Else
            Return "String Similarity With threshold " & threshold
        End If
    End Function

    Public Overloads Function Equals(x As String, y As String) As Boolean Implements IEqualityComparer(Of String).Equals
        Return compare(x, y)
    End Function

    Public Overloads Function GetHashCode(obj As String) As Integer Implements IEqualityComparer(Of String).GetHashCode
        If obj Is Nothing Then
            Return 0
        End If
        Return obj.GetHashCode
    End Function
End Class

