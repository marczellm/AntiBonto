Attribute VB_Name = "Module1"
Option Explicit
Sub �sszes()
Attribute �sszes.VB_ProcData.VB_Invoke_Func = "O\n14"
  Call Kit�z�_k�sz�t�
  Call Megoszt�_csoport_k�sz�t�
  Call Alv�_csoport_k�sz�t�
  Call Z�r�_el�lap_k�sz�t�
End Sub
Sub Kit�z�_k�sz�t�()
Attribute Kit�z�_k�sz�t�.VB_ProcData.VB_Invoke_Func = "K\n14"
'
' Kit�z�_k�sz�t� Makro
' R�gz�tette: Kiss L�szl�, d�tum: 2009.11.23.
'
' Billenty�parancs: Ctrl+Shift+K
'

If Not OK("Kit�z�1") Then
  Exit Sub
End If

Dim N As Integer ' A l�tsz�m
Dim Lap As Object
Set Lap = Sheets("Alapadatok")
Lap.Unprotect
N = Lap.Cells(1, 1).CurrentRegion.Rows.Count - 1
Lap.Protect
Dim M As Integer, DB As Integer ' A kit�z�oldalak �s az oldalakon l�v� kit�z�k sz�ma.
                                ' Ut�bbi mindig p�ros!

Dim I As Integer, J As Integer
Dim K As Integer, L As Integer  ' A lehets�ges r�sztvev�k feldolgoz�si sorsz�ma �s
                                ' a kit�z� lapon a sorindex.

DB = 10
M = Int(N / DB)
If N > M * DB Then
  M = M + 1
End If

For I = 1 To M   ' I az aktu�lisan gener�lt kit�z�oldal sorsz�ma
  Sheets("Kit�z�_alap").Copy After:=Sheets(Sheets.Count)
  Sheets("Kit�z�_alap (2)").Name = "Kit�z�" & I
  ActiveSheet.Unprotect
  
  For J = 1 To DB / 2   ' Az oldalon bel�l k�t kit�z� van soronk�nt, J a "sor" sz�ma
    ' A soron bel�li els� kit�z� gener�l�sa
    K = (I - 1) * DB + J * 2   ' A szem�lyt tartalmaz� sor sz�ma az "Alapadatok" lapon
    L = (J - 1) * 5 + 1        ' A kit�z�oldalon az aktu�lis kit�z�h�z tartoz� Excel sor sz�ma
    If IsEmpty(Lap.Cells(K, 3).Value) Then   ' Nincs beceneve
     Cells(L, 1).Value = Lap.Cells(K, 1).Value
     Cells(L + 1, 1).Value = " " + Lap.Cells(K, 2).Value
    Else                                     ' Van beceneve
      Cells(L, 1).Value = Lap.Cells(K, 1).Value + " " + Lap.Cells(K, 2).Value
      Cells(L + 1, 1).Value = " " + Lap.Cells(K, 3).Value
    End If
    ' Megjegyz�s
    If Not IsEmpty(Lap.Cells(K, 9).Value) Then
        Cells(L + 2, 1).Value = "(" + Lap.Cells(K, 9).Value + ")"
        Cells(L + 2, 1).Font.Size = 8
        Cells(L + 2, 1).VerticalAlignment = xlCenter
        Cells(L + 2, 1).HorizontalAlignment = xlRight
    End If
    ' Kiscsoport �s alv�csoport sz�ma
    Cells(L + 3, 1).Value = " " & Lap.Cells(K, 5).Value & "   " & Lap.Cells(K, 7).Value
    
    ' A soron bel�li m�sodik kit�z� gener�l�sa
    K = K + 1
    If IsEmpty(Lap.Cells(K, 3).Value) Then
      Cells(L, 4).Value = Lap.Cells(K, 1).Value
      Cells(L + 1, 4).Value = " " + Lap.Cells(K, 2).Value
    Else
      Cells(L, 4).Value = Lap.Cells(K, 1).Value + " " + Lap.Cells(K, 2).Value
      Cells(L + 1, 4).Value = " " + Lap.Cells(K, 3).Value
    End If
    
    If Not IsEmpty(Lap.Cells(K, 9).Value) Then
        Cells(L + 2, 4).Value = "(" + Lap.Cells(K, 9).Value + ")"
        Cells(L + 2, 4).Font.Size = 8
        Cells(L + 2, 4).VerticalAlignment = xlCenter
        Cells(L + 2, 4).HorizontalAlignment = xlRight
    End If
    Cells(L + 3, 4).Value = " " & Lap.Cells(K, 5).Value & "   " & Lap.Cells(K, 7).Value
   Next J
