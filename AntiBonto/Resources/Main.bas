Attribute VB_Name = "Main"
Option Explicit

Sub GenerateEverything()
Attribute GenerateEverything.VB_ProcData.VB_Invoke_Func = "O\n14"
    Call GenerateBadges
    Call GenerateSharingGroups
    Call GenerateSleepingGroups
    Call GenerateHandout
End Sub

Function Participants() As Person()
    Dim data As Worksheet: Set data = Sheets("Alapadatok")
    Dim numParticipants As Integer
    data.Unprotect
    numParticipants = data.Cells(1, 1).CurrentRegion.Rows.Count - 1
    data.Protect
    
    ReDim ret(numParticipants - 1) As Person
    
    Dim i As Integer
    For i = 2 To numParticipants + 1
        Set ret(i - 2) = New Person
        Call ret(i - 2).Init(data, i)
    Next i
    
    Participants = ret
End Function

Function GetNumParticipants() As Integer
    Dim data As Worksheet: Set data = Sheets("Alapadatok")
    Dim numParticipants As Integer
    
    data.Unprotect
    GetNumParticipants = data.Cells(1, 1).CurrentRegion.Rows.Count - 1
    data.Protect
End Function

Sub GenerateBadges()
Attribute GenerateBadges.VB_ProcData.VB_Invoke_Func = "K\n14"
'
' Ctrl+Shift+K
'
    If WorksheetExists("Kit�z�1") Then
      Exit Sub
    End If
    
    Const BADGES_PER_PAGE = 10
    
    Dim ppl() As Person: ppl = Participants()
    Dim numParticipants As Integer: numParticipants = ArrayLen(ppl)
    
    Dim numBadgePages As Integer
    numBadgePages = WorksheetFunction.RoundUp(numParticipants / BADGES_PER_PAGE, 0)
    
    Dim i As Integer: i = 0 ' index of current participant
    Dim page As Integer
    For page = 1 To numBadgePages
      Sheets("Kit�z�_alap").Copy After:=Sheets(Sheets.Count)
      ActiveSheet.Name = "Kit�z�" & page
      ActiveSheet.Unprotect
      
      Dim j As Integer    ' badge row index in rows of 2 badges
      For j = 1 To BADGES_PER_PAGE / 2
        Dim row As Integer: row = (j - 1) * 5 + 1 ' index of first Excel row within current badge
            
        ' Generate first badge in row
        Cells(row, 1).Value = ppl(i).FirstName
        Cells(row + 1, 1).Value = " " + ppl(i).LastName
        Cells(row + 3, 1).Value = " " & ppl(i).SharingGroup & "   " & ppl(i).SleepingGroup
        i = i + 1
        
        ' Generate last badge in row
        Cells(row, 4).Value = ppl(i).FirstName
        Cells(row + 1, 4).Value = " " + ppl(i).LastName
        Cells(row + 3, 4).Value = " " & ppl(i).SharingGroup & "   " & ppl(i).SleepingGroup
        i = i + 1
        
        If i >= numParticipants Then
            Exit For
        End If
      Next j
    Next page
End Sub

Sub GenerateHandout()
Attribute GenerateHandout.VB_ProcData.VB_Invoke_Func = "Z\n14"
'
' Generates a page you can handout to the closing ceremony participants
'
' Ctrl+Shift+Z
'

If WorksheetExists("Z�r� el�lap") Then
  Exit Sub
End If

Dim groupPropertiesSheet As Worksheet:      Set groupPropertiesSheet = Sheets("Alv�csoport c�mek")
Dim data As Worksheet:                      Set data = Sheets("Alapadatok")
Dim numParticipants As Integer: numParticipants = GetNumParticipants()

data.Unprotect
data.Range(data.Cells(2, 1), data.Cells(numParticipants, 8)).Sort _
    Key1:=data.Cells(2, 1), _
    Order1:=xlAscending, _
    Key2:=data.Cells(2, 2), _
    Order2:=xlAscending, _
    Key3:=data.Cells(2, 3), _
    Order3:=xlAscending, _
    Header:=xlGuess, _
    OrderCustom:=1, _
    MatchCase:=False, _
    Orientation:=xlTopToBottom, _
    DataOption1:=xlSortNormal, _
    DataOption2:=xlSortNormal, _
    DataOption3:=xlSortNormal
data.Protect

Sheets("Z�r�_el�lap_alap").Copy After:=Sheets(Sheets.Count)
ActiveSheet.Name = "Z�r� el�lap"
ActiveSheet.Unprotect

Dim weekend As WeekendProperties: weekend = GetWeekendProperties()
Cells(1, 6) = Str(weekend.Number) & ". " & weekend.CommunityName & " Anti�chia-h�tv�ge, "
Cells(2, 6) = weekend.Date
Cells(3, 6) = weekend.Address
Cells(33, 2).Value = weekend.MarriedCouple

Dim ppl() As Person: ppl = Participants()
Dim teamCount As Integer: teamCount = 0
Dim var As Variant
Dim pers As Person

For Each var In ppl
    Set pers = var
    If pers.Kind <> ptNewcomer And pers.Kind <> ptOtherParticipant Then
        teamCount = teamCount + 1
    End If
Next

Dim rowsPerCol As Integer: rowsPerCol = WorksheetFunction.RoundUp(teamCount / 3, 0)
Dim musicTeamIndex As Integer: musicTeamIndex = 0
Dim teamIndex As Integer: teamIndex = 0

Dim musicTeamRow As Integer, musicTeamCol As Integer
Dim teamRow As Integer, teamCol As Integer

Dim girlLeader As Person
Dim boyLeader As Person

For Each var In ppl
    Set pers = var
    Select Case pers.Kind
        Case ptBoyLeader
            Set boyLeader = pers
        Case ptGirlLeader
            Set girlLeader = pers
        Case ptMusicLeader, ptMusicTeam
            musicTeamRow = 27 + Int(musicTeamIndex / 3)
            musicTeamCol = 2 + (musicTeamIndex Mod 3)
            Cells(musicTeamRow, musicTeamCol).Value = pers.FirstName & " " & pers.LastName
            If pers.Kind = ptMusicLeader Then
              Cells(musicTeamRow, musicTeamCol).Font.Underline = xlUnderlineStyleSingle
            End If
            musicTeamIndex = musicTeamIndex + 1
    End Select
    If pers.Kind <> ptNewcomer And pers.Kind <> ptOtherParticipant Then ' team
        teamRow = 9 + teamIndex Mod rowsPerCol
        teamCol = 2 + Int(teamIndex / rowsPerCol)
        Cells(teamRow, teamCol).Value = pers.FirstName & " " & pers.LastName
        teamIndex = teamIndex + 1
    End If
Next

Cells(6, 2).Value = girlLeader.FirstName & " " & girlLeader.LastName & " & " & boyLeader.FirstName & " " & boyLeader.LastName

End Sub

Sub DeleteAllGeneratedWorksheets()
Attribute DeleteAllGeneratedWorksheets.VB_ProcData.VB_Invoke_Func = "T\n14"
'
' Ctrl+Shift+T
'
  Dim i As Integer
  Application.DisplayAlerts = False
  For i = Sheets.Count To 10 Step (-1)
    Sheets(i).Delete
  Next
  Application.DisplayAlerts = True
End Sub

