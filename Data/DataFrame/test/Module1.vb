#Region "Microsoft.VisualBasic::484c9978e93184eb9df5b24fcfd8abcb, ..\sciBASIC#\Data\DataFrame\test\Module1.vb"

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

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()

        Call CharsParser(<string>A,"","","","NA"</string>).GetJson(True).__DEBUG_ECHO

        Call CharsParser(<string>"Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"</string>).GetJson(True).__INFO_ECHO

        Dim row$ = <string>"2","132.065324663072","132.065155029297","132.065490722656","74.1104","70.5456","83.6562","1931963.63903523","1931952.91218068","410498.25","410497","1","292","METLIN005733-40992_POS_0V_[M+H]+!N-acetyl-L-alanine!1!0.604!0.959!1.563!lxy-CID30.mzXML!266","MSMScheck","METLIN005742_POS_20V_[M+H]+!N-Acetylglycine methyl ester!1!0.778!0.775!1.553!lxy-CID30.mzXML!266","MSMScomfirmed","L16690_[M+H]+!5-Aminolevulinate!1!0.94!0.415!1.355!lxy-CID30.mzXML!266","MSMScheck","N-Acetylglycine methyl ester","131.058244","C5H9NO3","","HMDB00776",NA,NA,NA,"","[M+H]+","132.0655",NA,"20 V",NA,NA,NA,NA,NA,"METLIN005742_POS_20V_[M+H]+","METLIN005742_POS_20V_[M+H]+!N-Acetylglycine methyl ester!1!0.778!0.775!1.553!lxy-CID30.mzXML!266","MSMScomfirmed","NA"</string>
        Dim columns = Tokenizer.CharsParser(row)

        Call columns .SaveTo ("D:\ddd.csv")

        Call columns.GetJson(True).__DEBUG_ECHO

        Pause()
    End Sub
End Module

