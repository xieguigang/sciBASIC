#Region "Microsoft.VisualBasic::758ae727edadca386f3e86946574a54c, gr\network-visualization\network_layout\Orthogonal\optimization\EmbeddingComparator.vb"

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

    '   Total Lines: 17
    '    Code Lines: 5 (29.41%)
    ' Comment Lines: 9 (52.94%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 3 (17.65%)
    '     File Size: 487 B


    '     Interface EmbeddingComparator
    ' 
    '         Function: compare
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

Namespace Orthogonal.optimization

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Interface EmbeddingComparator
        Function compare(oer1 As OrthographicEmbeddingResult, oer2 As OrthographicEmbeddingResult) As Integer
    End Interface

End Namespace
