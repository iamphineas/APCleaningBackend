using APCleaningBackend.Model;

namespace APCleaningBackend.Services
{
    public interface IEmailService
    {
        Task SendInvoiceAsync(Booking booking);

    }
}
