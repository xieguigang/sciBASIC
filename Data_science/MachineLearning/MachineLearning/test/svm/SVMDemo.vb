#Region "Microsoft.VisualBasic::b441153ce960052b11f9012ac8795e26, Data_science\MachineLearning\MachineLearning\test\svm\SVMDemo.vb"

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

    '   Total Lines: 151
    '    Code Lines: 69 (45.70%)
    ' Comment Lines: 55 (36.42%)
    '    - Xml Docs: 41.82%
    ' 
    '   Blank Lines: 27 (17.88%)
    '     File Size: 6.71 KB


    '     Module SVMDemoProgram
    ' 
    '         Function: classSummary, formatPercent
    ' 
    '         Sub: Main
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'   SVM demo test (command line)
'
'   A runnable console demo of the LibSVM style support vector machine port which
'   is provided by the Microsoft.VisualBasic.MachineLearning.SVM namespace:
'
'     1) generate a randomized two class classification problem (no external file
'        is required, the demo runs out of the box)
'     2) scale the samples via RangeTransform, then train a C-SVC model with the
'        RBF kernel (gamma = 0.5, C = 1)
'     3) evaluate the model on the training set and on an independent test set
'     4) render the decision regions, the decision boundary, the samples and the
'        support vectors into a PNG image via GDI+
'
'   How to run:
'
'       dotnet run --project test/test.vbproj
'
'   The generated image is written to the current working directory.
'
' /********************************************************************************/

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.SVM

Namespace SVMDemo

    ''' <summary>
    ''' The command line entry point of the SVM demo test. This module is registered
    ''' as the <c>StartupObject</c> of the test project.
    ''' </summary>
    Public Module SVMDemoProgram

        ''' <summary>
        ''' The number of the samples which was generated for the training set, and
        ''' the same amount is generated for the test set.
        ''' </summary>
        Public Const SAMPLE_SIZE As Integer = 300

        ''' <summary>
        ''' The file name of the rendered classification result, it is a relative path
        ''' which will be resolved against the current working directory.
        ''' </summary>
        Public Const OUTPUT_PNG As String = "./svm_demo.png"

        ''' <summary>
        ''' Runs the whole demo: synthesize the data, train the model, evaluate the
        ''' model and finally render the classification result into a PNG image.
        ''' </summary>
        Public Sub Main()
            Console.WriteLine("============================================================")
            Console.WriteLine(" sciBASIC# LibSVM port :: two class classification demo")
            Console.WriteLine("============================================================")

            ' the library logs nothing unless the verbose flag is enabled, keep the
            ' console output of this demo clean and readable.
            Logging.IsVerbose = False

            ' 1) build the training/testing problem. the demo uses the randomized
            '    synthetic data which is shipped with the library, so no data file
            '    is required for running this demo.
            Dim train As Problem = SVMUtilities.CreateTwoClassProblem(SAMPLE_SIZE, isTraining:=True)
            Dim test As Problem = SVMUtilities.CreateTwoClassProblem(SAMPLE_SIZE, isTraining:=False)

            Console.WriteLine($"training samples : {train.count} ({classSummary(train)})")
            Console.WriteLine($"testing samples  : {test.count} ({classSummary(test)})")

            ' 2) normalize the samples. the very same transform must be applied on the
            '    training set, the test set and the grid points of the plot below,
            '    otherwise the prediction result will be inconsistent.
            Dim transform As RangeTransform = RangeTransform.Compute(train)
            Dim scaledTrain As Problem = transform.Scale(train)
            Dim scaledTest As Problem = transform.Scale(test)

            Dim param As New Parameter With {
                .svmType = SvmType.C_SVC,
                .kernelType = KernelType.RBF,
                .gamma = 0.5,
                .c = 1
            }

            Console.WriteLine()
            Console.WriteLine($"svm type  : {param.svmType}")
            Console.WriteLine($"kernel    : {param.kernelType} (gamma = {param.gamma}, C = {param.c})")

            ' 3) train the model and evaluate it
            Dim watch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
            Dim model As Model = Training.Train(scaledTrain, param)

            Call Logging.flush()
            watch.Stop()

            Dim trainScore As Double = Prediction.Predict(scaledTrain, Nothing, model, predict_probability:=False)
            Dim testScore As Double = Prediction.Predict(scaledTest, Nothing, model, predict_probability:=False)

            Console.WriteLine($"training time : {watch.ElapsedMilliseconds} ms")
            Console.WriteLine($"support vectors: {model.supportVectorCount} / {train.count}")
            Console.WriteLine($"accuracy      : train {formatPercent(trainScore)}, test {formatPercent(testScore)}")

            ' 4) render the classification result as a PNG image
            Dim output As String = Path.GetFullPath(OUTPUT_PNG)
            Dim summary As String() = {
                $"samples: train {train.count} / test {test.count}, support vectors: {model.supportVectorCount}",
                $"accuracy: train {formatPercent(trainScore)}, test {formatPercent(testScore)}"
            }

            Call SVMDemoPlot.PlotResult(train, model, transform, output, summary)

            Console.WriteLine()
            Console.WriteLine($"plot image was saved to: {output}")
            Console.WriteLine("done.")
        End Sub

        ''' <summary>
        ''' Gets a brief description of the class distribution of the given problem,
        ''' the text is in format like <c>class 1 = 150, class -1 = 150</c>.
        ''' </summary>
        Private Function classSummary(problem As Problem) As String
            Dim counter As New Dictionary(Of String, Integer)

            For Each cls In problem.Y

                If counter.ContainsKey(cls.name) Then
                    counter(cls.name) += 1
                Else
                    counter(cls.name) = 1
                End If
            Next

            Dim parts As New List(Of String)

            For Each name As String In counter.Keys.OrderBy(Function(a) a)
                parts.Add($"class {name} = {counter(name)}")
            Next

            Return String.Join(", ", parts)
        End Function

        ''' <summary>
        ''' Format a score value (in range <c>[0, 1]</c>) as a percent string.
        ''' </summary>
        Private Function formatPercent(score As Double) As String
            Return (score * 100).ToString("F2") & "%"
        End Function

    End Module
End Namespace

