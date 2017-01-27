#Region "Microsoft.VisualBasic::2307bbc7c8a7e5e49a7a7f8a1b3c5d2a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\ValuePair\RefValue.vb"

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

Namespace ComponentModel

    Public Structure Binding(Of T, K)
        Public Bind As T
        Public Target As K

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Bind Is Nothing AndAlso Target Is Nothing
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Bind.ToString & " --> " & Target.ToString
        End Function
    End Structure
End Namespace
