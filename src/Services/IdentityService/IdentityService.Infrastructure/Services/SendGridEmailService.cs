using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Json;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// SendGrid email service implementation
/// </summary>
public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        ISendGridClient sendGridClient,
        IOptions<EmailSettings> emailSettings,
        ILogger<SendGridEmailService> logger)
    {
        _sendGridClient = sendGridClient ?? throw new ArgumentNullException(nameof(sendGridClient));
        _emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        Dictionary<string, string>? templateData = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            var to = new EmailAddress(toEmail, toName);

            var message = new SendGridMessage
            {
                From = from,
                Subject = subject,
                PlainTextContent = plainTextContent,
                HtmlContent = htmlContent
            };

            message.AddTo(to);

            // Add template data if provided
            if (templateData != null && templateData.Any())
            {
                message.SetTemplateData(templateData);
            }

            // Add tracking settings
            message.SetClickTracking(true, true);
            message.SetOpenTracking(true);
            message.SetGoogleAnalytics(true, "TravelPortal", "email");

            var response = await _sendGridClient.SendEmailAsync(message, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Error: {Error}",
                    toEmail, response.StatusCode, errorBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendTemplateEmailAsync(
        string toEmail,
        string toName,
        string templateId,
        Dictionary<string, string> templateData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            var to = new EmailAddress(toEmail, toName);

            var message = new SendGridMessage
            {
                From = from,
                TemplateId = templateId
            };

            message.AddTo(to);
            message.SetTemplateData(templateData);

            // Add tracking settings
            message.SetClickTracking(true, true);
            message.SetOpenTracking(true);
            message.SetGoogleAnalytics(true, "TravelPortal", "email");

            var response = await _sendGridClient.SendEmailAsync(message, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Template email sent successfully to {Email} using template {TemplateId}",
                    toEmail, templateId);
                return true;
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send template email to {Email}. Status: {StatusCode}, Error: {Error}",
                    toEmail, response.StatusCode, errorBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending template email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendBulkEmailAsync(
        List<EmailRecipient> recipients,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        Dictionary<string, string>? templateData = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            
            var message = new SendGridMessage
            {
                From = from,
                Subject = subject,
                PlainTextContent = plainTextContent,
                HtmlContent = htmlContent
            };

            // Add recipients
            foreach (var recipient in recipients)
            {
                message.AddTo(new EmailAddress(recipient.Email, recipient.Name));
            }

            // Add template data if provided
            if (templateData != null && templateData.Any())
            {
                message.SetTemplateData(templateData);
            }

            // Add tracking settings
            message.SetClickTracking(true, true);
            message.SetOpenTracking(true);
            message.SetGoogleAnalytics(true, "TravelPortal", "bulk_email");

            var response = await _sendGridClient.SendEmailAsync(message, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Bulk email sent successfully to {RecipientCount} recipients", recipients.Count);
                return true;
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send bulk email. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending bulk email to {RecipientCount} recipients", recipients.Count);
            return false;
        }
    }

    public async Task<EmailDeliveryStatus> GetEmailDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would typically involve querying SendGrid's Events API
            // For now, we'll return a basic status
            _logger.LogInformation("Retrieving email delivery status for message {MessageId}", messageId);
            
            // In a real implementation, you would call SendGrid's Events API here
            return new EmailDeliveryStatus
            {
                MessageId = messageId,
                Status = "delivered", // This would be determined by the API response
                DeliveredAt = DateTime.UtcNow,
                OpenedAt = null,
                ClickedAt = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving email delivery status for {MessageId}", messageId);
            throw;
        }
    }
}

/// <summary>
/// Email service interface
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        Dictionary<string, string>? templateData = null,
        CancellationToken cancellationToken = default);

    Task<bool> SendTemplateEmailAsync(
        string toEmail,
        string toName,
        string templateId,
        Dictionary<string, string> templateData,
        CancellationToken cancellationToken = default);

    Task<bool> SendBulkEmailAsync(
        List<EmailRecipient> recipients,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        Dictionary<string, string>? templateData = null,
        CancellationToken cancellationToken = default);

    Task<EmailDeliveryStatus> GetEmailDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Email settings configuration
/// </summary>
public class EmailSettings
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SendGridApiKey { get; set; } = string.Empty;
    public string? TemplateDirectory { get; set; }
}

/// <summary>
/// Email recipient model
/// </summary>
public class EmailRecipient
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Email delivery status model
/// </summary>
public class EmailDeliveryStatus
{
    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
}
