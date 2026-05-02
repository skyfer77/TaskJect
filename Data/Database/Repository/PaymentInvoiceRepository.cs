using AutoMapper;
using Data.DbContexts;
using Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
	public class PaymentInvoiceRepository : IPaymentInvoiceRepository
	{
		private readonly ApplicationDbContext _dbContext;
		private IMapper _mapper;
		public PaymentInvoiceRepository(ApplicationDbContext dbContext, IMapper mapper)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		public async Task<PaymentInvoiceDto> FindAsync(Guid paymentWayForPayId)
		{
			var invoice = await _dbContext.PaymentInvoices
				.Where(x => x.PaymentWayForPayId == paymentWayForPayId)
				.FirstOrDefaultAsync();

			return _mapper.Map<PaymentInvoiceDto>(invoice);
		}

		public async Task<bool> InsertAsync(PaymentInvoiceDto paymentInvoiceDto)
		{
			var invoice = _mapper.Map<PaymentInvoiceDto, PaymentInvoice>(paymentInvoiceDto);
			try
			{
				await _dbContext.PaymentInvoices.AddAsync(invoice);
				await _dbContext.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		//TODO
		public async Task<bool> DeleteAsync(Guid id)
		{
			var payment = await _dbContext.PaymentInvoices
			   .FirstOrDefaultAsync(x => x.Id == id);
			if (payment == null)
			{
				return false;
			}
			try
			{
				_dbContext.PaymentInvoices.Remove(payment);
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
