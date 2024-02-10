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

Sub GenerateBadges()
Attribute GenerateBadges.VB_ProcData.VB_Invoke_Func = "K\n14"
'
' Ctrl+Shift+K
'

If Not WorksheetExists("Kit�z�1") Then
  Exit Sub
End If

Const BADGES_PER_PAGE = 10

Dim ppl() As Person
ppl = Participants()

Dim numParticipants As Integer
numParticipants = ArrayLen(ppl)

Dim numBadgePages As Integer
numBadgePages = WorksheetFunction.RoundUp(numParticipants / BADGES_PER_PAGE, 0)

Dim k As Integer: k = 0 ' index of current participant
Dim i As Integer        ' index of current badge page being generated
For i = 1 To numBadgePages
  Sheets("Kit�z�_alap").Copy After:=Sheets(Sheets.Count)
  ActiveSheet.Name = "Kit�z�" & i
  ActiveSheet.Unprotect
  
  Dim j As Integer    ' badge row index in rows of 2 badges
  For j = 1 To BADGES_PER_PAGE / 2
    Dim m As Integer: m = (j - 1) * 5 + 1 ' index of first Excel row within current badge
        
    ' Generate first badge in row
    Cells(m, 1).Value = ppl(k).FirstName
    Cells(m + 1, 1).Value = " " + ppl(k).LastName
    Cells(m + 3, 1).Value = " " & ppl(k).SharingGroup & "   " & ppl(k).SleepingGroup
    k = k + 1
    
    ' Generate last badge in row
    Cells(m, 4).Value = ppl(k).FirstName
    Cells(m + 1, 4).Value = " " + ppl(k).LastName
    Cells(m + 3, 4).Value = " " & ppl(k).SharingGroup & "   " & ppl(k).SleepingGroup
    k = k + 1
    
    If k >= numParticipants Then
        Exit For
    End If
  Next j
Next i

End Sub

Sub GenerateSharingGroups()
Attribute GenerateSharingGroups.VB_ProcData.VB_Invoke_Func = "M\n14"
'
' Ctrl+Shift+M
'

If Not WorksheetExists("Megoszt�csoport1") Then
  Exit Sub
End If

Const GROUPS_PER_PAGE = 8
Dim weekendPropertiesSheet As Worksheet:    Set weekendPropertiesSheet = Sheets("Vez�rl� adatok")
Dim data As Worksheet:                      Set data = Sheets("Alapadatok")
Dim communityName As String:                communityName = weekendPropertiesSheet.Cells(1, 2).Value
Dim weekendNum As Integer:                  weekendNum = weekendPropertiesSheet.Cells(2, 2).Value
Dim weekendDate As String:                  weekendDate = weekendPropertiesSheet.Cells(3, 2).Value
Dim location As String:                     location = weekendPropertiesSheet.Cells(4, 2).Value
Dim address As String:                      address = weekendPropertiesSheet.Cells(5, 2).Value
Dim numParticipants As Integer

data.Unprotect
numParticipants = data.Cells(1, 1).CurrentRegion.Rows.Count - 1
data.Protect

Dim numSharingGroups As Integer: numSharingGroups = WorksheetFunction.Max(Range(data.Cells(2, 5), data.Cells(numParticipants + 1, 5)))
Dim numGroupPages As Integer: numGroupPages = WorksheetFunction.RoundUp(numSharingGroups / GROUPS_PER_PAGE, 0)

' Setup headers for printed page
With Sheets("Megoszt�csoport_alap").PageSetup
    .CenterHeader = _
      "&""Constantia,Norm�l""&26MEGOSZT� CSOPORTOK&12" & Chr(10) & _
      "&14" & Str(weekendNum) & ". " & communityName & " Anti�chia-h�tv�ge, " & weekendDate & Chr(10) _
      & location & Chr(10) _
      & address & Chr(10) & ""
    .LeftHeader = ""
    .RightHeader = ""
    .LeftFooter = ""
    .CenterFooter = ""
    .RightFooter = ""
End With


Dim i As Integer, j As Integer
For i = 1 To numGroupPages
  Sheets("Megoszt�csoport_alap").Copy After:=Sheets(Sheets.Count)
  ActiveSheet.Name = "Megoszt�csoport" & i
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

