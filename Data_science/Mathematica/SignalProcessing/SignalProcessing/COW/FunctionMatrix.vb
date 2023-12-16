Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Parallel

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
            Call New InitializeTask(Me).Run()
            Me(segmentNumber, enabledLength).Score = 0
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{dims.row} x {dims.col}]"
        End Function

        Private Class InitializeTask : Inherits VectorTask

            ReadOnly mat As FunctionMatrix

            Public Sub New(m As FunctionMatrix)
                MyBase.New(m.funcBeanMatrix.Length)
                Me.mat = m
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                For i As Integer = start To ends
                    Dim v As FunctionElement() = mat.funcBeanMatrix(i)

                    For j As Integer = 0 To v.Length - 1
                        v(j) = New FunctionElement(Double.MinValue, 0)
                    Next

                    mat.funcBeanMatrix(i) = v
                Next
            End Sub
        End Class
    End Class
End Namespace
