Attribute VB_Name = "BadgesModule"
Option Explicit


Sub GenerateBadges()
Attribute GenerateBadges.VB_ProcData.VB_Invoke_Func = "K\n14"
'
' Ctrl+Shift+K
'
    If WorksheetExists("Kitûzõ1") Then
      Exit Sub
    End If
    
    Const BADGES_PER_PAGE = 10
    
    Dim ppl() As Person: ppl = Participants()
    Dim numParticipants As Integer: numParticipants = ArrayLen(ppl)
    
    Dim numBadgePages As Integer
    numBadgePages = WorksheetFunction.RoundUp(numParticipants / BADGES_PER_PAGE, 0)
    
    Dim sharingGroups As GroupNaming: sharingGroups = GetSharingGroupNames()
    Dim sleepingGroups As GroupNaming: sleepingGroups = GetSleepingGroupNames()
    
    Dim i As Integer: i = 0 ' index of current participant
    Dim page As Integer
    For page = 1 To numBadgePages
      Sheets("Kitûzõ_alap").Copy After:=Sheets(Sheets.Count)
      ActiveSheet.Name = "Kitûzõ" & page
      ActiveSheet.Unprotect
      
      Dim j As Integer    ' badge row index in rows of 2 badges
      For j = 1 To BADGES_PER_PAGE / 2
        Dim row As Integer: row = (j - 1) * 5 + 1 ' index of first Excel row within current badge
        Call GenerateBadge(ppl(i), row, 1, sharingGroups, sleepingGroups)
        i = i + 1
        Call GenerateBadge(ppl(i), row, 4, sharingGroups, sleepingGroups)
        i = i + 1
        
        If i >= numParticipants Then
            Exit For
        End If
      Next j
    Next page
End Sub

Sub GenerateBadge(pers As Person, row As Integer, col As Integer, sharingGroups As GroupNaming, sleepingGroups As GroupNaming)
    Cells(row, col).Value = pers.FirstName
    Cells(row + 1, col).Value = " " + pers.LastName
    Dim sharingGroupName As String, sleepingGroupName As String
    If sharingGroups.GroupsAreNamed And pers.SharingGroup <> 0 Then
        sharingGroupName = sharingGroups.GroupNames(pers.SharingGroup - 1)
    Else
        sharingGroupName = pers.StrSharingGroup
    End If
    If sleepingGroups.GroupsAreNamed And pers.SleepingGroupNo <> 0 Then
        sleepingGroupName = sleepingGroups.GroupNames(pers.SleepingGroupNo - 1)
    Else
        sleepingGroupName = pers.SleepingGroup
    End If
    If sharingGroups.GroupsAreNamed Or sleepingGroups.GroupsAreNamed Then
        Cells(row + 3, col).Font.Size = 14
        Cells(row + 3, col).VerticalAlignment = xlVAlignTop
        Cells(row + 3, col).Value = "   " & sharingGroupName & vbLf & "   " & sleepingGroupName
    Else
        Cells(row + 3, col).Value = " " & sharingGroupName & "   " & sleepingGroupName
    End If
End Sub
