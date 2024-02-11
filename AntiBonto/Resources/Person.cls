VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "Person"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
Option Explicit

Public Enum PersonType
    ptTeam = 0
    ptBoyLeader = 1
    ptGirlLeader = 2
    ptMusicLeader = 3
    ptMusicTeam = 4
    ptOtherParticipant = 10
    ptNewcomer = 11
End Enum

Public FirstName As String
Private mLastName As String
Private mNickname As String
Public Kind As PersonType
Public SharingGroup As Integer
Public SleepingGroup As String
Public SharingGroupLeader As Boolean
Public SleepingGroupLeader As Boolean

Property Get LastName() As String
    If StrEmpty(mNickname) Then
        LastName = mLastName
    Else
        LastName = mNickname
    End If
End Property

Property Get FullName() As String
    FullName = FirstName & " " & LastName
End Property

Property Get SleepingGroupNo() As Integer
    SleepingGroupNo = Asc(SleepingGroup) - 64
End Property

' constructor
Public Sub Init(data As Worksheet, participantIndex As Integer)
    Dim i As Integer: i = participantIndex
    FirstName = data.Cells(i, 1)
    mLastName = data.Cells(i, 2)
    mNickname = data.Cells(i, 3)
    Kind = data.Cells(i, 4)
    SharingGroup = data.Cells(i, 5)
    SharingGroupLeader = data.Cells(i, 6) = SharingGroup
    SleepingGroup = data.Cells(i, 7)
    SleepingGroupLeader = data.Cells(i, 8) = SleepingGroup
End Sub

