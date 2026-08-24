#Region "Microsoft.VisualBasic::ebeca4ecbed01539fabf355e7094c02c, Microsoft.VisualBasic.Core\src\Data\Trinity\NLP\Token.vb"

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

    '   Total Lines: 14
    '    Code Lines: 12 (85.71%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (14.29%)
    '     File Size: 353 B


    '     Interface ITokenCount
    ' 
    '         Properties: Count, Id, Token
    ' 
    '     Interface ILink
    ' 
    '         Properties: Count, source, target
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Data.Trinity.NLP

    Public Interface ITokenCount
        Property Token As String
        Property Id As Integer
        Property Count As Integer
    End Interface

    Public Interface ILink
        Property source As Integer
        Property target As Integer
        Property Count As Integer
    End Interface
End Namespace
