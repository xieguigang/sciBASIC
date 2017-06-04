#Region "Microsoft.VisualBasic::4008ed89a1c0248512e9d96e9a4e7bda, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\NumericSequence.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Public Module NumericSequence

    <Extension>
    Public Function InRange(Of T As IComparable)(x As T, min As T, max As T) As Boolean
        If x.LessThanOrEquals(max) AndAlso x.GreaterThanOrEquals(min) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Iterator Function Range(from%, to%, Optional step% = 1) As IEnumerable(Of Integer)
        For i As Integer = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from#, to#, Optional step# = 1) As IEnumerable(Of Double)
        For i As Double = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from!, to!, Optional step! = 1) As IEnumerable(Of Single)
        For i As Single = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from&, to&, Optional step& = 1) As IEnumerable(Of Long)
        For i As Long = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from@, to@, Optional step@ = 1) As IEnumerable(Of Decimal)
        For i As Decimal = from To [to] Step [step]
            Yield i
        Next
    End Function

    Public Iterator Function Range(from As Short, [to] As Short, Optional [step] As Short = 1) As IEnumerable(Of Short)
        For i As Short = from To [to] Step [step]
            Yield i
        Next
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="from"></param>
    ''' <param name="[to]"></param>
    ''' <param name="step%">Milliseconds</param>
    ''' <returns></returns>
    Public Iterator Function Range(from As Date, [to] As Date, Optional step# = 1) As IEnumerable(Of Date)
        Dim t As TimeSpan = TimeSpan.FromMilliseconds([step])

        ' 由于后面的Do循环之中from是放在加减法之后的，所以需要在这里把最开始的值返回，要不然第一个值会被丢失的。。。
        Yield from

        If from < [to] Then
            Do While from < [to]
                from = from + t
                Yield from
            Loop
        Else
            Do While from > [to]
                from = from - t
                Yield from
            Loop
        End If
    End Function
End Module

