Attribute VB_Name = "Module1"
Option Explicit
Sub Összes()
Attribute Összes.VB_ProcData.VB_Invoke_Func = "O\n14"
  Call Kitûzõ_készítõ
  Call Megosztó_csoport_készítõ
  Call Alvó_csoport_készítõ
  Call Záró_elõlap_készítõ
End Sub
Sub Kitûzõ_készítõ()
Attribute Kitûzõ_készítõ.VB_ProcData.VB_Invoke_Func = "K\n14"
'
' Kitûzõ_készítõ Makro
' Rögzítette: Kiss László, dátum: 2009.11.23.
'
' Billentyûparancs: Ctrl+Shift+K
'

If Not OK("Kitûzõ1") Then
  Exit Sub
End If

Dim N As Integer ' A létszám
Dim Lap As Object
Set Lap = Sheets("Alapadatok")
Lap.Unprotect
N = Lap.Cells(1, 1).CurrentRegion.Rows.Count - 1
Lap.Protect
Dim M As Integer, DB As Integer ' A kitûzõoldalak és az oldalakon lévõ kitûzõk száma.
                                ' Utóbbi mindig páros!

Dim I As Integer, J As Integer
Dim K As Integer, L As Integer  ' A lehetséges résztvevõk feldolgozási sorszáma és
                                ' a kitûzõ lapon a sorindex.

DB = 10
M = Int(N / DB)
If N > M * DB Then
  M = M + 1
End If

For I = 1 To M   ' I az aktuálisan generált kitûzõoldal sorszáma
  Sheets("Kitûzõ_alap").Copy After:=Sheets(Sheets.Count)
  Sheets("Kitûzõ_alap (2)").Name = "Kitûzõ" & I
  ActiveSheet.Unprotect
  
  For J = 1 To DB / 2   ' Az oldalon belül két kitûzõ van soronként, J a "sor" száma
    ' A soron belüli elsõ kitûzõ generálása
    K = (I - 1) * DB + J * 2   ' A személyt tartalmazó sor száma az "Alapadatok" lapon
    L = (J - 1) * 5 + 1        ' A kitûzõoldalon az aktuális kitûzõhõz tartozó Excel sor száma
    If IsEmpty(Lap.Cells(K, 3).Value) Then   ' Nincs beceneve
     Cells(L, 1).Value = Lap.Cells(K, 1).Value
     Cells(L + 1, 1).Value = " " + Lap.Cells(K, 2).Value
    Else                                     ' Van beceneve
      Cells(L, 1).Value = Lap.Cells(K, 1).Value + " " + Lap.Cells(K, 2).Value
      Cells(L + 1, 1).Value = " " + Lap.Cells(K, 3).Value
    End If
    ' Megjegyzés
    If Not IsEmpty(Lap.Cells(K, 9).Value) Then
        Cells(L + 2, 1).Value = "(" + Lap.Cells(K, 9).Value + ")"
        Cells(L + 2, 1).Font.Size = 8
        Cells(L + 2, 1).VerticalAlignment = xlCenter
        Cells(L + 2, 1).HorizontalAlignment = xlRight
    End If
    ' Kiscsoport és alvócsoport száma
    Cells(L + 3, 1).Value = " " & Lap.Cells(K, 5).Value & "   " & Lap.Cells(K, 7).Value
    
    ' A soron belüli második kitûzõ generálása
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

Sub Megosztó_csoport_készítõ()
Attribute Megosztó_csoport_készítõ.VB_ProcData.VB_Invoke_Func = "M\n14"
'
' Megosztó_csoport_készítõ Makro
' Rögzítette: Kiss László, dátum: 2009.11.23.
'
' Billentyûparancs: Ctrl+Shift+M
'

If Not OK("Megosztócsoport1") Then
  Exit Sub
End If

Dim V_lap As Object, Lap As Object

Dim K_nev As String   ' A közösség beve.
Dim H_N As Integer    ' A hétvége sorszáma.
Dim H_D As String     ' A hétvége dátuma.
Dim H_H As String     ' A hétvége helyszíne.
Dim H_HC As String    ' A hétvége helyszínének címe.
Dim N As Integer      ' A hétvégén a lehetséges résztvevõk létszáma.
Dim MCs_N As Integer  ' A megosztó csoportok száma.
Dim MCs_I As Integer  ' Az éppen feldolgozott megosztó csoport indexe.
Dim I As Integer

Set V_lap = Sheets("Vezérlõ adatok")
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

Dim M As Integer, DB As Integer ' A megosztó csoportokat tartalmazó oldalak száma
                                ' és az oldalakon lévõ csoportok lehetséges száma.
                                ' Utóbbi mindig páros!
Dim J As Integer
Dim K As Integer, L As Integer

