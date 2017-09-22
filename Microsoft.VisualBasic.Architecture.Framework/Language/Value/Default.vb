Imports Microsoft.VisualBasic.Language.Perl

Namespace Language

    Public Delegate Function Assert(Of T)(obj As T) As Boolean

    ''' <summary>
    ''' The default value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure DefaultValue(Of T)

        Public ReadOnly Property DefaultValue As T
            Get
                If LazyValue Is Nothing Then
                    Return Value
                Else
                    ' using lazy loading, if the default value takes time to creates.
                    Return LazyValue()
                End If
            End Get
        End Property

        ''' <summary>
        ''' The default value for <see cref="DefaultValue"/>
        ''' </summary>
        Dim Value As T

        ''' <summary>
        ''' 假若生成目标值的时间比较久，可以将其申明为Lambda表达式，这样子可以进行惰性加载
        ''' </summary>
        Dim LazyValue As Func(Of T)

        ''' <summary>
        ''' asset that if target value is null?
        ''' </summary>
        Dim assert As Assert(Of Object)

        Public Overrides Function ToString() As String
            Return $"default({Value})"
        End Function

        ''' <summary>
        ''' Add handler
        ''' </summary>
        ''' <param name="[default]"></param>
        ''' <param name="assert"></param>
        ''' <returns></returns>
        Public Shared Operator +([default] As DefaultValue(Of T), assert As Assert(Of Object)) As DefaultValue(Of T)
            Return New DefaultValue(Of T) With {
                .assert = assert,
                .Value = [default].Value
            }
        End Operator

        ''' <summary>
        ''' if <see cref="assert"/> is true, then will using default <see cref="value"/>, 
        ''' otherwise, return the source <paramref name="obj"/>.
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="[default]"></param>
        ''' <returns></returns>
        Public Shared Operator Or(obj As T, [default] As DefaultValue(Of T)) As T
            With [default]
                If .assert(obj) Then
                    Return .Value
                Else
                    Return obj
                End If
            End With
        End Operator

        Public Shared Widening Operator CType(obj As T) As DefaultValue(Of T)
            Return New DefaultValue(Of T) With {
                .Value = obj,
                .assert = AddressOf ExceptionHandler.Default
            }
        End Operator

        Public Shared Widening Operator CType(lazy As Func(Of T)) As DefaultValue(Of T)
            Return New DefaultValue(Of T) With {
                .LazyValue = lazy,
                .assert = AddressOf ExceptionHandler.Default
            }
        End Operator
    End Structure
End Namespace