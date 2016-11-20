#Region "Microsoft.VisualBasic::4f4deb4100b7cd06d0da557eafe041de, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\System.Collections.Generic\MapsHelper.vb"

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

Imports Microsoft.VisualBasic.Language

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
End Namespace