With Sheets("Megosztócsoport_alap").PageSetup
    .LeftHeader = ""
    .CenterHeader = _
      "&""Monotype Corsiva,Normál""&26MEGOSZTÓ CSOPORTOK&12" & Chr(10) & _
      "&14" & Str(H_N) & ". " & K_nev & " Antióchia-hétvége, " & H_D & Chr(10) _
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
  Sheets("Megosztócsoport_alap").Copy After:=Sheets(Sheets.Count)
  Sheets("Megosztócsoport_alap (2)").Name = "Megosztócsoport" & I
  ActiveSheet.Unprotect
  
  For J = 1 To DB
    MCs_I = (I - 1) * DB + J
    
    If MCs_I > MCs_N Then
      Exit For
    End If
    
    Call Egy_megosztó_csoport_feldolgozása(Lap, N, MCs_I, DB)
  Next J
Next I

End Sub

Sub Egy_megosztó_csoport_feldolgozása(Lap As Object, N As Integer, MCs_I As Integer, DB As Integer)
  
Dim I As Integer, J As Integer, K As Integer, L As Integer
Dim S_Cspv As Integer, O_Cspv As Integer
Dim Cs_M As Integer ' Egy csoport maximális létszáma (a Megosztócsoport_alap lapon!).
Cs_M = 7
K = 0 ' Az adott csoportból éppen feldolgozott tag indexe.
L = MCs_I Mod DB ' A csoport sorszáma az adott lapon.
If L = 0 Then
  L = DB
End If
S_Cspv = 1 + Int((L - 1) / 2) * Cs_M    ' A csoportvezetõ sorindexe.
O_Cspv = 1 + ((L - 1) Mod 2)            ' A csoportvezetõ soszlopindexe.

For I = 2 To N + 1  ' I az "Alapadatok" lapon az éppen feldolgozott tag adatainak sorindexe.
  If Lap.Cells(I, 5) = MCs_I Then
    If Lap.Cells(I, 6) = MCs_I Then    ' A csoport vezetõje.
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

Sub Alvó_csoport_készítõ()
Attribute Alvó_csoport_készítõ.VB_ProcData.VB_Invoke_Func = "A\n14"
'
' Alvó_csoport_készítõ Makro
' Rögzítette: Kiss László, dátum: 2009.11.27.
'
' Billentyûparancs: Ctrl+Shift+A
'

If Not OK("Alvócsoport1") Then
  Exit Sub
End If

Dim V_lap As Object, A_lap As Object, Lap As Object

Dim K_nev As String   ' A közösség beve.
Dim H_N As Integer    ' A hétvége sorszáma.
Dim H_D As String     ' A hétvége dátuma.
Dim H_H As String     ' A hétvége helyszíne.
Dim H_HC As String    ' A hétvége helyszínének címe.
Dim N As Integer      ' A hétvégén a lehetséges résztvevõk létszáma.
Dim ACs_N As Integer  ' Az alvócsoportok száma.
Dim ACs_I As Integer  ' Az éppen feldolgozott alvócsoport indexe.
Dim I As Integer

Set V_lap = Sheets("Vezérlõ adatok")
Set A_lap = Sheets("Alvócsoport címek")
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

Dim M As Integer, DB As Integer ' Az alvócsoportokat tartalmazó oldalak száma
                                ' és az oldalakon lévõ csoportok lehetséges száma.
Dim J As Integer
Dim K As Integer, L As Integer

With Sheets("Alvócsoport_alap").PageSetup
    .LeftHeader = ""
    .CenterHeader = _
      "&""Monotype Corsiva,Normál""&26ALVÓCSOPORTOK&12" & Chr(10) & _
      "&14" & Str(H_N) & ". " & K_nev & " Antióchia-hétvége, " & H_D & Chr(10) _
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
  Sheets("Alvócsoport_alap").Copy After:=Sheets(Sheets.Count)
  Sheets("Alvócsoport_alap (2)").Name = "Alvócsoport" & I
  ActiveSheet.Unprotect
  
  For J = 1 To DB
    ACs_I = (I - 1) * DB + J
    
    If ACs_I > ACs_N Then
      Exit For
    End If
    
    Call Egy_alvó_csoport_feldolgozása(Lap, A_lap, N, ACs_I, DB)
  Next J
Next I

End Sub

Sub Egy_alvó_csoport_feldolgozása(Lap As Object, A_lap As Object, N As Integer, ACs_I As Integer, DB As Integer)
  
Dim I As Integer, J As Integer, K As Integer, L As Integer
Dim S_Csp As Integer
Dim ACsC_N As Integer, ACs_Char As String ' Az alvócsoportcímek száma és betûjele.
Dim Cs_A As Integer ' Egy csoport maximális létszáma - 1 (az Alvócsoport_alap lapon!).

