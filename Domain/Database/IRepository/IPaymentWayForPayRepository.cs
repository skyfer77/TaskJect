namespace Domain.Database
{
	public interface IPaymentWayForPayRepository
	{
		Task<PaymentWayForPayDto> FindByOrderReferenceAsync(string orderReference);
		Task<bool> InsertAsync(PaymentWayForPayDto paymentDto);
		Task<bool> UpdateAsync(PaymentWayForPayDto paymentDto);
		Task<bool> DeleteAsync(Guid id);
	}
}
