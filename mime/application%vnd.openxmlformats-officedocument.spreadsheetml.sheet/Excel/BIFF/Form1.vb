VERSION 5.00
Begin VB.Form MainForm 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "Excel Class Demo"
   ClientHeight    =   6720
   ClientLeft      =   45
   ClientTop       =   330
   ClientWidth     =   6345
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   6720
   ScaleWidth      =   6345
   StartUpPosition =   2  'CenterScreen
   Begin VB.CommandButton cmdCancel 
      Caption         =   "Cancel"
      Height          =   495
      Left            =   3015
      TabIndex        =   2
      Top             =   5415
      Width           =   1215
   End
   Begin VB.CommandButton cmdCreate 
      Caption         =   "Create"
      Height          =   495
      Left            =   1575
      TabIndex        =   1
      Top             =   5415
      Width           =   1215
   End
   Begin VB.Label Label15 
      BackStyle       =   0  'Transparent
      Caption         =   "(11) Abiltiy to set the height of individual rows."
      Height          =   390
      Left            =   630
      TabIndex        =   16
      Top             =   4800
      Width           =   5280
   End
   Begin VB.Label Label14 
      BackStyle       =   0  'Transparent
      Caption         =   "(10) Default (global) row heights that affect the entire spreadsheet."
      Height          =   465
      Left            =   630
      TabIndex        =   15
      Top             =   4530
      Width           =   5280
   End
   Begin VB.Label Label13 
      BackStyle       =   0  'Transparent
      Caption         =   "(9) Horizontal Page Breaks"
      Height          =   465
      Left            =   645
      TabIndex        =   14
      Top             =   4260
      Width           =   5280
   End
   Begin VB.Label Label12 
      BackStyle       =   0  'Transparent
      Caption         =   "(8) The spreadsheet can be password protected so all contents are encrypted."
      Height          =   465
      Left            =   645
      TabIndex        =   13
      Top             =   3825
      Width           =   5280
   End
   Begin VB.Label Label11 
      BackStyle       =   0  'Transparent
      Caption         =   "(7) You can specify whether to print GridLines."
      Height          =   330
      Left            =   645
      TabIndex        =   12
      Top             =   3555
      Width           =   3750
   End
   Begin VB.Label Label10 
      BackStyle       =   0  'Transparent
      Caption         =   "(6) You can specify Headers and Footers to appear on each printed page."
      Height          =   330
      Left            =   645
      TabIndex        =   11
      Top             =   3285
      Width           =   5415
   End
   Begin VB.Label Label3 
      BackStyle       =   0  'Transparent
      Caption         =   "(1) Set the spreadsheet margins in inches."
      Height          =   375
      Left            =   645
      TabIndex        =   10
      Top             =   1560
      Width           =   4515
   End
   Begin VB.Label Label4 
      BackStyle       =   0  'Transparent
      Caption         =   "(2) Set individual or a range of column widths."
      Height          =   375
      Left            =   645
      TabIndex        =   9
      Top             =   1830
      Width           =   4830
   End
   Begin VB.Label Label5 
      BackStyle       =   0  'Transparent
      Caption         =   $"Form1.frx":0000
      Height          =   690
      Left            =   645
      TabIndex        =   8
      Top             =   2100
      Width           =   5235
   End
   Begin VB.Label Label6 
      BackStyle       =   0  'Transparent
      Caption         =   "(4) You can specify the font, alignment and formatting for individual cells."
      Height          =   465
      Left            =   645
      TabIndex        =   7
      Top             =   2745
      Width           =   5235
   End
   Begin VB.Label Label8 
      Caption         =   "(5) Handles cell borders, shading, locking and hiding."
      Height          =   375
      Left            =   645
      TabIndex        =   6
      Top             =   3015
      Width           =   5145
   End
   Begin VB.Label Label9 
      Alignment       =   1  'Right Justify
      Caption         =   "rambo2000@canada.com"
      Height          =   195
      Left            =   4215
      TabIndex        =   5
      Top             =   6420
      Width           =   1995
   End
   Begin VB.Label Label7 
      Caption         =   "Paul Squires November 10, 2001"
      Height          =   195
      Left            =   105
      TabIndex        =   4
      Top             =   6420
      Width           =   2775
   End
   Begin VB.Label Label2 
      Caption         =   "The Excel class is able to:"
      Height          =   375
      Left            =   150
      TabIndex        =   3
      Top             =   1185
      Width           =   5640
   End
   Begin VB.Label Label1 
      Caption         =   $"Form1.frx":00A5
      Height          =   870
      Left            =   150
      TabIndex        =   0
      Top             =   135
      Width           =   6045
   End
End
Attribute VB_Name = "MainForm"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False



Private Sub cmdCreate_Click()

Dim myExcelFile As New ExcelFile

