#Region "Microsoft.VisualBasic::94b7d2f657850e09829efe2af1c54eab, Microsoft.VisualBasic.Core\ComponentModel\ValuePair\TagData\LineValue.vb"

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

    '     Structure LineValue
    ' 
    '         Properties: Line, value
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.TagData

    Public Structure LineValue(Of T)
        Implements IAddress(Of Integer)
        Implements Value(Of T).IValueOf

        Public Property Line As Integer Implements IAddress(Of Integer).Address
        Public Property value As T Implements Value(Of T).IValueOf.value

    End Structure
End Namespace
