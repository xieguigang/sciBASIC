#Region "Microsoft.VisualBasic::8e220ce66ccd73ecb967442fcabdca8d, Microsoft.VisualBasic.Core\Language\Language\Python\Collection.vb"

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

    '     Module Collection
    ' 
    '         Function: slice
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.Python

    Public Module Collection

        <Extension>
        Public Iterator Function slice(Of T)([set] As IEnumerable(Of T), start As Integer, [stop] As Integer, Optional [step] As Integer = 1) As IEnumerable(Of T)
            Dim array As T() = [set].Skip(start).ToArray

            If [stop] < 0 Then
                [stop] = array.Length + [stop]
            End If

            [stop] -= 1

            For i As Integer = 0 To [stop] Step [step]
                Yield array(i)
            Next
        End Function
    End Module
End Namespace
