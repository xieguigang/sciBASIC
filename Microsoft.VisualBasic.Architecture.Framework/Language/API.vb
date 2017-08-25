Namespace Language

    Public Module LanguageAPI

        ''' <summary>
        ''' 模拟R语言之中的``%||%``操作符
        ''' 
        ''' ```R
        ''' `%||%` &lt;- function(x, y) if (is.null(x)) y else x
        ''' 
        ''' NULL %||% 123
        ''' # 123
        ''' 
        ''' 233 %||% 123
        ''' # 233
        ''' ```
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="isNothing"></param>
        ''' <returns></returns>
        Public Function [Default](Of T)(x As T, Optional isNothing As Assert(Of Object) = Nothing) As DefaultValue(Of T)
            Return New DefaultValue(Of T) With {
                .Value = x,
                .assert = isNothing Or DefaultValue(Of T).defaultAssert
            }
        End Function
    End Module
End Namespace