Next I

End Sub

Sub Megoszt�_csoport_k�sz�t�()
Attribute Megoszt�_csoport_k�sz�t�.VB_ProcData.VB_Invoke_Func = "M\n14"
'
' Megoszt�_csoport_k�sz�t� Makro
' R�gz�tette: Kiss L�szl�, d�tum: 2009.11.23.
'
' Billenty�parancs: Ctrl+Shift+M
'

If Not OK("Megoszt�csoport1") Then
  Exit Sub
End If

Dim V_lap As Object, Lap As Object

Dim K_nev As String   ' A k�z�ss�g beve.
Dim H_N As Integer    ' A h�tv�ge sorsz�ma.
Dim H_D As String     ' A h�tv�ge d�tuma.
Dim H_H As String     ' A h�tv�ge helysz�ne.
Dim H_HC As String    ' A h�tv�ge helysz�n�nek c�me.
Dim N As Integer      ' A h�tv�g�n a lehets�ges r�sztvev�k l�tsz�ma.
Dim MCs_N As Integer  ' A megoszt� csoportok sz�ma.
Dim MCs_I As Integer  ' Az �ppen feldolgozott megoszt� csoport indexe.
Dim I As Integer

Set V_lap = Sheets("Vez�rl� adatok")
Set Lap = Sheets("Alapadatok")

K_nev = V_lap.Cells(1, 2).Value
H_N = V_lap.Cells(2, 2).Value
H_D = V_lap.Cells(3, 2).Value
H_H = V_lap.Cells(4, 2).Value
H_HC = V_lap.Cells(5, 2).Value
Lap.Unprotect
N = Lap.Cells(1, 1).CurrentRegion.Rows.Count - 1
Lap.Protect

MCs_N = 0
For I = 2 To N + 1
  If Lap.Cells(I, 5).Value > MCs_N Then
     MCs_N = Lap.Cells(I, 5).Value
  End If
Next I

' MCs_N = V_lap.Cells(6, 2).Value

Dim M As Integer, DB As Integer ' A megoszt� csoportokat tartalmaz� oldalak sz�ma
                                ' �s az oldalakon l�v� csoportok lehets�ges sz�ma.
                                ' Ut�bbi mindig p�ros!
Dim J As Integer
Dim K As Integer, L As Integer

With Sheets("Megoszt�csoport_alap").PageSetup
    .LeftHeader = ""
    .CenterHeader = _
      "&""Monotype Corsiva,Norm�l""&26MEGOSZT� CSOPORTOK&12" & Chr(10) & _
      "&14" & Str(H_N) & ". " & K_nev & " Anti�chia-h�tv�ge, " & H_D & Chr(10) _
      & H_H & Chr(10) _
      & H_HC & Chr(10) & ""
    .RightHeader = ""
    .LeftFooter = ""
    .CenterFooter = ""
    .RightFooter = ""
End With

DB = 8
M = Int(MCs_N / DB)
If MCs_N > M * DB Then
  M = M + 1
End If

For I = 1 To M
  Sheets("Megoszt�csoport_alap").Copy After:=Sheets(Sheets.Count)
  Sheets("Megoszt�csoport_alap (2)").Name = "Megoszt�csoport" & I
  ActiveSheet.Unprotect
  
  For J = 1 To DB
    MCs_I = (I - 1) * DB + J
    
    If MCs_I > MCs_N Then
      Exit For
    End If
    
    Call Egy_megoszt�_csoport_feldolgoz�sa(Lap, N, MCs_I, DB)
  Next J
Next I

End Sub

Sub Egy_megoszt�_csoport_feldolgoz�sa(Lap As Object, N As Integer, MCs_I As Integer, DB As Integer)
  
