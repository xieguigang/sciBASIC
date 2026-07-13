#Region "Microsoft.VisualBasic::e638ce37668ba633989320cdc374b825, gr\Landscape\COLLADA\ColladaTypes.vb"

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

    '   Total Lines: 13
    '    Code Lines: 8 (61.54%)
    ' Comment Lines: 3 (23.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (15.38%)
    '     File Size: 347 B


    '     Class GeometrySource
    ' 
    '         Properties: count, floatArray, sourceId, stride
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Collada

    ''' <summary>
    ''' 单个 &lt;source&gt; 元素的数据
    ''' </summary>
    Friend Class GeometrySource
        Public Property sourceId As String
        Public Property floatArray As Single()
        Public Property stride As Integer
        Public Property count As Integer
    End Class

End Namespace
