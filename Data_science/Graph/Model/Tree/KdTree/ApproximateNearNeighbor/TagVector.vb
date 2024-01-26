Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Math

Namespace KdTree.ApproximateNearNeighbor

    ''' <summary>
    ''' a matrix row data is a vector
    ''' </summary>
    Public Class TagVector : Implements INamedValue, IVector

        ''' <summary>
        ''' the row index inside the original matrix rows
        ''' </summary>
        ''' <returns></returns>
        Public Property index As Integer
        ''' <summary>
        ''' the vector row data
        ''' </summary>
        ''' <returns></returns>
        Public Property vector As Double() Implements IVector.Data
        ''' <summary>
        ''' maybe is the unique reference id tag
        ''' </summary>
        ''' <returns></returns>
        Public Property tag As String Implements INamedValue.Key

        Public ReadOnly Property size As Integer
            Get
                Return vector.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{index}] {vector.Take(6).JoinBy(", ")}..."
        End Function

    End Class
End Namespace