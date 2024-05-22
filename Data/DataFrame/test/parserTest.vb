#Region "Microsoft.VisualBasic::6ef3282c031882bafed72a0a9e396c87, Data\DataFrame\test\parserTest.vb"

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

    '   Total Lines: 49
    '    Code Lines: 26 (53.06%)
    ' Comment Lines: 3 (6.12%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 20 (40.82%)
    '     File Size: 3.92 KB


    ' Module parserTest
    ' 
    '     Sub: fileLoaderTest, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Data.csv.IO.Tokenizer

Module parserTest

    Sub Main()

        Dim tokens1 As String() = Microsoft.VisualBasic.Data.csv.IO.Tokenizer.CharsParser("1,2,3,""4,5,6,7,8""").ToArray
        Dim tokens2 As String() = Microsoft.VisualBasic.Data.csv.IO.Tokenizer.CharsParser("1,2,3,""Hello, world!""").ToArray

        Call Console.WriteLine(tokens1.GetJson)
        Call Console.WriteLine(tokens2.GetJson)

        Pause()

        Call CharsParser(<string>"A,""c""",123</string>).JoinBy(vbCrLf).__DEBUG_ECHO

        Call CharsParser(<string>,NA,,75184857,HMDB0030806,87207,Petunin,KLRABYJGMPNMSA-UHFFFAOYSA-O,"InChI=1S/C28H32O17/c1-40-15-3-9(2-12(32)19(15)33)26-16(43-28-25(39)23(37)21(35)18(8-30)45-28)6-11-13(41-26)4-10(31)5-14(11)42-27-24(38)22(36)20(34)17(7-29)44-27/h2-6,17-18,20-25,27-30,34-39H,7-8H2,1H3,(H2-,31,32,33)/p+1",COC1=C(O)C(O)=CC(=C1)C1=[O+]C2=C(C=C1OC1OC(CO)C(O)C(O)C1O)C(OC1OC(CO)C(O)C(O)C1O)=CC(O)=C2,Organic compounds,Phenylpropanoids and polyketides,Flavonoids,Flavonoid glycosides,Aromatic heteropolycyclic compounds,"Petunin is found in alcoholic beverages. Petunin is isolated from muscat grapes (Vitis vinifera), muscadine grapes (Vitis rotundifolia) and other plants In probability theory, the Vysochanskij Petunin inequality gives a lower bound for the probability that a random variable with finite variance lies within a certain number of standard deviations of the variable's mean, or equivalently an upper bound for the probability that it lies further away. The sole restriction on the distribution is that it be unimodal and have finite variance. (This implies that it is a continuous probability distribution except at the mode, which may have a non-zero probability.) The theorem applies even to heavily skewed distributions and puts bounds on how much of the data is, or is not, ""in the middle""",BioDeep_02030806,,Petunin,641.17177463,641.17177463,C28H33O17,HMDB,,,Homo sapiens,create_by [HMDB]HMDB0030806; modify_by [metlin]87207,25846-73-5</string>).JoinBy(vbCrLf).__DEBUG_ECHO

        ' Pause()

        ' Call fileLoaderTest()
        Call CharsParser(",").JoinBy(vbCrLf).__DEBUG_ECHO

        Call CharsParser(<string>A,"","NA"</string>).JoinBy(vbCrLf).__DEBUG_ECHO

        Call CharsParser(<string>45274,562.162767808,Vialinin A,"4,4"",5\',6\'-tetrahydroxy[1,1\':4\',1""-terphenyl]-2\',3\'-diyl ester benzeneacetic acid",C34H26O8,858134-23-3,,,,-1,,</string>).JoinBy(vbCrLf).__DEBUG_ECHO



        Call CharsParser(<string>"Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"</string>).JoinBy(vbCrLf).__DEBUG_ECHO

        Dim row$ = <string>"2","132.065324663072","132.065155029297","132.065490722656","74.1104","70.5456","83.6562","1931963.63903523","1931952.91218068","410498.25","410497","1","292","METLIN005733-40992_POS_0V_[M+H]+!N-acetyl-L-alanine!1!0.604!0.959!1.563!lxy-CID30.mzXML!266","MSMScheck","METLIN005742_POS_20V_[M+H]+!N-Acetylglycine methyl ester!1!0.778!0.775!1.553!lxy-CID30.mzXML!266","MSMScomfirmed","L16690_[M+H]+!5-Aminolevulinate!1!0.94!0.415!1.355!lxy-CID30.mzXML!266","MSMScheck","N-Acetylglycine methyl ester","131.058244","C5H9NO3","","HMDB00776",NA,NA,NA,"","[M+H]+","132.0655",NA,"20 V",NA,NA,NA,NA,NA,"METLIN005742_POS_20V_[M+H]+","METLIN005742_POS_20V_[M+H]+!N-Acetylglycine methyl ester!1!0.778!0.775!1.553!lxy-CID30.mzXML!266","MSMScomfirmed","NA"</string>
        Dim columns = Tokenizer.CharsParser(row).ToArray

        ' Call columns .SaveTo ("D:\ddd.csv")

        Call columns.JoinBy(vbCrLf).__DEBUG_ECHO

        Pause()
    End Sub

    Sub fileLoaderTest()
        Dim table As File = File.Load("D:\GCModeller\src\runtime\sciBASIC#\Data\data\outlining.csv")

        Pause()
    End Sub
End Module