Dim I As Integer, J As Integer, K As Integer, L As Integer
Dim S_Cspv As Integer, O_Cspv As Integer
Dim Cs_M As Integer ' Egy csoport maxim�lis l�tsz�ma (a Megoszt�csoport_alap lapon!).
Cs_M = 7
K = 0 ' Az adott csoportb�l �ppen feldolgozott tag indexe.
L = MCs_I Mod DB ' A csoport sorsz�ma az adott lapon.
If L = 0 Then
  L = DB
End If
S_Cspv = 1 + Int((L - 1) / 2) * Cs_M    ' A csoportvezet� sorindexe.
O_Cspv = 1 + ((L - 1) Mod 2)            ' A csoportvezet� soszlopindexe.

For I = 2 To N + 1  ' I az "Alapadatok" lapon az �ppen feldolgozott tag adatainak sorindexe.
  If Lap.Cells(I, 5) = MCs_I Then
    If Lap.Cells(I, 6) = MCs_I Then    ' A csoport vezet�je.
'      If IsEmpty(Lap.Cells(I, 3).Value) Then
        Cells(S_Cspv, O_Cspv).Value = MCs_I & ". " & Lap.Cells(I, 1) & " " & Lap.Cells(I, 2)
'      Else
'        Cells(S_Cspv, O_Cspv).Value = MCs_I & ". " & Lap.Cells(I, 1) & " " & Lap.Cells(I, 3)
'      End If
    Else                              ' A csoport tagja
      K = K + 1
'     If IsEmpty(Lap.Cells(I, 3).Value) Then
        Cells(S_Cspv + K, O_Cspv).Value = Lap.Cells(I, 1) & " " & Lap.Cells(I, 2)
        If Lap.Cells(I, 4).Value = 11 Then
          Cells(S_Cspv + K, O_Cspv).Font.Bold = True
        End If
        If Lap.Cells(I, 4).Value = 10 Then
          Cells(S_Cspv + K, O_Cspv).Font.Underline = xlUnderlineStyleSingle
          Cells(S_Cspv + K, O_Cspv).Font.Italic = True
        End If
'     Else
'        Cells(S_Cspv + K, O_Cspv).Value = Lap.Cells(I, 1) & " " & Lap.Cells(I, 3)
'        If Lap.Cells(I, 4).Value = 11 Then
'          Cells(S_Cspv + K, O_Cspv).Font.Bold = True
'        End If
'        If Lap.Cells(I, 4).Value = 10 Then
'          Cells(S_Cspv + K, O_Cspv).Font.Underline = xlUnderlineStyleSingle
'          Cells(S_Cspv + K, O_Cspv).Font.Italic = True
'        End If
'      End If
    End If
  End If
Next I

Range(Cells(S_Cspv + 1, O_Cspv), Cells(S_Cspv + Cs_M - 1, O_Cspv)).Select
    Selection.Sort Key1:=Cells(S_Cspv + 1, O_Cspv), Order1:=xlAscending, Header:=xlGuess, _
        OrderCustom:=1, MatchCase:=False, Orientation:=xlTopToBottom, _
        DataOption1:=xlSortNormal

End Sub

Sub Alv�_csoport_k�sz�t�()
Attribute Alv�_csoport_k�sz�t�.VB_ProcData.VB_Invoke_Func = "A\n14"
'
' Alv�_csoport_k�sz�t� Makro
' R�gz�tette: Kiss L�szl�, d�tum: 2009.11.27.
'
' Billenty�parancs: Ctrl+Shift+A
'

If Not OK("Alv�csoport1") Then
  Exit Sub
End If

Dim V_lap As Object, A_lap As Object, Lap As Object

Dim K_nev As String   ' A k�z�ss�g beve.
Dim H_N As Integer    ' A h�tv�ge sorsz�ma.
Dim H_D As String     ' A h�tv�ge d�tuma.
Dim H_H As String     ' A h�tv�ge helysz�ne.
Dim H_HC As String    ' A h�tv�ge helysz�n�nek c�me.
Dim N As Integer      ' A h�tv�g�n a lehets�ges r�sztvev�k l�tsz�ma.
Dim ACs_N As Integer  ' Az alv�csoportok sz�ma.
Dim ACs_I As Integer  ' Az �ppen feldolgozott alv�csoport indexe.
Dim I As Integer

