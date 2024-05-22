#Region "Microsoft.VisualBasic::784f1e218f53cd83a82567ee8982c527, Data_science\NLP\LDA\ParallelGibbsLda\ParallelGibbsLda.vb"

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
    '    Code Lines: 129 (59.17%)
    ' Comment Lines: 50 (22.94%)
    '    - Xml Docs: 96.00%
    ' 
    '   Blank Lines: 39 (17.89%)
    '     File Size: 7.04 KB


    '     Class ParallelGibbsLda
    ' 
    '         Properties: phi, theta
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: calcPhiMatrix, calcThetaMatrix, perplexity
    ' 
    '         Sub: (+2 Overloads) gibbsSampling, initial
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace LDA


    ''' <summary>
    ''' Created by chenjianfeng on 2018/1/17.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/fishermanff/ParallelGibbsLda
    ''' </remarks>
    Public Class ParallelGibbsLda

        ''' <summary>
        ''' document-word matrix
        ''' </summary>
        Friend documents As Integer()()

        ''' <summary>
        ''' vocabulary size
        ''' </summary>
        Friend V As Integer

        ''' <summary>
        ''' number of topics
        ''' </summary>
        Friend K As Integer

        ''' <summary>
        ''' document->topic Dirichlet alpha
        ''' </summary>
        Friend alpha As Double

        ''' <summary>
        ''' topic->word Dirichlet beta
        ''' </summary>
        Friend beta As Double

        ''' <summary>
        ''' max iteration
        ''' </summary>
        Friend iter As Integer

        ''' <summary>
        ''' nd[m][k]: number of words in document m assigned to topic k; size M * K
        ''' </summary>
        Friend nd As Integer()()

        ''' <summary>
        ''' ndsum[m]: total number of words in document m; size M
        ''' </summary>
        Friend ndsum As Integer()

        ''' <summary>
        ''' nw[i][k]: number of instances of word i assigned to topic k; size V * K
        ''' </summary>
        Friend nw As Integer()()

        ''' <summary>
        ''' nwsum[k]: total number of words assigned to topic k; size K
        ''' </summary>
        Friend nwsum As Integer()

        ''' <summary>
        ''' z[m][i]: the assigned topic of word i in document m
        ''' </summary>
        Friend z As Integer()()

        ''' <summary>
        ''' store estimated theta matrix
        ''' </summary>
        Public ReadOnly Property theta As Double()()

        ''' <summary>
        ''' store estimated phi matrix
        ''' </summary>
        Public ReadOnly Property phi As Double()()

        ''' <summary>
        ''' store perplexity
        ''' </summary>
        Private m_perplexity As Double = -1

        Public Sub New(documents As Integer()(), V As Integer)
            Me.V = V
            Me.documents = documents _
                .Select(Function(page) page.ToArray) _
                .ToArray
        End Sub

        Private Sub initial()
            Dim lM = documents.Length
            Dim N As Integer
            Dim topic As Integer

            z = New Integer(lM - 1)() {}
            nd = RectangularArray.Matrix(Of Integer)(lM, K)
            ndsum = New Integer(lM - 1) {}
            nw = RectangularArray.Matrix(Of Integer)(V, K)
            nwsum = New Integer(K - 1) {}

            For m As Integer = 0 To lM - 1
                N = documents(m).Length
                z(m) = New Integer(N - 1) {}

                For i As Integer = 0 To N - 1
                    topic = randf.Next(K)
                    z(m)(i) = topic
                    nd(m)(topic) += 1
                    nw(documents(m)(i))(topic) += 1
                    nwsum(topic) += 1
                Next

                ndsum(m) = N
            Next
        End Sub

        Public Overridable Sub gibbsSampling(K As Integer, alpha As Double, beta As Double, iter As Integer, threads As Integer)
            gibbsSampling(K, alpha, beta, iter, threads, 47)
        End Sub

        Public Overridable Sub gibbsSampling(K As Integer, alpha As Double, beta As Double, iter As Integer, threads As Integer, seed As Integer)
            Me.K = K
            Me.alpha = alpha
            Me.beta = beta
            Me.iter = iter
            Dim gibbsWorks = New GibbsWorker(threads - 1) {}
            Dim pieceSize As Integer = documents.Length / threads
            Dim i = 0, offset = 0

            Call initial()

            While i < threads
                If i = threads - 1 Then
                    pieceSize = documents.Length - offset
                End If

                gibbsWorks(i) = New GibbsWorker(Me, offset, offset + pieceSize - 1)
                offset += pieceSize
                i += 1
            End While

            Dim executor As New ExecutorService(gibbsWorks, workers:=threads)
            Dim nwDelta As Integer
            Dim wordCount As Integer

            For Each it As Integer In Tqdm.Range(0, iter)
                Call executor.Run()

                ' reduce result of each thread and update global nw, nwsum array
                For topic As Integer = 0 To K - 1
                    wordCount = 0

                    For word As Integer = 0 To V - 1
                        nwDelta = 0

                        For cpu_id As Integer = 0 To threads - 1
                            nwDelta += gibbsWorks(cpu_id).nw(word)(topic) - nw(word)(topic)
                        Next

                        nw(word)(topic) += nwDelta
                        wordCount += nw(word)(topic)
                    Next

                    nwsum(topic) = wordCount
                Next
            Next

            ' store theta and phi matrix after estimation
            _theta = calcThetaMatrix()
            _phi = calcPhiMatrix()
        End Sub

        Private Function calcThetaMatrix() As Double()()
            Dim lM = documents.Length
            Dim theta = RectangularArray.Matrix(Of Double)(lM, K)
            For m = 0 To lM - 1
                For topic = 0 To K - 1
                    theta(m)(topic) = (nd(m)(topic) + alpha) / (ndsum(m) + K * alpha)
                Next
            Next
            Return theta
        End Function

        Private Function calcPhiMatrix() As Double()()
            Dim M = documents.Length
            Dim phi = RectangularArray.Matrix(Of Double)(K, V)
            For w = 0 To V - 1
                For topic = 0 To K - 1
                    phi(topic)(w) = (nw(w)(topic) + beta) / (nwsum(topic) + V * beta)
                Next
            Next
            Return phi
        End Function

        Public Overridable Function perplexity() As Double
            If m_perplexity <> -1 Then
                Return m_perplexity
            End If
            Dim num As Double = 0
            Dim den As Double = 0
            Dim lM = documents.Length
            For m = 0 To lM - 1
                Dim N = documents(m).Length
                For i = 0 To N - 1
                    Dim topic = z(m)(i)
                    num -= std.Log(theta(m)(topic) * phi(topic)(documents(m)(i)))
                Next
                den += N
            Next
            m_perplexity = std.Exp(num / den)
            Return m_perplexity
        End Function
    End Class
End Namespace
