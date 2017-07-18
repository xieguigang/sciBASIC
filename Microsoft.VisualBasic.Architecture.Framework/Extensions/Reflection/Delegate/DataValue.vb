
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Emit.Delegates

    ''' <summary>
    ''' .NET object collection data property value ``get/set`` helper.
    ''' (将属性的<see cref="PropertyInfo.SetValue(Object, Object)"/>编译为方法调用)
    ''' </summary>
    Public Class DataValue(Of T)

        ReadOnly type As Type = GetType(T)
        ReadOnly data As T()
        ''' <summary>
        ''' Using for expression tree compile to delegate by using <see cref="BindProperty(Of T)"/>, 
        ''' to makes the get/set invoke faster
        ''' </summary>
        ReadOnly properties As Dictionary(Of String, PropertyInfo)

        Public ReadOnly Property PropertyNames As String()
            Get
                Return properties.Values _
                    .Select(Function(x) x.Name) _
                    .ToArray
            End Get
        End Property

        Public Function GetProperty(property$) As PropertyInfo
            Return properties([property])
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name$">The property name, using the ``nameof`` operator to get the property name!</param>
        ''' <returns></returns>
        Default Public Property Evaluate(name$) As Object
            Get
                Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
                Dim vector As Array = Array.CreateInstance([property].Type, data.Length)

                For i As Integer = 0 To data.Length - 1
                    Call vector.SetValue([property].__getValue(data(i)), i)
                Next

                Return vector
            End Get
            Set(value As Object)
                Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
                Dim array As IEnumerable = TryCast(value, IEnumerable)

                If value Is Nothing Then
                    For Each x In data
                        Call [property].__setValue(x, Nothing)
                    Next
                ElseIf array Is Nothing Then  ' 不是一个集合
                    Dim v As Object = value

                    For Each x As T In data
                        Call [property].__setValue(x, v)
                    Next
                Else
                    Dim vector = array.Cast(Of Object).ToArray

                    If vector.Length <> data.Length Then
                        Throw New InvalidExpressionException(DimNotAgree$)
                    End If
                    For i As Integer = 0 To data.Length - 1
                        Call [property].__setValue(data(i), vector(i))
                    Next
                End If
            End Set
        End Property

        Const DimNotAgree$ = "Value array should have the same length as the target data array"

        'Public Property Evaluate(Of V)(name$) As V()
        '    Get
        '        Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
        '        Return data _
        '            .Select(Function(x) DirectCast([property].__getValue(x), V)) _
        '            .ToArray
        '    End Get
        '    Set(ParamArray value As V())
        '        Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))

        '        If value.IsNullorEmpty Then  
        '            ' value array is nothing or have no data, 
        '            ' then means set all property value to nothing 
        '            For Each x In data
        '                Call [property].__setValue(x, Nothing)
        '            Next
        '        ElseIf value.Length = 1 Then 
        '            ' value array only have one element, 
        '            ' then means set all property value to a specific value
        '            Dim v As Object = value(Scan0)
        '            For Each x In data
        '                Call [property].__setValue(x, v)
        '            Next
        '        Else
        '            If value.Length <> data.Length Then
        '                Throw New InvalidExpressionException(DimNotAgree$)
        '            End If

        '            For i As Integer = 0 To data.Length - 1
        '                Call [property].__setValue(data(i), value(i))
        '            Next
        '        End If
        '    End Set
        'End Property

        Sub New(src As IEnumerable(Of T))
            data = src.ToArray
            properties = type.Schema(PropertyAccess.NotSure, PublicProperty, True)
        End Sub

        Public Overrides Function ToString() As String
            Return type.FullName
        End Function

        Private Shared Sub TestDEMO()
            Dim vector As NamedValue(Of String)() = {}
            Dim previousData = Linq.DATA(vector).Evaluate("Value")

            Linq.DATA(vector).Evaluate("Value") = {}    ' set all value property to nothing
            Linq.DATA(vector).Evaluate("Value") = {"1"} ' set all value property to a specifc value "1"
            Linq.DATA(vector).Evaluate("Value") = {"1", "2", "3"}
        End Sub
    End Class
End Namespace