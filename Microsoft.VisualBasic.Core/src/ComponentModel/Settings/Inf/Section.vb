#Region "Microsoft.VisualBasic::21e0ecc5295458f0b1869c321c3475b4, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\Inf\Section.vb"

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

    '   Total Lines: 136
    '    Code Lines: 85
    ' Comment Lines: 30
    '   Blank Lines: 21
    '     File Size: 4.49 KB


    '     Class Section
    ' 
    '         Properties: Comment, Items, Name
    ' 
    '         Function: CreateDocFragment, GetValue, Have, ToString
    ' 
    '         Sub: appendComments, Delete, SetComments, SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports HashValue = Microsoft.VisualBasic.Text.Xml.Models.Property

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' 一个配置数据区域的抽象模型
    ''' </summary>
    Public Class Section

        ''' <summary>
        ''' 区域的名称
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property Name As String

        <XmlText>
        Public Property Comment As String

        Shared ReadOnly emptyList As New [Default](Of HashValue())(Function() {}, isLazy:=False)

        <XmlElement>
        Public Property Items As HashValue()
            Get
                Return configTable.Values.ToArray
            End Get
            Set(value As HashValue())
                configTable = (value Or emptyList).ToDictionary(Function(x) x.name.ToLower)
            End Set
        End Property

        ''' <summary>
        ''' 这个字典之中的所有键名称都是小写形式的
        ''' </summary>
        Dim configTable As Dictionary(Of HashValue)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Have(key As String) As Boolean
            Return configTable.ContainsKey(key.ToLower)
        End Function

        Public Function GetValue(key As String, Optional default$ = "") As String
            With key.ToLower
                If .DoCall(AddressOf configTable.ContainsKey) Then
                    Return configTable(.ByRef).value
                Else
                    Return [default]
                End If
            End With
        End Function

        Public Sub Delete(name As String)
            With name.ToLower
                If .DoCall(AddressOf configTable.ContainsKey) Then
                    Call configTable.Remove(.ByRef)
                End If
            End With
        End Sub

        ''' <summary>
        ''' 不存在则自动添加
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="value"></param>
        Public Sub SetValue(Name$, value$, Optional comment$ = Nothing)
            Dim KeyFind As String = Name.ToLower

            If configTable.ContainsKey(KeyFind) Then
                Call configTable.Remove(KeyFind)
            End If

            configTable(KeyFind) = New HashValue With {
                .name = Name,
                .comment = comment,
                .value = value
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetComments(name$, comments$)
            Call SetValue(name, GetValue(name), comments)
        End Sub

        ''' <summary>
        ''' 利用这个函数所生成的文档片段的格式如下所示
        ''' 
        ''' ```
        ''' [name]
        ''' # comment region of 
        ''' # this section
        '''
        ''' ; comment of this key
        ''' key=value
        ''' ; comment of this key
        ''' key=value
        ''' ```
        ''' </summary>
        ''' <returns></returns>
        Public Function CreateDocFragment() As String
            Dim sb As New StringBuilder($"[{Name}]")

            Call sb.AppendLine()
            Call appendComments(sb, Comment, "#")

            If Not Comment.StringEmpty Then
                Call sb.AppendLine()
            End If

            For Each item As HashValue In configTable.Values
                Call appendComments(sb, item.comment, ";")
                Call sb.AppendLine($"{item.name}={item.value}")
            Next

            Return sb.ToString
        End Function

        Private Shared Sub appendComments(sb As StringBuilder, comments$, symbol$)
            If Not comments.StringEmpty Then
                For Each line As String In comments.LineTokens
                    Call sb.AppendLine(symbol & " " & line)
                Next
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{Name}] with {configTable.Keys.ToArray.GetJson()}"
        End Function
    End Class
End Namespace
