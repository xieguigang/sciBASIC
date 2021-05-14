#Region "Microsoft.VisualBasic::7de3b38abe167002542f089ac0bcd767, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Delegate\DelegateFactory.vb"

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

    '     Module DelegateFactory
    ' 
    '         Function: (+3 Overloads) Contructor, (+2 Overloads) DefaultContructor, DelegateIndexerGet, DelegateIndexerSet, (+2 Overloads) EventAccessor
    '                   EventAccessorImpl, (+4 Overloads) EventAdd, EventAddImpl, EventHandlerFactory, (+4 Overloads) EventRemove
    '                   EventRemoveImpl, (+3 Overloads) FieldGet, FieldGet2, (+3 Overloads) FieldSet, (+2 Overloads) FieldSetWithCast
    '                   GetConstructorInfo, GetEventInfo, GetEventInfoAccessor, GetFieldInfo, GetFuncDelegateArguments
    '                   GetFuncDelegateReturnType, GetIndexerPropertyInfo, GetMethodInfo, GetPropertyInfo, GetStaticFieldInfo
    '                   GetStaticMethodInfo, GetStaticPropertyInfo, (+8 Overloads) IndexerGet, (+7 Overloads) IndexerSet, (+2 Overloads) InstanceGenericMethod
    '                   InstanceGenericMethodVoid, (+9 Overloads) InstanceMethod, InstanceMethod2, InstanceMethodVoid, (+6 Overloads) PropertyGet
    '                   PropertyGet2, (+4 Overloads) PropertySet, StaticEventAdd, (+3 Overloads) StaticFieldGet, StaticFieldGetExpr
    '                   (+3 Overloads) StaticFieldSet, StaticGenericMethod, StaticGenericMethodVoid, (+12 Overloads) StaticMethod, StaticMethodVoid
    '                   (+3 Overloads) StaticPropertyGet, StaticPropertyGet2, (+3 Overloads) StaticPropertySet
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Emit.Delegates

    Public Module DelegateFactory

        Const AddAccessor As String = "add"
        Const Item As String = "Item"
        Const RemoveAccessor As String = "remove"

        ReadOnly EventHandlerFactoryMethodInfo As MethodInfo = GetType(DelegateFactory).GetMethod("EventHandlerFactory")
        ReadOnly EventsProxies As New Dictionary(Of WeakReference(Of Object), WeakReference(Of Object))()

        Public Function Contructor(Of TDelegate As Class)() As TDelegate
            Dim source = GetFuncDelegateReturnType(Of TDelegate)()
            Dim ctrArgs = GetFuncDelegateArguments(Of TDelegate)()
            Dim constructorInfo = GetConstructorInfo(source, ctrArgs)
            If constructorInfo Is Nothing Then
                Return Nothing
            End If
            Dim parameters = ctrArgs.[Select](AddressOf Expression.Parameter).AsList()
            Return TryCast(Expression.Lambda(Expression.[New](constructorInfo, parameters), parameters).Compile(), TDelegate)
        End Function

        <Extension>
        Public Function Contructor(source As Type, ParamArray ctrArgs As Type()) As Func(Of Object(), Object)
            Dim constructorInfo = GetConstructorInfo(source, ctrArgs)
            If constructorInfo Is Nothing Then
                Return Nothing
            End If
            Dim argsArray = Expression.Parameter(GetType(Object()))
            Dim paramsExpression = New Expression(ctrArgs.Length - 1) {}
            For i As Integer = 0 To ctrArgs.Length - 1
                Dim argType = ctrArgs(i)
                paramsExpression(i) = Expression.Convert(Expression.ArrayIndex(argsArray, Expression.Constant(i)), argType)
            Next
            Dim returnExpression As Expression = Expression.[New](constructorInfo, paramsExpression)
            If Not source.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return DirectCast(Expression.Lambda(returnExpression, argsArray).Compile(), Func(Of Object(), Object))
        End Function

        <Extension>
        Public Function Contructor(Of TDelegate As Class)(source As Type) As TDelegate
            Dim ctrArgs = GetFuncDelegateArguments(Of TDelegate)()
            Dim constructorInfo = GetConstructorInfo(source, ctrArgs)
            If constructorInfo Is Nothing Then
                Return Nothing
            End If
            Dim parameters = ctrArgs.[Select](AddressOf Expression.Parameter).AsList()
            Dim returnExpression As Expression = Expression.[New](constructorInfo, parameters)
            If Not source.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return TryCast(Expression.Lambda(returnExpression, parameters).Compile(), TDelegate)
        End Function

        Public Function DefaultContructor(Of TSource)() As Func(Of TSource)
            Return Contructor(Of Func(Of TSource))()
        End Function

        <Extension>
        Public Function DefaultContructor(type As Type) As Func(Of Object)
            Return type.Contructor(Of Func(Of Object))()
        End Function

        Public Function DelegateIndexerGet(source As Type, returnType As Type, ParamArray indexTypes As Type()) As [Delegate]
            Dim propertyInfo = GetIndexerPropertyInfo(source, returnType, indexTypes)
            If propertyInfo.GetMethod Is Nothing Then
                Return Nothing
            End If
            Dim sourceObjectParam = Expression.Parameter(GetType(Object))
            Dim paramsExpression = New ParameterExpression(indexTypes.Length - 1) {}
            For i As Integer = 0 To indexTypes.Length - 1
                Dim indexType = indexTypes(i)
                paramsExpression(i) = Expression.Parameter(indexType)
            Next
            Return Expression.Lambda(Expression.[Call](Expression.Convert(sourceObjectParam, source), propertyInfo.GetMethod, paramsExpression), {sourceObjectParam}.Concat(paramsExpression)).Compile()
        End Function

        Public Function DelegateIndexerSet(source As Type, returnType As Type, ParamArray indexTypes As Type()) As [Delegate]
            Dim propertyInfo = GetIndexerPropertyInfo(source, returnType, indexTypes)
            If propertyInfo.SetMethod Is Nothing Then
                Return Nothing
            End If
            Dim sourceObjectParam = Expression.Parameter(GetType(Object))
            Dim valueParam = Expression.Parameter(returnType)
            Dim indexExpressions = New ParameterExpression(indexTypes.Length - 1) {}
            For i As Integer = 0 To indexTypes.Length - 1
                Dim indexType = indexTypes(i)
                indexExpressions(i) = Expression.Parameter(indexType)
            Next
            Dim callArgs = indexExpressions.Concat({valueParam}).ToArray()
            Dim paramsExpressions = {sourceObjectParam}.Concat(callArgs)
            Return Expression.Lambda(Expression.[Call](Expression.Convert(sourceObjectParam, source), propertyInfo.SetMethod, callArgs), paramsExpressions).Compile()
        End Function

        Public Function EventAdd(Of TSource, TEventArgs)(eventName As String) As Action(Of TSource, EventHandler(Of TEventArgs))
            Return EventAccessor(Of TSource, TEventArgs)(eventName, AddAccessor)
        End Function

        <Extension>
        Public Function EventAdd(Of TEventArgs)(source As Type, eventName As String) As Action(Of Object, EventHandler(Of TEventArgs))
            Return EventAccessor(Of TEventArgs)(source, eventName, AddAccessor)
        End Function

        Public Function EventAdd(Of TSource)(eventName As String) As Action(Of TSource, Action(Of TSource, Object))
            Return GetType(TSource).EventAddImpl(Of Action(Of TSource, Action(Of TSource, Object)))(eventName)
        End Function

        <Extension>
        Public Function EventAdd(source As Type, eventName As String) As Action(Of Object, Action(Of Object, Object))
            Return source.EventAddImpl(Of Action(Of Object, Action(Of Object, Object)))(eventName)
        End Function

        Public Function EventHandlerFactory(Of TEventArgs As Class, TSource)(handler As Object, isRemove As Boolean) As EventHandler(Of TEventArgs)
            Dim newEventHandler As EventHandler(Of TEventArgs)
            Dim haveKey = False
            Dim kv = EventsProxies.FirstOrDefault(
                Function(k)
                    Dim keyTarget As Object = Nothing
                    k.Key.TryGetTarget(keyTarget)
                    If Equals(keyTarget, handler) Then
                        haveKey = True
                        Return True
                    End If
                    Return False
                End Function)

            If haveKey Then
                Dim fromCache As Object = Nothing
                EventsProxies(kv.Key).TryGetTarget(fromCache)
                newEventHandler = DirectCast(fromCache, EventHandler(Of TEventArgs))
                If isRemove Then
                    EventsProxies.Remove(kv.Key)
                    Return newEventHandler
                End If
            End If

            If Not isRemove Then
                Dim action = TryCast(handler, Action(Of TSource, Object))
                If action IsNot Nothing Then
                    newEventHandler = Sub(s, a) action(DirectCast(s, TSource), a)
                Else
                    newEventHandler = New EventHandler(Of TEventArgs)(AddressOf DirectCast(handler, Action(Of Object, Object)).Invoke)
                End If
                EventsProxies(New WeakReference(Of Object)(handler)) = New WeakReference(Of Object)(newEventHandler)
                Return newEventHandler
            End If

            Return Nothing
        End Function

        <Extension>
        Public Function EventRemove(Of TEventArgs)(source As Type, eventName As String) As Action(Of Object, EventHandler(Of TEventArgs))
            Return EventAccessor(Of TEventArgs)(source, eventName, RemoveAccessor)
        End Function

        Public Function EventRemove(Of TSource, TEventArgs)(eventName As String) As Action(Of TSource, EventHandler(Of TEventArgs))
            Return EventAccessor(Of TSource, TEventArgs)(eventName, RemoveAccessor)
        End Function

        Public Function EventRemove(Of TSource)(eventName As String) As Action(Of TSource, Action(Of TSource, Object))
            Return GetType(TSource).EventRemoveImpl(Of Action(Of TSource, Action(Of TSource, Object)))(eventName)
        End Function

        <Extension>
        Public Function EventRemove(source As Type, eventName As String) As Action(Of Object, Action(Of Object, Object))
            Return source.EventRemoveImpl(Of Action(Of Object, Action(Of Object, Object)))(eventName)
        End Function

        Public Function FieldGet(Of TSource, TField)(fieldName As String) As Func(Of TSource, TField)
            Dim source = GetType(TSource)
            Dim d As Object = source.FieldGet(fieldName)
            Return TryCast(d, Func(Of TSource, TField))
        End Function

        <Extension>
        Public Function FieldGet(Of TField)(source As Type, fieldName As String) As Func(Of Object, TField)
            Return TryCast(source.FieldGet(fieldName), Func(Of Object, TField))
        End Function

        <Extension>
        Public Function FieldGet(source As Type, fieldName As String) As Func(Of Object, Object)
            Dim fieldInfo = GetFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing Then
                Dim sourceParam = Expression.Parameter(GetType(Object))
                Dim returnExpression As Expression = Expression.Field(Expression.Convert(sourceParam, source), fieldInfo)
                If Not fieldInfo.FieldType.IsClass Then
                    returnExpression = Expression.Convert(returnExpression, GetType(Object))
                End If
                Dim lambda = Expression.Lambda(returnExpression, sourceParam)
                Return DirectCast(lambda.Compile(), Func(Of Object, Object))
            End If
            Return Nothing
        End Function

        <Obsolete>
        <Extension>
        Public Function FieldGet2(Of TField)(source As Type, fieldName As String) As Func(Of Object, TField)
            Dim fieldInfo = GetFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing Then
                Dim sourceParam = Expression.Parameter(GetType(Object))
                Dim returnExpression As Expression = Expression.Field(Expression.Convert(sourceParam, source), fieldInfo)
                Dim lambda = Expression.Lambda(returnExpression, sourceParam)
                Return DirectCast(lambda.Compile(), Func(Of Object, TField))
            End If
            Return Nothing
        End Function

        <Extension>
        Public Function FieldSet(Of TProperty)(source As Type, fieldName As String) As Action(Of Object, TProperty)
            Dim fieldInfo As FieldInfo = GetFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing AndAlso Not fieldInfo.IsInitOnly Then
                Dim sourceParam = Expression.Parameter(GetType(Object))  ' args1 参数
                Dim valueParam = Expression.Parameter(GetType(TProperty)) ' args2 参数
                Dim target = Expression.Convert(sourceParam, source)  ' args1 As <source Type>
                Dim fieldRef = Expression.Field(target, fieldInfo)  ' args1.Field
                Dim setValue = Expression.Assign(fieldRef, valueParam) ' args1.Field = args2
                Dim delgType As Type = GetType(Action(Of Object, TProperty))
                Dim te = Expression.Lambda(delgType, setValue, sourceParam, valueParam)
                Return DirectCast(te.Compile(), Action(Of Object, TProperty))
            End If
            Return Nothing
        End Function

        Public Function FieldSet(Of TSource, TProperty)(fieldName As String) As Action(Of TSource, TProperty)
            Dim source = GetType(TSource)
            Dim fieldInfo = GetFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing AndAlso Not fieldInfo.IsInitOnly Then
                Dim sourceParam = Expression.Parameter(source)
                Dim valueParam = Expression.Parameter(GetType(TProperty))
                Dim te = Expression.Lambda(GetType(Action(Of TSource, TProperty)), Expression.Assign(Expression.Field(sourceParam, fieldInfo), valueParam), sourceParam, valueParam)
                Return DirectCast(te.Compile(), Action(Of TSource, TProperty))
            End If
            Return Nothing
        End Function

        <Extension>
        Public Function FieldSet(source As Type, fieldName As String) As Action(Of Object, Object)
            Dim fieldInfo = GetFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing AndAlso Not fieldInfo.IsInitOnly Then
                Dim sourceParam = Expression.Parameter(GetType(Object))
                Dim valueParam = Expression.Parameter(GetType(Object))
                Dim convertedValueExpr = Expression.Convert(valueParam, fieldInfo.FieldType)
                Dim returnExpression As Expression = Expression.Assign(Expression.Field(Expression.Convert(sourceParam, source), fieldInfo), convertedValueExpr)
                If Not fieldInfo.FieldType.IsClass Then
                    returnExpression = Expression.Convert(returnExpression, GetType(Object))
                End If
                Dim lambda = Expression.Lambda(GetType(Action(Of Object, Object)), returnExpression, sourceParam, valueParam)
                Return DirectCast(lambda.Compile(), Action(Of Object, Object))
            End If
            Return Nothing
        End Function

        <Obsolete>
        <Extension>
        Public Function FieldSetWithCast(Of TProperty)(source As Type, fieldName As String) As Action(Of Object, TProperty)
            Return TryCast(source.FieldSet(fieldName), Action(Of Object, TProperty))
        End Function

        <Obsolete>
        Public Function FieldSetWithCast(Of TSource, TProperty)(fieldName As String) As Action(Of TSource, TProperty)
            Return TryCast(GetType(TSource).FieldSet(fieldName), Action(Of TSource, TProperty))
        End Function

        Public Function IndexerGet(Of TSource, TReturn, TIndex)() As Func(Of TSource, TIndex, TReturn)
            Dim propertyInfo = GetIndexerPropertyInfo(GetType(TSource), GetType(TReturn), {GetType(TIndex)})
            Return DirectCast(propertyInfo.GetMethod.CreateDelegate(GetType(Func(Of TSource, TIndex, TReturn))), Func(Of TSource, TIndex, TReturn))
        End Function

        Public Function IndexerGet(Of TSource, TReturn, TIndex, TIndex2)() As Func(Of TSource, TIndex, TIndex2, TReturn)
            Dim propertyInfo = GetIndexerPropertyInfo(GetType(TSource), GetType(TReturn), {GetType(TIndex), GetType(TIndex2)})
            Return DirectCast(propertyInfo.GetMethod.CreateDelegate(GetType(Func(Of TSource, TIndex, TIndex2, TReturn))), Func(Of TSource, TIndex, TIndex2, TReturn))
        End Function

        Public Function IndexerGet(Of TSource, TReturn, TIndex, TIndex2, TIndex3)() As Func(Of TSource, TIndex, TIndex2, TIndex2, TReturn)
            Dim propertyInfo = GetIndexerPropertyInfo(GetType(TSource), GetType(TReturn), {GetType(TIndex), GetType(TIndex2), GetType(TIndex3)})
            Return DirectCast(propertyInfo.GetMethod.CreateDelegate(GetType(Func(Of TSource, TIndex, TIndex2, TIndex2, TReturn))), Func(Of TSource, TIndex, TIndex2, TIndex2, TReturn))
        End Function

        <Extension>
        Public Function IndexerGet(Of TReturn, TIndex)(source As Type) As Func(Of Object, TIndex, TReturn)
            Dim indexType = GetType(TIndex)
            Return DirectCast(DelegateIndexerGet(source, GetType(TReturn), indexType), Func(Of Object, TIndex, TReturn))
        End Function

        <Extension>
        Public Function IndexerGet(Of TReturn, TIndex, TIndex2)(source As Type) As Func(Of Object, TIndex, TIndex2, TReturn)
            Dim indexType = GetType(TIndex)
            Dim indexType2 = GetType(TIndex2)
            Return DirectCast(DelegateIndexerGet(source, GetType(TReturn), indexType, indexType2), Func(Of Object, TIndex, TIndex2, TReturn))
        End Function

        <Extension>
        Public Function IndexerGet(Of TReturn, TIndex, TIndex2, TIndex3)(source As Type) As Func(Of Object, TIndex, TIndex2, TIndex3, TReturn)
            Dim indexType = GetType(TIndex)
            Dim indexType2 = GetType(TIndex2)
            Dim indexType3 = GetType(TIndex3)
            Return DirectCast(DelegateIndexerGet(source, GetType(TReturn), indexType, indexType2, indexType3), Func(Of Object, TIndex, TIndex2, TIndex3, TReturn))
        End Function

        <Extension>
        Public Function IndexerGet(source As Type, returnType As Type, ParamArray indexTypes As Type()) As Func(Of Object, Object(), Object)
            Dim propertyInfo = GetIndexerPropertyInfo(source, returnType, indexTypes)
            If propertyInfo.GetMethod Is Nothing Then
                Return Nothing
            End If
            Dim sourceObjectParam = Expression.Parameter(GetType(Object))
            Dim indexesParam = Expression.Parameter(GetType(Object()))
            Dim paramsExpression = New Expression(indexTypes.Length - 1) {}
            For i As Integer = 0 To indexTypes.Length - 1
                Dim indexType = indexTypes(i)
                paramsExpression(i) = Expression.Convert(Expression.ArrayIndex(indexesParam, Expression.Constant(i)), indexType)
            Next
            Dim returnExpression As Expression = Expression.[Call](Expression.Convert(sourceObjectParam, source), propertyInfo.GetMethod, paramsExpression)
            If Not propertyInfo.PropertyType.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return DirectCast(Expression.Lambda(returnExpression, sourceObjectParam, indexesParam).Compile(), Func(Of Object, Object(), Object))
        End Function

        <Extension>
        Public Function IndexerGet(source As Type, returnType As Type, indexType As Type) As Func(Of Object, Object, Object)
            Dim propertyInfo = GetIndexerPropertyInfo(source, returnType, {indexType})
            If propertyInfo.GetMethod Is Nothing Then
                Return Nothing
            End If
            Dim sourceObjectParam = Expression.Parameter(GetType(Object))
            Dim indexObjectParam = Expression.Parameter(GetType(Object))
            Dim returnExpression As Expression = Expression.[Call](Expression.Convert(sourceObjectParam, source), propertyInfo.GetMethod, Expression.Convert(indexObjectParam, indexType))
            If Not propertyInfo.PropertyType.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return DirectCast(Expression.Lambda(returnExpression, sourceObjectParam, indexObjectParam).Compile(), Func(Of Object, Object, Object))
        End Function

        <Extension>
        Public Function IndexerSet(Of TReturn, TIndex)(source As Type) As Action(Of Object, TIndex, TReturn)
            Dim indexType = GetType(TIndex)
            Return DirectCast(DelegateIndexerSet(source, GetType(TReturn), indexType), Action(Of Object, TIndex, TReturn))
        End Function

        <Extension>
        Public Function IndexerSet(Of TReturn, TIndex, TIndex2)(source As Type) As Action(Of Object, TIndex, TIndex2, TReturn)
            Dim indexType = GetType(TIndex)
            Dim indexType2 = GetType(TIndex2)
            Return DirectCast(DelegateIndexerSet(source, GetType(TReturn), indexType, indexType2), Action(Of Object, TIndex, TIndex2, TReturn))
        End Function

        <Extension>
        Public Function IndexerSet(Of TReturn, TIndex, TIndex2, TIndex3)(source As Type) As Action(Of Object, TIndex, TIndex2, TIndex3, TReturn)
            Dim indexType = GetType(TIndex)
            Dim indexType2 = GetType(TIndex2)
            Dim indexType3 = GetType(TIndex3)
            Return DirectCast(DelegateIndexerSet(source, GetType(TReturn), indexType, indexType2, indexType3), Action(Of Object, TIndex, TIndex2, TIndex3, TReturn))
        End Function

        Public Function IndexerSet(Of TSource, TIndex, TProperty)() As Action(Of TSource, TIndex, TProperty)
            Dim sourceType = GetType(TSource)
            Dim propertyInfo = GetIndexerPropertyInfo(sourceType, GetType(TProperty), {GetType(TIndex)})
            Return DirectCast(propertyInfo.SetMethod.CreateDelegate(GetType(Action(Of TSource, TIndex, TProperty))), Action(Of TSource, TIndex, TProperty))
        End Function

        Public Function IndexerSet(Of TSource, TReturn, TIndex, TIndex2)() As Action(Of TSource, TIndex, TIndex2, TReturn)
            Dim propertyInfo = GetIndexerPropertyInfo(GetType(TSource), GetType(TReturn), {GetType(TIndex), GetType(TIndex2)})
            Return DirectCast(propertyInfo.SetMethod.CreateDelegate(GetType(Action(Of TSource, TIndex, TIndex2, TReturn))), Action(Of TSource, TIndex, TIndex2, TReturn))
        End Function

        Public Function IndexerSet(Of TSource, TReturn, TIndex, TIndex2, TIndex3)() As Action(Of TSource, TIndex, TIndex2, TIndex2, TReturn)
            Dim propertyInfo = GetIndexerPropertyInfo(GetType(TSource), GetType(TReturn), {GetType(TIndex), GetType(TIndex2), GetType(TIndex3)})
            Return DirectCast(propertyInfo.SetMethod.CreateDelegate(GetType(Action(Of TSource, TIndex, TIndex2, TIndex2, TReturn))), Action(Of TSource, TIndex, TIndex2, TIndex2, TReturn))
        End Function

        <Extension>
        Public Function IndexerSet(source As Type, returnType As Type, ParamArray indexTypes As Type()) As Action(Of Object, Object(), Object)
            Dim propertyInfo = GetIndexerPropertyInfo(source, returnType, indexTypes)
            If propertyInfo.SetMethod Is Nothing Then
                Return Nothing
            End If
            Dim sourceObjectParam = Expression.Parameter(GetType(Object))
            Dim indexesParam = Expression.Parameter(GetType(Object()))
            Dim valueParam = Expression.Parameter(GetType(Object))
            Dim paramsExpression = New Expression(indexTypes.Length) {}
            For i As Integer = 0 To indexTypes.Length - 1
                Dim indexType = indexTypes(i)
                paramsExpression(i) = Expression.Convert(Expression.ArrayIndex(indexesParam, Expression.Constant(i)), indexType)
            Next
            paramsExpression(indexTypes.Length) = Expression.Convert(valueParam, returnType)
            Dim returnExpression As Expression = Expression.[Call](Expression.Convert(sourceObjectParam, source), propertyInfo.SetMethod, paramsExpression)
            Return DirectCast(Expression.Lambda(returnExpression, sourceObjectParam, indexesParam, valueParam).Compile(), Action(Of Object, Object(), Object))
        End Function

        <Extension>
        Public Function InstanceGenericMethod(source As Type, name As String, paramsTypes As Type(), typeParams As Type()) As Func(Of Object, Object(), Object)
            Return InstanceGenericMethod(Of Func(Of Object, Object(), Object))(source, name, typeParams, paramsTypes)
        End Function

        <Extension>
        Public Function InstanceGenericMethod(Of TDelegate As Class)(source As Type, name As String, typeParams As Type(), paramsTypes As Type()) As TDelegate
            Dim methodInfo = GetMethodInfo(source, name, paramsTypes, typeParams)
            If methodInfo Is Nothing Then
                Return Nothing
            End If
            Dim argsArray = Expression.Parameter(GetType(Object()))
            Dim sourceParameter = Expression.Parameter(GetType(Object))
            Dim paramsExpression = New Expression(paramsTypes.Length - 1) {}
            For i As Integer = 0 To paramsTypes.Length - 1
                Dim argType = paramsTypes(i)
                paramsExpression(i) = Expression.Convert(Expression.ArrayIndex(argsArray, Expression.Constant(i)), argType)
            Next
            Dim returnExpression As Expression = Expression.[Call](Expression.Convert(sourceParameter, source), methodInfo, paramsExpression)
            If methodInfo.ReturnType IsNot GetType(System.Void) AndAlso Not methodInfo.ReturnType.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return TryCast(Expression.Lambda(returnExpression, sourceParameter, argsArray).Compile(), TDelegate)
        End Function

        <Extension>
        Public Function InstanceGenericMethodVoid(source As Type, name As String, paramsTypes As Type(), typeParams As Type()) As Action(Of Object, Object())
            Return InstanceGenericMethod(Of Action(Of Object, Object()))(source, name, typeParams, paramsTypes)
        End Function

        <Extension>
        Public Function InstanceMethod(source As Type, name As String, ParamArray paramsTypes As Type()) As Func(Of Object, Object(), Object)
            Return InstanceGenericMethod(Of Func(Of Object, Object(), Object))(source, name, Nothing, paramsTypes)
        End Function

        Public Function InstanceMethod(Of TDelegate As Class, TParam1)(name As String) As TDelegate
            Return InstanceMethod(Of TDelegate)(name, GetType(TParam1))
        End Function

        Public Function InstanceMethod(Of TDelegate As Class, TParam1, TParam2)(name As String) As TDelegate
            Return InstanceMethod(Of TDelegate)(name, GetType(TParam1), GetType(TParam2))
        End Function

        Public Function InstanceMethod(Of TDelegate As Class, TParam1, TParam2, TParam3)(name As String) As TDelegate
            Return InstanceMethod(Of TDelegate)(name, GetType(TParam1), GetType(TParam2), GetType(TParam3))
        End Function

        Public Function InstanceMethod(Of TDelegate As Class)(name As String, ParamArray typeParameters As Type()) As TDelegate
            Dim paramsTypes = GetFuncDelegateArguments(Of TDelegate)()
            Dim source = paramsTypes.First()
            paramsTypes = paramsTypes.Skip(1).ToArray()
            Dim methodInfo = GetMethodInfo(source, name, paramsTypes, typeParameters)
            Return TryCast(methodInfo.CreateDelegate(GetType(TDelegate)), TDelegate)
        End Function

        <Extension>
        Public Function InstanceMethod(Of TDelegate As Class, TParam1)(source As Type, name As String) As TDelegate
            Return source.InstanceMethod(Of TDelegate)(name, {GetType(TParam1)})
        End Function

        <Extension>
        Public Function InstanceMethod(Of TDelegate As Class, TParam1, TParam2)(source As Type, name As String) As TDelegate
            Return source.InstanceMethod(Of TDelegate)(name, {GetType(TParam1), GetType(TParam2)})
        End Function

        <Extension>
        Public Function InstanceMethod(Of TDelegate As Class, TParam1, TParam2, TParam3)(source As Type, name As String) As TDelegate
            Return source.InstanceMethod(Of TDelegate)(name, {GetType(TParam1), GetType(TParam2), GetType(TParam3)})
        End Function

        <Extension>
        Public Function InstanceMethod(Of TDelegate As Class)(source As Type, name As String, Optional typeParams As Type() = Nothing) As TDelegate
            Dim delegateParams = GetFuncDelegateArguments(Of TDelegate)()
            Dim instanceParam = delegateParams(0)
            delegateParams = delegateParams.Skip(1).ToArray()
            Dim methodInfo = GetMethodInfo(source, name, delegateParams, typeParams)
            If methodInfo Is Nothing Then
                Return Nothing
            End If
            Dim deleg As [Delegate]
            If instanceParam Is source Then
                deleg = methodInfo.CreateDelegate(GetType(TDelegate))
            Else
                Dim sourceParameter = Expression.Parameter(GetType(Object))
                Dim expressions = delegateParams.[Select](AddressOf Expression.Parameter).ToArray()
                Dim returnExpression As Expression = Expression.[Call](Expression.Convert(sourceParameter, source), methodInfo, expressions.Cast(Of Expression)())
                If methodInfo.ReturnType IsNot GetType(System.Void) AndAlso Not methodInfo.ReturnType.IsClass Then
                    returnExpression = Expression.Convert(returnExpression, GetType(Object))
                End If
                Dim lamdaParams = {sourceParameter}.Concat(expressions)
                deleg = Expression.Lambda(returnExpression, lamdaParams).Compile()
            End If
            Return TryCast(deleg, TDelegate)
        End Function

        Public Function InstanceMethod2(Of TDelegate As Class)(methodName As String) As TDelegate
            Dim source = GetType(TDelegate).GenericTypeArguments(0)
            Dim param = Expression.Parameter(source)
            Dim parameters = New List(Of ParameterExpression)() From {
                param
            }
            Dim params = GetFuncDelegateArguments(Of TDelegate)().Skip(1)
            For Each type In params
                parameters.Add(Expression.Parameter(type))
            Next
            Dim te = Expression.Lambda(Expression.[Call](param, methodName, Nothing, parameters.Skip(1).Cast(Of Expression)().ToArray()), parameters)
            Return TryCast(te.Compile(), TDelegate)
        End Function

        <Extension>
        Public Function InstanceMethodVoid(source As Type, name As String, ParamArray paramsTypes As Type()) As Action(Of Object, Object())
            Return InstanceGenericMethod(Of Action(Of Object, Object()))(source, name, Nothing, paramsTypes)
        End Function

        <Extension>
        Public Function PropertyGet(Of TProperty)(source As Type, propertyName As String) As Func(Of Object, TProperty)
            Return TryCast(source.PropertyGet(propertyName), Func(Of Object, TProperty))
        End Function

        <Extension>
        Public Function PropertyGet(source As Type, propertyName As String) As Func(Of Object, Object)
            Dim propertyInfo = GetPropertyInfo(source, propertyName)
            If propertyInfo.GetMethod Is Nothing Then
                Return Nothing
            Else
                Return source.PropertyGet(propertyInfo)
            End If
        End Function

        <Extension>
        Public Function PropertyGet(source As Type, [propertyInfo] As PropertyInfo) As Func(Of Object, Object)
            Dim sourceObjectParam = Expression.Parameter(GetType(Object))
            Dim returnExpression As Expression = Expression.[Call](Expression.Convert(sourceObjectParam, source), propertyInfo.GetMethod)
            If Not propertyInfo.PropertyType.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return DirectCast(Expression.Lambda(returnExpression, sourceObjectParam).Compile(), Func(Of Object, Object))
        End Function

        <Extension> Public Function PropertyGet(Of TSource, TProperty)(propertyInfo As PropertyInfo) As Func(Of TSource, TProperty)
            Return PropertyGet(Of TSource, TProperty)(Nothing, propertyInfo)
        End Function

        Public Function PropertyGet(Of TSource, TProperty)(propertyName As String) As Func(Of TSource, TProperty)
            Return PropertyGet(Of TSource, TProperty)(propertyName, Nothing)
        End Function

        <Obsolete>
        <Extension>
        Public Function PropertyGet2(Of TSource, TProperty)(source As Type, propertyName As String) As Func(Of TSource, TProperty)
            Dim p = Expression.Parameter(source)
            Dim te = Expression.Lambda(Expression.[Property](p, propertyName), p)
            Return DirectCast(te.Compile(), Func(Of TSource, TProperty))
        End Function

        <Extension>
        Public Function PropertySet(Of TSource, TProperty)(source As TSource, propertyName As String) As Action(Of TSource, TProperty)
            Return PropertySet(Of TSource, TProperty)(propertyName)
        End Function

        ''' <summary>
        ''' 为指定类型的对象实例设置属性值，返回空值表名目标属性为一个只读属性或者写过程为私有访问类型
        ''' </summary>
        ''' <typeparam name="TProperty"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="propertyName"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PropertySet(Of TProperty)(source As Type, propertyName As String) As Action(Of Object, TProperty)
            Dim propertyInfo = GetPropertyInfo(source, propertyName)
            If propertyInfo.SetMethod Is Nothing Then
                Return Nothing
            End If
            Dim sourceObjectParam = Expression.Parameter(GetType(Object))
            Dim propertyValueParam As ParameterExpression
            Dim valueExpression As Expression
            If propertyInfo.PropertyType Is GetType(TProperty) Then
                propertyValueParam = Expression.Parameter(propertyInfo.PropertyType)
                valueExpression = propertyValueParam
            Else
                propertyValueParam = Expression.Parameter(GetType(TProperty))
                valueExpression = Expression.Convert(propertyValueParam, propertyInfo.PropertyType)
            End If

            Dim method = Expression.Lambda(
                Expression.[Call](
                    Expression.Convert(sourceObjectParam, source),
                    propertyInfo.SetMethod,
                    valueExpression),
                sourceObjectParam,
                propertyValueParam).Compile()

            Return DirectCast(method, Action(Of Object, TProperty))
        End Function

        <Extension>
        Public Function PropertySet(source As Type, propertyName As String) As Action(Of Object, Object)
            Return source.PropertySet(Of Object)(propertyName)
        End Function

        Public Function PropertySet(Of TSource, TProperty)(propertyName As String) As Action(Of TSource, TProperty)
            Dim source = GetType(TSource)
            Dim propertyInfo = GetPropertyInfo(source, propertyName)
            Return DirectCast(propertyInfo.SetMethod.CreateDelegate(GetType(Action(Of TSource, TProperty))), Action(Of TSource, TProperty))
        End Function

        <Extension>
        Public Function StaticEventAdd(Of TEvent)(source As Type, eventName As String) As Action(Of EventHandler(Of TEvent))
            Dim eventInfo = source.GetEvent(eventName)
            If eventInfo Is Nothing Then
                eventInfo = source.GetEvent(eventName, BindingFlags.NonPublic)
            End If
            If eventInfo Is Nothing Then
                eventInfo = source.GetEvent(eventName, BindingFlags.NonPublic Or BindingFlags.[Public])
            End If
            Return DirectCast(eventInfo.AddMethod.CreateDelegate(GetType(Action(Of EventHandler(Of TEvent)))), Action(Of EventHandler(Of TEvent)))
        End Function

        Public Function StaticFieldGet(Of TSource, TField)(fieldName As String) As Func(Of TField)
            Dim source = GetType(TSource)
            Return source.StaticFieldGet(Of TField)(fieldName)
        End Function

        <Extension>
        Public Function StaticFieldGet(Of TField)(source As Type, fieldName As String) As Func(Of TField)
            Dim fieldInfo = GetStaticFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing Then
                Dim lambda = Expression.Lambda(Expression.Field(Nothing, fieldInfo))
                Return DirectCast(lambda.Compile(), Func(Of TField))
            End If
            Return Nothing
        End Function

        <Extension>
        Public Function StaticFieldGet(source As Type, fieldName As String) As Func(Of Object)
            Dim fieldInfo = GetStaticFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing Then
                Dim returnExpression As Expression = Expression.Field(Nothing, fieldInfo)
                If Not fieldInfo.FieldType.IsClass Then
                    returnExpression = Expression.Convert(returnExpression, GetType(Object))
                End If
                Dim lambda = Expression.Lambda(returnExpression)
                Return DirectCast(lambda.Compile(), Func(Of Object))
            End If
            Return Nothing
        End Function

        <Obsolete>
        Public Function StaticFieldGetExpr(Of TSource, TField)(fieldName As String) As Func(Of TField)
            Dim source = GetType(TSource)
            Dim lambda = Expression.Lambda(Expression.Field(Nothing, source, fieldName))
            Return DirectCast(lambda.Compile(), Func(Of TField))
        End Function

        Public Function StaticFieldSet(Of TSource, TField)(fieldName As String) As Action(Of TField)
            Dim source = GetType(TSource)
            Return source.StaticFieldSet(Of TField)(fieldName)
        End Function

        <Extension>
        Public Function StaticFieldSet(Of TField)(source As Type, fieldName As String) As Action(Of TField)
            Dim fieldInfo = GetStaticFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing Then
                Dim valueParam = Expression.Parameter(GetType(TField))
                Dim lambda = Expression.Lambda(GetType(Action(Of TField)), Expression.Assign(Expression.Field(Nothing, fieldInfo), valueParam), valueParam)
                Return DirectCast(lambda.Compile(), Action(Of TField))
            End If
            Return Nothing
        End Function

        <Extension>
        Public Function StaticFieldSet(source As Type, fieldName As String) As Action(Of Object)
            Dim fieldInfo = GetStaticFieldInfo(source, fieldName)
            If fieldInfo IsNot Nothing AndAlso Not fieldInfo.IsInitOnly Then
                Dim valueParam = Expression.Parameter(GetType(Object))
                Dim convertedValueExpr = Expression.Convert(valueParam, fieldInfo.FieldType)
                Dim lambda = Expression.Lambda(GetType(Action(Of Object)), Expression.Assign(Expression.Field(Nothing, fieldInfo), convertedValueExpr), valueParam)
                Return DirectCast(lambda.Compile(), Action(Of Object))
            End If
            Return Nothing
        End Function

        <Extension>
        Public Function StaticGenericMethod(source As Type, name As String, paramsTypes As Type(), typeParams As Type()) As Func(Of Object(), Object)
            Return StaticMethod(Of Func(Of Object(), Object))(source, name, typeParams, paramsTypes)
        End Function

        <Extension>
        Public Function StaticGenericMethodVoid(source As Type, name As String, paramsTypes As Type(), typeParams As Type()) As Action(Of Object())
            Return StaticMethod(Of Action(Of Object()))(source, name, typeParams, paramsTypes)
        End Function

        Public Function StaticMethod(Of TSource, TDelegate As Class)(name As String) As TDelegate
            Return GetType(TSource).StaticMethod(Of TDelegate)(name)
        End Function

        Public Function StaticMethod(Of TSource, TDelegate As Class, TParam1)(name As String) As TDelegate
            Return GetType(TSource).StaticMethod(Of TDelegate)(name, GetType(TParam1))
        End Function

        Public Function StaticMethod(Of TSource, TDelegate As Class, TParam1, TParam2)(name As String) As TDelegate
            Return GetType(TSource).StaticMethod(Of TDelegate)(name, GetType(TParam1), GetType(TParam2))
        End Function

        Public Function StaticMethod(Of TSource, TDelegate As Class, TParam1, TParam2, TParam3)(name As String) As TDelegate
            Return GetType(TSource).StaticMethod(Of TDelegate)(name, GetType(TParam1), GetType(TParam2), GetType(TParam3))
        End Function

        Public Function StaticMethod(Of TSource, TDelegate As Class)(name As String, ParamArray typeParameters As Type()) As TDelegate
            Return GetType(TSource).StaticMethod(Of TDelegate)(name, typeParameters)
        End Function

        <Extension>
        Public Function StaticMethod(Of TDelegate As Class, TParam1)(source As Type, name As String) As TDelegate
            Return source.StaticMethod(Of TDelegate)(name, GetType(TParam1))
        End Function

        <Extension>
        Public Function StaticMethod(Of TDelegate As Class, TParam1, TParam2)(source As Type, name As String) As TDelegate
            Return source.StaticMethod(Of TDelegate)(name, GetType(TParam1), GetType(TParam2))
        End Function

        <Extension>
        Public Function StaticMethod(Of TDelegate As Class, TParam1, TParam2, TParam3)(source As Type, name As String) As TDelegate
            Return source.StaticMethod(Of TDelegate)(name, GetType(TParam1), GetType(TParam2), GetType(TParam3))
        End Function

        <Extension>
        Public Function StaticMethod(Of TDelegate As Class)(source As Type, name As String, ParamArray typeParameters As Type()) As TDelegate
            Dim paramsTypes = GetFuncDelegateArguments(Of TDelegate)()
            Dim methodInfo = GetStaticMethodInfo(source, name, paramsTypes, typeParameters)
            Return TryCast(methodInfo.CreateDelegate(GetType(TDelegate)), TDelegate)
        End Function

        <Extension>
        Public Function StaticMethod(Of TDelegate As Class)(source As Type, name As String) As TDelegate
            Return source.StaticMethod(Of TDelegate)(name, Nothing)
        End Function

        <Extension>
        Public Function StaticMethod(source As Type, name As String, ParamArray paramsTypes As Type()) As Func(Of Object(), Object)
            Return StaticMethod(Of Func(Of Object(), Object))(source, name, Nothing, paramsTypes)
        End Function

        <Extension>
        Public Function StaticMethod(Of TDelegate As Class)(source As Type, name As String, typeParams As Type(), paramsTypes As Type()) As TDelegate
            Dim methodInfo = GetStaticMethodInfo(source, name, paramsTypes, typeParams)
            If methodInfo Is Nothing Then
                Return Nothing
            End If
            Dim argsArray = Expression.Parameter(GetType(Object()))
            Dim paramsExpression = New Expression(paramsTypes.Length - 1) {}
            For i As Integer = 0 To paramsTypes.Length - 1
                Dim argType = paramsTypes(i)
                paramsExpression(i) = Expression.Convert(Expression.ArrayIndex(argsArray, Expression.Constant(i)), argType)
            Next
            Dim returnExpression As Expression = Expression.[Call](methodInfo, paramsExpression)
            If methodInfo.ReturnType IsNot GetType(System.Void) AndAlso Not methodInfo.ReturnType.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return TryCast(Expression.Lambda(returnExpression, argsArray).Compile(), TDelegate)
        End Function

        <Extension>
        Public Function StaticMethodVoid(source As Type, name As String, ParamArray paramsTypes As Type()) As Action(Of Object())
            Return StaticMethod(Of Action(Of Object()))(source, name, Nothing, paramsTypes)
        End Function

        Public Function StaticPropertyGet(Of TSource, TProperty)(propertyName As String) As Func(Of TProperty)
            Return GetType(TSource).StaticPropertyGet(Of TProperty)(propertyName)
        End Function

        <Extension>
        Public Function StaticPropertyGet(Of TProperty)(source As Type, propertyName As String) As Func(Of TProperty)
            Dim propertyInfo = GetStaticPropertyInfo(source, propertyName)
            Return DirectCast(propertyInfo.GetMethod.CreateDelegate(GetType(Func(Of TProperty))), Func(Of TProperty))
        End Function

        <Extension>
        Public Function StaticPropertyGet(source As Type, propertyName As String) As Func(Of Object)
            Dim propertyInfo = GetStaticPropertyInfo(source, propertyName)
            If propertyInfo.GetMethod Is Nothing Then
                Return Nothing
            End If
            Dim returnExpression As Expression = Expression.[Call](propertyInfo.GetMethod)
            If Not propertyInfo.PropertyType.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return DirectCast(Expression.Lambda(returnExpression).Compile(), Func(Of Object))
        End Function

        <Obsolete>
        <Extension>
        Public Function StaticPropertyGet2(Of TProperty)(source As Type, propertyName As String) As Func(Of TProperty)
            Dim te = Expression.Lambda(Expression.[Property](Nothing, source, propertyName))
            Return DirectCast(te.Compile(), Func(Of TProperty))
        End Function

        Public Function StaticPropertySet(Of TSource, TProperty)(propertyName As String) As Action(Of TProperty)
            Return GetType(TSource).StaticPropertySet(Of TProperty)(propertyName)
        End Function

        <Extension>
        Public Function StaticPropertySet(Of TProperty)(source As Type, propertyName As String) As Action(Of TProperty)
            Dim propertyInfo = GetStaticPropertyInfo(source, propertyName)
            Return DirectCast(propertyInfo.SetMethod.CreateDelegate(GetType(Action(Of TProperty))), Action(Of TProperty))
        End Function

        <Extension>
        Public Function StaticPropertySet(source As Type, propertyName As String) As Action(Of Object)
            Dim propertyInfo = GetStaticPropertyInfo(source, propertyName)
            If propertyInfo.GetMethod Is Nothing Then
                Return Nothing
            End If
            Dim valueParam = Expression.Parameter(GetType(Object))
            Dim convertedValue = Expression.Convert(valueParam, propertyInfo.PropertyType)
            Dim returnExpression As Expression = Expression.[Call](propertyInfo.SetMethod, convertedValue)
            If Not propertyInfo.PropertyType.IsClass Then
                returnExpression = Expression.Convert(returnExpression, GetType(Object))
            End If
            Return DirectCast(Expression.Lambda(returnExpression, valueParam).Compile(), Action(Of Object))
        End Function

        Private Function EventAccessor(Of TEventArgs)(source As Type, eventName As String, accessorName As String) As Action(Of Object, EventHandler(Of TEventArgs))
            Dim accessor = GetEventInfoAccessor(eventName, source, accessorName)
            If accessor IsNot Nothing Then
                Dim instanceParameter = Expression.Parameter(GetType(Object))
                Dim delegateTypeParameter = Expression.Parameter(GetType(EventHandler(Of TEventArgs)))
                Dim lambda = Expression.Lambda(Expression.[Call](Expression.Convert(instanceParameter, source), accessor, delegateTypeParameter), instanceParameter, delegateTypeParameter)
                Return DirectCast(lambda.Compile(), Action(Of Object, EventHandler(Of TEventArgs)))
            End If
            Return Nothing
        End Function

        Private Function EventAccessor(Of TSource, TEventArgs)(eventName As String, accessorName As String) As Action(Of TSource, EventHandler(Of TEventArgs))
            Dim sourceType = GetType(TSource)
            Dim accessor = GetEventInfoAccessor(eventName, sourceType, accessorName)
            Return DirectCast(accessor.CreateDelegate(GetType(Action(Of TSource, EventHandler(Of TEventArgs)))), Action(Of TSource, EventHandler(Of TEventArgs)))
        End Function

        Private Function EventAccessorImpl(Of TDelegate As Class)(source As Type, eventName As String, accessorName As String) As TDelegate
            Dim eventInfo = GetEventInfo(eventName, source)
            If eventInfo IsNot Nothing Then
                Dim accessor = If(accessorName = AddAccessor, eventInfo.AddMethod, eventInfo.RemoveMethod)
                Dim eventArgsType = eventInfo.EventHandlerType.GetGenericArguments()(0)
                Dim instanceParameter = Expression.Parameter(GetType(Object))
                Dim delegateTypeParameter = Expression.Parameter(GetType(Object))
                Dim methodCallExpression = Expression.[Call](EventHandlerFactoryMethodInfo.MakeGenericMethod(eventArgsType, source), delegateTypeParameter, Expression.Constant(accessorName = RemoveAccessor))
                Dim lambda = Expression.Lambda(Expression.[Call](Expression.Convert(instanceParameter, source), accessor, methodCallExpression), instanceParameter, delegateTypeParameter)
                Return TryCast(lambda.Compile(), TDelegate)
            End If
            Return Nothing
        End Function

        <Extension>
        Private Function EventAddImpl(Of TDelegate As Class)(source As Type, eventName As String) As TDelegate
            Return EventAccessorImpl(Of TDelegate)(source, eventName, AddAccessor)
        End Function

        <Extension>
        Private Function EventRemoveImpl(Of TDelegate As Class)(source As Type, eventName As String) As TDelegate
            Return EventAccessorImpl(Of TDelegate)(source, eventName, RemoveAccessor)
        End Function

        Private Function GetConstructorInfo(source As Type, types As Type()) As ConstructorInfo
            Return If((If(source.GetConstructor(BindingFlags.[Public], Nothing, types, Nothing), source.GetConstructor(BindingFlags.NonPublic, Nothing, types, Nothing))), source.GetConstructor(BindingFlags.NonPublic Or BindingFlags.[Public] Or BindingFlags.Instance, Nothing, types, Nothing))
        End Function

        Private Function GetEventInfo(eventName As String, sourceType As Type) As EventInfo
            Return If((If(sourceType.GetEvent(eventName), sourceType.GetEvent(eventName, BindingFlags.NonPublic))), sourceType.GetEvent(eventName, BindingFlags.NonPublic Or BindingFlags.[Public] Or BindingFlags.Instance))
        End Function

        Private Function GetEventInfoAccessor(eventName As String, sourceType As Type, accessor As String) As MethodInfo
            Dim eventInfo = GetEventInfo(eventName, sourceType)
            Return If(accessor = AddAccessor, eventInfo.AddMethod, eventInfo.RemoveMethod)
        End Function

        Private Function GetFieldInfo(source As Type, fieldName As String) As FieldInfo
            Dim fieldInfo = If((If(source.GetField(fieldName), source.GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic))), source.GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.[Public]))
            Return fieldInfo
        End Function

        Private Function GetFuncDelegateArguments(Of TDelegate As Class)() As Type()
            Dim arguments As IEnumerable(Of Type) = GetType(TDelegate).GenericTypeArguments
            Return arguments.Reverse().Skip(1).Reverse().ToArray()
        End Function

        Private Function GetFuncDelegateReturnType(Of TDelegate As Class)() As Type
            Return GetType(TDelegate).GenericTypeArguments.Last()
        End Function

        Private Function GetIndexerPropertyInfo(source As Type, returnType As Type, indexesTypes As Type(), Optional indexerName As String = Nothing) As PropertyInfo
            indexerName = If(indexerName, Item)

            Dim propertyInfo = If((If(source.GetProperty(indexerName, returnType, indexesTypes), source.GetProperty(indexerName, BindingFlags.NonPublic, Nothing, returnType, indexesTypes, Nothing))), source.GetProperty(indexerName, BindingFlags.NonPublic Or BindingFlags.[Public] Or BindingFlags.Instance, Nothing, returnType, indexesTypes, Nothing))
            If propertyInfo IsNot Nothing Then
                Return propertyInfo
            End If
            Dim indexer = source.GetProperties().FirstOrDefault(Function(p) p.GetIndexParameters().Length > 0)
            Return If(indexer IsNot Nothing, GetIndexerPropertyInfo(source, returnType, indexesTypes, indexer.Name), Nothing)
        End Function

        Private Function GetMethodInfo(source As Type, name As String, parametersTypes As Type(), Optional typeParameters As Type() = Nothing) As MethodInfo
            Dim methodInfo As MethodInfo = Nothing
            Try
                methodInfo = If((If(source.GetMethod(name, BindingFlags.Instance Or BindingFlags.[Public], Nothing, parametersTypes, Nothing), source.GetMethod(name, BindingFlags.Instance Or BindingFlags.NonPublic, Nothing, parametersTypes, Nothing))), source.GetMethod(name, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.[Public], Nothing, parametersTypes, Nothing))
                'swallow and test generics
            Catch generatedExceptionName As AmbiguousMatchException
            End Try
            'check for generic methods
            If typeParameters IsNot Nothing Then
                Dim ms = source.GetMethods(BindingFlags.Instance Or BindingFlags.[Public]).Concat(source.GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance)).Concat(source.GetMethods(BindingFlags.[Public] Or BindingFlags.NonPublic Or BindingFlags.Instance))
                For Each m In ms
                    If m.Name = name AndAlso m.IsGenericMethod Then
                        Dim parameters = m.GetParameters()
                        Dim genericArguments = m.GetGenericArguments()
                        Dim parametersTypesValid = parameters.Length = parametersTypes.Length
                        parametersTypesValid = parametersTypesValid And genericArguments.Length = typeParameters.Length
                        If Not parametersTypesValid Then
                            Continue For
                        End If
                        For index As Integer = 0 To parameters.Length - 1
                            Dim parameterInfo = parameters(index)
                            Dim parameterType = parametersTypes(index)
                            If parameterInfo.ParameterType IsNot parameterType AndAlso parameterInfo.ParameterType.IsGenericParameter AndAlso Not parameterInfo.ParameterType.CanBeAssignedFrom(parameterType) Then
                                parametersTypesValid = False
                                Exit For
                            End If
                        Next
                        For index As Integer = 0 To genericArguments.Length - 1
                            Dim genericArgument = genericArguments(index)
                            Dim typeParameter = typeParameters(index)
                            If Not genericArgument.CanBeAssignedFrom(typeParameter) Then
                                parametersTypesValid = False
                                Exit For
                            End If
                        Next
                        If parametersTypesValid Then
                            methodInfo = m.MakeGenericMethod(typeParameters)
                            Exit For
                        End If
                    End If
                Next
            End If
            Return methodInfo
        End Function

        Private Function GetPropertyInfo(source As Type, propertyName As String) As PropertyInfo
            Dim propertyInfo = If(source.GetProperty(propertyName), If(source.GetProperty(propertyName, BindingFlags.NonPublic), source.GetProperty(propertyName, BindingFlags.NonPublic Or BindingFlags.[Public] Or BindingFlags.Instance)))
            Return propertyInfo
        End Function

        Private Function GetStaticFieldInfo(source As Type, fieldName As String) As FieldInfo
            Dim fieldInfo = If((If(source.GetField(fieldName, BindingFlags.[Static]), source.GetField(fieldName, BindingFlags.[Static] Or BindingFlags.NonPublic))), source.GetField(fieldName, BindingFlags.[Static] Or BindingFlags.NonPublic Or BindingFlags.[Public]))
            Return fieldInfo
        End Function

        Private Function GetStaticMethodInfo(source As Type, name As String, parametersTypes As Type(), Optional typeParameters As Type() = Nothing) As MethodInfo
            Dim methodInfo As MethodInfo = Nothing
            Try
                methodInfo = If((If(source.GetMethod(name, BindingFlags.[Static], Nothing, parametersTypes, Nothing), source.GetMethod(name, BindingFlags.[Static] Or BindingFlags.NonPublic, Nothing, parametersTypes, Nothing))), source.GetMethod(name, BindingFlags.[Static] Or BindingFlags.NonPublic Or BindingFlags.[Public], Nothing, parametersTypes, Nothing))
                'swallow and test generics
            Catch generatedExceptionName As AmbiguousMatchException
            End Try
            'check for generic methods
            If typeParameters IsNot Nothing Then
                Dim ms = source.GetMethods(BindingFlags.[Static]).Concat(source.GetMethods(BindingFlags.NonPublic Or BindingFlags.[Static])).Concat(source.GetMethods(BindingFlags.[Public] Or BindingFlags.NonPublic Or BindingFlags.[Static]))
                For Each m In ms
                    If m.Name = name AndAlso m.IsGenericMethod Then
                        Dim parameters = m.GetParameters()
                        Dim genericArguments = m.GetGenericArguments()
                        Dim parametersTypesValid = parameters.Length = parametersTypes.Length
                        parametersTypesValid = parametersTypesValid And genericArguments.Length = typeParameters.Length
                        If Not parametersTypesValid Then
                            Continue For
                        End If
                        For index As Integer = 0 To parameters.Length - 1
                            Dim parameterInfo = parameters(index)
                            Dim parameterType = parametersTypes(index)
                            If parameterInfo.ParameterType IsNot parameterType AndAlso parameterInfo.ParameterType.IsGenericParameter AndAlso Not parameterInfo.ParameterType.CanBeAssignedFrom(parameterType) Then
                                parametersTypesValid = False
                                Exit For
                            End If
                        Next
                        For index As Integer = 0 To genericArguments.Length - 1
                            Dim genericArgument = genericArguments(index)
                            Dim typeParameter = typeParameters(index)
                            If Not genericArgument.CanBeAssignedFrom(typeParameter) Then
                                parametersTypesValid = False
                                Exit For
                            End If
                        Next
                        If parametersTypesValid Then
                            methodInfo = m.MakeGenericMethod(typeParameters)
                            Exit For
                        End If
                    End If
                Next
            End If
            Return methodInfo
        End Function

        Private Function GetStaticPropertyInfo(source As Type, propertyName As String) As PropertyInfo
            Dim propertyInfo = If((If(source.GetProperty(propertyName, BindingFlags.[Static]), source.GetProperty(propertyName, BindingFlags.[Static] Or BindingFlags.NonPublic))), source.GetProperty(propertyName, BindingFlags.[Static] Or BindingFlags.NonPublic Or BindingFlags.[Public]))
            Return propertyInfo
        End Function

        Private Function PropertyGet(Of TSource, TProperty)(Optional propertyName As String = Nothing, Optional propertyInfo As PropertyInfo = Nothing) As Func(Of TSource, TProperty)
            Dim source = GetType(TSource)
            propertyInfo = If(propertyInfo, GetPropertyInfo(source, propertyName))
            Return DirectCast(propertyInfo.GetMethod.CreateDelegate(GetType(Func(Of TSource, TProperty))), Func(Of TSource, TProperty))
        End Function
    End Module
End Namespace
