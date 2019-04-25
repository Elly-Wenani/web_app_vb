''Copyright (c) 2016-2017 Swiss Agency for Development and Cooperation (SDC)
''
''The program users must agree to the following terms:
''
''Copyright notices
''This program is free software: you can redistribute it and/or modify it under the terms of the GNU AGPL v3 License as published by the 
''Free Software Foundation, version 3 of the License.
''This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
''MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU AGPL v3 License for more details www.gnu.org.
''
''Disclaimer of Warranty
''There is no warranty for the program, to the extent permitted by applicable law; except when otherwise stated in writing the copyright 
''holders and/or other parties provide the program "as is" without warranty of any kind, either expressed or implied, including, but not 
''limited to, the implied warranties of merchantability and fitness for a particular purpose. The entire risk as to the quality and 
''performance of the program is with you. Should the program prove defective, you assume the cost of all necessary servicing, repair or correction.
''
''Limitation of Liability 
''In no event unless required by applicable law or agreed to in writing will any copyright holder, or any other party who modifies and/or 
''conveys the program as permitted above, be liable to you for damages, including any general, special, incidental or consequential damages 
''arising out of the use or inability to use the program (including but not limited to loss of data or data being rendered inaccurate or losses 
''sustained by you or third parties or a failure of the program to operate with any other programs), even if such holder or other party has been 
''advised of the possibility of such damages.
''
''In case of dispute arising out or in relation to the use of the program, it is subject to the public law of Switzerland. The place of jurisdiction is Berne.
'
' 
'

