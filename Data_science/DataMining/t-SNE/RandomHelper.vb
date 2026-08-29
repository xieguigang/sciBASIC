#Region "Microsoft.VisualBasic::4764b30e08872ff62f2394e48625b7b1, Data_science\DataMining\t-SNE\RandomHelper.vb"

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

    '   Total Lines: 108
    '    Code Lines: 46 (42.59%)
    ' Comment Lines: 43 (39.81%)
    '    - Xml Docs: 93.02%
    ' 
    '   Blank Lines: 19 (17.59%)
    '     File Size: 3.49 KB


    ' Class RandomHelper
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GaussRandom, randn, (+2 Overloads) randn2d
    ' 
    ' /********************************************************************************/

#End Region

Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Friend Class RandomHelper

    ReadOnly tSNE As tSNE

    ''' <summary>
    ''' Marsaglia polar 方法成对产出的第二个正态随机数的缓存
    ''' </summary>
    Private mSpare As Double = 0.0
    Private mHasSpare As Boolean = False

    Sub New(tSNE As tSNE)
        Me.tSNE = tSNE
    End Sub

    ''' <summary>
    ''' 返回 0 均值单位标准差随机数
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 这里使用的是 Marsaglia polar 方法，每一次采样会成对地产出两个正态随机数。
    ''' 
    ''' 原始的 JS 参考实现把第二个数字缓存在 tSNE 实例之上（mRet 与 mVal 字段），
    ''' 这在并行初始化的场景下会被多个线程互相覆盖，从而产生静默的数值错误。
    ''' 此处改为把缓存下沉到 RandomHelper 实例自身的字段之上，
    ''' 由于 InitSolution 保持串行执行，产出的随机序列与改造前逐位一致。
    ''' 
    ''' 底层的 RandomExtensions.NextDouble 基于 ThreadLocal(Of Random) 实现，
    ''' 本身即为线程安全的。
    ''' </remarks>
    Private Function GaussRandom() As Double
        If mHasSpare Then
            mHasSpare = False
            Return mSpare
        End If

        Dim u As Double = randf.NextDouble() - 1
        Dim v As Double = randf.NextDouble() - 1
        Dim r As Double = u * u + v * v

        If r = 0 OrElse r > 1 Then
            Return GaussRandom()
        End If

        Dim c = std.Sqrt(-2 * std.Log(r) / r)

        ' cache this for next function call for efficiency
        mSpare = v * c
        mHasSpare = True

        Return u * c
    End Function

    ''' <summary>
    ''' return random normal number
    ''' </summary>
    ''' <param name="mu"></param>
    ''' <param name="std"></param>
    ''' <returns></returns>
    Private Function randn(mu As Double, std As Double) As Double
        Return mu + GaussRandom() * std
    End Function

    ''' <summary>
    ''' utility that returns a contiguous row-major 2d buffer filled with
    ''' random normal numbers (mu = 0, sigma = 1e-4)
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="d"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 返回的是拉平之后的一维数组（行主序，索引为 <c>i * d + j</c>），
    ''' 相比改造前的锯齿数组版本减少了 n 次小数组分配，缓存局部性更好。
    ''' </remarks>
    Friend Function randn2d(n As Integer, d As Integer) As Double()
        Dim x = New Double(n * d - 1) {}

        For i As Integer = 0 To n - 1
            Dim offset = i * d

            For j As Integer = 0 To d - 1
                x(offset + j) = randn(0.0, 0.0001)
            Next
        Next

        Return x
    End Function

    ''' <summary>
    ''' utility that returns a contiguous row-major 2d buffer filled with value s
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="d"></param>
    ''' <param name="s"></param>
    ''' <returns></returns>
    Friend Shared Function randn2d(n As Integer, d As Integer, s As Double) As Double()
        Dim x = New Double(n * d - 1) {}

        For i As Integer = 0 To n * d - 1
            x(i) = s
        Next

        Return x
    End Function

End Class
