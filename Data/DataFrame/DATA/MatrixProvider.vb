Imports Microsoft.VisualBasic.Data.csv.IO

Namespace DATA

    ''' <summary>
    ''' A numeric data matrix provider
    ''' </summary>
    Public Interface MatrixProvider

        ''' <summary>
        ''' populate the matrix data in row by row
        ''' </summary>
        ''' <returns></returns>
        Function GetMatrix() As IEnumerable(Of DataSet)

    End Interface
End Namespace