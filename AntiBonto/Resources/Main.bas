Attribute VB_Name = "Main"
Option Explicit

Const GROUPS_PER_PAGE = 8
Const SLEEPING_GROUPS_PER_PAGE = 6

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
    If Not WorksheetExists("Kitûzõ1") Then
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
      Sheets("Kitûzõ_alap").Copy After:=Sheets(Sheets.Count)
      ActiveSheet.Name = "Kitûzõ" & page
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

Sub GenerateSharingGroups()
Attribute GenerateSharingGroups.VB_ProcData.VB_Invoke_Func = "M\n14"
'
' Ctrl+Shift+M
'

If Not WorksheetExists("Megosztócsoport1") Then
  Exit Sub
End If

Const GROUPS_PER_PAGE = 8
Dim data As Worksheet: Set data = Sheets("Alapadatok")
Dim numParticipants As Integer: numParticipants = GetNumParticipants()
Dim numSharingGroups As Integer: numSharingGroups = WorksheetFunction.Max(Range(data.Cells(2, 5), data.Cells(numParticipants + 1, 5)))
Dim numGroupPages As Integer: numGroupPages = WorksheetFunction.RoundUp(numSharingGroups / GROUPS_PER_PAGE, 0)

Call SetupPrintHeaders(Sheets("Megosztócsoport_alap"), "MEGOSZTÓ CSOPORTOK")

Dim i As Integer, j As Integer
For i = 1 To numGroupPages
  Sheets("Megosztócsoport_alap").Copy After:=Sheets(Sheets.Count)
  ActiveSheet.Name = "Megosztócsoport" & i
  ActiveSheet.Unprotect
  
  For j = 1 To GROUPS_PER_PAGE
    Dim sharingGroupIndex As Integer: sharingGroupIndex = (i - 1) * GROUPS_PER_PAGE + j
    
    If sharingGroupIndex <= numSharingGroups Then
      Call GenerateSharingGroup(data, numParticipants, sharingGroupIndex)
    End If
  Next j
Next i

End Sub

Sub GenerateSharingGroup(data As Worksheet, numParticipants As Integer, sharingGroupIndex As Integer)

Const MAX_GROUP_SIZE = 7

Dim k As Integer: k = 0  ' row index within group
Dim groupIndexWithinPage As Integer: groupIndexWithinPage = sharingGroupIndex Mod GROUPS_PER_PAGE
If groupIndexWithinPage = 0 Then
  groupIndexWithinPage = GROUPS_PER_PAGE
End If

Dim row As Integer: row = 1 + Int((groupIndexWithinPage - 1) / 2) * MAX_GROUP_SIZE
Dim col As Integer: col = 1 + ((groupIndexWithinPage - 1) Mod 2)
Dim ppl() As Person: ppl = Participants()
Dim var As Variant
Dim pers As Person
For Each var In ppl
    Set pers = var
    If pers.SharingGroup = sharingGroupIndex Then
        If pers.SharingGroupLeader Then
            Cells(row, col).Value = sharingGroupIndex & ". " & pers.FullName
        Else
            k = k + 1
            Cells(row + k, col).Value = pers.FullName
            If pers.Kind = ptNewcomer Then
                Cells(row + k, col).Font.Bold = True
            ElseIf pers.Kind = ptOtherParticipant Then
                Cells(row + k, col).Font.Underline = xlUnderlineStyleSingle
                Cells(row + k, col).Font.Italic = True
            End If
        End If
    End If
Next

' Sort list of group members
Range(Cells(row + 1, col), Cells(row + MAX_GROUP_SIZE - 1, col)).Select
    Selection.Sort _
        Key1:=Cells(row + 1, col), _
        Order1:=xlAscending, _
        Header:=xlGuess, _
        OrderCustom:=1, _
        MatchCase:=False, _
        Orientation:=xlTopToBottom, _
        DataOption1:=xlSortNormal

End Sub

Sub GenerateSleepingGroups()
Attribute GenerateSleepingGroups.VB_ProcData.VB_Invoke_Func = "A\n14"
'
' Ctrl+Shift+A
'

If Not WorksheetExists("Alvócsoport1") Then
  Exit Sub
End If

Dim data As Worksheet:          Set data = Sheets("Alapadatok")
Dim numParticipants As Integer: numParticipants = GetNumParticipants()

Dim numGroups As Integer: numGroups = 0
Dim i As Integer
For i = 2 To numParticipants + 1
  If Not IsEmpty(data.Cells(i, 7).Value) Then
    Dim numGroup As Integer: numGroup = Asc(data.Cells(i, 7).Value) - 64    ' convert letter to number
    If numGroup > numGroups Then
      numGroups = numGroup
    End If
  End If
Next i

Call SetupPrintHeaders(Sheets("Alvócsoport_alap"), "ALVÓCSOPORTOK")

