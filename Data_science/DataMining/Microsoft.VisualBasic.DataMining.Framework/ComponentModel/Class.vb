Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel

    ''' <summary>
    ''' Object entity classification class
    ''' </summary>
    Public Class ColorClass

        ''' <summary>
        ''' Using for the data visualization.(RGB表达式, html颜色值或者名称)
        ''' </summary>
        ''' <returns></returns>
        Public Property Color As String
        ''' <summary>
        ''' <see cref="Integer"/> encoding for this class.(即枚举类型)
        ''' </summary>
        ''' <returns></returns>
        Public Property int As Integer
        ''' <summary>
        ''' Class Name
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="colors$">Using the user custom colors</param>
        ''' <returns></returns>
        Public Shared Function FromEnums(Of T)(Optional colors$() = Nothing) As ColorClass()
            Dim values As T() = Enums(Of T)()
            If colors.IsNullOrEmpty OrElse colors.Length < values.Length Then
                colors$ = Imaging _
                    .ChartColors _
                    .Select(AddressOf Imaging.RGB2Hexadecimal) _
                    .ToArray
            End If
            Dim out As ColorClass() = values _
                .SeqIterator _
                .Select(Function(v)
                            Return New ColorClass With {
                                .int = CInt(DirectCast(+v, Object)),
                                .Color = colors(v),
                                .Name = DirectCast(CObj((+v)), [Enum]).Description
                            }
                        End Function) _
                .ToArray
            Return out
        End Function

        Public Shared Operator =(a As ColorClass, b As ColorClass) As Boolean
            Return a.Color = b.Color AndAlso a.int = b.int AndAlso a.Name = b.Name
        End Operator

        Public Shared Operator <>(a As ColorClass, b As ColorClass) As Boolean
            Return Not a = b
        End Operator
    End Class
End Namespace