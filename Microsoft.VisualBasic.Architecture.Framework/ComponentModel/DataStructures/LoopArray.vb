#Region "Microsoft.VisualBasic::2a23fb65e947d44ab1b57562081a7597, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\LoopArray.vb"

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

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataStructures

    Public Class LoopArray(Of T)

        Dim __innerArray As T()
        Dim __p As Integer

        Public ReadOnly Property Raw As T()
            Get
                Return __innerArray
            End Get
        End Property

        Public ReadOnly Property Length As Integer
            Get
                Return __innerArray.Length
            End Get
        End Property

        Sub New(source As IEnumerable(Of T))
            __innerArray = source.ToArray
        End Sub

        ''' <summary>
        ''' Gets the next elements in the array, is move to end, then the index will moves to the array begining position.
        ''' </summary>
        ''' <returns></returns>
        Public Function [Next]() As T
            If __p < __innerArray.Length - 1 Then
                __p += 1
            Else
                __p = 0
            End If

            Return __innerArray(__p)
        End Function

        Public Sub [Set](i%)
            __p = i%
        End Sub

        Public Overrides Function ToString() As String
            Return __innerArray.Take(10).ToArray.GetJson
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="delta%">The pointer move delta</param>
        ''' <returns></returns>
        Public Function [GET](delta%) As T
            __p += delta

            If __p >= 0 Then
                If __p <= __innerArray.Length - 1 Then
                    ' 正常的下标范围内，不需要进行任何处理
                Else
                    __p = __p - __innerArray.Length
                End If
            Else
                __p = __innerArray.Length + __p
            End If

            Return __innerArray(__p)
        End Function
    End Class
End Namespace
