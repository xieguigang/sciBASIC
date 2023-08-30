
Namespace COW

    ''' <summary>
    ''' This class is used in dynamic programming algorithm of CowAlignment.cs
    ''' </summary>
    Friend Class FunctionMatrix

        Dim funcBeanMatrix As FunctionElement(,)

        Default Public Property Item(rowPosition As Integer, columnPosition As Integer) As FunctionElement
            Get
                Return funcBeanMatrix(rowPosition, columnPosition)
            End Get
            Set(value As FunctionElement)
                funcBeanMatrix(rowPosition, columnPosition) = value
            End Set
        End Property

        Public Sub New(rowSize As Integer, columnSize As Integer)
            funcBeanMatrix = New FunctionElement(rowSize - 1, columnSize - 1) {}
        End Sub

    End Class
End Namespace
