#Region "Microsoft.VisualBasic::5e9b8b00c2da0f55498d02474d45a1a6, ..\visualbasic_App\UXFramework\Molk+\Molk+\Elements\MultipleTabPage.vb"

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

Namespace Visualise.Elements

    Public Class MultipleTabPage

        Public Property LabelHeight As Integer
        Public Property UIResource As ButtonResource
        Public Property Font As Font
        Public Property SeperatorBarResource As Image

        Public Overridable Sub SetupResource(ByRef ctrl As MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel)
            ctrl.Font = Font
            ctrl.UIResource = UIResource
            ctrl.Separable.BackgroundImage = SeperatorBarResource
        End Sub

        Public Shared ReadOnly Property MolkPlusTheme As MultipleTabPage
            Get
                Dim Theme = New MultipleTabPage With {.LabelHeight = 40, .Font = New Font(YaHei, 9, FontStyle.Regular)}
                Theme.UIResource = New ButtonResource With {.Active = My.Resources.Active, .InSensitive = My.Resources.Inactive, .Normal = My.Resources.Inactive, .PreLight = My.Resources.InactivePrelight}
                Return Theme
            End Get
        End Property
    End Class
End Namespace
