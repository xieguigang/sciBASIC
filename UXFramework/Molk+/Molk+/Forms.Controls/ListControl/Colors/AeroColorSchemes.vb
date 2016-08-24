#Region "Microsoft.VisualBasic::5c39ea19154d222dfbb1a42d3a7ec0fb, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\Colors\AeroColorSchemes.vb"

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

Module AeroColorSchemes

#Region "Internal ReadOnly Fields"

    ReadOnly _SelectedNormal As Color() = New Color() {Color.FromArgb(255, 237, 156), Color.FromArgb(255, 237, 156), Color.FromArgb(255, 216, 108), Color.FromArgb(255, 196, 0), Color.White}
    ReadOnly _SelectedHover As Color() = New Color() {Color.FromArgb(255, 237, 156), Color.FromArgb(255, 247, 166), Color.FromArgb(255, 216, 118), Color.FromArgb(255, 230, 50), Color.White}
    ReadOnly _SelectedPressed As Color() = New Color() {Color.FromArgb(225, 207, 126), Color.FromArgb(255, 227, 136), Color.FromArgb(255, 192, 35), Color.Gold, Color.White}
    ReadOnly _SelectedBorder As SolidBrush = New SolidBrush(Color.FromArgb(194, 138, 48))
    ReadOnly _UnSelectedNormal As Color() = New Color() {Color.White, Color.White, Color.White, Color.White, Color.White}
    ReadOnly _UnSelectedHover As Color() = New Color() {Color.FromArgb(235, 244, 253), Color.FromArgb(221, 236, 253), Color.FromArgb(209, 230, 253), Color.FromArgb(194, 220, 253), Color.White}
    ReadOnly _UnSelectedPressed As Color() = New Color() {Color.FromArgb(171, 210, 242), Color.FromArgb(194, 220, 253), Color.FromArgb(189, 210, 233), Color.FromArgb(194, 220, 253), Color.White}
    ReadOnly _UnSelectedBorder As SolidBrush = New SolidBrush(Color.FromArgb(125, 162, 206))
    ReadOnly _DisabledBorder As SolidBrush = New SolidBrush(Color.White)
    ReadOnly _DisabledAllColor As Color() = New Color() {Color.White, Color.White, Color.White, Color.White, Color.White}

#End Region

#Region "Public ReadOnly Property Interface"

    Public ReadOnly Property SelectedNormal As Color()
        Get
            Return _SelectedNormal
        End Get
    End Property
    Public ReadOnly Property SelectedHover As Color()
        Get
            Return _SelectedHover
        End Get
    End Property
    Public ReadOnly Property SelectedPressed As Color()
        Get
            Return _SelectedPressed
        End Get
    End Property
    Public ReadOnly Property SelectedBorder As SolidBrush
        Get
            Return _SelectedBorder
        End Get
    End Property
    Public ReadOnly Property UnSelectedNormal As Color()
        Get
            Return _UnSelectedNormal
        End Get
    End Property
    Public ReadOnly Property UnSelectedHover As Color()
        Get
            Return _UnSelectedHover
        End Get
    End Property
    Public ReadOnly Property UnSelectedPressed As Color()
        Get
            Return _UnSelectedPressed
        End Get
    End Property
    Public ReadOnly Property UnSelectedBorder As SolidBrush
        Get
            Return _UnSelectedBorder
        End Get
    End Property
    Public ReadOnly Property DisabledBorder As SolidBrush
        Get
            Return _DisabledBorder
        End Get
    End Property
    Public ReadOnly Property DisabledAllColor As Color()
        Get
            Return _DisabledAllColor
        End Get
    End Property
#End Region

End Module
