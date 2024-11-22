
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Emit.Delegates

    ''' <summary>
    ''' .net clr object vector data property ``get/set`` helper.
    ''' </summary>
    Public Class DataObjectVector

        Protected ReadOnly type As Type

        ''' <summary>
        ''' the raw input pool data
        ''' </summary>
        Protected ReadOnly data As Array

        ''' <summary>
        ''' Using for expression tree compile to delegate by using <see cref="BindProperty(Of T)"/>, 
        ''' to makes the get/set invoke faster
        ''' </summary>
        Protected ReadOnly properties As Dictionary(Of String, PropertyInfo)

        ''' <summary>
        ''' expose the internal clr array object to public directly.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RawArray As Array
            Get
                Return data
            End Get
        End Property

        ''' <summary>
        ''' get array of the clr property name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PropertyNames As String()
            Get
                Return properties.Values _
                    .Select(Function(x) x.Name) _
                    .ToArray
            End Get
        End Property

        Const DimNotAgree$ = "Value array should have the same length as the target data array"

        Default Public ReadOnly Property Evaluate(offset As IEnumerable(Of Integer)) As Array
            Get
                Dim pullIndex As Integer() = offset.ToArray
                Dim vector As Array = Array.CreateInstance(type, pullIndex.Length)

                For i As Integer = 0 To pullIndex.Length - 1
                    vector(i) = data(pullIndex(i))
                Next

                Return vector
            End Get
        End Property

        ''' <summary>
        ''' Evaluate the clr object property, and get vector value exports in batch 
        ''' </summary>
        ''' <param name="name$">The property name, using the ``nameof`` operator to get the property name!</param>
        ''' <returns>
        ''' get an array of the property value. the array is generic.
        ''' </returns>
        Default Public Property Evaluate(name As String) As Object
            Get
                Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
                Dim vector As Array = Array.CreateInstance([property].Type, data.Length)

                For i As Integer = 0 To data.Length - 1
                    Call vector.SetValue([property].handleGetValue(data(i)), i)
                Next

                Return vector
            End Get
            Set(value As Object)
                Call SetProperty(name, value)
            End Set
        End Property

        Private Sub SetProperty(name$, value As Object)
            Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
            Dim array As IEnumerable

            If [property].Type Is GetType(String) AndAlso value.GetType Is GetType(String) Then
                array = Nothing
            Else
                array = TryCast(value, IEnumerable)
            End If

            If value Is Nothing Then
                For Each x In data
                    Call [property].handleSetValue(x, Nothing)
                Next
            ElseIf array Is Nothing Then  ' 不是一个集合
                Dim v As Object = value

                For Each x As Object In data
                    Call [property].handleSetValue(x, v)
                Next
            Else
                Dim vector = array.As(Of Object).ToArray

                If vector.Length <> data.Length Then
                    Throw New InvalidExpressionException(DimNotAgree$)
                End If
                For i As Integer = 0 To data.Length - 1
                    Call [property].handleSetValue(data(i), vector(i))
                Next
            End If
        End Sub

        Sub New(array As Array)
            If array Is Nothing Then
                Throw New InvalidProgramException("the input clr object vector is nothing!")
            End If

            type = array.GetType.GetElementType

            If type Is Nothing OrElse type Is GetType(Object) Then
                Throw New InvalidProgramException("the general object type array is not accepted!")
            End If

            data = array
            properties = inspectType(type)
        End Sub

        Public Function GetSubVector(property$) As DataObjectVector
            Dim subvec As Array = Evaluate([property])
            Dim vec As New DataObjectVector(subvec)
            Return vec
        End Function

        Public Function GetProperty(property$) As PropertyInfo
            Return properties([property])
        End Function

        Private Shared Function inspectType(type As Type) As Dictionary(Of String, PropertyInfo)
            Static typeCache As New Dictionary(Of Type, Dictionary(Of String, PropertyInfo))

            If Not typeCache.ContainsKey(type) Then
                SyncLock typeCache
                    typeCache(type) = type.Schema(PropertyAccess.NotSure, PublicProperty, True)
                End SyncLock
            End If

            Return typeCache(type)
        End Function
    End Class

End Namespace