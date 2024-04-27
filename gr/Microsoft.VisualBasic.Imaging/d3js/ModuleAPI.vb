﻿#Region "Microsoft.VisualBasic::6d74122cace1213a8d034d726989ab43, G:/GCModeller/src/runtime/sciBASIC#/gr/Microsoft.VisualBasic.Imaging//d3js/ModuleAPI.vb"

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

    '   Total Lines: 86
    '    Code Lines: 68
    ' Comment Lines: 9
    '   Blank Lines: 9
    '     File Size: 3.60 KB


    '     Module ModuleAPI
    ' 
    '         Function: forcedirectedLabeler, GetLabelAnchors, Label, labeler
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace d3js

    Public Module ModuleAPI

        Public Function forcedirectedLabeler(Optional ejectFactor As Integer = 6,
                                             Optional condenseFactor As Integer = 5,
                                             Optional dist$ = "30,250",
                                             Optional avoidRegions As RectangleF() = Nothing) As Forcedirected
            Return New Forcedirected(
                ejectFactor:=ejectFactor,
                dist:=dist,
                condenseFactor:=condenseFactor,
                avoidRegions:=avoidRegions
            )
        End Function

        ''' <summary>
        ''' A D3 plug-in for automatic label placement using simulated annealing that easily 
        ''' incorporates into existing D3 code, with syntax mirroring other D3 layouts.
        ''' </summary>
        ''' <param name="w_len">
        ''' penalty for length of leader line. positive value for penalty, zero for dont care and negative for encourage
        ''' </param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function labeler(Optional maxMove# = 5,
                                Optional maxAngle# = 0.5,
                                Optional w_len# = 0.2,      ' leader line length 
                                Optional w_inter# = 1.0,    ' leader line intersection
                                Optional w_lab2# = 30.0,    ' label-label overlap
                                Optional w_lab_anc# = 30.0, ' label-anchor overlap
                                Optional w_orient# = 3.0    ' orientation bias
                                ) As Labeler

            Return New Labeler With {
                .maxAngle = maxAngle,
                .maxMove = maxMove,
                .w_inter = w_inter,
                .w_lab2 = w_lab2,
                .w_lab_anc = w_lab_anc,
                .w_len = w_len,
                .w_orient = w_orient
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetLabelAnchors(labels As IEnumerable(Of Label), r!) As Anchor()
            Return labels _
                .Select(Function(l)
                            Return New Anchor With {
                                .r = r,
                                .x = l.X,
                                .y = l.Y
                            }
                        End Function) _
                .ToArray
        End Function

        <Extension>
        Public Function Label(g As Graphics2D, labels As IEnumerable(Of String), Optional fontCSS$ = CSSFont.Win7Normal) As IEnumerable(Of Label)
            Dim font As Font = CSSFont _
                .TryParse(fontCSS) _
                .GDIObject(g.Dpi)

            Return labels _
                .SafeQuery _
                .Select(Function(s$)
                            Dim size As SizeF = g.MeasureString(s, font)

                            Return New Label With {
                                .text = s,
                                .width = size.Width,
                                .height = size.Height
                            }
                        End Function)
        End Function
    End Module
End Namespace
