#Region "Microsoft.VisualBasic::91d7e11d27e5f0164771feed74912b30, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\ContourTracing.vb"

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

    '   Total Lines: 361
    '    Code Lines: 198 (54.85%)
    ' Comment Lines: 120 (33.24%)
    '    - Xml Docs: 21.67%
    ' 
    '   Blank Lines: 43 (11.91%)
    '     File Size: 14.64 KB


    '     Module ContourTracing
    ' 
    '         Function: BitsToPaths, GetOutine, getRn, rem_euclid
    ' 
    '         Sub: trace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports bool = System.Boolean
Imports i8 = System.SByte
Imports stdNum = System.Math
Imports usize = System.Int32

Namespace Drawing2D.Math2D.MarchingSquares

    '
    ' Contour tracing library
    ' https://github.com/STPR/contour_tracing
    '
    ' Copyright (c) 2021, STPR - https://github.com/STPR
    '
    ' SPDX-License-Identifier: EUPL-1.2
    '

    ' A 2D library to trace contours.
    '
    ' # Features
    ' Core features:
    ' - Trace contours using the Theo Pavlidis' algorithm (connectivity: 4-connected)
    ' - Trace **outlines** in **clockwise direction**
    ' - Trace **holes** in **counterclockwise direction**
    ' - Input format: a 2D array of bits
    ' - Output format: a string of SVG Path commands
    '
    ' Manual parameters:
    ' - User can specify to close Or Not the paths (with the SVG Path **Z** command)
    ' 
    ' # Examples
    ' For examples, have a look at the **bits_to_paths** function below.

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://raw.githubusercontent.com/STPR/contour_tracing/main/rust/src/lib.rs
    ''' </remarks>
    Public Module ContourTracing

        ''' <summary>
        ''' Moore neighborhood
        ''' </summary>
        ReadOnly MN As (SByte, SByte)() = {(0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1)}
        ''' <summary>
        ''' Bottom left coordinates
        ''' </summary>
        ReadOnly O_VERTEX As (SByte, SByte)() = {(-1, 0), (0, 0), (-1, -1), (0, 0), (0, -1), (0, 0), (0, 0)}
        ''' <summary>
        ''' Bottom right coordinates
        ''' </summary>
        ReadOnly H_VERTEX As (SByte, SByte)() = {(0, 0), (0, 0), (-1, 0), (0, 0), (-1, -1), (0, 0), (0, -1)}
        ''' <summary>
        ''' Value to add into the array of contours
        ''' </summary>
        ReadOnly O_VALUE As SByte() = {1, 0, 2, 0, 4, 0, 8}
        ''' <summary>
        ''' (idem)
        ''' </summary>
        ReadOnly H_VALUE As SByte() = {-4, 0, -8, 0, -1, 0, -2}

        '
        ' contours: an array Of contours
        ' ol: outlines level
        ' hl: holes level
        ' rn: reachable neighbor - For the outlines: 0: none, 1: front left,  2: front, 3: front right
        '                        - for the holes:    0: none, 1: front right, 2: front, 3: front left
        ' o: orientation, e.g. to the east:
        '
        '          N
        '    ┏━━━━━━━━━━━┓
        '    ┃ 7   0   1 ┃
        '  W ┃ 6   o > 2 ┃ E   o = [2, 3, 4, 5, 6, 7, 0, 1]
        '    ┃ 5   4   3 ┃
        '    ┗━━━━━━━━━━━┛
        '          S

        ' /// A function that takes a 2D array of bits And an option as input And return a string of SVG Path commands as output.
        ' /// # Examples
        ' /// ```
        ' /// extern crate contour_tracing;
        ' /// use contour_tracing:bits_to_paths;
        ' /// ```
        ' /// - A simple example with the **closepaths option** set to **false**:
        ' /// ```
        ' /// # extern crate contour_tracing;
        ' /// # use contour_tracing:bits_to_paths;
        ' /// let bits = vec![vec![ 0,1,1,1,0,0,1,1,1,1,1 ],
        ' ///                 vec![ 1,0,0,0,1,0,1,0,0,0,1 ],
        ' ///                 vec![ 1,0,0,0,1,0,1,0,1,0,1 ],
        ' ///                 vec![ 1,0,0,0,1,0,1,0,0,0,1 ],
        ' ///                 vec![ 0,1,1,1,0,0,1,1,1,1,1 ]];
        ' ///
        ' /// # assert_eq!(bits_to_paths(bits.to_vec(), false), "M1 0H4V1H1M6 0H11V5H6M0 1H1V4H0M4 1H5V4H4M7 1V4H10V1M8 2H9V3H8M1 4H4V5H1");
        ' /// println!("{}", bits_to_paths(bits, false));
        ' /// ```
        ' /// - When the **closepaths option** Is set to **true**, each path Is closed with the SVG Path **Z** command:
        ' /// ```
        ' /// # extern crate contour_tracing;
        ' /// # use contour_tracing:bits_to_paths;
        ' /// # let bits = vec![vec![ 0,1,1,1,0,0,1,1,1,1,1 ],
        ' /// #                 vec![ 1,0,0,0,1,0,1,0,0,0,1 ],
        ' /// #                 vec![ 1,0,0,0,1,0,1,0,1,0,1 ],
        ' /// #                 vec![ 1,0,0,0,1,0,1,0,0,0,1 ],
        ' /// #                 vec![ 0,1,1,1,0,0,1,1,1,1,1 ]];
        ' /// # assert_eq!(bits_to_paths(bits.to_vec(), true), "M1 0H4V1H1ZM6 0H11V5H6ZM0 1H1V4H0ZM4 1H5V4H4ZM7 1V4H10V1ZM8 2H9V3H8ZM1 4H4V5H1Z");
        ' /// println!("{}", bits_to_paths(bits, true));
        ' /// ```
        ' /// - If you plan to reuse the array of bits after using this function, use the `to_vec()` method Like this:
        ' ///
        ' /// ```
        ' /// # extern crate contour_tracing;
        ' /// # use contour_tracing:bits_to_paths;
        ' /// let bits = vec![vec![ 1,0,0 ],
        ' ///                 vec![ 0,1,0 ],
        ' ///                 vec![ 0,0,1 ]];
        ' ///
        ' /// # assert_eq!(bits_to_paths(bits.to_vec(), true), "M0 0H1V1H0ZM1 1H2V2H1ZM2 2H3V3H2Z");
        ' /// println!("{}", bits_to_paths(bits.to_vec(), true));
        ' /// println!("{:?}", bits);
        ' /// ```

        Public Function GetOutine(map As MapMatrix) As GeneralPath
            Dim bitList As New List(Of SByte())

            For Each row In map.GetMatrixInterpolation
                Call row _
                    .Select(Function(d) CSByte(If(d > 0, 1, 0))) _
                    .ToArray _
                    .DoCall(AddressOf bitList.Add)
            Next

            Dim path As GeneralPath = bitList _
                .MatrixTranspose _
                .ToArray _
                .BitsToPaths

            Return path
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bits">
        ''' 在这里使用字节表示像素的情况：0表示空白，1表示有像素
        ''' </param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function BitsToPaths(bits As SByte()()) As GeneralPath
            Dim rows = bits.Length
            Dim cols = bits(Scan0).Length
            ' Add a border of 1 bit to prevent out-of-bounds error
            Dim contours = RectangularArray.Matrix(Of Integer)(rows + 2, cols + 2)

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    contours(r + 1)(c + 1) = If(bits(r)(c) = 1, 1, -1)
                Next
            Next

            Dim paths As New GeneralPath(0)
            Dim ol As Integer
            Dim hl As Integer

            For cursor_y As Integer = 1 To rows
                ol = 1
                hl = 1
                For cursor_x As Integer = 1 To cols
                    If ol = hl AndAlso contours(cursor_y)(cursor_x) = 1 Then
                        Call trace(True, cursor_x, cursor_y, {2, 3, 4, 5, 6, 7, 0, 1}, 2, (7, 1, 0), O_VERTEX, O_VALUE, contours, paths)
                    ElseIf ol > hl AndAlso contours(cursor_y)(cursor_x) = -1 Then
                        Call trace(False, cursor_x, cursor_y, {4, 5, 6, 7, 0, 1, 2, 3}, -2, (1, 7, 6), H_VERTEX, H_VALUE, contours, paths)
                    End If

                    Select Case stdNum.Abs(contours(cursor_y)(cursor_x))
                        Case 2, 4, 10, 12
                            If contours(cursor_y)(cursor_x) > 0 Then
                                ol += 1
                            Else
                                hl += 1
                            End If
                        Case 5, 7, 13, 15
                            If contours(cursor_y)(cursor_x) > 0 Then
                                ol -= 1
                            Else
                                hl -= 1
                            End If
                    End Select
                Next
            Next

            Return paths
        End Function

        Private Function getRn(neighbors As usize(), outline As bool, o As Integer()) As i8
            If outline Then
                If neighbors(o(7)) > 0 AndAlso neighbors(o(0)) > 0 Then
                    Return 1
                ElseIf neighbors(o(0)) > 0 Then
                    Return 2
                ElseIf neighbors(o(1)) > 0 AndAlso neighbors(o(2)) > 0 Then
                    Return 3
                Else
                    Return 0
                End If
            ElseIf neighbors(o(1)) < 0 AndAlso neighbors(o(0)) < 0 Then
                Return 1
            ElseIf neighbors(o(0)) < 0 Then
                Return 2
            ElseIf neighbors(o(7)) < 0 AndAlso neighbors(o(6)) < 0 Then
                Return 3
            Else
                Return 0
            End If
        End Function

        <Extension>
        Private Function rem_euclid(a As i8, b As Integer) As Integer
            If a > 0 Then
                Return a Mod b
            Else
                Return b + a
            End If
        End Function

        Private Sub trace(outline As bool, cursor_x As usize, cursor_y As usize,
                          o As Integer(),
                          rot As i8,
                          viv As (usize, usize, usize),
                          vertex As (i8, i8)(),
                          value As i8(),
                          contours As Integer()(),
                          paths As GeneralPath)

            Dim nsize As Integer = contours.Length * contours(Scan0).Length * 2
            Dim tracer_x = cursor_x
            Dim tracer_y = cursor_y
            Dim vertices_nbr As Int32 = 1
            Dim neighbors As usize()
            Dim rn As i8

            Call paths.MoveTo(tracer_x + vertex(o(0)).Item1, tracer_y + vertex(o(0)).Item2)

            Do
                neighbors = {
                    contours(tracer_y - 1)(tracer_x),
                    contours(tracer_y - 1)(tracer_x + 1),
                    contours(tracer_y)(tracer_x + 1),
                    contours(tracer_y + 1)(tracer_x + 1),
                    contours(tracer_y + 1)(tracer_x),
                    contours(tracer_y + 1)(tracer_x - 1),
                    contours(tracer_y)(tracer_x - 1),
                    contours(tracer_y - 1)(tracer_x - 1)
                }

                rn = getRn(neighbors, outline, o)

                ' let len = nums.len();
                ' nums.rotate_right(k As usize % len)

                ' let i = nums.len() as i32;
                ' nums.rotate_right(k.rem_euclid(i) As usize)

                Select Case rn
                    Case 1
                        contours(tracer_y)(tracer_x) += value(o(0))
                        tracer_x = tracer_x + MN(o(viv.Item1)).Item1
                        tracer_y = tracer_y + MN(o(viv.Item1)).Item2
                        vertices_nbr += 1

                        ' Rotate 90 degrees, counterclockwise For the outlines (rot = 2)
                        ' or clockwise For the holes (rot = -2)
                        Call o.RotateRight(rot.rem_euclid(8))

                        If o(0) = 0 OrElse o(0) = 4 Then
                            paths.LineTo(tracer_x + vertex(o(0)).Item1, tracer_y)
                        Else
                            paths.LineTo(tracer_x, tracer_y + vertex(o(0)).Item2)
                        End If
                    Case 2
                        contours(tracer_y)(tracer_x) += value(o(0))
                        tracer_x = tracer_x + MN(o(0)).Item1
                        tracer_y = tracer_y + MN(o(0)).Item2
                    Case 3
                        contours(tracer_y)(tracer_x) += value(o(0))

                        ' Rotate 90 degrees, clockwise For the outlines (rot = 2)
                        ' or counterclockwise For the holes (rot = -2)
                        Call o.RotateLeft(rot.rem_euclid(8))

                        contours(tracer_y)(tracer_x) += value(o(0))
                        vertices_nbr += 1

                        If o(0) = 0 OrElse o(0) = 4 Then
                            paths.LineTo(tracer_x + vertex(o(0)).Item1, tracer_y)
                        Else
                            paths.LineTo(tracer_x, tracer_y + vertex(o(0)).Item2)
                        End If

                        Call o.RotateRight(rot.rem_euclid(8))

                        tracer_x = tracer_x + MN(o(viv.Item2)).Item1
                        tracer_y = tracer_y + MN(o(viv.Item2)).Item2
                        vertices_nbr += 1

                        If o(0) = 0 OrElse o(0) = 4 Then
                            paths.LineTo(tracer_x + vertex(o(0)).Item1, tracer_y)
                        Else
                            paths.LineTo(tracer_x, tracer_y + vertex(o(0)).Item2)
                        End If
                    Case Else
                        contours(tracer_y)(tracer_x) += value(o(0))
                        vertices_nbr += 1

                        Call o.RotateLeft(rot.rem_euclid(8))

                        If o(0) = 0 OrElse o(0) = 4 Then
                            paths.LineTo(tracer_x + vertex(o(0)).Item1, tracer_y)
                        Else
                            paths.LineTo(tracer_x, tracer_y + vertex(o(0)).Item2)
                        End If
                End Select

                If tracer_x = cursor_x AndAlso tracer_y = cursor_y AndAlso vertices_nbr > 2 Then
                    Exit Do
                End If
                If paths.temp > nsize Then
                    Exit Do
                End If
            Loop

            Do
                contours(tracer_y)(tracer_x) += value(o(0))

                If o(0) = viv.Item3 Then
                    Exit Do
                Else
                    Call o.RotateLeft(rot.rem_euclid(8))
                End If

                vertices_nbr += 1

                If o(0) = 0 OrElse o(0) = 4 Then
                    paths.LineTo(tracer_x + (vertex(o(0)).Item1), tracer_y)
                Else
                    paths.LineTo(tracer_x, tracer_y + (vertex(o(0)).Item2))
                End If
                If paths.temp > nsize Then
                    Exit Do
                End If
            Loop

            Call paths.ClosePath()
        End Sub
    End Module
End Namespace
