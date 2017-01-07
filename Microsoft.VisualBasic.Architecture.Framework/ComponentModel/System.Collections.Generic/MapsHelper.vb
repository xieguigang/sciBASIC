#Region "Microsoft.VisualBasic::734241644b73c5962077099ba5ecdb0d, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\System.Collections.Generic\MapsHelper.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel

    Public Structure MapsHelper(Of T)
        ReadOnly __default As T
        ReadOnly __maps As IReadOnlyDictionary(Of String, T)

        Sub New(map As IReadOnlyDictionary(Of String, T), Optional [default] As T = Nothing)
            __default = [default]
            __maps = map
        End Sub

        Public Function GetValue(key As String) As T
            If __maps.ContainsKey(key) Then
                Return __maps(key)
            Else
                Return __default
            End If
        End Function
    End Structure

    Public Structure Map(Of T1, V)
        Dim key As T1
        Dim Maps As V

        ''' <summary>
        ''' 与<see cref="IKeyValuePairObject(Of TKey, TValue)"/>相比，这个类型更加倾向于特定化的描述两个对象之间的一一对应关系
        ''' </summary>
        Public Interface IMap
            Property Key As T1
            Property Maps As V
        End Interface
    End Structure
End Namespace
