#Region "Microsoft.VisualBasic::65b404216fc941973031380ad00d0d22, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\Drivers\TimeValue.vb"

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

    '   Total Lines: 57
    '    Code Lines: 33
    ' Comment Lines: 13
    '   Blank Lines: 11
    '     File Size: 1.75 KB


    '     Structure TimeValue
    ' 
    '         Properties: Point
    ' 
    '         Function: BuildIndex, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Language

Namespace Darwinism.GAF.Driver

    Public Structure TimeValue

        ''' <summary>
        ''' X
        ''' </summary>
        Dim Time#
        ''' <summary>
        ''' ``(y) = f(x)``
        ''' </summary>
        Dim Y#

        Public ReadOnly Property Point As PointF
            Get
                Return New PointF(Time, Y)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{Time}] {Y}"
        End Function

        ''' <summary>
        ''' 从<paramref name="X"/>之中找出离<paramref name="y"/>之中的<see cref="TimeValue.Time"/>最近的元素然后生成index
        ''' </summary>
        ''' <param name="X#">假设X是从小到大排序的</param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function BuildIndex(X#(), y As TimeValue()) As Dictionary(Of Double, Integer)
            Dim index As New Dictionary(Of Double, Integer)

            ' 在这个函数里面不需要任何排序操作，否则会打乱原有的一一对应关系
            For Each time As TimeValue In y
                Dim minD# = Integer.MaxValue
                Dim yx#

                For i As Integer = 0 To X.Length - 1
                    Dim xi = X(i)
                    Dim d = Math.Abs(xi - time.Time)

                    If d <= minD Then
                        minD = d
                        yx = i
                    End If
                Next

                Call index.Add(time.Time, yx)
            Next

            Return index
        End Function
    End Structure
End Namespace
