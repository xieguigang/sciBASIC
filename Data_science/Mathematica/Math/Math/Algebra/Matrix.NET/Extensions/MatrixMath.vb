#Region "Microsoft.VisualBasic::333b610775ef5789c4df737090c0628f, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Extensions\MatrixMath.vb"

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

    '     Module vbMatrix
    ' 
    '         Function: Adj, Cond, Cramer22, Det2, DetF
    '                   DFT, EigenValue, EigSym, GetRank, Hamiltonian
    '                   Hessenberg, IDFT, Inv, Inv2, LLt
    '                   LU, Mul, Orth, Pinv, Pinv2
    '                   PolyDiv, PolyDivEx, PolyGCF, PolyGCFCall, PolyMod
    '                   PolyMul, PolyRoots2, Pow, QR, QR2
    '                   QR22, RU, Scatter, Schmidt, SG
    '                   Sove2, SPD, Sqrt, Svd, SvdSplit
    '                   SymTridMatrix, VR
    ' 
    '         Sub: EigTorF, Lehmer, Magic, Magic_1, Magic_2
    '              Magic_4, Pascal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace LinearAlgebra.Matrix

    Public Module vbMatrix

        ''' <summary>
        ''' 矩阵的满秩分解Math_Matrinx_SG，把矩阵K分解成一种行满秩Return_m 是m*r与列满秩的矩阵Return_n是r*n.返回值为r.r是其秩
        ''' </summary>
        ''' <param name="K">为要满秩分解的方阵</param>
        ''' <returns>
        ''' + 所求得的m*r矩阵
        ''' + 所求得的r*n矩阵
        ''' </returns>
        ''' <remarks>
        ''' 其中A为m*n的矩阵,r为A的秩.即A=Return_M*Return_N.函数执行成功返回r(也就是其秩)
        ''' </remarks>
        <Extension> Public Function SG(K As GeneralMatrix) As (M As GeneralMatrix, N As GeneralMatrix, rank As Int16)
            Dim erro As Double = stdNum.Pow(0.1, 10)
            Dim n As Int16 = K.RowDimension
            Dim m As Int16 = K.Length / n
            Dim i As Int16
            Dim j As Int16
            Dim Rank As Int16 = GetRank(K, 9)
            Dim return_M, return_N As GeneralMatrix

            If Rank = m And m = n Then
                return_M = New GeneralMatrix(m - 1, n - 1) '    ReDim Return_M(m - 1, n - 1)
                return_N = New GeneralMatrix(m - 1, n - 1) '    ReDim Return_N(m - 1, n - 1)
                For i = 0 To m - 1
                    return_N(i, i) = 1
                    For j = 0 To m - 1
                        return_M(i, j) = K(i, j)
                    Next
                Next
                Return (return_M, return_N, Rank)
            End If

            Dim tempk(m - 1, n - 1) As Double
            Dim temp As Double
            Dim i1 As Int16
            Dim j1 As Int16
            For i = 0 To m - 1
                For j = 0 To n - 1
                    tempk(i, j) = K(i, j)
                Next
            Next
            i = 0
            j = 0
            While i < m And j < n
                If tempk(i, j) = 0 Then
                    i1 = i + 1
                    While i1 < n
                        If tempk(i1, j) <> 0 Then
                            For j1 = j To n - 1
                                temp = tempk(i, j1)
                                tempk(i, j1) = tempk(i1, j1)
                                tempk(i1, j1) = temp
                            Next
                            Exit While
                        End If
                        i1 += 1
                    End While
                End If
                If tempk(i, j) <> 0 Then
                    For i1 = 0 To m - 1
                        If i1 <> i And tempk(i1, j) <> 0 Then
                            temp = tempk(i, j) / tempk(i1, j)
                            For j1 = 0 To n - 1
                                tempk(i1, j1) = tempk(i1, j1) * temp - tempk(i, j1)
                                If stdNum.Abs(tempk(i1, j1)) <= erro Then
                                    tempk(i1, j1) = 0
                                End If
                            Next
                        End If
                    Next
                    i += 1
                End If
                j += 1
            End While
            j1 = 0
            return_M = New GeneralMatrix(m - 1, Rank - 1) '    ReDim Return_M(m - 1, Rank - 1)
            For i = 0 To m - 1
                j = i
                While j < n
                    If tempk(i, j) <> 0 Then
                        For i1 = 0 To m - 1
                            return_M(i1, j1) = K(i1, j)
                        Next
                        j1 += 1
                        temp = tempk(i, j)
                        While j < n
                            tempk(i, j) /= temp
                            If stdNum.Abs(tempk(i, j)) <= erro Then
                                tempk(i, j) = 0
                            End If
                            j += 1
                        End While
                        Exit While
                    End If
                    j += 1
                End While
            Next
            return_N = New GeneralMatrix(Rank - 1, n - 1) '    ReDim Return_N(Rank - 1, n - 1)
            For i = 0 To Rank - 1
                For j = 0 To n - 1
                    return_N(i, j) = tempk(i, j)
                Next
            Next
            Return (return_M, return_N, Rank)
        End Function

        ''' <summary>
        ''' 矩阵的广义逆A+ ，返回m*n矩阵Return_K(,)的m。此广义逆是Moore-Penrose A+逆
        ''' </summary>
        ''' <param name="K">要求广义逆的矩阵</param>
        ''' <param name="Return_K">求得的广义逆矩阵</param>
        ''' <returns>函数执行成功返回m,其中m代表Return_K的行数</returns>
        ''' <remarks></remarks>
        Public Function Pinv(K As GeneralMatrix, Return_K As GeneralMatrix) As Int16
            Dim n As Int16 = K.RowDimension
            Dim rank As Int16 = GetRank(K, 9)
            Dim m As Int16 = K.Length / n
            Dim temp1 As GeneralMatrix = GeneralMatrix.Number
            Dim temp2 As GeneralMatrix = GeneralMatrix.Number
            Dim temp3 As GeneralMatrix = GeneralMatrix.Number
            If rank = m Then
                If m = n Then
                    Inv2(K, Return_K, n)
                Else
                    temp1 = K.Transpose '    Math_Matrix_T(K, n, temp1) 'temp1为转置
                    Mul(K, temp1, n, temp2)
                    Inv2(temp2, temp3, m) 'temp3为逆
                    Mul(temp1, temp3, m, Return_K)
                End If
            ElseIf rank = n Then
                temp1 = K.Transpose '     Math_Matrix_T(K, n, temp1) 'temp1为转置
                Mul(temp1, K, m, temp2)
                Inv2(temp2, temp3, n) 'temp3为逆
                Mul(temp3, temp1, n, Return_K)
            Else
                Dim s = GeneralMatrix.Number
                Dim g = GeneralMatrix.Number
                Dim s1(0, 0) As Double
                Dim g1(0, 0) As Double
                Dim s2(0, 0) As Double '逆
                Dim g2(0, 0) As Double
                With SG(K)
                    s = .M
                    g = .N
                End With
                temp1 = s.Transpose '   Math_Matrix_T(s, rank, temp1)
                temp2 = g.Transpose '      Math_Matrix_T(g, n, temp2)
                Mul(s, temp1, rank, s1)
                Mul(temp2, g, rank, g1)
                Inv2(s1, s2, m)
                Inv2(g1, g2, n)
                Mul(temp1, s2, m, temp3) 'rank*m
                Mul(g2, temp2, n, temp1) 'n*rank
                Mul(temp1, temp3, rank, Return_K)
            End If
            Return n
        End Function

        ''' <summary>
        ''' 矩阵的广义逆A-，函数执行成功返回Ret的行数,出错返回0
        ''' </summary>
        ''' <param name="K">要求广义逆的矩阵</param>
        ''' <param name="Erro">误差控制参数</param>
        ''' <param name="m">矩阵K的行数</param>
        ''' <param name="Ret">求得的广义逆矩阵</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Pinv2(K As GeneralMatrix, Erro As Int16, m As Int16, Ret As GeneralMatrix) As Int16
            Dim n As Integer = K.Length / m
            Dim Erro1 As Double = stdNum.Pow(0.1, Erro)
            If Erro1 = 1 Then
                Erro1 = 0
            End If
            Dim i As Integer
            Dim j As Integer
            Dim ii As Integer
            Dim jj As Integer
            m -= 1
            n -= 1
            Dim P(m, m) As Double
            Dim Q(n, n) As Double
            Dim temp As Double
            For i = 0 To n
                Q(i, i) = 1
            Next
            For i = 0 To m
                P(i, i) = 1
            Next
            For i = 0 To m
                If i = n + 1 Then
                    Exit For
                End If
                If stdNum.Abs(K(i, i)) <= Erro1 Then
                    K(i, i) = 0
                    ii = i + 1
                    While ii <= m
                        If stdNum.Abs(K(ii, i)) > Erro1 Then
                            For j = 0 To n
                                temp = K(i, j)
                                K(i, j) = K(ii, j)
                                K(ii, j) = temp
                            Next
                            For j = 0 To m
                                temp = P(i, j)
                                P(i, j) = P(ii, j)
                                P(ii, j) = temp
                            Next
                            Exit While
                        End If
                        K(ii, i) = 0
                        ii += 1
                    End While
                End If
                If K(i, i) <> 0 Then
                    For ii = 0 To m
                        If ii <> i Then
                            If stdNum.Abs(K(ii, i)) > Erro1 Then
                                temp = K(i, i) / K(ii, i)
                                For j = 0 To n
                                    K(ii, j) = K(ii, j) * temp - K(i, j)
                                    If stdNum.Abs(K(ii, j)) <= Erro1 Then
                                        K(ii, j) = 0
                                    End If
                                Next
                                For j = 0 To m
                                    P(ii, j) = P(ii, j) * temp - P(i, j)
                                    If stdNum.Abs(P(ii, j)) <= Erro1 Then
                                        P(ii, j) = 0
                                    End If
                                Next
                            Else
                                K(ii, i) = 0
                            End If
                        End If
                    Next
                End If
            Next
            For j = 0 To n
                If j = m + 1 Then
                    Exit For
                End If
                If stdNum.Abs(K(j, j)) <= Erro1 Then
                    K(j, j) = 0
                    jj = j + 1
                    While jj <= n
                        If stdNum.Abs(K(j, jj)) > Erro1 Then
                            For i = 0 To m
                                temp = K(i, j)
                                K(i, j) = K(i, jj)
                                K(i, jj) = temp
                            Next
                            For i = 0 To n
                                temp = Q(i, j)
                                Q(i, j) = Q(i, jj)
                                Q(i, jj) = temp
                            Next
                            Exit While
                        End If
                        K(j, jj) = 0
                        jj += 1
                    End While
                End If
                If K(j, j) <> 0 Then
                    For jj = 0 To n
                        If jj <> j Then
                            If stdNum.Abs(K(j, jj)) > Erro1 Then
                                temp = K(j, j) / K(j, jj)
                                For i = 0 To m
                                    K(i, jj) = K(i, jj) * temp - K(i, j)
                                    If stdNum.Abs(K(i, jj)) <= Erro1 Then
                                        K(i, jj) = 0
                                    End If
                                Next
                                For i = 0 To n
                                    Q(i, jj) = Q(i, jj) * temp - Q(i, j)
                                    If stdNum.Abs(Q(i, jj)) <= Erro1 Then
                                        Q(i, jj) = 0
                                    End If
                                Next
                            Else
                                K(j, jj) = 0
                            End If
                        End If
                    Next
                End If
            Next
            Dim E(n, m) As Double
            For i = 0 To m
                If K(i, i) <> 0 Then
                    E(i, i) = 1
                    For j = 0 To m
                        P(i, j) /= K(i, i)
                        If stdNum.Abs(P(i, j)) <= Erro1 Then
                            P(i, j) = 0
                        End If
                    Next
                Else
                    Exit For
                End If
            Next
            m += 1
            n += 1
            Mul(Q, E, n, K)
            Mul(K, P, m, Ret)
            Return n
        End Function

        ''' <summary>
        ''' 矩阵求秩，函数执行成功返回秩的大小
        ''' </summary>
        ''' <param name="K">要求秩的矩阵</param>
        ''' <param name="[error]">误差控制参数</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRank(K As GeneralMatrix, [error] As Int16) As Int16
            Dim n As Int16 = K.RowDimension
            Dim m As Int16 = K.Length \ n
            Dim i As Int16 = 0
            Dim i1 As Int16
            Dim j As Int16 = 0
            Dim j1 As Int16
            Dim temp1 As Double
            If m > n Then '保证m≤n
                i = m
                m = n
                n = i
                i = 1
            End If
            m -= 1
            n -= 1
            Dim temp(m, n) As Double
            If i = 0 Then
                For i = 0 To m
                    For j = 0 To n
                        temp(i, j) = K(i, j)
                    Next
                Next
            Else
                For i = 0 To m
                    For j = 0 To n
                        temp(i, j) = K(j, i)
                    Next
                Next
            End If
            If m = 0 Then
                i = 0
                While i <= n
                    If K(0, i) <> 0 Then
                        Return 1
                    End If
                    i += 1
                End While
                Return 0
            End If
            Dim error0 As Double
            If [error] = -1 Then
                error0 = System.Math.Pow(0.1, 10)
            Else
                error0 = System.Math.Pow(0.1, [error])
            End If
            i = 0
            While i <= m '保证误差可控制
                j = 0
                While j <= n
                    If temp(i, j) <> 0 Then
                        error0 *= temp(i, j)
                        i = m
                        Exit While
                    End If
                    j += 1
                End While
                i += 1
            End While
            Dim error1 As Double
            For i = 0 To m '消0过程
                j = 0
                While j <= n
                    If temp(i, j) <> 0 Then
                        Exit While
                    End If
                    j += 1
                End While
                If j <= n Then
                    i1 = 0
                    While i1 <= m
                        If temp(i1, j) <> 0 And i1 <> i Then
                            temp1 = temp(i, j) / temp(i1, j)
                            error1 = System.Math.Abs((temp(i, j) - temp(i1, j) * temp1)) * 100 '误差控制。因为有时候temp(i, j) - temp(i1, j) * (temp(i, j) / temp(i1, j))≠0
                            error1 += error0
                            For j1 = 0 To n
                                temp(i1, j1) = temp(i, j1) - temp(i1, j1) * temp1
                                If System.Math.Abs(temp(i1, j1)) < error1 Then
                                    temp(i1, j1) = 0
                                End If
                            Next
                        End If
                        i1 += 1
                    End While
                End If
            Next
            i1 = 0 '作为返回值的临时变量
            For i = 0 To m
                For j = 0 To n
                    If temp(i, j) <> 0 Then
                        i1 += 1
                        Exit For
                    End If
                Next
            Next
            Return i1
        End Function

        ''' <summary>
        ''' 方阵的QR分解
        ''' </summary>
        ''' <param name="K">要QR分解的矩阵，K必须是非奇异的n阶方阵</param>
        ''' <param name="Q">分解后的Q矩阵</param>
        ''' <param name="R">分解后的R矩阵</param>
        ''' <returns>函数执行成功返回True,失败返回False</returns>
        ''' <remarks></remarks>
        Public Function QR(K As GeneralMatrix, Q As GeneralMatrix, R As GeneralMatrix) As Boolean
            Dim n As Int16 = K.RowDimension
            If n * n <> K.Length Or Det2(K, n) = 0 Then 'K必须是非奇异的n阶方阵
                Return False
            End If
            n -= 1
            Dim B(n, n) As Double
            Dim Btemp(n) As Double
            Dim BE(n, n) As Double
            Dim temp1 As Double
            Dim temp2 As Double
            Dim i As Int16
            Dim j As Int16
            Dim i1 As Int16
            For i = 0 To n 'i代表的是列
                For i1 = 0 To n
                    Btemp(i1) = 0
                Next
                j = i - 1
                While j >= 0
                    temp1 = 0
                    For i1 = 0 To n
                        temp1 += K(i1, i) * B(i1, j)
                    Next
                    temp2 = 0
                    For i1 = 0 To n
                        temp2 += B(i1, j) * B(i1, j)
                    Next
                    BE(j, i) = temp1 / temp2
                    For i1 = 0 To n
                        Btemp(i1) += BE(j, i) * B(i1, j)
                    Next
                    j -= 1
                End While
                BE(i, i) = 1
                For i1 = 0 To n
                    B(i1, i) = K(i1, i) - Btemp(i1)
                Next
            Next
            For i = 0 To n 'i代表列
                Btemp(i) = 0
                For j = 0 To n
                    Btemp(i) += B(j, i) * B(j, i)
                Next
                Btemp(i) = stdNum.Pow(Btemp(i), 0.5)
            Next
            Q = New GeneralMatrix(n, n) '  ReDim Q(n, n)
            R = New GeneralMatrix(n, n) '   ReDim R(n, n)
            For i = 0 To n
                For j = 0 To n
                    Q(j, i) = B(j, i) / Btemp(i)
                Next
            Next
            For i = 0 To n
                For j = i To n
                    R(i, j) = Btemp(i) * BE(i, j)
                Next
            Next
            Return True
        End Function

        ''' <summary>
        ''' 矩阵施密特(Schmidt)正交规范化
        ''' </summary>
        ''' <param name="K">要施密特(Schmidt)正交规范化的矩阵</param>
        ''' <param name="Ret">正交规范化后的矩阵</param>
        ''' <returns>函数执行成功返回True,失败返回False</returns>
        ''' <remarks></remarks>
        Public Function Schmidt(K As GeneralMatrix, Ret As GeneralMatrix) As Boolean
            Dim n As Int16 = K.RowDimension
            If n * n <> K.Length Or Det2(K, n) = 0 Then 'K必须是非奇异的n阶方阵
                Return False
            End If
            n -= 1
            Dim B(n, n) As Double
            Dim Btemp(n) As Double
            Dim temp1 As Double
            Dim temp2 As Double
            Dim i As Int16
            Dim j As Int16
            Dim i1 As Int16
            For i = 0 To n 'i代表的是列
                For i1 = 0 To n
                    Btemp(i1) = 0
                Next
                j = i - 1
                While j >= 0
                    temp1 = 0
                    For i1 = 0 To n
                        temp1 += K(i1, i) * B(i1, j)
                    Next
                    temp2 = 0
                    For i1 = 0 To n
                        temp2 += B(i1, j) * B(i1, j)
                    Next
                    temp1 = temp1 / temp2
                    For i1 = 0 To n
                        Btemp(i1) += temp1 * B(i1, j)
                    Next
                    j -= 1
                End While
                For i1 = 0 To n
                    B(i1, i) = K(i1, i) - Btemp(i1)
                Next
            Next
            For i = 0 To n 'i代表列
                Btemp(i) = 0
                For j = 0 To n
                    Btemp(i) += B(j, i) * B(j, i)
                Next
                Btemp(i) = stdNum.Pow(Btemp(i), 0.5)
            Next
            Ret = New GeneralMatrix(n, n) '  ReDim Ret(n, n)
            For i = 0 To n
                For j = 0 To n
                    Ret(j, i) = B(j, i) / Btemp(i)
                Next
            Next
            Return True
        End Function

        ''' <summary>
        ''' 方阵求特征值
        ''' </summary>
        ''' <param name="K11">要求特征值的方阵</param>
        ''' <param name="n">方阵K1的阶数</param>
        ''' <param name="LoopNumber">循环次数</param>
        ''' <param name="errors">误差控制变量</param>
        ''' <param name="Ret">返回的特征值,Ret是是n*2的数组,第一列是实数部分,第2列为虚数部分</param>
        ''' <param name="IsHess">K1是否已经是上Hessenberg矩阵</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function EigenValue(K11 As GeneralMatrix, n As Int16, LoopNumber As Int16, errors As Int16, Ret As GeneralMatrix, IsHess As Boolean) As Boolean 'ret里是n*2的数组，第一列是实数部分，第2列为虚数部分
            Dim i As Int16 = K11.Length / n
            If n * n <> K11.Length Then '只有方阵才有特征值
                Return False
            End If
            Dim j As Int16
            Dim k As Int16
            Dim t As Int16
            Dim m As Int16
            Dim A As GeneralMatrix = GeneralMatrix.Number
            Ret = New GeneralMatrix(n - 1, 1) ' ReDim Ret(n - 1, 1) 'u v
            Dim erro As Double = stdNum.Pow(0.1, errors)
            Dim b As Double
            Dim c As Double
            Dim d As Double
            Dim g As Double
            Dim xy As Double
            Dim p As Double
            Dim q As Double
            Dim r As Double
            Dim x As Double
            Dim s As Double
            Dim e As Double
            Dim f As Double
            Dim z As Double
            Dim y As Double
            Dim loop1 As Int16 = LoopNumber
            Dim K1(n - 1, n - 1) As Double
            For m = 0 To n - 1
                For t = 0 To n - 1
                    K1(m, t) = K11(m, t)
                Next
            Next
            If IsHess Then
                i -= 1
                A.Resize(i, n - 1)
                For j = 0 To i
                    For k = 0 To n - 1
                        A(j, k) = K1(j, k)
                    Next
                Next
            Else
                Hessenberg(K1, n, A) '将方阵K1转化成上Hessenberg矩阵A
            End If
            m = n
            While m <> 0
                t = m - 1
                While t > 0
                    If stdNum.Abs(A(t, t - 1)) > erro * (stdNum.Abs(A(t - 1, t - 1)) + stdNum.Abs(A(t, t))) Then
                        t -= 1
                    Else
                        Exit While
                    End If
                End While
                If t = m - 1 Then
                    Ret(m - 1, 0) = A(m - 1, m - 1)
                    Ret(m - 1, 1) = 0
                    m -= 1
                    loop1 = LoopNumber
                ElseIf t = m - 2 Then
                    b = -(A(m - 1, m - 1) + A(m - 2, m - 2))
                    c = A(m - 1, m - 1) * A(m - 2, m - 2) - A(m - 1, m - 2) * A(m - 2, m - 1)
                    d = b * b - 4 * c
                    y = stdNum.Abs(d) ^ 0.5
                    If d > 0 Then
                        xy = 1
                        If b < 0 Then
                            xy = -1
                        End If
                        Ret(m - 1, 0) = -(b + xy * y) / 2
                        Ret(m - 1, 1) = 0
                        Ret(m - 2, 0) = c / Ret(m - 1, 0)
                        Ret(m - 2, 1) = 0
                    Else
                        Ret(m - 1, 0) = -b / 2
                        Ret(m - 2, 0) = Ret(m - 1, 0)
                        Ret(m - 1, 1) = y / 2
                        Ret(m - 2, 1) = -Ret(m - 1, 1)
                    End If
                    m -= 2
                    loop1 = LoopNumber
                Else
                    If loop1 < 1 Then
                        Return False
                    End If
                    loop1 -= 1
                    j = t + 2
                    While j < m
                        A(j, j - 2) = 0
                        j += 1
                    End While
                    j = t + 3
                    While j < m
                        A(j, j - 3) = 0
                        j += 1
                    End While
                    k = t
                    While k < m - 1
                        If k <> t Then
                            p = A(k, k - 1)
                            q = A(k + 1, k - 1)
                            If k <> m - 2 Then
                                r = A(k + 2, k - 1)
                            Else
                                r = 0
                            End If
                        Else
                            b = A(m - 1, m - 1)
                            c = A(m - 2, m - 2)
                            x = b + c
                            y = c * b - A(m - 2, m - 1) * A(m - 1, m - 2)
                            p = A(t, t) * (A(t, t) - x) + A(t, t + 1) * A(t + 1, t) + y
                            q = A(t + 1, t) * (A(t, t) + A(t + 1, t + 1) - x)
                            r = A(t + 1, t) * A(t + 2, t + 1)
                        End If
                        If p <> 0 Or q <> 0 Or r <> 0 Then
                            If p < 0 Then
                                xy = -1
                            Else
                                xy = 1
                            End If
                            s = xy * stdNum.Pow(p * p + q * q + r * r, 0.5)
                            If k <> t Then
                                A(k, k - 1) = -s
                            End If
                            e = -q / s
                            f = -r / s
                            x = -p / s
                            y = -x - f * r / (p + s)
                            g = e * r / (p + s)
                            z = -x - e * q / (p + s)
                            For j = k To m - 1
                                b = A(k, j)
                                c = A(k + 1, j)
                                p = x * b + e * c
                                q = e * b + y * c
                                r = f * b + g * c
                                If k <> m - 2 Then
                                    b = A(k + 2, j)
                                    p += f * b
                                    q += g * b
                                    r += z * b
                                    A(k + 2, j) = r
                                End If
                                A(k + 1, j) = q
                                A(k, j) = p
                            Next
                            j = k + 3
                            If j >= m - 1 Then
                                j = m - 1
                            End If
                            For i = t To j
                                b = A(i, k)
                                c = A(i, k + 1)
                                p = x * b + e * c
                                q = e * b + y * c
                                r = f * b + g * c
                                If k <> m - 2 Then
                                    b = A(i, k + 2)
                                    p += f * b
                                    q += g * b
                                    r += z * b
                                    A(i, k + 2) = r
                                End If
                                A(i, k + 1) = q
                                A(i, k) = p
                            Next
                        End If
                        k += 1
                    End While
                End If
            End While
            Return True
        End Function

        ''' <summary>
        ''' 将方阵化为上(Hessenberg)矩阵，函数成功返回Ret的阶数
        ''' </summary>
        ''' <param name="A">要化为上(Hessenberg)矩阵的矩阵</param>
        ''' <param name="n">为方阵A的阶数</param>
        ''' <param name="ret">化为上(Hessenberg)矩阵后的矩阵</param>
        ''' <returns>函数成功返回Ret的阶数</returns>
        ''' <remarks></remarks>
        Public Function Hessenberg(A As GeneralMatrix, n As Int16, ByRef ret As GeneralMatrix) As Int16 '
            Dim i As Int16
            Dim j As Int16
            Dim k As Int16
            Dim temp As Double
            Dim MaxNumber As Int16
            n -= 1
            ret = New GeneralMatrix(n, n) '   ReDim ret(n, n)
            For k = 1 To n - 1
                i = k - 1
                MaxNumber = k
                temp = stdNum.Abs(A(k, i))
                For j = k + 1 To n
                    If stdNum.Abs(A(j, i)) > temp Then
                        MaxNumber = j
                    End If
                Next
                ret(0, 0) = A(MaxNumber, i) '储存最大值
                i = MaxNumber
                If ret(0, 0) <> 0 Then
                    If i <> k Then
                        For j = k - 1 To n
                            temp = A(i, j)
                            A(i, j) = A(k, j)
                            A(k, j) = temp
                        Next
                        For j = 0 To n
                            temp = A(j, i)
                            A(j, i) = A(j, k)
                            A(j, k) = temp
                        Next
                    End If
                    For i = k + 1 To n
                        temp = A(i, k - 1) / ret(0, 0)
                        A(i, k - 1) = 0
                        For j = k To n
                            A(i, j) -= temp * A(k, j)
                        Next
                        For j = 0 To n
                            A(j, k) += temp * A(j, i)
                        Next
                    Next
                End If
            Next
            For i = 0 To n
                For j = 0 To n
                    ret(i, j) = A(i, j)
                Next
            Next
            Return n + 1
        End Function

        ''' <summary>
        ''' 对矩阵A进行奇异值分解
        ''' </summary>
        ''' <param name="A">目标矩阵</param>
        ''' <param name="m">A矩阵的行数</param>
        ''' <param name="V">分解得到的一个V矩阵</param>
        ''' <param name="V_m">V矩阵的行数</param>
        ''' <param name="S">分解得到的一个S矩阵</param>
        ''' <param name="S_m">S矩阵的行数</param>
        ''' <param name="U">分解得到的一个U矩阵</param>
        ''' <param name="U_m">U矩阵的行数</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SvdSplit(A As GeneralMatrix, m As Int16, V As GeneralMatrix, V_m As Int16, S As GeneralMatrix, S_m As Int16, U As GeneralMatrix, U_m As Int16) As Boolean
            'A=USV*
            Dim n As Int16 = A.Length / m
            Dim i As Int16
            Dim j As Int16
            Dim ii As Int16
            Dim At = GeneralMatrix.Number
            Dim AtA(0, 0) As Double
            Dim b(0, 0) As Double
            Dim b1(0, 0) As Double
            Dim temp As Double
            Dim Error1 As Double = stdNum.Pow(0.1, 10) '误差控制
            At = A.Transpose '   Math_Matrix_T(A, n, At)
            Mul(At, A, m, AtA)
            EigSym(AtA, n, 9, b, b1) 'b特征值，b1特征向量
            V_m = n
            S_m = m
            U_m = m
            m -= 1
            n -= 1
            For i = 0 To n - 1 '把特征值由从大到小进行排列
                For j = i + 1 To n
                    If b(i, 0) < b(j, 0) Then
                        temp = b(i, 0)
                        b(i, 0) = b(j, 0)
                        b(j, 0) = temp
                        For ii = 0 To n
                            temp = b1(ii, i)
                            b1(ii, i) = b1(ii, j)
                            b1(ii, j) = temp
                        Next
                    End If
                Next
            Next
            S = New GeneralMatrix(m, n) '    ReDim S(m, n)
            V = New GeneralMatrix(n, n)  '    ReDim V(n, n)
            U = New GeneralMatrix(m, m) '   ReDim U(m, m)
            ii = 0
            If m > n Then
                j = n
            Else
                j = m
            End If
            For i = 0 To j '给s赋值
                If b(i, 0) > Error1 Then
                    ii += 1
                    S(i, i) = stdNum.Sqrt(b(i, 0))
                Else
                    Exit For
                End If
            Next
            For j = 0 To n '给v赋值
                temp = 0
                For i = 0 To n
                    temp += b1(i, j) * b1(i, j)
                Next
                temp = stdNum.Sqrt(temp)
                For i = 0 To n
                    V(i, j) = b1(i, j) / temp
                Next
            Next
            j = 0
            At = New GeneralMatrix(n, 0)
            While j < ii
                For i = 0 To n
                    At(i, 0) = V(i, j)
                Next
                Mul(A, At, n + 1, b1)
                For i = 0 To m
                    If S(j, j) = 0 Then
                        U(i, j) = b1(i, 0) / Error1
                    Else
                        U(i, j) = b1(i, 0) / S(j, j)
                    End If
                Next
                j += 1
            End While
            While ii <= m
                At = New GeneralMatrix(ii - 1, m)
                ReDim b(ii - 1, 0)
                For i = 0 To ii - 1
                    For j = 0 To m
                        At(i, j) = U(j, i)
                    Next
                Next
                Cramer22(At, b, ii, A)
                temp = 0
                For i = 0 To m
                    temp += A(i, 0) * A(i, 0)
                Next
                temp = stdNum.Sqrt(temp)
                If temp = 0 Then
                    temp = Error1
                End If
                For i = 0 To m
                    U(i, ii) = A(i, 0) / temp
                Next
                ii += 1
            End While
            Return True
        End Function

        ''' <summary>
        ''' 求Kx=B的最小二乘解
        ''' </summary>
        ''' <param name="K">是x的系数矩阵</param>
        ''' <param name="B">是等式右边的常数矩阵</param>
        ''' <param name="k_m">矩阵K的行数</param>
        ''' <param name="x">求解得到的解</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Cramer22(K As GeneralMatrix, B As GeneralMatrix, k_m As Integer, x As GeneralMatrix) As Boolean 'Kx=B求解x。K不一定为方阵。其结果返回最小二乘解
            Dim i As Integer = B.RowDimension
            If i <> 1 Or B.Length <> k_m Then
                Return False
            End If
            Dim kt = GeneralMatrix.Number
            Dim kmul(0, 0) As Double
            Dim n As Integer = K.Length / k_m
            kt = K.Transpose '   Math_Matrix_T(K, n, kt)
            Mul(kt, K, k_m, kmul)
            Mul(kt, B, k_m, K)
            ' 相当于求解kmul*x=k
            If Inv2(kmul, kt, n) = False Then
                If Inv(kmul, kt) = False Then
                    Return False
                End If
            End If
            Mul(kt, K, n, x)
            Return True
        End Function

        ''' <summary>
        ''' 求行列式
        ''' </summary>
        ''' <param name="k">所求的n阶方阵</param>
        ''' <param name="N">方阵K的阶数</param>
        ''' <returns>函数成功返回其行列式的大小</returns>
        ''' <remarks></remarks>
        Public Function Det2(k As GeneralMatrix, N As Integer) As Double '求矩阵的行列式。K的数组大小为N*N的,不然程序出错.这个适合N比较大的方阵
            N -= 1
            Dim l(N, N) As Double
            Dim u(N, N) As Double
            Dim temp As Double = 0
            If LU(k, N + 1, l, u) Then
                temp = 1
                While N > -1
                    temp *= l(N, N)
                    N -= 1
                End While
            End If
            Return temp
        End Function

        ''' <summary>
        ''' 矩阵正规、对称、正定性判断
        ''' </summary>
        ''' <param name="K">为要判断的矩阵</param>
        ''' <returns>函数返回-1矩阵非对称矩阵,返回0矩阵不正定,返回1矩阵正定</returns>
        ''' <remarks></remarks>
        Public Function SPD(K As GeneralMatrix) As Int16 '返回-1 矩阵非对称矩阵，返回0矩阵不正定，返回1矩阵正定.主要是判断矩阵是否是正定矩阵
            Dim n As Int16 = K.RowDimension
            Dim m As Int16 = K.Length / n
            If m <> n Then
                Return -1
            End If
            Dim i As Int16
            Dim j As Int16
            m -= 1
            For i = 0 To m
                j = i + 1
                While j < n
                    If K(i, j) <> K(j, i) Then
                        Return -1
                    End If
                    j += 1
                End While
            Next
            Dim temp(0, 0) As Double
            Dim det As Double = 1
            m = 0
            While det > 0 And m < n
                ReDim temp(m, m)
                For i = 0 To m
                    For j = 0 To m
                        temp(i, j) = K(i, j)
                    Next
                Next
                m += 1
                det = Det2(temp, m)
            End While
            If det > 0 Then
                Return 1
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' 矩阵的LLt分解
        ''' </summary>
        ''' <param name="A">要进行LLt分解的方阵</param>
        ''' <param name="L">分解得到的L方阵</param>
        ''' <param name="is1_是否已经正定"></param>
        ''' <returns>函数成功返回True,失败返回False.(其中Lt是L的转置,即分解后 A=L×Lt)</returns>
        ''' <remarks></remarks>
        Public Function LLt(A As GeneralMatrix, L As GeneralMatrix, is1_是否已经正定 As Boolean) As Boolean '。其中A=L*(L的转置)
            '用平方根法分解A
            If is1_是否已经正定 = False Then
                If SPD(A) < 1 Then
                    Return False
                End If
            End If
            Dim sun As Double
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim n As Integer = A.RowDimension
            n -= 1
            L = New GeneralMatrix(n, n) '     ReDim L(n, n)
            L(0, 0) = stdNum.Sqrt(A(0, 0))
            For i = 1 To n
                j = 0
                While j < i
                    sun = 0
                    k = 0
                    While k < j
                        sun += L(i, k) * L(j, k)
                        k += 1
                    End While
                    sun = A(i, j) - sun
                    L(i, j) = sun / L(j, j)
                    j += 1
                End While
                sun = 0
                k = 0
                While k < i
                    sun += L(i, k) * L(i, k)
                    k += 1
                End While
                sun = A(i, i) - sun
                If sun <= 0 Then
                    Return False
                End If
                L(i, i) = stdNum.Sqrt(sun)
            Next
            Return True
        End Function

        ''' <summary>
        ''' 矩阵的QR分解
        ''' </summary>
        ''' <param name="A">要QR分解的矩阵（不一定是方阵）</param>
        ''' <param name="Q">分解得到的Q矩阵</param>
        ''' <param name="R">分解得到的R矩阵</param>
        ''' <param name="Q_n">返回Q矩阵的列数</param>
        ''' <param name="R_n">返回R矩阵的列数</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function QR22(A As GeneralMatrix, Q As GeneralMatrix, R As GeneralMatrix, Q_n As Int16, R_n As Int16) As Boolean '此函数不像Math_Matrix_QR2函数一样行数不小于列数,所以应该通用.
            Dim n As Int16 = A.RowDimension
            Dim m As Int16 = A.Length / n
            If m = 1 Or n = 1 Then
                Return False
            End If
            Q_n = m
            R_n = n
            m -= 1
            n -= 1
            Dim max As Double = n
            If m < n Then
                max = m
            End If
            Q = New GeneralMatrix(m, m) '   ReDim Q(m, m)
            R = New GeneralMatrix(m, n) '    ReDim R(m, n)
            Dim i As Int16
            Dim j As Int16
            Dim l As Int16
            Dim k As Int16
            Dim t As Double
            Dim maxerr As Double
            Dim a1 As Double
            Dim p As Double
            For i = 0 To m
                Q(i, i) = 1
            Next
            For k = 0 To max
                '误差控制
                maxerr = 0
                For i = k To m
                    t = stdNum.Abs(A(i, k))
                    If t > maxerr Then
                        maxerr = t
                    End If
                Next
                t = 0
                For i = k To m
                    p = A(i, k) / maxerr
                    t += p * p
                Next
                a1 = maxerr * stdNum.Pow(t, 0.5)
                If A(k, k) > 0 Then
                    a1 = 0 - a1
                End If
                t = 2 * a1 * (a1 - A(k, k))
                If t <= 0 Then
                    Return False
                End If
                p = stdNum.Pow(t, 0.5)
                If p > 0 Then
                    A(k, k) = (A(k, k) - a1) / p
                    i = k + 1
                    While i <= m
                        A(i, k) = A(i, k) / p
                        i += 1
                    End While
                    For j = 0 To m
                        t = 0
                        For l = k To m
                            t += A(l, k) * Q(l, j)
                        Next
                        For i = k To m
                            Q(i, j) -= 2 * A(i, k) * t
                        Next
                    Next
                    j = k + 1
                    While j <= n '******************
                        t = 0
                        For l = k To m
                            t += A(l, k) * A(l, j)
                        Next
                        For i = k To m
                            A(i, j) -= 2 * A(i, k) * t
                        Next
                        j += 1
                    End While
                    A(k, k) = a1
                    i = k + 1
                    While i <= m
                        A(i, k) = 0
                        i += 1
                    End While
                End If
            Next
            For i = 0 To m
                j = i + 1
                While j <= m
                    t = Q(i, j)
                    Q(i, j) = Q(j, i)
                    Q(j, i) = t
                    j += 1
                End While
            Next
            For i = 0 To m
                j = i
                While j <= n
                    R(i, j) = A(i, j)
                    j += 1
                End While
            Next
            Return True
        End Function

        ''' <summary>
        ''' 矩阵的QR分解
        ''' </summary>
        ''' <param name="A">要QR分解的矩阵(不一定是方阵)</param>
        ''' <param name="Q">分解得到的Q矩阵</param>
        ''' <param name="R">分解得到的R矩阵</param>
        ''' <param name="Q_n">返回Q矩阵的列数</param>
        ''' <param name="R_n">返回R矩阵的列数</param>
        ''' <returns>函数成功返回True,失败返回False.使用本函数时,A矩阵的行数不能小于列数</returns>
        ''' <remarks></remarks>
        Public Function QR2(A As GeneralMatrix, Q As GeneralMatrix, R As GeneralMatrix, Q_n As Int16, R_n As Int16) As Boolean '行数不小于列数
            Dim n As Int16 = A.RowDimension
            Dim m As Int16 = A.Length / n
            Dim max As Double = n - 1
            If m = 1 Or n = 1 Or m < n Then
                Return False
            End If
            If m = n Then
                max -= 1
            End If
            Q_n = m
            R_n = n
            m -= 1
            n -= 1
            Q = New GeneralMatrix(m, m) '        ReDim Q(m, m)
            R = New GeneralMatrix(m, n) '  ReDim R(m, n)
            Dim i As Int16
            Dim j As Int16
            Dim l As Int16
            Dim k As Int16
            Dim t As Double
            Dim maxerr As Double
            Dim a1 As Double
            Dim p As Double
            For i = 0 To m
                Q(i, i) = 1
            Next
            For k = 0 To max
                '误差控制
                maxerr = 0
                For i = k To m
                    t = stdNum.Abs(A(i, k))
                    If t > maxerr Then
                        maxerr = t
                    End If
                Next
                t = 0
                For i = k To m
                    p = A(i, k) / maxerr
                    t += p * p
                Next
                a1 = maxerr * stdNum.Pow(t, 0.5)
                If A(k, k) > 0 Then
                    a1 = 0 - a1
                End If
                t = 2 * a1 * (a1 - A(k, k))
                If t <= 0 Then
                    Return False
                End If
                p = stdNum.Pow(t, 0.5)
                If p > 0 Then
                    A(k, k) = (A(k, k) - a1) / p
                    i = k + 1
                    While i <= m
                        A(i, k) = A(i, k) / p
                        i += 1
                    End While
                    For j = 0 To m
                        t = 0
                        For l = k To m
                            t += A(l, k) * Q(l, j)
                        Next
                        For i = k To m
                            Q(i, j) -= 2 * A(i, k) * t
                        Next
                    Next
                    j = k + 1
                    While j <= n '******************
                        t = 0
                        For l = k To m
                            t += A(l, k) * A(l, j)
                        Next
                        For i = k To m
                            A(i, j) -= 2 * A(i, k) * t
                        Next
                        j += 1
                    End While
                    A(k, k) = a1
                    i = k + 1
                    While i <= m
                        A(i, k) = 0
                        i += 1
                    End While
                End If
            Next
            For i = 0 To m
                j = i + 1
                While j <= m
                    t = Q(i, j)
                    Q(i, j) = Q(j, i)
                    Q(j, i) = t
                    j += 1
                End While
            Next
            For i = 0 To m
                j = i
                While j <= n
                    R(i, j) = A(i, j)
                    j += 1
                End While
            Next
            Return True
        End Function

        ''' <summary>
        ''' 方阵LU分解
        ''' </summary>
        ''' <param name="K">为要LU分解的方阵</param>
        ''' <param name="n">方阵K的阶数</param>
        ''' <param name="L">为分解得到的L矩阵</param>
        ''' <param name="U">为分解得到的U矩阵</param>
        ''' <returns>其意义是K=LU.函数执行成功返回True,失败返回False</returns>
        ''' <remarks></remarks>
        Public Function LU(K As GeneralMatrix, n As Int16, L As GeneralMatrix, U As GeneralMatrix) As Boolean '方阵的LU分解
            If n * n <> K.Length Then
                Return False
            End If
            n -= 1
            L = New GeneralMatrix(n, n) '    ReDim L(n, n)
            U = New GeneralMatrix(n, n) '      ReDim U(n, n)
            Dim j As Int16
            Dim i As Int16
            Dim a As Int16
            Dim temp As Double = 0
            For i = 0 To n
                U(i, i) = 1
            Next
            For j = 0 To n
                For i = j To n
                    temp = 0
                    a = 0
                    While a < j
                        temp += L(i, a) * U(a, j)
                        a += 1
                    End While
                    L(i, j) = K(i, j) - temp
                Next
                i = j + 1
                While i <= n
                    temp = 0
                    a = 0
                    While a < j
                        temp += L(j, a) * U(a, i)
                        a += 1
                    End While
                    If L(j, j) = 0 Then
                        Return False
                    End If
                    U(j, i) = (K(j, i) - temp) / L(j, j)
                    i += 1
                End While
            Next
            Return True
        End Function

        ''' <summary>
        ''' 矩阵求逆
        ''' </summary>
        ''' <param name="K">目标方阵</param>
        ''' <param name="Return_K">求得的逆矩阵</param>
        ''' <param name="N">方阵K的阶数</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inv2(K As GeneralMatrix, Return_K As GeneralMatrix, N As Integer) As Boolean
            Dim i As Integer = K.Length \ N
            If i * N <> K.Length Then
                Return False
            End If
            N -= 1
            Return_K = New GeneralMatrix(N, N) '!!!!! Redim Return_K(N,N)
            If i = 1 Then
                If K(0, 0) = 0 Then
                    Return False
                Else
                    Return_K(0, 0) = 1 / K(0, 0)
                End If
            Else
                'lu分解法求逆
                Dim l(N, N) As Double
                Dim u(N, N) As Double
                If (LU(K, N + 1, l, u)) Then
                    Dim d(N) As Double
                    Dim x(N) As Double
                    Dim e(N) As Double
                    Dim j As Integer = 0
                    Dim a As Integer = 0
                    Dim temp As Double = 0
                    For i = 0 To N
                        ReDim e(N)
                        e(i) = 1
                        j = 0
                        While j <= N
                            temp = 0
                            a = 0
                            While a < j
                                temp += d(a) * l(j, a)
                                a += 1
                            End While
                            d(j) = e(j) - temp
                            d(j) /= l(j, j)
                            j += 1
                        End While
                        j = N
                        While j > -1
                            temp = 0
                            a = j + 1
                            While a <= N
                                temp += u(j, a) * x(a)
                                a += 1
                            End While
                            x(j) = d(j) - temp
                            x(j) /= u(j, j)
                            j -= 1
                        End While
                        For j = 0 To N
                            Return_K(j, i) = x(j)
                        Next
                    Next
                Else
                    Return False
                End If
            End If
            Return True
        End Function

        ''' <summary>
        ''' 求行列式，函数执行成功返回其行列式大小.其原理是按行列式定义依次展开求解.不适合大于5阶的方阵，K的数组大小为N*N的,不然程序出错
        ''' </summary>
        ''' <param name="k">为n阶方阵</param>
        ''' <param name="N">为矩阵A的阶数</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DetF(k As GeneralMatrix, N As Integer) As Double '
            Dim i As Integer
            If N = 1 Then
                Return k(0, 0)
            End If
            Dim j As Integer = 0
            Dim m As Integer = 0
            Dim l As Integer = 0
            Dim t(N - 2, N - 2) As Double
            Dim temp As Double = 0
            Dim is1 As Boolean = True
            For i = 0 To N - 1
                If k(0, i) <> 0 Then
                    For m = 0 To N - 2
                        l = 0
                        For j = 0 To N - 1
                            If j <> i Then
                                t(m, l) = k(m + 1, j)
                                l += 1
                            End If
                        Next
                    Next
                    If is1 Then
                        temp += k(0, i) * DetF(t, N - 1)
                    Else
                        temp -= k(0, i) * DetF(t, N - 1)
                    End If
                End If
                is1 = Not is1
            Next
            Return temp
        End Function

        ''' <summary>
        ''' 矩阵求逆
        ''' </summary>
        ''' <param name="K">为要求逆的方阵</param>
        ''' <param name="Return_K">为所求得的逆</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inv(K As GeneralMatrix, Return_K As GeneralMatrix) As Boolean '求矩阵K的逆.成功返回True与其逆矩阵Return_K
            Dim i As Integer = K.Length
            Dim N As Integer = System.Math.Pow(i, 0.5)
            If i <> N * N Or N = 1 Then '必须是N阶方阵
                Return False
            End If
            Dim Det As Double = DetF(K, N)
            If Det = 0 Then
                Return False
            End If
            N -= 1
            Return_K = New GeneralMatrix(N, N) '  ReDim Return_K(N, N)
            Dim Temp((N - 1), (N - 1)) As Double
            Dim i_temp As Integer = 0
            Dim j_temp As Integer = 0
            Dim i1 As Integer = 0
            Dim j1 As Integer = 0
            Dim j As Integer = 0
            Dim is1 As Boolean = True
            Dim is2 As Boolean = True
            For i = 0 To N
                is2 = is1
                For j = 0 To N
                    i_temp = 0
                    For i1 = 0 To N
                        If i1 <> i Then
                            j_temp = 0
                            For j1 = 0 To N
                                If j1 <> j Then
                                    Temp(i_temp, j_temp) = K(i1, j1)
                                    j_temp += 1
                                End If
                            Next
                            i_temp += 1
                        End If
                    Next
                    Return_K(j, i) = DetF(Temp, N) / Det '编程时,最好返回Return_K/Det这种格式
                    If is2 = False Then
                        Return_K(j, i) = -Return_K(j, i)
                    End If
                    is2 = Not is2
                Next
                is1 = Not is1
            Next
            Return True
        End Function

        ''' <summary>
        ''' 矩阵奇异值
        ''' </summary>
        ''' <param name="A">为目的矩阵</param>
        ''' <param name="m">为A矩阵的行数</param>
        ''' <param name="Ret">获取到的奇异值矩阵,即返回的Ret是m*1的矩阵</param>
        ''' <returns>函数执行成功返回奇异值的个数,即Ret的行数,失败返回-1</returns>
        ''' <remarks></remarks>
        Public Function Svd(A As GeneralMatrix, m As Int16, Ret As GeneralMatrix) As Int16 '返回矩阵A的奇异值Ret。本函数出错返回-1。成功返回奇异值的个数，即m*1矩阵的Ret的m
            Dim n As Int16 = A.Length / m
            If n * m <> A.Length Then
                Return -1
            End If
            Dim At = GeneralMatrix.Number
            At = A.Transpose '    Call Math_Matrix_T(A, n, At) '???原文If Math_Matrix_T(A, n, At) Then
            Dim AtA(0, 0) As Double
            If n > m Then
                Mul(A, At, n, AtA)
                n = m
            Else
                Mul(At, A, m, AtA)
            End If
            If (EigSym(AtA, n, 7, At, Nothing)) Then
                Dim i As Int16
                m = -1
                n -= 1
                For i = 0 To n
                    If At(i, 0) > 0 Then
                        m += 1
                    End If
                Next
                If m <> -1 Then
                    Ret = New GeneralMatrix(m, 0) '       ReDim Ret(m, 0)
                    m = 0
                    For i = 0 To n
                        If At(i, 0) > 0 Then
                            Ret(m, 0) = stdNum.Sqrt(At(i, 0))
                            m += 1
                        End If
                    Next
                Else
                    m = 0
                End If
                'End If
                Return m
            End If

            Return -1
        End Function

        ''' <summary>
        ''' 实对称阵化为对称三对角阵
        ''' </summary>
        ''' <param name="A">目标方阵</param>
        ''' <param name="n">方阵A的阶数</param>
        ''' <param name="Is对称">不确定是否对称直接填False,对称则直接填True</param>
        ''' <param name="ret">返回的三对角阵</param>
        ''' <returns></returns>
        ''' <remarks>本函数采用用豪斯赫尔蒙德变换将实对称阵化为对称三对角</remarks>
        Public Function SymTridMatrix(A As GeneralMatrix, n As Int16, Is对称 As Boolean, ret As GeneralMatrix) As Boolean '用豪斯赫尔蒙德变换将实对称阵化为对称三对角阵，翻译徐士良老师的解法
            Dim m As Int16 = A.Length / n
            If m <> n Or m * n <> A.Length Then
                Return False
            End If
            Dim i As Int16
            Dim j As Int16
            If Is对称 = False Then
                i = 0
                While i < n
                    j = i + 1
                    While j < n
                        If A(i, j) <> A(j, i) Then
                            Return False
                        End If
                        j += 1
                    End While
                    i += 1
                End While
            End If
            Dim k As Int16
            Dim s As Double
            Dim d As Double
            n -= 1
            Dim w(n) As Double
            Dim b(n, n) As Double
            Dim c(n, n) As Double
            ret = New GeneralMatrix(n, n) '    ReDim ret(n, n)
            k = 0
            While k < n - 1
                s = 0
                For i = k + 1 To n
                    d = A(k, i)
                    s += d * d
                Next
                s = stdNum.Pow(s, 0.5)
                If s <> 0 Then
                    For i = 0 To n
                        w(i) = 0
                    Next
                    w(k + 1) = stdNum.Pow((1 + stdNum.Abs(A(k, k + 1) / s)) / 2, 0.5)
                    i = k + 2
                    While i <= n
                        w(i) = A(k, i) / (2 * s * w(k + 1))
                        If A(k, k + 1) < 0 Then
                            w(i) = 0 - w(i)
                        End If
                        i += 1
                    End While
                    For i = 0 To n
                        For j = 0 To n
                            If i = j Then
                                b(i, j) = 1 - 2 * w(i) * w(j)
                            Else
                                b(i, j) = -2 * w(i) * w(j)
                            End If
                        Next
                    Next
                    For i = 0 To n
                        For j = 0 To n
                            c(i, j) = 0
                            For m = 0 To n
                                c(i, j) += A(i, m) * b(m, j)
                            Next
                        Next
                    Next
                    For i = 0 To n
                        For j = 0 To n
                            A(i, j) = 0
                            For m = 0 To n
                                A(i, j) += b(i, m) * c(m, j)
                            Next
                        Next
                    Next
                End If
                k += 1
            End While
            For i = 0 To n
                For j = 0 To n
                    ret(i, j) = A(i, j)
                Next
            Next
            Return True
        End Function

        ''' <summary>
        ''' 矩阵伴随矩阵
        ''' </summary>
        ''' <param name="K">目标方阵</param>
        ''' <param name="n">方阵K的阶数</param>
        ''' <param name="Ret">获得的伴随矩阵</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 函数采用求代数余子式的方式进行求解,这样就存在一个问题,当目标矩阵的阶数很大的时候,本函数效率是相当慢的。
        ''' 建议使用左连翠提出的《伴随矩阵的新求法》里的方法进行求解。里面的方法可以求解非满秩矩阵的伴随矩阵。
        ''' </remarks>
        Public Function Adj(K As GeneralMatrix, n As Int16, Ret As GeneralMatrix) As Boolean '求方阵K的伴随矩阵
            If n < 1 Or n * n <> K.Length Then
                Return False
            End If
            Dim i As Int16
            Dim j As Int16
            Dim i1 As Int16
            Dim j1 As Int16
            Dim i2 As Int16
            Dim j2 As Int16
            Dim is1 As Boolean = True
            Dim is2 As Boolean
            n -= 1
            Ret = New GeneralMatrix(n, n) '   ReDim Ret(n, n)
            Dim det As Double
            Dim temp(n - 1, n - 1) As Double
            For i = 0 To n
                is2 = is1
                For j = 0 To n
                    i2 = 0
                    For i1 = 0 To n
                        If i1 <> i Then
                            j2 = 0
                            For j1 = 0 To n
                                If j1 <> j Then
                                    temp(i2, j2) = K(i1, j1)
                                    j2 += 1
                                End If
                            Next
                            i2 += 1
                        End If
                    Next
                    det = Det2(temp, n)
                    If is2 = False Then
                        det = -det
                    End If
                    Ret(j, i) = det
                    is2 = Not is2
                Next
                is1 = Not is1
            Next
            Return True
        End Function

        ''' <summary>
        ''' 方阵求n次方
        ''' </summary>
        ''' <param name="A">目标方阵</param>
        ''' <param name="m">方阵A的阶数</param>
        ''' <param name="n">方阵A要求的次方数</param>
        ''' <param name="Ret">方阵A进行n次方后获得的返回值</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 注意,本代码没有采用特征值法。而是直接采用2个矩阵相乘的方法(但又不是老老实实地去乘n次),因为用程序去求一个方阵的特征值,
        ''' 可能运算复杂度超过了你直接对矩阵相乘的复杂度,至少在n在1000以内大概是这样。
        ''' </remarks>
        Public Function Pow(A As GeneralMatrix, m As Integer, n As Integer, Ret As GeneralMatrix) As Boolean '求m*m方阵的n次方。注意,没有采用特征值法。为原始的2个矩阵相乘
            m -= 1
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim temp(m, m) As Double
            Ret = New GeneralMatrix(m, m) '  ReDim Ret(m, m)
            If n < 9 Then
                For i = 0 To m
                    For j = 0 To m
                        Ret(i, j) = A(i, j)
                    Next
                Next
                While n > 1
                    n -= 1
                    For i = 0 To m
                        For j = 0 To m
                            temp(i, j) = Ret(i, j)
                        Next
                    Next
                    For i = 0 To m
                        For j = 0 To m
                            Ret(i, j) = 0
                            For k = 0 To m
                                Ret(i, j) += temp(i, k) * A(k, j)
                            Next
                        Next
                    Next
                End While
            Else
                For i = 0 To m
                    For j = 0 To m
                        Ret(i, j) = 0
                        For k = 0 To m
                            temp(i, j) += A(i, k) * A(k, j)
                        Next
                    Next
                Next
                Pow(temp, m + 1, n \ 2, Ret)
                If n Mod 2 = 1 Then
                    For i = 0 To m
                        For j = 0 To m
                            temp(i, j) = Ret(i, j)
                        Next
                    Next
                    For i = 0 To m
                        For j = 0 To m
                            Ret(i, j) = 0
                            For k = 0 To m
                                Ret(i, j) += temp(i, k) * A(k, j)
                            Next
                        Next
                    Next
                End If
            End If
            Return True
        End Function

        ''' <summary>
        ''' 求多项式复数根贝尔斯托(Bairstow)算法
        ''' </summary>
        ''' <param name="A">多项式系数矩阵,为1*A_n的矩阵。A中的数据依次为多项式最高项系数,次高项系数……常数项系数</param>
        ''' <param name="A_n">A矩阵的列数或大小</param>
        ''' <param name="LoopNumber">控制的循环次数</param>
        ''' <param name="Erro">误差控制变量</param>
        ''' <param name="Ret">返回的一个n*2的矩阵</param>
        ''' <returns>函数执行完毕返回Ret的行数</returns>
        ''' <remarks>
        ''' 对于多项式f(x)=(x^2+2x+3)(x^2-5x+9)=x^4-3x^3+2x^2+3x+27,则A(0,0)=1,A(0,1)=-3,A(0,2)=2,A(0,3)=3,A(0,4)=27,A_n=5.
        ''' 当执行下面的函数后,Ret是一个2×2的矩阵,即Ret(0,0)=2,Ret(0,1)=3,Ret(0,0)的2对应于(x^2+2x+3)当中2x的2,Ret(0,1)的3
        ''' 对应于(x^2+2x+3)当中常系数的3.用此函数前建议先把重根与实数根处理掉
        ''' </remarks>
        Public Function PolyRoots2(A As GeneralMatrix, A_n As Integer, LoopNumber As Int16, Erro As Integer, Ret As GeneralMatrix) As Integer '失败返回0。成功返回Ret行数
            '本函数求解的根是复数根,且多项式A只有复数根,且不存在重根
            '如果A=(x^2+23x+4)则Ret（0,0）=23,Ret(0,1)=4
            '函数返回Ret的行数
            A_n -= 1
            Dim b(A_n) As Double
            Dim c(A_n) As Double
            Dim u As Double
            Dim v As Double
            Dim LoopNumber1 As Int16
            Dim N As Integer = (A_n + 1) \ 2 - 1
            Dim i As Integer
            Dim J As Double
            Dim ATemp(0, 0) As Double
            Dim ATemp2(0, 2) As Double
            Dim Erro1 As Double = stdNum.Pow(0.1, Erro)
            Ret = New GeneralMatrix(N, 1) '    ReDim Ret(N, 1)
            While N >= 0
                u = 1
                v = 1
                LoopNumber1 = LoopNumber
                b(0) = A(0, 0)
                c(0) = 0
                c(1) = A(0, 0)
                While LoopNumber1 > 0
                    LoopNumber1 -= 1
                    b(1) = A(0, 1) + u * b(0)
                    i = 2
                    While i <= A_n
                        b(i) = A(0, i) + u * b(i - 1) + v * b(i - 2)
                        c(i) = b(i - 1) + u * c(i - 1) + v * c(i - 2)
                        i += 1
                    End While
                    J = c(A_n - 2) * c(A_n) - c(A_n - 1) * c(A_n - 1)
                    u = u + (c(A_n - 1) * b(A_n - 1) - c(A_n - 2) * b(A_n)) / J
                    v = v + (c(A_n - 1) * b(A_n) - c(A_n) * b(A_n - 1)) / J
                    If stdNum.Abs(b(A_n)) <= Erro1 And stdNum.Abs(b(A_n - 1)) <= Erro1 Then
                        Exit While
                    End If
                End While
                Ret(N, 0) = -u
                Ret(N, 1) = -v
                ATemp2(0, 0) = 1
                ATemp2(0, 1) = Ret(N, 0)
                ATemp2(0, 2) = Ret(N, 1)
                PolyDiv(A, ATemp2, Nothing, ATemp, 11)
                A_n = ATemp.Length - 1
                i = A_n
                A = New GeneralMatrix(0, i) '     ReDim A(0, i)
                While i >= 0
                    A(0, i) = ATemp(0, i)
                    i -= 1
                End While
                N -= 1
            End While
            Return Ret.Length / 2
        End Function

        ''' <summary>
        ''' 矩阵范数Cond及
        ''' </summary>
        ''' <param name="k">目标矩阵</param>
        ''' <param name="m">矩阵的行数</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 函数运行原理是先求矩阵的奇异值,然后用最大的奇异值除以最小的奇异值即得矩阵的范数.对于只有1行或者1列的还得另行处理.这个函数和Matlab的Cond命令一样,即2范数
        ''' </remarks>
        Public Function Cond(k As GeneralMatrix, m As Integer) As Double
            '返回矩阵的2范数
            Dim s(0, 0) As Double
            Dim i As Integer
            Dim n As Integer
            Dim max As Double
            Dim min As Double
            n = Svd(k, m, s)
            If n < 2 Then
                Return 0
            End If
            n -= 1
            max = s(0, 0)
            min = max
            For i = 1 To n
                If s(i, 0) > max Then
                    max = s(i, 0)
                ElseIf s(i, 0) < min Then
                    min = s(i, 0)
                End If
            Next
            If min = 0 Then
                Return 0
            End If
            max /= min
            Return max
        End Function

        ''' <summary>
        ''' 构建散点图矩阵(Scatter GeneralMatrix)
        ''' </summary>
        ''' <param name="X">目标矩阵</param>
        ''' <param name="m">X矩阵的行数</param>
        ''' <param name="S">获得的散点矩阵</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Scatter(X As GeneralMatrix, m As Integer, S As GeneralMatrix) As Integer
            '返回X矩阵的散点矩阵(Scatter GeneralMatrix)S
            Dim n As Integer = X.Length \ m - 1
            m -= 1
            Dim i As Integer
            Dim j As Integer
            Dim i1 As Integer
            Dim j1 As Integer
            Dim temp(0, 0) As Double
            Dim tempx(m, 0) As Double
            Dim tempxt(0, m) As Double
            Dim x1 As Double
            For i = 0 To m
                x1 = 0
                For j = 0 To n
                    x1 += X(i, j)
                Next
                x1 /= (n + 1)
                For j = 0 To n
                    X(i, j) -= x1
                Next
            Next
            S = New GeneralMatrix(m, m) '    ReDim S(m, m)
            For j = 0 To n
                For i = 0 To m
                    tempx(i, 0) = X(i, j)
                    tempxt(0, i) = X(i, j)
                Next
                Mul(tempx, tempxt, 1, temp)
                For i1 = 0 To m
                    For j1 = 0 To m
                        S(i1, j1) += temp(i1, j1)
                    Next
                Next
            Next
            Return m + 1
        End Function

        ''' <summary>
        ''' 多项式除法
        ''' </summary>
        ''' <param name="A1">被除数存储多项式系数</param>
        ''' <param name="A2">除数存储多项式系数</param>
        ''' <param name="RetMod">求得的余数多项式系数</param>
        ''' <param name="Ret">求得的多项式商系数</param>
        ''' <param name="Erro">误差控制参数</param>
        ''' <returns></returns>
        ''' <remarks>A1/A2=Ret……RetMod</remarks>
        Public Function PolyDivEx(A1 As GeneralMatrix, A2 As GeneralMatrix, RetMod As GeneralMatrix, Ret As GeneralMatrix, Erro As Integer) As Integer '多项式的除法，里边的数组均为1*n的矩阵,原理:A1/A2=Ret……RetMod'函数最终返回Ret商的数组大小
            Dim n1 As Integer = A1.Length
            Dim n2 As Integer = A2.Length
            Dim N As Integer
            Dim i As Integer
            If n1 < n2 Then
                Ret = GeneralMatrix.Number '     ReDim Ret(0, 0)
                n1 -= 1
                RetMod = New GeneralMatrix(0, n1) '   ReDim RetMod(0, n1)
                For i = 0 To n1
                    RetMod(0, i) = A1(0, i)
                Next
                Return 1
            End If
            Dim error1 As Double = stdNum.Abs(A2(0, 0)) * stdNum.Pow(0.1, Erro)
            Dim j As Integer
            N = n1 - n2
            Ret = New GeneralMatrix(0, N) '  ReDim Ret(0, N)
            N = 0
            While n1 >= n2
                Ret(0, N) = A1(0, 0) / A2(0, 0)
                i = 1
                While i < n2
                    A1(0, i) -= A2(0, i) * Ret(0, N)
                    If stdNum.Abs(A1(0, i)) <= error1 Then
                        A1(0, i) = 0
                    End If
                    i += 1
                End While
                i = 1
                While i < n1
                    N += 1
                    If A1(0, i) <> 0 Then
                        Exit While
                    End If
                    i += 1
                End While
                If i < n1 Then
                    j = i
                    While j < n1
                        A1(0, j - i) = A1(0, j)
                        j += 1
                    End While
                    n1 -= i
                    j = n1 - 1
                    Call A1.Resize(0, j) '    ReDim Preserve A1(0, j)
                Else
                    n1 = 0
                    RetMod = New GeneralMatrix(0, 0) '    ReDim RetMod(0, 0)
                End If
            End While
            If n1 > 0 Then
                n1 -= 1
                RetMod = New GeneralMatrix(0, n1) '   ReDim RetMod(0, n1)
                For i = 0 To n1
                    RetMod(0, i) = A1(0, i)
                Next
            End If
            Return Ret.Length
        End Function

        ''' <summary>
        ''' 矩阵特征值获取特征值向量
        ''' </summary>
        ''' <param name="A1">目标方阵</param>
        ''' <param name="A_m">矩阵A的行数</param>
        ''' <param name="EigValve">方阵A的一个特征值</param>
        ''' <param name="X">函数执行成功后得到的一个特征向量</param>
        ''' <remarks>
        ''' 函数原理:已知方阵A的一个特征值为r,则求解方程组(A-r*E)*X=0的解X即为我们的一个特征向量(这里E为单位矩阵),
        ''' 我们下面采用的是全选主元素法求解.但是需要注意的是,由于这个方程组是非满秩矩阵,因此在最后处理解的时候,我们
        ''' 总是令X解中的一个量为1(当然,你可以设置为其它数,建议设置为非0的数据),然后根据这个量导出其它的量
        ''' 
        ''' 例子:
        ''' ```
        ''' a =
        '''  [ -1.0000000000000   0.00000000000000   0.00000000000000
        '''    8.00000000000000   2.00000000000000   4.00000000000000
        '''    8.00000000000000   3.00000000000000   3.00000000000000 ]
        '''
        '''  Math_Matrix_EigTor(a,3,6,x)'上面矩阵a的一个特征值为6,则我们执行如下的命令后求得6的特征向量x如下
        ''' x =
        '''  [ 0.00000000000000
        '''    1.00000000000000
        '''    1.00000000000000 ]
        ''' ```
        ''' </remarks>
        Public Sub EigTorF(A1 As GeneralMatrix, A_m As Integer, EigValve As Double, X As GeneralMatrix)
            '采用全选主元素法求解
            Dim n As Integer = A_m - 1
            A_m = n
            Dim i As Integer
            Dim Indez_i As Integer
            Dim Index_j As Integer
            Dim temp_i As Integer
            Dim temp_j As Integer
            Dim max As Double
            Dim temp, A(n, n) As Double
            Dim Index(n), b(n, 0) As Integer
            X = New GeneralMatrix(n, 0) '  ReDim X(n, 0)
            For i = 0 To n
                For temp_i = 0 To n
                    A(i, temp_i) = A1(i, temp_i)
                Next
            Next
            For i = 0 To n
                Index(i) = i
                A(i, i) -= EigValve
            Next
            For i = 0 To A_m
                max = 0
                For temp_i = i To A_m
                    For temp_j = i To n
                        temp = stdNum.Abs(A(temp_i, temp_j))
                        If temp > max Then
                            max = temp
                            Index_j = temp_j
                            Indez_i = temp_i
                        End If
                    Next
                Next
                If max > 0 Then
                    If Indez_i > i Then
                        For temp_j = i To n
                            temp = A(i, temp_j)
                            A(i, temp_j) = A(Indez_i, temp_j)
                            A(Indez_i, temp_j) = temp
                        Next
                    End If
                    If Index_j > i Then
                        temp_i = Index(i)
                        Index(i) = Index(Index_j)
                        Index(Index_j) = temp_i
                        For temp_i = 0 To A_m
                            temp = A(temp_i, i)
                            A(temp_i, i) = A(temp_i, Index_j)
                            A(temp_i, Index_j) = temp
                        Next
                    End If
                    temp_i = i
                    While temp_i < A_m
                        temp_i += 1
                        If A(temp_i, i) <> 0 Then
                            temp = A(i, i) / A(temp_i, i)
                            temp_j = i
                            While temp_j < n
                                temp_j += 1
                                A(temp_i, temp_j) = A(temp_i, temp_j) * temp - A(i, temp_j)
                            End While
                        End If
                    End While
                Else
                    Exit For
                End If
            Next
            If n > A_m Then
A:              For temp_i = A_m + 1 To n
                    X(Index(temp_i), 0) = 1
                Next
                For temp_i = 0 To A_m
                    For temp_j = A_m + 1 To n
                        b(temp_i, 0) -= A(temp_i, temp_j)
                    Next
                Next
            Else
                A_m = n - 1
                GoTo A
            End If
            For temp_i = A_m To 0 Step -1
                If A(temp_i, temp_i) = 0 Then
                    If b(temp_i, 0) <> 0 Then
                        Return
                    End If
                    X(Index(temp_i), 0) = 1
                Else
                    X(Index(temp_i), 0) = b(temp_i, 0) / A(temp_i, temp_i)
                End If
                temp_j = temp_i
                While temp_j > 0
                    temp_j -= 1
                    b(temp_j, 0) -= A(temp_j, temp_i) * X(Index(temp_i), 0)
                End While
            Next
        End Sub

        ''' <summary>
        ''' 求对称方阵特征值
        ''' </summary>
        ''' <param name="A">对称方阵</param>
        ''' <param name="n">方阵A的阶数</param>
        ''' <param name="Erro1">误差控制变量</param>
        ''' <param name="Ret">返回的特征值</param>
        ''' <param name="Ret_Eigenvectors">返回的特征值对应的特征向量</param>
        ''' <returns></returns>
        ''' <remarks>本代码采用雅可比过关法求解</remarks>
        Public Function EigSym(A As GeneralMatrix, n As Int16, Erro1 As Int16, Ret As GeneralMatrix, Ret_Eigenvectors As GeneralMatrix) As Boolean '返回n阶对称矩阵K的特征值ret.翻译徐士良老师的算法。其方法是雅可比过关法。特征向量Ret_Eigenvectors每一列对应ret每一行的特征值
            Dim i As Int16 = A.Length / n
            If i * i <> A.Length Then
                Return False
            End If
            n -= 1
            Dim p As Int16
            Dim j As Int16
            For i = 0 To n
                j = i + 1
                While j <= n
                    If A(j, i) <> A(i, j) Then
                        Return False
                    End If
                    j += 1
                End While
            Next
            Dim q As Int16
            Dim ff As Double
            Dim fm As Double
            Dim cn As Double
            Dim sn As Double
            Dim comega As Double
            Dim x As Double
            Dim y As Double
            Dim d As Double
            Dim ero As Double = stdNum.Pow(0.1, Erro1)
            Ret_Eigenvectors = New GeneralMatrix(n, n) '  ReDim Ret_Eigenvectors(n, n)
            For i = 0 To n
                Ret_Eigenvectors(i, i) = 1
            Next
            ff = 0
            For i = 1 To n
                For j = 0 To i - 1
                    d = A(i, j)
                    ff += d * d
                Next
            Next
            ff = stdNum.Pow(2 * ff, 0.5)
Loop0:
            ff /= (n + 1)
Loop1:
            For i = 1 To n
                For j = 0 To i - 1
                    d = stdNum.Abs(A(i, j))
                    If d > ff Then
                        p = i
                        q = j
                        GoTo Loop00
                    End If
                Next
            Next
            If ff < ero Then
                GoTo Loopexit
            Else
                GoTo Loop0
            End If
Loop00:
            x = -A(p, q)
            y = (A(q, q) - A(p, p)) / 2
            comega = x / stdNum.Pow(x * x + y * y, 0.5)
            If y < 0 Then
                comega = -comega
            End If
            sn = 1 + stdNum.Pow(1 - comega * comega, 0.5)
            sn = comega / stdNum.Pow(2 * sn, 0.5)
            cn = stdNum.Pow(1 - sn * sn, 0.5)
            fm = A(p, p)
            A(p, p) = fm * cn * cn + A(q, q) * sn * sn + A(p, q) * comega
            A(q, q) = fm * sn * sn + A(q, q) * cn * cn - A(p, q) * comega
            A(p, q) = 0
            A(q, p) = 0
            For j = 0 To n
                If j <> p And j <> q Then
                    fm = A(p, j)
                    A(p, j) = fm * cn + A(q, j) * sn
                    A(q, j) = A(q, j) * cn - fm * sn
                End If
            Next
            For i = 0 To n
                If i <> p And i <> q Then
                    fm = A(i, p)
                    A(i, p) = fm * cn + A(i, q) * sn
                    A(i, q) = A(i, q) * cn - fm * sn
                End If
            Next
            For i = 0 To n
                fm = Ret_Eigenvectors(i, p)
                Ret_Eigenvectors(i, p) = fm * cn + Ret_Eigenvectors(i, q) * sn
                Ret_Eigenvectors(i, q) = Ret_Eigenvectors(i, q) * cn - fm * sn
            Next
            GoTo Loop1
Loopexit:
            Ret = New GeneralMatrix(n, 0) '   ReDim Ret(n, 0)
            For i = 0 To n
                Ret(i, 0) = A(i, i)
            Next
            Return True
        End Function

        ''' <summary>
        ''' 矩阵相乘
        ''' </summary>
        ''' <param name="K1">K1为矩阵乘法中左边的矩阵</param>
        ''' <param name="K2">为矩阵乘法中右边的矩阵</param>
        ''' <param name="n">代表K1的列数,K2的行数</param>
        ''' <param name="Return_K">执行成功后返回的乘的结果的矩阵</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Mul(K1 As GeneralMatrix, K2 As GeneralMatrix, n As Integer, Return_K As GeneralMatrix) As Boolean '矩阵相乘K1为m*n,K2为n*l,Return_K为m*l
            Dim i As Integer = K1.RowDimension
            If i <> n Then
                Return False
            End If
            i = K2.RowDimension
            If i * n <> K2.Length Then
                Return False
            End If
            Dim a As Integer = K1.Length \ n - 1
            Dim b As Integer = i - 1
            Dim j As Integer = 0
            Dim k As Integer
            Return_K = New GeneralMatrix(a, b) '     ReDim Retrun_K(a, b)
            n -= 1
            For i = 0 To a
                For j = 0 To b
                    Return_K(i, j) = 0
                    For k = 0 To n
                        Return_K(i, j) += K1(i, k) * K2(k, j)
                    Next
                Next
            Next
            Return True
        End Function

        ''' <summary>
        ''' 矩阵求平方根（sqrtm）
        ''' </summary>
        ''' <param name="K">目标方阵</param>
        ''' <param name="n">方阵K的阶数</param>
        ''' <param name="ks">求得的平方根.即ks*ks=K</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 如果K可以化成K=Inv(P)*diag(R)*P,其中Inv(P)表示P的逆矩阵,diag(R)*为K的特征值组成的对角矩阵,
        ''' 那么ks=Inv(P)*diag(R^0.5)*P,根据对角化原理,P*K*Inv(P)=Diag(R),其中Inv(P)是特征值R对应于K
        ''' 的特征向量,因此我们的算法=求特征值R,如果所有R均为正实数,则求R对应的特征向量Inv(P),然后讲R每
        ''' 个值取根放入对角矩阵对结果相乘即可
        ''' 
        ''' 例子:
        ''' ```
        ''' c =
        '''  [  192.291902022941   136.423323830855  -22.2582056347830   10.9878603820001
        '''    -176.869155076020  -120.047935463800   20.4023293672721  -16.5962890811120
        '''    -21.6722775306690  -60.5101175154120   135.025037886378   5.36535497517843
        '''     31.2279467353500   93.4954928282741  -106.961070363850   59.2865617399033  ]
        '''  
        ''' Math_Matrix_Sqrt(a,4,x)'求a平方根如下,可以进行x*x进行验证
        ''' x =
        '''  [  18.0067271094031   10.1514259204440  -0.80764239842560   0.96148324464486
        '''    -13.6679053566890  -5.69822839694800   0.61394536284630  -1.38430646207250
        '''    -3.17102914065820  -5.10751280313280   11.6191076697081   0.04254172112235
        '''     4.81532683312183   7.92820343443191  -5.38805756370240   8.16305909603188  ]
        ''' ```
        ''' </remarks>
        Public Function Sqrt(K As GeneralMatrix, n As Integer, ks As GeneralMatrix) As Int16
            '求方阵K的平方根
            'n表示K的阶数
            'ks返回的平方根
            '函数成功返回1
            '原理:K=inv(p)*diag(eig(k))*p
            'ks=inv(p)*diag(sqrt(eig(k))*p
            Dim eigvalue(0, 0) As Double
            Dim i, j As Integer
            If EigenValue(K, n, 500, 10, eigvalue, False) Then
                n -= 1
                For i = 0 To n '判断是否只存在正的实数特征值
                    If eigvalue(i, 1) <> 0 Or eigvalue(i, 0) < 0 Then
                        Return -1
                    End If
                Next
                Dim diag(n, n), eigtor(n, n), temp(n, 0), temp2(n, 0) As Double
                For i = 0 To n
                    diag(i, i) = stdNum.Sqrt(eigvalue(i, 0))
                    EigTorF(K, n + 1, eigvalue(i, 0), temp)
                    For j = 0 To n
                        eigtor(j, i) = temp(j, 0)
                    Next
                Next
                n += 1
                If Inv2(eigtor, temp, n) = False Then
                    Inv(eigtor, temp)
                End If
                Mul(eigtor, diag, n, temp2)
                Mul(temp2, temp, n, ks)
                Return 1
            Else
                Return -1
            End If
        End Function

        ''' <summary>
        ''' 右极分解，即F=R*U
        ''' </summary>
        ''' <param name="F">目标方阵</param>
        ''' <param name="n">方阵F的阶数</param>
        ''' <param name="R">分解得到的一个正交矩阵</param>
        ''' <param name="U">分解得到的一个对称正定矩阵</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 原理:任何一个可逆方阵均可以唯一的进行右极分解与左极分解,即F=R*U=V*R,其中U^2=T(F)*F,V^2=F*T(F)
        '''【其中T(F)表示F的转置】,则我们可以先通过F求得U或V,然后求R=F*Inv(U)=Inv(V)*F
        ''' 
        ''' 例子:
        ''' a =
        '''  [ 67.5919611787386     69.8554906388072     38.8768396987006     89.3106376236820
        '''    17.0671848194055     1.12767200969517     31.5601159499772     96.9140055109346
        '''    40.6681714768839     51.0876563615574     86.9885893943666     77.3506165842296
        '''    73.6101518727886     87.9281915202402     23.9508483670423     3.45968334165387 ]
        ''' 
        ''' Math_Matrix_RU(a,4,r,u)'进行右极分解得到如下结果
        ''' r =
        '''  [ -0.01806739003090   0.71913865214108  -0.27739456376250   0.63664949766408
        '''     0.45823822484909  -0.57214688695250   0.04612554269224   0.67904624212867
        '''    -0.09675742692290   0.20970810955629   0.95672796354849   0.17708590026142
        '''     0.88326786307616   0.33437989881145   0.07527225185216  -0.31939364294910  ]
        ''' u =
        '''  [ 67.6841644139219  71.9849022272286  26.4791752992667  38.3171883770002
        '''    71.9863924990090  89.6984480926248  36.1676400008128  26.2017432919420
        '''    26.4820423188876  36.1668446729677  75.6890493922576  53.9420595009903
        '''    38.3140405638833  26.2042694530458  53.9408769958847  135.276124831623  ]
        '''
        ''' Math_Matrix_VR(a,4,v,r)'进行左极分解得到如下结果
        ''' v =
        '''  [ 95.0902981485406     53.4500420292151     61.1182769307013     57.4613130458042
        '''    53.4438098825046     74.4408296746995     45.9449363126334    -13.1234428571640
        '''    61.1187415068155     45.9418557217637     103.699821171875     34.8450484220080
        '''    57.4619847804705    -13.1244040926920     34.8441839555448     95.1168377384350 ]
        ''' r =
        '''  [ -0.01835015300540     0.71939575068999    -0.27678823268990     0.63628327754090
        '''     0.45799761221593    -0.57191020194030     0.04534571824682     0.67953103744654
        '''    -0.09684035444340     0.20977163011655     0.95676617484504     0.17707600892713
        '''     0.88364856259618     0.33406264059053     0.07478222238892    -0.31912379017620 ]
        ''' </remarks>
        Public Function RU(F As GeneralMatrix, n As Integer, R As GeneralMatrix, U As GeneralMatrix) As Boolean
            '正定、非奇异方阵F进行右极分解
            'n为F的阶数
            '其中F=R*U
            'U^2=T(F)*F其中T(F)表示F的转置
            Dim FT = GeneralMatrix.Number, temp(0, 0) As Double
            FT = F.Transpose '  Math_Matrix_T(F, n, FT)
            Mul(FT, F, n, temp)
            If Sqrt(temp, n, U) = -1 Then
                Return False
            End If
            If Inv2(U, temp, n) = False Then
                Inv(U, temp)
            End If
            Mul(F, temp, n, R)
            Return True
        End Function

        ''' <summary>
        ''' 左极分解
        ''' </summary>
        ''' <param name="F">目标方阵</param>
        ''' <param name="n">方阵F的阶数</param>
        ''' <param name="V">分解得到的一个对称正定矩阵</param>
        ''' <param name="R">分解得到的一个正交矩阵，即F=V*R</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function VR(F As GeneralMatrix, n As Integer, V As GeneralMatrix, R As GeneralMatrix) As Boolean
            '正定、非奇异方阵F进行左极分解
            'n为F的阶数
            '其中F=V*R
            'V^2=F*T(F)其中T(F)表示F的转置
            Dim FT = GeneralMatrix.Number, temp(0, 0) As Double
            FT = F.Transpose '     Math_Matrix_T(F, n, FT)
            Mul(F, FT, n, temp)
            If Sqrt(temp, n, V) = -1 Then
                Return False
            End If
            If Inv2(V, temp, n) = False Then
                Inv(V, temp)
            End If
            Mul(temp, F, n, R)
            Return True
        End Function

        ''' <summary>
        ''' 构建哈密顿矩阵
        ''' </summary>
        ''' <param name="k">m阶的对称矩阵</param>
        ''' <param name="m">矩阵k的行数</param>
        ''' <param name="ret">获得的关于矩阵K的Hamiltonian矩阵</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Hamiltonian(k As GeneralMatrix, m As Integer, ret As GeneralMatrix) As Int16
            '获取2n*2n对称矩阵对应的Hamiltonian矩阵
            '返回1表示矩阵维数不是2n*2n的
            '返回2表示矩阵非对称矩阵
            If m Mod 2 = 1 Then
                Return 1
            End If
            If SPD(k) = -1 Then
                Return 2
            End If
            Dim m2 As Integer = m \ 2
            Dim i As Integer
            m -= 1
            Dim e(m, m) As Double
            For i = m2 To m
                e(i, i - m2) = 1
            Next
            For i = 0 To m2 - 1
                e(i, i + m2) = -1
            Next
            Mul(e, k, m + 1, ret)
            Return 0
        End Function

        ''' <summary>
        ''' 构建Lehmer矩阵
        ''' </summary>
        ''' <param name="n">构建Lehmer矩阵的阶数</param>
        ''' <param name="k">构建的Lehmer矩阵</param>
        ''' <remarks>Lehmer GeneralMatrix</remarks>
        Public Sub Lehmer(n As Integer, k As GeneralMatrix)
            '创建n阶Lehmer矩阵
            n -= 1
            k = New GeneralMatrix(n, n) '   ReDim k(n, n)
            Dim i As Integer
            Dim j As Integer
            For i = 0 To n
                k(i, i) = 1
                j = i
                While j < n
                    j += 1
                    k(i, j) += (i + 1) / (j + 1)
                    k(j, i) = k(i, j)
                End While
            Next
        End Sub

        ''' <summary>
        ''' 多项式乘法
        ''' </summary>
        ''' <param name="Mul1">乘数多项式系数</param>
        ''' <param name="Mul2">乘数多项式系数</param>
        ''' <param name="Ret">获得的乘积结果多项式系数</param>
        ''' <returns></returns>
        ''' <remarks>Ret=Mul1*Mul2</remarks>
        Public Function PolyMul(Mul1 As GeneralMatrix, Mul2 As GeneralMatrix, Ret As GeneralMatrix) As Integer
            '多项式相乘,函数返回Ret的大小.Ret为1行的矩阵
            Dim i As Integer
            Dim j As Integer
            Dim size1 As Integer = Mul1.Length - 1
            Dim size2 As Integer = Mul2.Length - 1
            Ret = New GeneralMatrix(0, size1 + size2) '    ReDim Ret(0, size1 + size2)
            For i = 0 To size1
                For j = 0 To size2
                    Ret(0, i + j) += Mul1(0, i) * Mul2(0, j)
                Next
            Next
            Return (size1 + size2 + 1)
        End Function

        ''' <summary>
        ''' 多项式除法
        ''' </summary>
        ''' <param name="A1">被除数存储多项式系数</param>
        ''' <param name="A2">除数存储多项式系数</param>
        ''' <param name="RetMod">求得的余数多项式系数</param>
        ''' <param name="Ret">求得的多项式商系数</param>
        ''' <param name="Erro">误差控制参数</param>
        ''' <returns></returns>
        ''' <remarks>A1/A2=Ret……RetMod</remarks>
        Public Function PolyDiv(A1 As GeneralMatrix, A2 As GeneralMatrix, RetMod As GeneralMatrix, Ret As GeneralMatrix, Erro As Integer) As Integer '多项式的除法，里边的数组均为1*n的矩阵,原理:A1/A2=Ret……RetMod'函数最终返回Ret商的数组大小
            Dim n1 As Integer = A1.Length
            Dim n2 As Integer = A2.Length
            Dim N As Integer
            Dim i As Integer
            If n1 < n2 Then
                Ret = New GeneralMatrix(0, 0) '     ReDim Ret(0, 0)
                n1 -= 1
                RetMod = New GeneralMatrix(0, n1) '     ReDim RetMod(0, n1)
                For i = 0 To n1
                    RetMod(0, i) = A1(0, i)
                Next
                Return 1
            End If
            Dim error1 As Double = stdNum.Abs(A2(0, 0)) * stdNum.Pow(0.1, Erro)
            Dim j As Integer
            N = n1 - n2
            Ret = New GeneralMatrix(0, N) '     ReDim Ret(0, N)
            N = 0
            While n1 >= n2
                Ret(0, N) = A1(0, 0) / A2(0, 0)
                i = 1
                While i < n2
                    A1(0, i) -= A2(0, i) * Ret(0, N)
                    If stdNum.Abs(A1(0, i)) <= error1 Then
                        A1(0, i) = 0
                    End If
                    i += 1
                End While
                i = 1
                While i < n1
                    N += 1
                    If A1(0, i) <> 0 Then
                        Exit While
                    End If
                    i += 1
                End While
                If i < n1 Then
                    j = i
                    While j < n1
                        A1(0, j - i) = A1(0, j)
                        j += 1
                    End While
                    n1 -= i
                    j = n1 - 1
                    A1 = New GeneralMatrix(0, j) '     ReDim Preserve A1(0, j)
                Else
                    n1 = 0
                    RetMod = New GeneralMatrix(0, 0) '  ReDim RetMod(0, 0)
                End If
            End While
            If n1 > 0 Then
                n1 -= 1
                RetMod = New GeneralMatrix(0, n1) '      ReDim RetMod(0, n1)
                For i = 0 To n1
                    RetMod(0, i) = A1(0, i)
                Next
            End If
            Return Ret.Length
        End Function

        ''' <summary>
        ''' 多项式求余数
        ''' </summary>
        ''' <param name="A1">被除数多项式系数</param>
        ''' <param name="A2">除数多项式系数</param>
        ''' <param name="Ret">求得的余数多项式系数</param>
        ''' <param name="Erro">误差控制参数</param>
        ''' <returns></returns>
        ''' <remarks>A1%A2=Ret</remarks>
        Public Function PolyMod(A1 As GeneralMatrix, A2 As GeneralMatrix, Ret As GeneralMatrix, Erro As Integer) As Integer
            '多项式求余Ret=A1%A2,函数返回余项Ret的列数.A1，A2，Ret均为1行的矩阵
            Dim a1n As Integer = A1.Length
            Dim a2n As Integer = A2.Length
            Dim i As Integer
            Dim temp As Double
            a1n -= 1
            a2n -= 1
            If a2n = 0 Then
                Ret = New GeneralMatrix(0, 0) '   ReDim Ret(0, 0)
                Return 1
            End If
            If a1n < a2n Then
                Ret = New GeneralMatrix(0, a1n) '   ReDim Ret(0, a1n)
                For i = 0 To a1n
                    Ret(0, i) = A1(0, i)
                Next
                i = a1n + 1
            Else
                Dim Erro1 As Double = stdNum.Abs(A2(0, 0)) * stdNum.Pow(0.1, Erro)
                Dim n As Integer
                Dim is1 As Boolean
                While a1n >= a2n
                    temp = A1(0, 0) / A2(0, 0)
                    is1 = True
                    For i = 1 To a2n
                        A1(0, i) -= temp * A2(0, i)
                        If is1 Then
                            If stdNum.Abs(A1(0, i)) > Erro1 Then
                                n = i
                                is1 = False
                            End If
                        ElseIf stdNum.Abs(A1(0, i)) <= Erro1 Then
                            A1(0, i) = 0
                        End If
                    Next
                    If is1 Then
                        n = a2n + 1
                        While n <= a1n
                            If A1(0, n) <> 0 Then
                                Exit While
                            End If
                            n += 1
                        End While
                    End If
                    If n > a1n Then
                        Ret = GeneralMatrix.Number '       ReDim Ret(0, 0)
                        Return 1
                    End If
                    For i = n To a1n
                        A1(0, i - n) = A1(0, i)
                    Next
                    a1n -= n
                    If a1n < a2n Then
                        Ret = New GeneralMatrix(0, a1n) '     ReDim Ret(0, a1n)
                        For i = 0 To a1n
                            Ret(0, i) = A1(0, i)
                        Next
                        i = a1n + 1
                        Return i
                    End If
                End While
            End If
            Return i
        End Function

        ''' <summary>
        ''' 离散傅里叶变换逆变换
        ''' </summary>
        ''' <param name="k">m*2的矩阵数据(数据点)K里的第一列代表数据的实数部分,第2列代表数据的虚数部分</param>
        ''' <param name="m">矩阵k的行数</param>
        ''' <param name="Number">离散点数</param>
        ''' <param name="X">离散傅里叶变换逆变换的结果矩阵是Number*2的矩阵,X里的第一列代表数据的实数部分,第2列代表数据的虚数部分</param>
        ''' <returns>本函数执行成功返回True.本函数相当于Matlab的快速傅里叶变换逆变换函数IFFT</returns>
        ''' <remarks></remarks>
        Public Function IDFT(k As GeneralMatrix, m As Integer, Number As Integer, X As GeneralMatrix) As Boolean
            '离散傅里叶变换逆变换,Number为点数
            '返回Number*2的矩阵,第一列为实数部分,第2列为虚数部分
            'k是m*2的矩阵,第一列为实数,第2列为虚数
            If Number > m Or Number < 1 Or m * 2 <> k.Length Then
                Return False
            End If
            m = Number - 1
            X = New GeneralMatrix(m, 1) '     ReDim X(m, 1)
            Dim i As Integer
            Dim j As Integer
            Dim tempx As Double
            Dim tempy As Double
            Dim temp As Double
            Dim temp2 As Double
            Dim tempcos As Double
            Dim tempsin As Double
            For i = 0 To m
                temp = stdNum.PI * 2 * i / Number
                tempx = 0
                tempy = 0
                For j = 0 To m
                    temp2 = temp * j
                    tempcos = stdNum.Cos(temp2)
                    tempsin = stdNum.Sin(temp2)
                    tempx += tempcos * k(j, 0) - tempsin * k(j, 1)
                    tempy += tempcos * k(j, 1) + tempsin * k(j, 0)
                Next
                X(i, 0) = tempx / Number
                X(i, 1) = tempy / Number
            Next
            Return True
        End Function

        ''' <summary>
        ''' 离散傅里叶变换
        ''' </summary>
        ''' <param name="k">m*2的矩阵数据(数据点)K里的第一列代表数据的实数部分,第2列代表数据的虚数部分</param>
        ''' <param name="m">矩阵k的行数</param>
        ''' <param name="Number">离散点数</param>
        ''' <param name="X">离散傅里叶变换的结果矩阵是Number*2的矩阵,X里的第一列代表数据的实数部分,第2列代表数据的虚数部分</param>
        ''' <returns>本函数执行成功返回True.本函数相当于Matlab的快速傅里叶变换函数FFT</returns>
        ''' <remarks></remarks>
        Public Function DFT(k As GeneralMatrix, m As Integer, Number As Integer, X As GeneralMatrix) As Boolean
            '离散傅里叶变换,Number为点数
            '返回Number*2的矩阵,第一列为实数部分,第2列为虚数部分
            'k是m*2的矩阵,第一列为实数,第2列为虚数
            If Number > m Or Number < 1 Or m * 2 <> k.Length Then
                Return False
            End If
            m = Number - 1
            X = New GeneralMatrix(m, 1) '    ReDim X(m, 1)
            Dim i As Integer
            Dim j As Integer
            Dim tempx As Double
            Dim tempy As Double
            Dim temp As Double
            Dim temp2 As Double
            Dim tempcos As Double
            Dim tempsin As Double
            For i = 0 To m
                temp = -stdNum.PI * 2 * i / Number
                tempx = 0
                tempy = 0
                For j = 0 To m
                    temp2 = temp * j
                    tempcos = stdNum.Cos(temp2)
                    tempsin = stdNum.Sin(temp2)
                    tempx += tempcos * k(j, 0) - tempsin * k(j, 1)
                    tempy += tempcos * k(j, 1) + tempsin * k(j, 0)
                Next
                X(i, 0) = tempx
                X(i, 1) = tempy
            Next
            Return True
        End Function

        ''' <summary>
        ''' 求矩阵的一个正交基Orth
        ''' </summary>
        ''' <param name="k">目标矩阵</param>
        ''' <param name="m">k的行数</param>
        ''' <param name="ret">获得的一个正交基矩阵</param>
        ''' <returns>函数失败返回小于1的数据，成功返回ret的行数</returns>
        ''' <remarks>对矩阵进行svd分解即用SvdSplit得到k=usv*,则s是奇异值矩阵,可以奇异值是否为0获得矩阵的秩r,然后ret就是m*r的矩阵且其就是u里的m*r的部分值</remarks>
        Public Function Orth(k As GeneralMatrix, m As Integer, ret As GeneralMatrix) As Integer
            Dim u(0, 0) As Double
            Dim s(0, 0) As Double
            Dim sm As Integer
            If SvdSplit(k, m, Nothing, Nothing, s, sm, u, Nothing) = False Then
                Return 0
            End If
            Dim i As Integer
            Dim j As Integer
            Dim n As Integer = s.Length \ sm
            Dim rank As Integer = 0 '是矩阵的秩
            If n > sm Then
                n = sm
            End If
            n -= 1
            For i = 0 To n
                If s(i, i) <> 0 Then
                    rank += 1
                End If
            Next
            rank -= 1
            If rank < 0 Then
                Return 0
            End If
            m -= 1
            ret = New GeneralMatrix(m, rank) '     ReDim ret(m, rank)
            For i = 0 To m
                For j = 0 To rank
                    ret(i, j) = u(i, j)
                Next
            Next
            Return m + 1
        End Function

        ''' <summary>
        ''' 幻方
        ''' </summary>
        ''' <param name="n">幻方的阶数(大于2)</param>
        ''' <param name="start">幻方的中最小的正整数,一般可以设置为1</param>
        ''' <param name="k">获得的幻方</param>
        ''' <remarks></remarks>
        Private Sub Magic(n As Integer, start As Double, k As GeneralMatrix)
            k = New GeneralMatrix(n - 1, n - 1) ' ReDim k(n - 1, n - 1)
            If n Mod 4 = 0 Then
                Magic_4(n, start, k)
            ElseIf n Mod 2 = 0 Then
                Magic_2(n, start, k)
            Else
                Magic_1(n, start, k)
            End If
        End Sub

        Private Sub Magic_1(n As Integer, start As Double, k As GeneralMatrix)
            n -= 1
            Dim j As Integer = n \ 2
            k(0, j) = start
            Dim i As Integer = 0
            Dim Number As Integer = 1
            While Number < k.Length
                j += 1
                i -= 1
                If i < 0 Then
                    i = n
                End If
                If j > n Then
                    j = 0
                End If
                If k(i, j) > 0 Then
                    If i = n Then
                        i = 1
                    Else
                        i += 2
                        If i > n Then
                            i = 0
                        End If
                    End If
                    If j = 0 Then
                        j = n
                    Else
                        j -= 1
                        If j < 0 Then
                            j = n
                        End If
                    End If
                End If
                k(i, j) = Number + start
                Number += 1
            End While
        End Sub

        Private Sub Magic_2(n As Integer, start As Double, k As GeneralMatrix)
            n = n \ 2 - 1
            Dim A(n, n) As Double
            Dim B(n, n) As Double
            Dim C(n, n) As Double
            Dim D(n, n) As Double
            Dim temp As Double = (n + 1) * (n + 1)
            Magic_1(n + 1, start, A)
            start = start + temp
            Magic_1(n + 1, start, B)
            start = start + temp
            Magic_1(n + 1, start, C)
            start = start + temp
            Magic_1(n + 1, start, D)
            start = start + temp
            Dim i As Integer
            Dim j As Integer
            Dim number As Integer = n / 2
            For i = 0 To n
                If i <> number Then
                    For j = 0 To number - 1
                        temp = A(i, j)
                        A(i, j) = D(i, j)
                        D(i, j) = temp
                    Next
                Else
                    j = i
                    While j < i + number
                        temp = A(i, j)
                        A(i, j) = D(i, j)
                        D(i, j) = temp
                        j += 1
                    End While
                End If
            Next
            number = n - number + 2
            For i = 0 To n
                For j = number To n
                    temp = B(i, j)
                    B(i, j) = C(i, j)
                    C(i, j) = temp
                Next
            Next
            For i = 0 To n
                For j = 0 To n
                    k(i, j) = A(i, j)
                    k(i, j + n + 1) = C(i, j)
                    k(i + n + 1, j) = D(i, j)
                    k(i + n + 1, j + n + 1) = B(i, j)
                Next
            Next
        End Sub

        Private Sub Magic_4(n As Integer, start As Double, k As GeneralMatrix)
            Dim i As Integer
            Dim j As Integer
            Dim temp As Integer
            Dim tempf As Double
            n -= 1
            For i = 0 To n
                temp = i * (n + 1)
                For j = 0 To n
                    k(i, j) = temp + j + start
                Next
            Next
            temp = n \ 2
            For i = 0 To temp
                For j = 0 To temp
                    If (i + j) Mod 2 = 0 Then
                        tempf = k(i, j)
                        k(i, j) = k(n - i, n - j)
                        k(n - i, n - j) = tempf
                    End If
                Next
            Next
            For i = 0 To temp
                For j = temp + 1 To n
                    If (i + j) Mod 2 = 1 Then
                        tempf = k(i, j)
                        k(i, j) = k(n - i, n - j)
                        k(n - i, n - j) = tempf
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 高斯全选主元素法解方程，本函数是求解AX=B这类问题的。函数采用全选主元素的高斯消元法，对于出现非满秩矩阵时(A的化简过程中的A)，
        ''' 只要函数有解(可能不止一组解,此时只返回一组解)，本函数都能返回其解
        ''' </summary>
        ''' <param name="A">A_m*n的矩阵</param>
        ''' <param name="b">B_m*1的矩阵</param>
        ''' <param name="A_m"></param>
        ''' <param name="B_m"></param>
        ''' <param name="X">求解得到的矩阵</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 例子:
        ''' a =
        '''  [ 89.7234413259306  12.9170338217714  79.9443395249286  78.1627263772128
        '''    62.8960442556516  63.9951517172135  2.9257326400493   57.119458800703
        '''    83.5902038885235  55.9411662425572  89.4671598865963  33.7297967792162 ]
        ''' 
        ''' b =
        '''  [ 65.2027291083721
        '''    54.2041894766522
        '''    63.722165657078   ]
        ''' 
        ''' 经过本函数后得到的解如下
        ''' x =
        '''  [ -0.826689550370445
        '''    0.737377350436936
        '''    0.646558671079671
        '''    1                 ]
        ''' 
        ''' 即AX=B
        ''' </remarks>
        Public Function Sove2(A As GeneralMatrix, b As GeneralMatrix, A_m As Integer, B_m As Integer, X As GeneralMatrix) As Boolean
            '采用全选主元素法求解
            If A_m <> B_m Or B_m <> b.Length Then
                Return False
            End If
            Dim n As Integer = A.Length \ A_m - 1
            A_m -= 1
            Dim i As Integer
            Dim Indez_i As Integer
            Dim Index_j As Integer
            Dim temp_i As Integer
            Dim temp_j As Integer
            Dim max As Double
            Dim temp As Double
            Dim Index(n) As Integer
            X = New GeneralMatrix(n, 0) '   ReDim X(n, 0)
            For i = 0 To n
                Index(i) = i
            Next
            For i = 0 To A_m
                max = 0
                For temp_i = i To A_m
                    For temp_j = i To n
                        temp = stdNum.Abs(A(temp_i, temp_j))
                        If temp > max Then
                            max = temp
                            Index_j = temp_j
                            Indez_i = temp_i
                        End If
                    Next
                Next
                If max > 0 Then
                    If Indez_i > i Then
                        For temp_j = i To n
                            temp = A(i, temp_j)
                            A(i, temp_j) = A(Indez_i, temp_j)
                            A(Indez_i, temp_j) = temp
                        Next
                        temp = b(i, 0)
                        b(i, 0) = b(Indez_i, 0)
                        b(Indez_i, 0) = temp
                    End If
                    If Index_j > i Then
                        temp_i = Index(i)
                        Index(i) = Index(Index_j)
                        Index(Index_j) = temp_i
                        For temp_i = 0 To A_m
                            temp = A(temp_i, i)
                            A(temp_i, i) = A(temp_i, Index_j)
                            A(temp_i, Index_j) = temp
                        Next
                    End If
                    temp_i = i
                    While temp_i < A_m
                        temp_i += 1
                        If A(temp_i, i) <> 0 Then
                            temp = A(i, i) / A(temp_i, i)
                            'A(temp_i, i)=0
                            temp_j = i
                            While temp_j < n
                                temp_j += 1
                                A(temp_i, temp_j) = A(temp_i, temp_j) * temp - A(i, temp_j)
                            End While
                            b(temp_i, 0) = b(temp_i, 0) * temp - b(i, 0)
                        End If
                    End While
                Else
                    Exit For
                End If
            Next
            If A_m > n Then
                For temp_i = A_m + 1 To n
                    If b(temp_i, 0) <> 0 Then
                        Return False
                    End If
                Next
            End If
            If n > A_m Then
                For temp_i = A_m + 1 To n
                    X(Index(temp_i), 0) = 1
                Next
                For temp_i = 0 To A_m
                    For temp_j = A_m + 1 To n
                        b(temp_i, 0) -= A(temp_i, temp_j)
                    Next
                Next
            End If
            For temp_i = A_m To 0 Step -1
                If A(temp_i, temp_i) = 0 Then
                    If b(temp_i, 0) <> 0 Then
                        Return False
                    End If
                    X(Index(temp_i), 0) = 1
                Else
                    X(Index(temp_i), 0) = b(temp_i, 0) / A(temp_i, temp_i)
                End If
                temp_j = temp_i
                While temp_j > 0
                    temp_j -= 1
                    b(temp_j, 0) -= A(temp_j, temp_i) * X(Index(temp_i), 0)
                End While
            Next
            Return True
        End Function

        ''' <summary>
        ''' 多项式提取最大公因式
        ''' </summary>
        ''' <param name="A1">1*A1_n的存储多项式系数的矩阵</param>
        ''' <param name="A1_n">A1的列数</param>
        ''' <param name="A2">为1*A2_n的存储多项式系数的矩阵</param>
        ''' <param name="A2_n">A2的列数</param>
        ''' <param name="Ret">获得的最大公因式多项式系数</param>
        ''' <param name="Erro">误差控制参数</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PolyGCF(A1 As GeneralMatrix, A1_n As Integer, A2 As GeneralMatrix, A2_n As Integer, Ret As GeneralMatrix, Erro As Integer) As Integer '求2个多项式的最大公因式Ret,
            '不知道为什么在调用Math_Matrix_PolyGCF_Call函数时,其参数A!会被当作 ByRef属性
            Dim A11(0, A1_n - 1) As Double
            Dim A22(0, A2_n - 1) As Double
            Dim i As Integer
            For i = 0 To A1_n - 1
                A11(0, i) = A1(0, i)
            Next
            For i = 0 To A2_n - 1
                A22(0, i) = A2(0, i)
            Next
            Return PolyGCFCall(A11, A1_n, A22, A2_n, Ret, Erro)
        End Function

        ''' <summary>
        ''' 求2个多项式的最大公因式Ret，A1为1*A1_n的矩阵，A2为1*A2_n的矩阵。函数执行后返回公因式Ret的大小
        ''' </summary>
        ''' <param name="A1"></param>
        ''' <param name="A1_n"></param>
        ''' <param name="A2"></param>
        ''' <param name="A2_n"></param>
        ''' <param name="Ret"></param>
        ''' <param name="Erro"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function PolyGCFCall(A1 As GeneralMatrix, A1_n As Integer, A2 As GeneralMatrix, A2_n As Integer, Ret As GeneralMatrix, Erro As Integer) As Integer
            If A1_n < A2_n Then
                Return PolyGCFCall(A2, A2_n, A1, A1_n, Ret, Erro)
            ElseIf A2_n = 1 Then
                Ret = New GeneralMatrix(0, 0) '     ReDim Ret(0, 0)
                Ret(0, 0) = 1
                Return 1
            End If
            Dim i As Integer
            Dim j As Integer
            Dim Erro1 As Double = stdNum.Abs(A1(0, 0)) * stdNum.Pow(0.1, Erro)
            Dim temp As Double
            While True
                temp = A1(0, 0) / A2(0, 0)
                i = 1
                While i < A2_n
                    A1(0, i) -= A2(0, i) * temp
                    If stdNum.Abs(A1(0, i)) <= Erro1 Then
                        A1(0, i) = 0
                    End If
                    i += 1
                End While
                i = 1
                While i < A1_n
                    If A1(0, i) <> 0 Then
                        Exit While
                    End If
                    i += 1
                End While
                If i = A1_n Then
                    A2_n -= 1
                    Ret = New GeneralMatrix(0, A2_n) '  ReDim Ret(0, A2_n)
                    For i = 0 To A2_n
                        Ret(0, i) = A2(0, i)
                    Next
                    Return (A2_n + 1)
                End If
                j = i
                While i < A1_n
                    A1(0, i - j) = A1(0, i)
                    i += 1
                End While
                A1_n -= j
                If A1_n < A2_n Then
                    A1.Resize(0, A1_n - 1) '     ReDim Preserve A1(0, A1_n - 1)
                    Return PolyGCFCall(A2, A2_n, A1, A1_n, Ret, Erro)
                End If
            End While
            Return -1
        End Function

        ''' <summary>
        ''' n阶帕斯卡(Pascal)矩阵  
        ''' </summary>
        ''' <param name="n">表示产生帕斯卡(Pascal)矩阵的阶数</param>
        ''' <param name="k">产生的n阶帕斯卡(Pascal)矩阵</param>
        ''' <remarks>Pascal GeneralMatrix即产生n阶的帕斯卡矩阵由杨辉三角形表组成的矩阵称为帕斯卡(Pascal)矩阵</remarks>
        Public Sub Pascal(n As Integer, k As GeneralMatrix)
            '产生n阶的帕斯卡矩阵由杨辉三角形表组成的矩阵称为帕斯卡(Pascal)矩阵。
            Dim i As Integer
            Dim j As Integer
            n -= 1
            k = New GeneralMatrix(n, n) ' ReDim k(n, n)
            For i = 0 To n
                k(0, i) = 1
                k(i, 0) = 1
            Next
            i = 1
            While i <= n
                j = 1
                While j <= n
                    k(i, j) = k(i, j - 1) + k(i - 1, j)
                    j += 1
                End While
                i += 1
            End While
        End Sub
    End Module
End Namespace
