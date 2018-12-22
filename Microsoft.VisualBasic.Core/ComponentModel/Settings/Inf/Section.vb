#Region "Microsoft.VisualBasic::1e5551edd7581624cc72a643c231e37e, Microsoft.VisualBasic.Core\ComponentModel\Settings\Inf\Section.vb"

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

'     Class Section
' 
'         Properties: Items, Name
' 
'         Function: GetValue
' 
'         Sub: SetValue
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports HashValue = Microsoft.VisualBasic.Text.Xml.Models.NamedValue

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

        <XmlElement>
        Public Property Items As HashValue()
            Get
                Return _internalTable.Values.ToArray
            End Get
            Set(value As HashValue())
                If value Is Nothing Then
                    value = New HashValue() {}
                End If

                _internalTable = value.ToDictionary(Function(x) x.name.ToLower)
            End Set
        End Property

        ''' <summary>
        ''' 这个字典之中的所有键名称都是小写形式的
        ''' </summary>
        Dim _internalTable As Dictionary(Of HashValue)

        Public Function GetValue(Key As String) As String
            With Key.ToLower
                If _internalTable.ContainsKey(.ByRef) Then
                    Return _internalTable(.ByRef).text
                Else
                    Return ""
                End If
            End With
        End Function

        ''' <summary>
        ''' 不存在则自动添加
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="value"></param>
        Public Sub SetValue(Name As String, value As String)
            Dim KeyFind As String = Name.ToLower

            If _internalTable.ContainsKey(KeyFind) Then
                Call _internalTable.Remove(KeyFind)
            End If

            Call _internalTable.Add(KeyFind, New HashValue(Name, value))
        End Sub

        Public Function CreateDocFragment() As String
            Dim sb As New StringBuilder($"[{Name}]")

            For Each item As HashValue In _internalTable.Values
                Call sb.AppendLine($"{item.name}={item.text}")
            Next

            Return sb.ToString
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Name}] with {_internalTable.Keys.ToArray.GetJson()}"
        End Function
    End Class
End Namespace
