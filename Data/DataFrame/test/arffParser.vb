#Region "Microsoft.VisualBasic::8676e3f6153085af7f6b150342283e62, Data\DataFrame\test\arffParser.vb"

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

    '   Total Lines: 42
    '    Code Lines: 34 (80.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (19.05%)
    '     File Size: 1.71 KB


    ' Module arffParser
    ' 
    '     Sub: Main, readTest, writeTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.Framework
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module arffParser

    Sub Main()
        Call writeTest()
    End Sub

    Sub readTest()
        Dim df As DataFrame = DataFrame.read_arff("G:\GCModeller\src\runtime\sciBASIC#\Data\Example\dataframe\Smile.arff")
        Dim df2 As DataFrame = DataFrame.read_arff("G:\GCModeller\src\runtime\sciBASIC#\Data\Example\dataframe\COP.arff")
        Dim df3 As DataFrame = DataFrame.read_arff("G:\GCModeller\src\runtime\sciBASIC#\Data\Example\dataframe\Aggregation.arff")

        Pause()
    End Sub

    Sub writeTest()
        Dim df As New DataFrame With {
            .name = "file-test",
            .description = "text data writer in different vectro data type",
            .features = New Dictionary(Of String, FeatureVector) From {
                {"numbers", New FeatureVector("numbers", randf.ExponentialRandomNumbers(2, 6))},
                {"flags", New FeatureVector("flags", {True, True, True, False, False, True})},
                {"ints", New FeatureVector("ints", New Integer() {544, 53, 46, 4, 5, 99997})},
                {"string cols", New FeatureVector("string cols", {"as", "kjfsdha", "jdkh jkbvwucdxnjasdch", "uqweibcfweyf sdsadsadj", "fhjksacadslkdmas", "klx"})}
            },
            .rownames = {"# 1", "# 2", "# 3", "# 4", "# 5", "# 6"}
        }

        Dim text As New StringBuilder
        Dim writer As New StringWriter(text)

        Call DataFrame.write_arff(df, writer)
        Call writer.Flush()
        Call Console.WriteLine(text.ToString)

        Pause()
    End Sub
End Module