Set V_lap = Sheets("Vez�rl� adatok")
Set A_lap = Sheets("Alv�csoport c�mek")
Set Lap = Sheets("Alapadatok")

K_nev = V_lap.Cells(1, 2).Value
H_N = V_lap.Cells(2, 2).Value
H_D = V_lap.Cells(3, 2).Value
H_H = V_lap.Cells(4, 2).Value
H_HC = V_lap.Cells(5, 2).Value
Lap.Unprotect
N = Lap.Cells(1, 1).CurrentRegion.Rows.Count - 1
Lap.Protect

ACs_N = 0
For I = 2 To N + 1
  If Not IsEmpty(Lap.Cells(I, 7).Value) Then
    If Asc(Lap.Cells(I, 7).Value) - 64 > ACs_N Then
      ACs_N = Asc(Lap.Cells(I, 7).Value) - 64
    End If
  End If
Next I

' ACs_N = V_lap.Cells(7, 2).Value

Dim M As Integer, DB As Integer ' Az alv�csoportokat tartalmaz� oldalak sz�ma
                                ' �s az oldalakon l�v� csoportok lehets�ges sz�ma.
Dim J As Integer
Dim K As Integer, L As Integer

With Sheets("Alv�csoport_alap").PageSetup
    .LeftHeader = ""
    .CenterHeader = _
      "&""Monotype Corsiva,Norm�l""&26ALV�CSOPORTOK&12" & Chr(10) & _
      "&14" & Str(H_N) & ". " & K_nev & " Anti�chia-h�tv�ge, " & H_D & Chr(10) _
      & H_H & Chr(10) _
      & H_HC & Chr(10) & ""
    .RightHeader = ""
    .LeftFooter = ""
    .CenterFooter = ""
    .RightFooter = ""
End With

DB = 6
M = Int(ACs_N / DB)
If ACs_N > M * DB Then
  M = M + 1
End If
    
For I = 1 To M
  Sheets("Alv�csoport_alap").Copy After:=Sheets(Sheets.Count)
  Sheets("Alv�csoport_alap (2)").Name = "Alv�csoport" & I
  ActiveSheet.Unprotect
  
  For J = 1 To DB
    ACs_I = (I - 1) * DB + J
    
    If ACs_I > ACs_N Then
      Exit For
    End If
    
    Call Egy_alv�_csoport_feldolgoz�sa(Lap, A_lap, N, ACs_I, DB)
  Next J
Next I

End Sub

Sub Egy_alv�_csoport_feldolgoz�sa(Lap As Object, A_lap As Object, N As Integer, ACs_I As Integer, DB As Integer)
  
Dim I As Integer, J As Integer, K As Integer, L As Integer
Dim S_Csp As Integer
Dim ACsC_N As Integer, ACs_Char As String ' Az alv�csoportc�mek sz�ma �s bet�jele.
Dim Cs_A As Integer ' Egy csoport maxim�lis l�tsz�ma - 1 (az Alv�csoport_alap lapon!).

Cs_A = 5
K = 0 ' Az adott csoportb�l �ppen feldolgozott tag indexe.
L = ACs_I Mod DB ' A csoport sorsz�ma az adott lapon.
If L = 0 Then
  L = DB
End If
S_Csp = 1 + Int(L - 1) * Cs_A    ' A csoport sorindexe.

ACs_Char = Chr(ACs_I + 64)
Cells(S_Csp, 1).Value = ACs_Char

ACsC_N = A_lap.Cells(1, 1).CurrentRegion.Rows.Count

For I = 1 To ACsC_N
  If ACs_Char = A_lap.Cells(I, 1).Value Then
    Cells(S_Csp, 2).Value = A_lap.Cells(I, 2).Value
    Cells(S_Csp + 1, 2).Value = A_lap.Cells(I, 3).Value
    Cells(S_Csp + 2, 2).Value = A_lap.Cells(I, 4).Value
    Cells(S_Csp + 3, 2).Value = Cells(S_Csp + 3, 2).Value & " " & A_lap.Cells(I, 5).Value
    Cells(S_Csp + 4, 2).Value = Cells(S_Csp + 4, 2).Value & " " & A_lap.Cells(I, 6).Value
  End If
