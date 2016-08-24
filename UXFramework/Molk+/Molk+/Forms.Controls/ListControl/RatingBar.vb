#Region "Microsoft.VisualBasic::c9c5aae796189ca9d64833d6b7f7cf44, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\RatingBar.vb"

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

Public Class RatingBar

#Region "Public Property Stars As Integer"

    Dim mStars As Integer = 3

    Public Property Stars As Integer
        Get
            Return mStars
        End Get
        Set( value As Integer)
            mStars = value
            SetupStars()
        End Set
    End Property
#End Region

    Private Sub SetupStars()
        Star1.Image = ImageList1.Images(IIf(mStars >= 1, "full", "empty"))
        Star2.Image = ImageList1.Images(IIf(mStars >= 2, "full", "empty"))
        Star3.Image = ImageList1.Images(IIf(mStars >= 3, "full", "empty"))
        Star4.Image = ImageList1.Images(IIf(mStars >= 4, "full", "empty"))
        Star5.Image = ImageList1.Images(IIf(mStars = 5, "full", "empty"))
    End Sub

#Region "Mouse Events Handler"

    Private Sub Star1_Click(sender As System.Object, e As System.EventArgs) Handles Star1.Click
        mStars = 1
        SetupStars()
    End Sub

    Private Sub Star2_Click(sender As System.Object, e As System.EventArgs) Handles Star2.Click
        mStars = 2
        SetupStars()
    End Sub

    Private Sub Star3_Click(sender As System.Object, e As System.EventArgs) Handles Star3.Click
        mStars = 3
        SetupStars()
    End Sub

    Private Sub Star4_Click(sender As System.Object, e As System.EventArgs) Handles Star4.Click
        mStars = 4
        SetupStars()
    End Sub

    Private Sub Star5_Click(sender As System.Object, e As System.EventArgs) Handles Star5.Click
        mStars = 5
        SetupStars()
    End Sub
#End Region
End Class
