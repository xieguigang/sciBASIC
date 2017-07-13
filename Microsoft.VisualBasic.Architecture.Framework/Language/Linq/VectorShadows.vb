Imports System.Dynamic
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Language

    ''' <summary>
    ''' Vectorization programming language feature for VisualBasic
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class VectorShadows(Of T) : Inherits DynamicObject
        Implements IEnumerable(Of T)

        ReadOnly vector As T()
        ReadOnly linq As DataValue(Of T)
        ReadOnly propertyNames As Index(Of String)

        ''' <summary>
        ''' 运算符重载会带来重名运算符的问题
        ''' </summary>
        ReadOnly operators As New Dictionary(Of String, Object)

        Public ReadOnly Property Length As Integer
            Get
                Return vector.Length
            End Get
        End Property

        Sub New(source As IEnumerable(Of T))
            vector = source.ToArray
            linq = New DataValue(Of T)(vector)
            propertyNames = linq.PropertyNames.Indexing

            Dim methods = GetType(T).GetMethods()
            Dim operators = methods.Where(Function(x) InStr(x.Name, "op_") = 1 AndAlso x.IsStatic).ToArray

            For Each op In operators
                Me.operators.Add(op.Name, op)
            Next
        End Sub

        Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return propertyNames.Objects
        End Function

#Region "Property Get/Set"
        Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
            If propertyNames.IndexOf(binder.Name) = -1 Then
                Return False
            Else
                result = linq.Evaluate(binder.Name)
                Return True
            End If
        End Function

        Public Overrides Function TrySetMember(binder As SetMemberBinder, value As Object) As Boolean
            If propertyNames.IndexOf(binder.Name) = -1 Then
                Return False
            Else
                linq.Evaluate(binder.Name) = value
                Return True
            End If
        End Function
#End Region

#Region "Method/Function"
        Public Overrides Function TryInvoke(binder As InvokeBinder, args() As Object, ByRef result As Object) As Boolean
            Return MyBase.TryInvoke(binder, args, result)
        End Function
#End Region

#Region "Operator:Unary"
        Public Overrides Function TryUnaryOperation(binder As UnaryOperationBinder, ByRef result As Object) As Boolean
            Return MyBase.TryUnaryOperation(binder, result)
        End Function
#End Region

#Region "Operator:Binary"
        Public Overrides Function TryBinaryOperation(binder As BinaryOperationBinder, arg As Object, ByRef result As Object) As Boolean
            Return MyBase.TryBinaryOperation(binder, arg, result)
        End Function
#End Region

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In vector
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace