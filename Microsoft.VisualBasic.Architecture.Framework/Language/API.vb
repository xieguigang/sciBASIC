Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Perl
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Language

    ''' <summary>
    ''' The VisualBasic language syntax helper API.
    ''' </summary>
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
        ''' simulate the ``%||%`` operator in R language.
        ''' 
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

        ''' <summary>
        ''' Using this value as the default value for this <typeparamref name="T"/> type.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="isNothing"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AsDefault(Of T)(x As T, Optional isNothing As Assert(Of Object) = Nothing) As DefaultValue(Of T)
            Return [Default](x, isNothing)
        End Function

        ''' <summary>
        ''' Helper for update the value property of <see cref="Value(Of T)"/>
        ''' 
        ''' ```vbnet
        ''' Call Let$(<see cref="Value(Of T)"/> = x)
        ''' ```
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function Let$(Of T)(value As T)
            Try
                Return CStrSafe(value)
            Catch ex As Exception
                ' 在这里知识进行帮助值的设置，所以这个错误无所谓，直接忽略掉
                Return Nothing
            End Try
        End Function

#Region "Helper for ``With``"

        ''' <summary>
        ''' Extension method for VisualBasic ``With`` anonymous variable syntax source reference helper
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function ref(Of T)(x As T) As T
            Return x
        End Function

        ''' <summary>
        ''' Extension method for VisualBasic ``With`` anonymous variable syntax for determine that source reference is nothing or not?
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IsNothing(Of T As Class)(x As T) As Boolean
            Return x Is Nothing
        End Function
#End Region
    End Module
End Namespace