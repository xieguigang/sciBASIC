Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' dataframe helper
    ''' </summary>
    Public Interface ILabeledMatrix

        ''' <summary>
        ''' get the labels of each row data
        ''' </summary>
        ''' <returns></returns>
        Function GetLabels() As IEnumerable(Of String)

    End Interface
End Namespace