#Region "Microsoft.VisualBasic::905a686d5cbabf6bd96a04b13ef9d513, Data\BinaryData\HDSPack\test\feather_df.vb"

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

    '   Total Lines: 25
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 841 B


    ' Module feather_df
    ' 
    '     Sub: Main222222, readTest, writeTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataStorage.FeatherFormat
Imports Microsoft.VisualBasic.Math.Quantile

Module feather_df

    Sub Main222222()
        Call writeTest()
    End Sub

    Sub writeTest()
        Using writer As New FeatherWriter("./aaa.fea1", WriteMode.Eager)
            writer.AddColumn("row.names", {"1", "2", "3"})
            writer.AddColumn("x", {2, 4, 8})
            writer.AddColumn("y", {3.1, 6.2, 9.3})
        End Using
    End Sub

    Sub readTest()
        Using untyped = FeatherReader.ReadFromFile("\GCModeller\src\runtime\sciBASIC#\Data\BinaryData\Feather\examples\r-feather-test.feather", BasisType.Zero)
            Dim typed = untyped.Map(Of Boolean, Integer, Double, String, DateTimeOffset, TimeSpan, DateTime, String)()

            Pause()
        End Using
    End Sub
End Module
