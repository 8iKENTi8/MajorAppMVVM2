using System;
using System.Collections.Generic;
using MajorAppMVVM2.Logging;

namespace MajorAppMVVM2.Models
{
    public class Order
    {
        private string _status = "Новая"; // Значение по умолчанию для статуса
        private readonly List<ILogger> _loggers = new List<ILogger>();

        public int Id { get; set; }
        public string Description { get; set; }
        public string PickupAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public string Comment { get; set; }
        public string Executor { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public double Weight { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyLoggers($"Order {Id} status changed to {_status}");
                }
            }
        }

        public void AttachLogger(ILogger logger)
        {
            _loggers.Add(logger);
        }

        public void DetachLogger(ILogger logger)
        {
            _loggers.Remove(logger);
        }

        private void NotifyLoggers(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(message);
            }
        }
    }
}
