#Region "Microsoft.VisualBasic::7ab119a239a20823e2837f4b7134f4ff, Microsoft.VisualBasic.Core\Language\Language\JavaScript\UnionType.vb"

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

    '     Class UnionType
    ' 
    '         Function: [GetType], ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language.JavaScript

    Public Class UnionType(Of T)

        Public value As T
        Public lambda As Func(Of T)

        Public Overloads Function [GetType]() As Type
            If lambda Is Nothing Then
                Return value.GetType()
            Else
                Return GetType(Func(Of T))
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetType().ToString
        End Function
    End Class
End Namespace
