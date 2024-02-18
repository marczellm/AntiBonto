Attribute VB_Name = "SleepingGroupModule"
Option Explicit

Const SLEEPING_GROUPS_PER_PAGE = 6

Function GetSleepingGroupNames() As GroupNaming
    Dim groupPropertiesSheet As Worksheet:  Set groupPropertiesSheet = Sheets("Alvócsoport címek")
    Dim numGroups As Integer: numGroups = groupPropertiesSheet.Cells(1, 1).CurrentRegion.Rows.Count - 1
    ReDim ret(numGroups - 1) As String
    GetSleepingGroupNames.GroupsAreNamed = False
    Dim i As Integer
    For i = 1 To numGroups
        Dim groupName As String
        groupName = groupPropertiesSheet.Cells(i + 1, 2)
        If Not StrEmpty(groupName) Then
            GetSleepingGroupNames.GroupsAreNamed = True
            ret(i - 1) = groupName
        End If
    Next
    GetSleepingGroupNames.GroupNames = ret
End Function

Sub GenerateSleepingGroups()
Attribute GenerateSleepingGroups.VB_ProcData.VB_Invoke_Func = "A\n14"
'
' Ctrl+Shift+A
'

If WorksheetExists("Alvócsoport1") Then
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


