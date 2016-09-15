Imports System.Dynamic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace Language

    Public Class Tuple : Inherits DynamicObject
        Implements IDynamicMeta(Of Object)

        ReadOnly __meta As Dictionary(Of String, Object)

        Public Property Properties As Dictionary(Of String, Object) Implements IDynamicMeta(Of Object).Properties
            Get
                Return __meta
            End Get
            Private Set(value As Dictionary(Of String, Object))
                For Each x In value
                    __meta(x.Key) = x.Value
                Next
            End Set
        End Property

        Sub New(o As Object)
            __meta = __getValues(o).ToDictionary(
                Function(x) x.Name,
                Function(x) x.x)
        End Sub

        Sub New()
            __meta = New Dictionary(Of String, Object)
        End Sub

        Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return Properties.Keys
        End Function

        Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
            If __meta.ContainsKey(binder.Name) Then
                result = __meta(binder.Name)
                Return True
            Else
                Return False
            End If
        End Function

        Public Overrides Function TrySetMember(binder As SetMemberBinder, value As Object) As Boolean
            __meta(binder.Name) = value
            Return True
        End Function

        Public Shared Operator <=(t As Tuple, o As Object) As Tuple
            Return t + o
        End Operator

        Public Shared Operator +(t As Tuple, o As Object) As Tuple
            For Each x In __getValues(o)
                t.Properties(x.Name) = x.x
            Next

            Return t
        End Operator

        Private Shared Iterator Function __getValues(o As Object) As IEnumerable(Of NamedValue(Of Object))
            For Each p In DataFrameColumnAttribute.LoadMapping(o.GetType,, True).Values
                Yield New NamedValue(Of Object) With {
                    .Name = p.Identity,
                    .x = p.GetValue(o)
                }
            Next
        End Function

        Public Shared Operator >=(t As Tuple, o As Object) As Tuple
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace