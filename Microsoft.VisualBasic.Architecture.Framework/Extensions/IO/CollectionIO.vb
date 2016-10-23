#Region "Microsoft.VisualBasic::6d41b646d2c444e2b24372748db2ff58, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\IO\CollectionIO.vb"

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

Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileIO

    ''' <summary>
    ''' Collection IO extensions
    ''' </summary>
    Public Module CollectionIO

        Public Delegate Function ISave(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
        Public Delegate Function IRead(type As Type, path As String, encoding As Encoding) As IEnumerable

        Public ReadOnly Property DefaultHandle As ISave = AddressOf SaveJSON
        Public ReadOnly Property DefaultLoadHandle As IRead = AddressOf ReadJSON

        Public ReadOnly Property DefaultSaveDescription As String
            Get
                Return DefaultHandle.Method.FullName(True)
            End Get
        End Property

        Public Sub SetHandle(handle As ISave)
            _DefaultHandle = handle
        End Sub

        Public Function ReadJSON(type As Type, path As String, encoding As Encoding) As IEnumerable
            Dim text As String = path.ReadAllText(encoding)
            type = type.MakeArrayType
            Return DirectCast(JsonContract.LoadObject(text, type), IEnumerable)
        End Function

        Public Function SaveJSON(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
            Return GetObjectJson(obj, obj.GetType).SaveTo(path, encoding)
        End Function

        Public Function SaveXml(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
            Return GetXml(obj, obj.GetType).SaveTo(path, encoding)
        End Function

        Public Function [TypeOf](Of T)() As [Class](Of T)
            Dim cls As New [Class](Of T)
            Return cls
        End Function
    End Module
End Namespace
