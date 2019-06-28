Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Discretion

    ''' <summary>
    ''' 通过这个对象来执行对连续性数值的数据集的离散化操作
    ''' </summary>
    ''' <remarks>
    ''' 离散化是通过类似于等宽分bin来实现的
    ''' </remarks>
    Public Class Discretizer

        Public Property min As Double
        Public Property max As Double
        Public Property delta As Double

        Dim bins As DoubleRange()

        Sub New(sample As IEnumerable(Of Double), levels As Integer)
            With sample.ToArray
                min = .Min
                max = .Max
                delta = (max - min) / levels
            End With

            bins = createBins.ToArray
        End Sub

        ''' <summary>
        ''' json/xml serialization
        ''' </summary>
        Sub New()
        End Sub

        Private Iterator Function createBins() As IEnumerable(Of DoubleRange)
            Dim lower As VBDouble = min

            Do While lower < max
                Yield New DoubleRange(lower, lower = lower + delta)
            Loop
        End Function

        Public Function GetLevel(x As Double) As Integer
            If bins Is Nothing Then
                bins = createBins.ToArray
            End If

            If x < min Then
                Return 0
            ElseIf x > max Then
                Return bins.Length
            End If

            For i As Integer = 0 To bins.Length - 1
                If bins(i).IsInside(x) Then
                    Return i
                End If
            Next

            Throw New InvalidProgramException
        End Function
    End Class
End Namespace