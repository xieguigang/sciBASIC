Imports System.Runtime.CompilerServices

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
        Public Function asciiPrintContours(map As MarchingSquares, ByVal data As Double()(), ByVal levels As Double()) As String
            Dim s = ""
            ' Pad data to guarantee iso GeneralPaths will be closed shapes.
            Dim dataP = map.padData(data, levels)

            For i = 0 To levels.Length - 1
                ' Create contour for this level using Marching Squares algorithm.
                s += map.asciiContourPrint(map.mkContour(dataP, levels(i)))
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
        Private Function asciiContourPrint(map As MarchingSquares, ByVal a As IsoCell()()) As String
            Dim s = ""
            Dim rows = a.Length
            Dim cols = a(0).Length

            For j = 0 To cols - 1
                s += "==="
            Next

            s += vbLf

            For i = rows - 1 To 0 Step -1

                For j = 0 To cols - 1

                    Select Case a(i)(j).neighborInfo
                        Case 0, 1, 2, 3
                            s += "xxx"
                        Case 4
                            s += "x\ "
                        Case 5

                            If a(i)(j).flipped Then
                                s += "x\ "
                                Exit Select
                            End If

                        Case 7
                            s += "x/ "
                        Case 6
                            s += "x| "
                        Case 8
                            s += " /x"
                        Case 9
                            s += " |x"
                        Case 10

                            If a(i)(j).flipped Then
                                s += " /x"
                                Exit Select
                            End If

                        Case 11
                            s += " \x"
                        Case Else
                            s += "   "
                    End Select
                Next

                s += vbLf

                For j = 0 To cols - 1

                    Select Case a(i)(j).neighborInfo
                        Case 0
                            s += "xxx"
                        Case 1
                            s += "\xx"
                        Case 2
                            s += "xx/"
                        Case 3, 12
                            s += "---"
                        Case 4
                            s += "xx\"
                        Case 5

                            If a(i)(j).flipped Then
                                s += "\x\"
                            Else
                                s += "/ /"
                            End If

                        Case 6
                            s += "x| "
                        Case 7
                            s += "/  "
                        Case 8
                            s += "/xx"
                        Case 9
                            s += " |x"
                        Case 10

                            If a(i)(j).flipped Then
                                s += "/x/"
                            Else
                                s += "\ \"
                            End If

                        Case 11
                            s += "  \"
                        Case 13
                            s += "  /"
                        Case 14
                            s += "\  "
                        Case 15
                            s += "   "
                    End Select
                Next

                s += vbLf

                For j = 0 To cols - 1

                    Select Case a(i)(j).neighborInfo
                        Case 0, 4, 8, 12
                            s += "xxx"
                        Case 1
                            s += " \x"
                        Case 2
                            s += "x/ "
                        Case 3, 7, 11, 15
                            s += "   "
                        Case 5

                            If a(i)(j).flipped Then
                                s += " \x"
                            Else
                                s += " /x"
                            End If

                        Case 6
                            s += "x| "
                        Case 9
                            s += " |x"
                        Case 10

                            If a(i)(j).flipped Then
                                s += "x/ "
                                Exit Select
                            End If

                        Case 14
                            s += "x\ "
                        Case 13
                            s += " /x"
                    End Select
                Next

                s += vbLf
            Next

            For j = 0 To cols - 1
                s += "==="
            Next

            s += vbLf
            Return s
        End Function
    End Module
End Namespace