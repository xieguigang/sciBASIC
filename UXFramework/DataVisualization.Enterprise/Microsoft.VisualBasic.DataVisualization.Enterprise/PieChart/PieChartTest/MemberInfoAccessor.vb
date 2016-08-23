#Region "Microsoft.VisualBasic::fffb1039585c5ed91cb0c1ccdeb5053a, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartTest\MemberInfoAccessor.vb"

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
	Public MustInherit Class MemberInfoAccessor
		#Region "Factory Methods"
		Public Shared Function BindMember(member As MemberInfo) As MemberInfoAccessor
			If TypeOf member Is FieldInfo Then
				Return New FieldInfoAccessor(TryCast(member, FieldInfo))
			ElseIf TypeOf member Is PropertyInfo Then
				Return New PropertyInfoAccessor(TryCast(member, PropertyInfo))
			ElseIf TypeOf member Is MethodInfo Then
				Return New MethodInfoAccessor(TryCast(member, MethodInfo))
			End If

			Throw New Exception()
		End Function
		#End Region

		#Region "Fields"
		Private Const GetPrefix As String = "get_"
		Private Const SetPrefix As String = "set_"
		#End Region

		#Region "Methods"
		Public MustOverride ReadOnly Property MemberInfo() As MemberInfo
		Public MustOverride Function [Get](target As Object, ParamArray args As Object()) As Object
		Public MustOverride Sub [Set](target As Object, value As Object, ParamArray args As Object())

		Friend Shared Function IsGetMethod(method As MethodInfo) As Boolean
			Return method.IsSpecialName AndAlso method.Name.StartsWith(GetPrefix)
		End Function

		Friend Shared Function IsSetMethod(method As MethodInfo) As Boolean
			Return method.IsSpecialName AndAlso method.Name.StartsWith(SetPrefix)
		End Function

		Friend Shared Function GetPropertyName(method As MethodInfo) As String
			Return method.Name.Substring(GetPrefix.Length)
		End Function

		Friend Shared Function GetProperty(method As MethodInfo) As PropertyInfo
			If IsGetMethod(method) Then
				Return method.ReflectedType.GetProperty(GetPropertyName(method), BindingFlags.Instance Or BindingFlags.[Public] Or BindingFlags.NonPublic)
			ElseIf IsSetMethod(method) Then
				Return method.ReflectedType.GetProperty(GetPropertyName(method), BindingFlags.Instance Or BindingFlags.[Public] Or BindingFlags.NonPublic)
			End If

			Return Nothing
		End Function

		'internal static bool VerifyMatch(MethodInfo methodBase, MethodInfo methodInvoke)
		'{
		'  // check the return values
		'  if (methodBase.ReturnType != methodInvoke.ReturnType)
		'  {
		'    return false;
		'  }

		'  ParameterInfo[] baseParameters = methodBase.GetParameters();
		'  ParameterInfo[] invokeParameters = methodInvoke.GetParameters();

		'  if (baseParameters.Length != invokeParameters.Length)
		'  {
		'    return false;
		'  }

		'  for (int i = 0; i < baseParameters.Length; i++)
		'  {
		'    if (baseParameters[i].ParameterType != invokeParameters[i].ParameterType)
		'    {
		'      return false;
		'    }
		'  }

		'  return true;
		'}

		Friend Shared Function VerifyMatch(method As MethodInfo, returnType As Type, args As Object()) As Boolean
			If method.ReturnType IsNot returnType Then
				Return False
			End If

			Dim parameters As ParameterInfo() = method.GetParameters()

			If parameters.Length <> args.Length Then
				Return False
			End If

			For i As Integer = 0 To parameters.Length - 1
				If Not (parameters(i).ParameterType.IsAssignableFrom(args(i).[GetType]())) Then
					Return False
				End If
			Next

			Return True
		End Function
		#End Region

		#Region "class FieldInfoAccessor"
		Private Class FieldInfoAccessor
			Inherits MemberInfoAccessor
			Public Sub New(field As FieldInfo)
				Me.field = field
			End Sub

			Private field As FieldInfo

			Public Overrides ReadOnly Property MemberInfo() As MemberInfo
				Get
					Return field
				End Get
			End Property

			Public Overrides Function [Get](target As Object, ParamArray args As Object()) As Object
				If args.Length > 0 Then
						'todo: better exception
					Throw New Exception()
				End If

				Return field.GetValue(target)
			End Function

			Public Overrides Sub [Set](target As Object, value As Object, ParamArray args As Object())
				If args.Length > 0 Then
						'todo: better exception
					Throw New Exception()
				End If

				field.SetValue(target, value)
			End Sub
		End Class
		#End Region

		#Region "class PropertyInfoAccessor"
		Private Class PropertyInfoAccessor
			Inherits MemberInfoAccessor
			Public Sub New([property] As PropertyInfo)
				Me.[property] = [property]
			End Sub

			Private [property] As PropertyInfo

			Public Overrides ReadOnly Property MemberInfo() As MemberInfo
				Get
					Return [property]
				End Get
			End Property

			Public Overrides Function [Get](target As Object, ParamArray args As Object()) As Object
				Try
					Return [property].GetValue(target, args)
				Catch
						'todo
					Throw New Exception()
				End Try
			End Function

			Public Overrides Sub [Set](target As Object, value As Object, ParamArray args As Object())
				Try
					[property].SetValue(target, value, args)
				Catch
						'todo
					Throw New Exception()
				End Try
			End Sub
		End Class
		#End Region

		#Region "class MethodInfoAccessor"
		Private Class MethodInfoAccessor
			Inherits MemberInfoAccessor
			Public Sub New(method As MethodInfo)
				Me.method = method
			End Sub

			Private method As MethodInfo

			Public Overrides ReadOnly Property MemberInfo() As MemberInfo
				Get
					Return method
				End Get
			End Property

			Public Overrides Function [Get](target As Object, ParamArray args As Object()) As Object
				Try
					Return method.Invoke(target, args)
				Catch
					Throw New Exception()
				End Try
			End Function

			Public Overrides Sub [Set](target As Object, value As Object, ParamArray args As Object())
				Try
					Dim invoke As Object() = New Object(args.Length) {}
					invoke(0) = value
					Array.Copy(args, 0, invoke, 1, args.Length)
					method.Invoke(target, invoke)
				Catch
					Throw New Exception()
				End Try
			End Sub
		End Class
		#End Region
	End Class
End Namespace
