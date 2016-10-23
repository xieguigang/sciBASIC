#Region "Microsoft.VisualBasic::e24f8b8ba72fdcd857b4ffba7d9374dd, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\Attributes\Attribute.vb"

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

Imports System.Reflection
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace CommandLine.Reflection

    ''' <summary>
    ''' Optional commandline arguments.(本属性标记一个命令行字符串之中的可选参数)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class [Optional] : Inherits CLIToken

        Dim _type As CLITypes

        Public Overrides ReadOnly Property Name As String
            Get
                Return MyBase.Name
            End Get
        End Property

        Public Property Type As CLITypes
            Get
                Return _type
            End Get
            Protected Set(value As CLITypes)
                _type = value
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name">The name value of the target parameter switch which will be marked as an optional parameter.
        ''' (目标将要被标记为可选参数的命令行参数开关对象)</param>
        ''' <param name="Type">The data type of the target command line parameter switch, default type is string type.</param>
        ''' <remarks></remarks>
        Public Sub New(Name As String, Optional Type As CLITypes = CLITypes.String)
            Call MyBase.New(Name)
            Me.Type = Type
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("({0}) {1}", Type.ToString, Name)
        End Function
    End Class
End Namespace
