#Region "Microsoft.VisualBasic::56cb293f4b305e37cf08aa0416aa0015, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Viridis.vb"

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

    '   Total Lines: 79
    '    Code Lines: 34
    ' Comment Lines: 32
    '   Blank Lines: 13
    '     File Size: 3.52 KB


    '     Class Viridis
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: cividis, fromHtml, inferno, magma, mako
    '                   plasma, rocket, turbo, viridis
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Drawing2D.Colors

    Public NotInheritable Class Viridis

        Private Sub New()
        End Sub

        Friend Shared Function fromHtml(ParamArray html As String()) As IEnumerable(Of Color)
            Return From value As String In html Select value.TranslateColor
        End Function

        ''' <summary>
        ''' viridis(12, option = "A")
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function magma() As IEnumerable(Of Color)
            Return fromHtml("#000004FF", "#120D32FF", "#331068FF", "#5A167EFF", "#7D2482FF", "#A3307EFF", "#C83E73FF", "#E95562FF", "#F97C5DFF", "#FEA873FF", "#FED395FF", "#FCFDBFFF")
        End Function

        ''' <summary>
        ''' viridis(12, option = "B")
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function inferno() As IEnumerable(Of Color)
            Return fromHtml("#000004FF", "#140B35FF", "#3A0963FF", "#60136EFF", "#85216BFF", "#A92E5EFF", "#CB4149FF", "#E65D2FFF", "#F78311FF", "#FCAD12FF", "#F5DB4BFF", "#FCFFA4FF")
        End Function

        ''' <summary>
        ''' viridis(12, option = "C")
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function plasma() As IEnumerable(Of Color)
            Return fromHtml("#0D0887FF", "#3E049CFF", "#6300A7FF", "#8707A6FF", "#A62098FF", "#C03A83FF", "#D5546EFF", "#E76F5AFF", "#F58C46FF", "#FDAD32FF", "#FCD225FF", "#F0F921FF")
        End Function

        ''' <summary>
        ''' viridis(12, option = "D")
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function viridis() As IEnumerable(Of Color)
            Return fromHtml("#440154FF", "#482173FF", "#433E85FF", "#38598CFF", "#2D708EFF", "#25858EFF", "#1E9B8AFF", "#2BB07FFF", "#51C56AFF", "#85D54AFF", "#C2DF23FF", "#FDE725FF")
        End Function

        ''' <summary>
        ''' viridis(12, option = "E")
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function cividis() As IEnumerable(Of Color)
            Return fromHtml("#00204DFF", "#00306FFF", "#2A406CFF", "#48526BFF", "#5E626EFF", "#727374FF", "#878479FF", "#9E9677FF", "#B6A971FF", "#D0BE67FF", "#EAD357FF", "#FFEA46FF")
        End Function

        ''' <summary>
        ''' viridis(12, option = "G")
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function mako() As IEnumerable(Of Color)
            Return fromHtml("#0B0405FF", "#231526FF", "#35264CFF", "#403A75FF", "#3D5296FF", "#366DA0FF", "#3487A6FF", "#35A1ABFF", "#43BBADFF", "#6CD3ADFF", "#ADE3C0FF", "#DEF5E5FF")
        End Function

        ''' <summary>
        ''' viridis(12, option = "F")
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function rocket() As IEnumerable(Of Color)
            Return fromHtml("#03051AFF", "#221331FF", "#451C47FF", "#6A1F56FF", "#921C5BFF", "#B91657FF", "#D92847FF", "#ED513EFF", "#F47C56FF", "#F6A47BFF", "#F7C9AAFF", "#FAEBDDFF")
        End Function

        ''' <summary>
        ''' viridis(12, option = "H")
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function turbo() As IEnumerable(Of Color)
            Return fromHtml("#30123BFF", "#4454C4FF", "#4490FEFF", "#1FC8DEFF", "#29EFA2FF", "#7DFF56FF", "#C1F334FF", "#F1CA3AFF", "#FE922AFF", "#EA4F0DFF", "#BE2102FF", "#7A0403FF")
        End Function

    End Class
End Namespace
