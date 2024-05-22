#Region "Microsoft.VisualBasic::ef3317fb5e61d1d32694c9f58aec294d, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\DataModels\BindMapping.vb"

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
    '    Code Lines: 68 (76.40%)
    ' Comment Lines: 9 (10.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (13.48%)
    '     File Size: 2.99 KB


    '     Class BindMapping
    ' 
    '         Properties: AsOutString, BindProperty, Description, Name, Type
    '                     Value
    ' 
    '         Function: Initialize, IsFsysValid
    ' 
    '         Sub: [Set]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace ComponentModel.Settings

    Public Class BindMapping : Inherits ProfileItem
        Implements IProfileTable

        Dim _target As Object

        Public ReadOnly Property BindProperty As PropertyInfo

        Public Shared Function Initialize(attr As ProfileItem, prop As PropertyInfo, obj As Object) As BindMapping
            Dim maps As New BindMapping With {
                .Name = attr.Name,
                .Description = attr.Description,
                .Type = attr.Type,
                ._BindProperty = prop,
                ._target = obj
            }
            Return maps
        End Function

        Public ReadOnly Property Value As String Implements IProfileTable.value
            Get
                Dim result As Object =
                    BindProperty.GetValue(_target, Nothing)
                Return Scripting.ToString(result)
            End Get
        End Property

        Public Sub [Set](value As String)
            Dim obj As Object =
                Scripting.CTypeDynamic(value, _BindProperty.PropertyType)
            Call _BindProperty.SetValue(_target, obj, Nothing)
        End Sub

        ''' <summary>
        ''' 打印在终端窗口上面的字符串
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property AsOutString As String
            Get
                Return String.Format("{0} = ""{1}""", Name, Value)
            End Get
        End Property

        Public Overrides Property Name As String Implements IProfileTable.name
            Get
                Return MyBase.Name
            End Get
            Set(value As String)
                MyBase.Name = value
            End Set
        End Property

        Public Overrides Property Type As ValueTypes Implements IProfileTable.type
            Get
                Return MyBase.Type
            End Get
            Set(value As ValueTypes)
                MyBase.Type = value
            End Set
        End Property

        Public Overrides Property Description As String Implements IProfileTable.edges
            Get
                Return MyBase.Description
            End Get
            Set(value As String)
                MyBase.Description = value
            End Set
        End Property

        ''' <summary>
        ''' 这个方法只是针对<see cref="ValueTypes.File"/>和<see cref="ValueTypes.Directory"/>这两种类型而言才有效的
        ''' 对于其他的类型数据，都是返回False
        ''' </summary>
        ''' <returns></returns>
        Public Function IsFsysValid() As Boolean
            If Type = ValueTypes.Directory Then
                Return Value.DirectoryExists
            ElseIf Type = ValueTypes.File Then
                Return Value.FileExists
            Else
                Return False
            End If
        End Function
    End Class
End Namespace
