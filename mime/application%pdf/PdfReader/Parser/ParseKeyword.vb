Imports System.ComponentModel

Namespace PdfReader
    Public Enum ParseKeyword
        <Description("endobj")>
        EndObj
        <Description("endstream")>
        EndStream
        <Description("false")>
        [False]
        <Description("null")>
        Null
        <Description("obj")>
        Obj
        <Description("R")>
        R
        <Description("startxref")>
        StartXRef
        <Description("stream")>
        Stream
        <Description("trailer")>
        Trailer
        <Description("true")>
        [True]
        <Description("xref")>
        XRef
    End Enum
End Namespace
