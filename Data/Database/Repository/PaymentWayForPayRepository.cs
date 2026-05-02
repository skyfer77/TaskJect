using AutoMapper;
using Data.DbContexts;
using Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
	public class PaymentWayForPayRepository : IPaymentWayForPayRepository
	{
		private readonly ApplicationDbContext _dbContext;
		private IMapper _mapper;
		public PaymentWayForPayRepository(ApplicationDbContext dbContext, IMapper mapper)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		public async Task<PaymentWayForPayDto> FindByOrderReferenceAsync(string orderReference)
		{
			var payment = await _dbContext.PaymentWayForPays
				.Where(x => x.OrderReference == orderReference)
				.FirstOrDefaultAsync();

			return _mapper.Map<PaymentWayForPayDto>(payment);
		}

		public async Task<bool> InsertAsync(PaymentWayForPayDto paymentDto)
		{
			var payment = _mapper.Map<PaymentWayForPayDto, PaymentWayForPay>(paymentDto);
			try
			{
				await _dbContext.PaymentWayForPays.AddAsync(payment);
				await _dbContext.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}
		//TODO
		public async Task<bool> UpdateAsync(PaymentWayForPayDto paymentDto)
		{
			var existingPayment = await _dbContext.PaymentWayForPays
				.FirstOrDefaultAsync(t => t.Id == paymentDto.Id);

			if (existingPayment == null)
			{
				return false;
			}

			_mapper.Map(paymentDto, existingPayment);

			await _dbContext.SaveChangesAsync();
			return true;
		}
		//TODO
		public async Task<bool> DeleteAsync(Guid id)
		{
			var payment = await _dbContext.PaymentWayForPays
			   .FirstOrDefaultAsync(x => x.Id == id);
			if (payment == null)
			{
				return false;
			}
			try
			{
				_dbContext.PaymentWayForPays.Remove(payment);
				await _dbContext.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
