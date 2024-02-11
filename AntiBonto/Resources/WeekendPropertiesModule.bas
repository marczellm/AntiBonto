Attribute VB_Name = "WeekendPropertiesModule"
Option Explicit

Type WeekendProperties
    CommunityName As String
    Number As Integer
    Date As String
    Location As String
    Address As String
End Type

Function GetWeekendProperties() As WeekendProperties
    Dim weekendPropertiesSheet As Worksheet: Set weekendPropertiesSheet = Sheets("Vezérlõ adatok")
    
    Dim strNum As String: strNum = weekendPropertiesSheet.Cells(2, 2).Value
    If Right$(strNum, 1) = "." Then
        strNum = Left(strNum, Len(strNum) - 1)
    End If
    
    GetWeekendProperties.CommunityName = weekendPropertiesSheet.Cells(1, 2).Value
    GetWeekendProperties.Number = CInt(strNum)
    GetWeekendProperties.Date = weekendPropertiesSheet.Cells(3, 2).Value
    GetWeekendProperties.Location = weekendPropertiesSheet.Cells(4, 2).Value
    GetWeekendProperties.Address = weekendPropertiesSheet.Cells(5, 2).Value
End Function

Sub SetupPrintHeaders(sheet As Worksheet, sheetTitle As String)
    Dim weekend As WeekendProperties: weekend = GetWeekendProperties()
    With sheet.PageSetup
        .CenterHeader = _
          "&""Constantia,Normál""&26" & sheetTitle & "&12" & Chr(10) & _
          "&14" & Str(weekend.Number) & ". " & weekend.CommunityName & " Antióchia-hétvége, " & weekend.Date & Chr(10) _
          & weekend.Location & Chr(10) _
          & weekend.Address & Chr(10) & ""
        .LeftHeader = ""
        .RightHeader = ""
        .LeftFooter = ""
        .CenterFooter = ""
        .RightFooter = ""
End With
End Sub