With myExcelFile
    'Create the new spreadsheet
    FileName$ = ".\vbtest.xls"  'create spreadsheet in the current directory
    .CreateFile FileName$
    
    'set a Password for the file. If set, the rest of the spreadsheet will
    'be encrypted. If a password is used it must immediately follow the
    'CreateFile method.
    'This is different then protecting the spreadsheet (see below).
    'NOTE: For some reason this function does not work. Excel will
    'recognize that the file is password protected, but entering the password
    'will not work. Also, the file is not encrypted. Therefore, do not use
    'this function until I can figure out why it doesn't work. There is not
    'much documentation on this function available.
    '.SetFilePassword "PAUL"
    
    
    
    'specify whether to print the gridlines or not
    'this should come before the setting of fonts and margins
    .PrintGridLines = False
    
    
    'it is a good idea to set margins, fonts and column widths
    'prior to writing any text/numerics to the spreadsheet. These
    'should come before setting the fonts.
    
    .SetMargin xlsTopMargin, 1.5   'set to 1.5 inches
    .SetMargin xlsLeftMargin, 1.5
    .SetMargin xlsRightMargin, 1.5
    .SetMargin xlsBottomMargin, 1.5
    
    
    'to insert a Horizontal Page Break you need to specify the row just
    'after where you want the page break to occur. You can insert as many
    'page breaks as you wish (in any order).
    .InsertHorizPageBreak 10
    .InsertHorizPageBreak 20
    
    'set a default row height for the entire spreadsheet (1/20th of a point)
    .SetDefaultRowHeight 14
    
    
    'Up to 4 fonts can be specified for the spreadsheet. This is a
    'limitation of the Excel 2.1 format. For each value written to the
    'spreadsheet you can specify which font to use.
    
    .SetFont "Arial", 10, xlsNoFormat              'font0
    .SetFont "Arial", 10, xlsBold                  'font1
    .SetFont "Arial", 10, xlsBold + xlsUnderline   'font2
    .SetFont "Courier", 16, xlsBold + xlsItalic            'font3
    
    
    'Column widths are specified in Excel as 1/256th of a character.
    .SetColumnWidth 1, 5, 18
    
    'Set special row heights for row 1 and 2
    .SetRowHeight 1, 30
    .SetRowHeight 2, 30
        
    
    'set any header or footer that you want to print on
    'every page. This text will be centered at the top and/or
    'bottom of each page. The font will always be the font that
    'is specified as font0, therefore you should only set the
    'header/footer after specifying the fonts through SetFont.
    .SetHeader "BIFF 2.1 API"
    .SetFooter "Paul Squires - Excel BIFF Class"
    
    'write a normal left aligned string using font3 (Courier Italic)
    .WriteValue xlsText, xlsFont3, xlsLeftAlign, xlsNormal, 1, 1, "Quarterly Report"
    .WriteValue xlsText, xlsFont1, xlsLeftAlign, xlsNormal, 2, 1, "Cool Guy Corporation"
    
    'write some data to the spreadsheet
    'Use the default format #3 "#,##0" (refer to the WriteDefaultFormats function)
    'The WriteDefaultFormats function is compliments of Dieter Hauk in Germany.
    .WriteValue xlsinteger, xlsFont0, xlsLeftAlign, xlsNormal, 6, 1, 2000, 3
    
   
    'write a cell with a shaded number with a bottom border
    .WriteValue xlsnumber, xlsFont1, xlsrightAlign + xlsBottomBorder + xlsShaded, xlsNormal, 7, 1, 12123.456, 4
    
    'write a normal left aligned string using font2 (bold & underline)
    .WriteValue xlsText, xlsFont2, xlsLeftAlign, xlsNormal, 8, 1, "This is a test string"
    
    'write a locked cell. The cell will not be able to be overwritten, BUT you
    'must set the sheet PROTECTION to on before it will take effect!!!
    .WriteValue xlsText, xlsFont3, xlsLeftAlign, xlsLocked, 9, 1, "This cell is locked"
    
    'fill the cell with "F"'s
    .WriteValue xlsText, xlsFont0, xlsFillCell, xlsNormal, 10, 1, "F"
    
    'write a hidden cell to the spreadsheet. This only works for cells
    'that contain formulae. Text, Number, Integer value text can not be hidden
    'using this feature. It is included here for the sake of completeness.
    .WriteValue xlsText, xlsFont0, xlsCentreAlign, xlsHidden, 11, 1, "If this were a formula it would be hidden!"
    
    
    'write some dates to the file. NOTE: you need to write dates as xlsNumber
     Dim d As Date
     d = "15/01/2001"
    .WriteValue xlsnumber, xlsFont0, xlsCentreAlign, xlsNormal, 15, 1, d, 12

     d = "31/12/1999"
    .WriteValue xlsnumber, xlsFont0, xlsCentreAlign, xlsNormal, 16, 1, d, 12

     d = "01/04/2002"
    .WriteValue xlsnumber, xlsFont0, xlsCentreAlign, xlsNormal, 17, 1, d, 12
     
     d = "21/10/1998"
    .WriteValue xlsnumber, xlsFont0, xlsCentreAlign, xlsNormal, 18, 1, d, 12
    
    'PROTECT the spreadsheet so any cells specified as LOCKED will not be
    'overwritten. Also, all cells with HIDDEN set will hide their formulae.
    'PROTECT does not use a password.
    .ProtectSpreadsheet = True
    
    
    'Finally, close the spreadsheet
    .CloseFile
    
    MsgBox "Excel BIFF Spreadsheet created." & vbCrLf & "Filename: " & FileName$, vbInformation + vbOKOnly, "Excel Class"
    
End With


End Sub



Private Sub cmdCancel_Click()
    Unload Me
End Sub

Private Sub Form_Load()

    ChDir App.Path
    
End Sub

