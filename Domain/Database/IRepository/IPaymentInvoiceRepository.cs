namespace Domain.Database
{
	public interface IPaymentInvoiceRepository
	{
		Task<PaymentInvoiceDto> FindAsync(Guid paymentWayForPayId);
		Task<bool> InsertAsync(PaymentInvoiceDto paymentInvoiceDto);
		Task<bool> DeleteAsync(Guid id);
	}
}
