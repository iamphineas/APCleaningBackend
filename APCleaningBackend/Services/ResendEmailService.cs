namespace APCleaningBackend.Services
{
    using APCleaningBackend.Model;
    using Resend;

    public class ResendEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public ResendEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendInvoiceAsync(Booking booking)
        {
            var resend = ResendClient.Create(_config["Resend:ApiKey"]);

            await resend.EmailSendAsync(new EmailMessage
            {
                From = "onboarding@resend.dev",
                To = "alwandengcobo3@gmail.com",
                Subject = "Booking Confirmation & Invoice",
                HtmlBody = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <title>Booking Invoice</title>
  <style>
    body {{ font-family: Arial, sans-serif; color: #333; padding: 20px; }}
    .header {{ text-align: center; margin-bottom: 30px; }}
    .header h1 {{ margin: 0; color: #392C3A; }}
    .details, .summary {{ margin-bottom: 20px; }}
    .details td, .summary td {{ padding: 5px 10px; }}
    .summary {{ border-top: 1px solid #ccc; }}
    .total {{ font-weight: bold; font-size: 1.2em; }}
  </style>
</head>
<body>
  <div class='header'>
    <h1>AP Cleaning Services</h1>
    <p>Booking Confirmation & Invoice</p>
  </div>
  <table class='details'>
    <tr><td><strong>Invoice</strong></td><td>#{booking.BookingID}</td></tr>
    <tr><td><strong>Date</strong></td><td>{booking.ServiceStartTime}</td></tr>
    <tr><td><strong>Customer</strong></td><td>{booking.FullName}</td></tr>
    <tr><td><strong>Email</strong></td><td>{booking.Email}</td></tr>
    <tr><td><strong>Address</strong></td><td>{booking.Address}, {booking.City}, {booking.Province}</td></tr>
  </table>
  <table class='summary'>
    <tr><td><strong>Service Type</strong></td><td>{booking.ServiceType.Name}</td></tr>
    <tr><td><strong>Start Time</strong></td><td>{booking.ServiceStartTime}</td></tr>
    <tr><td><strong>End Time</strong></td><td>{booking.ServiceEndTime}</td></tr>
    <tr><td class='total'><strong>Total Amount</strong></td><td class='total'>R{booking.BookingAmount}</td></tr>
  </table>
  <p>Thank you for choosing AP Cleaning Services. We look forward to serving you!</p>
</body>
</html>"
            });
        }

        public async Task SendWaitlistConfirmationAsync(string email)
        {
            var resend = ResendClient.Create(_config["Resend:ApiKey"]);

            await resend.EmailSendAsync(new EmailMessage
            {
                From = "onboarding@resend.dev",
                To = "alwandengcobo3@gmail.com",
                Subject = "You're on the waitlist!",
                HtmlBody = "<p>Thanks for signing up! We'll notify you when our shop launches.</p>"
            });
        }

        public async Task SendServiceCompleteToCustomerAsync(Booking booking)
        {
            var resend = ResendClient.Create(_config["Resend:ApiKey"]);

            await resend.EmailSendAsync(new EmailMessage
            {
                From = "onboarding@resend.dev",
                To = "alwandengcobo3@gmail.com",
                Subject = "Your Cleaning Service is Complete!",
                HtmlBody = $@"
        <p>Hi there {booking.FullName},</p>
        <p>Your booking <strong>{booking.BookingID}</strong> for <strong>{booking.ServiceType.Name}</strong> has been completed.</p>
        <p>We hope your space feels spotless!</p>
        <p>Thanks for choosing APCleaning!</p>"

            });
        }
    }
}
