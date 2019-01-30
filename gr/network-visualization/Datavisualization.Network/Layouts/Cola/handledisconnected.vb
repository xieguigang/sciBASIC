Imports System.Threading
Imports any = System.Object
Imports number = System.Double

Namespace Layouts.Cola

    Public Class packingOptions
        Public PADDING As Integer = 10
        Public GOLDEN_SECTION As Double = (1 + Math.Sqrt(5)) / 2
        Public FLOAT_EPSILON As Double = 0.0001
        Public MAX_INERATIONS As Integer = 100
    End Class

    Public Module handleDisconnected

        ReadOnly packingOptions As New packingOptions

        ''' <summary>
        ''' get bounding boxes for all separate graphs
        ''' </summary>
        ''' <param name="graphs"></param>
        Private Sub calculate_bb(graphs As any(), node_size#)
            graphs.DoEach(Sub(graph)
                              Dim min_x = number.MaxValue, min_y = number.MaxValue
                              Dim max_x = 0, max_y = 0

                              graph.array.forEach(Sub(v)
                                                      Dim w = If(v.width IsNot Nothing, v.width, node_size)
                                                      Dim h = If(v.height IsNot Nothing, v.height, node_size)
                                                      w /= 2
                                                      h /= 2
                                                      max_x = System.Math.Max(v.x + w, max_x)
                                                      min_x = System.Math.Min(v.x - w, min_x)
                                                      max_y = System.Math.Max(v.y + h, max_y)
                                                      min_y = System.Math.Min(v.y - h, min_y)
                                                  End Sub)

                              graph.width = max_x - min_x
                              graph.height = max_y - min_y
                          End Sub)
        End Sub

        ''' <summary>
        ''' assign x, y to nodes while using box packing algorithm for disconnected graphs
        ''' </summary>
        ''' <param name="graphs"></param>
        ''' <param name="w"></param>
        ''' <param name="h"></param>
        ''' <param name="node_size"></param>
        ''' <param name="desired_ratio"></param>
        ''' <param name="centerGraph"></param>
        Public Sub applyPacking(graphs As Array(Of any), w As Integer, h As Integer, Optional node_size As Double? = Nothing, Optional desired_ratio As Integer? = 1, Optional centerGraph As Boolean = True)

            Dim init_x As Integer = 0, init_y As Integer = 0, svg_width As Integer = w, svg_height As Integer = h

            desired_ratio = If(desired_ratio IsNot Nothing, desired_ratio, 1)
            node_size = If(node_size IsNot Nothing, node_size, 0)

            Dim real_width As Double = 0, real_height As Double = 0, min_width As Double = 0, global_bottom As Double = 0

            Dim line As Object() = {}

            If graphs.length = 0 Then
                Return
            End If

            ' that would take care of single nodes problem
            ' graphs.forEach(function (g) {
            '     if (g.array.length == 1) {
            '         g.array[0].x = 0;
            '         g.array[0].y = 0;
            '     }
            ' });


            calculate_bb(graphs)
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

            ' actual assigning of position to nodes
            Dim put_nodes_to_right_positions = Function(graphs As Array(Of any))
                                                   graphs.forEach(Function(g)
                                                                      ' calculate current graph center:
                                                                      Dim center = New Point() With {
            Key.x = 0,
            Key.y = 0
        }

                                                                      g.array.forEach(Function(node)
                                                                                          center.x += node.x
                                                                                          center.y += node.y

                                                                                      End Function)

                                                                      center.x /= g.array.length
                                                                      center.y /= g.array.length

                                                                      ' calculate current top left corner:
                                                                      Dim corner = New Point() With {
            Key.x = center.x - g.width / 2,
            Key.y = center.y - g.height / 2
        }
                                                                      Dim offset = New Point() With {
            Key.x = g.x - corner.x + svg_width \ 2 - real_width / 2,
            Key.y = g.y - corner.y + svg_height \ 2 - real_height / 2
        }

                                                                      ' put nodes:
                                                                      g.array.forEach(Function(node)
                                                                                          node.x += offset.x
                                                                                          node.y += offset.y

                                                                                      End Function)

                                                                  End Function)

                                               End Function

            ' starts box packing algorithm
            ' desired ratio is 1 by default
            Dim apply = Function(data, desired_ratio)
                            Dim curr_best_f = number.POSITIVE_INFINITY
                            Dim curr_best = 0
                            data.sort(Function(a, b) b.height - a.height)

                            min_width = data.reduce(Function(a, b)
                                                        Return If(a.width < b.width, a.width, b.width)

                                                    End Function)

                            Dim left = InlineAssignHelper(x1, min_width)
                            Dim right = InlineAssignHelper(x2, get_entire_width(data))
                            Dim iterationCounter = 0

                            Dim f_x1 = number.MAX_VALUE
                            Dim f_x2 = number.MAX_VALUE
                            Dim flag = -1
                            ' determines which among f_x1 and f_x2 to recompute

                            Dim dx = number.MAX_VALUE
                            Dim df = number.MAX_VALUE

                            While (dx > min_width) OrElse df > packingOptions.FLOAT_EPSILON

                                If flag <> 1 Then
                                    Dim x1 = right - (right - left) / packingOptions.GOLDEN_SECTION
                                    Dim f_x1 = [step](data, x1)
                                End If
                                If flag <> 0 Then
                                    Dim x2 = left + (right - left) / packingOptions.GOLDEN_SECTION
                                    Dim f_x2 = [step](data, x2)
                                End If

                                dx = Math.Abs(x1 - x2)
                                df = Math.Abs(f_x1 - f_x2)

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
                            [step](data, curr_best)

                        End Function

            ' one iteration of the optimization method
            ' (gives a proper, but not necessarily optimal packing)
            Dim [step] = Function(data As any(), max_width As Double)
                             line = New any() {}
                             real_width = 0
                             real_height = 0
                             global_bottom = init_y

                             For i As var = 0 To data.Length - 1
                                 Dim o = data(i)
                                 put_rect(o, max_width)
                             Next

                             Return Math.Abs(get_real_ratio() - desired_ratio)

                         End Function

            ' looking for a position to one box
            Dim put_rect = Function(rect As any, max_width As Double)


                               Dim parent = undefined

                               For i As var = 0 To line.Length - 1
                                   If (line(i).space_left >= rect.height) AndAlso (line(i).x + line(i).width + rect.width + packingOptions.PADDING - max_width) <= packingOptions.FLOAT_EPSILON Then
                                       parent = line(i)
                                       Exit For
                                   End If
                               Next

                               line.push(rect)

                               If parent IsNot Nothing Then
                                   rect.x = parent.x + parent.width + packingOptions.PADDING
                                   rect.y = parent.bottom
                                   rect.space_left = rect.height
                                   rect.bottom = rect.y
                                   parent.space_left -= rect.height + packingOptions.PADDING
                                   parent.bottom += rect.height + packingOptions.PADDING
                               Else
                                   rect.y = global_bottom
                                   global_bottom += rect.height + packingOptions.PADDING
                                   rect.x = init_x
                                   rect.bottom = rect.y
                                   rect.space_left = rect.height
                               End If

                               If rect.y + rect.height - real_height > -packingOptions.FLOAT_EPSILON Then
                                   real_height = rect.y + rect.height - init_y
                               End If
                               If rect.x + rect.width - real_width > -packingOptions.FLOAT_EPSILON Then
                                   real_width = rect.x + rect.width - init_x
                               End If

                           End Function

            Dim get_entire_width = Function(data)
                                       Dim width = 0
                                       data.forEach(Function(d) width += d.width + packingOptions.PADDING)
                                       Return width

                                   End Function


            Dim get_real_ratio = Function()
                                     Return (real_width / real_height)

                                 End Function
        End Sub

        ''' <summary>
        ''' connected components of graph, returns an array of {}
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <param name="links"></param>
        ''' <returns></returns>
        Public Function separateGraphs(nodes As any, links As Link(Of Node)()) As Object
            Dim marks = New Object() {}
            Dim ways As New List(Of List(Of Node))
            Dim graphs = New Object() {}
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
                    ways(n2.index) = New List(Of Node) {n1}
                End If
            Next

            For i As Integer = 0 To nodes.length - 1
                Dim node = nodes(i)

                If marks(node.index) IsNot Nothing Then
                    Continue For
                Else
                    Call explore_node(node, True, marks, clusters, ways)
                End If
            Next



            Return graphs
        End Function

        Private Sub explore_node(n As Node, is_new As Boolean, marks As any(), ByRef clusters As Integer, ways As List(Of List(Of Node)))
            If marks(n.index) IsNot Nothing Then
                Return
            End If
            If is_new Then
                clusters += 1
                graphs.push(New With {
Key .array = New Object() {}
})
            End If
            marks(n.index) = clusters
            graphs(clusters - 1).array.push(n)

            Dim adjacent = ways(n.index)

            If adjacent Is Nothing Then
                Return
            End If

            For j As Integer = 0 To adjacent.Count - 1
                explore_node(adjacent(j), False, marks, clusters, ways)
            Next
        End Sub

        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Module
End Namespace