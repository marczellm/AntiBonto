Attribute VB_Name = "Utils"
Option Explicit


Function StrEmpty(s As String) As Boolean
    StrEmpty = Len(s) = 0
End Function

Function WorksheetExists(SheetName As String) As Boolean
    Dim i As Integer
    WorksheetExists = False

    For i = 1 To Sheets.Count
      If Sheets(i).Name = SheetName Then
        WorksheetExists = True
      End If
    Next i
End Function

Public Function ArrayLen(arr As Variant) As Integer
    ArrayLen = UBound(arr) - LBound(arr) + 1
End Function

