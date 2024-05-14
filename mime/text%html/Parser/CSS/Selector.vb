#Region "Microsoft.VisualBasic::96458672676fa96f6a85c04c012ccc2b, mime\text%html\Parser\CSS\Selector.vb"

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

    '   Total Lines: 63
    '    Code Lines: 41
    ' Comment Lines: 15
    '   Blank Lines: 7
    '     File Size: 2.17 KB


    '     Class Selector
    ' 
    '         Properties: CSSValue, Name, Selector, Type
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Text

Namespace Language.CSS

    ''' <summary>
    ''' CSS之中的样式选择器
    ''' </summary>
    Public Class Selector : Inherits [Property](Of String)
        Implements INamedValue

        ''' <summary>
        ''' 选择器的名称
        ''' </summary>
        ''' <returns></returns>
        Public Property Selector As String Implements IKeyedEntity(Of String).Key

        Public ReadOnly Property Type As CSSSelectorTypes
            Get
                If Selector.First = "."c Then
                    Return CSSSelectorTypes.class
                ElseIf Selector.First = "#" Then
                    Return CSSSelectorTypes.id
                ElseIf Selector.GetTagValue <> HtmlTags.NA Then
                    Return CSSSelectorTypes.tag
                Else
                    Return CSSSelectorTypes.expression
                End If
            End Get
        End Property

        ''' <summary>
        ''' CSS style value without selector name.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CSSValue As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Properties _
                    .Select(Function(x) $"{x.Key}: {x.Value};") _
                    .JoinBy(ASCII.LF)
            End Get
        End Property

        ''' <summary>
        ''' Get the selector text name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Selector.Trim("#"c, "."c)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Selector & " { " & CSSValue & " }"
        End Function
    End Class
End Namespace
