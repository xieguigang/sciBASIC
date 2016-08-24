#Region "Microsoft.VisualBasic::0eb34c5b0cbadce308ea35de4d2e72ba, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartTest\ProxyAttributes.vb"

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
Imports System.Reflection
#End Region

Namespace Nexus.Reflection
	''' <summary>
	''' Collects information about all of the attributes that apply to a method.  For
	''' property get and set methods, this includes attributes that are applied directly
	''' to the property and not to the get or set method.  This information governs
	''' how the proxy stub method is bound to instance methods of objects of different types.
	''' </summary>
	Public Class ProxyAttributeCollection
		#Region "Constructor"
		''' <summary>
		''' Constructs a new ProxyAttributeCollection.
		''' </summary>
		''' <param name="method">The method that is bound to this collection of attributes.</param>
		Friend Sub New(method As MethodInfo)
			Me.m_method = method
		End Sub
		#End Region

		#Region "Fields"
		''' <summary>
		''' The method bound to the collection of type bindings.
		''' </summary>
		Private m_method As MethodInfo

		''' <summary>
		''' A dictionary that maps types to the <see cref="ProxyTypeBindingAttribute"/>s that govern how this method is
		''' bound to that type.
		''' </summary>
		Private typeBindings As New Dictionary(Of Type, ProxyTypeBindingAttribute)()
		#End Region

		#Region "Properties"
		''' <summary>
		''' Gets the method associated with this collection of attributes.
		''' </summary>
		Public ReadOnly Property Method() As MethodInfo
			Get
				Return m_method
			End Get
		End Property
		#End Region

		#Region "Methods"
		''' <summary>
		''' Adds a binding that relates a type with a <see cref="ProxyTypeBindingAttribute"/>.
		''' </summary>
		''' <param name="attribute">The attributes that govern the invocation of methods on objects of a certain type.</param>
		Public Sub AddTypeBinding(attribute As ProxyTypeBindingAttribute)
			Try
				Me.typeBindings.Add(attribute.Type, attribute)
			Catch generatedExceptionName As ArgumentException
				Throw New ProxyAttributeReflectionException()
			End Try
		End Sub

		''' <summary>
		''' Gets the attribute that should be used for a given object instance.
		''' </summary>
		''' <param name="instance">The object instance that is going to be invoked by
		''' this method stub.</param>
		''' <returns>The attributes governing the invocation of methods on the instance object.</returns>
		''' <remarks>
		''' This method returns the best match attribute for the instance object.  For example, if the object
		''' instance is of type TextBox, and the list of bindings contains entries for the TextBoxBase and the
		''' Control classes, then the attributes for the TextBoxBase would be used for this object, since
		''' TextBoxBase is closer to TextBox in the inheritance hierarchy than Control.
		''' </remarks>
		Public Function GetTypeBinding(instance As Object) As ProxyTypeBindingAttribute
			' get the instance of the type to get the attributes for
			Dim instanceType As Type = instance.[GetType]()

			' if the type is already here, return it
			If typeBindings.ContainsKey(instanceType) Then
				Return typeBindings(instanceType)
			End If

			' look for the correct type, which is the type in the list that is closest to the instance type in the inheritance hierarcy
			Dim correctType As Type = Nothing
			For Each type As Type In typeBindings.Keys
				If type.IsAssignableFrom(instanceType) AndAlso (correctType Is Nothing OrElse correctType.IsAssignableFrom(type)) Then
					correctType = type
				End If
			Next

			' if no types are in the inheritance hierarchy, use the instance type
			If correctType Is Nothing Then
				correctType = instanceType
			End If

			' if the type isn't in the list, assume that the method/property/field has the same name
			If Not typeBindings.ContainsKey(correctType) Then
				If MemberInfoAccessor.IsGetMethod(Method) OrElse MemberInfoAccessor.IsSetMethod(Method) Then
					typeBindings.Add(correctType, New ProxyTypeBindingAttribute(correctType, MemberInfoAccessor.GetProperty(Method).Name))
				Else
					typeBindings.Add(correctType, New ProxyTypeBindingAttribute(correctType, Method.Name))
				End If
			End If

			Return typeBindings(correctType)
		End Function
		#End Region
	End Class

	''' <summary>
	''' Determines how a method or property stub will be invoked on remote objects.
	''' </summary>
	''' <remarks>
	''' By default, when a proxy stub is invoked and one of the proxy methods is called, each instance
	''' being proxied will be invoked with a method or property of the same name.  Use this attribute
	''' to redirect method or property invocation to a different name.  For all objects of the given type,
	''' the method, property, or field with the name targetName will be used.
	''' </remarks>
	<AttributeUsage(AttributeTargets.[Property] Or AttributeTargets.Method, AllowMultiple := True)> _
	Public Class ProxyTypeBindingAttribute
		Inherits Attribute
		#Region "Constructor"
		''' <summary>
		''' Constructs a new ProxyTypeBindingAttribute.
		''' </summary>
		''' <param name="type">The type of objects this binding applies to.</param>
		''' <param name="targetName">The name of the method, field, or property of the type that should be bound to.</param>
		Public Sub New(type As Type, targetName As String)
			Me.m_type = type
			Me.m_targetName = targetName
		End Sub
		#End Region

		#Region "Fields"
		''' <summary>
		''' The type of object that this attribute governs.
		''' </summary>
		Private m_type As Type

		''' <summary>
		''' The name of the method, field, or property on the type that will be invoked.
		''' </summary>
		Private m_targetName As String
		#End Region

		#Region "Properties"
		''' <summary>
		''' Gets the type of object that this attribute governs.
		''' </summary>
		Public ReadOnly Property Type() As Type
			Get
				Return m_type
			End Get
		End Property

		''' <summary>
		''' Gets the target name of the method, field, or property on the type that will be invoked.
		''' </summary>
		Public ReadOnly Property TargetName() As String
			Get
				Return m_targetName
			End Get
		End Property
		#End Region

		#Region "Methods"
		''' <summary>
		''' Gets the accessor object that can be used to invoke the method, field, or property on the type of object with the
		''' given return value and arguments.
		''' </summary>
		''' <param name="returnType">The return type of the method, field, or property.</param>
		''' <param name="arguments">The arguments passed to the method, field, or property.</param>
		''' <returns>An object which can be used to invoke the member that matches the given name, return type,
		''' and arguments.
		''' </returns>
		Public Function GetTargetAccessor(returnType As Type, arguments As Object()) As MemberInfoAccessor
			' get an array of members
			Dim members As MemberInfo() = Type.GetMember(TargetName, BindingFlags.Instance Or BindingFlags.[Public] Or BindingFlags.NonPublic)

			' look for the member that will fit the caller
			For Each member As MemberInfo In members
				If member.MemberType = MemberTypes.[Property] Then
					Dim propertyInfo As PropertyInfo = TryCast(member, PropertyInfo)
					Dim propertyGetMethod As MethodInfo = propertyInfo.GetGetMethod(True)
					If MemberInfoAccessor.VerifyMatch(propertyGetMethod, returnType, arguments) Then
						Return MemberInfoAccessor.BindMember(propertyGetMethod)
					End If

					Dim propertySetMethod As MethodInfo = propertyInfo.GetSetMethod(True)
					If MemberInfoAccessor.VerifyMatch(propertySetMethod, returnType, arguments) Then
						Return MemberInfoAccessor.BindMember(propertySetMethod)
					End If
				ElseIf member.MemberType = MemberTypes.Field Then
					Dim fieldInfo As FieldInfo = TryCast(member, FieldInfo)
					If (returnType Is fieldInfo.FieldType AndAlso arguments.Length = 0) Then
						Return MemberInfoAccessor.BindMember(fieldInfo)
					End If

					If returnType Is GetType(System.Void) AndAlso arguments.Length = 1 AndAlso arguments(0).[GetType]().IsAssignableFrom(fieldInfo.FieldType) Then
						Return MemberInfoAccessor.BindMember(fieldInfo)
					End If
				ElseIf member.MemberType = MemberTypes.Method Then
					Dim methodInfo As MethodInfo = TryCast(member, MethodInfo)
					If MemberInfoAccessor.VerifyMatch(methodInfo, returnType, arguments) Then
						Return MemberInfoAccessor.BindMember(methodInfo)
					End If
				End If
			Next

			Throw New MissingProxyTargetException()
		End Function
		#End Region
	End Class
End Namespace
