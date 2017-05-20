#Region "Microsoft.VisualBasic::f9c15fb13d735dc3fbf1d7cd2ca53bd0, ..\sciBASIC#\gr\Datavisualization.Network\NetworkCanvas\Styling\StyleMapper.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Styling

    ''' <summary>
    ''' Network object visualize styling object model
    ''' </summary>
    Public Structure StyleMapper

        Dim nodeStyles As StyleCreator()
        Dim edgeStyles As StyleCreator()

        ''' <summary>
        ''' node label styling
        ''' </summary>
        Dim labelStyles As StyleCreator()

        Public Shared Function FromJSON(json$) As StyleMapper
            If json.FileExists Then
                json = json.ReadAllText
            End If

            Dim styleJSON As StyleJSON = json.LoadObject(Of StyleJSON)
            Return FromJSON(styleJSON)
        End Function

        Public Shared Function FromJSON(json As StyleJSON) As StyleMapper
            Return New StyleMapper With {
                .edgeStyles = StyleMapper.__createSelector(json.edge),
                .labelStyles = StyleMapper.__createSelector(json.labels),
                .nodeStyles = StyleMapper.__createSelector(json.nodes)
            }
        End Function

        Private Shared Function __createSelector(styles As Dictionary(Of String, Style)) As StyleCreator()
            Return styles _
                .Select(Function(x) __createSelector(x.Key, x.Value)) _
                .ToArray
        End Function

        Private Shared Function __createSelector(selector$, style As Style) As StyleCreator
            Dim mapper As New StyleCreator With {
                .selector = selector,
                .fill = style.fill.GetBrush,
                .font = CSSFont.TryParse(style.fontCSS),
                .stroke = Stroke.TryParse(style.stroke)
            }



            Return mapper 
        End Function
    End Structure

    Public Structure StyleCreator
        Dim selector$
        Dim stroke As Pen
        Dim font As Font
        Dim fill As Brush
        Dim size As Func(Of Object, Single)
        Dim label As Func(Of Object, String)
    End Structure
End Namespace
