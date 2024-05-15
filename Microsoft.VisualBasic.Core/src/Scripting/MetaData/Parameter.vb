#Region "Microsoft.VisualBasic::55efe4d57b4beb5e985b2e10c0bf628c, Microsoft.VisualBasic.Core\src\Scripting\MetaData\Parameter.vb"

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

    '   Total Lines: 115
    '    Code Lines: 65
    ' Comment Lines: 34
    '   Blank Lines: 16
    '     File Size: 4.63 KB


    '     Class Parameter
    ' 
    '         Properties: [Alias], Description, ParameterInfo, TypeInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetAliasNameView, (+2 Overloads) GetParameterNameAlias, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Scripting.MetaData

    ''' <summary>
    ''' You Cann assign the parameter value using the parameter's alias name in the scripting using this attribute.
    ''' (你可以使用本属性将函数的参数名进行重命名，这样子你就可以使用本属性得到一个书写更加漂亮的编程脚本文件了)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class Parameter : Inherits Attribute
        Implements INamedValue

        ''' <summary>
        ''' 请使用这个方法<see cref="Parameter.GetParameterNameAlias"></see>来获取参数信息
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ParameterInfo As ParameterInfo

        Dim _alias As String

        ''' <summary>
        ''' The alias name of this function parameter in the scripting.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>(脚本函数的参数的别名)</remarks>
        Public Property [Alias] As String Implements INamedValue.Key
            Get
                If String.IsNullOrEmpty(_alias) AndAlso Not ParameterInfo Is Nothing Then
                    _alias = ParameterInfo.Name
                End If

                Return _alias
            End Get
            Protected Set(value As String)
                _alias = value
            End Set
        End Property

        ''' <summary>
        ''' The description information in the scripting help system.(在帮助信息里面进行显示的本参数的简要的描述信息)  
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Description As String

        ''' <summary>
        ''' You can using this attribute to customize your API interface.
        ''' </summary>
        ''' <param name="Alias">The alias name of this function parameter in the scripting.(当前脚本函数的这个参数的别名)</param>
        ''' <param name="MyDescription">The description information in the scripting help system.(这个信息会显示在脚本环境的帮助系统之中)</param>
        ''' <remarks></remarks>
        Sub New([Alias] As String, Optional MyDescription As String = "")
            _alias = [Alias]
            _Description = MyDescription
        End Sub

        Public Overrides Function ToString() As String
            Return _alias
        End Function

        ''' <summary>
        ''' 当没有定义属性的时候，会返回参数名
        ''' </summary>
        ''' <param name="pInfo"></param>
        ''' <returns></returns>
        Public Shared Function GetAliasNameView(pInfo As ParameterInfo) As String
            Dim [alias] = GetParameterNameAlias(pInfo, False)

            If [alias] Is Nothing Then
                Return pInfo.Name
            Else
                If String.IsNullOrEmpty([alias].Alias) Then
                    Return pInfo.Name
                Else
                    Return [alias].Alias
                End If
            End If
        End Function

        Public Shared Function GetParameterNameAlias(pInfo As ParameterInfo, AutoFill As Boolean) As Parameter
            Dim attrs As Object() = pInfo.GetCustomAttributes(GetType(Parameter), inherit:=True)
            If attrs.IsNullOrEmpty Then

                If Not AutoFill Then
                    Return Nothing
                End If

                Return New Parameter(pInfo.Name) With {._ParameterInfo = pInfo}
            Else
                Dim value = DirectCast(attrs.First, Parameter)
                value._ParameterInfo = pInfo
                Return value
            End If
        End Function

        Public Shared Function GetParameterNameAlias(pInfo As ParameterInfo, [Default] As Parameter) As Parameter
            Dim value = GetParameterNameAlias(pInfo, False)
            If value Is Nothing Then
                If [Default] Is Nothing Then [Default] = New Parameter(pInfo.Name)
                [Default]._ParameterInfo = pInfo
                Return [Default]
            Else
                Return value
            End If
        End Function

        Public Shared ReadOnly Property TypeInfo As Type = GetType(Parameter)
    End Class
End Namespace
