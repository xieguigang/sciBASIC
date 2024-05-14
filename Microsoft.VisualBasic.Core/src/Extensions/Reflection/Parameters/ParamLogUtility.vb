#Region "Microsoft.VisualBasic::e65ac0e10a02ece50ebe67cb3f1cabe6, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Parameters\ParamLogUtility.vb"

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

    '   Total Lines: 123
    '    Code Lines: 84
    ' Comment Lines: 23
    '   Blank Lines: 16
    '     File Size: 5.70 KB


    '     Module ParamLogUtility
    ' 
    '         Function: (+3 Overloads) Acquire, GetMyCaller, InitTable
    ' 
    '         Sub: AddProvidedParamaterDetail
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace Emit.Parameters

    ''' <summary>
    ''' Exception is a common issue in projects. To track this exception, we use error loggers 
    ''' which only log the exception detail and some other information if you want to. 
    ''' But hardly do we get any idea for which input set(parameters and its values) a 
    ''' particular method is throwing the error.
    ''' </summary>
    ''' <remarks>
    ''' https://www.codeproject.com/tips/795865/log-all-parameters-that-were-passed-to-some-method
    ''' </remarks>
    Public Module ParamLogUtility

        Public Function GetMyCaller() As MethodBase
            ' [2] a()
            ' [1]  ---> b()  ' my caller is a()
            ' [0]  ---> GetMyCaller  
            Return New StackTrace().GetFrame(2).GetMethod()
        End Function

        Public Function Acquire(ParamArray providedParameters As Expression(Of Func(Of Object))()) As Dictionary(Of Value)
            Dim currentMethod As MethodBase = New StackTrace().GetFrame(1).GetMethod()
            Return currentMethod.Acquire(providedParameters)
        End Function

        Public Function Acquire(Of T)(ParamArray providedParameters As Expression(Of Func(Of Object))()) As Dictionary(Of Value)
            Dim type As Type = GetType(T)
            Dim out As Dictionary(Of Value) = ParamLogUtility.Acquire(providedParameters)
            out -= out.Keys.Where(Function(k) Not out(k).Type.Equals(type))
            Return out
        End Function

        <Extension>
        Public Function Acquire(currentMethod As MethodBase, ParamArray providedParameters As Expression(Of Func(Of Object))()) As Dictionary(Of Value)
            Dim out As New Dictionary(Of Value)
            ' Set class and current method info
            Dim trace As New NamedValue(Of MethodBase) With {
                .Name = currentMethod.Name,
                .Value = currentMethod
            }

            ' Get current methods paramaters
            'Dim _methodParamaters As New Dictionary(Of String, Type)()
            'Call (From aParamater As ParameterInfo
            '      In currentMethod.GetParameters
            '      Select New With {
            '          .Name = aParamater.Name,
            '          .DataType = aParamater.ParameterType
            '}).AsList() _
            '  .ForEach(Sub(obj) _methodParamaters.Add(obj.Name, obj.DataType))

            ' Get provided methods paramaters
            For Each aExpression In providedParameters
                Dim bodyType As Expression = aExpression.Body

                If TypeOf bodyType Is MemberExpression Then
                    Call out.AddProvidedParamaterDetail(DirectCast(aExpression.Body, MemberExpression), trace)
                ElseIf TypeOf bodyType Is UnaryExpression Then
                    Dim unaryExpression As UnaryExpression = DirectCast(aExpression.Body, UnaryExpression)
                    Call out.AddProvidedParamaterDetail(DirectCast(unaryExpression.Operand, MemberExpression), trace)
                Else
                    Throw New Exception("Expression type unknown.")
                End If
            Next

            Return out
        End Function

        <Extension>
        Private Sub AddProvidedParamaterDetail(out As Dictionary(Of Value), memberExpression As MemberExpression, trace As NamedValue(Of MethodBase))
            Dim constantExpression As ConstantExpression = DirectCast(memberExpression.Expression, ConstantExpression)
            Dim name As String = memberExpression.Member.Name
            Dim value = DirectCast(memberExpression.Member, FieldInfo).GetValue(constantExpression.Value)
            Dim type As Type = value.[GetType]()

            name = name.Replace("$VB$Local_", "")
            out += New Value With {
                .Name = name,
                .Type = type,
                .Value = value,
                .Trace = trace
            }
        End Sub

        <Extension>
        Public Function InitTable(caller As MethodBase, array As Expression(Of Func(Of Object()))) As Dictionary(Of Value)
            Dim unaryExpression As NewArrayExpression = DirectCast(array.Body, NewArrayExpression)
            Dim arrayData As UnaryExpression() = unaryExpression _
                .Expressions _
                .Select(Function(e) DirectCast(e, UnaryExpression)) _
                .ToArray
            Dim out As New Dictionary(Of Value)
            Dim trace As New NamedValue(Of MethodBase) With {
                .Name = caller.Name,
                .Value = caller
            }

            For Each expr As UnaryExpression In arrayData
                Dim member = DirectCast(expr.Operand, MemberExpression)
                Dim constantExpression As ConstantExpression = DirectCast(member.Expression, ConstantExpression)
                Dim name As String = member.Member.Name.Replace("$VB$Local_", "")
                Dim field As FieldInfo = DirectCast(member.Member, FieldInfo)
                Dim value As Object = field.GetValue(constantExpression.Value)

                out += New Value With {
                    .Name = name,
                    .Type = value.GetType,
                    .Value = value,
                    .Trace = trace
                }
            Next

            Return out
        End Function
    End Module
End Namespace
