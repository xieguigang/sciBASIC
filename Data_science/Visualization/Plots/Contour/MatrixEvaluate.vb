Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Contour

    ''' <summary> 
    ''' 直接返回矩阵数据
    ''' </summary>
    Public Class MatrixEvaluate : Inherits EvaluatePoints

        ''' <summary>
        ''' [x -> [y, z]]
        ''' </summary>
        ReadOnly matrix As BinarySearchFunction(Of Double, (x#, bin As BinarySearchFunction(Of Double, (y#, z#))))

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="matrix">
        ''' id is the x axis value
        ''' properties in one data set object is the y vector and z vector
        ''' </param>
        Sub New(matrix As IEnumerable(Of DataSet), gridSize As SizeF)
            Dim compareWithError As Comparison(Of Double) =
                Function(a, b)
                    If stdNum.Abs(a - b) <= gridSize.Width Then
                        Return 0
                    ElseIf a < b Then
                        Return -1
                    Else
                        Return 1
                    End If
                End Function
            Dim ySearch = matrix _
                .DoCall(AddressOf fromMatrixQuery) _
                .Select(Function(row)
                            Dim bin As New BinarySearchFunction(Of Double, (y#, z#))(row.Item2, Function(p) p.y, compareWithError)

                            Return (row.x, bin)
                        End Function) _
                .ToArray

            Me.matrix = New BinarySearchFunction(Of Double, (x#, BinarySearchFunction(Of Double, (y#, z#))))(ySearch, Function(p) p.x, compareWithError)
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
            Dim row = matrix.BinarySearch(x)

            If row = -1 Then
                Return 0.0
            End If

            Dim point = matrix(row).bin.BinarySearch(y)

            If point = -1 Then
                Return 0.0
            Else
                Return matrix(row).bin(point).z
            End If
        End Function
    End Class
End Namespace