#Region "Microsoft.VisualBasic::7eff88cc715de96aee4fa9ea39b073da, Microsoft.VisualBasic.Core\src\Language\Linq\Vectorization\IVectorReader.vb"

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

    '   Total Lines: 9
    '    Code Lines: 6
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 196 B


    '     Interface IVectorReader
    ' 
    '         Properties: length, vector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language.Vectorization

    Public Interface IVectorReader

        ReadOnly Property vector As Array
        ReadOnly Property length As Integer

    End Interface
End Namespace
