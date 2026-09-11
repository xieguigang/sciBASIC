#Region "Microsoft.VisualBasic::20695c3534346675e1a08b264e80fe64, nlp\word2vec\Extensions.vb"

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

    '   Total Lines: 11
    '    Code Lines: 6 (54.55%)
    ' Comment Lines: 4 (36.36%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (9.09%)
    '     File Size: 332 B


    ' Module Extensions
    ' 
    '     Function: BuildWord2VecFactory
    ' 
    ' /********************************************************************************/

#End Region

<HideModuleName>
Public Module Extensions

    ''' <summary>
    ''' helper function for create <see cref="NLP.Word2Vec.Word2VecFactory"/> in script
    ''' </summary>
    ''' <returns></returns>
    Public Function BuildWord2VecFactory() As Word2VecFactory
        Return New Word2VecFactory
    End Function
End Module

