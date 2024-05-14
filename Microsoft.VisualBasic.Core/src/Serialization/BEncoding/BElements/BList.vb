#Region "Microsoft.VisualBasic::f298ad8c28fb982a2a3ff79012ee7490, Microsoft.VisualBasic.Core\src\Serialization\BEncoding\BElements\BList.vb"

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


    ' Code Statistics:

    '   Total Lines: 55
    '    Code Lines: 26
    ' Comment Lines: 20
    '   Blank Lines: 9
    '     File Size: 1.82 KB


    '     Class BList
    ' 
    '         Function: (+2 Overloads) ToBencodedString
    ' 
    '         Sub: (+2 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode list.
    ''' </summary>
    Public Class BList : Inherits List(Of BElement)
        Implements BElement

        ''' <summary>
        ''' Generates the bencoded equivalent of the list.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the list.</returns>
        Public Function ToBencodedString() As String Implements BElement.ToBencodedString
            Return ToBencodedString(New StringBuilder()).ToString()
        End Function

        ''' <summary>
        ''' Generates the bencoded equivalent of the list.
        ''' </summary>
        ''' <param name="u">The StringBuilder to append to.</param>
        ''' <returns>The bencoded equivalent of the list.</returns>
        Public Function ToBencodedString(u As StringBuilder) As StringBuilder Implements BElement.ToBencodedString
            If u Is Nothing Then
                u = New StringBuilder("l"c)
            Else
                u.Append("l"c)
            End If

            For Each element In ToArray()
                element.ToBencodedString(u)
            Next

            Return u.Append("e"c)
        End Function

        ''' <summary>
        ''' Adds the specified value to the list.
        ''' </summary>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(value As String)
            MyBase.Add(New BString(value))
        End Sub

        ''' <summary>
        ''' Adds the specified value to the list.
        ''' </summary>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(value As Integer)
            MyBase.Add(New BInteger(value))
        End Sub
    End Class

End Namespace
