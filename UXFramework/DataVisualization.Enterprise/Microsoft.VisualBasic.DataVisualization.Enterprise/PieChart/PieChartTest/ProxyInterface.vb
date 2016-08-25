#Region "Microsoft.VisualBasic::edc58333ddb8a060ca24c6efe01f1bb5, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartTest\ProxyInterface.vb"

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

''' Author:  Matthew Johnson
''' Version: 1.0
''' Date:    March 13, 2006
''' Notice:  You are free to use this code as you wish.  There are no guarantees whatsoever about
''' its usability or fitness of purpose.

#Region "using"
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Reflection
#End Region

Namespace Nexus.Reflection
	''' <summary>
	''' Provides a base for implementers who want their properties and methods to be bound to one or more
	''' other classes.
	''' </summary>
	''' <remarks>
	''' Implementers of this class should call <see cref="ProxySet"/>, <see cref="ProxyGet"/>, and <see cref="ProxyInvoke"/>
	''' to have the ProxyInterface automatically call properties and methods on objects that are in list
	''' of proxy objects.  By default, the ProxyInterface will attempt to call a method with the same name
	''' and parameters as the original method.  However, you can bind each proxy call to different methods for different
	''' types.  For each type that this class will proxy, you can add a <see cref="ProxyTypeBindingAttribute"/> to
	''' a property or method that specifies with member of the proxied type should be accessed.
	''' </remarks>
	Public MustInherit Class ProxyInterface
		#Region "Fields"
		Private objects As New List(Of Object)()
		Private attributes As New Dictionary(Of MethodBase, ProxyAttributeCollection)()
		#End Region

		#Region "Methods"
		''' <summary>
		''' Performs the get operation on the first object in the list of proxy objects (the default object).
		''' </summary>
		''' <typeparam name="T">The type to cast the return value as.</typeparam>
		''' <param name="args">The indices to pass to the get method.</param>
		''' <returns>The result of executing a get operation on the default proxy object, or the default value of T
		''' if there are no proxy objects.
		''' </returns>
		Public Function ProxyGet(Of T)(ParamArray args As Object()) As T
			If objects.Count = 0 Then
				Return Nothing
			End If

			Dim trace As New StackTrace()
			Dim frame As StackFrame = trace.GetFrame(1)
			Dim method As MethodInfo = TryCast(frame.GetMethod(), MethodInfo)
			Dim attributes As ProxyAttributeCollection = GetAttributes(method)
			Dim binding As ProxyTypeBindingAttribute = attributes.GetTypeBinding(objects(0))
			Dim accessor As MemberInfoAccessor = binding.GetTargetAccessor(GetType(T), args)
			Return DirectCast(accessor.[Get](objects(0), args), T)
		End Function

		''' <summary>
		''' Performs the set operation on all proxy objects.
		''' </summary>
		''' <param name="value">The value to set on the proxy objects.</param>
		''' <param name="args">The indices of the set operation.</param>
		Protected Sub ProxySet(value As Object, ParamArray args As Object())
			If objects.Count = 0 Then
				Return
			End If

			Dim trace As New StackTrace()
			Dim frame As StackFrame = trace.GetFrame(1)
			Dim method As MethodInfo = TryCast(frame.GetMethod(), MethodInfo)
			Dim attributes As ProxyAttributeCollection = GetAttributes(method)
			For Each invoke As Object In objects
				Dim binding As ProxyTypeBindingAttribute = attributes.GetTypeBinding(invoke)
				Dim argsWithValue As Object() = New Object(args.Length) {}
				argsWithValue(0) = value
				Array.Copy(args, 0, argsWithValue, 1, args.Length)

				Dim accessor As MemberInfoAccessor = binding.GetTargetAccessor(GetType(System.Void), argsWithValue)
				accessor.[Set](invoke, value, args)
			Next
		End Sub

		''' <summary>
		''' Invokes proxy methods which do not return a value.
		''' </summary>
		''' <param name="args">The arguments to pass to the method.</param>
		Protected Sub ProxyInvoke(ParamArray args As Object())
			If objects.Count = 0 Then
				Return
			End If

			Dim trace As New StackTrace()
			Dim frame As StackFrame = trace.GetFrame(1)
			Dim method As MethodInfo = TryCast(frame.GetMethod(), MethodInfo)
			Dim attributes As ProxyAttributeCollection = GetAttributes(method)

			For Each invoke As Object In objects
				Dim binding As ProxyTypeBindingAttribute = attributes.GetTypeBinding(invoke)
				Dim accessor As MemberInfoAccessor = binding.GetTargetAccessor(GetType(System.Void), args)
				accessor.[Get](invoke, args)
			Next
		End Sub

		''' <summary>
		''' Invokes proxy methods which return a value.
		''' </summary>
		''' <typeparam name="T">The return type of the method being invoked.</typeparam>
		''' <param name="args">The arguments to pass to the method.</param>
		''' <returns>The return value of the default proxy method.</returns>
		''' <remarks>This method will invoke methods on all of the proxy objects, but will return
		''' the result of the invocation on the default object.
		''' </remarks>
		Protected Function ProxyInvoke(Of T)(ParamArray args As Object()) As T
			If objects.Count = 0 Then
				Return Nothing
			End If

			Dim trace As New StackTrace()
			Dim frame As StackFrame = trace.GetFrame(1)
			Dim method As MethodInfo = TryCast(frame.GetMethod(), MethodInfo)
			Dim attributes As ProxyAttributeCollection = GetAttributes(method)

			Dim first As Boolean = True
			Dim result As T = Nothing
			For Each invoke As Object In objects
				Dim binding As ProxyTypeBindingAttribute = attributes.GetTypeBinding(invoke)
				Dim accessor As MemberInfoAccessor = binding.GetTargetAccessor(GetType(T), args)
				If first Then
					result = DirectCast(accessor.[Get](invoke, args), T)
					first = False
				Else
					accessor.[Get](invoke, args)
				End If
			Next

			Return result
		End Function

		''' <summary>
		''' Adds an object to the list of proxy objects.
		''' </summary>
		''' <param name="proxy">The object to proxy.</param>
		Protected Sub AddProxyObject(proxy As Object)
			objects.Add(proxy)
		End Sub

		''' <summary>
		''' Adds an object to the list of proxy objects and makes it the default object.
		''' </summary>
		''' <param name="proxy"></param>
		Protected Sub AddDefaultProxyObject(proxy As Object)
			objects.Insert(0, proxy)
		End Sub

		''' <summary>
		''' Removes an object from the list of proxy objects.
		''' </summary>
		''' <param name="proxy">The object to remove from the list.</param>
		Protected Sub RemoveProxyObject(proxy As Object)
			objects.Remove(proxy)
		End Sub

		''' <summary>
		''' Gets the <see cref="ProxyAttributeCollection"/> for a given <see cref="MethodInfo"/>.
		''' </summary>
		''' <param name="method">The method to get the proxy attributes for.</param>
		''' <returns>The collection of attributes for the given method as
		''' revealed by reflection of the method's attributes.
		''' </returns>
		Private Function GetAttributes(method As MethodInfo) As ProxyAttributeCollection
			If Not attributes.ContainsKey(method) Then
				Dim collection As ProxyAttributeCollection = ReflectMethod(method)
				attributes.Add(method, collection)
			End If

			Return attributes(method)
		End Function

		''' <summary>
		''' Performs reflection on a method in order to determine which methods on proxy objects
		''' the method stub is bound to.
		''' </summary>
		''' <param name="method">The method to reflect over.</param>
		''' <returns>The collection of attributes about the method.</returns>
		Private Function ReflectMethod(method As MethodInfo) As ProxyAttributeCollection
			Dim collection As New ProxyAttributeCollection(method)

			' get the information that is applied directly to the method
			Dim typeBindings As Object() = method.GetCustomAttributes(GetType(ProxyTypeBindingAttribute), True)
			For Each attribute As ProxyTypeBindingAttribute In typeBindings
				collection.AddTypeBinding(attribute)
			Next

			' if the method is a get or set method on a property, also get the attributes applied to the property
			If MemberInfoAccessor.IsGetMethod(method) OrElse MemberInfoAccessor.IsSetMethod(method) Then
				Dim propertyInfo As PropertyInfo = MemberInfoAccessor.GetProperty(method)
				Dim moreBindings As Object() = propertyInfo.GetCustomAttributes(GetType(ProxyTypeBindingAttribute), True)
				For Each attribute As ProxyTypeBindingAttribute In moreBindings
					collection.AddTypeBinding(attribute)
				Next
			End If

			Return collection
		End Function
		#End Region
	End Class
End Namespace
