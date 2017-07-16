Imports System.Dynamic
Imports System.Linq.Expressions
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Language

    ''' <summary>
    ''' Vectorization programming language feature for VisualBasic
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class VectorShadows(Of T) : Inherits DynamicObject
        Implements IEnumerable(Of T)

        ReadOnly vector As T()
        ''' <summary>
        ''' 无参数的属性
        ''' </summary>
        ReadOnly linq As DataValue(Of T)
        ReadOnly propertyNames As Index(Of String)

        ''' <summary>
        ''' 单目运算符无重名的问题
        ''' </summary>
        ReadOnly operatorsUnary As New Dictionary(Of ExpressionType, Func(Of Object, Object))
        ''' <summary>
        ''' 双目运算符重载会带来重名运算符的问题
        ''' </summary>
        ReadOnly operatorsBinary As New Dictionary(Of ExpressionType, BinaryOperator)

#Region "VisualBasic exclusive language features"
        ReadOnly op_Concatenates As BinaryOperator
        ReadOnly op_Likes As BinaryOperator
        ReadOnly op_IntegerDivisions As BinaryOperator
#End Region

        ReadOnly methods As New Dictionary(Of String, MethodInfo())

        ReadOnly type As Type = GetType(T)

        Public ReadOnly Property Length As Integer
            Get
                Return vector.Length
            End Get
        End Property

        Const stringContract$ = "op_Concatenate"
        Const objectLike$ = "op_Like"
        Const integerDivision$ = "op_IntegerDivision"

        Sub New(source As IEnumerable(Of T))
            vector = source.ToArray
            linq = New DataValue(Of T)(vector)
            propertyNames = linq.PropertyNames.Indexing

            Dim methods = GetType(T).GetMethods()
            Dim operators = methods _
                .Where(Function(x) InStr(x.Name, "op_") = 1 AndAlso x.IsStatic) _
                .GroupBy(Function(op) op.Name) _
                .ToArray

            Dim find = Function(opName$)
                           Return operators _
                               .Where(Function(m) m.Key = opName) _
                               .FirstOrDefault _
                              ?.OverloadsBinaryOperator
                       End Function

            ' 因为字符串连接操作符在Linq表达式中并没有被定义，所以在这里需要特殊处理
            op_Concatenates = find(stringContract)
            op_Likes = find(objectLike)
            op_IntegerDivisions = find(integerDivision)

            For Each op As IGrouping(Of String, MethodInfo) In operators
#If DEBUG Then
                ' Call op.Key.EchoLine
#End If
                With op
                    If .Key = stringContract OrElse
                        .Key = objectLike OrElse
                        .Key = integerDivision Then

                        ' 前面已经被处理过了，不需要再额外处理这个运算符了
                        Continue For
                    End If
                End With

                ' 将运算符字符串名称转换为Linq表达式类型名称
                Dim type As ExpressionType = OperatorExpression.opName2Linq(op.Key)

                If op.First.GetParameters.Length > 1 Then
                    operatorsBinary(type) = op.OverloadsBinaryOperator
                Else
                    Dim method = op.First
                    Dim invoke = Function(arg) method.Invoke(Nothing, {arg})

                    operatorsUnary(type) = invoke
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
            Dim type As Type = obj.GetType

            If vector.op_Concatenates Is Nothing Then
                If vector.type Is GetType(String) Then
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
                            .Select(Function(o) CStrSafe(o) & s) _
                            .ToArray
                    End If
                Else
                    Throw New NotImplementedException
                End If
            Else
                Return binaryOperatorSelfLeft(vector, vector.op_Concatenates, obj, type)
            End If
        End Operator

        Private Shared Function binaryOperatorSelfLeft(vector As VectorShadows(Of T), op As BinaryOperator, obj As Object, type As Type) As Object
            Dim method As MethodInfo = op.MatchRight(type)

            If Not method Is Nothing Then
                Return vector _
                    .Select(Function(self) method.Invoke(Nothing, {self, obj})) _
                    .ToArray
            End If

            If type.ImplementsInterface(GetType(IEnumerable)) Then
                type = type.GetInterfaces _
                    .Where(Function(i) i.Name = NameOf(IEnumerable)) _
                    .First _
                    .GenericTypeArguments _
                    .First

                With op.MatchRight(type)
                    If .IsNothing Then
                        Throw New NotImplementedException
                    Else
                        method = .ref
                    End If
                End With

                Dim out = New Object(vector.Length - 1) {}

                For Each o In DirectCast(obj, IEnumerable).SeqIterator
                    out(o) = method.Invoke(Nothing, {vector.vector(o), o.value})
                Next

                Return out
            Else
                Throw New NotImplementedException
            End If
        End Function

        ''' <summary>
        ''' Fix for Like operator not defined in Linq.
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Shared Operator Like(vector As VectorShadows(Of T), obj As Object) As Object
            If vector.op_Likes Is Nothing Then
                Throw New NotImplementedException
            Else
                Return binaryOperatorSelfLeft(vector, vector.op_Likes, obj, obj.GetType)
            End If
        End Operator

        Public Shared Operator \(vector As VectorShadows(Of T), obj As Object) As Object

        End Operator

        Const left% = 0
        Const right% = 1

        Public Overrides Function TryBinaryOperation(binder As BinaryOperationBinder, arg As Object, ByRef result As Object) As Boolean
            If Not operatorsBinary.ContainsKey(binder.Operation) Then
                Return False
            End If

            Dim op As BinaryOperator = operatorsBinary(binder.Operation)
            Dim type As Type = arg.GetType
            Dim target As MethodInfo = Nothing

            With op.MatchRight(type)
                If Not .IsNothing AndAlso .GetParameters(left).ParameterType Is Me.type Then

                    target = .ref
                    ' me op arg
                    result = vector _
                        .Select(Function(self) target.Invoke(Nothing, {self, arg})) _
                        .ToArray

                    Return True
                End If
            End With

            With op.MatchLeft(type)
                If Not .IsNothing AndAlso .GetParameters(right).ParameterType Is Me.type Then

                    target = .ref
                    ' arg op me
                    result = vector _
                        .Select(Function(self) target.Invoke(Nothing, {arg, self})) _
                        .ToArray

                    Return True
                End If
            End With

            ' target还是空值的话，则尝试将目标参数转换为集合类型
            If Not type.ImplementsInterface(GetType(IEnumerable)) Then
                Return False
            Else
                type = type.GetInterfaces _
                    .Where(Function(i) i.Name = NameOf(IEnumerable)) _
                    .First _
                    .GenericTypeArguments _
                    .First
            End If

            Dim out = New Object(vector.Length - 1) {}

            With op.MatchRight(type)
                If Not .IsNothing AndAlso .GetParameters(left).ParameterType Is Me.type Then

                    target = .ref

                    For Each o In DirectCast(arg, IEnumerable).SeqIterator
                        out(o) = target.Invoke(Nothing, {vector(o), o.value})
                    Next
                    result = out

                    Return True
                End If
            End With

            With op.MatchLeft(type)
                If Not .IsNothing AndAlso .GetParameters(right).ParameterType Is Me.type Then

                    target = .ref

                    For Each o In DirectCast(arg, IEnumerable).SeqIterator
                        out(o) = target.Invoke(Nothing, {o.value, vector(o)})
                    Next
                    result = out

                    Return True
                End If
            End With

            Return False
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