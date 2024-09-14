#Region "Microsoft.VisualBasic::c7d4f03ca9c8178b4d9fe011561c01e2, Data_science\DataMining\DataMining\AprioriRules\Helper\Item.vb"

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

    '   Total Lines: 61
    '    Code Lines: 30 (49.18%)
    ' Comment Lines: 21 (34.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (16.39%)
    '     File Size: 1.94 KB


    '     Structure Item
    ' 
    '         Properties: UnicodeChar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo, GetHashCode, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace AprioriRules

    ''' <summary>
    ''' mapping the <see cref="Item"/> string comparision to <see cref="Code"/> comparision
    ''' </summary>
    ''' <remarks>
    ''' constant value liked, so readonly field at here
    ''' </remarks>
    Public Structure Item : Implements IComparable(Of Item)

        ''' <summary>
        ''' the hashcode of the <see cref="Item"/> string
        ''' </summary>
        ReadOnly Code As Integer
        ReadOnly Item As String

        ''' <summary>
        ''' Convert the <see cref="Code"/> as chinese character BMP unicode char.
        ''' </summary>
        ''' <returns>
        ''' one of the 20992 chinese BMP chars.
        ''' </returns>
        Public ReadOnly Property UnicodeChar As Char
            Get
                Return Convert.ToChar(Code + 19968)
            End Get
        End Property

        Sub New(hashcode As Integer, item As String)
            Me.Code = hashcode
            Me.Item = item
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return Code
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Code}]{Item}"
        End Function

        Public Function CompareTo(other As Item) As Integer Implements IComparable(Of Item).CompareTo
            Return Code.CompareTo(other.Code)
        End Function

        ''' <summary>
        ''' check equals of the <see cref="Code"/>
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(a As Item, b As Item) As Boolean
            Return a.Code = b.Code
        End Operator

        Public Overloads Shared Operator <>(a As Item, b As Item) As Boolean
            Return a.Code <> b.Code
        End Operator

    End Structure
End Namespace
