Attribute VB_Name = "SharingGroupModule"
Option Explicit

Const GROUPS_PER_PAGE = 8

Function GetSharingGroupNames() As GroupNaming
    Dim namesSheet As Worksheet: Set namesSheet = Sheets("Kiscsoport nevek")
    Dim numGroups As Integer: numGroups = namesSheet.Cells(1, 1).CurrentRegion.Rows.Count - 1
    ReDim ret(numGroups - 1) As String
    Dim i As Integer
    GetSharingGroupNames.GroupsAreNamed = False

    For i = 1 To numGroups
        Dim groupName As String
        groupName = namesSheet.Cells(i + 1, 2)
        If Not StrEmpty(groupName) Then
            GetSharingGroupNames.GroupsAreNamed = True
            ret(i - 1) = groupName
        End If
    Next i
    GetSharingGroupNames.GroupNames = ret
End Function

Sub GenerateSharingGroups()
Attribute GenerateSharingGroups.VB_ProcData.VB_Invoke_Func = "M\n14"
'
' Ctrl+Shift+M
'

If WorksheetExists("Megosztócsoport1") Then
  Exit Sub
End If

Dim data As Worksheet: Set data = Sheets("Alapadatok")

Dim numParticipants As Integer: numParticipants = GetNumParticipants()
Dim numSharingGroups As Integer: numSharingGroups = WorksheetFunction.Max(Range(data.Cells(2, 5), data.Cells(numParticipants + 1, 5)))
Dim numGroupPages As Integer: numGroupPages = WorksheetFunction.RoundUp(numSharingGroups / GROUPS_PER_PAGE, 0)
Dim nameData As GroupNaming: nameData = GetSharingGroupNames()
Dim i As Integer, j As Integer
Call SetupPrintHeaders(Sheets("Megosztócsoport_alap"), "MEGOSZTÓ CSOPORTOK")

For i = 1 To numGroupPages
  Sheets("Megosztócsoport_alap").Copy After:=Sheets(Sheets.Count)
  ActiveSheet.Name = "Megosztócsoport" & i
  ActiveSheet.Unprotect
  
  For j = 1 To GROUPS_PER_PAGE
    Dim sharingGroupIndex As Integer: sharingGroupIndex = (i - 1) * GROUPS_PER_PAGE + j
    
    If sharingGroupIndex <= numSharingGroups Then
      Call GenerateSharingGroup(data, numParticipants, sharingGroupIndex, nameData.GroupNames(j - 1), nameData.GroupsAreNamed)
    End If
  Next j
Next i

End Sub

Sub GenerateSharingGroup(data As Worksheet, _
                         numParticipants As Integer, _
                         sharingGroupIndex As Integer, _
                         groupName As String, _
                         GroupsAreNamed As Boolean)

Const ROWS_PER_GROUP = 7

Dim k As Integer: k = 0  ' row index within group
Dim groupIndexWithinPage As Integer: groupIndexWithinPage = sharingGroupIndex Mod ROWS_PER_GROUP
If groupIndexWithinPage = 0 Then
  groupIndexWithinPage = ROWS_PER_GROUP
End If

Dim row As Integer: row = 1 + Int((groupIndexWithinPage - 1) / 2) * ROWS_PER_GROUP
Dim col As Integer: col = 1 + ((groupIndexWithinPage - 1) Mod 2)

If GroupsAreNamed Then
    Cells(row, col).Value = sharingGroupIndex & ". " & groupName
    k = k + 1
End If

Dim ppl() As Person: ppl = Participants()
Dim var As Variant
Dim pers As Person
For Each var In ppl
    Set pers = var
    If pers.SharingGroup = sharingGroupIndex Then
        If pers.SharingGroupLeader Then
            If GroupsAreNamed Then
                Cells(row + 1, col).Value = pers.FullName
                Cells(row + 1, col).Font.Bold = True
            Else
                Cells(row, col).Value = sharingGroupIndex & ". " & pers.FullName
            End If
        Else
            k = k + 1
            Cells(row + k, col).Value = pers.FullName
            If pers.Kind = ptNewcomer Then
                Cells(row + k, col).Font.Italic = True
            ElseIf pers.Kind = ptOtherParticipant Then
                Cells(row + k, col).Font.Underline = xlUnderlineStyleSingle
                Cells(row + k, col).Font.Italic = True
            End If
        End If
    End If
Next

' Sort list of group members
Range(Cells(row + 1, col), Cells(row + ROWS_PER_GROUP - 1, col)).Select
    Selection.Sort _
        Key1:=Cells(row + 1, col), _
        Order1:=xlAscending, _
        Header:=xlGuess, _
        OrderCustom:=1, _
        MatchCase:=False, _
        Orientation:=xlTopToBottom, _
        DataOption1:=xlSortNormal

End Sub


