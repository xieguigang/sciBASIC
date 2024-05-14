#Region "Microsoft.VisualBasic::fe0859768551c7e7b3e91488ee892039, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Nudge\ConflictIndexTuple.vb"

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

    '   Total Lines: 36
    '    Code Lines: 23
    ' Comment Lines: 8
    '   Blank Lines: 5
    '     File Size: 1.12 KB


    '     Class ConflictIndexTuple
    ' 
    '         Properties: i, j
    ' 
    '         Function: [In], Equals, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing2D.Text.Nudge

    Public Class ConflictIndexTuple

        ''' <summary>
        ''' [0] item 1
        ''' </summary>
        ''' <returns></returns>
        Public Property i As Integer
        ''' <summary>
        ''' [1] item 2
        ''' </summary>
        ''' <returns></returns>
        Public Property j As Integer

        Public Overrides Function ToString() As String
            Return $"[{i} == {j}]"
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Not TypeOf obj Is ConflictIndexTuple Then
                Return False
            ElseIf obj Is Me Then
                Return True
            Else
                With DirectCast(obj, ConflictIndexTuple)
                    Return i = .i AndAlso j = .j
                End With
            End If
        End Function

        Public Shared Function [In](conflicts As ConflictIndexTuple(), parent As IEnumerable(Of ConflictIndexTuple)) As Boolean
            Throw New NotImplementedException
        End Function
    End Class
End Namespace
