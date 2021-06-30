Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Drawing2D.Math2D.MarchingSquares

    Public Module ASCIIPrinter

        ''' <summary>
        ''' Mainly for debugging, this can be called to have ascii art contours
        ''' printed for the data.
        ''' </summary>
        ''' <param name="data"> measured data to use for isoline generation. </param>
        ''' <param name="levels"> thresholds to use as iso levels. </param>
        ''' <returns> return a string of ascii art corresponding to Marching Squares
        ''' generation if isolines. </returns>
        Public Function asciiPrintContours(map As MarchingSquares, data As Double()(), levels As Double()) As String
            Dim s = ""
            ' Pad data to guarantee iso GeneralPaths will be closed shapes.
            Dim dataP = map.padData(data, levels)

            For i = 0 To levels.Length - 1
                ' Create contour for this level using Marching Squares algorithm.
                s += map.asciiContourPrint(map.makeContour(dataP, levels(i)))
                s += vbLf
            Next

            Return s
        End Function

        ''' <summary>
        ''' Mainly for debugging, print an ascii version of this contour.
        ''' </summary>
        ''' <param name="a"> array of contour neighbor values. </param>
        ''' <returns> string roughly representing contour in 'a'. </returns>
        <Extension>
        Private Function asciiContourPrint(map As MarchingSquares, a As IsoCell()()) As String
            Dim s As New StringBuilder
            Dim rows = a.Length
            Dim cols = a(0).Length

            For j = 0 To cols - 1
                s.Append("===")
            Next

            s.AppendLine()

            For i = rows - 1 To 0 Step -1

                For j = 0 To cols - 1

                    Select Case a(i)(j).neighborInfo
                        Case 0, 1, 2, 3
                            s.Append("xxx")
                        Case 4
                            s.Append("x\ ")
                        Case 5

                            If a(i)(j).flipped Then
                                s.Append("x\ ")
                                Exit Select
                            End If

                        Case 7
                            s.Append("x/ ")
                        Case 6
                            s.Append("x| ")
                        Case 8
                            s.Append(" /x")
                        Case 9
                            s.Append(" |x")
                        Case 10

                            If a(i)(j).flipped Then
                                s.Append(" /x")
                                Exit Select
                            End If

                        Case 11
                            s.Append(" \x")
                        Case Else
                            s.Append("   ")
                    End Select
                Next

                s.AppendLine()

                For j = 0 To cols - 1

                    Select Case a(i)(j).neighborInfo
                        Case 0
                            s.Append("xxx")
                        Case 1
                            s.Append("\xx")
                        Case 2
                            s.Append("xx/")
                        Case 3, 12
                            s.Append("---")
                        Case 4
                            s.Append("xx\")
                        Case 5

                            If a(i)(j).flipped Then
                                s.Append("\x\")
                            Else
                                s.Append("/ /")
                            End If

                        Case 6
                            s.Append("x| ")
                        Case 7
                            s.Append("/  ")
                        Case 8
                            s.Append("/xx")
                        Case 9
                            s.Append(" |x")
                        Case 10

                            If a(i)(j).flipped Then
                                s.Append("/x/")
                            Else
                                s.Append("\ \")
                            End If

                        Case 11
                            s.Append("  \")
                        Case 13
                            s.Append("  /")
                        Case 14
                            s.Append("\  ")
                        Case 15
                            s.Append("   ")
                    End Select
                Next

                s.AppendLine()

                For j = 0 To cols - 1

                    Select Case a(i)(j).neighborInfo
                        Case 0, 4, 8, 12
                            s.Append("xxx")
                        Case 1
                            s.Append(" \x")
                        Case 2
                            s.Append("x/ ")
                        Case 3, 7, 11, 15
                            s.Append("   ")
                        Case 5

                            If a(i)(j).flipped Then
                                s.Append(" \x")
                            Else
                                s.Append(" /x")
                            End If

                        Case 6
                            s.Append("x| ")
                        Case 9
                            s.Append(" |x")
                        Case 10

                            If a(i)(j).flipped Then
                                s.Append("x/ ")
                            End If

                        Case 14
                            s.Append("x\ ")
                        Case 13
                            s.Append(" /x")
                    End Select
                Next

                s.AppendLine()
            Next

            For j = 0 To cols - 1
                s.Append("===")
            Next

            s.AppendLine()

            Return s.ToString
        End Function
    End Module
End Namespace