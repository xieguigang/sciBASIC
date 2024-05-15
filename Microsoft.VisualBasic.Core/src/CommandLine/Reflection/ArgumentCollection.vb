#Region "Microsoft.VisualBasic::a3a948fb061036548bb8c87a27764b1f, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\ArgumentCollection.vb"

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

    '   Total Lines: 156
    '    Code Lines: 108
    ' Comment Lines: 28
    '   Blank Lines: 20
    '     File Size: 6.19 KB


    '     Class ArgumentCollection
    ' 
    '         Properties: Count, EmptyExample, EmptyUsage, GetExample, GetUsage
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.Reflection

    ''' <summary>
    ''' The help information for a specific command line parameter switch.(某一个指定的命令的开关的帮助信息)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ArgumentCollection : Implements IEnumerable(Of NamedValue(Of ArgumentAttribute))

        ReadOnly _params As New Dictionary(Of String, ArgumentAttribute)

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
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
                Dim required = From par As ArgumentAttribute
                               In Me._params.Values
                               Where Not par.Optional
                               Select par
                Dim optionals = From par
                                In Me._params.Values
                                Where par.Optional
                                Select par
                Dim sb As New StringBuilder(1024)

                For Each Switch As ArgumentAttribute In required
                    Call sb.AppendFormat("{0} {1} ", Switch.Name, Switch.Example)
                Next
                For Each Switch As ArgumentAttribute In optionals
                    Call sb.AppendFormat("[{0} {1}] ", Switch.Name, Switch.Example)
                Next

                Return sb.ToString.Trim
            End Get
        End Property

        Public ReadOnly Property GetUsage() As String
            Get
                Dim requireds = From par
                                In Me._params.Values
                                Where Not par.Optional
                                Select par
                Dim optionals = From par
                                In Me._params.Values
                                Where par.Optional
                                Select par
                Dim sb As New StringBuilder(1024)

                For Each param As ArgumentAttribute In requireds
                    Call sb.AppendFormat("{0} {1} ", param.Name, param.Usage)
                Next
                For Each param As ArgumentAttribute In optionals
                    Call sb.AppendFormat("[{0} {1}] ", param.Name, param.Usage)
                Next

                Return sb.ToString.Trim
            End Get
        End Property

        Public ReadOnly Property EmptyUsage As Boolean
            Get
                Dim LQuery = From arg In _params.Values Where String.IsNullOrEmpty(arg.Usage) Select 1 '
                Return LQuery.Sum = _params.Count
            End Get
        End Property

        Public ReadOnly Property EmptyExample As Boolean
            Get
                Dim LQuery = From par As ArgumentAttribute
                             In _params.Values
                             Where String.IsNullOrEmpty(par.Example)
                             Select 1 '
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

            For Each parameter As ArgumentAttribute In _params.Values
                Call sb.AppendLine(parameter.ToString)
            Next
            Return Trim(sb.ToString)
        End Function

        ReadOnly __flag As Type = GetType(ArgumentAttribute)

        Sub New(methodInfo As MethodInfo)
            Dim attrs() = methodInfo.GetCustomAttributes(__flag, inherit:=False)
            Dim LQuery = LinqAPI.Exec(Of ArgumentAttribute) _
 _
                () <= From attr As Object
                      In attrs
                      Let parameter As ArgumentAttribute = TryCast(attr, ArgumentAttribute)
                      Select parameter
                      Order By parameter.Optional, parameter.TokenType Ascending ' 必须参数都在前面，可选参数都在后面

            For Each param As ArgumentAttribute In LQuery
                Call _params.Add(param.Name, param)
            Next
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of NamedValue(Of ArgumentAttribute)) Implements IEnumerable(Of NamedValue(Of ArgumentAttribute)).GetEnumerator
            For Each obj In _params
                Yield New NamedValue(Of ArgumentAttribute) With {
                    .Name = obj.Key,
                    .Value = obj.Value
                }
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
