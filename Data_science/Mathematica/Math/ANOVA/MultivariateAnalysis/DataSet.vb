
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports df = Microsoft.VisualBasic.Math.DataFrame.DataFrame

''' <summary>
''' helper module for create the input dataset for run analysis
''' </summary>
Public Module DataSet

    Public Function PCADataSet(Of Row As {INamedValue, IVector})(mat As IEnumerable(Of Row)) As StatisticsObject

    End Function

    Public Function PCADataSet(df As df) As StatisticsObject

    End Function

    Public Function PLSDADataSet(df As df) As StatisticsObject

    End Function

End Module
