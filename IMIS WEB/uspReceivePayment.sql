USE [IMIS_MASTER]
GO
/****** Object:  StoredProcedure [dbo].[uspReceivePayment]    Script Date: 05/04/2019 16:25:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[uspReceivePayment]
(
	
	@XML XML,
	@Payment_ID BIGINT =NULL OUTPUT
)
AS
BEGIN

	DECLARE
	@PaymentID BIGINT =NULL,
	@ControlNumberID BIGINT=NULL,
	@PaymentDate DATE,
	@ReceiveDate DATE,
    @ControlNumber NVARCHAR(50),
    @TransactionNo NVARCHAR(50),
    @ReceiptNo NVARCHAR(100),
    @PaymentOrigin NVARCHAR(50),
    @PhoneNumber NVARCHAR(50),
    @OfficerCode NVARCHAR(50),
    @InsuranceNumber NVARCHAR(12),
    @productCode NVARCHAR(8),
    @Amount  DECIMAL(18,2),
    @isRenewal  BIT,
	@ExpectedAmount DECIMAL(18,2),
	@ErrorMsg NVARCHAR(50)
	

		

	
				
	BEGIN TRY

		DECLARE @tblDetail TABLE(InsuranceNumber nvarchar(12),productCode nvarchar(8), isRenewal BIT)

		SELECT @PaymentID = NULLIF(T.H.value('(PaymentID)[1]','INT'),''),
			   @PaymentDate = NULLIF(T.H.value('(PaymentDate)[1]','DATE'),''),
			   @ReceiveDate = NULLIF(T.H.value('(ReceiveDate)[1]','DATE'),''),
			   @ControlNumber = NULLIF(T.H.value('(ControlNumber)[1]','NVARCHAR(50)'),''),
			   @ReceiptNo = NULLIF(T.H.value('(ReceiptNo)[1]','NVARCHAR(100)'),''),
			   @TransactionNo = NULLIF(T.H.value('(TransactionNo)[1]','NVARCHAR(50)'),''),
			   @PaymentOrigin = NULLIF(T.H.value('(PaymentOrigin)[1]','NVARCHAR(50)'),''),
			   @PhoneNumber = NULLIF(T.H.value('(PhoneNumber)[1]','NVARCHAR(25)'),''),
			   @OfficerCode = NULLIF(T.H.value('(OfficerCode)[1]','NVARCHAR(50)'),''),
			   @Amount = T.H.value('(Amount)[1]','DECIMAL(18,2)')
		FROM @XML.nodes('PaymentData') AS T(H)
	

		DECLARE @MyAmount Decimal(18,2)
		DECLARE @MyControlNumber nvarchar(50)
		DECLARE @MyReceivedAmount decimal(18,2) = 0 
		DECLARE @MyStatus INT = 0 
		DECLARE @ResultCode AS INT = 0 

		INSERT INTO @tblDetail(InsuranceNumber, productCode, isRenewal)
		SELECT 
		LEFT(NULLIF(T.D.value('(InsureeNumber)[1]','NVARCHAR(12)'),''),12),
		LEFT(NULLIF(T.D.value('(ProductCode)[1]','NVARCHAR(8)'),''),8),
		T.D.value('(IsRenewal)[1]','BIT')
		FROM @XML.nodes('PaymentData/Detail') AS T(D)
	
		--VERIFICATION START
		IF ISNULL(@PaymentID,'') <> ''
		BEGIN
			--lets see if all element are matching

			SELECT @MyControlNumber = ControlNumber , @MyAmount = P.ExpectedAmount, @MyReceivedAmount = ISNULL(ReceivedAmount,0) , @MyStatus = PaymentStatus  from tblPayment P left outer join tblControlNumber CN ON P.PaymentID = CN.PaymentID  WHERE CN.ValidityTo IS NULL and P.ValidityTo IS NULL AND P.PaymentID = @PaymentID 


			--CONTROl NUMBER CHECK
			IF ISNULL(@MyControlNumber,'') <> ISNULL(@ControlNumber,'') 
			BEGIN
				--Control Nr mismatch
				SET @ErrorMsg = 'Wrong Control Number'
				SET @ResultCode = 3
			END 

			--AMOUNT VALE CHECK
			IF ISNULL(@MyAmount,0) <> ISNULL(@Amount ,0) 
			BEGIN
				--Amount mismatch
				SET @ErrorMsg = 'Wrong Payment Amount'
				SET @ResultCode = 4
			END 

			--DUPLICATION OF PAYMENT
			IF @MyReceivedAmount = @Amount 
			BEGIN
				SET @ErrorMsg = 'Duplicated Payment'
				SET @ResultCode = 5
			END

			
		END
		--VERIFICATION END

		IF @ResultCode <> 0
		BEGIN
			--ERROR OCCURRED
	
			INSERT INTO [dbo].[tblPayment]
			(PaymentDate, ReceivedDate, ReceivedAmount, ReceiptNo, TransactionNo, PaymentOrigin, PhoneNumber, PaymentStatus, OfficerCode, ValidityFrom, AuditedUSerID, RejectedReason) 
			VALUES (@PaymentDate, @ReceiveDate,  @Amount, @ReceiptNo, @TransactionNo, @PaymentOrigin, @PhoneNumber, -3, @OfficerCode,  GETDATE(), -1,@ErrorMsg)
			SET @PaymentID= SCOPE_IDENTITY();

			INSERT INTO [dbo].[tblPaymentDetails]
				([PaymentID],[ProductCode],[InsuranceNumber],[PolicyStage],[ValidityFrom],[AuditedUserId]) SELECT
				@PaymentID, productCode, InsuranceNumber,  CASE isRenewal WHEN 0 THEN 'N' ELSE 'R' END, GETDATE(), -1
				FROM @tblDetail D

			SET @Payment_ID = @PaymentID
			SELECT @Payment_ID
			RETURN @ResultCode

		END
		ELSE
		BEGIN
			--ALL WENT OK SO FAR
		
		
			IF ISNULL(@PaymentID ,0) <> 0 
			BEGIN
				--REQUEST/INTEND WAS FOUND
				UPDATE tblPayment SET ReceivedAmount = @Amount, PaymentDate = @PaymentDate, ReceivedDate = GETDATE(), PaymentStatus = 4, TransactionNo = @TransactionNo, ReceiptNo= @ReceiptNo, PaymentOrigin = @PaymentOrigin, ValidityFrom = GETDATE(),AuditedUserID =-1 WHERE PaymentID = @PaymentID  AND ValidityTo IS NULL AND PaymentStatus = 3
				SET @Payment_ID = @PaymentID
				RETURN 0 
			END
			ELSE
			BEGIN
				--PAYMENT WITHOUT INTEND TP PAY
				INSERT INTO [dbo].[tblPayment]
					(PaymentDate, ReceivedDate, ReceivedAmount, ReceiptNo, TransactionNo, PaymentOrigin, PhoneNumber, PaymentStatus, OfficerCode, ValidityFrom, AuditedUSerID) 
					VALUES (@PaymentDate, @ReceiveDate,  @Amount, @ReceiptNo, @TransactionNo, @PaymentOrigin, @PhoneNumber, 4, @OfficerCode,  GETDATE(), -1)
				SET @PaymentID= SCOPE_IDENTITY();
							
				INSERT INTO [dbo].[tblPaymentDetails]
				([PaymentID],[ProductCode],[InsuranceNumber],[PolicyStage],[ValidityFrom],[AuditedUserId]) SELECT
				@PaymentID, productCode, InsuranceNumber,  CASE isRenewal WHEN 0 THEN 'N' ELSE 'R' END, GETDATE(), -1
				FROM @tblDetail D
				SET @Payment_ID = @PaymentID
				RETURN 0 

			END
		 
		
			
		END

		RETURN 0

	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE()
		RETURN -1
	END CATCH
	

	
END