Next

For I = 2 To N + 1  ' I az "Alapadatok" lapon az �ppen feldolgozott tag adatainak sorindexe.
  If Lap.Cells(I, 7) = ACs_Char Then
    If Lap.Cells(I, 8) = ACs_Char Then    ' A csoport vezet�je.
      Cells(S_Csp, 3).Value = Lap.Cells(I, 1)
'      If IsEmpty(Lap.Cells(I, 3).Value) Then
        Cells(S_Csp + 1, 3).Value = Lap.Cells(I, 2)
'      Else
'        Cells(S_Csp + 1, 3).Value = Lap.Cells(I, 3)
'      End If
    Else                              ' A csoport tagja
      K = K + 1
'     If IsEmpty(Lap.Cells(I, 3).Value) Then
        Cells(S_Csp + K, 4).Value = Lap.Cells(I, 1) & " " & Lap.Cells(I, 2)
        If Lap.Cells(I, 4).Value = 11 Then
          Cells(S_Csp + K, 4).Font.Bold = True
        End If
        If Lap.Cells(I, 4).Value = 10 Then
          Cells(S_Csp + K, 4).Font.Underline = xlUnderlineStyleSingle
          Cells(S_Csp + K, 4).Font.Italic = True
        End If
'     Else
'        Cells(S_Csp + K, 4).Value = Lap.Cells(I, 1) & " " & Lap.Cells(I, 3)
'        If Lap.Cells(I, 4).Value = 11 Then
'          Cells(S_Csp + K, 4).Font.Bold = True
'        End If
'        If Lap.Cells(I, 4).Value = 10 Then
'          Cells(S_Csp + K, 4).Font.Underline = xlUnderlineStyleSingle
'          Cells(S_Csp + K, 4).Font.Italic = True
'        End If
'      End If
    End If
  End If
Next I

' Itt Cs_A-1 volt, de az hib�s, mert a Cs_A alapb�l csoportl�tsz�m-1 -- Marczell M�rton
Range(Cells(S_Csp, 4), Cells(S_Csp + Cs_A, 4)).Select
    Selection.Sort Key1:=Cells(S_Csp, 4), Order1:=xlAscending, Header:=xlGuess, _
        OrderCustom:=1, MatchCase:=False, Orientation:=xlTopToBottom, _
        DataOption1:=xlSortNormal

End Sub

Sub Z�r�_el�lap_k�sz�t�()
Attribute Z�r�_el�lap_k�sz�t�.VB_ProcData.VB_Invoke_Func = "Z\n14"
    
'
' Z�r�_el�lap_k�sz�t� Makro
' R�gz�tette: Kiss L�szl�, d�tum: 2009.11.23.
'
' Billenty�parancs: Ctrl+Shift+Z
'

If Not OK("Z�r� el�lap") Then
  Exit Sub
End If

Dim V_lap As Object, Lap As Object

Dim K_nev As String   ' A k�z�ss�g beve.
Dim H_N As Integer    ' A h�tv�ge sorsz�ma.
Dim H_D As String     ' A h�tv�ge d�tuma.
Dim H_H As String     ' A h�tv�ge helysz�ne.
Dim H_HC As String    ' A h�tv�ge helysz�n�nek c�me.
Dim N As Integer      ' A h�tv�g�n a lehets�ges r�sztvev�k l�tsz�ma.
Dim VS As String      ' A vezet�ket le�r� string t�pus� v�ltoz�
Dim I As Integer
Dim J As Integer, J_S As Integer, J_O As Integer
Dim K As Integer, K_S As Integer, K_O As Integer
Dim L As Integer, L_O_DB As Integer

Set V_lap = Sheets("Vez�rl� adatok")
Set Lap = Sheets("Alapadatok")

K_nev = V_lap.Cells(1, 2).Value
H_N = V_lap.Cells(2, 2).Value
H_D = V_lap.Cells(3, 2).Value
H_H = V_lap.Cells(4, 2).Value
H_HC = V_lap.Cells(5, 2).Value
Lap.Unprotect
N = Lap.Cells(1, 1).CurrentRegion.Rows.Count - 1

