#Region "Microsoft.VisualBasic::81a2cdb2c2f9f8efcd71e11f50a706ec, ..\visualbasic_App\UXFramework\Molk+\TestProject\Form4.vb"

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

Public Class Form4
    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Me.ProcessingBar1.StartRollAnimation()
        '  Me.ProcessingBar1.PercentageValue = 60


        Call New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.MarginBlue().SetupResource(Me.MultipleTabpagePanel1)
        Call Me.MultipleTabpagePanel1.AddTabPage("查找好友", New Panel With {.BackColor = Color.Aqua})
        Call Me.MultipleTabpagePanel1.AddTabPage("好友请求列表", New Panel With {.BackColor = Color.DarkGoldenrod})
        Call Me.MultipleTabpagePanel1.UpdatesUILayout()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Me.ProcessingBar1.PercentageValue += 1
    End Sub
End Class
