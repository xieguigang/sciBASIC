Imports System.Dynamic
Imports System.Linq.Expressions
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language

    ''' <summary>
    ''' Schema provider of the <see cref="VectorShadows(Of T)"/>
    ''' </summary>
    Public Class VectorSchemaProvider

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
        ReadOnly propertyList As Dictionary(Of String, PropertyInfo)

        Public ReadOnly Property PropertyNames As Index(Of String)
        Public ReadOnly Property Type As Type

        Public Const stringContract$ = "op_Concatenate"
        Public Const objectLike$ = "op_Like"
        Public Const nameIntegerDivision$ = "op_IntegerDivision"

        Sub New(type As Type)
            Me.Type = type
            Me.propertyList = type.Schema(PropertyAccess.NotSure, PublicProperty, True)
            Me.PropertyNames = propertyList _
                .Values _
                .Select(Function([property]) [property].Name) _
                .Indexing

            Dim methods = type.GetMethods()
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
            op_IntegerDivisions = find(nameIntegerDivision)

            For Each op As IGrouping(Of String, MethodInfo) In operators
#If DEBUG Then
                ' Call op.Key.EchoLine
#End If
                With op
                    If .Key = stringContract OrElse
                        .Key = objectLike OrElse
                        .Key = nameIntegerDivision Then

                        ' 前面已经被处理过了，不需要再额外处理这个运算符了
                        Continue For
                    End If
                End With

                ' 将运算符字符串名称转换为Linq表达式类型名称
                Dim name As ExpressionType = OperatorExpression.opName2Linq(op.Key)

                If op.First.GetParameters.Length > 1 Then
                    operatorsBinary(name) = op.OverloadsBinaryOperator
                Else
                    operatorsUnary(name) = op.First
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
        Public Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return PropertyNames.Objects.AsList + methods.Keys
        End Function

#Region "Property Get/Set"

        ''' <summary>
        ''' Property Get
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <returns></returns>
        Public Function TryGetMember(binder As GetMemberBinder) As PropertyInfo
            If PropertyNames.IndexOf(binder.Name) = -1 Then
                Return Nothing
            Else
                Return propertyList(binder.Name)
            End If
        End Function

        ''' <summary>
        ''' Property Set
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <returns></returns>
        Public Function TrySetMember(binder As SetMemberBinder) As PropertyInfo
            If PropertyNames.IndexOf(binder.Name) = -1 Then
                Return Nothing
            Else
                Return propertyList(binder.Name)
            End If
        End Function
#End Region

        ''' <summary>
        ''' Function invoke
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function TryInvokeMember(binder As InvokeMemberBinder, args() As Object) As MethodInfo
            If Not methods.ContainsKey(binder.Name) Then
                Return Nothing
            End If

            Dim [overloads] = methods(binder.Name)
            Dim method As MethodInfo = [overloads].Match(args.Select(Function(o) o.GetType).ToArray)
            Return method
        End Function

        Public Function TryUnaryOperation(binder As UnaryOperationBinder) As MethodInfo
            If Not operatorsUnary.ContainsKey(binder.Operation) Then
                Return Nothing
            Else
                Dim method = operatorsUnary(binder.Operation)
                Return method
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Type.ToString
        End Function

#Region "Operator:Binary"

        ''' <summary>
        ''' Fix for &amp; operator not defined!
        ''' </summary>
        ''' <returns></returns>
        Public Function [Concatenate](type As Type, ByRef vector As Boolean) As MethodInfo
            If op_Concatenates Is Nothing Then
                Return Nothing
            Else
                Return binaryOperatorSelfLeft(op_Concatenates, type, vector)
            End If
        End Function

        Private Shared Function binaryOperatorSelfLeft(op As BinaryOperator, type As Type, ByRef vector As Boolean) As MethodInfo
            Dim method As MethodInfo = op.MatchRight(type)

            If Not method Is Nothing Then
                Return method
            End If

            If type.ImplementsInterface(GetType(IEnumerable)) Then
                vector = True
                type = type.GetInterfaces _
                    .Where(Function(i) i.Name = NameOf(IEnumerable)) _
                    .First _
                    .GenericTypeArguments _
                    .First

                Return op.MatchRight(type)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Fix for Like operator not defined in Linq.
        ''' </summary>
        ''' <returns></returns>
        Public Function [Like](type As Type, ByRef vector As Boolean) As MethodInfo
            If op_Likes Is Nothing Then
                Return Nothing
            Else
                Return binaryOperatorSelfLeft(op_Likes, type, vector)
            End If
        End Function

        Public Function [IntegerDivision](type As Type, ByRef vector As Boolean) As MethodInfo
            If op_IntegerDivisions Is Nothing Then
                Return Nothing
            Else
                Return binaryOperatorSelfLeft(op_IntegerDivisions, type, vector)
            End If
        End Function

        Const left% = 0
        Const right% = 1

        Public Function TryBinaryOperation(binder As BinaryOperationBinder, type As Type, ByRef vector As Boolean) As MethodInfo
            If Not operatorsBinary.ContainsKey(binder.Operation) Then
                Return Nothing
            End If

            Dim op As BinaryOperator = operatorsBinary(binder.Operation)
            Dim target As MethodInfo = Nothing

            With op.MatchRight(type)
                If Not .IsNothing AndAlso .GetParameters(left).ParameterType Is Me.Type Then
                    Return .ref
                End If
            End With

            ' target还是空值的话，则尝试将目标参数转换为集合类型
            If Not type.ImplementsInterface(GetType(IEnumerable)) Then
                Return Nothing
            Else
                type = type.GetInterfaces _
                    .Where(Function(i) i.Name = NameOf(IEnumerable)) _
                    .First _
                    .GenericTypeArguments _
                    .First
            End If

            With op.MatchRight(type)
                If Not .IsNothing AndAlso .GetParameters(left).ParameterType Is Me.Type Then
                    vector = True
                    Return .ref
                End If
            End With

            Return Nothing
        End Function
#End Region
    End Class
End Namespace