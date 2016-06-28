#Region "392e64d0df4c87d335d78b1ac9501553, ..\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Settings\Inf\Section.vb"

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

Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Xml.Serialization

Namespace ComponentModel.Settings.Inf

    Public Class Section

        <XmlAttribute> Public Property Name As String
        <XmlElement> Public Property Items As HashValue()
            Get
                Return _innerHash.Values.ToArray
            End Get
            Set(value As HashValue())
                If value Is Nothing Then
                    value = New HashValue() {}
                End If

                _innerHash = New Dictionary(Of HashValue)(value.ToDictionary(Function(x) x.Identifier.ToLower))
            End Set
        End Property

        Dim _innerHash As Dictionary(Of HashValue)

        Public Function GetValue(Key As String) As String
            Key = Key.ToLower

            If _innerHash.ContainsKey(Key) Then
                Return _innerHash(Key).Value
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' 不存在则自动添加
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="value"></param>
        Public Sub SetValue(Name As String, value As String)
            Dim KeyFind As String = Name.ToLower

            If _innerHash.ContainsKey(KeyFind) Then
                Call _innerHash.Remove(KeyFind)
            End If

            Call _innerHash.Add(KeyFind, New HashValue(Name, value))
        End Sub
    End Class
End Namespace
