#Region "Microsoft.VisualBasic::2b722e0129563a79efa5b00a6daed107, Data_science\NLP\LDA\LdaGibbsSampler.vb"

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

    '   Total Lines: 389
    '    Code Lines: 168 (43.19%)
    ' Comment Lines: 168 (43.19%)
    '    - Xml Docs: 79.76%
    ' 
    '   Blank Lines: 53 (13.62%)
    '     File Size: 14.94 KB


    '     Class LdaGibbsSampler
    ' 
    '         Properties: K, Phi, Theta
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: configure
    ' 
    '         Sub: (+2 Overloads) gibbs, initialState, sampling, update_params
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Parallel
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

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
    ''' <remarks>
    ''' https://github.com/hankcs/LDA4j
    ''' </remarks>
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
        Public ReadOnly Property K As Integer

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
        Friend Shared THIN_INTERVAL As Integer = 20

        ''' <summary>
        ''' burn-in period
        ''' 收敛前的迭代次数
        ''' </summary>
        Private Shared BURN_IN As Integer = 100

        ''' <summary>
        ''' max iterations
        ''' 最大迭代次数
        ''' </summary>
        Friend Shared ITERATIONS As Integer = 1000

        ''' <summary>
        ''' sample lag (if -1 only one sample taken)
        ''' 最后的模型个数（取收敛后的n个迭代的参数做平均可以使得模型质量更高）
        ''' </summary>
        Private Shared SAMPLE_LAG As Integer = 10

        ReadOnly println As Action(Of Object)
        ReadOnly counter As New ApplicationServices.PerformanceCounter

        ''' <summary>
        ''' Retrieve estimated document--topic associations. If sample lag > 0 then
        ''' the mean value of all sampled statistics for theta[][] is taken.
        ''' 获取文档——主题矩阵
        ''' </summary>
        ''' <returns> theta multinomial mixture of document topics (M x K) </returns>
        Public Overridable ReadOnly Property Theta As Double()()
            Get
                Dim lTheta = RectangularArray.Matrix(Of Double)(documents.Length, K)

                If SAMPLE_LAG > 0 Then
                    For m = 0 To documents.Length - 1
                        For K As Integer = 0 To Me.K - 1
                            lTheta(m)(K) = thetasum(m)(K) / numstats
                        Next
                    Next
                Else
                    For m = 0 To documents.Length - 1
                        For K As Integer = 0 To Me.K - 1
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
                Dim lPhi = RectangularArray.Matrix(Of Double)(K, V)

                If SAMPLE_LAG > 0 Then
                    For K As Integer = 0 To Me.K - 1
                        For w = 0 To V - 1
                            lPhi(K)(w) = phisum(K)(w) / numstats
                        Next
                    Next
                Else
                    For K As Integer = 0 To Me.K - 1
                        For w = 0 To V - 1
                            lPhi(K)(w) = (nw(w)(K) + beta) / (nwsum(K) + V * beta)
                        Next
                    Next
                End If

                Return lPhi
            End Get
        End Property

        ''' <summary>
        ''' Initialise the Gibbs sampler with data.
        ''' 用数据初始化采样器
        ''' </summary>
        ''' <param name="documents"> 文档 </param>
        ''' <param name="V">vocabulary size 词表大小 </param> 
        Public Sub New(documents As Integer()(), V As Integer, Optional log As Action(Of Object) = Nothing)
            Me.documents = documents
            Me.V = V
            Me.println = log

            If log Is Nothing Then
                Me.println = AddressOf VBDebugger.EchoLine
            End If
        End Sub

        ''' <summary>
        ''' Initialisation: Must start with an assignment of observations to topics ?
        ''' Many alternatives are possible, I chose to perform random assignments
        ''' with equal probabilities
        ''' 随机初始化状态
        ''' </summary>
        ''' <param name="K"> number of topics K个主题 </param>
        Private Sub initialState(K As Integer)
            Dim lM = documents.Length
            Dim lN As Integer
            Dim topic As Integer
            Dim t0 = Now

            Call println("allocating memory...")

            ' initialise count variables. 初始化计数器
            nw = RectangularArray.Matrix(Of Integer)(V, K)
            nd = RectangularArray.Matrix(Of Integer)(lM, K)
            nwsum = New Integer(K - 1) {}
            ndsum = New Integer(lM - 1) {}

            ' The z_i are are initialised to values in [1,K] to
            ' determine the initial state of the Markov chain.
            ' z_i := 1到K之间的值，表示马氏链的初始状态
            z = New Integer(lM - 1)() {}

            For m As Integer = 0 To lM - 1
                lN = documents(m).Length
                z(m) = New Integer(lN - 1) {}

                For n As Integer = 0 To lN - 1
                    topic = randf.NextInteger(K)
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

            Call println($"initial_markov_state[{(Now - t0).FormatTime}]")
        End Sub

        ''' <summary>
        ''' with default cutoff alpha=2.0 and beta=0.5
        ''' </summary>
        ''' <param name="K"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub gibbs(K As Integer)
            Call gibbs(K, 2.0, 0.5)
        End Sub

        ''' <summary>
        ''' for all z_i
        ''' do chain data sampling
        ''' </summary>
        ''' <param name="zi"></param>
        Private Sub sampling(zi As Integer)
            Dim v As Integer() = z(zi)
            Dim gibbs As New GibbsSamplingTask(v, zi, Me)

            If VectorTask.n_threads > 1 Then
                Call gibbs.Run()
            Else
                Call gibbs.Solve()
            End If
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
        Public Sub gibbs(K As Integer, alpha As Double, beta As Double)
            Me._K = K
            Me.alpha = alpha
            Me.beta = beta

            ' init sampler statistics  分配内存
            If SAMPLE_LAG > 0 Then
                thetasum = RectangularArray.Matrix(Of Double)(documents.Length, K)
                phisum = RectangularArray.Matrix(Of Double)(K, V)
                numstats = 0
            End If

            ' initial state of the Markov chain:
            Call initialState(K)
            Call println($"* Sampling {ITERATIONS} iterations with burn-in of {BURN_IN} unique temp var.")
            Call println($"* gibbs run in {If(VectorTask.n_threads <= 1, "sequential mode", $"parallel with {VectorTask.n_threads} CPU threads")}!")
            Call println($"z_index size={z.Length}")
            Call VBDebugger.WaitOutput()

            ' z is initialized after initialState is called
            Dim zIndex As Integer() = z.Sequence.ToArray
            Dim t0 As Date = Now
            Dim t1 As Date = Now
            Dim bar As Tqdm.ProgressBar = Nothing

            For Each i As Integer In Tqdm.Range(0, ITERATIONS, bar:=bar)
                Call counter.Set()

                ' for all z_i
                For Each m As Integer In zIndex '.AsParallel
                    Call sampling(zi:=m)
                Next

                Call counter.Mark("sampling")

                If i < BURN_IN AndAlso i Mod THIN_INTERVAL = 0 Then
                    t1 = Now
                    bar.SetLabel($"BURN_IN ... {StringFormats.ReadableElapsedTime((t1 - t0).TotalMilliseconds)}")
                End If
                ' display progress
                If i > BURN_IN AndAlso i Mod THIN_INTERVAL = 0 Then
                    t1 = Now
                    bar.SetLabel($" ... {StringFormats.ReadableElapsedTime((t1 - t0).TotalMilliseconds)}")
                End If
                ' get statistics after burn-in
                If i > BURN_IN AndAlso SAMPLE_LAG > 0 AndAlso i Mod SAMPLE_LAG = 0 Then
                    t1 = Now

                    Call counter.Mark("...")
                    Call update_params()
                    Call counter.Mark("update_pars")
                    Call bar.SetLabel($"get statistics after burn-in! {StringFormats.ReadableElapsedTime((t1 - t0).TotalMilliseconds)}")
                End If
            Next
        End Sub

        ''' <summary>
        ''' Add to the statistics the values of theta and phi for the current state.
        ''' 更新参数
        ''' </summary>
        Private Sub update_params()
            For m As Integer = 0 To documents.Length - 1
                For K As Integer = 0 To Me.K - 1
                    thetasum(m)(K) += (nd(m)(K) + alpha) / (ndsum(m) + Me.K * alpha)
                Next
            Next

            For K As Integer = 0 To Me.K - 1
                For w As Integer = 0 To V - 1
                    phisum(K)(w) += (nw(w)(K) + beta) / (nwsum(K) + V * beta)
                Next
            Next

            numstats += 1
        End Sub

        ''' <summary>
        ''' Configure the gibbs sampler
        ''' 配置采样器
        ''' </summary>
        ''' <param name="iterations">   number of total iterations </param>
        ''' <param name="burnIn">       number of burn-in iterations </param>
        ''' <param name="thinInterval"> update statistics interval </param>
        ''' <param name="sampleLag">    sample interval (-1 for just one sample at the end) </param>
        Public Function configure(iterations As Integer, burnIn As Integer, thinInterval As Integer, sampleLag As Integer) As LdaGibbsSampler
            LdaGibbsSampler.ITERATIONS = iterations
            BURN_IN = burnIn
            THIN_INTERVAL = thinInterval
            SAMPLE_LAG = sampleLag

            Return Me
        End Function
    End Class
End Namespace
