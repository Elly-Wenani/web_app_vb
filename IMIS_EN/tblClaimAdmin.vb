'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated from a template.
'
'     Manual changes to this file may cause unexpected behavior in your application.
'     Manual changes to this file will be overwritten if the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic

Partial Public Class tblClaimAdmin
    Public Property ClaimAdminId As Integer
    Public Property ClaimAdminCode As String
    Public Property LastName As String
    Public Property OtherNames As String
    Public Property DOB As Nullable(Of Date)
    Public Property Phone As String
    Public Property HFId As Nullable(Of Integer)
    Public Property ValidityFrom As Nullable(Of Date)
    Public Property ValidityTo As Nullable(Of Date)
    Public Property LegacyId As Nullable(Of Integer)
    Public Property AuditUserId As Nullable(Of Integer)
    Public Property RowId As Byte()
    Public Property EmailId As String

    Public Overridable Property tblClaim As ICollection(Of tblClaim) = New HashSet(Of tblClaim)
    Public Overridable Property tblHF As tblHF

End Class