Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports stdNum = System.Math

Namespace Convolutional

    Public Module Solver

        <Extension>
        Public Function DetectObject(cnn As CNN, image As Bitmap) As NamedValue(Of Double)()
            cnn.inputLayer.setInput(image, Input.ResizingMethod.ZeroPad)
            Dim CurrentLayer As Layer = cnn.inputLayer
            Dim i As Integer = 0
            Dim start As Long = App.NanoTime

            While CurrentLayer.nextLayer IsNot Nothing
                If i = 0 Then
                    Console.WriteLine("Loading bitmap data...")
                Else
                    Console.WriteLine("Layer " & i & " (" & CurrentLayer.type & ") ...")
                End If

                Application.DoEvents()
                CurrentLayer.feedNext()
                CurrentLayer = CurrentLayer.nextLayer
                i += 1
            End While

            Dim OutputLayer As Output = CType(CurrentLayer, Output)
            Console.WriteLine("Finished in " & TimeSpan.FromTicks(App.NanoTime - start).FormatTime & " seconds")

            Dim Decision As String = OutputLayer.getDecision()
            Dim HLine As String = New String("-"c, 100)
            Console.WriteLine(HLine, "")

            For i = 2 To 0 Step -1
                Console.WriteLine(" #" & (i + 1) & "   " & OutputLayer.sortedClasses(i) & " (" & stdNum.Round(OutputLayer.probabilities(i), 3) & ")", "")
            Next

            Console.WriteLine(HLine, "")
            Console.WriteLine("THE HIGHEST 3 PROBABILITIES: ", "")
            Console.WriteLine(HLine, "")
            Console.WriteLine("DECISION: " & Decision)
            Console.WriteLine(HLine, "")

            Return OutputLayer.sortedClasses _
                .Select(Function(tag, j)
                            Return New NamedValue(Of Double)(tag, OutputLayer.probabilities(j))
                        End Function) _
                .ToArray
        End Function
    End Module
End Namespace