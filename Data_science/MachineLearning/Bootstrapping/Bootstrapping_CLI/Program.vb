#Region "Microsoft.VisualBasic::b2740711792f583fed6a5795dd5cfca1, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Bootstrapping_CLI\Program.vb"

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

    '   Total Lines: 66
    '    Code Lines: 55
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 3.07 KB


    ' Module Program
    ' 
    '     Function: BootstrappingExport, BootstrappingExportBinary, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.EigenvectorBootstrapping
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Text

Module Program

    Public Function Main() As Integer
        Call Testing.Run()

        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/Build.Zone", Usage:="/Build.Zone /imports <odes_out.DIR> [/part.N 10 /cluster.N 10 /out <outDIR> /max.it -1]")>
    Public Function BootstrappingExport(args As CommandLine) As Integer
        Dim [in] As String = args("/imports")
        Dim partN As Integer = args.GetValue("/part.N", 10)
        Dim clusterN As Integer = args.GetValue("/cluster.N", 10)
        Dim maxIt As Integer = args.GetValue("/max.it", -1)
        Dim binary As Boolean = args.GetBoolean("/binary")
        Dim vec = DefaultEigenvector([in])
        Dim out = [in].LoadData(vec, partN).KMeans(clusterN, [stop]:=maxIt)
        Dim EXPORT As String = args.GetValue(
            "/out",
            [in].TrimDIR & $".partN={partN},.clusterN={clusterN}{If(binary, "-binaryTree", "")}/")
        Dim uid As New Uid(False)

        For Each cluster In out
            Dim Eigenvector As ODEsOut = cluster.Key.GetSample(vec, partN)
            Dim DIR As String = EXPORT & "/" & FormatZero(uid.Plus, "0000")

            Call Eigenvector.DataFrame.Save(DIR & "/Eigenvector.Sample.csv", Encodings.ASCII)
            Call cluster.Value.Select(Function(x) New With {.params = x}).ToArray.SaveTo(DIR & "/Eigenvector.paramZone.csv")
        Next

        Return 0
    End Function

    <ExportAPI("/Build.Zone.Binary", Usage:="/Build.Zone.Binary /imports <odes_out.DIR> [/depth 4 /part.N 10 /out <outDIR> /max.it -1]")>
    Public Function BootstrappingExportBinary(args As CommandLine) As Integer
        Dim [in] As String = args("/imports")
        Dim partN As Integer = args.GetValue("/part.N", 10)
        Dim vec = DefaultEigenvector([in])
        Dim depth As Integer = args.GetValue("/depth", 4)
        Dim maxIt As Integer = args.GetValue("/max.it", -1)
        Dim out = [in].LoadData(vec, partN).BinaryKMeans(depth, [stop]:=maxIt)
        Dim EXPORT As String = args.GetValue(
            "/out",
            [in].TrimDIR & $".partN={partN}-binaryTree/")

        For Each cluster In out
            Dim Eigenvector As ODEsOut = cluster.Key.Value.GetSample(vec, partN)
            Dim DIR As String = EXPORT & "/" & cluster.Key.Name

            Call Eigenvector.DataFrame.Save(DIR & "/Eigenvector.Sample.csv", Encodings.ASCII)
            Call cluster.Value.Select(Function(x) New With {.params = x}).ToArray.SaveTo(DIR & "/Eigenvector.paramZone.csv")
            Call cluster.Key.Name.SaveTo(DIR & "/branch.txt")
        Next

        Return 0
    End Function
End Module
