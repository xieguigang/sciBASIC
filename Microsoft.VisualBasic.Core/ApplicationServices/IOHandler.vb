#Region "Microsoft.VisualBasic::1811564255de9ee59499da9b7739ed07, Microsoft.VisualBasic.Core\ApplicationServices\IOHandler.vb"

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

    '     Module IOHandler
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Properties: DefaultHandle, DefaultLoadHandle, DefaultSaveDescription
    ' 
    '             Function: ReadJSON, SaveJSON, SaveXml
    ' 
    '             Sub: SetHandle
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices

    ''' <summary>
    ''' Collection IO extensions
    ''' </summary>
    Public Module IOHandler

        Public Delegate Function ISave(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
        Public Delegate Function IRead(type As Type, path As String, encoding As Encoding) As IEnumerable

        Public ReadOnly Property DefaultHandle As ISave = AddressOf SaveJSON
        Public ReadOnly Property DefaultLoadHandle As IRead = AddressOf ReadJSON

        Public ReadOnly Property DefaultSaveDescription As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return DefaultHandle _
                    .Method _
                    .DeclaringType _
                    .Module _
                    .Assembly _
                    .Location _
                    .FileName & "!" & DefaultHandle.Method.FullName(False)
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
    End Module
End Namespace
