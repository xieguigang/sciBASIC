#Region "Microsoft.VisualBasic::95155070099b2ba9b2814538e446dacd, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\NumericSequence.vb"

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

    '   Total Lines: 85
    '    Code Lines: 64 (75.29%)
    ' Comment Lines: 8 (9.41%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 13 (15.29%)
    '     File Size: 3.29 KB


    '     Module NumericSequence
    ' 
    '         Function: InRange, (+7 Overloads) Range
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default

Namespace Linq

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

        Public ReadOnly OneDay As [Default](Of TimeSpan) = New TimeSpan(days:=1, hours:=0, minutes:=0, seconds:=0)
        Public ReadOnly OneHour As [Default](Of TimeSpan) = New TimeSpan(hours:=1, minutes:=0, seconds:=0)
        Public ReadOnly OneMinute As [Default](Of TimeSpan) = New TimeSpan(minutes:=1, seconds:=0, hours:=0)
        Public ReadOnly OneSecond As [Default](Of TimeSpan) = New TimeSpan(seconds:=1, hours:=0, minutes:=0)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="from"></param>
        ''' <param name="[to]"></param>
        ''' <param name="step%">Milliseconds</param>
        ''' <returns></returns>
        Public Iterator Function Range(from As Date, [to] As Date, Optional [step] As TimeSpan = Nothing) As IEnumerable(Of Date)
            Dim t As TimeSpan = [step] Or OneDay

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
End Namespace
