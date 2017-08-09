#Region "Microsoft.VisualBasic::f918567811b01f9f0157327e7c1f6c30, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Linq\VectorShadows.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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
    Public Class VectorShadows(Of T) : Inherits Vector(Of T)
        Implements IEnumerable(Of T)

        ''' <summary>
        ''' 无参数的属性
        ''' </summary>
        ReadOnly linq As DataValue(Of T)
        ReadOnly propertyNames As Index(Of String)

        ''' <summary>
        ''' 单目运算符无重名的问题
        ''' </summary>
        ReadOnly operatorsUnary As New Dictionary(Of ExpressionType, MethodInfo)
        ''' <summary>
        ''' 双目运算符重载会带来重名运算符的问题
        ''' </summary>
        ReadOnly operatorsBinary As New Dictionary(Of ExpressionType, BinaryOperator)

#Region "VisualBasic exclusive language features"
        ReadOnly op_Concatenates As BinaryOperator
        ReadOnly op_Likes As BinaryOperator
        ReadOnly op_IntegerDivisions As BinaryOperator
#End Region

        ''' <summary>
        ''' The overloads function
        ''' </summary>
        ReadOnly methods As New Dictionary(Of String, OverloadsFunction)
        ReadOnly type As Type = GetType(T)

        Const stringContract$ = "op_Concatenate"
        Const objectLike$ = "op_Like"
        Const integerDivision$ = "op_IntegerDivision"

        Default Public Overloads Property Item(exp$) As Object
            Get
                If exp = "Me" Then
                    Return Me
                ElseIf propertyNames.IndexOf(exp) > -1 Then
                    Return linq(exp)
                Else
                    Return MyBase.Item(exp)
                End If
            End Get
            Set(value)
                If exp = "Me" Then
                    buffer = DirectCast(value, IEnumerable(Of T)).ToArray
                ElseIf propertyNames.IndexOf(exp) > -1 Then
                    linq(exp) = value
                Else
                    MyBase.Item(exp) = DirectCast(value, IEnumerable(Of T)).AsList
                End If
            End Set
        End Property

        Public Function [As](Of V)() As V()
            Return buffer.As(Of V)
        End Function

#Region ""
        Default Public Overloads Property Item(booleans As IEnumerable(Of Boolean)) As VectorShadows(Of T)
            Get
                Return New VectorShadows(Of T)(MyBase.Item(booleans))
            End Get
            Set(value As VectorShadows(Of T))
                MyBase.Item(booleans) = value.buffer
            End Set
        End Property
#End Region

        Sub New(source As IEnumerable(Of T))
            buffer = source.ToArray
            linq = New DataValue(Of T)(buffer)
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
                    operatorsUnary(type) = op.First
                End If
            Next

            Me.methods = methods _
                .Where(Function(m) Not m.IsStatic) _
                .GroupBy(Function(func) func.Name) _
                .Select(Function([overloads]) New OverloadsFunction([overloads].Key, [overloads])) _
                .ToDictionary(Function(g) g.Name)
        End Sub

        ''' <summary>
        ''' Returns property names and function names
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return propertyNames.Objects.AsList + methods.Keys
        End Function

        ''' <summary>
        ''' Vector array json string
        ''' </summary>
        ''' <returns></returns>
        Public Function GetJson() As String
            Return Me.ToArray.GetJson
        End Function

        ''' <summary>
        ''' Create a generic vector for a specific .NET <paramref name="type"/>
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function CreateVector(data As IEnumerable, type As Type) As Object
            With GetType(VectorShadows(Of )).MakeGenericType(type)
                Dim vector = Activator.CreateInstance(.ref, {data.CreateArray(type)})
                Return vector
            End With
        End Function

