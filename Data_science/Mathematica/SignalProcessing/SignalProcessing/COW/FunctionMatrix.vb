Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace COW

    ''' <summary>
    ''' This class is used in dynamic programming algorithm of CowAlignment.cs
    ''' </summary>
    Friend Class FunctionMatrix

        Dim funcBeanMatrix As FunctionElement()()
        Dim dims As (row As Integer, col As Integer)

        Default Public Property Item(rowPosition As Integer, columnPosition As Integer) As FunctionElement
            Get
                Return funcBeanMatrix(rowPosition)(columnPosition)
            End Get
            Set(value As FunctionElement)
                funcBeanMatrix(rowPosition)(columnPosition) = value
            End Set
        End Property

        Public Sub New(rowSize As Integer, columnSize As Integer)
            dims = (rowSize, columnSize)
            funcBeanMatrix = RectangularArray.Matrix(Of FunctionElement)(rowSize, columnSize)
        End Sub

        Public Sub Initialize(segmentNumber As Integer, enabledLength As Integer)
            Dim mat = funcBeanMatrix

            For i = 0 To segmentNumber
                For j = 0 To enabledLength
                    mat(i)(j) = New FunctionElement(Double.MinValue, 0)
                Next
            Next

            Me(segmentNumber, enabledLength).Score = 0
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{dims.row} x {dims.col}]"
        End Function
    End Class
End Namespace