Partial Public Class Officer
    Inherits System.Web.UI.Page
    Dim eOfficer As New IMIS_EN.tblOfficer
    Dim Officer As New IMIS_BI.OfficerBI
    Private eUsers As New IMIS_EN.tblUsers
    Public imisgen As New IMIS_Gen
    Private BIOfficer As New IMIS_BI.OfficerBI

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        RunPageSecurity()
        lblmsg.Text = ""
        eOfficer.OfficerID = HttpContext.Current.Request.QueryString("o")
        eUsers.UserID = imisgen.getUserId(Session("User"))
        If IsPostBack = True Then
            If Request.Params.Get("__EVENTARGUMENT").ToString = "Delete" Then
                DeleteLogin()
            End If
            Return
        End If

        Try
            Dim dtRegions As DataTable = Officer.GetRegions(imisgen.getUserId(Session("User")), True)
            ddlRegion.DataSource = dtRegions
            ddlRegion.DataValueField = "RegionId"
            ddlRegion.DataTextField = "RegionName"
            ddlRegion.DataBind()
            If dtRegions.Rows.Count = 1 Then
                FillDistrict()
            End If
            FillLanguage()

            If Not eOfficer.OfficerID = 0 Then
                Officer.LoadOfficer(eOfficer)
                txtCode.Text = eOfficer.Code
                txtLastName.Text = eOfficer.LastName
                txtOtherNames.Text = eOfficer.OtherNames
                txtDob.Text = If(eOfficer.DOB Is Nothing, "", eOfficer.DOB)
                txtPhone.Text = If(eOfficer.Phone Is Nothing, "", eOfficer.Phone)
                ddlRegion.SelectedValue = eOfficer.LocationId1
                FillDistrict()
                ddlDistrict.SelectedValue = eOfficer.tblLocations.LocationId
                txtpermaddress.Text = eOfficer.PermanentAddress
                txtEmail.Text = eOfficer.EmailId
                ddlSubstitution.SelectedValue = eOfficer.tblOfficer2.OfficerID
                txtWorksTo.Text = If(txtWorksTo.Text Is Nothing, "", eOfficer.WorksTo)

                txtVeoCode.Text = If(eOfficer.VEOCode Is Nothing, "", eOfficer.VEOCode)
                txtVeoLastName.Text = If(eOfficer.VEOLastName Is Nothing, "", eOfficer.VEOLastName)
                txtVeoOtherName.Text = If(eOfficer.VEOOtherNames Is Nothing, "", eOfficer.VEOOtherNames)
                txtVeoDOB.Text = If(eOfficer.VEODOB Is Nothing, "", eOfficer.VEODOB)
                txtVeoPhone.Text = If(eOfficer.VEOPhone Is Nothing, "", eOfficer.VEOPhone)
                chkCommunicate.Checked = If(eOfficer.PhoneCommunication Is Nothing, False, eOfficer.PhoneCommunication)

                If eOfficer.HasLogin = True Then
                    eUsers.LoginName = eOfficer.Code
                    eUsers.UserID = 0
                    eOfficer.eUsers = eUsers
                    BIOfficer.LoadUsers(eOfficer.eUsers)
                    hfUserID.Value = eOfficer.eUsers.UserID
                    chkOfficerIncludeLogin.Checked = True
                    ddlLanguage.SelectedValue = eUsers.LanguageID
                End If
            Else
                hfUserID.Value = 0
            End If

            FillWards()
            fillVillages()
            If HttpContext.Current.Request.QueryString("r") = 1 Or eOfficer.ValidityTo.HasValue Then
                Panel2.Enabled = False
                B_SAVE.Visible = False
            End If
        Catch ex As Exception
            imisgen.Alert(imisgen.getMessage("M_ERRORMESSAGE"), pnlVeoOfficer, alertPopupTitle:="IMIS")
            EventLog.WriteEntry("IMIS", Page.Title & " : " & imisgen.getLoginName(Session("User")) & " : " & ex.Message, EventLogEntryType.Error, 999)
        End Try

    End Sub

    Private Function SetLoginDetails() As Boolean
        eOfficer.eUsers = New IMIS_EN.tblUsers
        If hfUserID.Value <> "" Then
            eOfficer.eUsers.UserID = hfUserID.Value
        End If
        If eOfficer.eUsers.UserID > 0 Then
            BIOfficer.LoadUsers(eOfficer.eUsers)
            eOfficer.eUsers.LoginName = txtCode.Text
        Else
            eOfficer.eUsers.LoginName = txtCode.Text
            BIOfficer.LoadUsers(eOfficer.eUsers)
        End If
        eOfficer.eUsers.AuditUserID = eOfficer.AuditUserID
        eOfficer.eUsers.LastName = txtLastName.Text
        eOfficer.eUsers.OtherNames = txtOtherNames.Text
        If txtPassword.Text.Length > 0 Then
            eOfficer.eUsers.DummyPwd = txtPassword.Text
            eOfficer.eUsers.DummyPwd = txtConfirmPassword.Text
        End If

        eOfficer.eUsers.Phone = txtPhone.Text
        eOfficer.eUsers.EmailId = txtEmail.Text
        eOfficer.eUsers.LanguageID = ddlLanguage.SelectedValue
        eOfficer.eUsers.RoleID = 1
        eOfficer.eUsers.AuditUserID = imisgen.getUserId(Session("User"))
        eOfficer.eUsers.IsAssociated = True
        eOfficer.HasLogin = 1
        Return True
    End Function

    Private Sub FillDistrict()
        Dim dtDistricts As DataTable = Officer.GetDistricts(imisgen.getUserId(Session("User")), True, ddlRegion.SelectedValue)
        ddlDistrict.DataSource = dtDistricts
        ddlDistrict.DataValueField = "DistrictID"
        ddlDistrict.DataTextField = "DistrictName"
        ddlDistrict.DataBind()

        If dtDistricts.Rows.Count > 0 Then
            Dim dtOfficers As DataTable = Officer.GetSubstitutionOfficer(Request.QueryString("o"))
            If dtOfficers.Rows.Count > 0 Then
                ddlSubstitution.DataSource = dtOfficers
                ddlSubstitution.DataValueField = "OfficerID"
                ddlSubstitution.DataTextField = "Code"
                ddlSubstitution.DataBind()
            End If

        End If
    End Sub
    Private Sub FillWards()
        If Not Val(ddlDistrict.SelectedValue) > 0 Then Exit Sub
        Dim dtWards As DataTable = Officer.GetWards(Val(ddlDistrict.SelectedValue), eOfficer.OfficerID)
        gvWards.DataSource = dtWards
        gvWards.DataBind()
        SetCheckboxes(gvWards)
        fillVillages()
    End Sub
    Private Sub fillVillages()
        Dim dtVillages As DataTable = Officer.GetVillages(Val(ddlDistrict.SelectedValue), eOfficer.OfficerID)
        gvVillage.DataSource = dtVillages
        gvVillage.DataBind()
        SetCheckboxes(gvVillage)
    End Sub
    Private Sub FillLanguage()
        eUsers.UserID = imisgen.getUserId(Session("User"))
        ddlLanguage.DataSource = BIOfficer.GetLanguage
        ddlLanguage.DataValueField = "LanguageCode"
        ddlLanguage.DataTextField = "LanguageName"
        ddlLanguage.DataBind()
    End Sub

    Private Sub SetCheckboxes(gv As GridView)
        Dim _CheckWards As Boolean = True
        Dim _CheckVillages As Boolean = True

        For Each row As GridViewRow In gv.Rows
            Dim chkSelect As CheckBox = CType(row.Cells(0).Controls(1), CheckBox)

            Select Case gv.ID
                Case gvWards.ID
                    chkSelect.Checked = gvWards.DataKeys(row.RowIndex).Value
                    If chkSelect.Checked <> True Then
                        _CheckWards = False
                    End If

                Case gvVillage.ID
                    chkSelect.Checked = gvVillage.DataKeys(row.RowIndex).Value
                    If chkSelect.Checked <> True Then
                        _CheckVillages = False
                    End If
            End Select
        Next

        Select Case gv.ID
            Case gvWards.ID
                chkCheckAllWards.Checked = _CheckWards
            Case gvVillage.ID
                chkCheckAllVillages.Checked = _CheckVillages
        End Select

    End Sub
    Private Sub RunPageSecurity()
        Dim RefUrl = Request.Headers("Referer")
        Dim RoleID As Integer = imisgen.getRoleId(Session("User"))
        Dim userBI As New IMIS_BI.UserBI
        If userBI.RunPageSecurity(IMIS_EN.Enums.Pages.Officer, Page) Then
            Dim Add As Boolean = userBI.CheckRoles(IMIS_EN.Enums.Rights.AddOfficer, RoleID)
            Dim Edit As Boolean = userBI.CheckRoles(IMIS_EN.Enums.Rights.EditOfficer, RoleID)

            If Not Add And Not Edit Then
                B_SAVE.Visible = False
                Panel2.Enabled = False
                Panel2.Enabled = False
                pnlVeoOfficer.Enabled = False
            End If
        Else
            Server.Transfer("Redirect.aspx?perm=0&page=" & IMIS_EN.Enums.Pages.Officer.ToString & "&retUrl=" & RefUrl)
        End If
    End Sub
    Public Sub DeleteLogin()
        Try
            eUsers.UserID = hfUserID.Value
            eOfficer.eUsers = eUsers
            BIOfficer.LoadUsers(eOfficer.eUsers)
            BIOfficer.DeleteUser(eOfficer.eUsers)
            eOfficer.HasLogin = False
            If Not eOfficer.OfficerID = 0 Then
                SaveOfficer()
            End If
            Session("msg") = txtCode.Text & " " & imisgen.getMessage("M_DELETED")
        Catch ex As Exception
            imisgen.Alert(imisgen.getMessage("M_ERRORMESSAGE"), pnlVeoOfficer, alertPopupTitle:="IMIS")
            EventLog.WriteEntry("IMIS", Page.Title & " : " & imisgen.getLoginName(Session("User")) & " : " & ex.Message, EventLogEntryType.Error, 999)
        End Try

    End Sub

    Protected Sub B_SAVE_Click(ByVal sender As Object, ByVal e As EventArgs) Handles B_SAVE.Click
        SaveOfficer()
        Response.Redirect("FindOfficer.aspx?o=" & txtCode.Text)
    End Sub

    Public Sub SaveOfficer()
        'If CType(Me.Master.FindControl("hfDirty"), HiddenField).Value = True Then
        Try
            Dim dt As New DataTable
            dt = DirectCast(Session("User"), DataTable)

            Dim eLocations As New IMIS_EN.tblLocations

            Dim LocationId As Integer = -1
            If Val(ddlDistrict.SelectedValue) > 0 Then
                LocationId = ddlDistrict.SelectedValue
            ElseIf Val(ddlRegion.SelectedValue) > 0 Then
                LocationId = ddlRegion.SelectedValue
            Else
                LocationId = -1
            End If

            eLocations.LocationId = LocationId
            eOfficer.Code = txtCode.Text
            eOfficer.LastName = txtLastName.Text
            eOfficer.OtherNames = txtOtherNames.Text

            If Not txtDob.Text.Length = 0 Then
                eOfficer.DOB = Date.Parse(txtDob.Text)
            End If

            eOfficer.EmailId = txtEmail.Text
            eOfficer.PermanentAddress = txtpermaddress.Text

            eOfficer.Phone = txtPhone.Text
            eOfficer.tblLocations = eLocations
            eOfficer.VEOCode = txtVeoCode.Text
            eOfficer.VEOLastName = txtVeoLastName.Text
            eOfficer.VEOOtherNames = txtVeoOtherName.Text
            eOfficer.VEOPhone = txtVeoPhone.Text
            eOfficer.PhoneCommunication = chkCommunicate.Checked
            If Not txtVeoDOB.Text.Length = 0 Then
                eOfficer.VEODOB = Date.Parse(txtVeoDOB.Text)
            End If
            Dim eofficer2 As New IMIS_EN.tblOfficer

            If Not ddlSubstitution.SelectedValue = "" Then
                eofficer2.OfficerID = ddlSubstitution.SelectedValue
            End If
            If txtWorksTo.Text.Length > 0 Then
                eOfficer.WorksTo = Date.Parse(txtWorksTo.Text)
            End If

            eOfficer.AuditUserID = imisgen.getUserId(Session("User"))
            eOfficer.tblOfficer2 = eofficer2

            Dim dtData As DataTable = GetOfficersVillagesDT()
            If chkOfficerIncludeLogin.Checked = True Then
                If SetLoginDetails() = False Then
                    Exit Sub
                End If
            End If
            Dim chk As Integer = Officer.SaveOfficer(eOfficer, dtData)
            If chk = 0 Then
                Session("msg") = eOfficer.Code & " " & eOfficer.LastName & imisgen.getMessage("M_Inserted")

            ElseIf chk = 1 Then
                imisgen.Alert(eOfficer.Code & imisgen.getMessage("M_Exists"), pnlButtons, alertPopupTitle:="IMIS")
                Return
            Else
                Session("msg") = eOfficer.Code & " " & eOfficer.LastName & imisgen.getMessage("M_Updated")
            End If

        Catch ex As Exception
            imisgen.Alert(imisgen.getMessage("M_ERRORMESSAGE"), pnlVeoOfficer, alertPopupTitle:="IMIS")
            EventLog.WriteEntry("IMIS", Page.Title & " : " & imisgen.getLoginName(Session("User")) & " : " & ex.Message, EventLogEntryType.Error, 999)
            Return
        End Try
    End Sub

    Private Function GetOfficersVillagesDT() As DataTable
        Dim dtData As New DataTable
        With dtData
            .Columns.Add("OfficerId", GetType(Integer))
            .Columns.Add("VillageId", GetType(Integer))
            .Columns.Add("AuditUserId", GetType(Integer))
            .Columns.Add("Action")
        End With
        Dim UserId As Integer = imisgen.getUserId(Session("User"))

        For Each row As GridViewRow In gvVillage.Rows
            Dim chkSelect As CheckBox = CType(gvVillage.Rows(row.RowIndex).Cells(0).Controls(1), CheckBox)
            If eOfficer.OfficerID = 0 Or chkSelect.Checked <> CBool(gvVillage.DataKeys(gvVillage.Rows(row.RowIndex).RowIndex).Value) Then
                Dim Action As String = If(chkSelect.Checked, "I", "D")
                Dim VillageId As Integer = gvVillage.DataKeys(gvVillage.Rows(row.RowIndex).RowIndex)("VillageID")
                dtData.Rows.Add(New Object() {eOfficer.OfficerID, VillageId, UserId, Action})
            End If
        Next

        Return dtData
    End Function

    Private Sub B_CANCEL_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles B_CANCEL.Click
        Response.Redirect("FindOfficer.aspx?o=" & txtCode.Text)
    End Sub

    Private Sub ddlDistrict_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlDistrict.SelectedIndexChanged
        FillWards()
    End Sub

    Private Sub ddlRegion_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlRegion.SelectedIndexChanged
        FillDistrict()
    End Sub
End Class