Namespace ComponentModel.DataSourceModel.Repository

    Public Interface ILocalSearchHandle

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Keyword"></param>
        ''' <param name="CaseSensitive">是否大小写敏感，默认不敏感</param>
        ''' <returns></returns>
        Function Matches(Keyword As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As ILocalSearchHandle()
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Keyword"></param>
        ''' <param name="CaseSensitive">是否大小写敏感，默认不敏感</param>
        ''' <returns></returns>
        Function Match(Keyword As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As Boolean
    End Interface
End Namespace