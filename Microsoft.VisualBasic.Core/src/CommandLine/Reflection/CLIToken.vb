#Region "Microsoft.VisualBasic::dc7e39b244447c07223488c5089fa330, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\CLIToken.vb"

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

    '   Total Lines: 96
    '    Code Lines: 46 (47.92%)
    ' Comment Lines: 34 (35.42%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (16.67%)
    '     File Size: 3.37 KB


    '     Class CLIToken
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Argv
    ' 
    '         Properties: Format, IsOptional, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Prefix
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.Reflection

    ''' <summary>
    ''' A very basically type in the <see cref="CommandLine"/>
    ''' </summary>
    Public MustInherit Class CLIToken : Inherits Attribute
        Implements IReadOnlyId

        ''' <summary>
        ''' Name of this token object, this can be parameter name or api name.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Name As String Implements IReadOnlyId.Identity

        ''' <summary>
        ''' Init this token by using <see cref="name"/> value.
        ''' </summary>
        ''' <param name="name">Token name</param>
        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    ''' <summary>
    ''' 可以将这个自定义属性添加到类型的属性上面，添加额外的命名以及类型之类的标记
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class Argv : Inherits CLIToken

        ''' <summary>
        ''' 对于<see cref="CLITypes.String"/>和<see cref="CLITypes.File"/>
        ''' 程序会有不同的处理操作，虽然二者的值都是字符串输入
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As CLITypes
        ''' <summary>
        ''' Optional commandline arguments.(本属性标记一个命令行字符串之中的可选参数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsOptional As Boolean

        Public Property Format As String

        ''' <summary>
        ''' 默认为参数字符串通用类型
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="type"></param>
        ''' <param name="optional">
        ''' Optional commandline arguments.(本属性标记一个命令行字符串之中的可选参数)
        ''' </param>
        Sub New(name$,
                Optional type As CLITypes = CLITypes.String,
                Optional [optional] As Boolean = False)

            Call MyBase.New(name)

            Me.Type = type
            Me.IsOptional = [optional]
        End Sub

        Public Overrides Function ToString() As String
            With $"Dim [{Name}] As {Type.ToString}"
                If IsOptional Then
                    Return $"[{ .ByRef}]"
                Else
                    Return .ByRef
                End If
            End With
        End Function
    End Class

    ''' <summary>
    ''' 这个自定义属性添加在Class申明上表示该class类的命令行参数的名称都会添加这个prefix
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class Prefix : Inherits Attribute

        Public ReadOnly Property Value As String

        Sub New(prefix As String)
            Value = prefix
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function
    End Class
End Namespace