Dim i As Integer
For i = 2 To numParticipants + 1  ' index of participant
  Dim FirstName As String: FirstName = data.Cells(i, 1)
  Dim LastName As String: LastName = data.Cells(i, 2)
  Dim nickname As String: nickname = data.Cells(i, 3)
  Dim participantType As Integer: participantType = data.Cells(i, 4)
  Dim SharingGroup As Integer: SharingGroup = data.Cells(i, 5)
  Dim ledGroup As Integer: ledGroup = data.Cells(i, 6)
  Dim FullName As String
  If StrEmpty(nickname) Then
    FullName = FirstName & " " & LastName
  Else
    FullName = FirstName & " " & nickname
  End If
  
  If SharingGroup = sharingGroupIndex Then
    If ledGroup = sharingGroupIndex Then    ' leader
      Cells(row, col).Value = sharingGroupIndex & ". " & FullName
    Else                                    ' member
      k = k + 1
      Cells(row + k, col).Value = FullName
      If participantType = 11 Then   ' newcomer
        Cells(row + k, col).Font.Bold = True
      ElseIf participantType = 10 Then   ' other participant
        Cells(row + k, col).Font.Underline = xlUnderlineStyleSingle
        Cells(row + k, col).Font.Italic = True
      End If
    End If
  End If
Next i

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

If Not WorksheetExists("Alv�csoport1") Then
  Exit Sub
End If

Dim weekendPropertiesSheet As Worksheet:    Set weekendPropertiesSheet = Sheets("Vez�rl� adatok")
Dim groupPropertiesSheet As Worksheet:      Set groupPropertiesSheet = Sheets("Alv�csoport c�mek")
Dim data As Worksheet:                      Set data = Sheets("Alapadatok")
Dim communityName As String:                communityName = weekendPropertiesSheet.Cells(1, 2).Value
Dim weekendNum As Integer:                  weekendNum = weekendPropertiesSheet.Cells(2, 2).Value
Dim weekendDate As String:                  weekendDate = weekendPropertiesSheet.Cells(3, 2).Value
Dim location As String:                     location = weekendPropertiesSheet.Cells(4, 2).Value
Dim address As String:                      address = weekendPropertiesSheet.Cells(5, 2).Value
Dim numParticipants As Integer

data.Unprotect
numParticipants = data.Cells(1, 1).CurrentRegion.Rows.Count - 1
data.Protect

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

' Setup headers for printed page
With Sheets("Alv�csoport_alap").PageSetup
    .CenterHeader = _
      "&""Constantia,Norm�l""&26ALV�CSOPORTOK&12" & Chr(10) & _
      "&14" & Str(weekendNum) & ". " & communityName & " Anti�chia-h�tv�ge, " & weekendDate & Chr(10) _
      & location & Chr(10) _
      & address & Chr(10) & ""
    .LeftHeader = ""
    .RightHeader = ""
    .LeftFooter = ""
    .CenterFooter = ""
    .RightFooter = ""
End With

Dim numGroupPages As Integer: numGroupPages = WorksheetFunction.RoundUp(numGroups / SLEEPING_GROUPS_PER_PAGE, 0)
    
For i = 1 To numGroupPages
  Sheets("Alv�csoport_alap").Copy After:=Sheets(Sheets.Count)
  ActiveSheet.Name = "Alv�csoport" & i
  ActiveSheet.Unprotect
  
  Dim j As Integer
  For j = 1 To SLEEPING_GROUPS_PER_PAGE
    Dim groupIndex As Integer: groupIndex = (i - 1) * SLEEPING_GROUPS_PER_PAGE + j
    
    If groupIndex <= numGroups Then
        Call GenerateSleepingGroup(data, groupPropertiesSheet, numParticipants, groupIndex)
    End If
  Next j
Next i

End Sub

Sub GenerateSleepingGroup(data As Worksheet, groupPropertiesSheet As Worksheet, numParticipants As Integer, groupIndex As Integer)

Const MAX_GROUP_SIZE = 5    ' not counting the leader

Dim groupLetter As String: groupLetter = Chr(groupIndex + 64)
Dim groupIndexWithinPage As Integer: groupIndexWithinPage = groupIndex Mod SLEEPING_GROUPS_PER_PAGE
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

For i = 2 To numParticipants + 1
  Dim FirstName As String: FirstName = data.Cells(i, 1)
  Dim LastName As String: LastName = data.Cells(i, 2)
  Dim nickname As String: nickname = data.Cells(i, 3)
  Dim participantType As Integer: participantType = data.Cells(i, 4)
  Dim SleepingGroup As Integer: SleepingGroup = data.Cells(i, 7)
  Dim ledGroup As Integer: ledGroup = data.Cells(i, 8)
  Dim FullName As String
  If StrEmpty(nickname) Then
    FullName = FirstName & " " & LastName
  Else
    FullName = FirstName & " " & nickname
  End If
  
  If SleepingGroup = groupLetter Then
    If ledGroup = groupLetter Then    ' leader
      Cells(row, 3).Value = FirstName
      If StrEmpty(nickname) Then
        Cells(row + 1, 3).Value = LastName
      Else
        Cells(row + 1, 3).Value = nickname
      End If
    Else                              ' member
      k = k + 1
      Cells(row + k, 4).Value = FullName  ' newcomer
      If participantType = 11 Then
        Cells(row + k, 4).Font.Bold = True
      ElseIf participantType = 10 Then    ' other participant
        Cells(row + k, 4).Font.Underline = xlUnderlineStyleSingle
        Cells(row + k, 4).Font.Italic = True
      End If
    End If
  End If
