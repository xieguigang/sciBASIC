#Region "463d00435e7fb6d8189aa41ae89a264d, ..\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\ParameterInfo.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace CommandLine.Reflection

    ''' <summary>
    ''' The help information for a specific command line parameter switch.(某一个指定的命令的开关的帮助信息)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ParameterInfoCollection
        Implements IEnumerable(Of NamedValue(Of ParameterInfo))

        ReadOnly _params As New Dictionary(Of String, ParameterInfo)

        ''' <summary>
        ''' 本命令行对象中的包含有帮助信息的开关参数的数目
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer
            Get
                Return _params.Count
            End Get
        End Property

        ''' <summary>
        ''' Returns the parameter switch help information with the specific name value.(显示某一个指定名称的开关信息)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public ReadOnly Property Parameter(Name As String) As String
            Get
                Return _params(Name).ToString
            End Get
        End Property

        ''' <summary>
        ''' Gets the usage example of this parameter switch.(获取本参数开关的帮助信息)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property GetExample() As String
            Get
                Dim RequiredSwitchs = (From switch In Me._params.Values Where switch.Optional = False Select switch).ToArray
                Dim OptionalSwitchs = (From switch In Me._params.Values Where switch.Optional Select switch).ToArray
                Dim sBuilder As StringBuilder = New StringBuilder(1024)
                For Each Switch As ParameterInfo In RequiredSwitchs
                    Call sBuilder.AppendFormat("{0} {1} ", Switch.Name, Switch.Example)
                Next
                For Each Switch As ParameterInfo In OptionalSwitchs
                    Call sBuilder.AppendFormat("[{0} {1}] ", Switch.Name, Switch.Example)
                Next

                Return sBuilder.ToString.Trim
            End Get
        End Property

        Public ReadOnly Property GetUsage() As String
            Get
                Dim requiredParameters = (From parameter In Me._params.Values Where parameter.Optional = False Select parameter).ToArray
                Dim optionalParameters = (From parameter In Me._params.Values Where parameter.Optional Select parameter).ToArray
                Dim sb As New StringBuilder(1024)

                For Each param As ParameterInfo In requiredParameters
                    Call sb.AppendFormat("{0} {1} ", param.Name, param.Usage)
                Next
                For Each param As ParameterInfo In optionalParameters
                    Call sb.AppendFormat("[{0} {1}] ", param.Name, param.Usage)
                Next

                Return sb.ToString.Trim
            End Get
        End Property

        Public ReadOnly Property EmptyUsage As Boolean
            Get
                Dim LQuery = From switch In _params.Values Where String.IsNullOrEmpty(switch.Usage) Select 1 '
                Return LQuery.Sum = _params.Count
            End Get
        End Property

        Public ReadOnly Property EmptyExample As Boolean
            Get
                Dim LQuery = From switch In _params.Values Where String.IsNullOrEmpty(switch.Example) Select 1 '
                Return LQuery.Sum = _params.Count
            End Get
        End Property

        ''' <summary>
        ''' 显示所有的开关信息
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder(1024)

            For Each parameter As ParameterInfo In _params.Values
                Call sb.AppendLine(parameter.ToString)
            Next
            Return sb.ToString
        End Function

        ReadOnly __flag As Type = GetType(ParameterInfo)

        Sub New(methodInfo As MethodInfo)
            Dim attrs As Object() = methodInfo.GetCustomAttributes(__flag, inherit:=False)
            Dim LQuery As IEnumerable(Of ParameterInfo) =
                From attr As Object
                In attrs
                Let parameter As ParameterInfo =
                    TryCast(attr, ParameterInfo)
                Select parameter
                Order By parameter.Optional Ascending '

            For Each param As ParameterInfo In LQuery
                Call _params.Add(param.Name, param)
            Next
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of NamedValue(Of ParameterInfo)) Implements IEnumerable(Of NamedValue(Of ParameterInfo)).GetEnumerator
            For Each obj In _params
                Yield New NamedValue(Of ParameterInfo) With {
                    .Name = obj.Key,
                    .x = obj.Value
                }
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
