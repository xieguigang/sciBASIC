#Region "Microsoft.VisualBasic::38df023f1c3c8ee8c536c0ff8e753eb8, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\complex\ComplEx.vb"

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

    '   Total Lines: 191
    '    Code Lines: 165 (86.39%)
    ' Comment Lines: 7 (3.66%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 19 (9.95%)
    '     File Size: 10.78 KB


    '     Class ComplEx
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: initialization, learn
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.util

Namespace GraphEmbedding.complex

    ''' <summary>
    ''' ComplEx (triple only)
    ''' </summary>
    <Description("ComplEx (triple only)")>
    Public Class ComplEx : Inherits Algorithm

        Public m_TrainTriples As TripleSet
        Public m_ValidTriples As TripleSet
        Public m_TestTriples As TripleSet
        Public m_AllTriples As TripleDict
        Public m_Real_MatrixE As Matrix
        Public m_Real_MatrixR As Matrix
        Public m_Imag_MatrixE As Matrix
        Public m_Imag_MatrixR As Matrix
        Public m_Real_MatrixEGradient As Matrix
        Public m_Real_MatrixRGradient As Matrix
        Public m_Imag_MatrixEGradient As Matrix
        Public m_Imag_MatrixRGradient As Matrix
        Public m_Real_MatrixEGSquare As Matrix
        Public m_Real_MatrixRGSquare As Matrix
        Public m_Imag_MatrixEGSquare As Matrix
        Public m_Imag_MatrixRGSquare As Matrix

        Public m_NumRelation As Integer
        Public m_NumEntity As Integer
        Public m_MatrixE_prefix As String = ""
        Public m_MatrixR_prefix As String = ""

        Public Sub New()
        End Sub

        Public Overrides Sub initialization(strNumRelation As String, strNumEntity As String, fnTrainTriples As String, fnValidTriples As String, fnTestTriples As String, fnAllTriples As String, other As Dictionary(Of String, String))
            m_NumRelation = Integer.Parse(strNumRelation)
            m_NumEntity = Integer.Parse(strNumEntity)
            m_MatrixE_prefix = "model/MatrixE-k" & m_NumFactor.ToString() & "-lmbda" & m_Lambda.ToString("F5") & "-gamma" & m_Gamma.ToString("F5") & "-neg" & m_NumNegative.ToString()
            m_MatrixR_prefix = "model/MatrixR-k" & m_NumFactor.ToString() & "-lmbda" & m_Lambda.ToString("F5") & "-gamma" & m_Gamma.ToString("F5") & "-neg" & m_NumNegative.ToString()

            Console.WriteLine(vbLf & "Loading train, valid, test, and all triples")
            m_TrainTriples = New TripleSet(m_NumEntity, m_NumRelation)
            m_ValidTriples = New TripleSet(m_NumEntity, m_NumRelation)
            m_TestTriples = New TripleSet(m_NumEntity, m_NumRelation)
            m_TrainTriples.load(fnTrainTriples, -1)
            m_ValidTriples.load(fnValidTriples, 1000)
            m_TestTriples.load(fnTestTriples, -1)
            m_AllTriples = New TripleDict()
            m_AllTriples.load(fnAllTriples)
            Console.WriteLine("# train triples: " & m_TrainTriples.triples().ToString())
            Console.WriteLine("# valid triples: " & m_ValidTriples.triples().ToString())
            Console.WriteLine("# test triples: " & m_TestTriples.triples().ToString())
            Console.WriteLine("# all triples: " & m_AllTriples.tripleDict().Count.ToString())
            Console.WriteLine("Success.")


            Console.WriteLine(vbLf & "Initializing (real/imaginary) matrix E and matrix R")
            m_Real_MatrixE = New Matrix(m_NumEntity, m_NumFactor)
            m_Real_MatrixE.initializeGaussian()
            m_Real_MatrixR = New Matrix(m_NumRelation, m_NumFactor)
            m_Real_MatrixR.initializeGaussian()
            m_Imag_MatrixE = New Matrix(m_NumEntity, m_NumFactor)
            m_Imag_MatrixE.initializeGaussian()
            m_Imag_MatrixR = New Matrix(m_NumRelation, m_NumFactor)
            m_Imag_MatrixR.initializeGaussian()
            Console.WriteLine("Success.")

            Console.WriteLine(vbLf & "Initializing gradients/gradient squares of matrix E and matrix R")
            m_Real_MatrixEGradient = New Matrix(m_NumEntity, m_NumFactor)
            m_Real_MatrixRGradient = New Matrix(m_NumRelation, m_NumFactor)
            m_Imag_MatrixEGradient = New Matrix(m_NumEntity, m_NumFactor)
            m_Imag_MatrixRGradient = New Matrix(m_NumRelation, m_NumFactor)
            m_Real_MatrixEGSquare = New Matrix(m_NumEntity, m_NumFactor)
            m_Real_MatrixRGSquare = New Matrix(m_NumRelation, m_NumFactor)
            m_Imag_MatrixEGSquare = New Matrix(m_NumEntity, m_NumFactor)
            m_Imag_MatrixRGSquare = New Matrix(m_NumRelation, m_NumFactor)
            Console.WriteLine("Success.")
        End Sub

        Public Overrides Sub learn()
            Dim PATHLOG As String = "log/log-k" & m_NumFactor.ToString() & "-lmbda" & m_Lambda.ToString("F5") & "-gamma" & m_Gamma.ToString("F5") & "-neg" & m_NumNegative.ToString() & ".txt"
            Dim writer As New StreamWriter(PATHLOG.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False), Encoding.UTF8)

            Dim lstPosTriples As New Dictionary(Of Integer, List(Of Triple))()
            Dim lstHeadNegTriples As New Dictionary(Of Integer, List(Of Triple))()
            Dim lstTailNegTriples As New Dictionary(Of Integer, List(Of Triple))()

            Dim iCurIter = 0
            Dim dCurMRR = 0.0
            Dim iBestIter = -1
            Dim dBestMRR = -1.0
            While iCurIter < m_NumIteration
                m_TrainTriples.randomShuffle()
                For iIndex = 0 To m_TrainTriples.triples() - 1
                    Dim PosTriple = m_TrainTriples.get(iIndex)
                    Dim negTripGen As NegativeTripleGenerator = New NegativeTripleGenerator(PosTriple, m_NumEntity, m_NumRelation)
                    Dim headNegTripleSet = negTripGen.generateHeadNegTriple(m_NumNegative / 2)
                    Dim tailNegTripleSet = negTripGen.generateTailNegTriple(m_NumNegative / 2)

                    Dim iID = iIndex Mod m_NumBatch
                    If Not lstPosTriples.ContainsKey(iID) Then
                        Dim tmpPosLst As List(Of Triple) = New List(Of Triple)()
                        Dim tmpHeadNegLst As List(Of Triple) = New List(Of Triple)()
                        Dim tmpTailNegLst As List(Of Triple) = New List(Of Triple)()
                        tmpPosLst.Add(PosTriple)
                        tmpHeadNegLst.AddRange(headNegTripleSet)
                        tmpTailNegLst.AddRange(tailNegTripleSet)
                        lstPosTriples(iID) = tmpPosLst
                        lstHeadNegTriples(iID) = tmpHeadNegLst
                        lstTailNegTriples(iID) = tmpTailNegLst
                    Else
                        lstPosTriples(iID).Add(PosTriple)
                        lstHeadNegTriples(iID).AddRange(headNegTripleSet)
                        lstTailNegTriples(iID).AddRange(tailNegTripleSet)
                    End If
                Next

                For Each iID As Integer In Tqdm.Range(0, m_NumBatch, wrap_console:=App.EnableTqdm)
                    Dim adagrad As New AdaGrad(
                        lstPosTriples(iID), lstHeadNegTriples(iID), lstTailNegTriples(iID),
                        m_Real_MatrixE,
                        m_Real_MatrixR,
                        m_Imag_MatrixE,
                        m_Imag_MatrixR,
                        m_Real_MatrixEGradient,
                        m_Real_MatrixRGradient,
                        m_Imag_MatrixEGradient,
                        m_Imag_MatrixRGradient,
                        m_Real_MatrixEGSquare,
                        m_Real_MatrixRGSquare,
                        m_Imag_MatrixEGSquare,
                        m_Imag_MatrixRGSquare,
                        m_Gamma, m_Lambda)

                    adagrad.gradientDescent()
                Next
                lstPosTriples = New Dictionary(Of Integer, List(Of Triple))()
                lstHeadNegTriples = New Dictionary(Of Integer, List(Of Triple))()
                lstTailNegTriples = New Dictionary(Of Integer, List(Of Triple))()
                iCurIter += 1
                Console.WriteLine("Complete iteration #" & iCurIter.ToString())

                If iCurIter Mod m_OutputIterSkip = 0 Then
                    writer.Write("Complete iteration #" & iCurIter.ToString() & ":" & vbLf)
                    'Dim eval As Evaluation = New Evaluation(m_ValidTriples, m_AllTriples.tripleDict(), m_Real_MatrixE, m_Real_MatrixR, m_Imag_MatrixE, m_Imag_MatrixR)
                    'Eval.calculateMetrics()
                    'dCurMRR = eval.dMRR
                    dCurMRR = 1
                    ' writer.Write("------Current iteration #" & iCurIter.ToString() & vbTab & dCurMRR.ToString() & vbTab & Eval.dHits10.ToString() & vbLf)
                    If dCurMRR > dBestMRR Then
                        m_Real_MatrixE.output(m_MatrixE_prefix & ".real")
                        m_Real_MatrixR.output(m_MatrixR_prefix & ".real")
                        m_Imag_MatrixE.output(m_MatrixE_prefix & ".imag")
                        m_Imag_MatrixR.output(m_MatrixR_prefix & ".imag")
                        dBestMRR = dCurMRR
                        iBestIter = iCurIter
                    End If
                    writer.Write("------Best iteration #" & iBestIter.ToString() & vbTab & dBestMRR.ToString() & vbLf)
                    writer.Flush()
                End If
            End While

            m_Real_MatrixE = New Matrix(m_NumEntity, m_NumFactor)
            m_Real_MatrixR = New Matrix(m_NumRelation, m_NumFactor)
            m_Imag_MatrixE = New Matrix(m_NumEntity, m_NumFactor)
            m_Imag_MatrixR = New Matrix(m_NumRelation, m_NumFactor)
            m_Real_MatrixE.load(m_MatrixE_prefix & ".real")
            m_Real_MatrixR.load(m_MatrixR_prefix & ".real")
            m_Imag_MatrixE.load(m_MatrixE_prefix & ".imag")
            m_Imag_MatrixR.load(m_MatrixR_prefix & ".imag")
            Dim testEval As Evaluation = New Evaluation(m_TestTriples, m_AllTriples.tripleDict(), m_Real_MatrixE, m_Real_MatrixR, m_Imag_MatrixE, m_Imag_MatrixR)
            testEval.calculateMetrics()
            writer.Write("################################################" & vbLf)
            writer.Write("MRR:" & vbTab & testEval.dMRR.ToString() & vbLf)
            writer.Write("MeanRank:" & vbTab & testEval.dMeanRank.ToString() & vbLf)
            writer.Write("Median:" & vbTab & testEval.dMedian.ToString() & vbLf)
            writer.Write("Hits@1:" & vbTab & testEval.dHits1.ToString() & vbLf)
            writer.Write("Hits@3:" & vbTab & testEval.dHits3.ToString() & vbLf)
            writer.Write("Hits@5:" & vbTab & testEval.dHits5.ToString() & vbLf)
            writer.Write("Hits@10:" & vbTab & testEval.dHits10.ToString() & vbLf)
            writer.Close()
        End Sub
    End Class

End Namespace
