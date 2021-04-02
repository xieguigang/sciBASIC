Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq

Namespace Contour

    Public MustInherit Class EvaluatePoints

        Public MustOverride Function Evaluate(x As Double, y As Double) As Double

    End Class

    Public Class FormulaEvaluate : Inherits EvaluatePoints

        ''' <summary>
        ''' evaluate of the z from [x, y]
        ''' </summary>
        Public formula As Func(Of Double, Double, Double)

        Public Overrides Function Evaluate(x As Double, y As Double) As Double
            Return formula(x, y)
        End Function
    End Class

    Public Class MatrixEvaluate : Inherits EvaluatePoints

        ReadOnly matrix As (x As Double, (y As Double, z As Double)())()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="matrix">
        ''' id is the x axis value
        ''' properties in one data set object is the y vector and z vector
        ''' </param>
        Sub New(matrix As IEnumerable(Of DataSet))
            Me.matrix = matrix _
                .DoCall(AddressOf fromMatrixQuery) _
                .OrderBy(Function(row) row.x) _
                .ToArray
        End Sub

        Private Shared Function fromMatrixQuery(matrix As IEnumerable(Of DataSet)) As IEnumerable(Of (x As Double, (y As Double, z As Double)()))
            Return From line As DataSet
                   In matrix
                   Let xi = Val(line.ID)
                   Let data = line.Properties.Select(Function(o) (Y:=Val(o.Key), Z:=o.Value)).OrderBy(Function(pt) pt.Y).ToArray
                   Select (xi, data)
        End Function

        ''' <summary>
        ''' query data by binary search
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(x As Double, y As Double) As Double
            Dim row = matrix.BinarySearch(x, Function(r) r.x, Nothing)

            If row.Item2.IsNullOrEmpty Then
                Return 0
            Else
                Return row.Item2.BinarySearch(y, Function(c) c.y, Nothing).z
            End If
        End Function
    End Class
End Namespace