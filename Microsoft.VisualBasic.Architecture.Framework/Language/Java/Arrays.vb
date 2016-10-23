#Region "Microsoft.VisualBasic::dfe93dc2a7363b6fae6135b7f5f97d76, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Language\Java\Arrays.vb"

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

Imports System.Runtime.CompilerServices

Namespace Language.Java

    Public Module Arrays

        <Extension>
        Public Sub Fill(Of T)(ByRef a As T(), val As T)
            For i% = 0 To a.Length - 1
                a(i%) = val
            Next
        End Sub

        Public Function copyOfRange(Of T)(matrix As T(), start As Integer, length As Integer) As T()
            Dim out As T() = New T(length - 1) {}
            Call Array.Copy(matrix, start, out, Scan0, length)
            Return out
        End Function
    End Module
End Namespace
