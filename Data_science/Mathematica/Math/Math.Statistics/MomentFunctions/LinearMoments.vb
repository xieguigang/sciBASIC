#Region "Microsoft.VisualBasic::2b418f72a98360b0a393d3834b4dfbfe, Data_science\Mathematica\Math\Math.Statistics\MomentFunctions\LinearMoments.vb"

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

    '   Total Lines: 86
    '    Code Lines: 61 (70.93%)
    ' Comment Lines: 11 (12.79%)
    '    - Xml Docs: 27.27%
    ' 
    '   Blank Lines: 14 (16.28%)
    '     File Size: 2.62 KB


    '     Class LinearMoments
    ' 
    '         Properties: L1, L2, L3, L4, Max
    '                     Min, SampleSize, T1, T3, T4
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace MomentFunctions


    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class LinearMoments

        Public ReadOnly Property L1 As Double
        Public ReadOnly Property L2 As Double
        Public ReadOnly Property L3 As Double
        Public ReadOnly Property L4 As Double
        Public ReadOnly Property SampleSize As Integer
        Public ReadOnly Property Max As Double
        Public ReadOnly Property Min As Double

        Public ReadOnly Property T1() As Double
            Get
                Return _L2 / _L1
            End Get
        End Property

        Public ReadOnly Property T3() As Double
            Get
                Return _L3 / _L2
            End Get
        End Property

        Public ReadOnly Property T4() As Double
            Get
                Return _L4 / _L2
            End Get
        End Property

        Public Sub New(data As Double())
            Dim Count = data.Length

            ' sorts ascending based on javadocs
            Call Array.Sort(data)

            _Min = data(0)
            _Max = data(Count - 1)
            _SampleSize = Count

            Dim cl2 As Long
            Dim cl3 As Long
            Dim cl4 As Long
            Dim cr1 As Long
            Dim cr2 As Long
            Dim cr3 As Long
            Dim sl1 As Double = 0
            Dim sl2 As Double = 0
            Dim sl3 As Double = 0
            Dim sl4 As Double = 0

            For i As Integer = 0 To data.Length - 1
                cl2 = CLng((i * (i - 1)) \ 2)
                cl3 = CLng((cl2 * (i - 2)) \ 3)
                cr1 = Count - (i + 1)
                cr2 = CLng(cr1 * ((Count - (i + 2))) \ 2)
                cr3 = CLng(cr2 * ((Count - (i + 3))) \ 3)
                sl1 += data(i)
                sl2 += data(i) * (i - cr1)
                sl3 += data(i) * (cl2 - 2 * i * cr1 + cr2)
                sl4 += data(i) * (cl3 - 3 * cl2 * cr1 + 3 * i * cr2 - cr3)
            Next i

            ' not sure order of operations is correct here..
            cl2 = CLng(Count) * (Count - 1) \ 2
            cl3 = CLng(cl2) * (Count - 2) \ 3
            cl4 = CLng(cl3) * (Count - 3) \ 4
            _L1 = sl1 / Count
            _L2 = sl2 / cl2 \ 2
            _L3 = sl3 / cl3 \ 3
            _L4 = sl4 / cl4 \ 4
        End Sub

    End Class

End Namespace
