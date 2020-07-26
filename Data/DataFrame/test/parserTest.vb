#Region "Microsoft.VisualBasic::5a4a1636783b89d8aeed2c4132bc5866, Data\DataFrame\test\parserTest.vb"

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

    ' Module parserTest
    ' 
    '     Sub: fileLoaderTest, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Serialization.JSON

Module parserTest

    Sub Main()

        Call fileLoaderTest()


        Call CharsParser(<string>A,"","","","NA"</string>).GetJson(True).__DEBUG_ECHO

        Call CharsParser(<string>45274,562.162767808,Vialinin A,"4,4"",5\',6\'-tetrahydroxy[1,1\':4\',1""-terphenyl]-2\',3\'-diyl ester benzeneacetic acid",C34H26O8,858134-23-3,,,,-1,,</string>).GetJson(True).__DEBUG_ECHO



        Call CharsParser(<string>"Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"</string>).GetJson(True).__INFO_ECHO

        Dim row$ = <string>"2","132.065324663072","132.065155029297","132.065490722656","74.1104","70.5456","83.6562","1931963.63903523","1931952.91218068","410498.25","410497","1","292","METLIN005733-40992_POS_0V_[M+H]+!N-acetyl-L-alanine!1!0.604!0.959!1.563!lxy-CID30.mzXML!266","MSMScheck","METLIN005742_POS_20V_[M+H]+!N-Acetylglycine methyl ester!1!0.778!0.775!1.553!lxy-CID30.mzXML!266","MSMScomfirmed","L16690_[M+H]+!5-Aminolevulinate!1!0.94!0.415!1.355!lxy-CID30.mzXML!266","MSMScheck","N-Acetylglycine methyl ester","131.058244","C5H9NO3","","HMDB00776",NA,NA,NA,"","[M+H]+","132.0655",NA,"20 V",NA,NA,NA,NA,NA,"METLIN005742_POS_20V_[M+H]+","METLIN005742_POS_20V_[M+H]+!N-Acetylglycine methyl ester!1!0.778!0.775!1.553!lxy-CID30.mzXML!266","MSMScomfirmed","NA"</string>
        Dim columns = Tokenizer.CharsParser(row)

        ' Call columns .SaveTo ("D:\ddd.csv")

        Call columns.GetJson(True).__DEBUG_ECHO

        Pause()
    End Sub

    Sub fileLoaderTest()
        Dim table As File = File.Load("D:\GCModeller\src\runtime\sciBASIC#\Data\data\outlining.csv")

        Pause()
    End Sub
End Module
