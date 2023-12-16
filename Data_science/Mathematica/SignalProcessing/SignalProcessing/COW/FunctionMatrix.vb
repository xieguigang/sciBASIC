Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace COW

    ''' <summary>
    ''' This class is used in dynamic programming algorithm of CowAlignment.cs
    ''' </summary>
    Friend Class FunctionMatrix : Implements IDisposable

        Dim funcBeanMatrix As FunctionElement()()
        Dim disposedValue As Boolean

        Public ReadOnly Property dims As (row As Integer, col As Integer)

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

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    For i As Integer = 0 To funcBeanMatrix.Length - 1
                        Erase funcBeanMatrix(i)
                    Next

                    Erase funcBeanMatrix
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