Cs_A = 5
K = 0 ' Az adott csoportból éppen feldolgozott tag indexe.
L = ACs_I Mod DB ' A csoport sorszáma az adott lapon.
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

For I = 2 To N + 1  ' I az "Alapadatok" lapon az éppen feldolgozott tag adatainak sorindexe.
  If Lap.Cells(I, 7) = ACs_Char Then
    If Lap.Cells(I, 8) = ACs_Char Then    ' A csoport vezetõje.
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

' Itt Cs_A-1 volt, de az hibás, mert a Cs_A alapból csoportlétszám-1 -- Marczell Márton
Range(Cells(S_Csp, 4), Cells(S_Csp + Cs_A, 4)).Select
    Selection.Sort Key1:=Cells(S_Csp, 4), Order1:=xlAscending, Header:=xlGuess, _
        OrderCustom:=1, MatchCase:=False, Orientation:=xlTopToBottom, _
        DataOption1:=xlSortNormal

End Sub

Sub Záró_elõlap_készítõ()
Attribute Záró_elõlap_készítõ.VB_ProcData.VB_Invoke_Func = "Z\n14"
    
'
' Záró_elõlap_készítõ Makro
' Rögzítette: Kiss László, dátum: 2009.11.23.
'
' Billentyûparancs: Ctrl+Shift+Z
'

If Not OK("Záró elõlap") Then
  Exit Sub
End If

Dim V_lap As Object, Lap As Object

Dim K_nev As String   ' A közösség beve.
Dim H_N As Integer    ' A hétvége sorszáma.
Dim H_D As String     ' A hétvége dátuma.
Dim H_H As String     ' A hétvége helyszíne.
Dim H_HC As String    ' A hétvége helyszínének címe.
Dim N As Integer      ' A hétvégén a lehetséges résztvevõk létszáma.
Dim VS As String      ' A vezetõket leíró string típusú változó
Dim I As Integer
Dim J As Integer, J_S As Integer, J_O As Integer
Dim K As Integer, K_S As Integer, K_O As Integer
Dim L As Integer, L_O_DB As Integer

Set V_lap = Sheets("Vezérlõ adatok")
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

Sheets("Záró_elõlap_alap").Copy After:=Sheets(Sheets.Count)
Sheets("Záró_elõlap_alap (2)").Name = "Záró elõlap"
ActiveSheet.Unprotect

Cells(1, 6) = Str(H_N) & ". " & K_nev & " Antióchia-hétvége, "
Cells(2, 6) = H_D
Cells(3, 6) = H_HC

L = 0
For I = 2 To N + 1  ' I az "Alapadatok" lapon az éppen feldolgozott tag adatainak sorindexe.
  
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

For I = 2 To N + 1  ' I az "Alapadatok" lapon az éppen feldolgozott tag adatainak sorindexe.
  
  If IsEmpty(Lap.Cells(I, 4)) Or Lap.Cells(I, 4) = 0 _
      Or Lap.Cells(I, 4) = 1 Or Lap.Cells(I, 4) = 2 _
      Or Lap.Cells(I, 4) = 3 Or Lap.Cells(I, 4) = 4 _
      Or Lap.Cells(I, 4) = 10 Then
      
    If Lap.Cells(I, 4) = 1 Then ' A Hétvége fiú vezetõje
      VS = VS & " & " & Lap.Cells(I, 1) & " " & Lap.Cells(I, 2)
    End If
      
    If Lap.Cells(I, 4) = 2 Then ' A Hétvége lány vezetõje
      VS = Lap.Cells(I, 1) & " " & Lap.Cells(I, 2) & VS
    End If
      
    If Lap.Cells(I, 4) = 3 Or Lap.Cells(I, 4) = 4 Then ' A Hétvége zeneszolgálatában résztvesz
      J_S = 27 + Int(J / 3)
      J_O = 2 + (J Mod 3)
      Cells(J_S, J_O).Value = Lap.Cells(I, 1) & " " & Lap.Cells(I, 2)
      If Lap.Cells(I, 4) = 3 Then ' A Hétvége zeneszolgálatának vezetõje
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
Dim Lapnév As String

Lapnév = S
L_N = Sheets.Count

OK = True

For I = 1 To L_N
  If Sheets(I).Name = Lapnév Then
    OK = False
  End If
Next I
End Function
Sub Töröl()
Attribute Töröl.VB_ProcData.VB_Invoke_Func = "T\n14"
  
  ' Hivása Ctrl+Shift+T
  
  Dim I As Integer
  Application.DisplayAlerts = False
  For I = Sheets.Count To 9 Step (-1)
    Sheets(I).Delete
  Next
End Sub

