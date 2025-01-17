#Region "Microsoft.VisualBasic::e6ecd19cab8bfb5bd347aaf56de84f68, Data_science\DataMining\DataMining\Evaluation\LabelEvaluate\FakeAUCGenerator.vb"

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

    '   Total Lines: 83
    '    Code Lines: 60 (72.29%)
    ' Comment Lines: 9 (10.84%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 14 (16.87%)
    '     File Size: 3.29 KB


    '     Module FakeAUCGenerator
    ' 
    '         Function: BuildOutput, BuildOutput2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Evaluation

    Public Module FakeAUCGenerator

        ''' <summary>
        ''' Create a fake output vector based on the target AUC
        ''' </summary>
        ''' <param name="labels"></param>
        ''' <param name="auc"></param>
        ''' <returns></returns>
        Public Function BuildOutput(labels As Double(), auc As Double, Optional cutoff As Double = 0.5) As Double()
            Dim pos = labels.Count(Function(i) i >= cutoff)
            Dim neg = labels.Count(Function(i) i < cutoff)
            Dim totalPairs = pos * neg
            Dim posRank = totalPairs * (1 - auc) / 2
            Dim indices = Enumerable.Range(0, labels.Length) _
                .OrderBy(Function(x) randf.NextDouble) _
                .ToArray
            Dim label_sorts = indices.Select(Function(i) labels(i)).ToArray
            Dim probs As Double() = New Double(labels.Length - 1) {}

            For i As Integer = 0 To labels.Length - 1
                If label_sorts(i) > cutoff Then
                    If i < posRank Then
                        probs(indices(i)) = randf.NextDouble(cutoff, 1)
                    Else
                        probs(indices(i)) = randf.NextDouble(0, cutoff)
                    End If
                Else
                    probs(indices(i)) = randf.NextDouble(0, cutoff)
                End If
            Next

            Return probs
        End Function

        Public Function BuildOutput2(labels As Double(), auc As Double, Optional cutoff As Double = 0.5, Optional itrs As Integer = 10) As Double()
            Dim copy As SeqValue(Of Double)() = labels.SeqIterator.Shuffles.ToArray
            Dim ordinal = copy.Select(Function(i) i.i).ToArray
            Dim out As Double() = copy.Select(Function(i) i.value).ToArray
            Dim d As Integer = 100
            Dim bar As ProgressBar = Nothing
            Dim auc_delta As Double = 0.05

            labels = out.ToArray

            If d < 1 Then
                d = 1
            End If

            For Each i As Integer In TqdmWrapper.Range(0, itrs, bar:=bar, wrap_console:=App.EnableTqdm)
                For offset As Integer = i * d To (i + 1) * d
                    ' flip the position
                    If out(offset) > cutoff Then
                        ' less than cutoff
                        out(offset) = randf.NextDouble * cutoff
                    Else
                        ' greater than cutoff
                        out(offset) = randf.NextDouble * cutoff + cutoff
                    End If
                Next

                Dim eval As Double = Evaluation.AUC(out, labels)
                Dim delta = std.Abs(eval - auc)

                Call bar.SetLabel($"auc: {eval }")

                If delta < auc_delta Then
                    Exit For
                End If
            Next

            Return ordinal.Zip(out).OrderBy(Function(a) a.First).Select(Function(i) i.Second).ToArray
        End Function

    End Module
End Namespace
