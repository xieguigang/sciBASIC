Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

'  (C) Copyright 2005, Gregor Heinrich (gregor :: arbylon : net) (This file is
'  part of the org.knowceans experimental software packages.)
' 
'  LdaGibbsSampler is free software; you can redistribute it and/or modify it
'  under the terms of the GNU General Public License as published by the Free
'  Software Foundation; either version 2 of the License, or (at your option) any
'  later version.
' 
'  LdaGibbsSampler is distributed in the hope that it will be useful, but
'  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
'  FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
'  details.
' 
'  You should have received a copy of the GNU General Public License along with
'  this program; if not, write to the Free Software Foundation, Inc., 59 Temple
'  Place, Suite 330, Boston, MA 02111-1307 USA
' 
'  Created on Mar 6, 2005
' 
Namespace LDA

    ''' <summary>
    ''' Gibbs sampler for estimating the best assignments of topics for words and
    ''' documents in a corpus. The algorithm is introduced in Tom Griffiths' paper
    ''' "Gibbs sampling in the generative model of Latent Dirichlet Allocation"
    ''' (2002).
    ''' Gibbs sampler采样算法的实现
    ''' 
    ''' @author heinrich
    ''' </summary>
    Public Class LdaGibbsSampler

        ''' <summary>
        ''' document data (term lists)
        ''' 文档
        ''' </summary>  
        Friend documents As Integer()()

        ''' <summary>
        ''' vocabulary size
        ''' 词表大小
        ''' </summary> 
        Friend V As Integer

        ''' <summary>
        ''' number of topics
        ''' 主题数目
        ''' </summary> 
        Friend K As Integer

        ''' <summary>
        ''' Dirichlet parameter (document--topic associations)
        ''' 文档——主题参数
        ''' </summary> 
        Friend alpha As Double = 2.0

        ''' <summary>
        ''' Dirichlet parameter (topic--term associations)
        ''' 主题——词语参数
        ''' </summary>
        Friend beta As Double = 0.5

        ''' <summary>
        ''' topic assignments for each word.
        ''' 每个词语的主题 z[i][j] := 文档i的第j个词语的主题编号
        ''' </summary>
        Friend z As Integer()()

        ''' <summary>
        ''' cwt[i][j] number of instances of word i (term?) assigned to topic j.
        ''' 计数器，nw[i][j] := 词语i归入主题j的次数
        ''' </summary>
        Friend nw As Integer()()

        ''' <summary>
        ''' na[i][j] number of words in document i assigned to topic j.
        ''' 计数器，nd[i][j] := 文档[i]中归入主题j的词语的个数
        ''' </summary> 
        Friend nd As Integer()()

        ''' <summary>
        ''' nwsum[j] total number of words assigned to topic j.
        ''' 计数器，nwsum[j] := 归入主题j词语的个数
        ''' </summary>
        Friend nwsum As Integer()

        ''' <summary>
        ''' nasum[i] total number of words in document i.
        ''' 计数器,ndsum[i] := 文档i中全部词语的数量
        ''' </summary>
        Friend ndsum As Integer()


        ''' <summary>
        ''' cumulative statistics of theta
        ''' theta的累积量
        ''' </summary>
        Friend thetasum As Double()()

        ''' <summary>
        ''' cumulative statistics of phi
        ''' phi的累积量
        ''' </summary>
        Friend phisum As Double()()

        ''' <summary>
        ''' size of statistics
        ''' 样本容量
        ''' </summary>
        Friend numstats As Integer

        ''' <summary>
        ''' sampling lag (?)
        ''' 多久更新一次统计量
        ''' </summary> 
        Private Shared THIN_INTERVAL As Integer = 20

        ''' <summary>
        ''' burn-in period
        ''' 收敛前的迭代次数
        ''' </summary>
        Private Shared BURN_IN As Integer = 100

        ''' <summary>
        ''' max iterations
        ''' 最大迭代次数
        ''' </summary>
        Private Shared ITERATIONS As Integer = 1000

        ''' <summary>
        ''' sample lag (if -1 only one sample taken)
        ''' 最后的模型个数（取收敛后的n个迭代的参数做平均可以使得模型质量更高）
        ''' </summary>
        Private Shared SAMPLE_LAG As Integer = 10
        Private Shared dispcol As Integer = 0

        ''' <summary>
        ''' Initialise the Gibbs sampler with data.
        ''' 用数据初始化采样器
        ''' </summary>
        ''' <param name="documents"> 文档 </param>
        ''' <param name="V">         vocabulary size 词表大小 </param> 
        Public Sub New(ByVal documents As Integer()(), ByVal V As Integer)
            Me.documents = documents
            Me.V = V
        End Sub

        ''' <summary>
        ''' Initialisation: Must start with an assignment of observations to topics ?
        ''' Many alternatives are possible, I chose to perform random assignments
        ''' with equal probabilities
        ''' 随机初始化状态
        ''' </summary>
        ''' <param name="K"> number of topics K个主题 </param>
        Public Overridable Sub initialState(ByVal K As Integer)
            Dim lM = documents.Length

            ' initialise count variables. 初始化计数器
            nw = MAT(Of Integer)(V, K)
            nd = MAT(Of Integer)(lM, K)
            nwsum = New Integer(K - 1) {}
            ndsum = New Integer(lM - 1) {}

            ' The z_i are are initialised to values in [1,K] to determine the
            ' initial state of the Markov chain.

            z = New Integer(lM - 1)() {} ' z_i := 1到K之间的值，表示马氏链的初始状态

            For m = 0 To lM - 1
                Dim lN = documents(m).Length
                z(m) = New Integer(lN - 1) {}

                For n = 0 To lN - 1
                    Dim topic As Integer = randf.NextInteger(K)
                    z(m)(n) = topic
                    ' number of instances of word i assigned to topic j
                    nw(documents(m)(n))(topic) += 1
                    ' number of words in document i assigned to topic j.
                    nd(m)(topic) += 1
                    ' total number of words assigned to topic j.
                    nwsum(topic) += 1
                Next
                ' total number of words in document i
                ndsum(m) = lN
            Next
        End Sub

        Public Overridable Sub gibbs(ByVal K As Integer)
            gibbs(K, 2.0, 0.5)
        End Sub

        ''' <summary>
        ''' Main method: Select initial state ? Repeat a large number of times: 1.
        ''' Select an element 2. Update conditional on other elements. If
        ''' appropriate, output summary for each run.
        ''' 采样
        ''' </summary>
        ''' <param name="K">     number of topics 主题数 </param>
        ''' <param name="alpha"> symmetric prior parameter on document--topic associations 对称文档——主题先验概率？ </param>
        ''' <param name="beta">  symmetric prior parameter on topic--term associations 对称主题——词语先验概率？ </param> 
        Public Overridable Sub gibbs(ByVal K As Integer, ByVal alpha As Double, ByVal beta As Double)
            Me.K = K
            Me.alpha = alpha
            Me.beta = beta

            ' init sampler statistics  分配内存
            If SAMPLE_LAG > 0 Then
                thetasum = MAT(Of Double)(documents.Length, K)
                phisum = MAT(Of Double)(K, V)
                numstats = 0
            End If

            ' initial state of the Markov chain:
            initialState(K)
            Console.WriteLine("Sampling " & ITERATIONS & " iterations with burn-in of " & BURN_IN & " uniquetempvar.")

            For i = 0 To ITERATIONS - 1

                ' for all z_i
                For m = 0 To z.Length - 1

                    For n = 0 To z(m).Length - 1

                        ' (z_i = z[m][n])
                        ' sample from p(z_i|z_-i, w)
                        Dim topic = sampleFullConditional(m, n)
                        z(m)(n) = topic
                    Next
                Next

                If i < BURN_IN AndAlso i Mod THIN_INTERVAL = 0 Then
                    Console.Write("B")
                    dispcol += 1
                End If
                ' display progress
                If i > BURN_IN AndAlso i Mod THIN_INTERVAL = 0 Then
                    Console.Write("S")
                    dispcol += 1
                End If
                ' get statistics after burn-in
                If i > BURN_IN AndAlso SAMPLE_LAG > 0 AndAlso i Mod SAMPLE_LAG = 0 Then
                    updateParams()
                    Console.Write("|")

                    If i Mod THIN_INTERVAL <> 0 Then
                        dispcol += 1
                    End If
                End If

                If dispcol >= 100 Then
                    Console.WriteLine()
                    dispcol = 0
                End If
            Next

            Console.WriteLine()
        End Sub

        ''' <summary>
        ''' Sample a topic z_i from the full conditional distribution: p(z_i = j |
        ''' z_-i, w) = (n_-i,j(w_i) + beta)/(n_-i,j(.) + W * beta) * (n_-i,j(d_i) +
        ''' alpha)/(n_-i,.(d_i) + K * alpha) 
        ''' 根据上述公式计算文档m中第n个词语的主题的完全条件分布，输出最可能的主题
        ''' </summary>
        ''' <param name="m"> document </param>
        ''' <param name="n"> word </param> 
        Private Function sampleFullConditional(ByVal m As Integer, ByVal n As Integer) As Integer

            ' remove z_i from the count variables  先将这个词从计数器中抹掉
            Dim topic = z(m)(n)
            nw(documents(m)(n))(topic) -= 1
            nd(m)(topic) -= 1
            nwsum(topic) -= 1
            ndsum(m) -= 1

            ' do multinomial sampling via cumulative method: 通过多项式方法采样多项式分布
            Dim p = New Double(K - 1) {}

            For K = 0 To Me.K - 1
                p(K) = (nw(documents(m)(n))(K) + beta) / (nwsum(K) + V * beta) * (nd(m)(K) + alpha) / (ndsum(m) + Me.K * alpha)
            Next
            ' cumulate multinomial parameters  累加多项式分布的参数
            For K = 1 To p.Length - 1
                p(K) += p(K - 1)
            Next
            ' scaled sample because of unnormalised p[] 正则化
            Dim u = randf.NextDouble * p(K - 1)
            topic = 0

            Do While topic < p.Length - 1
                If u < p(topic) Then
                    Exit Do
                Else
                    topic += 1
                End If
            Loop

            ' add newly estimated z_i to count variables   将重新估计的该词语加入计数器
            nw(documents(m)(n))(topic) += 1
            nd(m)(topic) += 1
            nwsum(topic) += 1
            ndsum(m) += 1
            Return topic
        End Function

        ''' <summary>
        ''' Add to the statistics the values of theta and phi for the current state.
        ''' 更新参数
        ''' </summary>
        Private Sub updateParams()
            For m = 0 To documents.Length - 1

                For K = 0 To Me.K - 1
                    thetasum(m)(K) += (nd(m)(K) + alpha) / (ndsum(m) + Me.K * alpha)
                Next
            Next

            For K = 0 To Me.K - 1

                For w = 0 To V - 1
                    phisum(K)(w) += (nw(w)(K) + beta) / (nwsum(K) + V * beta)
                Next
            Next

            numstats += 1
        End Sub

        ''' <summary>
        ''' Retrieve estimated document--topic associations. If sample lag > 0 then
        ''' the mean value of all sampled statistics for theta[][] is taken.
        ''' 获取文档——主题矩阵
        ''' </summary>
        ''' <returns> theta multinomial mixture of document topics (M x K) </returns>
        Public Overridable ReadOnly Property Theta As Double()()
            Get
                Dim lTheta = MAT(Of Double)(documents.Length, K)

                If SAMPLE_LAG > 0 Then
                    For m = 0 To documents.Length - 1

                        For K = 0 To Me.K - 1
                            lTheta(m)(K) = thetasum(m)(K) / numstats
                        Next
                    Next
                Else

                    For m = 0 To documents.Length - 1

                        For K = 0 To Me.K - 1
                            lTheta(m)(K) = (nd(m)(K) + alpha) / (ndsum(m) + Me.K * alpha)
                        Next
                    Next
                End If

                Return lTheta
            End Get
        End Property

        ''' <summary>
        ''' Retrieve estimated topic--word associations. If sample lag > 0 then the
        ''' mean value of all sampled statistics for phi[][] is taken.
        ''' 获取主题——词语矩阵
        ''' </summary>
        ''' <returns> phi multinomial mixture of topic words (K x V) </returns>
        Public Overridable ReadOnly Property Phi As Double()()
            Get
                Dim lPhi = MAT(Of Double)(K, V)

                If SAMPLE_LAG > 0 Then
                    For K = 0 To Me.K - 1

                        For w = 0 To V - 1
                            lPhi(K)(w) = phisum(K)(w) / numstats
                        Next
                    Next
                Else

                    For K = 0 To Me.K - 1

                        For w = 0 To V - 1
                            lPhi(K)(w) = (nw(w)(K) + beta) / (nwsum(K) + V * beta)
                        Next
                    Next
                End If

                Return lPhi
            End Get
        End Property

        ''' <summary>
        ''' Print table of multinomial data
        ''' </summary>
        ''' <param name="data"> vector of evidence </param>
        ''' <param name="fmax"> max frequency in display </param>
        ''' <return> the scaled histogram bin values </return>
        Public Shared Sub hist(ByVal data As Double(), ByVal fmax As Integer)
            Dim lHist = New Double(data.Length - 1) {}
            ' scale maximum
            Dim hmax As Double = 0

            For i = 0 To data.Length - 1
                hmax = stdNum.Max(data(i), hmax)
            Next

            Dim shrink = fmax / hmax

            For i = 0 To data.Length - 1
                lHist(i) = shrink * data(i)
            Next

            Dim scale = ""

            For i As Integer = 1 To fmax / 10 + 1 - 1
                scale += "    .    " & i Mod 10
            Next

            Console.WriteLine("x" & (hmax / fmax).ToString("F2") & vbTab & "0" & scale)

            For i = 0 To lHist.Length - 1
                Console.Write(i & vbTab & "|")

                For j As Integer = 0 To stdNum.Round(lHist(i)) - 1

                    If (j + 1) Mod 10 = 0 Then
                        Console.Write("]")
                    Else
                        Console.Write("|")
                    End If
                Next

                Console.WriteLine()
            Next
        End Sub

        ''' <summary>
        ''' Configure the gibbs sampler
        ''' 配置采样器
        ''' </summary>
        ''' <param name="iterations">   number of total iterations </param>
        ''' <param name="burnIn">       number of burn-in iterations </param>
        ''' <param name="thinInterval"> update statistics interval </param>
        ''' <param name="sampleLag">    sample interval (-1 for just one sample at the end) </param>
        Public Overridable Sub configure(ByVal iterations As Integer, ByVal burnIn As Integer, ByVal thinInterval As Integer, ByVal sampleLag As Integer)
            LdaGibbsSampler.ITERATIONS = iterations
            BURN_IN = burnIn
            THIN_INTERVAL = thinInterval
            SAMPLE_LAG = sampleLag
        End Sub

        ''' <summary>
        ''' Inference a new document by a pre-trained phi matrix
        ''' </summary>
        ''' <param name="phi"> pre-trained phi matrix </param>
        ''' <param name="doc"> document </param>
        ''' <returns> a p array </returns>
        Public Shared Function inference(ByVal alpha As Double, ByVal beta As Double, ByVal phi As Double()(), ByVal doc As Integer()) As Double()
            Dim lK = phi.Length
            Dim V = phi(0).Length
            ' init

            ' initialise count variables. 初始化计数器          
            Dim nw = MAT(Of Integer)(V, lK)
            Dim nd = New Integer(lK - 1) {}
            Dim nwsum = New Integer(lK - 1) {}
            Dim ndsum = 0

            ' The z_i are are initialised to values in [1,K] to determine the
            ' initial state of the Markov chain.

            Dim N = doc.Length
            Dim z = New Integer(N - 1) {} ' z_i := 1到K之间的值，表示马氏链的初始状态

            For N = 0 To N - 1
                Dim topic As Integer = randf.NextInteger(lK)
                z(N) = topic
                ' number of instances of word i assigned to topic j
                nw(doc(N))(topic) += 1
                ' number of words in document i assigned to topic j.
                nd(topic) += 1
                ' total number of words assigned to topic j.
                nwsum(topic) += 1
            Next
            ' total number of words in document i
            ndsum = N

            For i = 0 To ITERATIONS - 1

                For N = 0 To z.Length - 1

                    ' (z_i = z[m][n])
                    ' sample from p(z_i|z_-i, w)
                    ' remove z_i from the count variables  先将这个词从计数器中抹掉
                    Dim topic = z(N)
                    nw(doc(N))(topic) -= 1
                    nd(topic) -= 1
                    nwsum(topic) -= 1
                    ndsum -= 1

                    ' do multinomial sampling via cumulative method: 通过多项式方法采样多项式分布
                    Dim p = New Double(lK - 1) {}

                    For K As Integer = 0 To lK - 1
                        p(K) = phi(K)(doc(N)) * (nd(K) + alpha) / (ndsum + lK * alpha)
                    Next
                    ' cumulate multinomial parameters  累加多项式分布的参数
                    For K As Integer = 1 To p.Length - 1
                        p(K) += p(K - 1)
                    Next
                    ' scaled sample because of unnormalised p[] 正则化
                    Dim u = randf.NextDouble * p(lK - 1)

                    For topic = 0 To p.Length - 1

                        If u < p(topic) Then
                            Exit For
                        End If
                    Next

                    If topic = lK Then
                        Throw New Exception("the param K or topic is set too small")
                    End If
                    ' add newly estimated z_i to count variables   将重新估计的该词语加入计数器
                    nw(doc(N))(topic) += 1
                    nd(topic) += 1
                    nwsum(topic) += 1
                    ndsum += 1
                    z(N) = topic
                Next
            Next

            Dim lTheta = New Double(lK - 1) {}

            For K As Integer = 0 To lK - 1
                lTheta(K) = (nd(K) + alpha) / (ndsum + lK * alpha)
            Next

            Return lTheta
        End Function

        Public Shared Function inference(ByVal phi As Double()(), ByVal doc As Integer()) As Double()
            Return inference(2.0, 0.5, phi, doc)
        End Function

        Friend Shared shades As String() = New String() {"     ", ".    ", ":    ", ":.   ", "::   ", "::.  ", ":::  ", ":::. ", ":::: ", "::::.", ":::::"}

        ''' <summary>
        ''' create a string representation whose gray value appears as an indicator
        ''' of magnitude, cf. Hinton diagrams in statistics.
        ''' </summary>
        ''' <param name="d">   value </param>
        ''' <param name="max"> maximum value
        ''' @return </param>
        Public Shared Function shadeDouble(ByVal d As Double, ByVal max As Double) As String
            Dim a As Integer = stdNum.Floor(d * 10 / max + 0.5)

            If a > 10 OrElse a < 0 Then
                Dim x = d.ToString("G2")
                a = 5 - x.Length

                For i = 0 To a - 1
                    x += " "
                Next

                Return "<" & x & ">"
            End If

            Return "[" & shades(a) & "]"
        End Function
    End Class
End Namespace
