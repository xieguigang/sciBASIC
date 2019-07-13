#Region "Microsoft.VisualBasic::cb7c12209490c29b0ba650281109540a, Data_science\DataMining\DataMining\ComponentModel\Class.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class ColorClass
    ' 
    '         Properties: color, enumInt, name
    ' 
    '         Function: FromEnums, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        Public Property color As String
        ''' <summary>
        ''' <see cref="Integer"/> encoding for this class.(即枚举类型)
        ''' </summary>
        ''' <returns></returns>
        Public Property enumInt As Integer
        ''' <summary>
        ''' Class Name
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="colors$">Using the user custom colors</param>
        ''' <returns></returns>
        Public Shared Function FromEnums(Of T As Structure)(Optional colors$() = Nothing) As ColorClass()
            Dim values As T() = Enums(Of T)()

            If colors.IsNullOrEmpty OrElse colors.Length < values.Length Then
                colors$ = Imaging _
                    .ChartColors _
                    .Select(AddressOf Imaging.ToHtmlColor) _
                    .ToArray
            End If

            Dim out As ColorClass() = values _
                .SeqIterator _
                .Select(Function(v)
                            Return New ColorClass With {
                                .enumInt = CInt(DirectCast(+v, Object)),
                                .color = colors(v),
                                .name = DirectCast(CObj((+v)), [Enum]).Description
                            }
                        End Function) _
                .ToArray

            Return out
        End Function

        Public Shared Operator =(a As ColorClass, b As ColorClass) As Boolean
            Return a.color = b.color AndAlso a.enumInt = b.enumInt AndAlso a.name = b.name
        End Operator

        Public Shared Operator <>(a As ColorClass, b As ColorClass) As Boolean
            Return Not a = b
        End Operator
    End Class
End Namespace