Next i

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

If Not WorksheetExists("Z�r� el�lap") Then
  Exit Sub
End If

Dim weekendPropertiesSheet As Worksheet:    Set weekendPropertiesSheet = Sheets("Vez�rl� adatok")
Dim groupPropertiesSheet As Worksheet:      Set groupPropertiesSheet = Sheets("Alv�csoport c�mek")
Dim data As Worksheet:                      Set data = Sheets("Alapadatok")
Dim communityName As String:                communityName = weekendPropertiesSheet.Cells(1, 2).Value
Dim weekendNum As Integer:                  weekendNum = weekendPropertiesSheet.Cells(2, 2).Value
Dim weekendDate As String:                  weekendDate = weekendPropertiesSheet.Cells(3, 2).Value
Dim location As String:                     location = weekendPropertiesSheet.Cells(4, 2).Value
Dim address As String:                      address = weekendPropertiesSheet.Cells(5, 2).Value
Dim numParticipants As Integer

data.Unprotect
numParticipants = data.Cells(1, 1).CurrentRegion.Rows.Count - 1

Dim VS As String      ' A vezet�ket le�r� string t�pus� v�ltoz�
Dim j As Integer, J_S As Integer, J_O As Integer
Dim k As Integer, K_S As Integer, K_O As Integer
Dim L As Integer, L_O_DB As Integer

data.Range(Cells(2, 1), Cells(numParticipants, 8)).Sort _
    Key1:=Cells(2, 1), _
    Order1:=xlAscending, _
    Key2:=Cells(2, 2), _
    Order2:=xlAscending, _
    Key3:=Cells(2, 3), _
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

Cells(1, 6) = Str(weekendNum) & ". " & communityName & " Anti�chia-h�tv�ge, "
Cells(2, 6) = weekendDate
Cells(3, 6) = address

L = 0
Dim i As Integer
For i = 2 To numParticipants + 1  ' I az "Alapadatok" lapon az �ppen feldolgozott tag adatainak sorindexe.
  If IsEmpty(data.Cells(i, 4)) Or data.Cells(i, 4) = 0 _
      Or data.Cells(i, 4) = 1 Or data.Cells(i, 4) = 2 _
      Or data.Cells(i, 4) = 3 Or data.Cells(i, 4) = 4 _
      Or data.Cells(i, 4) = 10 Then
    L = L + 1
  End If
Next i

L_O_DB = Int(L / 3)
If (L Mod 3) <> 0 Then
  L = L + 1
End If

VS = ""
j = 0
k = 0

For i = 2 To numParticipants + 1  ' I az "Alapadatok" lapon az �ppen feldolgozott tag adatainak sorindexe.
  
  If IsEmpty(data.Cells(i, 4)) Or data.Cells(i, 4) = 0 _
      Or data.Cells(i, 4) = 1 Or data.Cells(i, 4) = 2 _
      Or data.Cells(i, 4) = 3 Or data.Cells(i, 4) = 4 _
      Or data.Cells(i, 4) = 10 Then
      
    If data.Cells(i, 4) = 1 Then ' Boy leader
      VS = VS & " & " & data.Cells(i, 1) & " " & data.Cells(i, 2)
    End If
      
    If data.Cells(i, 4) = 2 Then ' Girl leader
      VS = data.Cells(i, 1) & " " & data.Cells(i, 2) & VS
    End If
      
    If data.Cells(i, 4) = 3 Or data.Cells(i, 4) = 4 Then ' Music team member
      J_S = 27 + Int(j / 3)
      J_O = 2 + (j Mod 3)
      Cells(J_S, J_O).Value = data.Cells(i, 1) & " " & data.Cells(i, 2)
      If data.Cells(i, 4) = 3 Then ' Music team leader
      Cells(J_S, J_O).Font.Underline = xlUnderlineStyleSingle
      End If
      j = j + 1
    End If
      
    K_S = 9 + k Mod L_O_DB
    K_O = 2 + Int(k / L_O_DB)
    Cells(K_S, K_O).Value = data.Cells(i, 1) & " " & data.Cells(i, 2)
    k = k + 1
  End If
Next i

Cells(6, 2).Value = VS

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

