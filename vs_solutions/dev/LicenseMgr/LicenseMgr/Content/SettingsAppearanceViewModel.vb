#Region "Microsoft.VisualBasic::340ec83c9b335e42c3dfb73e45ead265, sciBASIC#\vs_solutions\dev\LicenseMgr\LicenseMgr\Content\SettingsAppearanceViewModel.vb"

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

    '   Total Lines: 210
    '    Code Lines: 140
    ' Comment Lines: 43
    '   Blank Lines: 27
    '     File Size: 8.22 KB


    '     Class SettingsAppearanceViewModel
    ' 
    '         Properties: AccentColors, FontSizes, Palettes, SelectedAccentColor, SelectedFontSize
    '                     SelectedPalette, SelectedTheme, Themes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: OnAppearanceManagerPropertyChanged, SyncThemeAndColor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports FirstFloor.ModernUI.Presentation
Imports System.ComponentModel
Imports System.Linq
Imports System.Windows.Media

Namespace Content
    ''' <summary>
    ''' A simple view model for configuring theme, font and accent colors.
    ''' </summary>
    Public Class SettingsAppearanceViewModel
        Inherits NotifyPropertyChanged
        Private Const FontSmall As String = "small"
        Private Const FontLarge As String = "large"

        Private Const PaletteMetro As String = "metro"
        Private Const PaletteWP As String = "windows phone"
        Private Const PaletteCustom As String = "custom"

        ' 9 accent colors from metro design principles
        ' blue
        ' teal
        ' green
        ' lime
        ' orange
        ' orange red
        ' red
        ' magenta
        ' purple
        Private metroAccentColors As Color() = New Color() {Color.FromRgb(&H33, &H99, &HFF), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H33, &H99, &H33), Color.FromRgb(&H8C, &HBF, &H26), Color.FromRgb(&HF0, &H96, &H9), Color.FromRgb(&HFF, &H45, &H0),
            Color.FromRgb(&HE5, &H14, &H0), Color.FromRgb(&HFF, &H0, &H97), Color.FromRgb(&HA2, &H0, &HFF)}

        ' 20 accent colors from Windows Phone 8
        ' lime
        ' green
        ' emerald
        ' teal
        ' cyan
        ' cobalt
        ' indigo
        ' violet
        ' pink
        ' magenta
        ' crimson
        ' red
        ' orange
        ' amber
        ' yellow
        ' brown
        ' olive
        ' steel
        ' mauve
        ' taupe
        Private wpAccentColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF),
            Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0),
            Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87),
            Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}

        ' Custom accent colors
        ' white
        ' black
        Private customAccentColors As Color() = New Color() {Color.FromRgb(&HFF, &HFF, &HFF), Color.FromRgb(&H0, &H0, &H0)}

        Private m_selectedPalette As String = PaletteWP

        Private m_selectedAccentColor As Color
        Private m_themes As New LinkCollection()
        Private m_selectedTheme As Link
        Private m_selectedFontSize As String

        Public Sub New()
            ' add the default themes
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "dark",
                 .Source = AppearanceManager.DarkThemeSource
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "light",
                 .Source = AppearanceManager.LightThemeSource
            })

            ' add additional themes
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "bing image",
                 .Source = New Uri("/Assets/ModernUI.BingImage.xaml", UriKind.Relative)
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "hello kitty",
                 .Source = New Uri("/Assets/ModernUI.HelloKitty.xaml", UriKind.Relative)
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "love",
                 .Source = New Uri("/Assets/ModernUI.Love.xaml", UriKind.Relative)
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "snowflakes",
                 .Source = New Uri("/Assets/ModernUI.Snowflakes.xaml", UriKind.Relative)
            })

            ' add custom themes
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "windows 8.1",
                 .Source = New Uri("/Assets/ModernUI.Windows8.1.xaml", UriKind.Relative)
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "metro",
                 .Source = New Uri("/Assets/ModernUI.Metro.xaml", UriKind.Relative)
            })

            Me.SelectedFontSize = If(AppearanceManager.Current.FontSize = FontSize.Large, FontLarge, FontSmall)
            SyncThemeAndColor()

            AddHandler AppearanceManager.Current.PropertyChanged, AddressOf OnAppearanceManagerPropertyChanged
        End Sub

        Private Sub SyncThemeAndColor()
            ' synchronizes the selected viewmodel theme with the actual theme used by the appearance manager.
            Me.SelectedTheme = Me.m_themes.FirstOrDefault(Function(l) l.Source.Equals(AppearanceManager.Current.ThemeSource))

            ' and make sure accent color is up-to-date
            Me.SelectedAccentColor = AppearanceManager.Current.AccentColor
        End Sub

        Private Sub OnAppearanceManagerPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
            If e.PropertyName = "ThemeSource" OrElse e.PropertyName = "AccentColor" Then
                SyncThemeAndColor()
            End If
        End Sub

        Public ReadOnly Property Themes() As LinkCollection
            Get
                Return Me.m_themes
            End Get
        End Property

        Public ReadOnly Property FontSizes() As String()
            Get
                Return New String() {FontSmall, FontLarge}
            End Get
        End Property

        Public ReadOnly Property Palettes() As String()
            Get
                Return New String() {PaletteMetro, PaletteWP, PaletteCustom}
            End Get
        End Property

        Public ReadOnly Property AccentColors() As Color()
            Get
                Return If(Me.m_selectedPalette = PaletteMetro, Me.metroAccentColors, If(Me.m_selectedPalette = PaletteWP, Me.wpAccentColors, Me.customAccentColors))
            End Get
        End Property

        Public Property SelectedPalette() As String
            Get
                Return Me.m_selectedPalette
            End Get
            Set
                If Me.m_selectedPalette <> Value Then
                    Me.m_selectedPalette = Value
                    OnPropertyChanged("AccentColors")

                    Me.SelectedAccentColor = Me.AccentColors.FirstOrDefault()
                End If
            End Set
        End Property

        Public Property SelectedTheme() As Link
            Get
                Return Me.m_selectedTheme
            End Get
            Set
                If Me.m_selectedTheme IsNot Value Then
                    Me.m_selectedTheme = Value
                    OnPropertyChanged("SelectedTheme")

                    ' and update the actual theme
                    AppearanceManager.Current.ThemeSource = Value.Source
                End If
            End Set
        End Property

        Public Property SelectedFontSize() As String
            Get
                Return Me.m_selectedFontSize
            End Get
            Set
                If Me.m_selectedFontSize <> Value Then
                    Me.m_selectedFontSize = Value
                    OnPropertyChanged("SelectedFontSize")

                    AppearanceManager.Current.FontSize = If(Value = FontLarge, FontSize.Large, FontSize.Small)
                End If
            End Set
        End Property

        Public Property SelectedAccentColor() As Color
            Get
                Return Me.m_selectedAccentColor
            End Get
            Set
                If Me.m_selectedAccentColor <> Value Then
                    Me.m_selectedAccentColor = Value
                    OnPropertyChanged("SelectedAccentColor")

                    AppearanceManager.Current.AccentColor = Value
                End If
            End Set
        End Property
    End Class
End Namespace
