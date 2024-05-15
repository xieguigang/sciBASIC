#Region "Microsoft.VisualBasic::c1f1ebf06eea663a911d8f27a4726092, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Attributes\Namespace.vb"

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

    '   Total Lines: 89
    '    Code Lines: 42
    ' Comment Lines: 35
    '   Blank Lines: 12
    '     File Size: 3.32 KB


    '     Class [Namespace]
    ' 
    '         Properties: [Namespace], AutoExtract, Description, TypeInfo
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: CreateInstance, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace CommandLine.Reflection

    ''' <summary>
    ''' (<see cref="Interpreter">CommandLine interpreter</see> executation Entry and the ShellScript software packages namespace.)这是一个命令行解释器所使用的执行入口点的集合
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class [Namespace] : Inherits Attribute
        Implements INamedValue

        ''' <summary>
        ''' A brief description text about the function of this namespace.(关于本模块之中的描述性的摘要文本)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description As String

        ''' <summary>
        ''' The name value of this namespace module.(本命名空间模块的名称值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlAttribute>
        Public Property [Namespace] As String Implements INamedValue.Key

        Dim _TypeAutoExtract As Boolean

        ''' <summary>
        ''' Readonly
        ''' </summary>
        ''' <returns></returns>
        <XmlIgnore> Public Property AutoExtract As Boolean
            Get
                Return _TypeAutoExtract
            End Get
            Protected Set(value As Boolean)
                _TypeAutoExtract = value
            End Set
        End Property

        ''' <summary>
        ''' The name value of this namespace module.(本命名空间模块的名称值)
        ''' </summary>
        ''' <param name="Namespace">The name value of this namespace module.(本命名空间模块的名称值)</param>
        ''' <remarks></remarks>
        Sub New([Namespace] As String, Optional Description As String = "")
            Me.Namespace = [Namespace]
            Me.Description = Description
            Me.AutoExtract = False
        End Sub

        Protected Sub New()
        End Sub

        Friend Sub New([Namespace] As String, Description As String, auto As Boolean)
            Call Me.New([Namespace], Description)
            Me.AutoExtract = auto
        End Sub

        ''' <summary>
        ''' Constant of type information for the reflection
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TypeInfo As Type = GetType([Namespace])

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(Description) Then
                Return String.Format("Namespace {0}", [Namespace])
            Else
                Return String.Format("Namespace {0} ({1})", [Namespace], Description)
            End If
        End Function

        ''' <summary>
        ''' 从目标类型之中构造出一个命令行解释器
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateInstance(Type As Type) As Interpreter
            Return New Interpreter(Type)
        End Function
    End Class
End Namespace
