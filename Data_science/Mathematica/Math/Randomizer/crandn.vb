#Region "Microsoft.VisualBasic::512944ef23ab5129c65b2a7039ec4bcf, Data_science\Mathematica\Math\Randomizer\crandn.vb"

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

    '   Total Lines: 218
    '    Code Lines: 92 (42.20%)
    ' Comment Lines: 92 (42.20%)
    '    - Xml Docs: 40.22%
    ' 
    '   Blank Lines: 34 (15.60%)
    '     File Size: 7.73 KB


    ' Module crandn
    ' 
    '     Function: (+2 Overloads) rand, (+2 Overloads) randn
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports MAT = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.GeneralMatrix
Imports stdNum = System.Math

''' <summary>
''' 正态分布随机数
''' </summary>
Public Module crandn

    ''' <summary>
    ''' 生成标准正态分布随机向量
    ''' </summary>
    ''' <param name="m">向量维数</param>
    ''' <param name="seed">混合同余种子</param>
    ''' <returns></returns>
    Public Function randn(m As Integer, seed As Integer) As Vector
        '------------------------------------------ comment
        '    Author      : syz 
        '    Date        : 2011-07-12 21:55:12         
        '    ---------------------------------------------------
        '    Desciption  : 生成标准正态分布随机数向量
        '     *
        '    Post Script :
        '     *
        '    paramtegers :
        '     *   m--------向量维数
        '     *   seed-----混合同余种子
        '    -------------------------------------------------

        Dim gauss As New Vector(m)
        Dim tmp1 As Double

        If m Mod 2 = 0 Then
            '如果是偶数
            '同时用两个均匀分布随机数
            '产生两个高斯分布随机数

            '生成均匀分布向量
            Dim u As Vector = rand(m, seed)
            Dim k As Integer = 0

            While k < m
                tmp1 = stdNum.Sqrt(-2 * stdNum.Log(1 - u(k)))
                gauss(k) = tmp1 * stdNum.Cos(2 * stdNum.PI * u(k + 1))
                gauss(k + 1) = tmp1 * stdNum.Sin(2 * stdNum.PI * u(k + 1))

                k = k + 2
            End While
        Else
            '生成均匀分布向量
            '如果是奇数，则多生成一个均匀数，

            Dim u As Vector = rand(m + 1, seed)
            Dim k As Integer = 0

            While k < m - 1
                tmp1 = stdNum.Sqrt(-2 * stdNum.Log(1 - u(k)))
                gauss(k) = tmp1 * stdNum.Cos(2 * stdNum.PI * u(k + 1))
                gauss(k + 1) = tmp1 * stdNum.Sin(2 * stdNum.PI * u(k + 1))

                k = k + 2
            End While

            tmp1 = stdNum.Sqrt(-2 * stdNum.Log(1 - u(m - 1)))
            gauss(m - 1) = tmp1 * stdNum.Cos(2 * stdNum.PI * u(m))
        End If

        Return gauss
    End Function

    ''' <summary>
    ''' 生成标准正态分布随机矩阵
    ''' </summary>
    ''' <param name="m">矩阵维数</param>
    ''' <param name="n">矩阵维数</param>
    ''' <param name="seed">混合同余种子</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 由于矩阵的行和列都可能为奇数，所以先判断总元素
    ''' 的奇偶性，如果是偶数，先生成偶数随机向量，后把
    ''' 随机向量赋值给随机矩阵。
    ''' 如果总元素为奇数，按照VEC rand中类似的方法先
    ''' 生成随机向量，后把向量复制给随机矩阵。
    ''' </remarks>
    Public Function randn(m As Integer, n As Integer, seed As Integer) As MAT
        '------------------------------------------ comment
        '    Author      : syz 
        '    Date        : 2011-07-12 21:57:36         
        '    ---------------------------------------------------
        '    Desciption  :  标准正态分布随机矩阵发生器
        '     *
        '    Post Script :
        '     *    由于矩阵的行和列都可能为奇数，所以先判断总元素
        '     *    的奇偶性，如果是偶数，先生成偶数随机向量，后把
        '     *    随机向量赋值给随机矩阵。
        '     *    如果总元素为奇数，按照VEC rand中类似的方法先
        '     *    生成随机向量，后把向量复制给随机矩阵。
        '     *
        '    paramtegers :
        '     *  [m,n]----矩阵维数
        '     *  seed-----混合同余种子
        '     *
        '    -------------------------------------------------

        Dim p As Integer = m * n
        Dim gauss As New Vector(p)
        Dim tmp1 As Double

        If p Mod 2 = 0 Then
            '生成均匀分布向量
            Dim u As Vector = rand(p, seed)
            Dim k As Integer = 0

            While k < p
                tmp1 = stdNum.Sqrt(-2 * stdNum.Log(1 - u(k)))
                gauss(k) = tmp1 * stdNum.Cos(2 * stdNum.PI * u(k + 1))
                gauss(k + 1) = tmp1 * stdNum.Sin(2 * stdNum.PI * u(k + 1))

                k = k + 2
            End While
        Else
            '生成均匀分布向量
            '如果是奇数，则多生成一个均匀数，
            Dim u As Vector = rand(p + 1, seed)
            Dim k As Integer = 0

            While k < p - 1
                tmp1 = stdNum.Sqrt(-2 * stdNum.Log(1 - u(k)))
                gauss(k) = tmp1 * stdNum.Cos(2 * stdNum.PI * u(k + 1))
                gauss(k + 1) = tmp1 * stdNum.Sin(2 * stdNum.PI * u(k + 1))

                k = k + 2
            End While

            tmp1 = stdNum.Sqrt(-2 * stdNum.Log(1 - u(m)))
            gauss(m) = tmp1 * stdNum.Cos(2 * stdNum.PI * u(m + 1))
        End If

        Dim goal As New NumericMatrix(m, n)

        For i As Integer = 0 To m - 1
            For j As Integer = 0 To n - 1
                goal(i, j) = gauss(i * n + j)
            Next
        Next

        Return goal
    End Function

    Const Mlng As Long = 2147483648UI
    Const namda As Long = 314159269
    Const C As Long = 453806245

    ''' <summary>
    ''' 混合同余法产生[0，1]均匀分布随机数向量
    ''' </summary>
    ''' <param name="m">向量维数</param>
    ''' <param name="seed">种子</param>
    ''' <returns></returns>
    Public Function rand(m As Integer, seed As Integer) As Vector
        '------------------------------------------ comment
        '    Author      : syz 
        '    Date        : 2011-07-12 19:39:36         
        '    ---------------------------------------------------
        '    Desciption  : 混合同余法产生[0，1]均匀分布随机数向量
        '     *
        '    Post Script :
        '     *       
        '    paramtegers :
        '     * m--------向量维数
        '     * seed-----种子
        '    -------------------------------------------------
        Dim goal As New Vector(m)
        Dim x0 As Long = seed
        Dim x1 As Long

        For k As Integer = 0 To m - 1
            x1 = (x0 * namda + C) Mod Mlng
            '得到余数

            '通过乘以1.0把整数运算变为浮点数运算，否则结果都为0
            goal(k) = x1 * 1.0 / Mlng

            '下一个起点以之前的余数为种子
            x0 = x1
        Next

        Return goal
    End Function

    ''' <summary>
    ''' 混合同余法生成[0，1]均匀分布随机矩阵
    ''' 与随机向量函数同名重载
    ''' </summary>
    ''' <param name="m">矩阵维数</param>
    ''' <param name="n">矩阵维数</param>
    ''' <param name="seed">种子</param>
    ''' <returns></returns>
    Public Function rand(m As Integer, n As Integer, seed As Integer) As MAT
        Dim goal As New NumericMatrix(m, n)
        Dim x0 As Long = seed
        Dim x1 As Long

        For i As Integer = 0 To m - 1
            For j As Integer = 0 To n - 1
                x1 = (x0 * namda + C) Mod Mlng
                '得到余数

                goal(i, j) = x1 * 1.0 / Mlng


                x0 = x1
            Next
        Next
        Return goal
    End Function
End Module
