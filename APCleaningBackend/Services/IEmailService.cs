using APCleaningBackend.Model;

namespace APCleaningBackend.Services
{
    public interface IEmailService
    {
        Task SendInvoiceAsync(Booking booking);
        Task SendWaitlistConfirmationAsync(string email);
        Task SendServiceCompleteToCustomerAsync(Booking booking);
        Task SendDriverStatusToCustomerAsync(Booking booking);

    }
}
