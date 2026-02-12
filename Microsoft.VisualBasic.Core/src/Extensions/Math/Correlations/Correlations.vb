#Region "Microsoft.VisualBasic::80a08adfffd989f2df5e315dc6e4c757, Microsoft.VisualBasic.Core\src\Extensions\Math\Correlations\Correlations.vb"

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

    '   Total Lines: 557
    '    Code Lines: 309 (55.48%)
    ' Comment Lines: 163 (29.26%)
    '    - Xml Docs: 87.73%
    ' 
    '   Blank Lines: 85 (15.26%)
    '     File Size: 21.60 KB


    '     Module Correlations
    ' 
    '         Properties: PearsonDefault
    ' 
    '         Function: (+2 Overloads) GetPearson, (+2 Overloads) JaccardIndex, JSD, kendallTauBeta, KLD
    '                   KLDi, rankKendallTauBeta, SW
    ' 
    '         Sub: TestStats
    '         Structure Pearson
    ' 
    '             Properties: P
    ' 
    '             Function: Measure, RankPearson, ToString
    ' 
    '         Delegate Function
    ' 
    '             Function: CorrelationMatrix, sortRanking, Spearman
    ' 
    '             Sub: throwDimensionSizeNotAgree
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports DataSet = Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue(Of System.Collections.Generic.Dictionary(Of String, Double))
Imports std = System.Math
Imports Vector = Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue(Of Double())

