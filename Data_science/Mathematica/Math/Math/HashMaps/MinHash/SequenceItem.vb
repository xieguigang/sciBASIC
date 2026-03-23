#Region "Microsoft.VisualBasic::0447458e5692331a4a8579ce4014a763, Data_science\Mathematica\Math\Math\HashMaps\MinHash\SequenceItem.vb"

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

    '   Total Lines: 22
    '    Code Lines: 10 (45.45%)
    ' Comment Lines: 7 (31.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (22.73%)
    '     File Size: 582 B


    '     Class SequenceItem
    ' 
    '         Properties: ID, Signature
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace HashMaps.MinHash

    ''' <summary>
    ''' 定义一个简单的数据结构来存储序列
    ''' </summary>
    Public Class SequenceItem

        Public Property ID As Integer
        ''' <summary>
        ''' 最终生成的MinHash签名
        ''' </summary>
        ''' <returns></returns>
        Public Property Signature As UInteger()

        Public Overrides Function ToString() As String
            Return $"[{ID}]_{Signature.GetJson}"
        End Function
    End Class

End Namespace
