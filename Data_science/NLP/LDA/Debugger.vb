#Region "Microsoft.VisualBasic::0dda50e5aa9e2884c94b037fa8cce20a, Data_science\NLP\LDA\Debugger.vb"

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

    '   Total Lines: 176
    '    Code Lines: 105 (59.66%)
    ' Comment Lines: 35 (19.89%)
    '    - Xml Docs: 54.29%
    ' 
    '   Blank Lines: 36 (20.45%)
    '     File Size: 6.48 KB


    '     Module Debugger
    ' 
    '         Function: (+2 Overloads) inference, shadeDouble
    ' 
    '         Sub: hist
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace LDA

    Module Debugger

        ''' <summary>
        ''' Inference a new document by a pre-trained phi matrix
        ''' </summary>
        ''' <param name="phi"> pre-trained phi matrix </param>
        ''' <param name="doc"> document </param>
        ''' <returns> a p array </returns>
        Public Function inference(alpha As Double, beta As Double, phi As Double()(), doc As Integer()) As Double()
            Dim lK = phi.Length
            Dim V = phi(0).Length
            ' init

            ' initialise count variables. 初始化计数器          
            Dim nw = RectangularArray.Matrix(Of Integer)(V, lK)
            Dim nd = New Integer(lK - 1) {}
            Dim nwsum = New Integer(lK - 1) {}
            Dim ndsum = 0

            ' The z_i are are initialised to values in [1,K] to determine the
            ' initial state of the Markov chain.

            Dim Nl = doc.Length
            Dim z = New Integer(Nl - 1) {} ' z_i := 1到K之间的值，表示马氏链的初始状态

            For N As Integer = 0 To N - 1
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
            ndsum = Nl

            For i As Integer = 0 To LdaGibbsSampler.ITERATIONS - 1

                For N As Integer = 0 To z.Length - 1

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function inference(phi As Double()(), doc As Integer()) As Double()
            Return inference(2.0, 0.5, phi, doc)
        End Function

        ReadOnly shades As String() = New String() {"     ", ".    ", ":    ", ":.   ", "::   ", "::.  ", ":::  ", ":::. ", ":::: ", "::::.", ":::::"}

        ''' <summary>
        ''' create a string representation whose gray value appears as an indicator
        ''' of magnitude, cf. Hinton diagrams in statistics.
        ''' </summary>
        ''' <param name="d">   value </param>
        ''' <param name="max"> maximum value
        ''' @return </param>
        Public Function shadeDouble(d As Double, max As Double) As String
            Dim a As Integer = std.Floor(d * 10 / max + 0.5)

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

        ''' <summary>
        ''' Print table of multinomial data
        ''' </summary>
        ''' <param name="data"> vector of evidence </param>
        ''' <param name="fmax"> max frequency in display </param>
        ''' <return> the scaled histogram bin values </return>
        Public Sub hist(data As Double(), fmax As Integer)
            Dim lHist = New Double(data.Length - 1) {}
            ' scale maximum
            Dim hmax As Double = 0

            For i = 0 To data.Length - 1
                hmax = std.Max(data(i), hmax)
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

                For j As Integer = 0 To std.Round(lHist(i)) - 1

                    If (j + 1) Mod 10 = 0 Then
                        Console.Write("]")
                    Else
                        Console.Write("|")
                    End If
                Next

                Console.WriteLine()
            Next
        End Sub
    End Module
End Namespace
