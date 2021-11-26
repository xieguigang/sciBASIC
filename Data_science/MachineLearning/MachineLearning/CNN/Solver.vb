Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Convolutional.ImageProcessor
Imports stdNum = System.Math

Namespace Convolutional

    Public Module Solver

        <Extension>
        Public Function DetectObject(cnn As CeNiN,
                                     image As Bitmap,
                                     Optional resize As ResizingMethod = ResizingMethod.ZeroPad,
                                     Optional dev As TextWriter = Nothing) As NamedValue(Of Double)()

            Dim currentLayer As Layer = cnn.inputLayer.setInput(image, resizingMethod:=resize)
            Dim i As Integer = 0
            Dim start As Long = App.NanoTime

            dev = dev Or App.StdOut

            While currentLayer.nextLayer IsNot Nothing
                If i = 0 Then
                    dev.WriteLine("Loading bitmap data...")
                Else
                    dev.WriteLine("Layer " & i & " (" & currentLayer.type & ") ...")
                End If

                Call Application.DoEvents()

                currentLayer = currentLayer.feedNext().nextLayer
                i += 1

                Call dev.Flush()
            End While

            Dim OutputLayer As Output = CType(currentLayer, Output)

            Call dev.WriteLine("Finished in " & TimeSpan.FromTicks(App.NanoTime - start).FormatTime & " seconds")

            Dim Decision As String = OutputLayer.getDecision()
            Dim HLine As String = New String("-"c, 100)

            Call dev.WriteLine(HLine, "")

            For i = 2 To 0 Step -1
                Call dev.WriteLine(" #" & (i + 1) & "   " & OutputLayer.sortedClasses(i) & " (" & stdNum.Round(OutputLayer.probabilities(i), 3) & ")", "")
            Next

            Call dev.WriteLine(HLine, "")
            Call dev.WriteLine("THE HIGHEST 3 PROBABILITIES: ", "")
            Call dev.WriteLine(HLine, "")
            Call dev.WriteLine("DECISION: " & Decision)
            Call dev.WriteLine(HLine, "")

            Return OutputLayer.sortedClasses _
                .Select(Function(tag, j)
                            Return New NamedValue(Of Double)(tag, OutputLayer.probabilities(j))
                        End Function) _
                .ToArray
        End Function
    End Module
End Namespace