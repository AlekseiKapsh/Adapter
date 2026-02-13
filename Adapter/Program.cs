using System;

namespace Adapter
{
   
    public interface IMessageSender
    {
        void Send(string header, string body);
    }

    
    public class LegacyEmailService
    {
        public void SendEmail(string fullMessage)
        {
            Console.WriteLine("[LegacyEmailService] Отправка email:");
            Console.WriteLine(fullMessage);
            Console.WriteLine("Email отправлен");
        }
    }

    
    public class LegacySmsService
    {
        public void SendSms(string phoneNumber, string text)
        {
            Console.WriteLine($"[LegacySmsService] Отправка SMS на номер {phoneNumber}:");
            Console.WriteLine(text);
            Console.WriteLine("SMS отправлено");
        }
    }

    
    public class EmailAdapter : IMessageSender
    {
        private readonly LegacyEmailService _legacyEmailService;
        private readonly string _defaultRecipient;

        public EmailAdapter(LegacyEmailService legacyEmailService, string defaultRecipient = "user@example.com")
        {
            _legacyEmailService = legacyEmailService;
            _defaultRecipient = defaultRecipient;
        }

        public void Send(string header, string body)
        {
            
            string formattedMessage = $"Кому: {_defaultRecipient}Тема: {header} {body}";
            _legacyEmailService.SendEmail(formattedMessage);
        }
    }

    
    public class SmsAdapter : IMessageSender
    {
        private readonly LegacySmsService _legacySmsService;
        private readonly string _defaultPhoneNumber;

        public SmsAdapter(LegacySmsService legacySmsService, string defaultPhoneNumber = "+7-999-123-4567")
        {
            _legacySmsService = legacySmsService;
            _defaultPhoneNumber = defaultPhoneNumber;
        }

        public void Send(string header, string body)
        {
            
            string phoneNumber = !string.IsNullOrEmpty(header) && header.StartsWith("+")
                ? header
                : _defaultPhoneNumber;

            
            string smsText = body.Length > 160 ? body.Substring(0, 157) + "..." : body;

            _legacySmsService.SendSms(phoneNumber, smsText);
        }
    }

    
    public class UniversalMessageAdapter : IMessageSender
    {
        private readonly object _legacyService;
        private readonly string _methodName;
        private readonly Func<string, string, object[]> _parameterConverter;

        public UniversalMessageAdapter(object legacyService, string methodName, Func<string, string, object[]> parameterConverter)
        {
            _legacyService = legacyService;
            _methodName = methodName;
            _parameterConverter = parameterConverter;
        }

        public void Send(string header, string body)
        {
            try
            {
                var method = _legacyService.GetType().GetMethod(_methodName);
                if (method != null)
                {
                    var parameters = _parameterConverter(header, body);
                    method.Invoke(_legacyService, parameters);
                    Console.WriteLine("[UniversalAdapter] Метод успешно вызван через рефлексию");
                }
                else
                {
                    Console.WriteLine($"[UniversalAdapter] Ошибка: метод {_methodName} не найден");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UniversalAdapter] Ошибка при вызове метода: {ex.Message}");
            }
        }
    }

    
    public class NotificationService
    {
        private readonly IMessageSender _messageSender;

        public NotificationService(IMessageSender messageSender)
        {
            _messageSender = messageSender;
        }

        public void NotifyUser(string subject, string message)
        {
            Console.WriteLine("[NotificationService] Отправка уведомления пользователю...");
            _messageSender.Send(subject, message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Демонстрация паттерна Адаптер");

            
            Console.WriteLine("Пример 1: Email адаптер");
            var legacyEmailService = new LegacyEmailService();
            var emailAdapter = new EmailAdapter(legacyEmailService, "admin@company.com");
            var emailNotification = new NotificationService(emailAdapter);
            emailNotification.NotifyUser("Важное обновление", "Вышла новая версия приложения!");

            
            Console.WriteLine("Пример 2: SMS адаптер");
            var legacySmsService = new LegacySmsService();
            var smsAdapter = new SmsAdapter(legacySmsService);
            var smsNotification = new NotificationService(smsAdapter);
            smsNotification.NotifyUser("Срочно!", "Ваш заказ готов к выдаче");

            
            Console.WriteLine("Пример 3: SMS адаптер с динамическим номером");
            var smsAdapterWithNumber = new SmsAdapter(legacySmsService);
            var smsNotification2 = new NotificationService(smsAdapterWithNumber);
            smsNotification2.NotifyUser("+7-999-888-7777", "Код подтверждения: 123456");

            
            Console.WriteLine(" Пример 4: Универсальный адаптер");
            var anotherLegacyService = new LegacyEmailService();

            var universalAdapter = new UniversalMessageAdapter(
                anotherLegacyService,
                "SendEmail",
                (header, body) => new object[] { $"Универсальный вызов: {header} {body}" }
            );

            var universalNotification = new NotificationService(universalAdapter);
            universalNotification.NotifyUser("Тест", "Проверка универсального адаптера");

            Console.WriteLine("Демонстрация завершена");
            Console.WriteLine("Ключевые моменты паттерна Адаптер:");
            Console.WriteLine("Целевой интерфейс (IMessageSender) - то, что ожидает клиентский код");
            Console.WriteLine("Адаптируемые классы (LegacyEmailService, LegacySmsService) - имеют несовместимый интерфейс");
            Console.WriteLine("Адаптеры (EmailAdapter, SmsAdapter, UniversalMessageAdapter) - преобразуют вызовы");
            Console.WriteLine("Клиентский код (NotificationService) - работает только с целевым интерфейсом");
        }
    }
}