Namespace Math.Correlations

    ''' <summary>
    ''' 计算两个数据向量之间的相关度的大小
    ''' </summary>
    <Package("Correlations", Category:=APICategories.UtilityTools, Publisher:="amethyst.asuka@gcmodeller.org")>
    Public Module Correlations

        ReadOnly objectEquals As New [Default](Of Func(Of Object, Object, Boolean))(Function(x, y) x.Equals(y))

        ''' <summary>
        ''' The Jaccard index, also known as Intersection over Union and the Jaccard similarity coefficient 
        ''' (originally coined coefficient de communauté by Paul Jaccard), is a statistic used for comparing 
        ''' the similarity and diversity of sample sets. The Jaccard coefficient measures similarity between 
        ''' finite sample sets, and is defined as the size of the intersection divided by the size of the 
        ''' union of the sample sets.
        ''' 
        ''' https://en.wikipedia.org/wiki/Jaccard_index
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="equal"></param>
        ''' <returns></returns>
        Public Function JaccardIndex(Of T)(a As IEnumerable(Of T),
                                           b As IEnumerable(Of T),
                                           Optional equal As Func(Of Object, Object, Boolean) = Nothing) As Double

            Dim setA As New [Set](a, equal Or objectEquals)
            Dim setB As New [Set](b, equal Or objectEquals)

            ' (If A and B are both empty, we define J(A,B) = 1.)
            If setA.Length = setB.Length AndAlso setA.Length = 0 Then
                Return 1
            End If

            Dim intersects = setA And setB
            Dim union = setA + setB
            Dim similarity# = intersects.Length / union.Length

            Return similarity
        End Function

        ''' <summary>
        ''' The Jaccard index, also known as Intersection over Union and the Jaccard similarity coefficient 
        ''' (originally coined coefficient de communauté by Paul Jaccard), is a statistic used for comparing 
        ''' the similarity and diversity of sample sets. The Jaccard coefficient measures similarity between 
        ''' finite sample sets, and is defined as the size of the intersection divided by the size of the 
        ''' union of the sample sets.
        ''' 
        ''' https://en.wikipedia.org/wiki/Jaccard_index
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension>
        Public Function JaccardIndex(a As IReadOnlyCollection(Of String), b As IReadOnlyCollection(Of String)) As Double
            ' 1. 计算交集的大小
            Dim intersectionCount As Double = a.Intersect(b).Count()
            ' 2. 计算并集的大小
            ' 公式: |A| + |B| - |A ∩ B|
            Dim unionCount As Double = a.Count + b.Count - intersectionCount

            ' 3. 防止除以零（如果两个集合都为空）
            If unionCount = 0 Then
                Return 1.0 ' 两个空集合通常被认为完全相似
            End If

            ' 4. 计算比率
            Return intersectionCount / unionCount
        End Function

        ''' <summary>
        ''' Sandelin-Wasserman similarity function.
        ''' (假若所有的元素都是0-1之间的话，结果除以2可以得到相似度)
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Function SW(x As Double(), y As Double()) As Double
            Dim p = From i As Integer
                    In x.Sequence
                    Select x(i) - y(i)
            Dim s# = Aggregate n As Double In p Into Sum(n ^ 2)
            s = 2 - s
            Return s
        End Function

        ''' <summary>
        ''' Jensen–Shannon divergence（J-S散度） is a method of measuring the similarity between two 
        ''' probability distributions.
        ''' It is based on the Kullback–Leibler divergence（K-L散度）, with some notable (and useful) 
        ''' differences, including that it is symmetric and it is always a finite value.
        ''' </summary>
        ''' <param name="P"></param>
        ''' <param name="Q"></param>
        ''' <returns></returns>
        Public Function JSD(P As Double(), Q As Double()) As Double
            ' 计算混合分布 M
            Dim M As Double() = P.Select(Function(pi, i) 0.5 * (pi + Q(i))).ToArray
            ' 计算 JSD = 0.5 * KLD(P, M) + 0.5 * KLD(Q, M)
            ' 注意：KLD 现在是非对称的，顺序很重要
            Dim divergence = 0.5 * KLD(P, M) + 0.5 * KLD(Q, M)

            Return divergence
        End Function

        ''' <summary>
        ''' Kullback-Leibler divergence, (KL散度/相对熵)
        ''' </summary>
        ''' <param name="x"><paramref name="x"/>和<paramref name="y"/>必须是等长的</param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 计算 D_KL(P || Q) = Σ P(i) * ln(P(i) / Q(i))
        ''' </remarks>
        Public Function KLD(x As Double(), y As Double()) As Double
            Dim a, b As Double

            For i As Integer = 0 To x.Length - 1
                a += KLD(x(i), y(i))
                b += KLD(y(i), x(i))
            Next

            Dim value As Double = (a + b) / 2
            Return value
        End Function

        Private Function KLD(Pi As Double, Qi As Double) As Double
            If Pi = 0R Then
                ' 0 * n = 0
                Return 0R
            Else
                ' KLD(P||Q) = Σ[P(i)*ln(P(i)/Q(i))]
                Dim value As Double = Pi * std.Log(Pi / Qi)
                Return value
            End If
        End Function

#Region "https://en.wikipedia.org/wiki/Kendall_tau_distance"

        ''' <summary>
        ''' Provides rank correlation coefficient metrics Kendall tau
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://github.com/felipebravom/RankCorrelation
        ''' </remarks>
        Public Function rankKendallTauBeta(x As Double(), y As Double()) As Double
            Dim x_n As Integer = x.Length
            Dim y_n As Integer = y.Length
            Dim x_rank As Double() = New Double(x_n - 1) {}
            Dim y_rank As Double() = New Double(y_n - 1) {}
            Dim sorted As New SortedDictionary(Of Double?, HashSet(Of Integer?))()

            For i As Integer = 0 To x_n - 1
                Dim v As Double = x(i)
                If sorted.ContainsKey(v) = False Then
                    sorted(v) = New HashSet(Of Integer?)()
                End If
                sorted(v).Add(i)
            Next

            Dim c As Integer = 1

            For Each v As Double In sorted.Keys.OrderByDescending(Function(k) k)
                Dim r As Double = 0
                For Each i As Integer In sorted(v)
                    r += c
                    c += 1
                Next

                r /= sorted(v).Count

                For Each i As Integer In sorted(v)
                    x_rank(i) = r
                Next
            Next

            sorted.Clear()

            For i As Integer = 0 To y_n - 1
                Dim v As Double = y(i)
                If sorted.ContainsKey(v) = False Then
                    sorted(v) = New HashSet(Of Integer?)()
                End If
                sorted(v).Add(i)
            Next

            c = 1

            For Each v As Double In sorted.Keys.OrderByDescending(Function(k) k)
                Dim r As Double = 0
                For Each i As Integer In sorted(v)
                    r += c
                    c += 1
                Next

                r /= (sorted(v).Count)

                For Each i As Integer In sorted(v)
                    y_rank(i) = r
                Next
            Next

            Return kendallTauBeta(x_rank, y_rank)
        End Function

        ''' <summary>
        ''' Provides rank correlation coefficient metrics Kendall tau
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://github.com/felipebravom/RankCorrelation
        ''' </remarks>
        Private Function kendallTauBeta(x As Double(), y As Double()) As Double
            Dim c As Integer = 0
            Dim d As Integer = 0
            Dim xTies As New Dictionary(Of Double?, HashSet(Of Integer?))()
            Dim yTies As New Dictionary(Of Double?, HashSet(Of Integer?))()

            For i As Integer = 0 To x.Length - 2
                For j As Integer = i + 1 To x.Length - 1
                    If x(i) > x(j) AndAlso y(i) > y(j) Then
                        c += 1
                    ElseIf x(i) < x(j) AndAlso y(i) < y(j) Then
                        c += 1
                    ElseIf x(i) > x(j) AndAlso y(i) < y(j) Then
                        d += 1
                    ElseIf x(i) < x(j) AndAlso y(i) > y(j) Then
                        d += 1
                    Else
                        If x(i) = x(j) Then
                            If xTies.ContainsKey(x(i)) = False Then
                                xTies(x(i)) = New HashSet(Of Integer?)()
                            End If
                            xTies(x(i)).Add(i)
                            xTies(x(i)).Add(j)
                        End If

                        If y(i) = y(j) Then
                            If yTies.ContainsKey(y(i)) = False Then
                                yTies(y(i)) = New HashSet(Of Integer?)()
                            End If
                            yTies(y(i)).Add(i)
                            yTies(y(i)).Add(j)
                        End If
                    End If
                Next
            Next

            Dim diff As Integer = c - d
            Dim denom As Double = 0

            Dim n0 As Double = (x.Length * (x.Length - 1)) / 2.0
            Dim n1 As Double = 0
            Dim n2 As Double = 0

            For Each t As Double In xTies.Keys
                Dim s As Double = xTies(t).Count
                n1 += (s * (s - 1)) / 2
            Next

            For Each t As Double In yTies.Keys
                Dim s As Double = yTies(t).Count
                n2 += (s * (s - 1)) / 2
            Next

            denom = std.Sqrt((n0 - n1) * (n0 - n2))

            If denom = 0 Then
                denom += 0.000000001
            End If

            ' 0.000..1 added on 11/02/2013 fixing NaN error
            Dim td As Double = diff / (denom)

            Return td
        End Function
#End Region

        ''' <summary>
        ''' will regularize the unusual case of complete correlation
        ''' </summary>
        ''' <remarks>
        ''' A this tiny value for avoid divid ZERO
        ''' </remarks>
        Public Const TINY As Double = 1.0E-20

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="prob">p-value in R ``cor.test`` function.</param>
        ''' <param name="prob2"></param>
        ''' <param name="z">fisher's z trasnformation</param>
        ''' <param name="df">degree of freedom</param>
        ''' <param name="t">
        ''' student's t probability
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' checked by Excel
        ''' </remarks>
        <ExportAPI("Pearson")>
        Public Function GetPearson(x#(), y#(),
                                   Optional ByRef prob# = 0,
                                   Optional ByRef prob2# = 0,
                                   Optional ByRef z# = 0,
                                   Optional ByRef t# = 0,
                                   Optional ByRef df# = 0,
                                   Optional throwMaxIterError As Boolean = True) As Double

            Dim pcc As Double = GetPearson(x, y)
            Dim n As Integer = x.Length

            If pcc > 1 Then
                pcc = 1
            End If

            Call TestStats(pcc, n, z, prob, prob2, t, df, throwMaxIterError)

            Return pcc
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="cor"></param>
        ''' <param name="n">should be the length of x or y vector</param>
        ''' <param name="z"></param>
        ''' <param name="pvalue"></param>
        ''' <param name="prob2"></param>
        ''' <param name="t"></param>
        ''' <param name="df"></param>
        ''' <param name="throwMaxIterError"></param>
        ''' <remarks>
        ''' Fisher's z 变换，主要用于皮尔逊相关系数的非线性修正上面。因为普通皮尔逊相关系数
        ''' 在0-1上并不服从正态分布，相关系数的绝对值越趋近1时，概率变得非常非常小。相关系数
        ''' 的分布非常像断了两头的正态分布。所以需要通过Fisherz-transformation对皮尔逊相
        ''' 关系数进行修正，使得满足正态分布。
        ''' </remarks>
        Public Sub TestStats(cor As Double, n As Integer,
                             ByRef z#,
                             ByRef pvalue#,
                             ByRef prob2#,
                             ByRef t#,
                             ByRef df#,
                             Optional throwMaxIterError As Boolean = True)

            ' fisher's z trasnformation
            ' 1/2 * ln((1+r)/(1-r))
            z = 0.5 * std.Log((1.0 + cor + TINY) / (1.0 - cor + TINY))

            ' student's t probability
            df = n - 2
            t = cor * std.Sqrt(df / ((1.0 - cor + TINY) * (1.0 + cor + TINY)))

            pvalue = Beta.betai(0.5 * df, 0.5, df / (df + t ^ 2), throwMaxIterError)
            ' for a large n
            prob2 = Beta.erfcc(std.Abs(z * std.Sqrt(n - 1.0)) / 1.4142136)
        End Sub

        Public Structure Pearson

            Dim pearson#
            Dim pvalue#
            Dim pvalue2#
            Dim Z#

            Public ReadOnly Property P As Double
                <MethodImpl(MethodImplOptions.AggressiveInlining)>
                Get
                    Return -std.Log10(pvalue)
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{pearson} @ {pvalue}"
            End Function

            Public Shared Function Measure(x As IEnumerable(Of Double), y As IEnumerable(Of Double)) As Pearson
                Dim pvalue1, pvalue2, z As Double
                Dim pearson# = GetPearson(x.ToArray, y.ToArray, pvalue1, pvalue2, z)

                Return New Pearson With {
                    .pearson = pearson,
                    .pvalue = pvalue1,
                    .pvalue2 = pvalue2,
                    .Z = z
                }
            End Function

            Public Shared Function RankPearson(x As IEnumerable(Of Double), y As IEnumerable(Of Double)) As Pearson
                Dim r1 = x.Ranking(Strategies.FractionalRanking)
                Dim r2 = y.Ranking(Strategies.FractionalRanking)

                Return Measure(r1, r2)
            End Function
        End Structure

        ''' <summary>
        ''' 默认使用Pearson相似度
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PearsonDefault As [Default](Of ICorrelation) = New ICorrelation(AddressOf GetPearson)

        ''' <summary>
        ''' Pearson correlations
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="y#"></param>
        ''' <returns></returns>
        <ExportAPI("Pearson")>
        Public Function GetPearson(x#(), y#()) As Double
            Dim n As Integer = x.Length
            Dim yt As Double, xt As Double
            Dim syy As Double = 0.0, sxy As Double = 0.0, sxx As Double = 0.0
            Dim ay As Double = 0.0, ax As Double = 0.0

            For j As Integer = 0 To n - 1
                ' finds the mean
                ax += x(j)
                ay += y(j)
            Next

            ax /= n
            ay /= n

            For j As Integer = 0 To n - 1
                ' compute correlation coefficient
                xt = x(j) - ax
                yt = y(j) - ay
                sxx += xt * xt
                syy += yt * yt
                sxy += xt * yt
            Next

            Return sxy / (std.Sqrt(sxx * syy) + TINY)
        End Function

        ''' <summary>
        ''' 相关性的计算分析函数
        ''' </summary>
        ''' <param name="X"></param>
        ''' <param name="Y"></param>
        ''' <returns></returns>
        Public Delegate Function ICorrelation(X#(), Y#()) As Double

        Const VectorSizeMustAgree$ = "[X:={0}, Y:={1}] The vector length betwen the two samples is not agreed!!!"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub throwDimensionSizeNotAgree(x#(), y#())
            Throw New DataException(String.Format(VectorSizeMustAgree, x.Length, y.Length))
        End Sub

        ''' <summary>
        ''' This method should not be used in cases where the data set is truncated; that is,
        ''' when the Spearman correlation coefficient is desired for the top X records
        ''' (whether by pre-change rank or post-change rank, or both), the user should use the
        ''' Pearson correlation coefficient formula given above.
        ''' (斯皮尔曼相关性)
        ''' </summary>
        ''' <param name="X"></param>
        ''' <param name="Y"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://en.wikipedia.org/wiki/Spearman%27s_rank_correlation_coefficient
        ''' spearman rho checked!
        ''' </remarks>
        '''
        Public Function Spearman(x As Double(), y As Double()) As Double
            Dim x_rank As Double() = x.sortRanking
            Dim y_rank As Double() = y.sortRanking

            Return GetPearson(x_rank, y_rank)
        End Function

        <Extension>
        Private Function sortRanking(x As Double()) As Double()
            Dim sorted As New Dictionary(Of Double, List(Of Integer))()
            Dim size As Integer = x.Length
            Dim ranks As Double() = New Double(size - 1) {}
            Dim v As Double
            Dim c As Double = 1

            For i As Integer = 0 To size - 1
                v = x(i)

                If sorted.ContainsKey(v) = False Then
                    sorted(v) = New List(Of Integer)
                End If

                Call sorted(v).Add(i)
            Next

            For Each vi As Double In sorted.Keys.OrderByDescending(Function(xi) xi)
                Dim r As Double = 0
                Dim sortSet = sorted(vi)

                For Each i As Integer In sortSet
                    r += c
                    c += 1
                Next

                r /= sortSet.Count

                For Each i As Integer In sortSet
                    ranks(i) = r
                Next
            Next

            Return ranks
        End Function

        ''' <summary>
        ''' 输入的数据为一个对象属性的集合，默认的<paramref name="compute"/>计算方法为<see cref="GetPearson"/>
        ''' </summary>
        ''' <param name="data">``[ID, properties]``</param>
        ''' <param name="compute">
        ''' Using pearson method as default if this parameter is nothing.
        ''' (默认的计算形式为<see cref="GetPearson"/>)
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function CorrelationMatrix(data As IEnumerable(Of Vector), Optional compute As ICorrelation = Nothing) As DataSet()
            Dim array As Vector() = data.ToArray
            Dim outMatrix As New List(Of DataSet)

            compute = compute Or PearsonDefault

            For Each a As Vector In array
                Dim ca As New Dictionary(Of String, Double)
                Dim v#() = a.Value

                For Each b In array
                    ca(b.Name) = compute(v, b.Value)
                Next

                outMatrix += New DataSet With {
                    .Name = a.Name,
                    .Value = ca
                }
            Next

            Return outMatrix
        End Function
    End Module
End Namespace
