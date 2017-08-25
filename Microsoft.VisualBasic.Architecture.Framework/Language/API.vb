Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Perl

Namespace Language

    Public Module LanguageAPI

        ''' <summary>
        ''' The default value assertor
        ''' </summary>
        Friend ReadOnly defaultAssert As New DefaultValue(Of Assert(Of Object)) With {
            .Value = AddressOf ExceptionHandler.Default,
            .assert = Function(assert)
                          Return assert Is Nothing
                      End Function
        }

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
                .assert = isNothing Or defaultAssert
            }
        End Function

        <Extension>
        Public Function AsDefault(Of T)(x As T, Optional isNothing As Assert(Of Object) = Nothing) As DefaultValue(Of T)
            Return [Default](x, isNothing)
        End Function
    End Module
End Namespace