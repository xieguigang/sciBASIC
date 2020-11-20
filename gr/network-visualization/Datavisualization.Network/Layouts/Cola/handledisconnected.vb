#Region "Microsoft.VisualBasic::4586ccd331c6179128d502679b2f50b2, gr\network-visualization\Datavisualization.Network\Layouts\Cola\handledisconnected.vb"

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

    '     Class packingOptions
    ' 
    ' 
    ' 
    '     Class handleDisconnected
    ' 
    '         Properties: get_real_ratio
    ' 
    '         Function: [step], get_entire_width, separateGraphs
    ' 
    '         Sub: apply, applyPacking, calculate_bb, explore_node, put_nodes_to_right_positions
    '              put_rect
    ' 
    '     Class Graph
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports any = System.Object
Imports number = System.Double
Imports stdNum = System.Math

Namespace Layouts.Cola

    Public Class packingOptions
        Public PADDING As Integer = 10
        Public GOLDEN_SECTION As Double = (1 + stdNum.Sqrt(5)) / 2
        Public FLOAT_EPSILON As Double = 0.0001
        Public MAX_INERATIONS As Integer = 100
    End Class

    Public Class handleDisconnected

        ReadOnly packingOptions As New packingOptions

        ''' <summary>
        ''' get bounding boxes for all separate graphs
        ''' </summary>
        ''' <param name="graphs"></param>
        Private Sub calculate_bb(graphs As List(Of Graph), node_size#)
            graphs.DoEach(Sub(graph)
                              Dim min_x = number.MaxValue, min_y = number.MaxValue
                              Dim max_x = 0, max_y = 0

                              graph.array.DoEach(Sub(v)
                                                     Dim w = If(v.width <> 0, v.width, node_size)
                                                     Dim h = If(v.height <> 0, v.height, node_size)
                                                     w /= 2
                                                     h /= 2
                                                     max_x = System.Math.Max(v.x + w, max_x)
                                                     min_x = System.Math.Min(v.x - w, min_x)
                                                     max_y = System.Math.Max(v.y + h, max_y)
                                                     min_y = System.Math.Min(v.y - h, min_y)
                                                 End Sub)

                              graph.Width = max_x - min_x
                              graph.Height = max_y - min_y
                          End Sub)
        End Sub

        ''' <summary>
        ''' starts box packing algorithm, desired ratio is 1 by default
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="desired_ratio"></param>
        Private Sub apply(data As List(Of Graph), desired_ratio As Double)
            Dim curr_best_f = number.MaxValue
            Dim curr_best = 0
            data.Sort(Function(a, b) b.Height - a.Height)

            'min_width = data.Reduce(Function(a, b) {
            '    return a.width < b.width ? a.width : b.width;
            '});

            min_width = Aggregate g In data Into Min(g.Width)

            Dim x1 = min_width
            Dim x2 = get_entire_width(data)
            Dim left = x1
            Dim right = x2
            Dim iterationCounter = 0
            Dim f_x1 = number.MaxValue
            Dim f_x2 = number.MaxValue
            Dim flag = -1

            ' determines which among f_x1 and f_x2 to recompute
            Dim dx = number.MaxValue
            Dim df = number.MaxValue

            While (dx > min_width) OrElse df > packingOptions.FLOAT_EPSILON

                If flag <> 1 Then
                    x1 = right - (right - left) / packingOptions.GOLDEN_SECTION
                    f_x1 = [step](data, x1, desired_ratio)
                End If
                If flag <> 0 Then
                    x2 = left + (right - left) / packingOptions.GOLDEN_SECTION
                    f_x2 = [step](data, x2, desired_ratio)
                End If

                dx = stdNum.Abs(x1 - x2)
                df = stdNum.Abs(f_x1 - f_x2)

                If f_x1 < curr_best_f Then
                    curr_best_f = f_x1
                    curr_best = x1
                End If

                If f_x2 < curr_best_f Then
                    curr_best_f = f_x2
                    curr_best = x2
                End If

                If f_x1 > f_x2 Then
                    left = x1
                    x1 = x2
                    f_x1 = f_x2
                    flag = 1
                Else
                    right = x2
                    x2 = x1
                    f_x2 = f_x1
                    flag = 0
                End If

                If System.Math.Max(Interlocked.Increment(iterationCounter), iterationCounter - 1) > 100 Then
                    Exit While
                End If
            End While

            ' plot(data, min_width, get_entire_width(data), curr_best, curr_best_f);
            [step](data, curr_best, desired_ratio)
        End Sub

        ' one iteration of the optimization method
        ' (gives a proper, but not necessarily optimal packing)
        Private Function [step](data As List(Of Graph), max_width As Double, desired_ratio As Double) As Double
            line = New List(Of Rectangle2D)
            real_width = 0
            real_height = 0
            global_bottom = init_y

            For i As Integer = 0 To data.Count - 1
                Dim o = data(i)
                put_rect(o, max_width)
            Next

            Return stdNum.Abs(get_real_ratio() - desired_ratio)
        End Function

        ' looking for a position to one box
        Private Sub put_rect(rect As Rectangle2D, max_width As Double)
            Dim parent As Rectangle2D = Nothing

            For i As Integer = 0 To line.Count - 1
                If (line(i).space_left >= rect.Height) AndAlso (line(i).X + line(i).Width + rect.Width + packingOptions.PADDING - max_width) <= packingOptions.FLOAT_EPSILON Then
                    parent = line(i)
                    Exit For
                End If
            Next

            line.Add(rect)

            If parent IsNot Nothing Then
                rect.X = parent.X + parent.Width + packingOptions.PADDING
                rect.Y = parent.Bottom
                rect.space_left = rect.Height
                rect.Bottom = rect.Y
                parent.space_left -= rect.Height + packingOptions.PADDING
                parent.Bottom += rect.Height + packingOptions.PADDING
            Else
                rect.Y = global_bottom
                global_bottom += rect.Height + packingOptions.PADDING
                rect.X = init_x
                rect.Bottom = rect.Y
                rect.space_left = rect.Height
            End If

            If rect.Y + rect.Height - real_height > -packingOptions.FLOAT_EPSILON Then
                real_height = rect.Y + rect.Height - init_y
            End If
            If rect.X + rect.Width - real_width > -packingOptions.FLOAT_EPSILON Then
                real_width = rect.X + rect.Width - init_x
            End If

        End Sub

        ''' <summary>
        ''' actual assigning of position to nodes
        ''' </summary>
        ''' <param name="graphs"></param>
        Private Sub put_nodes_to_right_positions(graphs As List(Of Graph))
            graphs.DoEach(Sub(g)
                              ' calculate current graph center:
                              Dim center = New Point2D() With {.X = 0, .Y = 0}

                              g.array.ForEach(Sub(node)
                                                  center.X += node.x
                                                  center.Y += node.y
                                              End Sub)

                              center.X /= g.array.Count
                              center.Y /= g.array.Count

                              ' calculate current top left corner:
                              Dim corner = New Point2D() With {
                                .X = center.X - g.Width / 2,
                                .Y = center.Y - g.Height / 2
                              }
                              Dim offset = New Point2D() With {
                                .X = g.X - corner.X + svg_width \ 2 - real_width / 2,
                                .Y = g.Y - corner.Y + svg_height \ 2 - real_height / 2
                              }

                              ' put nodes:
                              g.array.ForEach(Sub(node)
                                                  node.x += offset.X
                                                  node.y += offset.Y
                                              End Sub)
                          End Sub)
        End Sub

        Dim init_x As Integer = 0, init_y As Integer = 0
        Dim svg_width As Integer, svg_height As Integer
        Dim real_width As Double = 0, real_height As Double = 0, min_width As Double = 0, global_bottom As Double = 0
        Dim line As New List(Of Rectangle2D)

        ''' <summary>
        ''' assign x, y to nodes while using box packing algorithm for disconnected graphs
        ''' </summary>
        ''' <param name="graphs"></param>
        ''' <param name="w"></param>
        ''' <param name="h"></param>
        ''' <param name="node_size"></param>
        ''' <param name="desired_ratio"></param>
        ''' <param name="centerGraph"></param>
        ''' <remarks>
        ''' 这个函数是整个计算流程的起始函数
        ''' </remarks>
        Public Sub applyPacking(graphs As List(Of Graph), w As Integer, h As Integer, Optional node_size As Double? = Nothing, Optional desired_ratio As Integer? = 1, Optional centerGraph As Boolean = True)

            init_x = 0
            init_y = 0
            svg_width = w
            svg_height = h

            desired_ratio = If(desired_ratio IsNot Nothing, desired_ratio, 1)
            node_size = If(node_size IsNot Nothing, node_size, 0)

            real_width = 0
            real_height = 0
            min_width = 0
            global_bottom = 0

            If graphs.Count = 0 Then
                Return
            End If

            ' that would take care of single nodes problem
            ' graphs.forEach(function (g) {
            '     if (g.array.length == 1) {
            '         g.array[0].x = 0;
            '         g.array[0].y = 0;
            '     }
            ' });


            calculate_bb(graphs, node_size)
            apply(graphs, desired_ratio)
            If centerGraph Then
                put_nodes_to_right_positions(graphs)
            End If



            'function plot(data, left, right, opt_x, opt_y) {
            '    // plot the cost function
            '    var plot_svg = d3.select("body").append("svg")
            '        .attr("width", function () { return 2 * (right - left); })
            '        .attr("height", 200);


            '    var x = d3.time.scale().range([0, 2 * (right - left)]);

            '    var xAxis = d3.svg.axis().scale(x).orient("bottom");
            '    plot_svg.append("g").attr("class", "x axis")
            '        .attr("transform", "translate(0, 199)")
            '        .call(xAxis);

            '    var lastX = 0;
            '    var lastY = 0;
            '    var value = 0;
            '    for (var r = left; r < right; r += 1) {
            '        value = step(data, r);
            '        // value = 1;

            '        plot_svg.append("line").attr("x1", 2 * (lastX - left))
            '            .attr("y1", 200 - 30 * lastY)
            '            .attr("x2", 2 * r - 2 * left)
            '            .attr("y2", 200 - 30 * value)
            '            .style("stroke", "rgb(6,120,155)");

            '        lastX = r;
            '        lastY = value;
            '    }

            '    plot_svg.append("circle").attr("cx", 2 * opt_x - 2 * left).attr("cy", 200 - 30 * opt_y)
            '        .attr("r", 5).style('fill', "rgba(0,0,0,0.5)");

            '}

        End Sub

        Private Function get_entire_width(data) As Double
            Dim width = 0.0
            data.forEach(Sub(d) width += d.width + packingOptions.PADDING)
            Return width
        End Function

        Public ReadOnly Property get_real_ratio() As Double
            Get
                Return (real_width / real_height)
            End Get
        End Property

        ''' <summary>
        ''' connected components of graph, returns an array of {}
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <param name="links"></param>
        ''' <returns></returns>
        Public Shared Function separateGraphs(nodes As any, links As Link(Of Node)()) As List(Of Graph)
            Dim marks = New Object() {}
            Dim ways As New List(Of List(Of Node))
            Dim graphs As New List(Of Graph)
            Dim clusters = 0

            For i As Integer = 0 To links.Length - 1
                Dim link = links(i)
                Dim n1 = link.source
                Dim n2 = link.target

                If Not ways(n1.index) Is Nothing Then
                    ways(n1.index).Add(n2)
                Else
                    ways(n1.index) = New List(Of Node) From {n2}
                End If

                If Not ways(n2.index) Is Nothing Then
                    ways(n2.index).Add(n1)
                Else
                    ways(n2.index) = New List(Of Node) From {n1}
                End If
            Next

            For i As Integer = 0 To nodes.length - 1
                Dim node = nodes(i)

                If marks(node.index) IsNot Nothing Then
                    Continue For
                Else
                    Call explore_node(node, True, marks, clusters, ways, graphs)
                End If
            Next

            Return graphs
        End Function

        Private Shared Sub explore_node(n As Node, is_new As Boolean, marks As any(), ByRef clusters As Integer, ways As List(Of List(Of Node)), graphs As List(Of Graph))
            If marks(n.index) IsNot Nothing Then
                Return
            End If
            If is_new Then
                clusters += 1
                graphs.Add(New Graph With {
                           .array = New List(Of Node)
                           })
            End If
            marks(n.index) = clusters
            graphs(clusters - 1).array.Add(n)

            Dim adjacent = ways(n.index)

            If adjacent Is Nothing Then
                Return
            End If

            For j As Integer = 0 To adjacent.Count - 1
                explore_node(adjacent(j), False, marks, clusters, ways, graphs)
            Next
        End Sub
    End Class

    Public Class Graph : Inherits Rectangle2D
        Public array As New List(Of Node)
    End Class
End Namespace
