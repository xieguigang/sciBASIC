#Region "Microsoft.VisualBasic::062abf9170cbd65288b462d737aac04f, vs_solutions\dev\ApplicationServices\CodeDOM\CodeDOMExpressions.vb"

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

    '     Module CodeDOMExpressions
    ' 
    '         Properties: EntryPoint
    ' 
    '         Function: (+4 Overloads) [Call], [CType], [GetType], (+4 Overloads) [New], (+2 Overloads) [Return]
    '                   (+2 Overloads) Argument, Comments, DeclareFunc, (+2 Overloads) Field, FieldRef
    '                   GetValue, (+3 Overloads) LocalsInit, LocalVariable, (+2 Overloads) Reference, Type
    '                   TypeRef, Value, ValueAssign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.CodeDom
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Emit.CodeDOM_VBC

    Public Module CodeDOMExpressions

        ''' <summary>
        ''' Public Shared Function Main(Argvs As String()) As Integer
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EntryPoint As CodeDom.CodeMemberMethod
            Get
                Dim Func As New CodeMemberMethod With {
                    .Name = "Main",
                    .ReturnType = Type(Of Integer)(),
                    .Attributes = MemberAttributes.Public Or MemberAttributes.Static
                }

                Func.Parameters.Add(Argument(Of String())("Argvs"))

                Return Func
            End Get
        End Property

        ''' <summary>
        ''' ```
        ''' Public Shared Function xxx() As T
        ''' Public Shared Property XXX As T
        ''' ```
        ''' 
        ''' Or declare a method in a standard Module type.
        ''' </summary>
        Public Const PublicShared As MemberAttributes =
            CodeDom.MemberAttributes.Public Or
            CodeDom.MemberAttributes.Static

        ''' <summary>
        ''' 声明一个函数
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="args"></param>
        ''' <param name="returns"></param>
        ''' <param name="control"></param>
        ''' <returns></returns>
        Public Function DeclareFunc(name As String,
                                    args As Dictionary(Of String, Type),
                                    returns As Type,
                                    Optional control As CodeDom.MemberAttributes = PublicShared) As CodeDom.CodeMemberMethod

            Dim Func As New CodeDom.CodeMemberMethod With {
                .Name = name,
                .ReturnType = returns.TypeRef,
                .Attributes = control
            }

            If Not args.IsNullOrEmpty Then
                For Each x In args
                    Call Func.Parameters.Add(Argument(x.Key, x.Value))
                Next
            End If

            Return Func
        End Function

        ''' <summary>
        ''' ```
        ''' Dim Name As &lt;Type>
        ''' ```
        ''' 
        ''' Declare a field in the type
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        Public Function Field(Name As String, Type As Type) As CodeDom.CodeMemberField
            Return New CodeDom.CodeMemberField(name:=Name, type:=New CodeDom.CodeTypeReference(Type))
        End Function

        ''' <summary>
        ''' Reference of ``Me.Field``
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function FieldRef(Name As String) As CodeDom.CodeFieldReferenceExpression
            Return New CodeDom.CodeFieldReferenceExpression(New CodeDom.CodeThisReferenceExpression, Name)
        End Function

        Public Function Field(Name As String, type As String) As CodeDom.CodeFieldReferenceExpression
            Return New CodeDom.CodeFieldReferenceExpression(New CodeDom.CodeTypeReferenceExpression(type), Name)
        End Function

        Public Function Comments(text As String) As CodeDom.CodeCommentStatement
            Return New CodeDom.CodeCommentStatement(text)
        End Function

        ''' <summary>
        ''' Class object instance constructor
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        Public Function [New](Type As Type, parameters As CodeDom.CodeExpression()) As CodeDom.CodeObjectCreateExpression
            Return New CodeDom.CodeObjectCreateExpression(New CodeDom.CodeTypeReference(Type), parameters)
        End Function

        Public Function [New](type As String) As CodeDom.CodeObjectCreateExpression
            Dim typeRef As New CodeDom.CodeTypeReference(type)
            Return New CodeDom.CodeObjectCreateExpression(typeRef, {})
        End Function

        ''' <summary>
        ''' Class object instance constructor.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        Public Function [New](Of T As Class)(parameters As Object()) As CodeDom.CodeObjectCreateExpression
            If parameters.IsNullOrEmpty Then
                Return [New](GetType(T), {})
            Else
                Dim args As CodePrimitiveExpression() = LinqAPI.Exec(Of CodePrimitiveExpression) <=
                    From obj As Object
                    In parameters
                    Select New CodePrimitiveExpression(obj)

                Return [New](GetType(T), args)
            End If
        End Function

        ''' <summary>
        ''' New object
        ''' </summary>
        ''' <param name="typeRef"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        Public Function [New](typeRef As String, parameters As CodeDom.CodeExpression()) As CodeDom.CodeObjectCreateExpression
            Dim objectType As New CodeDom.CodeTypeReference(typeRef)
            If parameters Is Nothing Then
                parameters = New CodeDom.CodeExpression() {}
            End If

            Return New CodeDom.CodeObjectCreateExpression(objectType, parameters)
        End Function

        ''' <summary>
        ''' 声明一个局部变量
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Type"></param>
        ''' <param name="initExpression"></param>
        ''' <returns></returns>
        Public Function LocalsInit(Name As String, Type As System.Type, Optional initExpression As CodeDom.CodeExpression = Nothing) As CodeDom.CodeVariableDeclarationStatement
            Dim expr As New CodeDom.CodeVariableDeclarationStatement(New CodeDom.CodeTypeReference(Type), Name)
            If Not initExpression Is Nothing Then
                expr.InitExpression = initExpression
            End If
            Return expr
        End Function

        Public Function LocalsInit(Name As String, Type As String, Optional initExpression As CodeDom.CodeExpression = Nothing) As CodeDom.CodeVariableDeclarationStatement
            Dim typeRef As New CodeDom.CodeTypeReference(Type)
            Dim Expr = New CodeDom.CodeVariableDeclarationStatement(typeRef, Name)
            If Not initExpression Is Nothing Then
                Expr.InitExpression = initExpression
            End If
            Return Expr
        End Function

        ''' <summary>
        ''' Declare a local variable.
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Type"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        Public Function LocalsInit(Name As String, Type As System.Type, Optional init As Object = Nothing) As CodeDom.CodeVariableDeclarationStatement
            If Not init Is Nothing Then
                Return LocalsInit(Name, Type, New CodeDom.CodePrimitiveExpression(init))
            Else
                Return LocalsInit(Name, Type, initExpression:=Nothing)
            End If
        End Function

        ''' <summary>
        ''' Ctype
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function [CType](obj As CodeDom.CodeExpression, type As Type) As CodeDom.CodeCastExpression
            Return New CodeDom.CodeCastExpression(New CodeDom.CodeTypeReference(type), obj)
        End Function

        ''' <summary>
        ''' Call method
        ''' </summary>
        ''' <param name="Method"></param>
        ''' <param name="Parameters"></param>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <Extension>
        Public Function [Call](Method As MethodInfo,
                               Parameters As CodeDom.CodeExpression(),
                               Optional obj As CodeDom.CodeExpression = Nothing) As CodeDom.CodeMethodInvokeExpression
            Return [Call](If(obj Is Nothing, New CodeDom.CodeTypeReferenceExpression(Method.DeclaringType), obj), Method.Name, Parameters)
        End Function

        ''' <summary>
        ''' Call object by method name
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="Name"></param>
        ''' <param name="Parameters"></param>
        ''' <returns></returns>
        Public Function [Call](obj As CodeDom.CodeExpression, Name As String, Parameters As CodeDom.CodeExpression()) As CodeDom.CodeMethodInvokeExpression
            Dim MethodRef As New CodeDom.CodeMethodReferenceExpression(obj, Name)
            Dim Expression As New CodeDom.CodeMethodInvokeExpression(MethodRef, Parameters)
            Return Expression
        End Function

        Public Function [Call](type As Type, Name As String, Parameters As CodeDom.CodeExpression()) As CodeDom.CodeMethodInvokeExpression
            Return [Call](New CodeDom.CodeTypeReferenceExpression(type), Name, Parameters)
        End Function

        ''' <summary>
        ''' Call a statics function from a specific type with a known function name
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="Name"></param>
        ''' <param name="parametersValue"></param>
        ''' <returns></returns>
        Public Function [Call](type As Type, Name As String, parametersValue As Object()) As CodeDom.CodeMethodInvokeExpression
            If parametersValue.IsNullOrEmpty Then
                Return [Call](type, Name, Parameters:={})
            Else
                Return [Call](type, Name, (From obj In parametersValue Select New CodeDom.CodePrimitiveExpression(obj)).ToArray)
            End If
        End Function

        ''' <summary>
        ''' Function returns
        ''' </summary>
        ''' <param name="variable"></param>
        ''' <returns></returns>
        Public Function [Return](variable As String) As CodeDom.CodeMethodReturnStatement
            Return New CodeDom.CodeMethodReturnStatement(New CodeDom.CodeVariableReferenceExpression(variable))
        End Function

        ''' <summary>
        ''' Returns value in a function body
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        Public Function [Return](expression As CodeDom.CodeExpression) As CodeDom.CodeMethodReturnStatement
            Return New CodeDom.CodeMethodReturnStatement(expression)
        End Function

        ''' <summary>
        ''' Reference to a statics field in the specific target type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function Reference(obj As Type, Name As String) As CodeDom.CodeFieldReferenceExpression
            Return New CodeDom.CodeFieldReferenceExpression(New CodeDom.CodeTypeReferenceExpression(obj), Name)
        End Function

        ''' <summary>
        ''' Reference to a instance field in the specific object instance. 
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function Reference(obj As CodeDom.CodeExpression, Name As String) As CodeDom.CodeFieldReferenceExpression
            Return New CodeDom.CodeFieldReferenceExpression(obj, Name)
        End Function

        ''' <summary>
        ''' ``left = value``
        ''' </summary>
        ''' <param name="LeftAssigned"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function ValueAssign(LeftAssigned As CodeDom.CodeExpression, value As CodeDom.CodeExpression) As CodeDom.CodeAssignStatement
            Return New CodeDom.CodeAssignStatement(LeftAssigned, value)
        End Function

        ''' <summary>
        ''' Variable value initializer
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function Value(obj As Object) As CodeDom.CodePrimitiveExpression
            Return New CodeDom.CodePrimitiveExpression(obj)
        End Function

        ''' <summary>
        ''' Reference to a local variable in a function body.(引用局部变量)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function LocalVariable(Name As String) As CodeDom.CodeVariableReferenceExpression
            Return New CodeDom.CodeVariableReferenceExpression(Name)
        End Function

        ''' <summary>
        ''' Gets the element value in a array object.
        ''' </summary>
        ''' <param name="Array"></param>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Public Function GetValue(Array As CodeDom.CodeExpression, index As Integer) As CodeDom.CodeArrayIndexerExpression
            Dim idx = New CodeDom.CodePrimitiveExpression(index)
            Return New CodeDom.CodeArrayIndexerExpression(Array, idx)
        End Function

        Public Function Type(Of T)() As CodeDom.CodeTypeReference
            Dim refType As Type = GetType(T)
            Return New CodeDom.CodeTypeReference(refType)
        End Function

        <Extension> Public Function TypeRef(type As Type) As CodeDom.CodeTypeReference
            Return New CodeDom.CodeTypeReference(type)
        End Function

        ''' <summary>
        ''' System.Type.GetType(TypeName)
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        Public Function [GetType](Type As Type) As CodeDom.CodeMethodInvokeExpression
            Return [Call](GetType(System.Type), NameOf(System.Type.GetType), parametersValue:={Type.FullName, True, False})
        End Function

        Public Function Argument(Name As String, Type As Type) As CodeDom.CodeParameterDeclarationExpression
            Return New CodeDom.CodeParameterDeclarationExpression(New CodeDom.CodeTypeReference(Type), Name)
        End Function

        Public Function Argument(Of T)(Name As String) As CodeDom.CodeParameterDeclarationExpression
            Return Argument(Name, GetType(T))
        End Function

    End Module
End Namespace
