#Region "Microsoft.VisualBasic::253e7c96b0c24844b07dfc674bd4ac15, Microsoft.VisualBasic.Core\src\Language\Linq\Vectorization\VectorShadows.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 411
    '    Code Lines: 272
    ' Comment Lines: 84
    '   Blank Lines: 55
    '     File Size: 15.87 KB


    '     Class VectorShadows
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [As], binaryOperatorSelfLeft, CreateVector, GetDataProperties, GetDynamicMemberNames
    '                   GetJson, GetMapName, inspectType, TryBinaryOperation, (+2 Overloads) TryGetMember
    '                   TryInvokeMember, TrySetMember, TryUnaryOperation
    ' 
    '         Sub: writeBuffer
    ' 
    '         Operators: \, (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Dynamic
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace Language.Vectorization

    ''' <summary>
    ''' Vectorization programming language feature for VisualBasic
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class VectorShadows(Of T) : Inherits Vector(Of T)
        Implements IEnumerable(Of T)

        ''' <summary>
        ''' 无参数的属性
        ''' </summary>
        Protected linq As DataValue(Of T)
        Protected ReadOnly type As VectorSchemaProvider = inspectType(GetType(T))

        Private Shared Function inspectType(type As Type) As VectorSchemaProvider
            Static typeCache As New Dictionary(Of Type, VectorSchemaProvider)

            If Not typeCache.ContainsKey(type) Then
                SyncLock typeCache
                    typeCache(type) = VectorSchemaProvider.CreateSchema(type)
                End SyncLock
            End If

            Return typeCache(type)
        End Function

        ''' <summary>
        ''' get property value
        ''' </summary>
        ''' <param name="exp$"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(exp$) As Object
            Get
                If exp = "Me" Then
                    Return Me
                ElseIf type.PropertyNames.IndexOf(exp) > -1 Then
                    Return linq(exp)
                Else
                    Return MyBase.Item(exp)
                End If
            End Get
            Set(value)
                If exp = "Me" Then
                    buffer = DirectCast(value, IEnumerable(Of T)).ToArray
                ElseIf type.PropertyNames.IndexOf(exp) > -1 Then
                    linq(exp) = value
                Else
                    MyBase.Item(exp) = DirectCast(value, IEnumerable(Of T)).AsList
                End If
            End Set
        End Property

        ''' <summary>
        ''' get the vector value copy
        ''' </summary>
        ''' <typeparam name="V"></typeparam>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [As](Of V)() As IEnumerable(Of V)
            Return buffer.As(Of V)
        End Function

#Region ""
        Default Public Overloads Property Item(booleans As IEnumerable(Of Boolean)) As VectorShadows(Of T)
            Get
                Return New VectorShadows(Of T)(MyBase.Item(booleans))
            End Get
            Set(value As VectorShadows(Of T))
                MyBase.Item(booleans) = value
            End Set
        End Property
#End Region

        Sub New(source As IEnumerable(Of T))
            Call writeBuffer(source)
        End Sub

        Protected Sub writeBuffer(seq As IEnumerable(Of T))
            If seq Is Nothing Then
                seq = {}
            End If

            buffer = seq.ToArray
            linq = New DataValue(Of T)(buffer)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' this function returns nothing if has no name mapping
        ''' </returns>
        Public Function GetMapName(name As String) As String
            Dim p As PropertyInfo = type.TryGetMember(name, caseSensitive:=False)

            If p Is Nothing Then
                Return Nothing
            Else
                Return p.GetAliasName
            End If
        End Function

        ''' <summary>
        ''' Returns property names and function names
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return type.GetDynamicMemberNames
        End Function

        ''' <summary>
        ''' get all property names
        ''' </summary>
        ''' <returns></returns>
        Public Function GetDataProperties() As IEnumerable(Of String)
            Return type.PropertyNames.Objects
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
                Dim vector = Activator.CreateInstance(.ByRef, {data.CreateArray(type)})
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
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
            Return TryGetMember(binder.Name, result)
        End Function

        ''' <summary>
        ''' 大小写不敏感
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="result"></param>
        ''' <returns></returns>
        Public Overloads Function TryGetMember(name$, ByRef result As Object) As Boolean
            With type.TryGetMember(name, False)
                If .IsNothing Then
                    Return False
                Else
                    Dim type As Type = .PropertyType
                    Dim source = linq.Evaluate(name)
                    result = CreateVector(DirectCast(source, IEnumerable), type)
                    Return True
                End If
            End With
        End Function

        ''' <summary>
        ''' Property Set
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Overrides Function TrySetMember(binder As SetMemberBinder, value As Object) As Boolean
            If type.TrySetMember(binder) Is Nothing Then
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
            Dim method As MethodInfo = type.TryInvokeMember(binder, args)

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
            Dim method As MethodInfo = type.TryUnaryOperation(binder)

            If method Is Nothing Then
                Return False
            Else
                result = Me.Select(Function(x) method.Invoke(Nothing, {x}))
                result = CreateVector(DirectCast(result, IEnumerable), method.ReturnType)
            End If

            Return True
        End Function
#End Region

#Region "Operator:Binary"

        Public Overloads Shared Operator &(a As VectorShadows(Of T), b As VectorShadows(Of T)) As Object
            If a.type = GetType(String) AndAlso b.type = GetType(String) Then
                Dim av As String() = a.As(Of String).ToArray
                Dim bv As String() = b.As(Of String).ToArray
                Dim concat As String() = New String(std.Max(av.Length, bv.Length) - 1) {}
                Dim get_a As Func(Of Integer, String) = Function(i) av(i)
                Dim get_b As Func(Of Integer, String) = Function(i) bv(i)

                If av.Length <> bv.Length Then
                    If av.Length = 1 Then
                        Dim scalar As String = av(0)
                        get_a = Function() scalar
                    ElseIf bv.Length = 1 Then
                        Dim scalar As String = bv(0)
                        get_b = Function() scalar
                    Else
                        Throw New InvalidConstraintException($"the size of vector a({av.Length}) should be equals to the size of vector b({bv.Length})!")
                    End If
                End If

                For i As Integer = 0 To concat.Length - 1
                    concat(i) = get_a(i) & get_b(i)
                Next

                Return New VectorShadows(Of String)(concat)
            Else
                Throw New NotImplementedException
            End If
        End Operator

        ''' <summary>
        ''' Fix for &amp; operator not defined!
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator &(vector As VectorShadows(Of T), obj As Object) As Object
            Dim type As Type = obj.GetType
            Dim isVector As Boolean = False
            Dim op As MethodInfo = vector.type.Concatenate(type, isVector)

            If op Is Nothing Then
                If vector.type = GetType(String) Then
                    If type.ImplementInterface(GetType(IEnumerable(Of String))) Then
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
                Return binaryOperatorSelfLeft(vector, op, obj, isVector, type)
            End If
        End Operator

        Private Shared Function binaryOperatorSelfLeft(vector As VectorShadows(Of T), method As MethodInfo, obj As Object, vectorMode As Boolean, type As Type) As Object
            If Not method Is Nothing Then
                If Not vectorMode Then
                    obj = vector _
                    .Select(Function(self) method.Invoke(Nothing, {self, obj})) _
                    .ToArray
                    Return CreateVector(DirectCast(obj, IEnumerable), method.ReturnType)
                Else
                    Dim out = New Object(vector.Length - 1) {}

                    For Each o In DirectCast(obj, IEnumerable).SeqIterator
                        out(o) = method.Invoke(Nothing, {vector.buffer(o), o.value})
                    Next

                    Return CreateVector(out, method.ReturnType)
                End If
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
            Dim type As Type = obj.GetType
            Dim isVector As Boolean = False
            Dim op As MethodInfo = vector.type.Like(type, isVector)

            If op Is Nothing Then

                ' string like
                If vector.type Is GetType(String) Then
                    If type Is GetType(String) Then
                        Dim str$ = obj.ToString

                        Return New VectorShadows(Of Boolean)(vector.Select(Function(s) CStrSafe(s) Like str))
                    ElseIf type.ImplementInterface(GetType(IEnumerable(Of String))) Then
                        Dim out As Boolean() = New Boolean(vector.Length - 1) {}

                        For Each s In DirectCast(obj, IEnumerable(Of String)).SeqIterator
                            out(s) = DirectCast(CObj(vector.buffer(s)), String) Like s.value
                        Next

                        Return New VectorShadows(Of Boolean)(out)
                    End If
                End If

                Throw New NotImplementedException
            Else
                Return binaryOperatorSelfLeft(vector, op, obj, isVector, type)
            End If
        End Operator

        Public Shared Operator \(vector As VectorShadows(Of T), obj As Object) As Object
            Dim type As Type = obj.GetType
            Dim isVector As Boolean = False
            Dim op As MethodInfo = vector.type.Concatenate(type, isVector)

            If op Is Nothing Then
                Throw New NotImplementedException
            Else
                Return binaryOperatorSelfLeft(vector, op, obj, isVector, obj.GetType)
            End If
        End Operator

        Const left% = 0
        Const right% = 1

        Public Overrides Function TryBinaryOperation(binder As BinaryOperationBinder, arg As Object, ByRef result As Object) As Boolean
            Dim type As Type = arg.GetType
            Dim isVector As Boolean = False
            Dim target As MethodInfo = Me.type.TryBinaryOperation(binder, type, isVector)

            If Not target Is Nothing Then
                If Not isVector Then
                    ' me op arg
                    result = buffer _
                        .Select(Function(self) target.Invoke(Nothing, {self, arg}))
                    result = CreateVector(DirectCast(result, IEnumerable), target.ReturnType)

                    Return True
                Else
                    Dim out = New Object(buffer.Length - 1) {}

                    For Each o In DirectCast(arg, IEnumerable).SeqIterator
                        out(o) = target.Invoke(Nothing, {buffer(o), o.value})
                    Next
                    result = CreateVector(out, target.ReturnType)

                    Return True
                End If
            Else
                Return False
            End If
        End Function
#End Region
    End Class
End Namespace
