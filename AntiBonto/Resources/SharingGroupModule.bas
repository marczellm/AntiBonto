Attribute VB_Name = "SharingGroupModule"
Option Explicit

Const GROUPS_PER_PAGE = 8

Sub GenerateSharingGroups()
Attribute GenerateSharingGroups.VB_ProcData.VB_Invoke_Func = "M\n14"
'
' Ctrl+Shift+M
'

If Not WorksheetExists("Megosztócsoport1") Then
  Exit Sub
End If

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


