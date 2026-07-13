#Region "Microsoft.VisualBasic::79bc8001ac5d8d574a42d35b8418231f, gr\Landscape\Vector.vb"

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

    '   Total Lines: 40
    '    Code Lines: 28 (70.00%)
    ' Comment Lines: 6 (15.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (15.00%)
    '     File Size: 1.31 KB


    '     Structure Vector
    ' 
    '         Properties: Point3D, PointData
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Data

    Public Structure Vector

        ''' <summary>
        ''' 2017-1-30
        ''' 因为在进行XML存储的时候，可能会由于需要调整格式对齐数字，所以在这里使用字符串来存储数据，
        ''' 否则直接使用数组会因为格式的变化而无法被读取
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property Point3D As String

        Public ReadOnly Property PointData As Point3D
            Get
                Dim v As Single() = Me.Point3D _
                    .Split() _
                    .Where(Function(t) Not t.StringEmpty) _
                    .Select(Function(s) CSng(s)) _
                    .ToArray
                Return New Point3D(v(Scan0), v(1), v(2))
            End Get
        End Property

        Sub New(pt As Point3D)
            With pt
                Point3D = { .X, .Y, .Z}.JoinBy(" ")
            End With
        End Sub

        Public Overrides Function ToString() As String
            Return PointData.GetJson
        End Function
    End Structure
End Namespace