Dim numGroupPages As Integer: numGroupPages = WorksheetFunction.RoundUp(numGroups / SLEEPING_GROUPS_PER_PAGE, 0)
    
For i = 1 To numGroupPages
  Sheets("Alvócsoport_alap").Copy After:=Sheets(Sheets.Count)
  ActiveSheet.Name = "Alvócsoport" & i
  ActiveSheet.Unprotect
  
  Dim j As Integer
  For j = 1 To SLEEPING_GROUPS_PER_PAGE
    Dim groupIndex As Integer: groupIndex = (i - 1) * SLEEPING_GROUPS_PER_PAGE + j
    
    If groupIndex <= numGroups Then
        Call GenerateSleepingGroup(data, numParticipants, groupIndex)
    End If
  Next j
Next i

End Sub

Sub GenerateSleepingGroup(data As Worksheet, numParticipants As Integer, groupIndex As Integer)

Const MAX_GROUP_SIZE = 5    ' not counting the leader

Dim groupPropertiesSheet As Worksheet:  Set groupPropertiesSheet = Sheets("Alvócsoport címek")
Dim groupLetter As String:              groupLetter = Chr(groupIndex + 64)
Dim groupIndexWithinPage As Integer:    groupIndexWithinPage = groupIndex Mod SLEEPING_GROUPS_PER_PAGE
If groupIndexWithinPage = 0 Then
  groupIndexWithinPage = SLEEPING_GROUPS_PER_PAGE
End If
Dim row As Integer: row = 1 + Int(groupIndexWithinPage - 1) * MAX_GROUP_SIZE    ' starting row of group on page

Cells(row, 1).Value = groupLetter

Dim numGroups As Integer: numGroups = groupPropertiesSheet.Cells(1, 1).CurrentRegion.Rows.Count
Dim i As Integer
For i = 1 To numGroups
  If groupLetter = groupPropertiesSheet.Cells(i, 1).Value Then
    Cells(row, 2).Value = groupPropertiesSheet.Cells(i, 2).Value
    Cells(row + 1, 2).Value = groupPropertiesSheet.Cells(i, 3).Value
    Cells(row + 2, 2).Value = groupPropertiesSheet.Cells(i, 4).Value
    Cells(row + 3, 2).Value = "      " & groupPropertiesSheet.Cells(i, 5).Value
    Cells(row + 4, 2).Value = "      " & groupPropertiesSheet.Cells(i, 6).Value
  End If
Next

Dim k As Integer: k = 0 ' index of participant within group

Dim ppl() As Person: ppl = Participants()
Dim var As Variant
Dim pers As Person

For Each var In ppl
    Set pers = var
    If pers.SleepingGroup = groupLetter Then
        If pers.SleepingGroupLeader Then
            Cells(row, 3).Value = pers.FirstName
            Cells(row + 1, 3).Value = pers.LastName
        Else
            k = k + 1
            Cells(row + k, 4).Value = pers.FullName
            If pers.Kind = ptNewcomer Then
              Cells(row + k, 4).Font.Bold = True
            ElseIf pers.Kind = ptOtherParticipant Then
              Cells(row + k, 4).Font.Underline = xlUnderlineStyleSingle
              Cells(row + k, 4).Font.Italic = True
            End If
        End If
    End If
Next

' Sort list of group members
Range(Cells(row, 4), Cells(row + MAX_GROUP_SIZE, 4)).Select
    Selection.Sort _
        Key1:=Cells(row, 4), _
        Order1:=xlAscending, _
        Header:=xlGuess, _
        OrderCustom:=1, _
        MatchCase:=False, _
        Orientation:=xlTopToBottom, _
        DataOption1:=xlSortNormal

End Sub

Sub GenerateHandout()
Attribute GenerateHandout.VB_ProcData.VB_Invoke_Func = "Z\n14"
'
' Generates a page you can handout to the closing ceremony participants
'
' Ctrl+Shift+Z
'

If Not WorksheetExists("Záró elõlap") Then
  Exit Sub
End If

Dim groupPropertiesSheet As Worksheet:      Set groupPropertiesSheet = Sheets("Alvócsoport címek")
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

Sheets("Záró_elõlap_alap").Copy After:=Sheets(Sheets.Count)
ActiveSheet.Name = "Záró elõlap"
ActiveSheet.Unprotect

Dim weekend As WeekendProperties: weekend = GetWeekendProperties()
Cells(1, 6) = Str(weekend.Number) & ". " & weekend.CommunityName & " Antióchia-hétvége, "
Cells(2, 6) = weekend.Date
Cells(3, 6) = weekend.Address

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
  For i = Sheets.Count To 9 Step (-1)
    Sheets(i).Delete
  Next
  Application.DisplayAlerts = True
End Sub

