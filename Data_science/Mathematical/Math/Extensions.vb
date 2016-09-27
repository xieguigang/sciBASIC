#Region "Microsoft.VisualBasic::94b8a0dae23d7b9403e1f7f6b811f1f1, ..\visualbasic_App\Data_science\Mathematical\Math\Extensions.vb"

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
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    ''' <summary>
    ''' 返回数值序列之中的首次出现符合条件的减少的位置
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="ratio"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FirstDecrease(data As IEnumerable(Of Double), Optional ratio As Double = 10) As Integer
        Dim pre As Double = data.First
        Dim pr As Double = 1000000

        For Each x In data.SeqIterator
            Dim d = (pre - x.obj)

            If d / pr > ratio Then
                Return x.i
            Else
                pr = d
                pre = x.obj
            End If
        Next

        Return -1 ' 没有找到符合条件的点
    End Function

    ''' <summary>
    ''' 只对单调递增的那一部分曲线有效
    ''' </summary>
    ''' <param name="data">y值</param>
    ''' <param name="alpha"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FirstIncrease(data As IEnumerable(Of Double), dx As Double, Optional alpha As Double = 30) As Integer
        Dim pre As Double = data.First
        Dim pr As Double = 1000000

        For Each x In data.Skip(1).SeqIterator(offset:=1)
            Dim dy = (x.obj - pre) ' 对边
            Dim tanX As Double = dy / dx
            Dim a As Double = Atn(tanX)

            If a >= alpha Then
                Return x.i
            End If
        Next

        Return -1 ' 没有找到符合条件的点
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="n"></param>
    ''' <param name="offset">距离目标数据点<paramref name="n"/>的正负偏移量</param>
    ''' <returns></returns>
    <Extension>
    Public Function Reach(data As IEnumerable(Of Double), n As Double, Optional offset As Double = 0) As Integer
        For Each x In data.SeqIterator
            If Math.Abs(x.obj - n) <= offset Then
                Return x.i
            End If
        Next

        Return -1
    End Function
End Module

