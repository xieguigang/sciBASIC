Imports System.Dynamic
Imports System.Linq.Expressions
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

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
        ReadOnly operatorsUnary As New Dictionary(Of ExpressionType, Object)
        ReadOnly operatorsBinary As New Dictionary(Of ExpressionType, Object)
        ReadOnly op_Concatenates As MethodInfo()

        ReadOnly type As Type = GetType(T)

        Public ReadOnly Property Length As Integer
            Get
                Return vector.Length
            End Get
        End Property

        Const stringContract$ = "op_Concatenate"

        Sub New(source As IEnumerable(Of T))
            vector = source.ToArray
            linq = New DataValue(Of T)(vector)
            propertyNames = linq.PropertyNames.Indexing

            Dim methods = GetType(T).GetMethods()
            Dim operators = methods _
                .Where(Function(x) InStr(x.Name, "op_") = 1 AndAlso x.IsStatic) _
                .GroupBy(Function(op) op.Name) _
                .ToArray

            For Each op In operators
                If op.Key = stringContract Then
                    ' 因为字符串连接操作符在Linq表达式中并没有被定义，所以在这里需要特殊处理
                Else
                    ' 将运算符字符串名称转换为Linq表达式类型名称

                End If
            Next
        End Sub

        Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return propertyNames.Objects
        End Function

        Public Function GetJson() As String
            Return Me.ToArray.GetJson
        End Function

        Public Shared Function CreateVector(data As IEnumerable, type As Type) As Object
            With GetType(VectorShadows(Of )).MakeGenericType(type)
                Dim vector = Activator.CreateInstance(.ref, {data})
                Return vector
            End With
        End Function

#Region "Property Get/Set"
        Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
            If propertyNames.IndexOf(binder.Name) = -1 Then
                Return False
            Else
                With linq
                    Dim type As Type = .GetProperty(binder.Name).PropertyType
                    Dim source As IEnumerable = DirectCast(.Evaluate(binder.Name), IEnumerable)
                    result = CreateVector(source, type)
                End With

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

        End Function
#End Region

#Region "Operator:Binary"

        ''' <summary>
        ''' Fix for &amp; operator not defined!
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Shared Operator &(vector As VectorShadows(Of T), obj As Object) As Object
            If vector.op_Concatenates Is Nothing Then
                If vector.type Is GetType(String) Then
                    Dim type As Type = obj.GetType

                    If type.ImplementsInterface(GetType(IEnumerable(Of String))) Then
                        ' 如果是字符串的集合，则分别添加字符串
                        Dim out$() = New String(vector.Length - 1) {}

                        For Each s In DirectCast(obj, IEnumerable(Of String)).SeqIterator
                            out(s) = DirectCast(CObj(vector.vector(s)), String) & s.value
                        Next

                        Return out
                    Else
                        ' 否则直接将目标对象转换为字符串，进行统一添加
                        Dim s$ = CStr(obj)
                        Return vector _
                            .Select(Function(o) Scripting.CStrSafe(o) & s) _
                            .ToArray
                    End If
                Else
                    Throw New NotImplementedException
                End If
            Else

            End If
        End Operator

        Public Overrides Function TryBinaryOperation(binder As BinaryOperationBinder, arg As Object, ByRef result As Object) As Boolean
            If Not operatorsBinary.ContainsKey(binder.Operation) Then
                If binder.Operation = ExpressionType.GreaterThan Then

                End If
                Return False
            Else

            End If
        End Function
#End Region

        ''' <summary>
        ''' 没用？？？
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Overloads Shared Narrowing Operator CType(v As VectorShadows(Of T)) As T()
            Return v.ToArray
        End Operator

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