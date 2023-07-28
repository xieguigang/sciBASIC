Imports Microsoft.VisualBasic.Data.csv.IO

Namespace DATA

    ''' <summary>
    ''' A numeric data matrix provider
    ''' </summary>
    Public Interface MatrixProvider

        Function GetMatrix() As IEnumerable(Of DataSet)

    End Interface
End Namespace