Lap.Select
Range(Cells(2, 1), Cells(N, 8)).Sort Key1:=Cells(2, 1), Order1:=xlAscending, Key2:=Cells(2, 2) _
    , Order2:=xlAscending, Key3:=Cells(2, 3), Order3:=xlAscending, Header:= _
    xlGuess, OrderCustom:=1, MatchCase:=False, Orientation:=xlTopToBottom, _
    DataOption1:=xlSortNormal, DataOption2:=xlSortNormal, DataOption3:= _
    xlSortNormal
ActiveSheet.Protect

Sheets("Z�r�_el�lap_alap").Copy After:=Sheets(Sheets.Count)
Sheets("Z�r�_el�lap_alap (2)").Name = "Z�r� el�lap"
ActiveSheet.Unprotect

Cells(1, 6) = Str(H_N) & ". " & K_nev & " Anti�chia-h�tv�ge, "
Cells(2, 6) = H_D
Cells(3, 6) = H_HC

L = 0
For I = 2 To N + 1  ' I az "Alapadatok" lapon az �ppen feldolgozott tag adatainak sorindexe.
  
  If IsEmpty(Lap.Cells(I, 4)) Or Lap.Cells(I, 4) = 0 _
      Or Lap.Cells(I, 4) = 1 Or Lap.Cells(I, 4) = 2 _
      Or Lap.Cells(I, 4) = 3 Or Lap.Cells(I, 4) = 4 _
      Or Lap.Cells(I, 4) = 10 Then
    L = L + 1
  End If
Next I

L_O_DB = Int(L / 3)
If (L Mod 3) <> 0 Then
  L = L + 1
End If

VS = ""
J = 0
K = 0

For I = 2 To N + 1  ' I az "Alapadatok" lapon az �ppen feldolgozott tag adatainak sorindexe.
  
  If IsEmpty(Lap.Cells(I, 4)) Or Lap.Cells(I, 4) = 0 _
      Or Lap.Cells(I, 4) = 1 Or Lap.Cells(I, 4) = 2 _
      Or Lap.Cells(I, 4) = 3 Or Lap.Cells(I, 4) = 4 _
      Or Lap.Cells(I, 4) = 10 Then
      
    If Lap.Cells(I, 4) = 1 Then ' A H�tv�ge fi� vezet�je
      VS = VS & " & " & Lap.Cells(I, 1) & " " & Lap.Cells(I, 2)
    End If
      
    If Lap.Cells(I, 4) = 2 Then ' A H�tv�ge l�ny vezet�je
      VS = Lap.Cells(I, 1) & " " & Lap.Cells(I, 2) & VS
    End If
      
    If Lap.Cells(I, 4) = 3 Or Lap.Cells(I, 4) = 4 Then ' A H�tv�ge zeneszolg�lat�ban r�sztvesz
      J_S = 27 + Int(J / 3)
      J_O = 2 + (J Mod 3)
      Cells(J_S, J_O).Value = Lap.Cells(I, 1) & " " & Lap.Cells(I, 2)
      If Lap.Cells(I, 4) = 3 Then ' A H�tv�ge zeneszolg�lat�nak vezet�je
      Cells(J_S, J_O).Font.Underline = xlUnderlineStyleSingle
      End If
      J = J + 1
    End If
      
    K_S = 9 + K Mod L_O_DB
    K_O = 2 + Int(K / L_O_DB)
    Cells(K_S, K_O).Value = Lap.Cells(I, 1) & " " & Lap.Cells(I, 2)
    K = K + 1
  End If
Next I

Cells(6, 2).Value = VS

End Sub
Function OK(S As String) As Boolean

Dim L_N As Integer
Dim I As Integer
Dim Lapn�v As String

Lapn�v = S
L_N = Sheets.Count

OK = True

For I = 1 To L_N
  If Sheets(I).Name = Lapn�v Then
    OK = False
  End If
Next I
End Function
Sub T�r�l()
Attribute T�r�l.VB_ProcData.VB_Invoke_Func = "T\n14"
  
  ' Hiv�sa Ctrl+Shift+T
  
  Dim I As Integer
  Application.DisplayAlerts = False
  For I = Sheets.Count To 9 Step (-1)
    Sheets(I).Delete
  Next
End Sub

