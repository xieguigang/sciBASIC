#Region "Microsoft.VisualBasic::8d3630ce3d12501be83f4273fc7b23fd, gr\network-visualization\Datavisualization.Network\Layouts\ForceDirected\Layout\Spring.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Spring
    ' 
    '         Properties: K, length, point1, point2
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class NearestPoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class BoundingBox
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file Spring.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Spring Interface
'@version 1.0
'
'@section LICENSE
'
'The MIT License (MIT)
'
'Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>
'
'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:
'
'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.
'
'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.
'
'@section DESCRIPTION
'
'An Interface for the Spring Class.
'
'

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Layouts

    Public Class Spring

        Public Property point1 As LayoutPoint
        Public Property point2 As LayoutPoint
        Public Property length As Single
        Public Property K As Single

        Public Sub New(point1 As LayoutPoint, point2 As LayoutPoint, length As Single, K As Single)
            Me.point1 = point1
            Me.point2 = point2
            Me.length = length
            Me.K = K
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class NearestPoint

        Public node As Node
        Public point As LayoutPoint
        Public distance As Single?

        Public Sub New()
            node = Nothing
            point = Nothing
            distance = Nothing
        End Sub
    End Class

    Public Class BoundingBox

        ' ~5% padding
        Public Const defaultBB As Single = 2.0F
        Public Const defaultPadding As Single = 0.07F

        Public topRightBack As AbstractVector
        Public bottomLeftFront As AbstractVector

        Public Sub New()
            topRightBack = Nothing
            bottomLeftFront = Nothing
        End Sub
    End Class
End Namespace