#Region "Property Get/Set"

        ''' <summary>
        ''' Property Get
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="result"></param>
        ''' <returns></returns>
        Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
            If propertyNames.IndexOf(binder.Name) = -1 Then
                Return False
            Else
                With linq
                    Dim type As Type = .GetProperty(binder.Name).PropertyType
                    Dim source = .Evaluate(binder.Name)
                    result = CreateVector(DirectCast(source, IEnumerable), type)
                End With

                Return True
            End If
        End Function

        ''' <summary>
        ''' Property Set
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
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

        ''' <summary>
        ''' Function invoke
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="args"></param>
        ''' <param name="result"></param>
        ''' <returns></returns>
        Public Overrides Function TryInvokeMember(binder As InvokeMemberBinder, args() As Object, ByRef result As Object) As Boolean
            If Not methods.ContainsKey(binder.Name) Then
                Return False
            End If

            Dim [overloads] = methods(binder.Name)
            Dim method As MethodInfo = [overloads].Match(args.Select(Function(o) o.GetType).ToArray)

            If method Is Nothing Then
                Return False
            Else
                result = Me.Select(Function(o) method.Invoke(o, args))
                result = CreateVector(DirectCast(result, IEnumerable), method.ReturnType)
                Return True
            End If
        End Function
#End Region

#Region "Operator:Unary"
        Public Overrides Function TryUnaryOperation(binder As UnaryOperationBinder, ByRef result As Object) As Boolean
            If Not operatorsUnary.ContainsKey(binder.Operation) Then
                Return False
            Else
                Dim method = operatorsUnary(binder.Operation)
                result = Me.Select(Function(x) method.Invoke(Nothing, {x}))
                result = CreateVector(DirectCast(result, IEnumerable), method.ReturnType)
            End If

            Return True
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
                            out(s) = DirectCast(CObj(vector.buffer(s)), String) & s.value
                        Next

                        Return New VectorShadows(Of String)(out)
                    Else
                        ' 否则直接将目标对象转换为字符串，进行统一添加
                        Dim s$ = CStr(obj)
                        Return New VectorShadows(Of String)(
                            vector _
                            .Select(Function(o) CStrSafe(o) & s) _
                            .ToArray)
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
                obj = vector _
                    .Select(Function(self) method.Invoke(Nothing, {self, obj})) _
                    .ToArray
                Return CreateVector(DirectCast(obj, IEnumerable), method.ReturnType)
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
                    out(o) = method.Invoke(Nothing, {vector.buffer(o), o.value})
                Next

                Return CreateVector(out, method.ReturnType)
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

                ' string like
                If vector.type Is GetType(String) Then
                    Dim type As Type = obj.GetType

                    If type Is GetType(String) Then
                        Dim str$ = obj.ToString

                        Return New VectorShadows(Of Boolean)(vector.Select(Function(s) CStrSafe(s) Like str))
                    ElseIf type.ImplementsInterface(GetType(IEnumerable(Of String))) Then
                        Dim out As Boolean() = New Boolean(vector.Length - 1) {}

                        For Each s In DirectCast(obj, IEnumerable(Of String)).SeqIterator
                            out(s) = DirectCast(CObj(vector.buffer(s)), String) Like s.value
                        Next

                        Return New VectorShadows(Of Boolean)(out)
                    End If
                End If

                Throw New NotImplementedException
            Else
                Return binaryOperatorSelfLeft(vector, vector.op_Likes, obj, obj.GetType)
            End If
        End Operator

        Public Shared Operator \(vector As VectorShadows(Of T), obj As Object) As Object
            If vector.op_IntegerDivisions Is Nothing Then
                Throw New NotImplementedException
            Else
                Return binaryOperatorSelfLeft(vector, vector.op_IntegerDivisions, obj, obj.GetType)
            End If
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
                    result = buffer _
                        .Select(Function(self) target.Invoke(Nothing, {self, arg}))
                    result = CreateVector(DirectCast(result, IEnumerable), target.ReturnType)

                    Return True
                End If
            End With

            'With op.MatchLeft(type)
            '    If Not .IsNothing AndAlso .GetParameters(right).ParameterType Is Me.type Then

            '        target = .ref
            '        ' arg op me
            '        result = buffer _
            '            .Select(Function(self) target.Invoke(Nothing, {arg, self}))
            '        result = CreateVector(DirectCast(result, IEnumerable), target.ReturnType)

            '        Return True
            '    End If
            'End With

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

            Dim out = New Object(buffer.Length - 1) {}

            With op.MatchRight(type)
                If Not .IsNothing AndAlso .GetParameters(left).ParameterType Is Me.type Then

                    target = .ref

                    For Each o In DirectCast(arg, IEnumerable).SeqIterator
                        out(o) = target.Invoke(Nothing, {buffer(o), o.value})
                    Next
                    result = CreateVector(out, target.ReturnType)

                    Return True
                End If
            End With

            'With op.MatchLeft(type)
            '    If Not .IsNothing AndAlso .GetParameters(right).ParameterType Is Me.type Then

            '        target = .ref

            '        For Each o In DirectCast(arg, IEnumerable).SeqIterator
            '            out(o) = target.Invoke(Nothing, {o.value, buffer(o)})
            '        Next
            '        result = out.CreateArray(target.ReturnType)

            '        Return True
            '    End If
            'End With

            Return False
        End Function
#End Region
    End Class
End Namespace
