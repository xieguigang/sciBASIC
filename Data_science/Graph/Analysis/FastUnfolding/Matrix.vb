#Region "Microsoft.VisualBasic::4dc22c15b021af6d429d95295b6f146e, Data_science\Graph\Analysis\FastUnfolding\Matrix.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14 (73.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (26.32%)
    '     File Size: 532 B


    '     Class Matrix
    ' 
    '         Properties: Q, tag_dict, tag_dict2
    ' 
    '         Function: ToString
    ' 
    '     Class Map
    ' 
    '         Properties: map2, tag2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Analysis.FastUnfolding

    Friend Class Matrix

        Public Property Q As Double
        Public Property tag_dict As Dictionary(Of String, String)
        Public Property tag_dict2 As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return $"Q={Q.ToString("F4")}"
        End Function
    End Class

    Friend Class Map
        Public Property tag2 As Dictionary(Of String, String)
        Public Property map2 As KeyMaps
    End Class

End Namespace
